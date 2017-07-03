using System;
using System.Collections;
using System.Configuration;
using System.Reflection;
using System.Web;
using System.Xml;
using WDA.Application;

namespace WDA.HttpHandlers.ServiceTrace
{
	using This = Configuration;
	/// <summary>
	/// Configuration handler for reading the Web.Config section "WDA.HttpHandlers"
	/// </summary>
	internal class Configuration : IConfigurationSectionHandler
	{
		// Wrap executing assembly to expose information
		private static WDA.Application.AssemblyInfo _thisAssembly = null;

		// This hash table is used to keep track of what configuration sections/child sections has already been loaded
		private static readonly Hashtable configurationIsLoaded = new Hashtable();

		// These variables is used for transfering values from LoadSettings to IConfigurationSectionHandler.Create
		// As soon as LoadSettings is done, these values are no longer valid.
		private static string _currentChildNodeName = "";
		private static ConfigurationReaderDelegate _currentReader = null;

		/// <summary>
		/// A delegate for configuration reader callback.
		/// The different modules of this project may decide to read their own configuration.
		/// In that case they call the LoadSettings specifying what node to read and a delegate to receive the callback.
		/// </summary>
		internal delegate void ConfigurationReaderDelegate(WDA.Application.WebConfigEx.Reader reader);

		/// <summary>
		/// Static function responsible of loading config settings from the web.config file.
		/// If the specified settings section has already been loaded, this call is ignored.
		/// </summary>
		/// <param name="context">The HttpContext of the caller.</param>
		internal static void LoadSettings(HttpContext context)
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
		internal static void LoadSettings(HttpContext context, string childNodeName, ConfigurationReaderDelegate customReader)
		{
			const string sectionName = "WDA.HttpHandlers";
			childNodeName = (childNodeName ?? "");
			string sectionKey = sectionName + (childNodeName.Length == 0 ? ".Common" : "." + childNodeName);

			// Do this as a synchronized operation
			lock(typeof(WDA.HttpHandlers.ServiceTrace.Configuration))
			{
				// Initialize firt time execution
				if (_thisAssembly == null)
				{
					_thisAssembly = new WDA.Application.AssemblyInfo(Assembly.GetExecutingAssembly());
				}

				// If the section/child has already been loaded, do nothing
				if (!Configuration.configurationIsLoaded.ContainsKey(sectionKey))
				{
					// Set intermediate values to be picked up by IConfigurationSectionHandler.Create
					_currentReader					= customReader;
					_currentChildNodeName	= childNodeName;

					// Request the configuration section
					// This will result in the .Net framework calling back to IConfigurationSectionHandler.Create
					ConfigurationManager.GetSection(sectionName);

					// Flag that this section/child has been read
					Configuration.configurationIsLoaded.Add(sectionKey, true);

					// Reset intermediat values before exiting
					_currentReader = null;
					_currentChildNodeName	= null;
				}
			}
		}

		/// <summary>Expose a AssemblyInfo object for executing assembly</summary>
		internal static WDA.Application.AssemblyInfo ThisAssembly	{get{return _thisAssembly;}}

		/// <summary>Configuration file setting wmbd.httpHandler/eventLogSource</summary>
		internal static string EventLogSource { get; private set; }

		/// <summary>Configuration file setting wmbd.httpHandler/maxTraceRecords</summary>
		internal static int MaxTraceRecords { get; private set; }

		/// <summary>Maximum number of seconds allowed for updating/loading a service trace.</summary>
		internal static int SynchWaitMaxSeconds { get; private set; }

		/// <summary>Configuration file setting wmbd.httpHandler/debug</summary>
		internal static bool Debug { get; private set; }

		// This function is called automatically when context.GetConfig("sectionName")
		// is called in LoadSettings. This wiring is done in Web.Config <configSections>.
		object IConfigurationSectionHandler.Create(object parent, Object configContext , XmlNode section)
		{
			var configReader = new WDA.Application.WebConfigEx.Reader(section);
			if (_currentReader != null)
			{
				// Call back to the custom reader
				if (_currentChildNodeName.Length > 0)
				{
					// Call back with the child section
					_currentReader(configReader.Child(_currentChildNodeName));
				}
				else
				{
					// Call back with the main section
					_currentReader(configReader);
				}
				return null;
			}
			else
			{
				// Read common settings
				// Read the child node eventLogSource/@value
				This.EventLogSource = Utl.SafeString(configReader.Child("eventLogSource").StringValueAttribute.Value("Application"));
				This.MaxTraceRecords= Math.Min(1000, Math.Max(10, configReader.Child("maxTraceRecords").IntegerValueAttribute.Value(100)));
				This.SynchWaitMaxSeconds = Math.Min(1000, Math.Max(10, configReader.Child("synchWaitMaxSeconds").IntegerValueAttribute.Value(100)));
				This.Debug = configReader.Child("debug").BoolValueAttribute.Value(false);
				return configReader;
			}
		}
	}
}
