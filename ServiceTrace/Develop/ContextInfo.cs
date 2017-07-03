using System.Web;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// Wrapper for a HttpContext to expose values
	/// </summary>
	internal class ContextInfo
	{
		private readonly HttpContext _context = null;

		internal ContextInfo(HttpContext context)
		{
			_context = context;
		}

		/// <summary>
		/// Expose the base fielname of the request path
		/// </summary>
		internal string RequestBaseFileName
		{
			get
			{
				string fileName = _context.Request.FilePath;
				int	idx = fileName.LastIndexOf("/");
				if (idx >= 0) fileName = fileName.Substring(idx + 1);
				idx = fileName.LastIndexOf(".");
				if (idx >= 0) fileName = fileName.Substring(0, idx);
				return fileName;
			}
		}

		internal string ApplicationName
		{
			get
			{
				return _context.Request.ApplicationPath.Substring(1);
			}
		}
	}
}
