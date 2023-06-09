﻿namespace AIdentities.UI.Features.AIdentityManagement.Components;

public partial class TabGeneric
{
   class State
   {
      public bool IsEditing { get; set; } = false;
      public bool IsDragging { get; set; } = false;
      public string? Name { get; set; }
      public string? Image { get; set; }
      public string? Description { get; set; }
      public string? Personality { get; set; }

      public bool IsTouched { get; set; }

      public AIdentity? CurrentAIdentity { get; private set; }

      internal void SetAIdentity(AIdentity? aIdentity)
      {
         CurrentAIdentity = aIdentity;

         Name = aIdentity?.Name;
         Description = aIdentity?.Description;
         Image = aIdentity?.Image;
         Personality = aIdentity?.Personality;
      }
   }

   private readonly State _state = new State();
}
