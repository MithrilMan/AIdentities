using System.Text;
using AIdentities.Chat.Extendability;

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

   readonly ILogger<ChatPromptGenerator> _logger;
   readonly IAIdentityProvider _aIdentityProvider;

   /// <summary>
   /// Contains the history of messages that have been sent to the API.
   /// This is used to generate the prompt.
   /// </summary>
   private readonly List<ChatApiRequest.Message> _history = new();
   private ConversationMetadata? _conversationMetadata;
   private AIdentity? _currentAIdentity;
   private AIdentityChatFeature? _chatFeature;

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
         _history.Clear();
         return;
      }

      _conversationMetadata = conversation.Metadata with { };
      _currentAIdentity = _aIdentityProvider.Get(_conversationMetadata.AIdentityId ?? Guid.Empty);
      _chatFeature = _currentAIdentity?.Features.Get<AIdentityChatFeature>();

      _history.Clear();
      _history.Add(GenerateInstruction());

      if (conversation is not { Messages.Count: > 0 })
      {
         _logger.LogDebug("Conversation {ConversationId} has no messages.", conversation.Id);
         return;
      }

      _history.AddRange(conversation.Messages.Select(message =>
      {
         var role = message.AIDentityId == null ? ChatApiRequest.MessageRole.User : ChatApiRequest.MessageRole.Assistant;
         return new ChatApiRequest.Message(message.AIDentityId == null ? ChatApiRequest.MessageRole.User : ChatApiRequest.MessageRole.Assistant, message.Message!, null);
      }));
   }

   /// <summary>
   /// Generates the system instruction to start the conversation.
   /// It has to take into account the used AIdentity.
   /// </summary>
   /// <returns>The system instruction to start the conversation.</returns>
   private ChatApiRequest.Message GenerateInstruction()
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

         systemPrompt.Replace(TOKEN_AIDENTITY_NAME, _currentAIdentity.Name);
         systemPrompt.Replace(TOKEN_AIDENTITY_PERSONALITY, _currentAIdentity?.Personality ?? "");
         systemPrompt.Replace(TOKEN_AIDENTITY_BACKGROUND, _chatFeature?.Background ?? "");
      }

      var instruction = new ChatApiRequest.Message(ChatApiRequest.MessageRole.System, systemPrompt.ToString(), null);

      return instruction;
   }

   public void AppendMessage(ChatMessage message)
   {
      if (_conversationMetadata == null) { throw new ArgumentNullException(nameof(_conversationMetadata)); }

      var role = message.AIDentityId == null ? ChatApiRequest.MessageRole.User : ChatApiRequest.MessageRole.Assistant;
      var chatMessage = new ChatApiRequest.Message(role, message.Message!, null);

      _history.Add(chatMessage);

      //TODO: check token, create summary, etc...
   }

   public Task<ChatApiRequest> GenerateApiRequest()
   {
      var request = new ChatApiRequest()
      {
         Messages = _history,
         MaxGeneratedTokens = 500
      };

      return Task.FromResult(request);
   }
}
