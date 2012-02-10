using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Kelp.Test")]
[assembly: AssemblyDescription("Provides unit tests for Kelp")]

[assembly: ComVisible(false)]
[assembly: Guid("9c8a88d6-a59c-4f02-9e7c-fa81e101febc")]

// configure logging from the file specified by System.Configuration.ConfigurationSettings for the AppDomain, and watch for changes
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]
