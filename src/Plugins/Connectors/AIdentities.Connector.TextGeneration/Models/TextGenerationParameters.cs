namespace AIdentities.Connector.TextGeneration.Models;

/// <summary>
/// This class represents the default parameters used to generate text.
/// This can be set in the corresponding settings page.
/// Specific plugins can override these parameters when calling the API.
/// </summary>
public class TextGenerationParameters
{
   private const int DEFAULT_MAX_NEW_TOKENS = 200;
   private const bool DEFAULT_DO_SAMPLE = true;
   private const float DEFAULT_TEMPERATURE = 0.5f;
   private const float DEFAULT_TOP_P = 1;
   private const float DEFAULT_TYPICAL_P = 1;
   private const float DEFAULT_REPETITION_PENALTY = 1.18f;
   private const float DEFAULT_ENCODER_REPETITION_PENALTY = 1f;
   private const int DEFAULT_TOP_K = 0;
   private const int DEFAULT_MIN_LENGTH = 0;
   private const int DEFAULT_NO_REPEAT_NGRAM_SIZE = 0;
   private const int DEFAULT_NUM_BEAMS = 1;
   private const float DEFAULT_PENALTY_ALPHA = 0;
   private const float DEFAULT_LENGTH_PENALTY = 1;
   private const bool DEFAULT_EARLY_STOPPING = false;
   private const int DEFAULT_SEED = -1;
   private const bool DEFAULT_ADD_BOS_TOKEN = true;
   private const int DEFAULT_TRUNCATION_LENGTH = 2048;
   private const bool DEFAULT_BAN_EOS_TOKEN = false;
   private const bool DEFAULT_SKIP_SPECIAL_TOKENS = true;
   private const string DEFAULT_STOPPING_STRING = "\nsystem:,\nuser:,\nhuman:,\nassistant:,\n###";
   private const float DEFAULT_EPSILON_CUT_OFF = 0f;
   private const float DEFAULT_ETA_CUT_OFF = 0f;
   private const float DEFAULT_TFS = 1f;
   private const float DEFAULT_TOP_A = 0f;
   private const int DEFAULT_MIROSTAT_MODE = 0;
   private const float DEFAULT_MIROSTAT_TAU = 5.0f;
   private const float DEFAULT_MIROSTAT_ETA = 0.1f;

   public int MaxNewTokens { get; set; } = DEFAULT_MAX_NEW_TOKENS;

   public bool DoSample { get; set; } = DEFAULT_DO_SAMPLE;

   /// <summary>
   /// Primary factor to control randomness of outputs.
   /// 0 = deterministic (only the most likely token is used).
   /// Higher value = more randomness.
   /// </summary>
   [Range(0.01, 1.99)]
   public float Temperature { get; set; } = DEFAULT_TEMPERATURE;

   /// <summary>
   /// If not set to 1, select tokens with probabilities adding up to less than this number.
   /// Higher value = higher range of possible random results.
   /// </summary>
   [Range(0, 1)]
   public float TopP { get; set; } = DEFAULT_TOP_P;

   /// <summary>
   /// If not set to 1, select only tokens that are at least this much more likely to appear than random tokens, given the prior text.
   /// </summary>
   [Range(0, 1)]
   public float TypicalP { get; set; } = DEFAULT_TYPICAL_P;

   /// <summary>
   /// This sets a probability floor below which tokens are excluded from being sampled.
   /// Should be used with <see cref="TopP"/>, <see cref="TopK"/>, and <see cref="EtaCutOff"/> set to 0.
   /// In units of 1e-4.
   /// A reasonable value is 3.
   /// </summary>
   [Range(0f, 9.0f)]
   public float EpsilonCutOff { get; set; } = DEFAULT_EPSILON_CUT_OFF;

   /// <summary>
   /// Should be used with <see cref="TopP"/>, <see cref="TopK"/>, and <see cref="EpsilonCutOff"/> set to 0.')
   /// In units of 1e-4.
   /// A reasonable value is 3. 
   /// </summary>
   [Range(0f, 20.0f)]
   public float EtaCutOff { get; set; } = DEFAULT_ETA_CUT_OFF;

   /// <summary>
   /// ?
   /// </summary>
   [Range(0f, 1.0f)]
   public float Tfs { get; set; } = DEFAULT_TFS;

   /// <summary>
   /// ?
   /// </summary>
   [Range(0f, 1.0f)]
   public float TopA { get; set; } = DEFAULT_TOP_A;

