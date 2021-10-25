using System.Collections.Generic;
using SpotifyAPI.Web;

namespace SpotifyManager
{
    // ReSharper disable once ClassNeverInstantiated.Global
    internal sealed class FullTrackComparer : IEqualityComparer<FullTrack>
    {
        public static FullTrackComparer Instance { get; } = new FullTrackComparer();

        private FullTrackComparer()
        {
        }
        
        public bool Equals(FullTrack x, FullTrack y)
        {
            return y != null && x != null && x.Id == y.Id; //fine so far
        }

        public int GetHashCode(FullTrack obj)
        {
            return obj.GetHashCode();
        }
    }
}