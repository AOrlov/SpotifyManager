using System;
using System.Collections.Generic;
using System.Security.Authentication;
using System.Threading;
using System.Threading.Tasks;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;

namespace SpotifyManager
{
    internal class SpotifyAuthenticator
    {
        private readonly ManualResetEventSlim _tokenReceived = new ManualResetEventSlim();
        private EmbedIOAuthServer _server;

        private string _accessToken;

        public async Task<string> Auth()
        {
            _server = new EmbedIOAuthServer(new Uri("http://localhost:5001/callback"), 5001);
            await _server.Start();

            _server.ImplictGrantReceived += OnImplicitGrantReceived;
            _server.ErrorReceived += OnErrorReceived;

            var request = new LoginRequest(_server.BaseUri, "41b261ad535c4476bb2421cee5c795da", LoginRequest.ResponseType.Token)
            {
                Scope = new List<string> { 
                    Scopes.PlaylistModifyPrivate, 
                    Scopes.UserLibraryModify,
                    Scopes.PlaylistReadPrivate, 
                    Scopes.PlaylistModifyPublic,
                    Scopes.PlaylistReadCollaborative
                }
            };
            BrowserUtil.Open(request.ToUri());

            _tokenReceived.Wait();

            return _accessToken;
        }

        private async Task OnImplicitGrantReceived(object sender, ImplictGrantResponse response)
        {
            Console.WriteLine($"Token received. Continue...");
            _accessToken = response.AccessToken;
            await _server.Stop();
            _tokenReceived.Set();
        }

        private async Task OnErrorReceived(object sender, string error, string state)
        {
            Console.WriteLine($"Aborting authorization, error received: {error}");
            await _server.Stop();
            throw new AuthenticationException(error);
        }
    }
}