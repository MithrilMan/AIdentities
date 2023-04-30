using AIdentities.UI.Features.Core.Services.Authorization;

namespace AIdentities.Test.Services.Authorization;

/// <summary>
/// Tests for <see cref="CryptographyHelper"/> class
/// </summary>
public class CryptographyHelperTests
{
   [Fact()]
   public void SecurePasswordTest()
   {
      //Arrange
      var password = "password";
      //Act
      var securePassword = CryptographyHelper.SecurePassword(password);
      //Assert
      Assert.NotNull(securePassword);
   }

   [Fact()]
   public void ComparePasswordTest()
   {
      //Arrange
      var password = "password";
      var securePassword = CryptographyHelper.SecurePassword(password);
      //Act
      var comparePassword = CryptographyHelper.ComparePassword(password, securePassword);
      //Assert
      Assert.True(comparePassword);
   }
}
