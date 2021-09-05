using CommandLine;

namespace SpotifyManager
{
    internal class Parameters
    {
        [Option('f', "file", Required = true, HelpText = "The path to input file.")]
        public bool InputFilePath { get; set; }
    }
}