using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using MelonLoader;
using DecentM.Mods.NoSteamRestart;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NoSteamRestart")]
[assembly: AssemblyDescription("Prevents games using the C# Steamworks API from restarting under Steam")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("DecentM")]
[assembly: AssemblyProduct("NoSteamRestart")]
[assembly: AssemblyCopyright("Copyright © DecentM 2022")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("6e0ff123-f41b-4c4f-a091-d46cdfa5554b")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("1.0.0.0")]

[assembly: MelonInfo(typeof(Mod), "NoSteamRestart", "1.0.0", "DecentM")]
[assembly: MelonGame(null, null)] // FIXME: Figure out what the heck are the values for CVR
