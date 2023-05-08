using AIdentities.Shared;

namespace AIdentities.Chat.Services.Connectors.OpenAI;

public class OpenAIOptions
{
   public const string SECTION_NAME = $"{AppOptions.SECTION_NAME}:Chat:OpenAI";

   const bool DEFAULT_ENABLED = false;
   const string DEFAULT_MODEL = "gpt-3.5-turbo";
   const int DEFAULT_TIMEOUT = 15000;

   /// <summary>
   /// Enable or disable the OpenAI API.
   /// </summary>
   public bool Enabled { get; set; } = DEFAULT_ENABLED;

   /// <summary>
   /// OpenAI API Endpoint.
   /// </summary>
   public string? EndPoint { get; set; } = default!;


   /// <summary>
   /// OpenAI API Key.
   /// </summary>
   public string ApiKey { get; set; } = default!;

   /// <summary>
   /// The default model to use if no model has been specified in the request.
   /// </summary>
   public string DefaultModel { get; set; } = DEFAULT_MODEL;

   /// <summary>
   /// The default timeout for the API.
   /// </summary>
   public int Timeout { get; set; } = DEFAULT_TIMEOUT;
}
