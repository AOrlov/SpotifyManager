using CommandLine;
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable once ClassNeverInstantiated.Global

namespace SpotifyManager
{
    internal class Parameters
    {
        [Option('f', "file", Required = true, HelpText = "The path to input file.")]
        public string InputFilePath { get; set; }
        
        [Option('n', "name", Required = false, HelpText = "Playlist name.", Default = null)]
        public string PlaylistName { get; set; }
        
        [Option('t', "template", Required = true, HelpText = @"Track template. Example: {Any} — {Artist} | {Track} will match abc — BODIEV | Караван")]
        public string Template { get; set; }

        public static Parameters Current { get; set; }
    }
}