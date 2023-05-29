namespace AIdentities.Shared.Plugins.Connectors;

/// <summary>
/// Allows to get the completion connectors.
/// </summary>
public interface IConnectorsManager<TConnectorType> where TConnectorType : IConnector
{
   /// <summary>
   /// Returns the enabled connectors of <see cref="TConnectorType"/> Type.
   /// </summary>
   /// <returns></returns>
   IEnumerable<TConnectorType> GetEnabled();

   /// <summary>
   /// Returns all the connectors of <see cref="TConnectorType"/> Type.
   /// </summary>
   /// <returns></returns>
   IEnumerable<TConnectorType> GetAll();

   /// <summary>
   /// Returns the first enabled connector of <see cref="TConnectorType"/> Type.
   /// If no connector is enabled, returns null.
   /// </summary>
   /// <returns>The first enabled connector of <see cref="TConnectorType"/> Type or null if no connector is enabled.</returns>
   TConnectorType? GetFirstEnabled();
}
