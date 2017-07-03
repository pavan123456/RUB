using System.Data;
using System.Web;
using WDA.Application;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// A HttpHandler prepared to receive query string command to view TraceData
	/// </summary>
	public class ViewTraceHandler : IHttpHandler
	{
		// ReSharper disable InconsistentNaming
		private const string COMMAND_SERVICEREQUESTS = "SERVICEREQUESTS";
		private const string COMMAND_SERVICERECORDS = "SERVICERECORDS";
		private const string COMMAND_CLEAR = "CLEAR";
		private const string COMMAND_SAVEAS_DIALOG = "SAVEASDIALOG";
		private const string COMMAND_SAVEAS = "SAVEAS";
		private const string COMMAND_LOAD_DIALOG = "LOADDIALOG";
		private const string COMMAND_LOAD = "LOAD";
		// ReSharper restore InconsistentNaming

		void IHttpHandler.ProcessRequest(HttpContext context)
		{
			Configuration.LoadSettings(context);

			string command = Utl.SafeString(context.Request.QueryString["Command"], COMMAND_SERVICEREQUESTS).ToUpper();


			try
			{
				if (command == COMMAND_SAVEAS_DIALOG)
				{
					InputSaveAsRenderer.WriteForm(context, "Save Trace Records to File", COMMAND_SAVEAS);
				}
				else if (command == COMMAND_SAVEAS)
				{
					string fileName = "";
					try
					{
						fileName = Utl.SafeString(context.Request.QueryString["fileName"]);
						if (fileName.Length > 0)
						{
							TraceData.GetData().WriteXml(fileName, XmlWriteMode.WriteSchema);
						}
						context.Response.Redirect(context.Request.FilePath + "?Command=" + COMMAND_SERVICEREQUESTS, true);
					}
					catch (System.Exception exc)
					{
						InputSaveAsRenderer.WriteForm(context, "Save Trace Records to File", COMMAND_SAVEAS, fileName, exc.Message);
					}
				}
				else if (command == COMMAND_LOAD_DIALOG)
				{
					InputOpenFileRenderer.WriteForm(context, "Load Trace Records from File", COMMAND_LOAD);
				}
				else if (command == COMMAND_LOAD)
				{
					string fileName = "";
					try
					{
						fileName = Utl.SafeString(context.Request.QueryString["fileName"]);
						if (fileName.Length > 0)
						{
							TraceData.ReadXml(fileName);
						}
						context.Response.Redirect(context.Request.FilePath + "?Command=" + COMMAND_SERVICEREQUESTS, true);
					}
					catch (System.Exception exc)
					{
						InputOpenFileRenderer.WriteForm(context, "Load Trace Records from File", COMMAND_LOAD, fileName, exc.Message);
					}
				}
				else if (command == COMMAND_CLEAR)
				{
					TraceData.ClearData();
					context.Response.Redirect(context.Request.FilePath + "?Command=" + COMMAND_SERVICEREQUESTS, true);
				}
				else if (command == COMMAND_SERVICERECORDS)
				{
					int id = Utl.ToInt(context.Request.QueryString["Id"]);
					TraceRecordRenderer.WriteTraceRecord(context, TraceData.GetServiceRequestRow(id), "Service Request Trace Records");
				}
				else // COMMAND_SERVICEREQUESTS
				{
					string hyperlink = context.Request.FilePath + "?Command=" + COMMAND_SERVICERECORDS + "&Id={0}";
					string clearlink = context.Request.FilePath + "?Command=" + COMMAND_CLEAR;
					string saveAslink = context.Request.FilePath + "?Command=" + COMMAND_SAVEAS_DIALOG;
					string loadlink = context.Request.FilePath + "?Command=" + COMMAND_LOAD_DIALOG;
					ServiceRequestRenderer.WriteServiceRequest(context, TraceData.GetData(), hyperlink, "Save As," + saveAslink + ";Open," + loadlink + ";Clear All," + clearlink);
				}
			}
			catch (System.Exception exc)
			{
				HTMLRenderer.WriteErrorPage(context, exc);
			}
		}

		/// <summary>
		/// This method should return true to indicate that the handler may be pooled by the application.
		/// </summary>
		bool IHttpHandler.IsReusable
		{
			get { return true; }
		}
	}
}

