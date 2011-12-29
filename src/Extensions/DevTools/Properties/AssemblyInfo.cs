using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Sage")]
[assembly: AssemblyDescription("Provides the framework for Sage projects")]

[assembly: ComVisible(false)]
[assembly: Guid("5d93622d-b28f-4c94-8418-f7309f464f85")]

[assembly: AssemblyVersion("1.0.*")]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]
