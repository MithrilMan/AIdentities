namespace AIdentities.BrainButler.Commands.ChangeTheme;

record Args
{
   public string? WhatToChange { get; set; }
   public bool? IsDarkPalette { get; set; }
}
