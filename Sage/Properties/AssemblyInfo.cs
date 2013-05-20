using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Sage")]
[assembly: AssemblyDescription("Provides the framework for Sage projects")]

[assembly: ComVisible(false)]

[assembly: Guid("26d37712-4f75-4279-a483-203719a54708")]

[assembly: InternalsVisibleTo("Sage.Test")]
[assembly: InternalsVisibleTo("Sage.Tools")]
[assembly: InternalsVisibleTo("Sage.Explorables")]

// configure logging from the file specified by System.Configuration.ConfigurationSettings for the AppDomain, and watch for changes
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]

