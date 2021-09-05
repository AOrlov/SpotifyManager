using System;
using System.Threading.Tasks;
using CommandLine;

namespace SpotifyManager
{
    internal static class Program
    {
        static async Task Main(string[] args)
        {
            Parser.Default.ParseArguments<Parameters>(args)
                .WithParsed(p =>
                {
                    Parameters.Current = p;
                })
                .WithNotParsed(errors =>
                {
                    Console.WriteLine(string.Join(",", errors));
                    Environment.Exit(1);
                });

            var auth = new SpotifyAuthenticator();
            var spotify = new Spotify(await auth.Auth());
            await spotify.ExportTracksToPlaylist();
        }
    }
}