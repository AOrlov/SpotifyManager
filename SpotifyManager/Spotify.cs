using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
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
        
        public async Task ExportTracksToPlaylist(IEnumerable<InputEntry> tracks)
        {
            var spotify = new SpotifyClient(_accessToken);
            
            var playlistId = await FindOrCreatePlaylist(spotify, Parameters.Current.PlaylistName);
            var existingTrackUrl = await GetAllTracksFromPlaylist(spotify, playlistId);
            var newTrackUrls = await SearchTracks(tracks, spotify);
            var urlsToAdd = newTrackUrls.Except(existingTrackUrl).ToArray();
            
            foreach (var batch in urlsToAdd.Batch(100))
            {
                await spotify.Playlists.AddItems(playlistId!, new PlaylistAddItemsRequest(batch.ToList()));
            }
            
            Console.WriteLine($"Export completed. {urlsToAdd.Length} tracks have been added to {Parameters.Current.PlaylistName}.");
        }

        private async Task<IEnumerable<string>> SearchTracks(IEnumerable<InputEntry> input, SpotifyClient spotify)
        {
            var urlsToAdd = new ConcurrentBag<string>();

            var tasks = input.Select(async song =>
            {
                var result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song.ToString()));
                if (result.Tracks.Total == 0)
                {
                    if (!string.IsNullOrWhiteSpace(song.Track))
                    {
                        result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song.Track));
                    }
                }

                if (result.Tracks.Total > 0)
                {
                    var url = result.Tracks.Items![0].Uri;
                    urlsToAdd.Add(url);
                }
                else
                {
                    Console.WriteLine($"Could not found {song}");
                }
            });

            await Task.WhenAll(tasks);
            return urlsToAdd;
        }

        private static async Task<IEnumerable<string>> GetAllTracksFromPlaylist(SpotifyClient spotify, string playlistId)
        {
            var tracks = new List<string>();
            await foreach (var track in spotify.Paginate((await spotify.Playlists.Get(playlistId)).Tracks!))
            {
                if (track.Track.Type == ItemType.Track)
                {
                    tracks.Add(((FullTrack)track.Track).Uri);
                }
            }

            return tracks;
        }

        private static async Task<string> FindOrCreatePlaylist(SpotifyClient spotify, string name)
        {
            await foreach (var playlist in spotify.Paginate(await spotify.Playlists.CurrentUsers()))
            {
                if (playlist.Name == name)
                {
                    return playlist.Id;
                }
            }

            var userId = (await spotify.UserProfile.Current()).Id;

            return (await spotify.Playlists.Create(userId, new PlaylistCreateRequest(name))).Id;
        }
    }
}