namespace AIdentities.Shared.Plugins.Pages;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class PageDefinitionAttribute : Attribute
{
   public string Title { get; }
   public string Icon { get; }
   public string Url { get; }

   public string Description { get; set; } = string.Empty;

   // This is a positional argument
   public PageDefinitionAttribute(string title, string icon, string url)
   {
      Title = title;
      Icon = icon;
      Url = url;
   }

   public PageDefinition GetPageDefinition() => new PageDefinition(
      Title: Title,
      Url: Url,
      Icon: Icon,
      Description: Description
      );
}
