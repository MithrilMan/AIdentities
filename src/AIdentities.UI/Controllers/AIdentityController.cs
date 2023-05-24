using Microsoft.AspNetCore.Mvc;

namespace AIdentities.UI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AIdentityController : ControllerBase
{
   readonly ILogger<AIdentityController> _logger;
   readonly IAIdentityProvider _aIdentityProvider;

   public AIdentityController(ILogger<AIdentityController> logger, IAIdentityProvider aIdentityProvider)
   {
      _logger = logger;
      _aIdentityProvider = aIdentityProvider;
   }

   [HttpGet("Image/{aidentityId}")]
   [ResponseCache(Duration = 120, Location = ResponseCacheLocation.Client)]
   public IActionResult GetImage(Guid aidentityId, CancellationToken cancellationToken)
   {
      var aidentity = _aIdentityProvider.Get(aidentityId);
      if (aidentity == null) return NotFound();
      if (string.IsNullOrEmpty(aidentity.Image)) return NotFound(); //maybe return emtpy image

      (string contentType, int dataIndex) = GetContentTypeAndDataIndexFromDataUrl(aidentity.Image);
      byte[] bytes = Convert.FromBase64String(aidentity.Image[dataIndex..]);

      return File(bytes, contentType);
   }

   public static (string contentType, int dataIndex) GetContentTypeAndDataIndexFromDataUrl(string dataUrl)
   {
      var contentTypeStartIndex = dataUrl.IndexOf("data:") + "data:".Length;
      var contentTypeEndIndex = dataUrl.IndexOf(";base64,");
      if (contentTypeEndIndex < 0)
      {
         contentTypeEndIndex = dataUrl.IndexOf(",");
      }
      if (contentTypeEndIndex < 0)
      {
         throw new ArgumentException("Invalid data URL: " + dataUrl);
      }
      var contentType = dataUrl.Substring(contentTypeStartIndex, contentTypeEndIndex - contentTypeStartIndex);
      var dataIndex = contentTypeEndIndex + ";base64,".Length;
      return (contentType, dataIndex);
   }
}
