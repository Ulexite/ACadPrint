﻿#region Using directives

using System.Reflection;
using System.Runtime.InteropServices;
using Autodesk.AutoCAD.Runtime;

#endregion

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("CSPDS")]
[assembly: AssemblyDescription("AutoCAD plugin. Печать форматов СПДС из модели")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("")]
[assembly: AssemblyProduct("CSPDS")]
[assembly: AssemblyCopyright("Copyright 2019")]
[assembly: AssemblyTrademark("ЦПП")]
[assembly: AssemblyCulture("")]
[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log4net.config")]

// This sets the default COM visibility of types in the assembly to invisible.
// If you need to expose a type to COM, use [ComVisible(true)] on that type.
[assembly: ComVisible(false)]

// The assembly version has following format :
//
// Major.Minor.Build.Revision
//
// You can specify all the values or you can use the default the Revision and 
// Build Numbers by using the '*' as shown below:
[assembly: AssemblyVersion("1.0.0.0")]
