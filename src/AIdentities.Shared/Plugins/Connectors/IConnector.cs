using Microsoft.AspNetCore.Http.Features;

namespace AIdentities.Shared.Plugins.Connectors;

/// <summary>
/// A generic interface for connectors.
/// Connectors are used to connect to external services like LLM apis or even services like Discord, Telegram, etc.
/// Features can be custom types that consumers of the connector can use to interact with the connector in a type-safe way.
/// </summary>
public interface IConnector
{
   /// <summary>
   /// The name of the connector.
   /// </summary>
   string Name { get; }

   /// <summary>
   /// The description of the connector.
   /// </summary>
   string Description { get; }

   /// <summary>
   /// Additional features the connector can have.
   /// Plugin developers can use this to add and manage custom features to their connectors or access other plugins' features.
   /// </summary>
   IFeatureCollection Features { get; }
}
