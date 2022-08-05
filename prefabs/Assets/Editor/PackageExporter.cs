using System.IO;
using UnityEditor;

public static class PackageExporter
{
	private const string name = "DecentM.Prefabs";

	private static string[] paths = new string[]
	{
		"Assets/DecentM",
		"Assets/Packages",
		"Assets/UserLibs"
	};

	[MenuItem("DecentM/PackageExporter/Export Package")]
	public static void ExportPackage()
	{
		string output = $"./output/{name}.unitypackage";

		DirectoryInfo dir = new FileInfo(output).Directory;

		if (dir != null && !dir.Exists)
			dir.Create();

		AssetDatabase.ExportPackage(
			paths,
			output,
			ExportPackageOptions.Recurse
		);
	}
}
