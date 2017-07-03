using System;
using System.Text;
using System.Web;
using WDA.Application;
using DataColumn = System.Data.DataColumn;
using DataRow = System.Data.DataRow;
using DataSet = System.Data.DataSet;
using DataTable = System.Data.DataTable;

namespace WDA.HttpHandlers.ServiceTrace
{
	#region HTMLRenderer
	// ============================================================================================================================
	/// <summary>
	/// Class for rendering HTML output
	/// </summary>
	// ============================================================================================================================
	public class HTMLRenderer
	{
		/// <summary>Cached reference to executing assembly</summary>
		protected static WDA.Application.AssemblyInfo assemblyUtility = null;

		/// <summary>Current HttpContext</summary>
		protected HttpContext context = null;

		/// <summary>Instanciate a renderer for specified HttpContext</summary>
		protected HTMLRenderer(HttpContext context)
		{
			this.context = context;
		}

		/// <summary>Format a header for HTML output</summary>
		internal static void WriteHeader(HttpContext context)
		{
			(new HTMLRenderer(context)).WriteHeader();
		}

		/// <summary>Format a trailer for HTML output</summary>
		internal static void WriteTrailer(HttpContext context)
		{
			(new HTMLRenderer(context)).WriteTrailer();
		}

		/// <summary>Format an exception for HTML output</summary>
		internal static void WriteErrorPage(HttpContext context, System.Exception exception)
		{
			(new HTMLRenderer(context)).WriteErrorPage(exception);
		}

		/// <summary>Format an exception for HTML output</summary>
		internal void WriteErrorPage(System.Exception exception)
		{
			this.WriteErrorPage(exception.Message);
		}

		/// <summary>Format an exception for HTML output</summary>
		internal static void WriteErrorPage(HttpContext context, string message)
		{
			(new HTMLRenderer(context)).WriteErrorPage(message);
		}

		/// <summary>Format an exception for HTML output</summary>
		internal void WriteErrorPage(string message)
		{
			this.WriteHeader();
			this.WriteLine("<div style='font-weight:bold;padding-bottom:3px;color:#ff0000'>");
			this.WriteLine(message);
			this.WriteLine("</div>");
			this.WriteTrailer();
		}

		/// <summary>Reference to executing assembly</summary>
		protected static WDA.Application.AssemblyInfo ThisAssembly{get{return Configuration.ThisAssembly;}}

		/// <summary>Write to current context response</summary>
		protected void Write(string html)
		{
			this.context.Response.Write(html);
		}

		/// <summary>Write to current context response</summary>
		protected void WriteLine(string html)
		{
			this.context.Response.Write(html + "\n");
		}

		/// <summary>Format a header for HTML output</summary>
		protected void WriteHeader()
		{
			this.WriteHeader(ThisAssembly.Title, "");
		}

