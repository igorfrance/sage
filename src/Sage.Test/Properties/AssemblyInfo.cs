using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Sage.Test")]
[assembly: AssemblyDescription("Provides unit tests for Sage")]

[assembly: ComVisible(false)]

[assembly: Guid("8ae75efd-e555-425c-85f3-ba634d2df6ab")]

// configure logging from the file specified by System.Configuration.ConfigurationSettings for the AppDomain, and watch for changes
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]
