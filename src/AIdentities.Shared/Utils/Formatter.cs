namespace AIdentities.Shared.Utils;
public static partial class Formatter
{
   public static string FormatFileSize(long bytes) => bytes switch
   {
      < 1024 => $"{bytes} B",
      < 1048576 => $"{bytes / 1024:0.##} KB",
      < 1073741824 => $"{bytes / 1048576:0.##} MB",
      _ => $"{bytes / 1073741824:0.##} GB"
   };
}
