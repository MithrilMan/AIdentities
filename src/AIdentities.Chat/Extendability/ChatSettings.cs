namespace AIdentities.Chat.Extendability;

public class ChatSettings : IChatSettings
{
   public float Temperature { get; set; }

   public float RepetitionPenality { get; set; }

   public float RepetitionPenalityRange { get; set; }

   public float TopPSamplings { get; set; }

   public float TopKSamplings { get; set; }

   public float TopASamplings { get; set; }

   public float TypicalSampling { get; set; }

   public float TailFreeSampling { get; set; }

   public float MaxGeneratedTokens { get; set; }

   public int ContextSize { get; set; }
}