		/// <summary>Format a header for HTML output</summary>
		protected void WriteHeader(string title, string headerLink)
		{
			this.context.Response.CacheControl = "no-cache";
			this.context.Response.ContentType = "text/html";

			this.WriteLine("<html>");
			this.WriteLine("<head>");
			this.WriteLine("<title>" + ThisAssembly.Title + " - " + ThisAssembly.Version + "</title>");
			this.WriteLine("<script type=\"text/javascript\" src=\"js/jquery.min.js\"></script>");
			this.WriteLine("<style>");
			this.WriteLine("body, table, tr, th, td, div, span{font-family: Verdana, Arial, sans-serif; font-size:10pt;}");
			this.WriteLine(".title{font-size:16pt; font-weight: bold; color:#3377C0}");
			this.WriteLine(".section{font-size:10pt; font-weight: bold; color:#3377C0}");
			this.WriteLine("td{padding:2px;}");
			this.WriteLine("td.head{font-size:8pt; font-weight: bold; background-color:#777777; color:#ffffff}");
			this.WriteLine("td.name{font-size:8pt; font-style: italic;}");
			this.WriteLine("td.setting{font-size:8pt; border-left:1px solid #c0c0c0}");
			this.WriteLine("td.data, td.data1, td.dataline{font-size:8pt; border-left:1px solid #c0c0c0; padding-left:5px; padding-right:5px;}");
			this.WriteLine("td.data1{border:none}");
			this.WriteLine("td.dataline{border-bottom:1px solid #c0c0c0;}");
			this.WriteLine("td.dataname{font-size:8pt; font-style: italic;border-left:1px solid #c0c0c0;}");
			this.WriteLine("tr.even{background-color:#eeeeee;}");
			this.WriteLine("hyperLink{cursor:default; font-size:8pt; color:#0000ff;}");
			this.WriteLine("</style>");
			this.WriteLine("</head>");
			this.WriteLine("<body style='text-align:center; margin: 2px'>");
			this.WriteLine("<div style='margin-bottom:0px'>");

			this.WriteLine("<table style='width:100%; border-bottom:#c0c0c0 1px solid' cellpadding='0' cellspacing='0'>");
			this.WriteLine("<tr>");
			this.WriteLine("<td width='200'><img src='./WDA_Logo_w182.gif'/></td>");
			this.WriteLine("<td class='title' style='text-align:left;'>" + title + "</td>");
			this.WriteLine("<td style='font-size:8pt; text-align:right'>");
			if (headerLink.Length > 0)
			{
				this.WriteLine("<table cellpadding='0' cellspacing='0'>");
				this.WriteLine("<tr>");
				string[] links = headerLink.Split(';');
				foreach(string link in links)
				{
					this.WriteLine("<td style='font-size:8pt;'>");
					string[] segs = link.Split(',');
					this.WriteLine("<a href='" + segs[1] + "'>" + segs[0] + "</a>");
					this.WriteLine("</td>");
				}
				this.WriteLine("</tr>");
				this.WriteLine("</table>");
			}
			this.WriteLine(DateTime.Now.ToString("d. MMM yyyy HH:mm:ss"));
			this.WriteLine("<br />" + ThisAssembly.Title + " - version " + ThisAssembly.VersionMajorMinor);
			this.WriteLine("<br />" + Utl.ExecutingUser);
			this.WriteLine("</td>");
			this.WriteLine("</tr>");
			this.WriteLine("</table>");
			this.WriteLine("</div>");
		}

		/// <summary>Format a trailer for HTML output</summary>
		protected void WriteTrailer()
		{
			this.WriteLine("<div style='font-size:8pt;text-align:center;margin-top:30px;padding-top:10px;border-top:1px dotted #c0c0c0'>");
			this.WriteLine(ThisAssembly.Copyright);
			this.WriteLine("<div style='font-size:7pt;font-style:italic;color:#a0a0a0'>");
			this.WriteLine(ThisAssembly.Title + " - version " + ThisAssembly.Version + " of " + ThisAssembly.VersionDate.ToString("yyyy-MM-dd HH:mm:ss"));
			this.WriteLine("</div>");
			this.WriteLine("</div></body></html>");
		}
	}
	#endregion

	#region ConfigRenderer
	// ============================================================================================================================
	/// <summary>
	/// Renderer for dumping current configuration settings
	/// </summary>
	// ============================================================================================================================
	public class ConfigRenderer: HTMLRenderer
	{
		private bool _oddRow = true;

		private ConfigRenderer(HttpContext context):base(context){}

		/// <summary>Format the current configuration settings as HTML</summary>
		internal static void Write(HttpContext context)
		{
			Write(context, true);
		}

		/// <summary>Format the current configuration settings as HTML</summary>
		internal static void Write(HttpContext context, bool includeHeaderAndTrailer)
		{
			(new ConfigRenderer(context)).Write(includeHeaderAndTrailer);
		}

		/// <summary>Format the current configuration settings as HTML</summary>
		private void Write()
		{
			this.Write(true);
		}

		/// <summary>Format the current configuration settings as HTML</summary>
		private void Write(bool includeHeaderAndTrailer)
		{
			if (includeHeaderAndTrailer) this.WriteHeader();

			this.WriteLine("<div style='font-weight:bold;padding-bottom:3px'>Configuration settings</div>");
			this.WriteLine("<table style='border:1px solid #c0c0c0;'>");
			this.WriteLine("<tr><td class='head'>Setting</td><td class='head'>Value</td></tr>");

			WriteConfigSetting("EventLogSource", Configuration.EventLogSource);
			WriteConfigSetting("MaxTraceRecords", Configuration.MaxTraceRecords.ToString());
			WriteConfigSetting("Debug", Configuration.Debug.ToString());

			this.WriteLine("</table>");

			if (includeHeaderAndTrailer) this.WriteTrailer();
		}

