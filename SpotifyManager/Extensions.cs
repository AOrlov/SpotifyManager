using System.Collections.Generic;
using System.Diagnostics;

namespace SpotifyManager
{
    internal static class Extensions
    {
        public static Process Start(string fileName)
        {
            return Process.Start(new ProcessStartInfo(fileName)
            {
                UseShellExecute = true,
            });
        }
        
        public static IAsyncEnumerable<T> Moderate<T>(this IAsyncEnumerable<T> entries, IModerator<T> moderator)
        {
            return moderator.ModerateAsync(entries);
        }
    }
}