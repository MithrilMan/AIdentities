namespace AIdentities.Shared.Plugins.Connectors;

/// <summary>
/// A generic interface for connectors that have an endpoint.
/// </summary>
public interface IEndpointConnector : IConnector
{
   /// <summary>
   /// The endpoint of the connector.
   /// </summary>
   Uri EndPoint { get; }
}
