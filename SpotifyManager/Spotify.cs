using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MoreLinq;
using SpotifyAPI.Web;

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

            var userId = (await spotify.UserProfile.Current()).Id;
            
            var playlistName = $"My Shazam Tracks - {DateTime.Now.Date.ToShortDateString()}";

            string playlistId = null;

            var playlists = await spotify.Playlists.CurrentUsers();
            await foreach (var playlist in spotify.Paginate(playlists))
            {
                if (playlist.Name == playlistName)
                {
                    playlistId = playlist.Id;
                    break;
                }
            }

            playlistId ??= (await spotify.Playlists.Create(userId, new PlaylistCreateRequest(playlistName))).Id;
            
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

            var urlsToAdd = new HashSet<string>();
            
            foreach (var song in input)
            {
                var result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song));
                if (result.Tracks.Total == 0)
                {
                    result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song.Split("|")[1].Trim()));
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
            }

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
    }
}