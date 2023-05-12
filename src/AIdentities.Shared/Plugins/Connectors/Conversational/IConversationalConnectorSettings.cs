namespace AIdentities.Shared.Plugins.Connectors.Conversational;

/// <summary>
/// This interface is used to define the settings for a conversational connector.
/// To implement plugins, do not use this interface directly but use the generic version instead.
/// </summary>
public interface IConversationalConnectorSettings
{
   /// <summary>
   /// Each conversational connector is expected to have an endpoint.
   /// </summary>
   Uri EndPoint { get; }

   /// <summary>
   /// An optional API key to use for the endpoint.
   /// Not every connector will require an API key.
   /// </summary>
   string? ApiKey { get; }
}

/// <summary>
/// This interface is used to define the settings for a conversational connector.
/// </summary>
/// <typeparam name="TConversationalConnector"></typeparam>
public interface IConversationalConnectorSettings<TConversationalConnector> : IConversationalConnectorSettings
   where TConversationalConnector : IConversationalConnector
{ }
