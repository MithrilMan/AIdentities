namespace AIdentities.Connector.OpenAI.Services;

public class OpenAISettings : IConversationalConnectorSettings<OpenAIConnector>
{
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
   public Uri EndPoint { get; set; } = default!;


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
