using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Web;
using WDA.Application;

namespace WDA.HttpHandlers.ServiceTrace
{
	// =========================================================================================================================
	/// <summary>A HttpHandler prepared to receive WDA.Application.ServiceTrace.TraceRecord sendt over HTTP POST</summary>
	// =========================================================================================================================
	public class WriteTraceHandler : IHttpHandler
	{
		// ReSharper disable InconsistentNaming
		private const int SYNCRONIZATION_WAITSECONDS = 10;
		private const int THREAD_RETRY_MAX = 100;
		private const int THREAD_RETRY_DELAY = 100;
		// ReSharper restore InconsistentNaming

		private static readonly Queue inputQueue = new Queue();
		private static int _threadsRunning = 0;
		private static int _threadCount = 0;

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			int id = Interlocked.Increment(ref _threadCount);
			Configuration.LoadSettings(context);

			DebugLog.Add(id, DebugLog.ThreadType.Request, "Enter");
			try
			{
			  bool startThread;
        try
        {
          var queue = (Queue)Utl.AquireLock(typeof(WriteTraceHandler), "Input Queue", WriteTraceHandler.inputQueue, SYNCRONIZATION_WAITSECONDS);
          queue.Enqueue(new RequestData(id, context.Request.InputStream));
          startThread = (WriteTraceHandler._threadsRunning == 0);
          if (startThread) WriteTraceHandler._threadsRunning = 1;
        } // Do not catch exceptions. They will be handled above
        finally
        {
          Utl.ReleaseLock(WriteTraceHandler.inputQueue);
        }

				if (startThread)
				// This is the only item in the queue, start a pooled thread to process the request
				{
					DebugLog.Add(id, DebugLog.ThreadType.Request, "QueueUserWorkItem");
					ThreadPool.QueueUserWorkItem(RequestData.Execute);
				}

        context.Response.StatusCode = 200;
      }
			catch (System.Exception exc)
			{
        context.Response.StatusDescription = exc.Message;
			}
			finally
			{
				context.Response.Flush();
				// context.Response.Close(); // SV: Should be closed by listener
			}
			DebugLog.Add(id, DebugLog.ThreadType.Request, "Exit");
		}

		/// <summary>
		/// This method should return true to indicate that the handler may be pooled by the application.
		/// </summary>
		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}

		// =======================================================================================================================
		private class RequestData
		{
			private readonly byte[] _data;
			private readonly int _dataCount;

			public RequestData(int threadId, System.IO.Stream inputStream)
			// Construct and read all bytes from input stream
			// Keep state until Execute is invoked on a separat thread
			{
				_dataCount = Utl.ToInt(inputStream.Length);
				DebugLog.Add(threadId, DebugLog.ThreadType.Worker, "Before reading inputStream, length=" + _dataCount);
				_data = new byte[_dataCount];
				_dataCount = inputStream.Read(_data, 0, _dataCount);
				if (Configuration.Debug)
				{
					string inputData = Encoding.UTF8.GetString(_data, 0, _dataCount);
					var traceRecord = new WDA.Application.ServiceTrace.TraceRecord(inputData);
					DebugLog.Add(threadId, DebugLog.ThreadType.Worker, "After reading inputStream,  " + traceRecord.SequenceCounter + " " + traceRecord.ServiceTitle);
				}
			}

			public static void Execute(object dummy)
			// This method is invoked on a separat thread from the thread pool
			{
				int retryCount = 0;
				int id = Interlocked.Increment(ref WriteTraceHandler._threadCount);
				DebugLog.Add(id, DebugLog.ThreadType.Worker, "Started");
				string operation = "";

				try
				{
					while (true)
					{
						RequestData requestData;
						int queueCount;
						bool keepRunning = false;

						try
						{
							// Lock the queue and remove the next item in line
							operation = "Lock Queue";
							var queue = (Queue)Utl.AquireLock(typeof(WriteTraceHandler.RequestData), "Input Queue", WriteTraceHandler.inputQueue, SYNCRONIZATION_WAITSECONDS);
							queueCount = queue.Count;
							requestData = (queueCount > 0 ? (RequestData)queue.Dequeue() : null);
							if (requestData == null)
							{
								retryCount++;
								keepRunning = (retryCount <= THREAD_RETRY_MAX);
								WriteTraceHandler._threadsRunning = (keepRunning ? 1 : 0);
							}
						} // Do not catch exceptions. They will be rethrown and handled above
						finally
						{
							operation = "Unlock Queue";
							Utl.ReleaseLock(WriteTraceHandler.inputQueue);
						}

						if (requestData == null)
						{
							operation = "Waiting for Queue Entry";
							if (keepRunning)
							{
								DebugLog.Add(id, DebugLog.ThreadType.Worker, "Sleeping");
								Thread.Sleep(THREAD_RETRY_DELAY);
								continue;
							}
							else
							{
								DebugLog.Add(id, DebugLog.ThreadType.Worker, "Completed");
								break; // Queue is empty, we are done
							}
						}

						retryCount = 0; // Reset each time we have a trace to process
						operation = "Processing Queue Entry";
						DebugLog.Add(id, DebugLog.ThreadType.Worker, "Processing, Queue Size = " + queueCount);

						string inputData = Encoding.UTF8.GetString(requestData._data, 0, requestData._dataCount);
						var traceRecord = new WDA.Application.ServiceTrace.TraceRecord(inputData);
						TraceData.AddTraceRecord(traceRecord);
					}
				} // while(true)
				catch (System.Exception exc)
				{
					WDA.Application.EventLog.WriteError(Configuration.EventLogSource, exc.Message);
					EventLog.WriteError
						(Configuration.EventLogSource
						 , "An error occured during operation \"" + operation + "\" when attempting to process a TraceRecord. \n"
							 + "Reason: " + exc.Message
						);
				}
				finally
				{
					Interlocked.Exchange(ref WriteTraceHandler._threadsRunning, 0);
					//Utl.ReleaseLock(WriteTraceHandler.inputQueue);
					DebugLog.Write();
				}
			}
		}

		// =======================================================================================================================
		private class DebugLog
		{
			public enum ThreadType
			{
				Request,
				Worker
			}

			private static readonly List<string> itemList = new List<string>();
			private static int _lineNo = 0;

			public static void Add(int threadId, ThreadType threadType, string item)
			{
				if (!Configuration.Debug) return;
				lock (itemList)
				{
					_lineNo++;
					if (itemList.Count > 100) DebugLog.Flush();
					itemList.Add(_lineNo.ToString("0000#") + ": " + threadType + " Thread " + threadId + " - " + item);
				}
			}

			public static void Write()
			{
				if (!Configuration.Debug) return;
				lock (itemList)
				{
					DebugLog.Flush();
				}
			}

			private static void Flush()
			{
				if (itemList.Count == 0) return;
				string[] items = itemList.ToArray();
				EventLog.WriteInformation(Configuration.EventLogSource, Utl.Join(items, "\n", "\n"));
				itemList.Clear();
			}
		}
	}
}
