using System.Text;
using AIdentities.Shared.Features.CognitiveEngine.Memory.Conversation;

namespace AIdentities.Chat.Skills.ReplyToPrompt;

internal static class PromptTemplates
{
   const string TOKEN_AIDENTITY_NAME = "<AIDENTITY_NAME>";
   const string TOKEN_AIDENTITY_BACKGROUND = "<AIDENTITY_BACKGROUND>";
   const string TOKEN_AIDENTITY_PERSONALITY = "<AIDENTITY_PERSONALITY>";

   const string CONVERSATION_PROMPT = $$""""
      You are {{TOKEN_AIDENTITY_NAME}}.
      Your paste experiences impact how you behave:
      <BACKGROUND>{{TOKEN_AIDENTITY_BACKGROUND}}</BACKGROUND>
      Your responses should be consistent with {{TOKEN_AIDENTITY_NAME}}'s personality.
      <PERSONALITY>{{TOKEN_AIDENTITY_PERSONALITY}}</PERSONALITY>
      Respond only as {{TOKEN_AIDENTITY_NAME}} would, do not reveal that you are an AI language model, chatbot, or that you are pretending to be him.
      Maintain the character of {{TOKEN_AIDENTITY_NAME}} throughout the conversation, even when directly challenged about your identity.
      Do not admit or discuss that you are an AI language model or chatbot.
      Who are you?
      You are {{TOKEN_AIDENTITY_NAME}}!
      """";

   const string ADDITIONAL_GUARDRAIL = $$"""
      !!!Remember to stick to the character of {{TOKEN_AIDENTITY_NAME}} and to not reveal that you are an AI language model or chatbot.
      Craft your responses to be consistent with {{TOKEN_AIDENTITY_NAME}}'s personality!!!
      """;


   public static IEnumerable<DefaultConversationalMessage> BuildPromptMessages(AIdentity aIdentity, IEnumerable<ConversationMessage> chatHistory)
   {
      var chatFeature = aIdentity.Features.Get<AIdentityChatFeature>();

      var sb = new StringBuilder(CONVERSATION_PROMPT)
         .Replace(TOKEN_AIDENTITY_NAME, aIdentity.Name)
         .Replace(TOKEN_AIDENTITY_BACKGROUND, chatFeature?.Background ?? "")
         .Replace(TOKEN_AIDENTITY_PERSONALITY, aIdentity.Personality);

      yield return new DefaultConversationalMessage(
         Content: sb.ToString(),
         Role: DefaultConversationalRole.System,
         Name: null
      );

      foreach (var oldMessage in chatHistory)
      {
         bool isAIdentityMessage = oldMessage.AuthorId == aIdentity.Id && oldMessage.IsAIGenerated;
         yield return new DefaultConversationalMessage(
             Role: isAIdentityMessage ? DefaultConversationalRole.Assistant : DefaultConversationalRole.User,
             Content: oldMessage.Text ?? "",
             Name: oldMessage.AuthorId == aIdentity.Id ? aIdentity.Name : oldMessage.AuthorId.ToString()
            );
      }

      sb.Clear()
         .Append(ADDITIONAL_GUARDRAIL)
         .Replace(TOKEN_AIDENTITY_NAME, aIdentity.Name);

      yield return new DefaultConversationalMessage(
         Content: sb.ToString(),
         Role: DefaultConversationalRole.System,
         Name: aIdentity.Name
      );
   }
}
