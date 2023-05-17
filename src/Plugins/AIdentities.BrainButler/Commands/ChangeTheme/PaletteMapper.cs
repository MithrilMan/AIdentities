namespace AIdentities.BrainButler.Commands.ChangeTheme;
public static class PaletteMapper
{
   public static PaletteLight CreateLightPalette(PaletteReference paletteDto)
   {
      return new PaletteLight()
      {
         Primary = paletteDto.Primary,
         Secondary = paletteDto.Secondary,
         Tertiary = paletteDto.Tertiary,
         Background = paletteDto.Background,
         Surface = paletteDto.Surface,
         DrawerBackground = paletteDto.DrawerBackground,
         DrawerText = paletteDto.DrawerText,
         AppbarBackground = paletteDto.AppbarBackground,
         AppbarText = paletteDto.AppbarText,
         TextPrimary = paletteDto.TextPrimary,
         TextSecondary = paletteDto.TextSecondary,
         TableLines = paletteDto.TableLines,
         ActionDefault = paletteDto.ActionDefault,
         ActionDisabled = paletteDto.ActionDisabled,
         Divider = paletteDto.Divider,
         DividerLight = paletteDto.DividerLight,
         TextDisabled = paletteDto.TextDisabled,
      };
   }

   public static PaletteDark CreateDarkPalette(PaletteReference paletteDto)
   {
      return new PaletteDark()
      {
         Primary = paletteDto.Primary,
         Secondary = paletteDto.Secondary,
         Tertiary = paletteDto.Tertiary,
         Background = paletteDto.Background,
         Surface = paletteDto.Surface,
         DrawerBackground = paletteDto.DrawerBackground,
         DrawerText = paletteDto.DrawerText,
         AppbarBackground = paletteDto.AppbarBackground,
         AppbarText = paletteDto.AppbarText,
         TextPrimary = paletteDto.TextPrimary,
         TextSecondary = paletteDto.TextSecondary,
         TableLines = paletteDto.TableLines,
         ActionDefault = paletteDto.ActionDefault,
         ActionDisabled = paletteDto.ActionDisabled,
         Divider = paletteDto.Divider,
         DividerLight = paletteDto.DividerLight,
         TextDisabled = paletteDto.TextDisabled,
      };
   }
}
