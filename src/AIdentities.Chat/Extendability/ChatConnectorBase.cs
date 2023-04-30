using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIdentities.Chat.Extendability;
public class ChatConnectorBase : IChatConnector
{
   public IChatSettings Settings => throw new NotImplementedException();

   public Uri Endpoint => throw new NotImplementedException();

   public string Name => "";// throw new NotImplementedException();

   public string Description => throw new NotImplementedException();

   public TFeatureType? GetFeature<TFeatureType>() => throw new NotImplementedException();
   public void SetFeature<TFeatureType>(TFeatureType? feature) => throw new NotImplementedException();
}
