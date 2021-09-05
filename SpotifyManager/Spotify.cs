using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SpotifyAPI.Web;
using Swan;

namespace SpotifyManager
{
    internal class Spotify
    {
        private readonly string _accessToken;

        public Spotify(string accessToken)
        {
            _accessToken = accessToken ?? throw new ArgumentNullException(nameof(accessToken));
        }
        
        public async Task ExportTracksToPlaylist()
        {
            var spotify = new SpotifyClient(_accessToken);
            var input = (await File.ReadAllLinesAsync(Parameters.Current.InputFilePath)).Select(s =>
            {
                var separator = " — ";
                return s.Contains(separator) ? s.Split(separator)[1] : s;
            }).Where(s => !string.IsNullOrWhiteSpace(s)); //author | track

            
            var playlistName = Parameters.Current.PlaylistName ?? $"TOP Shazam Tracks - {DateTime.Now.Date.ToShortDateString()}";

            string playlistId = await FindOrCreatePlaylist(spotify, playlistName);

            var existingTracks = Enumerable.ToHashSet((await spotify.PaginateAll((await spotify.Playlists.Get(playlistId!)).Tracks!))
                    .Select(t =>
                    {
                        return t.Track.Type switch
                        {
                            ItemType.Track => (t.Track as FullTrack)?.Uri,
                            ItemType.Episode => (t.Track as FullEpisode)?.Uri,
                            _ => null
                        };
                    }).Where(url => !string.IsNullOrEmpty(url)));

            var urlsToAdd = new ConcurrentBag<string>();

            var tasks = input.Select(async song =>
            {
                var result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song));
                if (result.Tracks.Total == 0)
                {
                    var songArtistPair = song.Split("|");
                    if (songArtistPair.Count() > 1)
                    {
                        result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track,
                            songArtistPair[1].Trim()));
                    }
                }

                if (result.Tracks.Total > 0)
                {
                    var url = result.Tracks.Items![0].Uri;
                    if (!existingTracks.Contains(url))
                    {
                        urlsToAdd.Add(url);
                    }
                }
                else
                {
                    Console.WriteLine($"Could not found {song}");
                }
            });
            
            await Task.WhenAll(tasks);

            if (urlsToAdd.Any())
            {                
                Console.WriteLine($"{urlsToAdd.Count} tracks will be added to {playlistName} playlist.");

                foreach (var batch in urlsToAdd.Batch(100))
                {
                    await spotify.Playlists.AddItems(playlistId!, new PlaylistAddItemsRequest(batch.ToList()));
                }
            }
            else
            {
                Console.WriteLine($"No tracks will be added to {playlistName} playlist.");
            }
            Console.WriteLine($"Export completed.");
        }

        private static async Task<string> FindOrCreatePlaylist(SpotifyClient spotify, string name)
        {
            var userId = (await spotify.UserProfile.Current()).Id;

            var playlists = await spotify.Playlists.CurrentUsers();
            await foreach (var playlist in spotify.Paginate(playlists))
            {
                if (playlist.Name == name)
                {
                    return playlist.Id;
                }
            }

            return (await spotify.Playlists.Create(userId, new PlaylistCreateRequest(name))).Id;
        }
    }
}