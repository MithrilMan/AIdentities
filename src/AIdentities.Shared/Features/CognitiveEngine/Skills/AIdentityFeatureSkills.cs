namespace AIdentities.Shared.Features.CognitiveEngine.Skills;

/// <summary>
/// This feature allows users to enable or disable skills for an AIdentity
/// and customize the settings for each skill that have customizable settings.
/// By default, skills are disabled.
/// </summary>
public record AIdentityFeatureSkills : IAIdentityFeature
{
   public bool AreSkillsEnabled { get; set; } = false;

   public List<string> EnabledSkills { get; set; } = new();
}
