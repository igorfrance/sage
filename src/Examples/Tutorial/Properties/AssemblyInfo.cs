﻿using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyTitle("Sage.Tutorial")]
[assembly: AssemblyDescription("Provides an example Sage project")]

[assembly: ComVisible(false)]
[assembly: Guid("6fcf3f53-a41c-44ec-8559-9b826c401211")]

[assembly: AssemblyVersion("1.0.*")]

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.xml", Watch = true)]