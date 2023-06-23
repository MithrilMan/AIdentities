using System.Reflection.Metadata;
using AIdentities.Shared.Plugins.Connectors.Completion;
using Fluid;
using Fluid.Ast;

namespace AIdentities.Chat.Skills.CreateStableDiffusionPrompt;

public partial class CreateStableDiffusionPrompt : Skill
{
   /// <summary>
   /// We expect the conversation history to be in the context with this key.
   /// </summary>
   public const string CONVERSATION_HISTORY_KEY = nameof(CognitiveChatMissionContext.ConversationHistory);

   private IFluidTemplate _defaultRequestSummary = default!;
   private IFluidTemplate _defaultTemplate = default!;

   public CreateStableDiffusionPrompt(ILogger<CreateStableDiffusionPrompt> logger, IAIdentityProvider aIdentityProvider, FluidParser templateParser)
      : base(logger, aIdentityProvider, templateParser) { }

   protected override void CreateDefaultPromptTemplates()
   {
      _defaultRequestSummary = TemplateParser.Parse(PROMPT_REQUEST_SUMMARY);
      _defaultTemplate = TemplateParser.Parse(PROMPT);

      // Register the ConversationMessage type so that it can be used in the template
      TemplateOptions.Default.MemberAccessStrategy.Register<ConversationMessage>();
   }

   protected override async IAsyncEnumerable<Thought> ExecuteAsync(SkillExecutionContext context,
                                                                   [EnumeratorCancellation] CancellationToken cancellationToken)
   {
      var completionConnector = context.GetDefaultCompletionConnector();

      var templateContext = CreateTemplateContext(context);

      yield return context.ActionThought($"Creating a summary of the request");
      var requestSummary = await completionConnector.RequestCompletionAsync(new DefaultCompletionRequest
      {
         Prompt = _defaultRequestSummary.Render(templateContext),
      }, cancellationToken).ConfigureAwait(false);
      yield return context.ActionThought(requestSummary?.GeneratedMessage ?? "");

      templateContext.SetValue("RequestSummary", requestSummary?.GeneratedMessage ?? "");

      var prompt = _defaultTemplate.Render(templateContext);
      yield return context.ActionThought($"Creating a stable diffusion prompt for {context.AIdentity.Name}");
      var responses = await completionConnector.RequestCompletionAsStreamAsync(new DefaultCompletionRequest
      {
         Prompt = prompt,
      }, cancellationToken).ToListAsync(cancellationToken).ConfigureAwait(false);

      var response = string.Join("", responses.Select(r => r.GeneratedMessage));
      SetGeneratedPrompt(context, response);
      yield return context.ActionThought($"I've stored the prompts in SetGeneratedPrompt");

      yield return context.FinalThought(response);
   }

   private TemplateContext CreateTemplateContext(SkillExecutionContext context)
   {
      var aidentity = context.AIdentity;
      var chatFeature = aidentity.Features.Get<AIdentityChatFeature>();

      // check in the context if there is a conversation history
      if (!TryExtractFromContext<IConversationHistory>(CONVERSATION_HISTORY_KEY, context, out var conversationHistory))
      {
         Logger.LogWarning("No conversation history found in the context, using the prompt instead");
      }

      IEnumerable<ConversationMessage>? history = null;
      if (conversationHistory is not null)
      {
         history = conversationHistory.GetConversationHistory(aidentity, null);
      }
      else
      {
         var prompt = context.PromptChain.Peek();
         var conversationMessage = prompt switch
         {
            UserPrompt userPrompt => new ConversationMessage(prompt.Text, userPrompt.UserId, "User"),
            AIdentityPrompt aIdentityPrompt => new ConversationMessage(prompt.Text, AIdentityProvider.Get(aIdentityPrompt.AIdentityId) ?? aidentity),
            _ => null
         };

         if (conversationMessage is not null)
         {
            history = new ConversationMessage[] { conversationMessage };
         }
      }

      var templateContext = new TemplateContext(
            new
            {
               AIdentityName = aidentity.Name.AsSingleLine(),
               Personality = aidentity.Personality.AsSingleLine(),
               Background = chatFeature?.Background.AsSingleLine(),
               PromptsCount = TryExtractFromContext<int>("PromptsCount", context, out var promptsCount) ? promptsCount : 1,
               PromptSpecifications = PromptSpecifications(context),
               ConversationHistory = history ?? Array.Empty<ConversationMessage>(),
            });

      return templateContext;
   }
}
