using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Kelp")]
[assembly: AssemblyDescription("Provides various common utilities work working with .NET and .NET web applications.")]

[assembly: ComVisible(false)]
[assembly: Guid("d3cfe3ea-1ddd-44d2-b024-59f45e966084")]

// configure logging from the file specified by System.Configuration.ConfigurationSettings for the AppDomain, and watch for changes
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]
[assembly: InternalsVisibleTo("Kelp.Test")]
