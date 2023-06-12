using Fluid;

namespace AIdentities.Chat.Skills.CreateStableDiffusionPrompt;

public partial class CreateStableDiffusionPrompt : Skill
{
   private IFluidTemplate _defaultTemplate = default!;

   public CreateStableDiffusionPrompt(ILogger<CreateStableDiffusionPrompt> logger, IAIdentityProvider aIdentityProvider, FluidParser templateParser)
      : base(logger, aIdentityProvider, templateParser) { }

   protected override void CreateDefaultPromptTemplates()
   {
      _defaultTemplate = TemplateParser.Parse(PROMPT);
   }

   protected override IAsyncEnumerable<Thought> ExecuteAsync(SkillExecutionContext context, CancellationToken cancellationToken)
   {
      return Enumerable.Empty<Thought>().ToAsyncEnumerable();
   }

   private object CreateTemplateModel(SkillExecutionContext context) => new
   {
      PromptSpecifications = PromptSpecifications(context),
   };
}