		private void WriteConfigSetting(string name, string setting)
		{
			if (string.IsNullOrEmpty(setting)) setting = "&nbsp;";
			string rowClass = (_oddRow ? "odd" : "even");
			_oddRow = !_oddRow;
			this.WriteLine("<tr class='" + rowClass + "'><td class='name'>" + name + "</td><td class='setting'>" + setting + "</td></tr>");
		}
	}

	#endregion

	#region FormRenderer
	// ============================================================================================================================
	/// <summary>
	/// Renderer for Input Form  Dialog
	/// </summary>
	// ============================================================================================================================
	public abstract class InputFormRenderer: HTMLRenderer
	{
		protected string title = "";
		protected string headerLink = "";
		protected StringHashtable defaultValues = new StringHashtable();
		protected string submittButtonCaption = "";
		protected string cancelButtonCaption  = "Cancel";
		protected string errorMessage = "";
		protected string command = "";

		protected InputFormRenderer(HttpContext context):base(context){}

		protected InputFormRenderer(HttpContext context, string title, string headerLink):base(context)
		{
			this.title = title;
			this.headerLink = headerLink;
		}

		/// <summary>Render the ServiceRequest table</summary>
		protected void Render()
		{
			this.WriteHeader(this.title, this.headerLink);
			this.RenderFormContents();
			this.WriteTrailer();
		}

		internal void AddDefaultValue(string name, string value)
		{
			this.defaultValues.Add(name, value);
		}

		internal string SubmittButtonCaption
		{
			get{return this.submittButtonCaption;}
			set{this.submittButtonCaption = Utl.SafeString(value);}
		}

		internal string CancelButtonCaption
		{
			get{return this.cancelButtonCaption;}
			set{this.cancelButtonCaption = Utl.SafeString(value);}
		}

		internal string ErrorMessage
		{
			get{return this.errorMessage;}
			set{this.errorMessage = Utl.SafeString(value);}
		}

		internal string Command
		{
			get{return this.command;}
			set{this.command = Utl.SafeString(value);}
		}

		protected string DefaultValue(string name)
		{
			return this.defaultValues[name];
		}

		protected abstract void RenderFormContents();
	}
	#endregion

	#region InputFileNameRenderer
	// ============================================================================================================================
	/// <summary>Renderer for SaveAs/Open Dialog</summary>
	// ============================================================================================================================
	public class InputFileNameRenderer: InputFormRenderer
	{
		protected const string DEFAULT_FILENAME = "C:\\Temp\\WDA.ServiceTrace.xml";

		/// <summary>Render the InputFileNameRenderer dialog form</summary>
		internal InputFileNameRenderer(HttpContext context, string title):this(context, title, ""){}

		/// <summary>Render the InputFileNameRenderer dialog form</summary>
		internal InputFileNameRenderer(HttpContext context, string title, string headerLink):base(context, title, headerLink){}

		/// <summary>Render the InputFileNameRenderer dialog form contents</summary>
		protected override void RenderFormContents()
		{
			this.WriteLine("<div style='width:50%;font-size:8pt;text-align:left;margin-top:30px;padding:10px;border:3px double #c0c0c0'>");
			if (Utl.SafeString(this.errorMessage).Length > 0)
			{
				this.WriteLine(		"<div style='padding-bottom:10px;text-align:center;font-size:10pt;color:red;font-weight:bold'>" + this.errorMessage + "</div>");
			}
			this.WriteLine(	"<form action='" + context.Request.FilePath + "' method='get' style='padding:0; margin:0'>");
			this.WriteLine(		"<div style='font-size:8pt;color:#777777;'>Target file name:</div>");
			this.WriteLine(		"<input name='command' type='hidden' value='" + this.command + "' />");
			this.WriteLine(		"<input name='fileName' type='text' size='80' value='" + this.DefaultValue("fileName") + "' />");
			this.WriteLine(		"<div style='padding-top:10px'>");
			this.WriteLine(			"<input type='submit' value='" + this.SubmittButtonCaption + "' />");
			this.WriteLine(			"<input type='button' value='" + this.CancelButtonCaption  + "' onclick='window.navigate(\"" + context.Request.FilePath + "\")' />");
			this.WriteLine("	</div>");
			this.WriteLine(	"</form>");
			this.WriteLine("</div>");
		}
	}

