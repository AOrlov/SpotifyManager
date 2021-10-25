using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SpotifyAPI.Web;

namespace SpotifyManager
{
    internal class PlaylistModerator
    {
        public async Task<IEnumerable<FullTrack>> ModerateAsync(IEnumerable<FullTrack> inputTracks)
        {
            var tracks = inputTracks.ToDictionary(k => k.Id, t => t);

            string filePath = Path.GetTempFileName() + ".txt";
            await using FileStream stream = new FileStream(filePath, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose);
            await using StreamWriter writer = new StreamWriter(stream);
            foreach (var track in tracks.Values)
            {
                await writer.WriteLineAsync($"{track.Artists} | {track.Name}, | {track.Id}");
            }

            var process = Process.Start(
                new ProcessStartInfo(filePath)
                {
                    UseShellExecute = true,
                });

            process.WaitForExit();

            using StreamReader reader = new StreamReader(stream);

            List<FullTrack> filteredTracks = new List<FullTrack>();
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var id = line?.Split(" | ").Last();
                if (id != null && tracks.TryGetValue(id, out var track))
                {
                    filteredTracks.Add(track);
                }
            }

            return filteredTracks;
        }
    }
}