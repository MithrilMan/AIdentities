using Xunit;
namespace AIdentities.Shared.Utils.Tests;

public class PathUtilsTests
{
   [Theory]
   [InlineData(@"C:\TestFolder", true)]
   [InlineData(@"\\server\share\folder", true)]
   [InlineData("invalid|path", false)]
   [InlineData("", false)]
   public void IsValidFolder_ValidatesCorrectly(string path, bool expectedResult)
   {
      bool result = PathUtils.IsValidFolder(path);
      Assert.Equal(expectedResult, result);
   }

   public static IEnumerable<object?[]> SanitizeFileName_TestCases
   {
      get
      {
         yield return new object?[] { "test.txt", true, "test.txt" };
         yield return new object?[] { "invalid|file.txt", true, "invalid_file.txt" };
         yield return new object?[] { "a" + new string('a', 255), true, "a" + new string('a', 254) };
         yield return new object?[] { "a" + new string('a', 255), false, null };
      }
   }

   [Theory]
   [MemberData(nameof(SanitizeFileName_TestCases))]
   public void SanitizeFileName_SanitizesCorrectly(string fileName, bool shortenIfNeeded, string expectedResult)
   {
      if (expectedResult == null)
      {
         Assert.Throws<ArgumentException>(() => PathUtils.SanitizeFileName(fileName, shortenIfNeeded));
      }
      else
      {
         string result = PathUtils.SanitizeFileName(fileName, shortenIfNeeded);
         Assert.Equal(expectedResult, result);
      }
   }

   [Theory]
   [InlineData(@"C:\TestFolder\test.txt", @"C:\TestFolder\test.txt")]
   [InlineData(@"C:\Invalid|Path\invalid|file.txt", @"C:\Invalid_Path\invalid_file.txt")]
   public void SanitizePath_SanitizesCorrectly(string path, string expectedResult)
   {
      string result = PathUtils.SanitizePath(path);
      Assert.Equal(expectedResult, result);
   }

   [Theory]
   [InlineData(@"C:\TestFolder\file.txt", @"C:\", @"C:\TestFolder\file.txt")]
   [InlineData(@"file.txt", @"C:\TestFolder", @"C:\TestFolder\file.txt")]
   public void GetAbsolutePath_GeneratesCorrectPath(string path, string defaultRootPath, string expectedResult)
   {
      string result = PathUtils.GetAbsolutePath(path, defaultRootPath);
      Assert.Equal(expectedResult, result);
   }
}
