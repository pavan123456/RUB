using System;
using System.Web;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// A HttpHandler serving information about this application
	/// </summary>
	public class InfoHandler: IHttpHandler 
	{
	
		void IHttpHandler.ProcessRequest(System.Web.HttpContext context)
		{
			Configuration.LoadSettings(context);

			HTMLRenderer.WriteHeader(context);
			ConfigRenderer.Write(context, false);

			context.Response.Write("<div style='text-align:left;padding-top:30px'>"
				+ "You have requested file " + context.Request.Url
				+ "<br />Request Time=" + DateTime.Now.ToString("HH:mm:ss")
				+ "<br />context.Request.FilePath=" + context.Request.FilePath 
				+ "<br />context.Request.Path=" + context.Request.Path 
				+ "<br />context.Request.PathInfo=" + context.Request.PathInfo 
				+ "<br />context.Request.PhysicalApplicationPath=" + context.Request.PhysicalApplicationPath 
				+ "<br />context.Request.PhysicalPath=" + context.Request.PhysicalPath 
				+ "<br />context.Request.RequestType=" + context.Request.RequestType 
				+ "<br />context.Request.UserHostName=" + context.Request.UserHostName 
				+ "<br />context.Request.ApplicationPath=" + context.Request.ApplicationPath 				
				+ "<br />context.Request.Url.GetLeftPart(System.UriPartial.Scheme)=" + context.Request.Url.GetLeftPart(System.UriPartial.Scheme) 
				+ "</div>"); 
			HTMLRenderer.WriteTrailer(context);

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
