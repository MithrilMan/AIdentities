namespace AIdentities.Shared;

public static class AppConstants
{
   public const string APP_TITLE = "AIdentities - Road to AGI";

   public static class LocalStorage
   {
      /// <summary>
      /// The local storage key for the current theme.
      /// </summary>
      public const string THEME = "theme";
   }

   public static class SpecialFolders
   {
      /// <summary>
      /// The name of the folder where the plugin storage is located.
      /// </summary>
      public const string STORAGE = "_storage_";

      /// <summary>
      /// The name of the folder where the plugin assets are located once extracted from the plugin package.
      /// </summary>
      public const string PLUGINS = "_plugins_";

      /// <summary>
      /// This folder contains the plugin assets extracted from the plugin package that are waiting to be installed.
      /// </summary>
      public const string PLUGIN_UPDATES = "_updates_";

      /// <summary>
      /// The name of the folder where the aidentities are located.
      /// It's a subfolder of <see cref="STORAGE"/>.
      /// </summary>
      public const string AIDENTITIES = "aidentities";
   }
}
