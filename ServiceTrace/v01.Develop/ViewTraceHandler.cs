using System;
using System.Web;
using WDA.Application;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// A HttpHandler prepared to receive query string command to view TraceData
	/// </summary>
	public class ViewTraceHandler: IHttpHandler 
	{
		private const string COMMAND_SERVICEREQUESTS = "SERVICEREQUESTS";
		private const string COMMAND_SERVICERECORDS = "SERVICERECORDS";
		private const string COMMAND_CLEAR = "CLEAR";

		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			Configuration.LoadSettings(context);

			string command = Utl.SafeString(context.Request.QueryString["Command"], COMMAND_SERVICEREQUESTS).ToUpper();


			try
			{
				if (command == COMMAND_CLEAR)
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
					ServiceRequestRenderer.WriteServiceRequest(context, TraceData.GetData(), hyperlink, "Clear All," + clearlink);
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
			get{return true;}
		}
	}
}
