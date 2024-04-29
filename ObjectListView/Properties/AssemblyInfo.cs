using System.Reflection;
using System.Runtime.InteropServices;

[assembly: System.CLSCompliant(true)]
[assembly: Guid("ef28c7a8-77ae-442d-abc3-bb023fa31e57")]
[assembly: ComVisible(false)]

#if NETCOREAPP
[assembly: AssemblyMetadata("RepositoryUrl", "https://github.com/DKorablin/ObjectListView")]
#else
[assembly: AssemblyTitle("ObjectListView")]
[assembly: AssemblyDescription("A much easier to use ListView and friends")]
[assembly: AssemblyCompany("Bright Ideas Software")]
[assembly: AssemblyProduct("ObjectListView")]
[assembly: AssemblyCopyright("Copyright ©  2006-2016")]

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

//[assembly: AssemblyVersion("2.9.1.*")]
//[assembly: AssemblyFileVersion("2.9.1.0")]
//[assembly: AssemblyInformationalVersion("2.9.1")]
#endif