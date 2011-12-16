using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Sage Batch Globalizer")]
[assembly: AssemblyDescription("Provides a utility for batch processing globalizable resources within a Sage category")]

[assembly: ComVisible(false)]
[assembly: Guid("cc056797-064d-463c-8b14-29a400f6b49a")]

// configure logging from the file specified by System.Configuration.ConfigurationSettings for the AppDomain, and watch for changes
[assembly: log4net.Config.XmlConfigurator]
