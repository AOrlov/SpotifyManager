using System;
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
                })
                .WithNotParsed(errors => throw new ArgumentException(string.Join(",", errors)));

            var auth = new SpotifyAuthenticator();
            var spotify = new Spotify(await auth.Auth());
            await spotify.ExportTracksToPlaylist();
        }

        private static void CurrentDomainOnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Console.WriteLine(e);
            Environment.Exit(1);
        }
    }
}