namespace AIdentities.Connector.TextGeneration.Models.API;
public record CommonRequestParameters
{
   [JsonPropertyName("max_new_tokens")]
   public int? MaxNewTokens { get; set; }

   [JsonPropertyName("do_sample")]
   public bool? DoSample { get; set; }

   [JsonPropertyName("temperature")]
   public float? Temperature { get; set; }

   [JsonPropertyName("top_p")]
   public float? TopP { get; set; }

   [JsonPropertyName("typical_p")]
   public float? TypicalP { get; set; }

   [JsonPropertyName("epsilon_cutoff")]//: 0
   public float? EpsilonCutOff { get; set; }

   [JsonPropertyName("eta_cutoff")]//: 0,  
   public float? EtaCutOff { get; set; }

   [JsonPropertyName("tfs")]//: 1,
   public float? Tfs { get; set; }

   [JsonPropertyName("top_a")]//: 0,
   public float? TopA { get; set; }

   [JsonPropertyName("repetition_penalty")]
   public float? RepetitionPenalty { get; set; }

   [JsonPropertyName("encoder_repetition_penalty")]
   public float? EncoderRepetitionPenalty { get; set; }

   [JsonPropertyName("top_k")]
   public int? TopK { get; set; }

   [JsonPropertyName("min_length")]
   public int? MinLength { get; set; }

   [JsonPropertyName("no_repeat_ngram_size")]
   public int? NoRepeatNgramSize { get; set; }

   [JsonPropertyName("num_beams")]
   public int? NumBeams { get; set; }

   [JsonPropertyName("penalty_alpha")]
   public float? PenaltyAlpha { get; set; }

   [JsonPropertyName("length_penalty")]
   public float? LengthPenalty { get; set; }

   [JsonPropertyName("early_stopping")]
   public bool? EarlyStopping { get; set; }

   [JsonPropertyName("mirostat_mode")]
   public float? MirostatMode { get; set; }

   [JsonPropertyName("mirostat_tau")]
   public float? MirostatTau { get; set; }

   [JsonPropertyName("mirostat_eta")]
   public float? MirostatEta { get; set; }

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

   /// <summary>
   /// Initialize the common request parameters from the text generation parameters
   /// </summary>
   /// <param name="parameters"></param>
   public CommonRequestParameters(TextGenerationParameters parameters)
   {
      MaxNewTokens = parameters.MaxNewTokens;
      DoSample = parameters.DoSample;
      Temperature = parameters.Temperature;
      TopP = parameters.TopP;
      TypicalP = parameters.TypicalP;
      EpsilonCutOff = parameters.EpsilonCutOff;
      EtaCutOff = parameters.EtaCutOff;
      Tfs = parameters.Tfs;
      TopA = parameters.TopA;
      RepetitionPenalty = parameters.RepetitionPenalty;
      EncoderRepetitionPenalty = parameters.EncoderRepetitionPenalty;
      TopK = parameters.TopK;
      MinLength = parameters.MinLength;
      NoRepeatNgramSize = parameters.NoRepeatNgramSize;
      NumBeams = parameters.NumBeams;
      PenaltyAlpha = parameters.PenaltyAlpha;
      LengthPenalty = parameters.LengthPenalty;
      EarlyStopping = parameters.EarlyStopping;
      MirostatMode = parameters.MirostatMode;
      MirostatTau = parameters.MirostatTau;
      MirostatEta = parameters.MirostatEta;
      Seed = parameters.Seed;
      AddBosToken = parameters.AddBosToken;
      TruncationLength = parameters.TruncationLength;
      BanEosToken = parameters.BanEosToken;
      SkipSpecialTokens = parameters.SkipSpecialTokens;
      StoppingStrings = parameters.StoppingStrings;
   }
}
