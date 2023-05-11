using AIdentities.UI.Features.Core.Components;

namespace AIdentities.UI.Features.Core.Extensions;

public static class MudBlazorExtensions
{
   public static async Task<bool> OkCancelDialog(this IDialogService dialogService,
                                                       string title,
                                                       string contentText,
                                                       string okText = "Ok",
                                                       Color okColor = Color.Success,
                                                       string cancelText = "Cancel",
                                                       Color cancelColor = Color.Default)
   {
      var parameters = new DialogParameters
      {
         { "ContentText", contentText },
         { "OkText", okText },
         { "OkColor", okColor },
         { "CancelText", cancelText },
         { "CancelColor", cancelColor }
      };

      var options = new DialogOptions() { CloseButton = true, MaxWidth = MaxWidth.ExtraSmall };

      var reference = await dialogService.ShowAsync<OkCancelDialog>(title, parameters, options).ConfigureAwait(false);
      var result = await reference.Result.ConfigureAwait(false);

      return result.Data is true;
   }
}
