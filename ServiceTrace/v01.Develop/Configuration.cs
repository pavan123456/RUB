using System;
using System.Configuration;
using WDA.Application;

namespace WDA.HttpHandlers.ServiceTrace
{
	/// <summary>
	/// Configuration handler for reading the Web.Config section "WDA.HttpHandlers"
	/// </summary>
	internal class Configuration : IConfigurationSectionHandler
	{
		// Wrap executing assembly to expose information
		private static WDA.Application.AssemblyInfo thisAssembly = null;

		// This hash table is used to keep track of what configuration sections/child sections has already been loaded
		private static System.Collections.Hashtable configurationIsLoaded = new System.Collections.Hashtable();

		// These variables is used for transfering values from LoadSettings to IConfigurationSectionHandler.Create
		// As soon as LoadSettings is donoe, these values are no longer valid.
		private static string currentChildNodeName = "";
		private static ConfigurationReaderDelegate currentReader = null;

		// Fields for holding values read from the configuration file
		private static string eventLogSource	= "";
		private static int		maxTraceRecords = 100;

		/// <summary>
		/// A delegate for configuration reader callback.
		/// The different modules of this project may decide to read their own configuration. 
		/// In that case they call the LoadSettings specifying what node to read and a delegate to receive the callback.
		/// </summary>
		internal delegate void ConfigurationReaderDelegate(WDA.Client.Types.IConfigurationReader reader);

		/// <summary>
		/// Static function responsible of loading config settings from the web.config file.
		/// If the specified settings section has already been loaded, this call is ignored.
		/// </summary>
		/// <param name="context">The HttpContext of the caller.</param>
		internal static void LoadSettings(System.Web.HttpContext context)
		{
			Configuration.LoadSettings(context, null, null);
		}

		/// <summary>
		/// Static function responsible of requesting config settings from the web.config file.
		/// If the specified settings section has already been loaded, this call is ignored.
		/// </summary>
		/// <param name="context">The HttpContext of the caller.</param>
		/// <param name="childNodeName">A child node of the "WDA.HttpHandlers" section of the configuration file</param>
		/// <param name="customReader">A delegate that will be called back to read the configuration settings.</param>
		internal static void LoadSettings(System.Web.HttpContext context, string childNodeName, ConfigurationReaderDelegate customReader)
		{
			string sectionName = "WDA.HttpHandlers";
			childNodeName = (childNodeName == null ? "" : childNodeName);
			string sectionKey = sectionName + (childNodeName.Length == 0 ? ".Common" : "." + childNodeName);

			// Do this as a synchronized operation
			lock(typeof(WDA.HttpHandlers.ServiceTrace.Configuration))
			{
				// Initialize firt time execution
				if (Configuration.thisAssembly == null)
				{
					Configuration.thisAssembly = new WDA.Application.AssemblyInfo(System.Reflection.Assembly.GetExecutingAssembly());
				}

				// If the section/child has already been loaded, do nothing
				if (!Configuration.configurationIsLoaded.ContainsKey(sectionKey))
				{ 
					// Set intermediate values to be picked up by IConfigurationSectionHandler.Create
					Configuration.currentReader					= customReader;
					Configuration.currentChildNodeName	= childNodeName;

					// Request the configuration section 
					// This will result in the .Net framework calling back to IConfigurationSectionHandler.Create
					System.Configuration.ConfigurationSettings.GetConfig(sectionName);

					// Flag that this section/child has been read
					Configuration.configurationIsLoaded.Add(sectionKey, true);

					// Reset intermediat values before exiting
					Configuration.currentReader = null;
					Configuration.currentChildNodeName	= null;
				}
			}
		}

		/// <summary>Expose a AssemblyInfo object for executing assembly</summary>
		internal static WDA.Application.AssemblyInfo ThisAssembly	{get{return Configuration.thisAssembly;}}

		/// <summary>Configuration file setting wmbd.httpHandler/eventLogSource</summary>
		internal static string EventLogSource	{get{return Configuration.eventLogSource;}}

		/// <summary>Configuration file setting wmbd.httpHandler/maxTraceRecords</summary>
		internal static int MaxTraceRecords	{get{return Configuration.maxTraceRecords;}}

		// This function is called automatically when context.GetConfig("sectionName")
		// is called in LoadSettings. This wiring is done in Web.Config <configSections>.
		object IConfigurationSectionHandler.Create(object parent, Object configContext , System.Xml.XmlNode section )
		{ 
			WDA.Client.Types.IConfigurationReader configReader = new WDA.Client.Library.ConfigurationReader(section);
			if (Configuration.currentReader != null) 
			{
				// Call back to the custom reader
				if (Configuration.currentChildNodeName.Length > 0) 
				{
					// Call back with the child section
					Configuration.currentReader(configReader.Child(Configuration.currentChildNodeName));
				}
				else 
				{
					// Call back with the main section
					Configuration.currentReader(configReader);
				}
				return null;
			}
			else 
				// Read common settings
			{

				// Read the child node eventLogSource/@value
				Configuration.eventLogSource = Utl.SafeString(configReader.Child("eventLogSource").StringValue, "Application");
				Configuration.maxTraceRecords= Math.Min(1000, Math.Max(10, configReader.Child("maxTraceRecords").IntegerValue));

				return configReader;
			}
		}
	}
}
