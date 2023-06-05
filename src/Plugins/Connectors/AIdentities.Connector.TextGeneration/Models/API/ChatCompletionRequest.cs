using AIdentities.Shared.Serialization.Converters;

namespace AIdentities.Connector.TextGeneration.Models.API;
public class ChatCompletionRequest
{
   const int DEFAULT_CHAT_PROMPT_SIZE = 2048;
   const int DEFAULT_CHAT_GENERATION_ATTEMPTS = 1;
   const string DEFAULT_CHAT_INSTRUCT_COMMAND = "Continue the chat dialogue below. Write a single reply for the character \"<|character|>\".\\n\\n<|prompt|>";
   const bool DEFAULT_STOP_AT_NEWLINE = false;
   const bool DEFAULT_CONTINUE = false;
   const bool DEFAULT_REGENERATE = false;
   const string DEFAULT_CHARACTER = "Example";
   const string DEFAULT_INSTRUCTION_TEMPLATE = "Vicuna-v1.1";
   const string DEFAULT_YOUR_NAME = "You";

   [JsonPropertyName("user_input")]
   public string UserInput { get; private set; }

   [JsonPropertyName("history")]
   public IList<ChatCompletionHistoryMessage> History { get; set; } = new List<ChatCompletionHistoryMessage>();

   /// <summary>
   /// The chat mode to use.
   /// </summary>
   [JsonPropertyName("mode")]
   [JsonConverter(typeof(JsonStringEnumConverterEx<ChatCompletionMode>))]
   public ChatCompletionMode ChatCompletionMode { get; private set; }   

   /// <summary>
   /// The character name of the chatbot that will reply.
   /// </summary>
   [JsonPropertyName("character")]
   public string Character { get; private set; } = DEFAULT_CHARACTER;


   /// <summary>
   /// The instruction template to use.
   /// e.g. Vicuna-v1.1
   /// </summary>
   [JsonPropertyName("instruction_template")]
   public string InstructionTemplate { get; private set; } = DEFAULT_INSTRUCTION_TEMPLATE;

   /// <summary>
   /// The name of the user.
   /// </summary>
   [JsonPropertyName("your_name")]
   public string YourName { get; private set; } = DEFAULT_YOUR_NAME;

   [JsonPropertyName("regenerate")]
   public bool Regenerate { get; private set; } = DEFAULT_REGENERATE;

   /// <summary>
   /// Set to true to continue the <see cref="UserInput"/>.
   /// </summary>
   [JsonPropertyName("_continue")]
   public bool Continue { get; private set; } = DEFAULT_CONTINUE;

   /// <summary>
   /// Stop generating at new line character.
   /// </summary>
   [JsonPropertyName("stop_at_newline")]
   public bool StopAtNewline { get; private set; } = DEFAULT_STOP_AT_NEWLINE;



   /// <summary>
   /// Set limit on prompt size by removing old messages (while retaining context and user input)
   /// e.g. 2048
   /// </summary>
   [JsonPropertyName("chat_prompt_size")]
   public int ChatPromptSize { get; private set; } = DEFAULT_CHAT_PROMPT_SIZE;

   /// <summary>
   /// Generation attempts (for longer replies)
   /// New generations will be called until either this number is reached or no new content is generated between two iterations.
   /// </summary>
   [JsonPropertyName("chat_generation_attempts")]
   public int ChatGenerationAttempts { get; private set; } = DEFAULT_CHAT_GENERATION_ATTEMPTS;

   /// <summary>
   /// Command for chat-instruct mode.
   /// <|character|> gets replaced by the bot name, and <|prompt|> gets replaced by the regular chat prompt.
   /// </summary>
   [JsonPropertyName("chat-instruct_command")]
   public string ChatInstructCommand { get; private set; } = DEFAULT_CHAT_INSTRUCT_COMMAND;


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

   public ChatCompletionRequest(
      string userInput,
      IList<ChatCompletionHistoryMessage> history,
      ChatCompletionMode chatCompletionMode,
      string character,
      string instructionTemplate,
      string yourName,
      bool regenerate,
      bool @continue,
      bool stopAtNewline,
      int chatPromptSize,
      int chatGenerationAttempts,
      string chatInstructCommand,
      TextGenerationParameters parameters)
   {
      UserInput = userInput;
      History = history;
      ChatCompletionMode = chatCompletionMode;
      Character = character;
      InstructionTemplate = instructionTemplate;
      YourName = yourName;
      Regenerate = regenerate;
      Continue = @continue;
      StopAtNewline = stopAtNewline;
      ChatPromptSize = chatPromptSize;
      ChatGenerationAttempts = chatGenerationAttempts;
      ChatInstructCommand = chatInstructCommand;

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
