using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LiveTex.SDK;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.LiveTex
{
	internal static class LiveTexClient
	{
		private static ILiveTexClient _client;
		
		public static async Task Initialize(string key, string applicationID, string authServerUri)
		{
			var factory = new LiveTexClientFactory(key, applicationID, new Uri(authServerUri));
			_client = await factory.CreateAsync(Token, Capabilities.Chat, Capabilities.FilesReceive, Capabilities.Invitation);

			Token = _client.GetToken();
		}

		public static ILiveTexClient Client
		{
			get
			{
				return _client;
			}
		}

		public static string Message { get; set; }

		public static void RemoveToken()
		{
			Token = null;
		}

		private static string Token
		{
			get
			{
				var settings = IsolatedStorageSettings.ApplicationSettings;
				
				string token;
				settings.TryGetValue("TOKEN", out token);

				return token;
			}
			set
			{
				var settings = IsolatedStorageSettings.ApplicationSettings;

				settings.Remove("TOKEN");

				if(string.IsNullOrWhiteSpace(value))
				{
					return;
				}

				settings.Add("TOKEN", value);
			}
		}
	}
}
