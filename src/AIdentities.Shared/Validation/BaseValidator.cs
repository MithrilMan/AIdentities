using FluentValidation;

namespace AIdentities.Shared.Validation;

/// <summary>
/// The base validator for all validators that are needed in the UI.
/// </summary>
/// <typeparam name="TModel"></typeparam>
public class BaseValidator<TModel> : AbstractValidator<TModel>
{

   public Func<object, string, Task<IEnumerable<string>>> ValidateValue => async (model, propertyName) =>
   {
      var result = await ValidateAsync(ValidationContext<TModel>.CreateWithOptions((TModel)model, x => x.IncludeProperties(propertyName))).ConfigureAwait(false);

      return result.IsValid ? Array.Empty<string>() : result.Errors.Select(e => e.ErrorMessage);
   };
}
