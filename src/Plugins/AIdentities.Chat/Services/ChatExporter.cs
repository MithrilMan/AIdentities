using System.Drawing;
using System.Text;
using AIdentities.Shared.Utils;
using Microsoft.JSInterop;

namespace AIdentities.Chat.Services;

public class ChatExporter : IConversationExporter
{
   readonly ILogger<ChatExporter> _logger;
   private readonly IJSRuntime _jsRuntime;
   readonly ICognitiveChatStorage _cognitiveChatStorage;
   readonly IAIdentityProvider _aIdentityProvider;
   readonly IDownloadService _downloadService;
   readonly INotificationService _notificationService;

   public ChatExporter(ILogger<ChatExporter> logger,
                               IJSRuntime jsRuntime,
                               ICognitiveChatStorage cognitiveChatStorage,
                               IAIdentityProvider aIdentityProvider,
                               IDownloadService downloadService,
                               INotificationService notificationService
                               )
   {
      _logger = logger;
      _jsRuntime = jsRuntime;
      _cognitiveChatStorage = cognitiveChatStorage;
      _aIdentityProvider = aIdentityProvider;
      _downloadService = downloadService;
      _notificationService = notificationService;
   }

   public async Task ExportConversationAsync(Guid conversationId, ConversationExportFormat format)
   {
      var conversation = await _cognitiveChatStorage.LoadConversationAsync(conversationId).ConfigureAwait(false);
      if (conversation is null)
      {
         _notificationService.ShowError("Conversation not found");
         return;
      }

      var messages = conversation.Messages;

      if (messages is not { Count: > 0 })
      {
         _notificationService.ShowError("Conversation has no messages");
         return;
      }


      var fileName = PathUtils.SanitizeFileName($"{conversation.CreatedAt:yyyy-MM-dd HH-mm-ss}-{conversation.Title}.html");
      try
      {
         await _downloadService.DownloadFileFromStreamAsync(fileName, ExportConversation(conversation, ConversationExportFormat.Html)).ConfigureAwait(false);
      }
      catch (Exception ex)
      {
         _notificationService.ShowError($"Export failed: {ex.Message}");
         return;
      }
   }

   public string ExportConversation(Conversation exportedConversation, ConversationExportFormat format)
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

   private string ExportHtml(Conversation conversation)
   {
      const string TOKEN_BODY = "<<MESSAGES>>";
      const string TOKEN_PARTICIPANTS = "<<PARTICIPANTS>>";

      string template = $$"""
      <html>
         <head>
            <title>Conversation Export: {{conversation.Title}}</title>
            <style>
               body {
                  font-family: Arial, Helvetica, sans-serif;
               }
               h2 {
                  font-size: 1.5em;
               }
               .message p {
                  font-size: 0.8em;
                  white-space: pre-wrap;
               }
               .participants{
                 display: inline-flex;
                  flex-direction: row;
                  flex-wrap: wrap;
                  gap: 2em;
                  padding: 10px;
                  border: solid 1px black;
                  border-radius: 10px;
                  margin-bottom: 25px;
               }
            </style>
         </head>
         <body>
            <h1>{{conversation.Title}}</h1>
            <p>Created at: {{conversation.CreatedAt}}</p>
            <p>Last update at: {{conversation.UpdatedAt}}</p>
            <hr/>
            <h2>Participants</h2>
            <div class="participants">
            {{TOKEN_PARTICIPANTS}}
            </div>
      {{TOKEN_BODY}}
         </body>
      </html>
      """;


      //create 15 different dark colors for the participants
      //colors have to be very different to ensure that the text is readable but visible on a white background
      string[] palette = new string[] { "#ff0000", "#0000ff", "#ff00ff", "#ff8000", "#ff0080", "#80ff00", "#8000ff", "#00ff80", "#0080ff", "#ff80ff", "#80ffff", "#ffff80" };
      var colors = new Dictionary<Guid, string>();
      for (int i = 0; i < conversation.AIdentityIds.Count; i++)
      {
         colors.Add(conversation.AIdentityIds.ElementAt(i), palette[i % palette.Length]);
      }


      ////assign a color to each participant
      //var colors = new Dictionary<Guid, string>();
      //var random = new Random();
      //foreach (var id in conversation.AIdentityIds)
      //{
      //   if (!colors.ContainsKey(id))
      //   {
      //      var color = $"#{random.Next(0x1000000):X6}";
      //      // ensure that the color is not too bright
      //      var rgb = new ColorConverter().ConvertFromString(color) as System.Drawing.Color?;
      //      while (rgb is not null && rgb.Value.GetBrightness() > 0.9)
      //      {
      //         color = $"#{random.Next(0x1000000):X6}";
      //         rgb = new ColorConverter().ConvertFromString(color) as System.Drawing.Color?;
      //      }

      //      colors.Add(id, color);
      //   }
      //}

      var sbParticipants = new StringBuilder();
      foreach (var id in conversation.AIdentityIds)
      {
         var aIdentity = _aIdentityProvider.Get(id);
         if (aIdentity is not null)
         {
            sbParticipants.AppendLine($"<div><strong style=\"color:{colors[id]}\">{aIdentity.Name}</strong></div>");
         }
      }

      var sb = new StringBuilder();
      // Add the messages
      foreach (var message in conversation.Messages)
      {
         string color = colors.TryGetValue(message.AuthorId, out var c) ? c : "#000000";
         sb.AppendLine($"""
            <div class="message">
               <strong style="color:{color}">{message.AuthorName}:</strong> <span>{message.CreationDate.ToLocalTime()}</span>
               <p>{message.Text}</p>
            </div>
            """);
      }

      // Return the formatted chat as an HTML string
      return template
         .Replace(TOKEN_BODY, sb.ToString())
         .Replace(TOKEN_PARTICIPANTS, sbParticipants.ToString())
         ;
   }
}