	// ============================================================================================================================
	/// <summary>Renderer for SaveAs Dialog</summary>
	// ============================================================================================================================
	public class InputSaveAsRenderer: InputFileNameRenderer
	{
		/// <summary>Render the InputFileNameRenderer dialog form</summary>
		private InputSaveAsRenderer(HttpContext context, string title):base(context, title)
		{
			this.SubmittButtonCaption = "Save";
		}

		internal static void WriteForm(HttpContext context, string title, string command)
		{
			InputSaveAsRenderer.WriteForm(context, title, command, DEFAULT_FILENAME, "");
		}

		internal static void WriteForm(HttpContext context, string title, string command, string fileName, string errorMessage)
		{
			var formRenderer = new InputSaveAsRenderer(context, title);
			formRenderer.Command = command;
			formRenderer.ErrorMessage = errorMessage;
			formRenderer.AddDefaultValue("fileName", fileName);
			formRenderer.Render();
		}
	}

		// ============================================================================================================================
		/// <summary>Renderer for Open File Dialog</summary>
		// ============================================================================================================================
	public class InputOpenFileRenderer: InputFileNameRenderer
	{
		/// <summary>Render the InputFileNameRenderer dialog form</summary>
		private InputOpenFileRenderer(HttpContext context, string title):base(context, title)
		{
			this.SubmittButtonCaption = "Open";
		}

		internal static void WriteForm(HttpContext context, string title, string command)
		{
			InputOpenFileRenderer.WriteForm(context, title, command, DEFAULT_FILENAME, "");
		}

		internal static void WriteForm(HttpContext context, string title, string command, string fileName, string errorMessage)
		{
			var formRenderer = new InputOpenFileRenderer(context, title);
			formRenderer.Command = command;
			formRenderer.ErrorMessage = errorMessage;
			formRenderer.AddDefaultValue("fileName", fileName);
			formRenderer.Render();
		}
	}
	#endregion

	#region DataRenderer
	// ============================================================================================================================
	/// <summary>
	/// Renderer for TraceData
	/// </summary>
	// ============================================================================================================================
	public class DataRenderer: HTMLRenderer
	{
		protected bool oddRow = true;

		internal DataRenderer(HttpContext context):base(context){}

		internal void WriteTableHeader(DataTable table, string[] columns, bool showRowNumber)
		{
			this.WriteTableHeader(table, columns, showRowNumber, "");
		}

		internal void WriteTableHeader(DataTable table, string[] columns, bool showRowNumber, string height)
		{
			if (height.Length > 0) height = "height:" + height + "px;";
			this.WriteLine("<div style='padding-top:5px;" + height + "'>");
			this.WriteLine("<table width='100%' cellpadding='0' cellspacing='0' style='border:1px solid #c0c0c0;'>");

			this.WriteTableRowHeaders(table, columns, showRowNumber);
		}

		internal void WriteTableTrailer()
		{
			this.WriteLine("</table>");
			this.WriteLine("</div>");
		}

		protected void WriteTableRowHeaders(DataTable table, string[] columns, bool showRowNumber)
		{
			this.WriteLine("<tr>");
			if (showRowNumber) this.WriteLine("<td class='head' align='right'>Row#</td>");
			foreach(string name in columns)
			{
				string columnName = (name.Substring(0,1) == "_" ? name.Substring(1) : name);
				this.WriteTableRowHeaderColum(table.Columns[columnName]);
			}
			this.WriteLine("</tr>");
		}

		protected void WriteTableRowHeaderColum(DataColumn col)
		{
			this.WriteTableRowHeaderColum(col.Caption, this.ColumnAlign(col));
		}

		protected void WriteTableRowHeaderColum(string caption)
		{
			this.WriteTableRowHeaderColum(caption, "left");
		}

		protected void WriteTableRowHeaderColum(string caption, string align)
		{
			this.WriteLine("<td class='head' align='" + align + "'>" + caption + "</td>");
		}

		protected void WriteTableRow(int rowCount, DataRow row, string[] columns)
		{
			this.WriteTableRow(rowCount, row, columns, "");
		}

