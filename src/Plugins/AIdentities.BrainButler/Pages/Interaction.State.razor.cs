namespace AIdentities.BrainButler.Pages;

public partial class Interaction
{
   class State
   {
      public List<ConversationPiece> ConversationPieces { get; set; } = new();

      public ConversationPiece? SelectedMessage { get; set; }

      public string? UserRequest { get; set; }

      /// <summary>
      /// This is used to prevent the user from sending multiple messages before the first one has been replied to.
      /// When the user send a message, this is set to true, and when the message is replied to, it is set to false.
      /// </summary>
      public bool IsWaitingReply { get; set; }

      public bool CanSendMessage => CompletionConnector != null && !IsWaitingReply;

      public bool HasMessageGenerationFailed { get; internal set; }

      /// <summary>
      /// The response from the chat API, streamed as it comes in.
      /// During the streaming, this is used to store the response, and when the streaming is done, it is 
      /// cleared and the response is added to the <see cref="Messages"/> collection.
      /// </summary>
      public ConversationPiece? StreamedResponse { get; set; }

      /// <summary>
      /// Holds the reference to the current Conversational Connector.
      /// This connector is used to try to understand user commands and to execute them.
      /// </summary>
      public ICompletionConnector? CompletionConnector { get; set; }

      /// <summary>
      /// Holds the reference to the current Conversational Connector.
      /// This connector is used to communicate with the user whenever the user input doesn't refers to a command.
      /// </summary>
      public IConversationalConnector? ConversationalConnector { get; set; }

      /// <summary>
      /// The cancellation token source used to cancel the message generation.
      /// </summary>
      public CancellationTokenSource MessageGenerationCancellationTokenSource { get; set; } = new CancellationTokenSource();
   }

   private readonly State _state = new State();
}
