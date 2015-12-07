using System;
using System.IO.IsolatedStorage;
using System.Threading.Tasks;
using LiveTex.SDK;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.LiveTex
{
	internal static class LiveTexClient
	{
		public static async Task Initialize(string key, string applicationID, string authServerUri)
		{
			if(Client != null)
			{
				Client.Dispose();
				Client = null;
			}

			var factory = new LiveTexClientFactory(key, applicationID, new Uri(authServerUri));
			Client = await factory.CreateAsync(Token, Capabilities.Chat, Capabilities.FilesReceive, Capabilities.Invitation, Capabilities.Offline, Capabilities.FilesSend);

			Token = Client.GetToken();
		}

		public static ILiveTexClient Client { get; private set; }

		public static string Message { get; set; }
		public static string OfflineMessage { get; set; }

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
