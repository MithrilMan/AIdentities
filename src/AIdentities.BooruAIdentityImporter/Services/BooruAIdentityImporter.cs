using System.Buffers;
using System.Text.Json;
using AIdentities.BooruAIdentityImporter.Models;

namespace AIdentities.BooruAIdentityImporter.Services;

public class BooruAIdentityImporter : IAIdentityImporter
{
   const int MAX_FILE_SIZE = 5 * 1024 * 1024; // 5MB
   private static readonly string[] _allowedFileExtensions = new[] { ".png", ".webp" };
   readonly ILogger<BooruAIdentityImporter> _logger;

   public BooruAIdentityImporter(ILogger<BooruAIdentityImporter> logger)
   {
      _logger = logger;
   }

   public string Name => "Booru Importer";

   public string Description => "Imports AIdentities from booru extrapolating data from the image metadata";

   public IEnumerable<string> AllowedFileExtensions => _allowedFileExtensions;

   public async Task<AIdentity?> ImportAIdentity(IBrowserFile aIdentityFile)
   {
      if (aIdentityFile == null)
         throw new FileLoadException("The file is null");

      if (aIdentityFile.Size > MAX_FILE_SIZE)
         throw new FileLoadException($"The file is too big, max size is {Shared.Utils.Formatter.FormatFileSize(MAX_FILE_SIZE)}");

      byte[] buffer = ArrayPool<byte>.Shared.Rent((int)aIdentityFile.Size);
      try
      {
         using var openedStream = aIdentityFile.OpenReadStream(MAX_FILE_SIZE); // 5MB max
         using var memoryStream = new MemoryStream(buffer);
         await openedStream.CopyToAsync(memoryStream).ConfigureAwait(false);
         int dataLength = (int)memoryStream.Length;
         if (dataLength > 0)
         {
            string base64Data = Convert.ToBase64String(buffer);
            var newAIdentity = new AIdentity
            {
               Image = $"data:{aIdentityFile.ContentType};base64,{base64Data}"
            };

            memoryStream.Position = 0;
            IReadOnlyList<MetadataExtractor.Directory> metaData = MetadataExtractor.ImageMetadataReader.ReadMetadata(memoryStream);

            var tEXt = metaData.FirstOrDefault(d => d.Name == "PNG-tEXt");
            if (tEXt == null)
            {
               _logger.LogError("The file {FileName} doesn't have any booru metadata", aIdentityFile.Name);
               return null;
            }

            var description = tEXt.Tags.First().Description;
            const string charaPrefix = "chara:";
            if (description?.StartsWith(charaPrefix) ?? false)
            {
               var characterData = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(description[charaPrefix.Length..]));

               var decodedJson = JsonSerializer.Deserialize<BooruMetadata>(characterData);
            }
            return newAIdentity;
         }
         else
         {
            _logger.LogError("The file {FileName} is empty", aIdentityFile.Name);
            return null;
         }
      }
      finally
      {
         ArrayPool<byte>.Shared.Return(buffer);
      }

   }
}
