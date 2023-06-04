namespace AIdentities.Shared.Plugins.Connectors;

public class ConnectorsManager<TConnectorType> : IConnectorsManager<TConnectorType>
where TConnectorType : IConnector
{
   readonly IEnumerable<TConnectorType> _connectors;

   public ConnectorsManager(IEnumerable<TConnectorType> connectors)
   {
      _connectors = connectors;
   }

   public IEnumerable<TConnectorType> GetEnabled() => _connectors.Where(c => c.Enabled);

   public IEnumerable<TConnectorType> GetAll() => _connectors;

   public TConnectorType? GetFirstEnabled() => GetEnabled().FirstOrDefault();
}
