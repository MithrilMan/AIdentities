using System.Diagnostics.CodeAnalysis;

namespace AIdentities.Chat.Skills.CreateStableDiffusionPrompt;
public partial class CreateStableDiffusionPrompt : Skill
{
   readonly ILogger<CreateStableDiffusionPrompt> _logger;
   readonly IAIdentityProvider _aIdentityProvider;

   public CreateStableDiffusionPrompt(ILogger<CreateStableDiffusionPrompt> logger, IAIdentityProvider aIdentityProvider)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;

   }

   protected override IAsyncEnumerable<Thought> ExecuteAsync(SkillExecutionContext context, CancellationToken cancellationToken)
   {


   }
}
