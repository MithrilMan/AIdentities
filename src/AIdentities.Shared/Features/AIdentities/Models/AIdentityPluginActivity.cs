using static AIdentities.Shared.Features.AIdentities.Models.AIdentityPluginActivity;

namespace AIdentities.Shared.Features.AIdentities.Models;

/// <summary>
/// A key-value collection representing the activity of an AIdentity regarding a specific plugin.
/// E.g. if a plugin is used to chat and a specific AIdentity has been used to have some conversations,
/// the plugin can report back the number of conversations the AIdentity has had.
/// </summary>
public abstract class AIdentityPluginActivity : Dictionary<string, PluginActivityDetail>
{
   public record PluginActivityDetail(int Count, string Description);
}

