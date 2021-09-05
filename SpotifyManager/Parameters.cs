using CommandLine;

namespace SpotifyManager
{
    internal class Parameters
    {
        [Option('f', "file", Required = true, HelpText = "The path to input file.")]
        public string InputFilePath { get; set; }

        public static Parameters Current { get; set; }
    }
}