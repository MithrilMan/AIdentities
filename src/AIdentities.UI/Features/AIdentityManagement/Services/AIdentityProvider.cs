using System.Text.Json;
using Microsoft.Extensions.Options;

namespace AIdentities.UI.Features.AIdentityManagement.Services;

public class AIdentityProvider : IAIdentityProvider
{
   readonly ILogger<AIdentityProvider> _logger;
   readonly IOptions<AppOptions> _options;
   readonly AIdentityProviderSerializationSettings _serializationSettings;
   readonly Dictionary<Guid, AIdentity> _aidentities = new();

   public AIdentityProvider(ILogger<AIdentityProvider> logger, IOptions<AppOptions> options, AIdentityProviderSerializationSettings serializationSettings)
   {
      _logger = logger;
      _options = options;
      _serializationSettings = serializationSettings;

      CheckPath();
      FetchAIdentities();
   }

   private string GetAIdentitiesPath() => Path.Combine(_options.Value.PackageFolder, AppConstants.SpecialFolders.STORAGE, AppConstants.SpecialFolders.AIDENTITIES);
   private string GetAIdentityFileName(Guid aidentityId) => Path.Combine(GetAIdentitiesPath(), $"{aidentityId}.json");


   private void FetchAIdentities()
   {
      var files = Directory.GetFiles(GetAIdentitiesPath(), "*.json");
      foreach (var file in files)
      {
         var filename = Path.GetFileNameWithoutExtension(file);
         if (!Guid.TryParse(filename, out var id))
         {
            _logger.LogWarning("Invalid AIdentity file name {AIdentityFile}. AIdentity skipped", file);
            continue;
         }

         try
         {
            var aidentity = ReadAIdentity(id);
            if (aidentity != null)
            {
               _aidentities[aidentity.Id] = aidentity;
            }
         }
         catch (Exception ex)
         {
            _logger.LogError("Error reading AIdentity {AIdentityId} from file {AIdentityFile}: {ErrorMessage}. AIdentity skipped", id, file, ex.Message);
            continue;
         }
      }
   }

   private void CheckPath()
   {
      var path = GetAIdentitiesPath();
      if (!Directory.Exists(path))
      {
         Directory.CreateDirectory(path);
      }
   }

   public IEnumerable<AIdentity> All()
   {
      return _aidentities.Values;
   }

   public bool Create(AIdentity newAIdentity)
   {
      WriteAIdentity(newAIdentity);
      _aidentities[newAIdentity.Id] = newAIdentity;
      return true;
   }

   public bool Delete(AIdentity deletedAIdentity)
   {
      File.Delete(GetAIdentityFileName(deletedAIdentity.Id));
      return _aidentities.Remove(deletedAIdentity.Id);
   }

   public AIdentity? Get(Guid id)
   {
      if (id == Guid.Empty) return null;

      if (!_aidentities.TryGetValue(id, out var aidentity))
      {
         //try to read again from disk
         aidentity = ReadAIdentity(id);
         if (aidentity != null)
         {
            _aidentities[aidentity.Id] = aidentity;
         }
      }

      return aidentity;
   }

   public bool Update(AIdentity updatedAIdentity)
   {
      WriteAIdentity(updatedAIdentity);
      _aidentities[updatedAIdentity.Id] = updatedAIdentity;
      return true;
   }

   private AIdentity? ReadAIdentity(Guid id)
   {
      if (!File.Exists(GetAIdentityFileName(id))) return null;

      var json = File.ReadAllText(GetAIdentityFileName(id));
      var aidentity = JsonSerializer.Deserialize<AIdentity>(json, _serializationSettings.SerializerOptions);
      return aidentity;
   }

   /// <summary>
   /// Writes the aidentity to disk.
   /// </summary>
   /// <param name="aidentity"></param>
   private void WriteAIdentity(AIdentity aidentity)
   {
      var json = JsonSerializer.Serialize(aidentity, _serializationSettings.SerializerOptions);
      File.WriteAllText(GetAIdentityFileName(aidentity.Id), json);
   }
}
