namespace AIdentities.Shared;

public class AppOptions
{
   public const string SECTION_NAME = "AIdentities";

   const string DEFAULT_PACKAGE_FOLDER = "plugins_packages";
   const string DEFAULT_ALLOWED_PLUGIN_RESOURCE_EXTENSIONS = ".dll,.pdb,.css,.js,.png,.jpg,.jpeg,.gif,.json,.txt,.csv";

   /// <summary>
   /// Whether to allow plugins to be installed and loaded.
   /// </summary>
   public bool AllowPlugins { get; set; }

   /// <summary>
   /// The folder where the plugins packages are uploaded.
   /// Can either be a relative path or an absolute path.
   /// </summary>
   public string PackageFolder { get; set; } = DEFAULT_PACKAGE_FOLDER;

   /// <summary>
   /// The extensions of the plugins packages resources that are allowed to be uploaded.
   /// </summary>
   public string[] AllowedPluginResourceExtensions { get; set; } = DEFAULT_ALLOWED_PLUGIN_RESOURCE_EXTENSIONS.Split(',');

   public DiagnosticOptions Diagnostic { get; set; } = new DiagnosticOptions();

   public class DiagnosticOptions
   {
      public bool LogNotifications { get; set; } = true;
   }
}
