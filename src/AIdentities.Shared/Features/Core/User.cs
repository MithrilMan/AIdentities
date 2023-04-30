using System.ComponentModel.DataAnnotations;
using AIdentities.Shared.Common;

namespace AIdentities.Shared.Features.Core;

public record User : Entity
{
   public DateTime? DateCreated { get; set; }
   [Required] public string? Username { get; set; }
   [Required] public string? Email { get; set; }
   [Required] public string? Password { get; set; }
   public string? AvatarUrl { get; set; }
   public string? Token { get; set; }
}
