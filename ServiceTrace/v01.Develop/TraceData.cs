using System;
using System.Data;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// A class for managing the current cache of trace records recieved.
	/// </summary>
	public class TraceData
	{
		public const string TABLE_SERVICEREQUEST	= "ServiceRequest";
		public const string TABLE_TRACERECORD			= "TraceRecord";

		private const int SYNCHWAITSECONDS = 10;
		private static DataSet traceData = null;

		/// <summary>Clear the current trace data.</summary>
		public static void ClearData()
		{
			TraceData.LockData();
			TraceData.traceData = null;
			TraceData.ReleaseData();
		}

		/// <summary>Get a copy of the current trace data.</summary>
		/// <returns>A copy of the dataset containing the current trace records.</returns>
		public static DataSet GetData()
		{
			System.Data.DataSet ds = TraceData.LockData();
			System.Data.DataSet copy = null;
			System.Exception exception = null;
			try 
			{
				copy = ds.Copy();
			}
			catch (System.Exception exc)
			{
				exception = exc;
			}
			TraceData.ReleaseData();
			if (exception != null) throw exception;
			return copy;
		}

		/// <summary>Get a copy of the specified ServiceRequestRow.</summary>
		/// <returns>A copy of the specified ServiceRequestRow.</returns>
		public static DataRow GetServiceRequestRow(int pk)
		{
			System.Data.DataSet ds = TraceData.GetData();
			System.Data.DataTable dt = ds.Tables[TABLE_SERVICEREQUEST];
			System.Data.DataRow[] rows = dt.Select("PK = " + pk.ToString());
			if (rows.Length == 0)
			{
				throw new System.Exception("Sorry, the selected row is now longer available");
			}
			else
			{
				return rows[0];
			}
		}

		public static void AddTraceRecord(WDA.Application.ServiceTrace.TraceRecord traceRecord)
		{
			TraceData traceData = new TraceData();
			traceData.InternalAddTraceRecord(traceRecord);
		}

		private void InternalAddTraceRecord(WDA.Application.ServiceTrace.TraceRecord traceRecord)
		{
			if (traceRecord == null) throw WDA.Application.Exception.NullArgument(this, "AddTraceRecord", "traceRecord");
		
			WDA.Application.ObjectContext							objectContext			= traceRecord.ObjectContext;
			WDA.Application.ExecutionContext					executionContext	= traceRecord.ExecutionContext;
			WDA.Application.ServiceTrace.TraceContext traceContext			= executionContext.TraceContext;
			TimeSpan elapsedTime = traceRecord.TimeStamp.Subtract(traceContext.RequestStarted);

			System.Data.DataSet ds = TraceData.LockData();
			System.Exception exception = null;
			try 
			{
				DataTable main = ds.Tables[TABLE_SERVICEREQUEST];
				DataRow mainRow = null;

				// Try to see if this a new record for an already existing Service Request
				string criteria = "RequestId = '" + traceContext.RequestId + "'";
				DataRow[] rows = main.Select(criteria);
				if (rows.Length > 0)
				{
					mainRow = rows[0];
					if (TimeSpan.Compare((TimeSpan)mainRow["ElapsedTime"], elapsedTime) < 0)
					{
						mainRow["ElapsedTime"] = elapsedTime;
					}
				}
				else 
				{
					// Truncate if we have exceeded the maximum number of Service Requests
					if (main.Rows.Count >= Configuration.MaxTraceRecords)
					{
						main.Rows[0].Delete();
					}
					mainRow = main.NewRow();				
					mainRow["Origin"]					= traceContext.Origin;
					mainRow["Title"]					= traceRecord.ServiceTitle; 
					mainRow["UserName"]				= traceContext.UserName;
					mainRow["RequestId"]			= traceContext.RequestId;
					mainRow["RequestStarted"] = traceContext.RequestStarted;
					mainRow["ElapsedTime"]		= elapsedTime;
					main.Rows.Add(mainRow);
				}

				DataTable record = ds.Tables[TABLE_TRACERECORD];
				DataRow recordRow = record.NewRow();
				recordRow["FK"]											= mainRow["PK"];
				recordRow["TraceSwitch"]						= traceRecord.TraceSwitch.ToString();
				recordRow["SequenceCounter"]				= traceRecord.SequenceCounter;
				recordRow["Title"]									= traceRecord.Title;
				recordRow["Data"]										= traceRecord.Data;
				recordRow["ElapsedTime"]						= elapsedTime;
				recordRow["ComponentName"]					= objectContext.TypeName;
				recordRow["MachineName"]						= objectContext.MachineName;
				recordRow["ExecutingUser"]					= traceRecord.ExecutingUser;
				recordRow["AssemblyName"]						= objectContext.Assembly.Name;
				recordRow["AssemblyVersion"]				= objectContext.Assembly.Version;
				recordRow["AssemblyDate"]						= objectContext.Assembly.VersionDate;
				recordRow["ApplicationFactoryName"]	= executionContext.ApplicationFactoryName;
				recordRow["ApplicationDomain"]			= executionContext.ApplicationDomain;
				recordRow["OwnerDomain"]						= executionContext.OwnerDomain;
				recordRow["UserLanguage"]						= executionContext.UserLanguage;
				recordRow["UserCulture"]						= executionContext.UserCulture;
				recordRow["UserTimezoneOffset"]			= executionContext.UserTimezoneOffset;
				record.Rows.Add(recordRow);

				ds.AcceptChanges();
			}
			catch (System.Exception exc)
			{
				exception = exc;
			}
			TraceData.ReleaseData();
			if (exception != null) throw exception;
		}

		/// <summary>Lock the dataset and return the reference</summary>
		/// <returns>The dataset containing the current trace records.</returns>
		private static DataSet LockData()
		{
			// Try to lock the context collection, wait for maximum SYNCHWAITSECONDS seconds
			if (!System.Threading.Monitor.TryEnter(typeof(TraceData), SYNCHWAITSECONDS * 1000))
			{
				// The request for synchronization timed out!!!!
				throw new WDA.Application.Exception(typeof(TraceData)
					, "A synchronization request timed out after " + SYNCHWAITSECONDS.ToString() + " seconds.\n"
					+ "The reason for this situation may either be due to an extremely high load on the system, "
					+ "or due to a dead-lock situation "
					+ "(in which case the problem are very severe, and the system administrator should be informed immediately!!)."
					);
			}
			if (TraceData.traceData == null)
			{
				TraceData.traceData = TraceData.DefineDataSet();
			}
			return TraceData.traceData;
		}

		/// <summary>Release the lock on the dataset</summary>
		private static void ReleaseData()
		{
			System.Threading.Monitor.Exit(typeof(TraceData));
		}

		private static DataSet DefineDataSet()
		{
			DataSet ds = new DataSet("ServiceTrace");

			DataTable main = new DataTable(TABLE_SERVICEREQUEST);
			DataColumn pk = TraceData.Column("PK", typeof(long), "Trace Id");
			pk.AutoIncrement = true;
			main.Columns.Add(pk);
			main.Columns.Add(TraceData.Column("Origin"					, typeof(string)	, "Origin of request"));
			main.Columns.Add(TraceData.Column("Title"						, typeof(string)	, "Service Request"));
			main.Columns.Add(TraceData.Column("UserName"				, typeof(string)	, "Request User"));
			main.Columns.Add(TraceData.Column("RequestId"				, typeof(string)	,	"Request Id"));
			main.Columns.Add(TraceData.Column("RequestStarted"	, typeof(DateTime),	"Started"));
			main.Columns.Add(TraceData.Column("TraceCount"			, typeof(int)			,	"Trace Count"));
			main.Columns.Add(TraceData.Column("ElapsedTime"			, typeof(TimeSpan),	"Time used"));

			ds.Tables.Add(main);

			DataTable record = new DataTable(TABLE_TRACERECORD);
			pk = TraceData.Column("PK", typeof(long), "Record Id");
			pk.AutoIncrement = true;
			record.Columns.Add(pk);
			record.Columns.Add(TraceData.Column("FK"										, typeof(long)));
			record.Columns.Add(TraceData.Column("TraceSwitch"						, typeof(string)	, "Trace Switch"));
			record.Columns.Add(TraceData.Column("SequenceCounter"				, typeof(string)	, "Sequence Counter"));
			record.Columns.Add(TraceData.Column("Title"									, typeof(string)	, "Title"));
			record.Columns.Add(TraceData.Column("Data"									, typeof(string)	,	"Data"));
			record.Columns.Add(TraceData.Column("ElapsedTime"						, typeof(TimeSpan),	"Time used"));
			record.Columns.Add(TraceData.Column("ComponentName"					, typeof(string)	,	"Name of Component writing the trace"));
			record.Columns.Add(TraceData.Column("MachineName"						, typeof(string)	, "Executing on"));
			record.Columns.Add(TraceData.Column("ExecutingUser"					, typeof(string)	, "Executing as"));
			record.Columns.Add(TraceData.Column("AssemblyName"					, typeof(string)	, "Assembly"));
			record.Columns.Add(TraceData.Column("AssemblyVersion"				, typeof(string)	, "Version"));
			record.Columns.Add(TraceData.Column("AssemblyDate"					, typeof(DateTime),	"Version Date"));
			record.Columns.Add(TraceData.Column("ApplicationFactoryName", typeof(string)	,	"Application Factory used"));
			record.Columns.Add(TraceData.Column("ApplicationDomain"			, typeof(string)	,	"Application Domain"));
			record.Columns.Add(TraceData.Column("OwnerDomain"						, typeof(string)	, "Owner Domain"));
			record.Columns.Add(TraceData.Column("UserLanguage"					, typeof(string)	, "User Language"));
			record.Columns.Add(TraceData.Column("UserCulture"						, typeof(string)	, "User Culture"));
			record.Columns.Add(TraceData.Column("UserTimezoneOffset"		, typeof(int)			, "User Timezone Offset"));

			ds.Tables.Add(record);

			DataRelation rel = new DataRelation
				( "ServiceRequest_TraceRecord"
				, main.Columns["PK"]
				, record.Columns["FK"]
				, true
				);
			ds.Relations.Add(rel);
			rel.ChildKeyConstraint.DeleteRule = System.Data.Rule.Cascade;

			return ds;
		}

		private static DataColumn Column(string name, System.Type type)
		{
			return TraceData.Column(name, type, "_hidden");
		}

		private static DataColumn Column(string name, System.Type type, string caption)
		{
			DataColumn col = new DataColumn(name, type);
			col.Caption = caption;
			return col;
		}
	}
}
