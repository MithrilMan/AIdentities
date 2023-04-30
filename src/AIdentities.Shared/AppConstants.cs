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
      /// It's a subfolder of <see cref="STORAGE"/>.
      /// </summary>
      public const string AIDENTITIES = "aidentities";
   }
}
