#region Copyright 2014 Andrey Kurnoskin, Mike Goatly
// This source is subject to the the MIT License (MIT)
// All rights reserved.
#endregion

using System;
using System.Reflection;
using System.Runtime.InteropServices;

#if DEBUG
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyConfiguration("RELEASE")]
#endif

[assembly: AssemblyCompany("Andrey Kurnoskin")]
[assembly: AssemblyProduct("DynaCache.Extended")]
[assembly: AssemblyCopyright("Copyright © Andrey Kurnoskin, Mike Goatly 2012-2014")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: CLSCompliant(true)]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.1.3.0")]
