using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace AIdentities.UI.Features.Core.Services.Authorization;

internal static class TokenGenerator
{
   public static async Task<dynamic> GenerateJwtToken(User user, string jwtsecret)
   {
      if (string.IsNullOrWhiteSpace(user.Username))
         throw new ArgumentNullException(nameof(user.Username));

      var claims = new List<Claim> {
         new Claim(ClaimTypes.Name, user.Username),
         new Claim(JwtRegisteredClaimNames.Nbf, DateTimeOffset.Now.ToUnixTimeSeconds().ToString()),
         new Claim(JwtRegisteredClaimNames.Exp, DateTimeOffset.Now.AddHours(12).ToUnixTimeSeconds().ToString())
      };

      var token = new JwtSecurityToken(
          new JwtHeader(
              new SigningCredentials(
                  new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtsecret)), SecurityAlgorithms.HmacSha256)),
          new JwtPayload(claims));

      var output = new
      {
         Access_Token = new JwtSecurityTokenHandler().WriteToken(token),
         user.Username,
      };

      return await Task.FromResult(output).ConfigureAwait(false);
   }
}