   /// <summary>
   /// Exponential penalty factor for repeating prior tokens. 1 means no penalty, higher value = less repetition, lower value = more repetition.
   /// </summary>
   [Range(1f, 1.5)]
   public float RepetitionPenalty { get; set; } = DEFAULT_REPETITION_PENALTY;

   /// <summary>
   /// Also known as the "Hallucinations filter".
   /// Used to penalize tokens that are *not* in the prior text.
   /// Higher value = more likely to stay in context, lower value = more likely to diverge.
   /// </summary>
   [Range(0.8, 1.5)]
   public float EncoderRepetitionPenalty { get; set; } = DEFAULT_ENCODER_REPETITION_PENALTY;

   /// <summary>
   /// Similar to top_p, but select instead only the top_k most likely tokens.
   /// Higher value = higher range of possible random results.
   /// </summary>
   [Range(0, 200)]
   public int TopK { get; set; } = DEFAULT_TOP_K;

   /// <summary>
   /// Minimum generation length in tokens.
   /// </summary>
   [Range(0, 2000)]
   public int MinLength { get; set; } = DEFAULT_MIN_LENGTH;

   /// <summary>
   /// If not set to 0, specifies the length of token sets that are completely blocked from repeating at all.
   /// Higher values = blocks larger phrases, lower values = blocks words or letters from repeating.
   /// Only 0 or high values are a good idea in most cases.
   /// </summary>
   [Range(0, 20)]
   public int NoRepeatNgramSize { get; set; } = DEFAULT_NO_REPEAT_NGRAM_SIZE;

   /// <summary>
   /// Beam search (uses a lot of VRAM)
   /// </summary>
   [Range(1, 20)]
   public int NumBeams { get; set; } = DEFAULT_NUM_BEAMS;

   /// <summary>
   /// Beam search (uses a lot of VRAM)
   /// </summary>
   [Range(1, 20)]
   public float LengthPenalty { get; set; } = DEFAULT_LENGTH_PENALTY;

   /// <summary>
   /// Contrastive search.
   /// </summary>
   [Range(0, 5)]
   public float PenaltyAlpha { get; set; } = DEFAULT_PENALTY_ALPHA;

   /// <summary>
   /// Beam search (uses a lot of VRAM)
   /// </summary>
   [Range(1, 20)]
   public bool EarlyStopping { get; set; } = DEFAULT_EARLY_STOPPING;

   /// <summary>
   /// ?
   /// </summary>
   [Range(0, 2)]
   public int MirostatMode { get; set; } = DEFAULT_MIROSTAT_MODE;

   /// <summary>
   /// ?
   /// </summary>
   [Range(0f, 10.0f)]
   public float MirostatTau { get; set; } = DEFAULT_MIROSTAT_TAU;

   /// <summary>
   /// ?
   /// </summary>
   [Range(0f, 1.0f)]
   public float MirostatEta { get; set; } = DEFAULT_MIROSTAT_ETA;

   /// <summary>
   /// -1 for random.
   /// </summary>
   public int Seed { get; set; } = DEFAULT_SEED;

   /// <summary>
   /// Add the bos_token to the beginning of prompts.
   /// Disabling this can make the replies more creative.
   /// </summary>
   public bool AddBosToken { get; set; } = DEFAULT_ADD_BOS_TOKEN;

   /// <summary>
   /// Truncate the prompt up to this length.
   /// The leftmost tokens are removed if the prompt exceeds this length.
   /// Most models require this to be at most 2048.
   /// </summary>
   public int TruncationLength { get; set; } = DEFAULT_TRUNCATION_LENGTH;

   /// <summary>
   /// Ban the eos_token.
   /// Forces the model to never end the generation prematurely.
   /// </summary>
   public bool BanEosToken { get; set; } = DEFAULT_BAN_EOS_TOKEN;

   /// <summary>
   /// Skip special tokens.
   /// Some specific models need this unset.
   /// </summary>
   public bool SkipSpecialTokens { get; set; } = DEFAULT_SKIP_SPECIAL_TOKENS;

   /// <summary>
   /// List of stopping string, e.g. "\\nYour Assistant:", "\\nThe assistant:"'
   /// </summary>
   public List<string> StoppingStrings { get; set; } = DEFAULT_STOPPING_STRING.Split(',').ToList();
}

