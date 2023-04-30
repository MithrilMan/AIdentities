namespace AIdentities.Shared.Plugins;

public record PluginDependency
{
   public PluginSignature Signature { get; set; }
   public bool IsMandatory { get; set; }

   public PluginDependency(PluginSignature signature, bool isMandatory)
   {
      Signature = signature;
      IsMandatory = isMandatory;
   }
}
