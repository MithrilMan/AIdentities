using System.Text;
using AIdentities.Shared.Utils;
using Microsoft.JSInterop;

namespace AIdentities.Chat.Services;

public class ConversationExporter : IConversationExporter
{
   readonly ILogger<ConversationExporter> _logger;
   private readonly IJSRuntime _jsRuntime;
   readonly IChatStorage _chatStorage;
   readonly IAIdentityProvider _aIdentityProvider;
   readonly IDownloadService _downloadService;
   readonly INotificationService _notificationService;

   public ConversationExporter(ILogger<ConversationExporter> logger,
                               IJSRuntime jsRuntime,
                               IChatStorage chatStorage,
                               IAIdentityProvider aIdentityProvider,
                               IDownloadService downloadService,
                               INotificationService notificationService
                               )
   {
      _logger = logger;
      _jsRuntime = jsRuntime;
      _chatStorage = chatStorage;
      _aIdentityProvider = aIdentityProvider;
      _downloadService = downloadService;
      _notificationService = notificationService;
   }

   public async Task ExportConversationAsync(Guid conversationId, ConversationExportFormat format)
   {
      var conversation = await _chatStorage.LoadConversationAsync(conversationId).ConfigureAwait(false);
      if (conversation is null)
      {
         _notificationService.ShowError("Conversation not found");
         return;
      }

      var medaData = conversation.Metadata;
      var messages = conversation.Messages;

      if (messages is not { Count: > 0 })
      {
         _notificationService.ShowError("Conversation has no messages");
         return;
      }

      Dictionary<Guid, AIdentity?> involvedAIdentities = messages
      .Select(m => m.AIDentityId)
         .Where(m => m != null)
         .DistinctBy(m => m)
         .ToDictionary(id => id!.Value, id => _aIdentityProvider.Get(id!.Value));

      var exportedConversation = new
      {
         Title = medaData.Title,
         CreatedAt = medaData.CreatedAt,
         Messages = messages.Select(m =>
         {
            string from;
            if (m.IsGenerated)
            {
               from = involvedAIdentities.TryGetValue(m.AIDentityId ?? Guid.Empty, out var aidentity)
                  ? aidentity?.Name ?? "Unknown"
                  : "Unknown";
            }
            else
            {
               from = m.User ?? "User";
            }

            return new
            {
               m.CreationDate,
               m.Message,
               From = from
            };
         })
      };

      var fileName = PathUtils.SanitizeFileName($"{medaData.CreatedAt:yyyy-MM-dd HH-mm-ss}-{medaData.Title}.html");
      try
      {
         await _downloadService.DownloadFileFromStreamAsync(fileName, ExportConversation(exportedConversation, ConversationExportFormat.Html)).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         _notificationService.ShowError($"Export failed: {ex.Message}");
         return;
      }
   }

   public string ExportConversation(object exportedConversation, ConversationExportFormat format)
   {
      // Create a StringBuilder to store the formatted chat

      // Check the export format
      if (format == ConversationExportFormat.Html)
      {
         return ExportHtml(exportedConversation);
      }
      else if (format == ConversationExportFormat.Pdf)
      {
         // Create a PDF document using a PDF library (e.g. iTextSharp, PDFSharp)
         // Add the conversation title and creation date
         // Add the messages
         // Save the PDF document

         // Note: this code snippet only shows an example of how you can create a PDF document.
         // The actual implementation may vary depending on the PDF library you use.
         throw new NotImplementedException("PDF export is not implemented yet.");
      }
      else
      {
         throw new ArgumentException("Invalid export format. Only HTML and PDF are supported.");
      }

   }

   private static string ExportHtml(object exportedConversation)
   {
      var sb = new StringBuilder();
      // Add HTML tags to the beginning and end of the chat
      sb.AppendLine("<html>");
      sb.AppendLine("<head><title>Conversation Export</title></head>");
      sb.AppendLine("<body>");

      // Add the conversation title and creation date
      dynamic conversation = exportedConversation;
      sb.AppendLine($"<h2>{conversation.Title}</h2>");
      sb.AppendLine($"<p>Created at: {conversation.CreatedAt}</p>");

      // Add the messages
      foreach (var message in conversation.Messages)
      {
         sb.AppendLine($"<p><strong>{message.From}:</strong> {message.Message}</p>");
      }

      // Close the HTML tags
      sb.AppendLine("</body>");
      sb.AppendLine("</html>");

      // Return the formatted chat as an HTML string
      return sb.ToString();
   }
}
