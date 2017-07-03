using System;
using WDA.Application;

namespace WDA.HttpHandlers.ServiceTrace
{
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
		protected System.Web.HttpContext context = null;

		/// <summary>Instanciate a renderer for specified HttpContext</summary>
		protected HTMLRenderer(System.Web.HttpContext context) 
		{
			this.context = context;
		}

		/// <summary>Format a header for HTML output</summary>
		internal static void WriteHeader(System.Web.HttpContext context)
		{
			(new HTMLRenderer(context)).WriteHeader();
		}

		/// <summary>Format a trailer for HTML output</summary>
		internal static void WriteTrailer(System.Web.HttpContext context)
		{
			(new HTMLRenderer(context)).WriteTrailer();
		}

		/// <summary>Format an exception for HTML output</summary>
		internal static void WriteErrorPage(System.Web.HttpContext context, System.Exception exception)
		{
			(new HTMLRenderer(context)).WriteErrorPage(exception);
		}

		/// <summary>Format an exception for HTML output</summary>
		internal void WriteErrorPage(System.Exception exception)
		{
			this.WriteErrorPage(exception.Message);
		}

		/// <summary>Format an exception for HTML output</summary>
		internal static void WriteErrorPage(System.Web.HttpContext context, string message)
		{
			(new HTMLRenderer(context)).WriteErrorPage(message);
		}

		/// <summary>Format an exception for HTML output</summary>
		internal void WriteErrorPage(string message)
		{
			this.WriteHeader();
			this.WriteLine("<div style='height:300px;font-weight:bold;padding-bottom:3px;color:#ff0000'>");
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
		protected void WriteHeader(string title, string clearLink)
		{
			this.context.Response.CacheControl = "no-cache";
			this.context.Response.ContentType = "text/html";

			this.WriteLine("<html>");
			this.WriteLine("<head>");
			this.WriteLine("<title>" + ThisAssembly.Title + " - " + ThisAssembly.Version + "</title>");
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
			this.WriteLine("<td width='200'><img src='WDA_Logo_w182.gif'/></td>");
			this.WriteLine("<td class='title' style='text-align:left;'>" + title + "</td>");
			this.WriteLine("<td style='font-size:8pt; text-align:right'>");
			if (clearLink.Length > 0)
			{
				string[] segs = clearLink.Split(',');
				this.WriteLine("<a href='" + segs[1] + "'>" + segs[0] + "</a>");
			}
			this.WriteLine("<br />" + DateTime.Now.ToString("d. MMM yyyy HH:mm:ss"));
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
			this.WriteLine("&copy;" + ThisAssembly.Copyright);
			this.WriteLine("<div style='font-size:7pt;font-style:italic;color:#a0a0a0'>");
			this.WriteLine(ThisAssembly.Title + " - version " + ThisAssembly.Version + " of " + ThisAssembly.VersionDate.ToString("yyyy-MM-dd HH:mm:ss"));
			this.WriteLine("</div>");
			this.WriteLine("</div></body></html>");
		}
	}

	// ============================================================================================================================
	/// <summary>
	/// Renderer for dumping current configuration settings
	/// </summary>
	// ============================================================================================================================
	public class ConfigRenderer: HTMLRenderer
	{
		private bool oddRow = true;

		private ConfigRenderer(System.Web.HttpContext context):base(context){}

		/// <summary>Format the current configuration settings as HTML</summary>
		internal static void Write(System.Web.HttpContext context)
		{
			ConfigRenderer.Write(context, true);
		}

		/// <summary>Format the current configuration settings as HTML</summary>
		internal static void Write(System.Web.HttpContext context, bool includeHeaderAndTrailer)
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
			System.Web.HttpResponse response = context.Response;
			if (includeHeaderAndTrailer) this.WriteHeader();

			this.WriteLine("<div style='font-weight:bold;padding-bottom:3px'>Configuration settings</div>");
			this.WriteLine("<table style='border:1px solid #c0c0c0;'>");
			this.WriteLine("<tr><td class='head'>Setting</td><td class='head'>Value</td></tr>");

			WriteConfigSetting("EventLogSource", Configuration.EventLogSource);
			WriteConfigSetting("MaxTraceRecords", Configuration.MaxTraceRecords.ToString());

			this.WriteLine("</table>");

			if (includeHeaderAndTrailer) this.WriteTrailer();
		}

