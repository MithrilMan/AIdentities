namespace AIdentities.Connector.TextGeneration.Models.API;

[Serializable]
public class ChatCompletionRequest
{
   [JsonPropertyName("prompt")]
   public string Prompt { get; set; }

   [JsonPropertyName("max_new_tokens")]
   public int? MaxNewTokens { get; set; }

   [JsonPropertyName("do_sample")]
   public bool? DoSample { get; set; }

   [JsonPropertyName("temperature")]
   public double? Temperature { get; set; }

   [JsonPropertyName("top_p")]
   public double? TopP { get; set; }

   [JsonPropertyName("typical_p")]
   public double? TypicalP { get; set; }

   [JsonPropertyName("repetition_penalty")]
   public double? RepetitionPenalty { get; set; }

   [JsonPropertyName("encoder_repetition_penalty")]
   public double? EncoderRepetitionPenalty { get; set; }

   [JsonPropertyName("top_k")]
   public int? TopK { get; set; }

   [JsonPropertyName("min_length")]
   public int? MinLength { get; set; }

   [JsonPropertyName("no_repeat_ngram_size")]
   public int? NoRepeatNgramSize { get; set; }

   [JsonPropertyName("num_beams")]
   public int? NumBeams { get; set; }

   [JsonPropertyName("penalty_alpha")]
   public double? PenaltyAlpha { get; set; }

   [JsonPropertyName("length_penalty")]
   public double? LengthPenalty { get; set; }

   [JsonPropertyName("early_stopping")]
   public bool? EarlyStopping { get; set; }

   [JsonPropertyName("seed")]
   public int? Seed { get; set; }

   [JsonPropertyName("add_bos_token")]
   public bool? AddBosToken { get; set; }

   [JsonPropertyName("truncation_length")]
   public int? TruncationLength { get; set; }

   [JsonPropertyName("ban_eos_token")]
   public bool? BanEosToken { get; set; }

   [JsonPropertyName("skip_special_tokens")]
   public bool? SkipSpecialTokens { get; set; }

   [JsonPropertyName("stopping_strings")]
   public IList<string> StoppingStrings { get; set; } = new List<string>();

   public ChatCompletionRequest(string prompt, TextGenerationParameters parameters)
   {
      Prompt = prompt;
      //map parameters value to these properties
      MaxNewTokens = parameters.MaxNewTokens;
      DoSample = parameters.DoSample;
      Temperature = parameters.Temperature;
      TopP = parameters.TopP;
      TypicalP = parameters.TypicalP;
      RepetitionPenalty = parameters.RepetitionPenalty;
      EncoderRepetitionPenalty = parameters.EncoderRepetitionPenalty;
      TopK = parameters.TopK;
      MinLength = parameters.MinLength;
      NoRepeatNgramSize = parameters.NoRepeatNgramSize;
      NumBeams = parameters.NumBeams;
      PenaltyAlpha = parameters.PenaltyAlpha;
      LengthPenalty = parameters.LengthPenalty;
      EarlyStopping = parameters.EarlyStopping;
      Seed = parameters.Seed;
      AddBosToken = parameters.AddBosToken;
      TruncationLength = parameters.TruncationLength;
      BanEosToken = parameters.BanEosToken;
      SkipSpecialTokens = parameters.SkipSpecialTokens;
      StoppingStrings = parameters.StoppingStrings;
   }
}
