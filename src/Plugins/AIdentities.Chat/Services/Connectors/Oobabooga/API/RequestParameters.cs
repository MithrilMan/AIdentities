using System.Text.Json.Serialization;

namespace AIdentities.Chat.Services.Connectors.Oobabooga.API;
public class RequestParameters
{
   [JsonPropertyName("max_new_tokens")]
   public int MaxNewTokens { get; set; } = 250;

   [JsonPropertyName("do_sample")]
   public bool DoSample { get; set; } = true;

   [JsonPropertyName("temperature")]
   public double Temperature { get; set; } = 1.3;

   [JsonPropertyName("top_p")]
   public double TopP { get; set; } = 0.1;

   [JsonPropertyName("typical_p")]
   public double TypicalP { get; set; } = 1;

   [JsonPropertyName("repetition_penalty")]
   public double RepetitionPenalty { get; set; } = 1.18;

   [JsonPropertyName("top_k")]
   public int TopK { get; set; } = 40;

   [JsonPropertyName("min_length")]
   public int MinLength { get; set; } = 0;

   [JsonPropertyName("no_repeat_ngram_size")]
   public int NoRepeatNgramSize { get; set; } = 0;

   [JsonPropertyName("num_beams")]
   public int NumBeams { get; set; } = 1;

   [JsonPropertyName("penalty_alpha")]
   public double PenaltyAlpha { get; set; } = 0;

   [JsonPropertyName("length_penalty")]
   public double LengthPenalty { get; set; } = 1;

   [JsonPropertyName("early_stopping")]
   public bool EarlyStopping { get; set; } = false;

   [JsonPropertyName("seed")]
   public int Seed { get; set; } = -1;

   [JsonPropertyName("add_bos_token")]
   public bool AddBosToken { get; set; } = true;

   [JsonPropertyName("truncation_length")]
   public int TruncationLength { get; set; } = 2048;

   [JsonPropertyName("ban_eos_token")]
   public bool BanEosToken { get; set; } = false;

   [JsonPropertyName("skip_special_tokens")]
   public bool SkipSpecialTokens { get; set; } = true;

   [JsonPropertyName("stopping_strings")]
   public List<string> StoppingStrings { get; set; } = new List<string>();
}
