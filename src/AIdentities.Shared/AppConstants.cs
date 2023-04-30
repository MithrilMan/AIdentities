namespace AIdentities.Shared;

public static class AppConstants
{
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
      /// The name of the folder where the aidentities are located.
      /// </summary>
      public const string AIDENTITIES = "_aidentities_";

      public static string[] ALL_FOLDERS = new[] { STORAGE, AIDENTITIES };
   }
}
