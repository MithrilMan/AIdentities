namespace AIdentities.Shared.Common;
public abstract record Entity
{
   public Guid Id { get; set; } = Guid.NewGuid();
}
