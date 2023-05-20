namespace AIdentities.Chat.Pages;

public partial class Chat
{
   class State
   {
      public string? MessageSearchText { get; set; }

      public ConversationMetadata? SelectedConversation { get; set; }
      /// <summary>
      /// True if no conversation is selected.
      /// </summary>
      public bool NoConversation => SelectedConversation is null;

      public string? Message { get; set; }
      public ChatMessage? SelectedMessage { get; set; } = default!;
      public FilteredObservableCollection<ChatMessage> Messages { get; private set; } = default!;

      /// <summary>
      /// This is used to prevent the user from sending multiple messages before the first one has been replied to.
      /// When the user send a message, this is set to true, and when the message is replied to, it is set to false.
      /// </summary>
      public bool IsWaitingReply { get; set; }

      public bool CanSendMessage => Connector != null && !IsWaitingReply && SelectedConversation != null;

      public bool HasMessageGenerationFailed { get; internal set; }

      /// <summary>
      /// The response from the chat API, streamed as it comes in.
      /// During the streaming, this is used to store the response, and when the streaming is done, it is 
      /// cleared and the response is added to the <see cref="Messages"/> collection.
      /// </summary>
      public ChatMessage? StreamedResponse { get; set; }

      /// <summary>
      /// Holds the reference to the current Conversational Connector.
      /// If null, no chat message can be sent.
      /// </summary>
      public IConversationalConnector? Connector { get; set; }

      /// <summary>
      /// A collection of all the AIdentities that are partecipating in the current conversation.
      /// </summary>
      public HashSet<AIdentity> PartecipatingAIdentities { get; set; } = new HashSet<AIdentity>();

      public string PartecipatingAIdentitiesTooltip => string.Join(", ", PartecipatingAIdentities.Select(aidentity => aidentity.Name));

      /// <summary>
      /// The chat prompt generator used to generate the chat prompts.
      /// </summary>
      public IChatPromptGenerator ChatPromptGenerator { get; private set; } = default!;
      public IAIdentityProvider AIdentityProvider { get; private set; } = default!;

      /// <summary>
      /// The cancellation token source used to cancel the message generation.
      /// </summary>
      public CancellationTokenSource MessageGenerationCancellationTokenSource { get; set; } = new CancellationTokenSource();

      public void Initialize(Func<IEnumerable<ChatMessage>, ValueTask<IEnumerable<ChatMessage>>> messageFilter, IChatPromptGenerator chatPromptGenerator, IAIdentityProvider aidentityProvider)
      {
         Messages = new(messageFilter);
         ChatPromptGenerator = chatPromptGenerator;
         AIdentityProvider = aidentityProvider;
         MessageSearchText = null;
      }

      public async Task InitializeConversation(Conversation conversation, bool loadMessages = true)
      {
         ChatPromptGenerator.InitializeConversation(conversation);
         // if the last message is not generated, we need to generate a reply so we enable the "resend" button
         HasMessageGenerationFailed = conversation.Messages?.LastOrDefault()?.IsGenerated == false;
         if (loadMessages)
         {
            await Messages.LoadItemsAsync(conversation.Messages).ConfigureAwait(false);
         }

         PartecipatingAIdentities = conversation.Metadata.AIdentityIds
            .Select(AIdentityProvider.Get)
            .Where(aidentity => aidentity is not null)
            .Select(aidentity => aidentity!)
            .ToHashSet();
      }

      public async Task CloseConversation()
      {
         SelectedConversation = null;
         HasMessageGenerationFailed = false;
         await Messages.LoadItemsAsync(null).ConfigureAwait(false);

         ChatPromptGenerator.InitializeConversation(null);
         PartecipatingAIdentities.Clear();
      }
   }

   private readonly State _state = new State();
}
