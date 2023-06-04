// Inspired by https://github.com/Azure/azure-sdk-for-net/blob/0ae9e6f717a176527aebcc819294b24e7463a884/sdk/openai/Azure.AI.OpenAI/src/Helpers/SseReader.cs

namespace AIdentities.Connector.OpenAI.Services;

/// <summary>
/// SSE specification: https://html.spec.whatwg.org/multipage/server-sent-events.html#parsing-an-event-stream
/// </summary>
public sealed class SseReader : IDisposable
{
   private readonly Stream _stream;
   private readonly StreamReader _reader;
   private bool _disposedValue;

   public SseReader(Stream stream)
   {
      _stream = stream;
      _reader = new StreamReader(stream);
   }

   public SseLine? TryReadSingleFieldEvent()
   {
      while (true)
      {
         var line = TryReadLine();
         if (line == null) return null;
         if (line.Value.IsEmpty) throw new InvalidDataException("event expected.");

         var empty = TryReadLine();
         if (empty != null && !empty.Value.IsEmpty) throw new NotSupportedException("Multi-filed events not supported.");
         if (!line.Value.IsComment)
         {
            return line; // skip comment lines
         }
      }
   }

   // TODO: we should support cancellation tokens, but StreamReader does not in NS2
   public async Task<SseLine?> TryReadSingleFieldEventAsync()
   {
      while (true)
      {
         var line = await TryReadLineAsync().ConfigureAwait(false);
         if (line == null) return null;
         if (line.Value.IsEmpty) throw new InvalidDataException("event expected.");

         var empty = await TryReadLineAsync().ConfigureAwait(false);
         if (empty != null && !empty.Value.IsEmpty) throw new NotSupportedException("Multi-filed events not supported.");

         if (!line.Value.IsComment)
         {
            return line; // skip comment lines
         }
      }
   }

   public SseLine? TryReadLine()
   {
      if (_reader.EndOfStream) return null;
      var lineText = _reader.ReadLine();
      if (lineText is not { Length: > 0 }) return SseLine.Empty;

      return TryParseLine(lineText, out SseLine line) ? line : null;
   }

   // TODO: we should support cancellation tokens, but StreamReader does not in NS2
   public async Task<SseLine?> TryReadLineAsync()
   {
      if (_reader.EndOfStream) return null;
      var lineText = await _reader.ReadLineAsync().ConfigureAwait(false);
      if (lineText is not { Length: > 0 }) return SseLine.Empty;

      return TryParseLine(lineText, out SseLine line) ? line : null;
   }

   private static bool TryParseLine(string lineText, out SseLine line)
   {
      if (lineText.Length == 0)
      {
         line = default;
         return false;
      }

      ReadOnlySpan<char> lineSpan = lineText.AsSpan();
      int colonIndex = lineSpan.IndexOf(':');
      ReadOnlySpan<char> fieldValue = lineSpan.Slice(colonIndex + 1);

      bool hasSpace = false;
      if (fieldValue.Length > 0 && fieldValue[0] == ' ')
      {
         hasSpace = true;
      }
      line = new SseLine(lineText, colonIndex, hasSpace);
      return true;
   }

   private void Dispose(bool disposing)
   {
      if (!_disposedValue)
      {
         if (disposing)
         {
            _reader.Dispose();
            _stream.Dispose();
         }

         _disposedValue = true;
      }
   }
   public void Dispose()
   {
      Dispose(disposing: true);
      GC.SuppressFinalize(this);
   }

   public readonly struct SseLine
   {
      public static SseLine Empty { get; } = new SseLine(string.Empty, 0, false);

      private readonly string _original;
      private readonly int _colonIndex;
      private readonly int _valueIndex;

      public bool IsEmpty => _original.Length == 0;
      public bool IsComment => !IsEmpty && _original[0] == ':';

      // TODO: we should not expose UTF16 publicly
      public ReadOnlyMemory<char> FieldName => _original.AsMemory(0, _colonIndex);
      public ReadOnlyMemory<char> FieldValue => _original.AsMemory(_valueIndex);

      internal SseLine(string original, int colonIndex, bool hasSpaceAfterColon)
      {
         _original = original;
         _colonIndex = colonIndex;
         _valueIndex = colonIndex + (hasSpaceAfterColon ? 2 : 1);
      }

      public override string ToString() => _original;
   }
}
