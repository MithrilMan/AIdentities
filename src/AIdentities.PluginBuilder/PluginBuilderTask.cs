using System.IO;
using Microsoft.Build.Framework;
using Microsoft.Build.Utilities;

namespace AIdentities.PluginBuilder;

public class PluginBuilderTask2 : Task
{
   [Required]
   public string AssemblyName { get; set; }

   [Required]
   public string ProjectFile { get; set; }

   [Required]
   public string IntermediateOutputPath { get; set; }

   [Required]
   public ITaskItem[] PublishBlazorBootStaticWebAsset { get; set; }

   [Required]
   public string BundlePath { get; set; }

   [Output]
   public ITaskItem[] Extension { get; set; }

   public override bool Execute()
   {
      Log.LogError("Hello, from MyCustomTask, again! " + AssemblyName);

      string cssPath = Path.Combine(IntermediateOutputPath, "scopedcss", "projectbundle", $"{AssemblyName}.bundle.scp.css");
      if (File.Exists(cssPath))
      {
         Log.LogError("ESISTE! => " + cssPath);
      }
      Log.LogError("Hello, from MyCustomTask, again! " + cssPath);

      return true;
   }
}



/// <summary>
/// automatically generates the Plugin aid-manifest.json file
/// </summary>
public class ManifestGenerator : ToolTask
{
   /// <summary>
   /// 
   /// </summary>
   [Required] public string PackageOutputPath { get; set; }

   protected override string ToolName => throw new System.NotImplementedException();

   protected override string GenerateFullPathToTool()
   {
      throw new System.NotImplementedException();
   }
}

public class GenerateBlazorServerLibraryPackage : Task
{
   /// <summary>
   /// Specify the target framework for the package
   /// MSBuild property for that is $(TargetFramework)
   /// </summary>
   [Required] public string TargetFramework { get; set; }

   /// <summary>
   /// Specify the project directory, used to fetch wwwroot content.
   /// MSBuild property for that is $(ProjectDir)
   /// </summary>
   [Required] public string ProjectDirectory { get; set; }

   /// <summary>
   /// Specify the configuration for the package.
   /// MSBuild property for that is $(Configuration)
   /// </summary>
   [Required] public string Configuration { get; set; }

   /// <summary>
   /// Specify where the package should be created
   /// MSBuild property for that is $(OutputPath)
   /// </summary>
   [Required] public string OutputPath { get; set; }

   /// <summary>
   /// Specify the name of the assembly.
   /// This is the same as the project name.
   /// MSBuild property for that is $(AssemblyName)
   /// </summary>
   [Required] public string AssemblyName { get; set; }

   /// <summary>
   /// Specify the path to the intermediate output folder.
   /// This is the obj folder in the project root.
   /// MSBuild property for that is $(IntermediateOutputPath)
   /// </summary>
   [Required] public string IntermediateOutputPath { get; set; }

   /// <summary>
   /// The path to the folder where the package should be created, within OutputPath.
   /// By default it's set to `package`.
   /// </summary>
   public string PackageSubfolder { get; set; } = "package";

   public override bool Execute()
   {
      // Implement the required logic here
      // 1. Create the custom folder structure
      // 2. Move scoped CSS and JS files to their respective folders
      // 3. Keep the original wwwroot content in the same path
      // 4. Include referenced assemblies in the package
      // 5. Create an aid-manifest.json file with a list of all included resources
      // 6. Move aid-manifest.json to the root folder of the folder structure
      // 7. Create a nupkg


      // 1. Create the custom folder structure
      string packageRoot = Path.Combine(OutputPath, PackageSubfolder);
      Directory.CreateDirectory(packageRoot);



      return true;
   }

   /// <summary>
   /// Gets the path to the scoped CSS file.
   /// </summary>
   /// <returns></returns>
   public string GetScopedCssPath()
      => Path.Combine(IntermediateOutputPath, "scopedcss", "projectbundle", $"{AssemblyName}.bundle.scp.css");
}
