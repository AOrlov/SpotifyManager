using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace SpotifyManager
{
    internal class Spotify
    {
        private readonly string _accessToken;

        public Spotify(string accessToken)
        {
            _accessToken = accessToken;
        }
        
        public async Task ExportTracksToPlaylist()
        {
            var spotify = new SpotifyClient(_accessToken);
            var input = (await File.ReadAllLinesAsync(Parameters.Current.InputFilePath)).Select(s => s.Split(" — ")[1]); //author | track

            var userId = (await spotify.UserProfile.Current()).Id;
            
            var playlistName = $"My Shazam Tracks - {DateTime.Now.Date.ToShortDateString()}";
            var playlistId = (await spotify.Playlists.CurrentUsers()).Items?.FirstOrDefault(p => p.Name == playlistName)?.Id;
            if (playlistId == null)
            {
                try
                {
                    playlistId = (await spotify.Playlists.Create(userId, new PlaylistCreateRequest(playlistName))).Id;

                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }
            }

            var urlsToAdd = new List<string>();
            foreach (var song in input)
            {
                var result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song));
                var url = result.Tracks.Items?.FirstOrDefault()?.Uri;
                if (url == null)
                {
                    result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song.Split("|")[1].Trim()));
                    url = result.Tracks.Items?.FirstOrDefault()?.Uri;
                }
                if (url != null)
                {
                    urlsToAdd.Add(url);
                }
                else
                {
                    Console.WriteLine($"Could not found {song}");
                }
            }

            await spotify.Playlists.AddItems(playlistId!, new PlaylistAddItemsRequest(urlsToAdd));
            Console.WriteLine("Done");
        }
    }
}