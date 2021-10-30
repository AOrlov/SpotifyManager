using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SpotifyAPI.Web;

namespace SpotifyManager
{
    internal class PlaylistModerator : IModerator<FullTrack>
    {
        public async IAsyncEnumerable<FullTrack> ModerateAsync(IAsyncEnumerable<FullTrack> inputTracks)
        {
            var tracks = await inputTracks.ToDictionaryAsync(k => k.Id, t => t);

            if (!tracks.Any())
            {
                yield break;
            }

            string filePath = Path.GetTempFileName() + ".txt";
            await using FileStream stream = CreateFile(filePath);
            await using (StreamWriter writer = new StreamWriter(stream, leaveOpen: true))
            {
                foreach (var track in tracks.Values)
                {
                    await writer.WriteLineAsync($"{string.Join(',', track.Artists.Select(a => a.Name))} | {track.Name}, | {track.Id}");
                }
            }
            
            OpenEditorAndWait(filePath);

            using StreamReader reader = new StreamReader(filePath);
            
            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                var id = line?.Split(" | ").Last();
                if (id != null && tracks.TryGetValue(id, out var track))
                {
                    yield return track;
                }
            }
        }

        private static void OpenEditorAndWait(string filePath)
        {
            Console.WriteLine("Choose tracks to add and press ENTER key to continue...");
            Extensions.Start(filePath);
            Console.ReadLine();
        }

        private FileStream CreateFile(string path)
        {
            return new FileStream(path, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.ReadWrite, 4096, FileOptions.DeleteOnClose);
        }
    }

    internal interface IModerator<T>
    {
        public IAsyncEnumerable<T> ModerateAsync(IAsyncEnumerable<T> inputTracks);
    }
}