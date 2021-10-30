using System;
using System.Collections;
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
        
        public async Task ExportTracksToPlaylist(IEnumerable<InputEntry> tracks, PlaylistModerator moderator)
        {
            var spotify = new SpotifyClient(_accessToken);
            
            var playlistId = await FindOrCreatePlaylist(spotify, Parameters.Current.PlaylistName);

            var moderatedTracks = await SearchTracks(tracks, spotify)
                .Except(GetAllTracksFromPlaylist(spotify, playlistId), FullTrackComparer.Instance)
                .Moderate(moderator)
                .ToArrayAsync();
            
            foreach (var batchOfUrls in moderatedTracks.Select(t => t.Uri).Batch(100))
            {
                await spotify.Playlists.AddItems(playlistId!, new PlaylistAddItemsRequest(batchOfUrls.ToList()));
            }
            
            Console.WriteLine($"Export completed. {moderatedTracks.Length} tracks have been added to {Parameters.Current.PlaylistName}.");
        }

        private async IAsyncEnumerable<FullTrack> SearchTracks(IEnumerable<InputEntry> input, SpotifyClient spotify)
        {
            await foreach (InputEntry song in input.ToAsyncEnumerable())
            {
                var result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, $"{song.Artist} {song.Track}"));
                if (result.Tracks.Total == 0)
                {
                    if (!string.IsNullOrWhiteSpace(song.Track))
                    {
                        result = await spotify.Search.Item(new SearchRequest(SearchRequest.Types.Track, song.Track));
                    }
                }

                if (result.Tracks.Total > 0)
                {
                    yield return result.Tracks.Items![0];
                }
                else
                {
                    Console.WriteLine($"Could not found {song}");
                }
            }
        }

        private static async IAsyncEnumerable<FullTrack> GetAllTracksFromPlaylist(SpotifyClient spotify, string playlistId)
        {
            await foreach (var track in spotify.Paginate((await spotify.Playlists.Get(playlistId)).Tracks!))
            {
                if (track.Track.Type == ItemType.Track)
                {
                    yield return (FullTrack)track.Track;
                }
            }
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