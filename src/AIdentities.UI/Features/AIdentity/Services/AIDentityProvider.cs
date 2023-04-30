using System.Text.Json;
using Microsoft.Extensions.Options;
using Entities = AIdentities.Shared.Features.Core;

namespace AIdentities.UI.Features.AIdentity.Services;

public class AIdentityProvider : IAIdentityProvider
{
   readonly ILogger<AIdentityProvider> _logger;
   readonly IOptions<AppOptions> _options;

   readonly Dictionary<Guid, Entities.AIdentity> _aidentities = new();

   public AIdentityProvider(ILogger<AIdentityProvider> logger, IOptions<AppOptions> options)
   {
      _logger = logger;
      _options = options;

      CheckPath();
      FetchAIdentities();
   }

   private void FetchAIdentities()
   {
      var path = Path.Combine(_options.Value.PackageFolder, AppConstants.SpecialFolders.AIDENTITIES);
      var files = Directory.GetFiles(path, "*.json");
      foreach (var file in files)
      {
         var json = File.ReadAllText(file);
         var aidentity = JsonSerializer.Deserialize<Entities.AIdentity>(json)!;
         _aidentities[aidentity.Id] = aidentity;
      }
   }

   private string GetAIdentitiesPath() => Path.Combine(_options.Value.PackageFolder, AppConstants.SpecialFolders.AIDENTITIES);
   private string GetAIdentityFileName(Guid aidentityId) => Path.Combine(GetAIdentitiesPath(), $"{aidentityId}.json");

   private void CheckPath()
   {
      var path = GetAIdentitiesPath();
      if (!Directory.Exists(path))
         Directory.CreateDirectory(path);
   }

   public IEnumerable<Entities.AIdentity> All()
   {
      return _aidentities.Values;
   }

   public bool Create(Entities.AIdentity newAIdentity)
   {
      WriteAIdentity(newAIdentity);
      _aidentities[newAIdentity.Id] = newAIdentity;
      return true;
   }

   public bool Delete(Entities.AIdentity deletedAIdentity)
   {
      File.Delete(GetAIdentityFileName(deletedAIdentity.Id));
      return _aidentities.Remove(deletedAIdentity.Id);
   }

   public Entities.AIdentity? Get(Guid id)
   {
      if (!_aidentities.TryGetValue(id, out var aidentity))
      {
         //try to read again from disk
         aidentity = ReadAIdentity(id);
         if (aidentity != null)
            _aidentities[aidentity.Id] = aidentity;
      }

      return aidentity;
   }

   public bool Update(Entities.AIdentity updatedAIdentity)
   {
      WriteAIdentity(updatedAIdentity);
      _aidentities[updatedAIdentity.Id] = updatedAIdentity;
      return true;
   }

   private Entities.AIdentity? ReadAIdentity(Guid id)
   {
      if (!File.Exists(GetAIdentityFileName(id))) return null;

      var json = File.ReadAllText(GetAIdentityFileName(id));
      var aidentity = JsonSerializer.Deserialize<Entities.AIdentity>(json);
      return aidentity;
   }

   /// <summary>
   /// Writes the aidentity to disk.
   /// </summary>
   /// <param name="aidentity"></param>
   private void WriteAIdentity(Entities.AIdentity aidentity)
   {
      var json = JsonSerializer.Serialize(aidentity);
      File.WriteAllText(GetAIdentityFileName(aidentity.Id), json);
   }
}
