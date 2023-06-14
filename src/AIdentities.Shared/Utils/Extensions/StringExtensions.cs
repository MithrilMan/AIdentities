using System.Text;

namespace System;

public static class StringExtensions
{
   /// <summary>
   /// Returns the string as a single line string.
   /// If trim is true, the returned string is trimmed.
   /// </summary>
   /// <param name="text">The string to convert.</param>
   /// <param name="trim">True to return a trimmed single line string.</param>
   /// <returns>
   /// The string as a single line string.
   /// If the text is null or empty, returns an empty string.
   /// </returns>
   public static string AsSingleLine(this string? text, bool trim = true)
   {
      if (string.IsNullOrWhiteSpace(text))
         return string.Empty;

      var sb = new StringBuilder(trim ? text.TrimStart() : text)
         .Replace("\r\n", "")
         .Replace("\n", "");

      if (trim)
      {
         int i = sb.Length - 1;
         for (; i >= 0; i--)
         {
            if (!char.IsWhiteSpace(sb[i])) break;
         }

         if (i < sb.Length - 1)
         {
            sb.Length = i + 1;
         }
      }

      return sb.ToString();
   }
}
