using System.Text;

namespace AIdentities.Shared.Utils;
public static partial class PathUtils
{
   public static bool IsValidPath(string path)
   {
      if (string.IsNullOrEmpty(path))
         return false;

      string fileNamePart = path;
      if (fileNamePart.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
         return false;

      return true;
   }

   public static string SanitizePath(string path)
   {
      if (string.IsNullOrEmpty(path)) return path;

      string fileName = Path.GetFileName(path);
      if (string.IsNullOrEmpty(fileName)) return path;

      var sb = new StringBuilder(fileName.Length);
      foreach (char c in fileName)
      {
         if (Array.IndexOf(Path.GetInvalidFileNameChars(), c) < 0)
         {
            sb.Append(c);
         }
      }
      return Path.Combine(Path.GetDirectoryName(path)!, sb.ToString());
   }

   public static string GetAbsolutePath(string path, string defaultRootPath)
   {
      bool isPathRooted = Path.IsPathRooted(path);
      var absolutePath = isPathRooted ? path : Path.Combine(defaultRootPath, path);

      return absolutePath;
   }
}
