namespace AIdentities.Shared.Features.Automation.Models;

public class Duty
{
   public string UserName { get; set; } = string.Empty;
   public Guid Id { get; set; } = Guid.NewGuid();
   public DateTimeOffset CreationDate { get; set; } = DateTimeOffset.Now;
   public string? Title { get; set; }
   public string? Description { get; set; }
}