		protected void WriteTableRow(int rowCount, DataRow row, string[] columns, string hyperlink)
		{
			string rowClass = (this.oddRow ? "odd" : "even");
			this.oddRow = !this.oddRow;
			this.WriteLine("<tr class='" + rowClass + "'>");
			if (hyperlink.Length > 0) hyperlink = hyperlink.Replace("{0}", row["PK"].ToString());
			if (rowCount > 0) this.WriteLine("<td class='data1' align='right'>" + rowCount.ToString("# ##0") + "</td>");
			foreach(string name in columns)
			{
				if (name.Substring(0,1) == "_")
				{
					this.WriteColumnValue(row, name.Substring(1), hyperlink);
				}
				else
				{
					this.WriteColumnValue(row, name, "");
				}
			}
			this.WriteLine("</tr>");
		}

		protected void WriteColumnValue(DataRow row, string name)
		{
			this.WriteColumnValue(row, name, "");
		}

		protected void WriteColumnValue(DataRow row, string name, string hyperlink)
		{
			DataColumn col = row.Table.Columns[name];
			if (hyperlink.Length == 0)
			{
				this.WriteLine("<td class='data' align='" + this.ColumnAlign(col) + "'>" + this.ColumnValue(row, col) + "</td>");
			}
			else if (hyperlink.StartsWith("JavaScript:"))
			{
				this.WriteLine
					( "<td class='data' onclick='" + hyperlink + "'align='" + this.ColumnAlign(col) + "'>"
					+ "<span onmouseover='this.style.textDecoration=\"underline\"' onmouseout='this.style.textDecoration=\"none\"' style='cursor:default; font-size:8pt; color:#0000ff;'>"
					+ this.ColumnValue(row, col) + "</span></td>");
			}
			else
			{
				this.WriteLine("<td class='data' align='" + this.ColumnAlign(col) + "'><a target='_blank' href='" + hyperlink + "'>" + this.ColumnValue(row, col) + "</a></td>");
			}
		}

		protected string ColumnValue(DataRow row, DataColumn col)
		{
			if (row.IsNull(col)) return "&nbsp;";

			if (col.DataType == typeof(DateTime)) return ((DateTime)row[col]).ToString("u");
			if (col.DataType == typeof(TimeSpan)) return ((TimeSpan)row[col]).TotalMilliseconds.ToString("# ##0") + " ms";
			if (col.DataType == typeof(int)) return ((int)row[col]).ToString("# ##0");
			if (col.DataType == typeof(long)) return ((long)row[col]).ToString("# ##0");
			return row[col].ToString();
		}

		protected string ColumnAlign(DataColumn col)
		{
			return (col.DataType == typeof (DateTime)
			        	? "center"
			        	: (col.DataType == typeof (string)
			        	   	? "left"
			        	   	: "right"
			        	  )
			       );

		}
	}

	#endregion

	#region ServiceRequestRenderer
	// ============================================================================================================================
	/// <summary>
	/// Renderer for TraceData ServiceRequest
	/// </summary>
	// ============================================================================================================================
	public class ServiceRequestRenderer: DataRenderer
	{
		private readonly string[] _tableColumns = new string[] {"PK", "RequestStarted", "_Title", "Origin", "UserName", "TraceCount", "ElapsedTime"};

		private ServiceRequestRenderer(HttpContext context):base(context){}

