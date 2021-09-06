using System;
using Tokens;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace SpotifyManager
{
    internal class InputEntry
    {
        private static readonly Tokenizer Tokenizer = new(new TokenizerOptions
        {
            TokenStringComparison = StringComparison.OrdinalIgnoreCase,
            TrimTrailingWhiteSpace = true,
        });

        public string Any { get; set; }
        
        public string Artist { get; set; }

        public string Track { get; set; }

        public static InputEntry Parse(string input, string template)
        {
            return Tokenizer.Tokenize<InputEntry>(template, input).Value;
        }

        public override string ToString()
        {
            return $"Artist: {Artist ?? "N/A"}, Track: {Track}";
        }
    }
}