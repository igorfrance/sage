using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Sage.Build")]
[assembly: AssemblyDescription("Provides utilities for offline building of Sage resources and extensions")]

[assembly: ComVisible(false)]
[assembly: Guid("cc056797-064d-463c-8b14-29a400f6b49a")]

// configure logging from the file specified by System.Configuration.ConfigurationSettings for the AppDomain, and watch for changes
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]