		/// <summary>Render the ServiceRequest table</summary>
		internal static void WriteServiceRequest(HttpContext context, DataSet dataSet, string hyperlink, string headerLink)
		{
			(new ServiceRequestRenderer(context)).WriteServiceRequest(dataSet, hyperlink, headerLink);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal static void WriteServiceRequest(HttpContext context, DataRow row)
		{
			(new ServiceRequestRenderer(context)).WriteServiceRequest(row);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal static void WriteServiceRequest(HttpContext context, DataRow row, string title)
		{
			(new ServiceRequestRenderer(context)).WriteServiceRequest(row, title);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal void WriteServiceRequest(DataRow row, string title)
		{
			this.WriteHeader(title, "");
			this.WriteServiceRequest(row);
			this.WriteTrailer();
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal void WriteServiceRequest(DataRow row)
		{
			string[] columns = new string[] {"_PK", "_RequestStarted", "_Title", "Origin", "_UserName"};

			this.WriteTableHeader(row.Table, columns, false);

			row["TraceCount"] = row.GetChildRows(row.Table.ChildRelations[0]).Length;
			this.WriteTableRow(0, row, columns);

			this.WriteTableTrailer();
		}

		/// <summary>Render the ServiceRequest table</summary>
		internal void WriteServiceRequest(DataSet dataSet, string hyperlink, string headerLink)
		{
			this.WriteHeader("Service Requests", headerLink);

			DataTable main = dataSet.Tables[TraceData.TABLE_SERVICEREQUEST];
			this.WriteTableHeader(main, _tableColumns, true, "300");

			System.Data.DataView view = main.DefaultView;
			System.Data.DataRelation rel = main.ChildRelations[0];
			view.Sort = "RequestStarted desc, PK desc";
			for(int rowIdx=0; rowIdx < view.Count; rowIdx++)
			{
				System.Data.DataRowView rowView = view[rowIdx];
				DataRow row = rowView.Row;
				row["TraceCount"] = row.GetChildRows(rel).Length;
				this.WriteTableRow(rowIdx+1, row, _tableColumns, hyperlink);
			}

			this.WriteTableTrailer();
			this.WriteTrailer();
		}
	}

	#endregion

	#region TraceRecordRenderer
	// ============================================================================================================================
	/// <summary>
	/// Renderer for TraceData TraceRecord
	/// </summary>
	// ============================================================================================================================
	public class TraceRecordRenderer: DataRenderer
	{
		private string[] tableColumns = new string[] {"_PK", "_RequestStarted", "_Title", "Origin", "_UserName", "TraceCount"};

		private TraceRecordRenderer(HttpContext context):base(context){}

		/// <summary>Render the ServiceRequest single row</summary>
		internal static void WriteTraceRecord(HttpContext context, DataRow serviceRequestRow, string title)
		{
			(new TraceRecordRenderer(context)).WriteTraceRecord(serviceRequestRow, title);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal void WriteTraceRecord(DataRow serviceRequestRow, string title)
		{
			this.WriteHeader(title, "");
			this.WriteLine("<div style='padding-top:10px;'>");
			this.WriteLine("<div class='section' style='text-align:left; padding-top:5px;'>Service Request</div>");
			ServiceRequestRenderer.WriteServiceRequest(this.context, serviceRequestRow);

			DataTable recordsTable = serviceRequestRow.Table.DataSet.Tables[TraceData.TABLE_TRACERECORD];
			System.Data.DataView view = recordsTable.DefaultView;
			view.RowFilter = "FK = " + serviceRequestRow["PK"].ToString();
			view.Sort			 = "ElapsedTime Asc, SequenceCounter Asc";


			this.WriteLine("<div class='section' style='text-align:left; padding-top:10px;'>Trace Records</div>");
			string[] columns = new string[] { "ElapsedTime", "TraceSwitch", "_ComponentName", "Title", "AppDomainId", "ExecutingUser" };
			this.WriteTableHeader(recordsTable, columns, true);

			for (int rowIdx = 0; rowIdx < view.Count; rowIdx++)
			{
				System.Data.DataRowView rowView = view[rowIdx];
				DataRow row = rowView.Row;
				string detailsId = "details" + row["PK"];
				string hyperLink = "JavaScript:" + "$(\"#" + detailsId + "\").toggle();";

				this.WriteTableRow(rowIdx + 1, row, columns, hyperLink);
				this.WriteLine("<tr><td colspan='7'>");

				this.WriteLine("<table id='" + detailsId + "' width='100%' cellpadding='0' cellspacing='0' style='display:none; border:1px solid #c0c0c0;'>");
				this.WriteObjectContext(row);
				this.WriteExecutionContext(row);
				this.WriteUserContext(row);

				if (!row.IsNull("TransactionStatus"))
				{
					this.WriteTransactionContext(row);
				}

				this.oddRow = !this.oddRow;
				string rowClass = (this.oddRow ? "odd" : "even");
				DataColumn col = recordsTable.Columns["Data"];
				this.WriteLine("<tr class='" + rowClass + "'>");
				this.WriteLine("<td class='dataname' width='150' valign='top' style='border-top:1px solid #c0c0c0'>" + col.Caption + "</td>");
				this.WriteLine("<td class='data' style='border-top:1px solid #c0c0c0'>" + this.HTMLString(this.ColumnValue(row, col)) + "</td>");
				this.WriteLine("</tr>");


				this.WriteLine("</table></td></tr>");
			}

			this.WriteTableTrailer();
			this.WriteLine("</div>");
			this.WriteTrailer();
		}

		private void WriteTransactionContext(DataRow row)
		{
			this.WriteRow("Transaction", this.ColDisplayValue(row
			                                                  , "TransactionStatus"
			                                                  , "TransactionIsolationLevel"
			                                                  , "TransactionCreationTime"
			                                                  , "TransactionDistributedIdentifier"
			                                                  , "TransactionLocalIdentifier")
				);

		}

		private void WriteObjectContext(DataRow row)
		{
			this.WriteRow("ObjectContext", this.ColDisplayValue(row
			                                                    , "AssemblyName"
			                                                    , "AssemblyVersion"
			                                                    , "AssemblyDate"
																													)
				);
		}
		private void WriteUserContext(DataRow row)
		{
			this.WriteRow("UserContext", this.ColDisplayValue(row
			                                                  , "UserContextUserId"
			                                                  , "UserContextLangCountry"
			                                                  //, "UserContextProperties"
																												)
				);
		}

		private void WriteExecutionContext(DataRow row)
		{
			this.WriteRow("ExecutionContext", this.ColDisplayValue(row
			                                                       , "ApplicationDomain"
			                                                       , "OwnerDomain"
																														 , "ApplicationFactoryName"
																														 , "UserLanguage"
			                                                       , "UserCulture"
			                                                       , "UserTimezoneOffset")
				);

		}

		private string ColDisplayValue(DataRow row, params string[] names)
		{
			var html = new StringBuilder("<table cellpadding='0' cellspacing='0'>");
			foreach (var name in names)
			{
				DataColumn col = row.Table.Columns[name];
				string data = this.ColumnValue(row, col);
				if (data.IndexOf('<') >= 0) data = this.HTMLString(data);
				html.Append("<tr>")
					.Append("<td class='dataname' style='border-left:none'>" + col.Caption + "</td>")
					.Append("<td class='data'>" + (string.IsNullOrEmpty(data) ? "&nbsp;" : data) + "</td>")
					.Append("</tr>");
			}
			return html.Append("</table>").ToString();
		}

		private void WriteRow(string caption, string data)
		{
			this.oddRow = !this.oddRow;
			string rowClass = (this.oddRow ? "odd" : "even");
			this.WriteLine("<tr class='" + rowClass + "'>");
			this.WriteLine("<td class='dataname'>" + caption + "</td>");
			this.WriteLine("<td class='data'>" + data + "</td>");
			this.WriteLine("</tr>");
		}

		private void WriteColCell(DataRow row, string name)
		{
			DataColumn col = row.Table.Columns[name];
			this.WriteLine("<td class='dataline' align='" + this.ColumnAlign(col) + "'>" + this.ColumnValue(row, col) + "</td>");
		}

		private void WriteColRow(DataRow row, params string[] names)
		{
			if (names == null || names.Length == 0) return;

			this.oddRow = !this.oddRow;
			string rowClass = (this.oddRow ? "odd" : "even");
			this.WriteLine("<tr class='" + rowClass + "'>");

			foreach (string name in names)
			{
				if (string.IsNullOrEmpty(name))
				{
					this.WriteLine("<td class='dataname'>&nbsp;</td>");
					this.WriteLine("<td class='data'>&nbsp;</td>");
				}
				else
				{
					DataColumn col = row.Table.Columns[name];
					this.WriteLine("<td class='dataname'>" + col.Caption + "</td>");
					this.WriteLine("<td class='data'>" + this.ColumnValue(row, col) + "</td>");
				}
			}

			this.WriteLine("</tr>");
		}

		private string HTMLString(string plain)
		{
			string result = plain.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\t", "&nbsp;&nbsp;");
			return result.Replace("  ", "&nbsp;&nbsp;").Replace("\n\n","<p />").Replace("\n", "<br />");
		}
	}
	#endregion
}
