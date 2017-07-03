using System;
using System.Data;
using System.Threading;
using WDA.Application;
using DataRow = System.Data.DataRow;
using DataSet = System.Data.DataSet;
using DataTable = System.Data.DataTable;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// A class for managing the current cache of trace records recieved.
	/// </summary>
	public class TraceData
	{
		// ReSharper disable InconsistentNaming
		public const string TABLE_SERVICEREQUEST	= "ServiceRequest";
		public const string TABLE_TRACERECORD			= "TraceRecord";
		// ReSharper restore InconsistentNaming

		private static DataSet _traceData = null;

		/// <summary>Clear the current trace data.</summary>
		public static void ClearData()
		{
			LockData();
			_traceData = null;
			ReleaseData();
		}

		/// <summary>Read data contenst from xml file.</summary>
		public static void ReadXml(string fileName)
		{
			DataSet ds = LockData();
			ds.Clear();
			ds.ReadXml(fileName, XmlReadMode.IgnoreSchema);
			ReleaseData();
		}

		/// <summary>Get a copy of the current trace data.</summary>
		/// <returns>A copy of the dataset containing the current trace records.</returns>
		public static DataSet GetData()
		{
			DataSet ds = LockData();
			DataSet copy = null;
			System.Exception exception = null;
			try
			{
				copy = ds.Copy();
			}
			catch (System.Exception exc)
			{
				exception = exc;
			}
			ReleaseData();
			if (exception != null) throw exception;
			return copy;
		}

		/// <summary>Get a copy of the specified ServiceRequestRow.</summary>
		/// <returns>A copy of the specified ServiceRequestRow.</returns>
		public static DataRow GetServiceRequestRow(int pk)
		{
			DataSet ds = GetData();
			DataTable dt = ds.Tables[TABLE_SERVICEREQUEST];
			DataRow[] rows = dt.Select("PK = " + pk.ToString());
			if (rows.Length == 0)
			{
				throw new System.Exception("Sorry, the selected row is now longer available");
			}

			return rows[0];
		}

		public static void AddTraceRecord(WDA.Application.ServiceTrace.TraceRecord traceRecord)
		{
			var traceData = new TraceData();
			traceData.InternalAddTraceRecord(traceRecord);
		}

		private static readonly string[] separators = "                                                                                            ".Split(' ');

		private void InternalAddTraceRecord(WDA.Application.ServiceTrace.TraceRecord traceRecord)
		{
			if (traceRecord == null) throw WDA.Application.Exception.NullArgument(this, "AddTraceRecord", "traceRecord");

			WDA.Application.ObjectContext							objectContext			= traceRecord.ObjectContext;
			WDA.Application.ExecutionContext					executionContext	= traceRecord.ExecutionContext;
			WDA.Application.ServiceTrace.TraceContext traceContext			= executionContext.TraceContext;
			WDA.Application.ServiceTrace.TraceRecord.TransactionInformation transaction = traceRecord.Transaction;
			TimeSpan elapsed = traceRecord.TimeStamp.Subtract(traceContext.RequestStarted);
			int elapsedTime = (int)elapsed.TotalMilliseconds;

			try
			{
				DataSet ds = LockData();
				DataTable main = ds.Tables[TABLE_SERVICEREQUEST];
				DataRow mainRow;

				// Try to see if this a new record for an already existing Service Request
				string criteria = "RequestId = '" + traceContext.RequestId + "'";
				DataRow[] rows = main.Select(criteria);
				if (rows.Length > 0)
				{
					mainRow = rows[0];
					if (Utl.ToInt(mainRow["ElapsedTime"]) < elapsedTime) mainRow["ElapsedTime"] = elapsedTime;
				}
				else
				{
					// Truncate if we have exceeded the maximum number of Service Requests
					if (main.Rows.Count >= Configuration.MaxTraceRecords)
					{
						main.Rows[0].Delete();
					}
					mainRow = main.NewRow();
					mainRow["Origin"] = traceContext.Origin;
					mainRow["Title"] = traceRecord.ServiceTitle;
					mainRow["UserName"] = traceContext.UserName;
					mainRow["RequestId"] = traceContext.RequestId;
					mainRow["RequestStarted"] = traceContext.RequestStarted;
					mainRow["ElapsedTime"] = elapsedTime;
					main.Rows.Add(mainRow);
				}

				DataTable record = ds.Tables[TABLE_TRACERECORD];
				DataRow recordRow = record.NewRow();
				recordRow["FK"] = mainRow["PK"];
				recordRow["TraceSwitch"] = traceRecord.TraceSwitch.ToString();
				recordRow["SequenceCounter"] = traceRecord.SequenceCounter;
				recordRow["Title"] = (traceRecord.Indentation > 0 ? string.Join("&nbsp;", separators, 0, (traceRecord.Indentation * 2) + 1) : "") + traceRecord.Title;
				recordRow["Data"] = traceRecord.Data;
				recordRow["ElapsedTime"] = elapsedTime;
				recordRow["ComponentName"] = objectContext.TypeName;
				recordRow["AppDomainId"] = objectContext.MachineName + "-" + objectContext.ProcessId + "-" + objectContext.AppDomainId;
				recordRow["ExecutingUser"] = traceRecord.ExecutingUser;
				recordRow["AssemblyName"] = objectContext.Assembly.Name;
				recordRow["AssemblyVersion"] = objectContext.Assembly.Version;
				recordRow["AssemblyDate"] = objectContext.Assembly.VersionDate;
				recordRow["ApplicationFactoryName"] = executionContext.ApplicationFactoryName;
				recordRow["ApplicationDomain"] = executionContext.ApplicationDomain;
				recordRow["OwnerDomain"] = executionContext.OwnerDomain;
				recordRow["UserLanguage"] = executionContext.UserLanguage;
				recordRow["UserCulture"] = executionContext.UserCulture;
				recordRow["UserTimezoneOffset"] = executionContext.UserTimezoneOffset;
				recordRow["UserContextUserId"] = executionContext.UserContext.UserID;
				recordRow["UserContextLangCountry"] = executionContext.UserContext.LanguageCountryCode;
				recordRow["UserContextProperties"] = (executionContext.UserContext.Properties.Count > 0
				                                      	? executionContext.UserContext.Properties.Serialize(true)
				                                      	: null);

				if (transaction != null)
				{
					recordRow["TransactionStatus"] = transaction.Status;
					recordRow["TransactionIsolationLevel"] = transaction.IsolationLevel;
					recordRow["TransactionCreationTime"] = transaction.CreationTime;
					recordRow["TransactionDistributedIdentifier"] = transaction.DistributedIdentifier;
					recordRow["TransactionLocalIdentifier"] = transaction.LocalIdentifier;
				}

				record.Rows.Add(recordRow);

				ds.AcceptChanges();
			}
			finally
			{
				ReleaseData();
			}
		}

		/// <summary>Lock the dataset and return the reference</summary>
		/// <returns>The dataset containing the current trace records.</returns>
		private static DataSet LockData()
		{
			// Try to lock the context collection, wait for maximum Configuration.SynchWaitMaxSeconds seconds
			if (!Monitor.TryEnter(typeof(TraceData), Configuration.SynchWaitMaxSeconds * 1000))
			{
				// The request for synchronization timed out!!!!
				throw new WDA.Application.Exception(typeof(TraceData)
					, "A synchronization request timed out after " + Configuration.SynchWaitMaxSeconds + " seconds.\n"
					+ "The reason for this situation may either be due to an extremely high load on the system, "
					+ "or due to a dead-lock situation "
					+ "(in which case the problem are very severe, and the system administrator should be informed immediately!!)."
					);
			}
			if (_traceData == null)
			{
				_traceData = DefineDataSet();
			}
			return _traceData;
		}

		/// <summary>Release the lock on the dataset</summary>
		private static void ReleaseData()
		{
			Monitor.Exit(typeof(TraceData));
		}

		private static DataSet DefineDataSet()
		{
			var ds = new DataSet("ServiceTrace");

			var main = new DataTable(TABLE_SERVICEREQUEST);
			DataColumn pk = NewColumn("PK", typeof(long), "Trace Id");
			pk.AutoIncrement = true;
			main.Columns.Add(pk);
			main.Columns.Add(NewColumn("Origin"					, typeof(string)	, "Origin of request"));
			main.Columns.Add(NewColumn("Title"						, typeof(string)	, "Service Request"));
			main.Columns.Add(NewColumn("UserName"				, typeof(string)	, "Request User"));
			main.Columns.Add(NewColumn("RequestId"				, typeof(string)	,	"Request Id"));
			main.Columns.Add(NewColumn("RequestStarted"	, typeof(DateTime),	"Started"));
			main.Columns.Add(NewColumn("TraceCount"			, typeof(int)			,	"Trace Count"));
			main.Columns.Add(NewColumn("ElapsedTime"			, typeof(int)			,	"Time used"));

			ds.Tables.Add(main);

			var record = new DataTable(TABLE_TRACERECORD);
			pk = NewColumn("PK", typeof(long), "Record Id");
			pk.AutoIncrement = true;
			record.Columns.Add(pk);
			record.Columns.Add(NewColumn("FK"										, typeof(long)));
			record.Columns.Add(NewColumn("TraceSwitch"						, typeof(string)	, "Trace Switch"));
			record.Columns.Add(NewColumn("SequenceCounter"				, typeof(long)	, "Sequence Counter"));
			record.Columns.Add(NewColumn("Title"									, typeof(string)	, "Title"));
			record.Columns.Add(NewColumn("Data"									, typeof(string)	,	"Data"));
			record.Columns.Add(NewColumn("ElapsedTime"						, typeof(int)			,	"Time used"));
			record.Columns.Add(NewColumn("ComponentName"					, typeof(string)	,	"Tracing Component"));
			record.Columns.Add(NewColumn("AppDomainId"						, typeof(string)	, "Executing in"));
			record.Columns.Add(NewColumn("ExecutingUser"					, typeof(string)	, "Executing as"));
			record.Columns.Add(NewColumn("AssemblyName"					, typeof(string)	, "Assembly"));
			record.Columns.Add(NewColumn("AssemblyVersion"				, typeof(string)	, "Version"));
			record.Columns.Add(NewColumn("AssemblyDate"					, typeof(DateTime),	"Version Date"));
			record.Columns.Add(NewColumn("ApplicationFactoryName", typeof(string)	,	"Application Factory"));
			record.Columns.Add(NewColumn("ApplicationDomain"			, typeof(string)	,	"Application Domain"));
			record.Columns.Add(NewColumn("OwnerDomain"						, typeof(string)	, "Owner Domain"));
			record.Columns.Add(NewColumn("UserLanguage"					, typeof(string)	, "User Language"));
			record.Columns.Add(NewColumn("UserCulture"						, typeof(string)	, "User Culture"));
			record.Columns.Add(NewColumn("UserTimezoneOffset"		, typeof(int)			, "User Timezone Offset"));
			record.Columns.Add(NewColumn("UserContextUserId", typeof(string), "UserID"));
			record.Columns.Add(NewColumn("UserContextLangCountry", typeof(string), "Language Country Code"));
			record.Columns.Add(NewColumn("UserContextProperties", typeof(string), "Properties"));
			record.Columns.Add(NewColumn("TransactionStatus", typeof(string), "Status"));
			record.Columns.Add(NewColumn("TransactionIsolationLevel", typeof(string), "Isolation Level"));
			record.Columns.Add(NewColumn("TransactionCreationTime", typeof(string), "Creation Time"));
			record.Columns.Add(NewColumn("TransactionDistributedIdentifier", typeof(string), "Distributed Identifier"));
			record.Columns.Add(NewColumn("TransactionLocalIdentifier", typeof(string), "Local Identifier"));

			ds.Tables.Add(record);

			var rel = new DataRelation
				( "ServiceRequest_TraceRecord"
				, main.Columns["PK"]
				, record.Columns["FK"]
				, true
				);
			ds.Relations.Add(rel);
			rel.ChildKeyConstraint.DeleteRule = Rule.Cascade;

			return ds;
		}

		private static DataColumn NewColumn(string name, Type type)
		{
			return NewColumn(name, type, "_hidden");
		}

		private static DataColumn NewColumn(string name, Type type, string caption)
		{
			return new DataColumn(name, type) {Caption = caption};
		}
	}
}
