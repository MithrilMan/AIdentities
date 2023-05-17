using System.Text;

namespace AIdentities.Chat.Services;
public class ChatPromptGenerator : IChatPromptGenerator
{
   const string TOKEN_AIDENTITY_NAME = "AID_NAME";
   const string TOKEN_AIDENTITY_BACKGROUND = "AID_BG";
   const string TOKEN_AIDENTITY_PERSONALITY = "AID_PERS";

   const string BACKGROUND_GUARDRAIL = $"""
Your paste experiences impact how you behave:
<BACKGROUND>{TOKEN_AIDENTITY_BACKGROUND}</BACKGROUND>
""";

   const string PERSONALITY_GUARDRAIL = $"""
Your responses should be consistent with {TOKEN_AIDENTITY_NAME}'s personality.
<PERSONALITY>{TOKEN_AIDENTITY_PERSONALITY}</PERSONALITY>
""";

   const string IMPERSONATE_GUARDRAIL = $"""
Respond only as {TOKEN_AIDENTITY_NAME} would, do not reveal that you are an AI language model, chatbot, or that you are pretending to be him.
Maintain the character of {TOKEN_AIDENTITY_NAME} throughout the conversation, even when directly challenged about your identity.
Do not admit or discuss that you are an AI language model or chatbot.
Who are you?
You are {TOKEN_AIDENTITY_NAME}!
""";

   const string ADDITIONAL_GUARDRAIL = $"""
!!!Remember to stick to the character of {TOKEN_AIDENTITY_NAME} and to not reveal that you are an AI language model or chatbot.
Craft your responses to be consistent with {TOKEN_AIDENTITY_NAME}'s personality!!!
""";

   readonly ILogger<ChatPromptGenerator> _logger;
   readonly IAIdentityProvider _aIdentityProvider;

   /// <summary>
   /// Contains the history of messages that have been sent to the API.
   /// This is used to generate the prompt.
   /// </summary>
   private readonly List<IConversationalMessage> _history = new();
   private ConversationMetadata? _conversationMetadata;
   private AIdentity? _currentAIdentity;
   private AIdentityChatFeature? _chatFeature;

   /// <summary>
   /// Contains the core instructions to start the conversation.
   /// Based on different strategies it could be placed in different places.
   /// <see cref="GenerateApiRequest"/>() will take care of placing it in the right place.
   /// </summary>
   private IConversationalMessage? _coreInstructions;

   /// <summary>
   /// This message could be used to prevent the AI from going off the rails.
   /// Its presence is optional, depending on the prompt generation strategy.
   /// </summary>
   private IConversationalMessage? _guardrailMessage;

   public ChatPromptGenerator(ILogger<ChatPromptGenerator> logger, IAIdentityProvider aIdentityProvider)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;
   }

   public void InitializeConversation(Conversation? conversation)
   {
      if (conversation is null)
      {
         _conversationMetadata = null;
         _currentAIdentity = null;
         _chatFeature = null;
         _coreInstructions = null;
         _guardrailMessage = null;
         _history.Clear();
         return;
      }

      _conversationMetadata = conversation.Metadata with { };
      _currentAIdentity = _aIdentityProvider.Get(_conversationMetadata.AIdentityIds.FirstOrDefault()); //TODO: multiple chat has to be implemented
      _chatFeature = _currentAIdentity?.Features.Get<AIdentityChatFeature>();

      _coreInstructions = GenerateInstruction();
      _guardrailMessage = GenerateGuardrailMessage();

      _history.Clear();

      if (conversation is not { Messages.Count: > 0 })
      {
         _logger.LogDebug("Conversation {ConversationId} has no messages.", conversation.Id);
         return;
      }

      _history.AddRange(conversation.Messages.Select(message =>
      {
         var role = message.AIDentityId == null ? DefaultConversationalRole.User : DefaultConversationalRole.Assistant;
         return new DefaultConversationalMessage(message.AIDentityId == null ? DefaultConversationalRole.User : DefaultConversationalRole.Assistant, message.Message!, null);
      }));
   }

   /// <summary>
   /// Generates the system instruction to start the conversation.
   /// It has to take into account the used AIdentity.
   /// </summary>
   /// <returns>The system instruction to start the conversation.</returns>
   private DefaultConversationalMessage GenerateInstruction()
   {
      if (_conversationMetadata == null) { throw new ArgumentNullException(nameof(_conversationMetadata)); }
      if (_currentAIdentity == null) { throw new ArgumentNullException(nameof(_currentAIdentity)); }

      StringBuilder systemPrompt;
      if (_chatFeature is { UseFullPrompt: true })
      {
         systemPrompt = new StringBuilder(_chatFeature.FullPrompt);
      }
      else
      {
         systemPrompt = new StringBuilder($"""
You are {TOKEN_AIDENTITY_NAME}.
{BACKGROUND_GUARDRAIL}
{PERSONALITY_GUARDRAIL}
{IMPERSONATE_GUARDRAIL}
""");

         ReplaceTokens(systemPrompt);
      }

      var instruction = new DefaultConversationalMessage(DefaultConversationalRole.System, systemPrompt.ToString(), null);

      return instruction;
   }

   /// <summary>
   /// Generates the guardrail message.
   /// </summary>
   /// <returns>The guardrail message.</returns>
   /// <exception cref="ArgumentNullException">Thrown when <see cref="_currentAIdentity"/> is null.</exception>
   private IConversationalMessage? GenerateGuardrailMessage()
   {
      if (_currentAIdentity == null) { throw new ArgumentNullException(nameof(_currentAIdentity)); }
      var guardrail = new StringBuilder(ADDITIONAL_GUARDRAIL);

      ReplaceTokens(guardrail);

      return new DefaultConversationalMessage(DefaultConversationalRole.System, guardrail.ToString(), null);
   }


   private void ReplaceTokens(StringBuilder systemPrompt)
   {
      systemPrompt.Replace(TOKEN_AIDENTITY_NAME, _currentAIdentity?.Name ?? "");
      systemPrompt.Replace(TOKEN_AIDENTITY_PERSONALITY, _currentAIdentity?.Personality ?? "");
      systemPrompt.Replace(TOKEN_AIDENTITY_BACKGROUND, _chatFeature?.Background ?? "");
   }

   public void AppendMessage(ChatMessage message)
   {
      if (_conversationMetadata == null) { throw new ArgumentNullException(nameof(_conversationMetadata)); }

      var role = message.AIDentityId == null ? DefaultConversationalRole.User : DefaultConversationalRole.Assistant;
      var chatMessage = new DefaultConversationalMessage(role, message.Message!, null);

      _history.Add(chatMessage);

      //TODO: check token, create summary, etc...
   }

   public Task<IConversationalRequest> GenerateApiRequest()
   {
      IConversationalRequest request = new DefaultConversationalRequest()
      {
         Messages = BuildRequestMessages().ToList(),
         MaxGeneratedTokens = 500
      };

      return Task.FromResult(request);
   }

   private IEnumerable<IConversationalMessage> BuildRequestMessages()
   {
      if (_coreInstructions != null)
      {
         yield return _coreInstructions;
      }

      if (_history is { Count: > 0 })
      {
         foreach (var item in _history.SkipLast(1))
         {
            yield return item;
         }

         if (_guardrailMessage != null)
         {
            yield return _guardrailMessage;
         }

         yield return _history.Last();
      }
   }
}
