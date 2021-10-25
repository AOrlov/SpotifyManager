using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CommandLine;

namespace SpotifyManager
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += CurrentDomainOnUnhandledException;
            
            Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(p =>
                {
                    Parameters.Current = p;
                    Parameters.Current.PlaylistName ??= $"TOP Shazam Tracks - {DateTime.Now.Date.ToShortDateString()}";
                })
                .WithNotParsed(errors => throw new ArgumentException(string.Join(",", errors)));

            var auth = new SpotifyAuthenticator();
            var spotify = new Spotify(await auth.Auth());
            var moderator = new PlaylistModerator();
            await spotify.ExportTracksToPlaylist(ReadInputFile(Parameters.Current.InputFilePath), moderator);
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e);
            Environment.Exit(1);
        }
        
        
        private static IEnumerable<InputEntry> ReadInputFile(string path)
        {
            foreach (var line in File.ReadAllLines(path))
            {
                if (!string.IsNullOrWhiteSpace(line))
                {
                    yield return InputEntry.Parse(line, Parameters.Current.Template);
                }
            }
        }
    }
}