		private void WriteConfigSetting(string name, string setting)
		{
			if (setting == null || setting.Length == 0) setting = "&nbsp;";
			string rowClass = (this.oddRow ? "odd" : "even");
			this.oddRow = !this.oddRow;
			this.WriteLine("<tr class='" + rowClass + "'><td class='name'>" + name + "</td><td class='setting'>" + setting + "</td></tr>");
		}
	}

	// ============================================================================================================================
	/// <summary>
	/// Renderer for TraceData
	/// </summary>
	// ============================================================================================================================
	public class DataRenderer: HTMLRenderer
	{
		protected bool oddRow = true;

		internal DataRenderer(System.Web.HttpContext context):base(context){}

		internal void WriteTableHeader(System.Data.DataTable table, string[] columns, bool showRowNumber)
		{
			this.WriteTableHeader(table, columns, showRowNumber, "");
		}

		internal void WriteTableHeader(System.Data.DataTable table, string[] columns, bool showRowNumber, string height)
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

		protected void WriteTableRowHeaders(System.Data.DataTable table, string[] columns, bool showRowNumber)
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

		protected void WriteTableRowHeaderColum(System.Data.DataColumn col)
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

		protected void WriteTableRow(int rowCount, System.Data.DataRow row, string[] columns)
		{
			this.WriteTableRow(rowCount, row, columns, "");
		}

		protected void WriteTableRow(int rowCount, System.Data.DataRow row, string[] columns, string hyperlink)
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

		protected void WriteColumnValue(System.Data.DataRow row, string name)
		{
			this.WriteColumnValue(row, name, "");
		}

		protected void WriteColumnValue(System.Data.DataRow row, string name, string hyperlink)
		{
			System.Data.DataColumn col = row.Table.Columns[name];
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

		protected string ColumnValue(System.Data.DataRow row, System.Data.DataColumn col)
		{
			if (col.DataType == typeof(DateTime))
			{
				return ((DateTime)row[col]).ToString("u");
			}
			if (col.DataType == typeof(TimeSpan))
			{
				return ((TimeSpan)row[col]).TotalMilliseconds.ToString("# ##0") + " ms"; 
			}
			else if (col.DataType == typeof(int))
			{
				return ((int)row[col]).ToString("# ##0");
			}
			else if (col.DataType == typeof(long))
			{
				return ((long)row[col]).ToString("# ##0");
			}
			else 
			{
				return row[col].ToString();
			}
		}

		protected string ColumnAlign(System.Data.DataColumn col)
		{
			if (col.DataType == typeof(DateTime))
			{
				return "center";
			}
			else if (col.DataType == typeof(string))
			{
				return "left";
			}
			else 
			{
				return "right";
			}
		}
	}

	// ============================================================================================================================
	/// <summary>
	/// Renderer for TraceData ServiceRequest
	/// </summary>
	// ============================================================================================================================
	public class ServiceRequestRenderer: DataRenderer
	{
		private string[] tableColumns = new string[] {"PK", "RequestStarted", "_Title", "Origin", "UserName", "TraceCount", "ElapsedTime"};

		private ServiceRequestRenderer(System.Web.HttpContext context):base(context){}

		/// <summary>Render the ServiceRequest table</summary>
		internal static void WriteServiceRequest(System.Web.HttpContext context, System.Data.DataSet dataSet, string hyperlink, string clearLink)
		{
			(new ServiceRequestRenderer(context)).WriteServiceRequest(dataSet, hyperlink, clearLink);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal static void WriteServiceRequest(System.Web.HttpContext context, System.Data.DataRow row)
		{
			(new ServiceRequestRenderer(context)).WriteServiceRequest(row);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal static void WriteServiceRequest(System.Web.HttpContext context, System.Data.DataRow row, string title)
		{
			(new ServiceRequestRenderer(context)).WriteServiceRequest(row, title);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal void WriteServiceRequest(System.Data.DataRow row, string title)
		{
			this.WriteHeader(title, "");
			this.WriteServiceRequest(row);
			this.WriteTrailer();
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal void WriteServiceRequest(System.Data.DataRow row)
		{
			string[] columns = new string[] {"_PK", "_RequestStarted", "_Title", "Origin", "_UserName"};

			this.WriteTableHeader(row.Table, columns, false);
			
			row["TraceCount"] = row.GetChildRows(row.Table.ChildRelations[0]).Length;
			this.WriteTableRow(0, row, columns);

			this.WriteTableTrailer();
		}

		/// <summary>Render the ServiceRequest table</summary>
		internal void WriteServiceRequest(System.Data.DataSet dataSet, string hyperlink, string clearLink)
		{
			this.WriteHeader("Service Requests", clearLink);

			System.Data.DataTable main = dataSet.Tables[TraceData.TABLE_SERVICEREQUEST];
			this.WriteTableHeader(main, tableColumns, true, "300");
			
			System.Data.DataView view = main.DefaultView;
			System.Data.DataRelation rel = main.ChildRelations[0];
			view.Sort = "RequestStarted desc";
			for(int rowIdx=0; rowIdx < view.Count; rowIdx++)
			{
				System.Data.DataRowView rowView = view[rowIdx];
				System.Data.DataRow row = rowView.Row;
				row["TraceCount"] = row.GetChildRows(rel).Length;
				this.WriteTableRow(rowIdx+1, row, tableColumns, hyperlink);
			}

			this.WriteTableTrailer();
			this.WriteTrailer();
		}
	}

	// ============================================================================================================================
	/// <summary>
	/// Renderer for TraceData TraceRecord
	/// </summary>
	// ============================================================================================================================
	public class TraceRecordRenderer: DataRenderer
	{
		private string[] tableColumns = new string[] {"_PK", "_RequestStarted", "_Title", "Origin", "_UserName", "TraceCount"};

		private TraceRecordRenderer(System.Web.HttpContext context):base(context){}

		/// <summary>Render the ServiceRequest single row</summary>
		internal static void WriteTraceRecord(System.Web.HttpContext context, System.Data.DataRow serviceRequestRow, string title)
		{
			(new TraceRecordRenderer(context)).WriteTraceRecord(serviceRequestRow, title);
		}

		/// <summary>Render the ServiceRequest single row</summary>
		internal void WriteTraceRecord(System.Data.DataRow serviceRequestRow, string title)
		{
			this.WriteHeader(title, "");
			this.WriteLine("<div style='padding-top:10px; height:300px'>");
			this.WriteLine("<div class='section' style='text-align:left; padding-top:5px;'>Service Request</div>");
			ServiceRequestRenderer.WriteServiceRequest(this.context, serviceRequestRow); 

			System.Data.DataTable recordsTable = serviceRequestRow.Table.DataSet.Tables[TraceData.TABLE_TRACERECORD];
			System.Data.DataView view = recordsTable.DefaultView;
			view.RowFilter = "FK = " + serviceRequestRow["PK"].ToString();
			view.Sort			 = "ElapsedTime Asc, SequenceCounter Asc";
			

			this.WriteLine("<div class='section' style='text-align:left; padding-top:10px;'>Trace Records</div>");
			string[] columns = new string[]{"ElapsedTime", "TraceSwitch", "_ComponentName", "Title", "MachineName", "ExecutingUser"};
			this.WriteTableHeader(recordsTable, columns, true);

			for(int rowIdx=0; rowIdx < view.Count; rowIdx++)
			{
				System.Data.DataRowView rowView = view[rowIdx];
				System.Data.DataRow row = rowView.Row;
				string detailsId = "details" + row["PK"].ToString();
				string htmlElement = "window." + detailsId;
				string displayStyle = htmlElement + ".runtimeStyle.display";
				string hyperLink = "JavaScript:" + displayStyle + "=(" + displayStyle + " == \"block\" ? \"none\" : \"block\");";

				this.WriteTableRow(rowIdx+1, row, columns, hyperLink);
				this.WriteLine("<tr><td colspan='7'>");

				this.WriteLine("<table id='" + detailsId + "' width='100%' cellpadding='0' cellspacing='0' style='display:none; border:1px solid #c0c0c0;'>");
				this.WriteColRow(row, "AssemblyName"					);
				this.WriteColRow(row, "AssemblyVersion"				);
				this.WriteColRow(row, "AssemblyDate"					);
				this.WriteColRow(row, "ApplicationFactoryName");
				this.WriteColRow(row, "ApplicationDomain"			);
				this.WriteColRow(row, "OwnerDomain"						);
				this.WriteColRow(row, "UserLanguage"					);
				this.WriteColRow(row, "UserCulture"						);
				this.WriteColRow(row, "UserTimezoneOffset"		);

				this.oddRow = !this.oddRow;
				string rowClass = (this.oddRow ? "odd" : "even");
				System.Data.DataColumn col = recordsTable.Columns["Data"];
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

		private void WriteColCell(System.Data.DataRow row, string name)
		{
			System.Data.DataColumn col = row.Table.Columns[name];
			this.WriteLine("<td class='dataline' align='" + this.ColumnAlign(col) + "'>" + this.ColumnValue(row, col) + "</td>");
		}

		private void WriteColRow(System.Data.DataRow row, string name)
		{
			System.Data.DataColumn col = row.Table.Columns[name];

			this.oddRow = !this.oddRow;
			string rowClass = (this.oddRow ? "odd" : "even");
			this.WriteLine("<tr class='" + rowClass + "'>");
			this.WriteLine("<td class='dataname'>" + col.Caption + "</td>");
			this.WriteLine("<td class='data'>" + this.ColumnValue(row, col) + "</td>");
			this.WriteLine("</tr>");
		}

		private string HTMLString(string plain)
		{
			string result = plain.Replace("<", "&lt;").Replace(">", "&gt;").Replace("\t", "&nbsp;&nbsp;");
			return result.Replace("  ", "&nbsp;&nbsp;").Replace("\n\n","<p />").Replace("\n", "<br />");
		}
	}
}
