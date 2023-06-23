namespace AIdentities.Connector.OpenAI.Models;

public class OpenAISettings : IPluginSettings
{
   public const bool DEFAULT_ENABLED = true;
   public const string DEFAULT_CHAT_MODEL = "gpt-3.5-turbo";
   public const string DEFAULT_CHAT_ENDPOINT = "https://api.openai.com/v1/chat/completions";
   public const string DEFAULT_COMPLETION_MODEL = "text-davinci-003";
   public const string DEFAULT_COMPLETION_ENDPOINT = "https://api.openai.com/v1/completions";
   public const int DEFAULT_TIMEOUT = 2 * 60 * 1000;

   /// <summary>
   /// Enable or disable the OpenAI API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// OpenAI API Chat Endpoint.
   /// </summary>
   public Uri ChatEndPoint { get; set; } = new Uri(DEFAULT_CHAT_ENDPOINT);

   /// <summary>
   /// The default model to use if no model has been specified in the request.
   /// </summary>
   public string DefaultChatModel { get; set; } = DEFAULT_CHAT_MODEL;

   /// <summary>
   /// OpenAI API Completion Endpoint.
   /// </summary>
   public Uri CompletionEndPoint { get; set; } = new Uri(DEFAULT_COMPLETION_ENDPOINT);

   /// <summary>
   /// The default model to use if no model has been specified in the request.
   /// </summary>
   public string DefaultCompletionModel { get; set; } = DEFAULT_COMPLETION_MODEL;

   /// <summary>
   /// OpenAI API Key.
   /// </summary>
   public string? ApiKey { get; set; } = default!;

   /// <summary>
   /// The default timeout for the API.
   /// </summary>
   public int Timeout { get; set; } = DEFAULT_TIMEOUT;
}
