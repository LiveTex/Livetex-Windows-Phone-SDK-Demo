using System;
using System.Threading;
using System.Threading.Tasks;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using Microsoft.Phone.Notification;

namespace LiveTex.SampleApp.LiveTex
{
	internal static class LiveTexClient
	{
		private const string cPushChanelName = "LiveTextPushChanel";

		public static string Message { get; set; }
		public static string OfflineMessage { get; set; }

		private static string Token
		{
			get { return Storage.GetValue<string>("TOKEN"); }
			set { Storage.SetValue("TOKEN", value); }
		}

		public static void RemoveToken()
		{
			Token = null;
		}

		private static HttpNotificationChannel _notificationChannel;

		private static async Task<string> GetPushChanelUri()
		{
			if (_notificationChannel != null)
			{
				return _notificationChannel.ChannelUri.ToString();
			}

			_notificationChannel = HttpNotificationChannel.Find(cPushChanelName);

			if (_notificationChannel == null)
			{
				_notificationChannel = new HttpNotificationChannel(cPushChanelName);

				var tcs = new TaskCompletionSource<bool>();

				EventHandler<NotificationChannelUriEventArgs> uriUpdated = null;
				uriUpdated = (o, e) =>
				{
					_notificationChannel.ChannelUriUpdated -= uriUpdated;
					tcs.TrySetResult(true);
				};

				EventHandler<NotificationChannelErrorEventArgs> errorOccured = null;
				errorOccured = (o, e) =>
				{
					_notificationChannel.ErrorOccurred -= errorOccured;
					tcs.SetException(new Exception(e.Message));
				};

				_notificationChannel.ChannelUriUpdated += uriUpdated;
				_notificationChannel.ErrorOccurred += errorOccured;

				_notificationChannel.Open();
				_notificationChannel.BindToShellToast();

				await tcs.Task;
			}

			return _notificationChannel.ChannelUri.ToString();
		}

		private static TaskCompletionSource<ILiveTexClient> _getClientTcs;

		public static async Task<ILiveTexClient> GetClient()
		{
			var tcs = new TaskCompletionSource<ILiveTexClient>();

			var currentTcs = Interlocked.CompareExchange(ref _getClientTcs, tcs, null);
			if (currentTcs != null)
			{
				return await currentTcs.Task;
			}

			try
			{
				if (!AppCredentials.IsSet)
				{
					throw new Exception("Не заданы параметры авторизации приложения Key, ApplicationID, AuthServerUri");
				}

				var pushChanelUri = await GetPushChanelUri();

				var factory = new LiveTexClientFactory(AppCredentials.Key, AppCredentials.ApplicationID, new Uri(AppCredentials.AuthServerUri, UriKind.Absolute));
				var client = await factory.CreateAsync(pushChanelUri, Token, Capabilities.Chat, Capabilities.FilesReceive, Capabilities.Invitation, Capabilities.Offline, Capabilities.FilesSend);

				Token = client.GetToken();
				tcs.TrySetResult(client);

				return client;
			}
			catch (Exception ex)
			{
				tcs.SetException(ex);
				Interlocked.Exchange(ref _getClientTcs, null);

				throw;
			}
		}
	}
}
