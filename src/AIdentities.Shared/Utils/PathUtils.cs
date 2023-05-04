namespace AIdentities.Shared.Utils;
public static partial class PathUtils
{
   /// <summary>
   /// Maximum filename length for most file systems.
   /// </summary>
   const int MAX_FILE_NAME_LENGTH = 255;

   /// <summary>
   /// Returns true if the given path is a valid folder path.
   /// It assumes the input is a path and doesn't contain a filename.
   /// </summary>
   /// <param name="path">The path to check.</param>
   /// <returns>True if the path is valid, false otherwise.</returns>
   public static bool IsValidFolder(string path)
   {
      if (string.IsNullOrEmpty(path))
      {
         return false;
      }

      char[] invalidPathChars = Path.GetInvalidPathChars();

      if (path.Any(ch => invalidPathChars.Contains(ch))) return false;

      try
      {
         string fullPath = Path.GetFullPath(path);
         return true;
      }
      catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is PathTooLongException)
      {
         return false;
      }
   }

   /// <summary>
   /// Sanitize a filename by replacing invalid characters with an underscore and shortening the filename if it's too long.
   /// </summary>
   /// <param name="fileName">The filename to sanitize.</param>
   /// <param name="shortenIfNeeded">If true, the filename will be shortened if it's too long. If false, an exception will be thrown if the filename is too long.</param>
   /// <returns>The sanitized filename.</returns>
   /// <exception cref="ArgumentException">Thrown if the filename is null or empty or if it's too long and <paramref name="shortenIfNeeded"/> is false.</exception>
   public static string SanitizeFileName(string fileName, bool shortenIfNeeded = false)
   {

      if (string.IsNullOrEmpty(fileName)) throw new ArgumentException("The filename cannot be null or empty.");

      // Replace invalid characters with an underscore
      char[] invalidChars = Path.GetInvalidFileNameChars();
      string sanitizedFileName = new string(fileName.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray());


      if (sanitizedFileName.Length > MAX_FILE_NAME_LENGTH)
      {
         if (!shortenIfNeeded) throw new ArgumentException($"The filename '{sanitizedFileName}' is too long. The maximum length is {MAX_FILE_NAME_LENGTH} characters.");

         sanitizedFileName = sanitizedFileName.Substring(0, MAX_FILE_NAME_LENGTH);
      }

      return sanitizedFileName;
   }

   /// <summary>
   /// Sanitize a path by replacing invalid characters with an underscore and shortening the filename if it's too long.
   /// </summary>
   /// <param name="path">The path to sanitize.</param>
   /// <returns>The sanitized path.</returns>
   /// <exception cref="ArgumentException">Thrown if the path is null or empty.</exception>
   public static string SanitizePath(string path)
   {
      if (string.IsNullOrEmpty(path)) throw new ArgumentException("The path cannot be null or empty.");

      // Separate the path into directory and filename components
      string directory = Path.GetDirectoryName(path)!;
      string fileName = Path.GetFileName(path);

      // Sanitize directory
      char[] invalidPathChars = Path.GetInvalidPathChars();
      string sanitizedDirectory = new string(directory.Select(ch => invalidPathChars.Contains(ch) ? '_' : ch).ToArray());

      // Sanitize filename
      string sanitizedFileName = SanitizeFileName(fileName);

      // Combine sanitized directory and filename
      string sanitizedPath = Path.Combine(sanitizedDirectory, sanitizedFileName);

      return sanitizedPath;
   }

   /// <summary>
   /// Returns the absolute path for the given path.
   /// </summary>
   /// <param name="path">The path to convert to an absolute path.</param>
   /// <param name="defaultRootPath">The default root path to use if the given path is not rooted.</param>
   /// <returns>The absolute path.</returns>
   public static string GetAbsolutePath(string path, string defaultRootPath)
   {
      bool isPathRooted = Path.IsPathRooted(path);
      var absolutePath = isPathRooted ? path : Path.Combine(defaultRootPath, path);

      return absolutePath;
   }
}
