using System;
using System.Web;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// A HttpHandler prepared to receive WDA.Application.ServiceTrace.TraceRecord sendt over HTTP POST
	/// </summary>
	public class WriteTraceHandler: IHttpHandler 
	{
	
		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			Configuration.LoadSettings(context);

			try
			{
				int count = WDA.Application.Utl.ToInt(context.Request.InputStream.Length);
				byte[] bytes = new byte[count];
				count = context.Request.InputStream.Read(bytes, 0, count);				
				string inputData = System.Text.Encoding.UTF8.GetString(bytes, 0, count);

				WDA.Application.ServiceTrace.TraceRecord traceRecord = new WDA.Application.ServiceTrace.TraceRecord(inputData);
				TraceData.AddTraceRecord(traceRecord);
			}
			catch (System.Exception exc)
			{
				context.Response.StatusDescription = exc.Message;
				context.Response.Flush();
				context.Response.Close();
				return;
			}

			//context.Response.Output.Write(context.Request.HttpMethod);
			context.Response.StatusCode = 200;
			context.Response.Flush();
			context.Response.Close();
			return;
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
