using System;
using System.Threading.Tasks;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class MainViewModel
		: ViewModel
	{
		private const string cKeyKey = "ApplicationKey";
		private const string cAppIDKey = "ApplicationID";
		private const string cAuthUriKey = "AuthenticatedUriKey";

		protected override Task Initialize(object parameter)
		{
			Key = Storage.GetValue(cKeyKey, Config.cKey);
			AppID = Storage.GetValue(cAppIDKey, Config.cApplicationID);
			AuthUri = Storage.GetValue(cAuthUriKey, Config.cAuthServiceUri);

			return Task.FromResult(true);
		}

		private string _key;
		public string Key
		{
			get { return _key; }
			set { SetValue(ref _key, value); }
		}

		private string _appID;
		public string AppID
		{
			get { return _appID; }
			set { SetValue(ref _appID, value); }
		}

		private string _authUri;
		public string AuthUri
		{
			get { return _authUri; }
			set { SetValue(ref _authUri, value); }
		}

		private DelegateCommand _removeTokenCommand;
		public DelegateCommand RemoveTokenCommand => GetCommand(ref _removeTokenCommand, LiveTexClient.RemoveToken);

		private AsyncCommand _initializeClientCommand;
		public AsyncCommand InitializeClientCommand => GetAsyncCommand(ref _initializeClientCommand, InitializeClient);

		private async Task InitializeClient()
		{
			await WrapRequest(async () =>
				{
					if (string.IsNullOrWhiteSpace(Key))
					{
						throw new Exception("Key не задан");
					}

					if (string.IsNullOrWhiteSpace(AppID))
					{
						throw new Exception("ApplicationID не задан");
					}

					if (string.IsNullOrWhiteSpace(AuthUri))
					{
						throw new Exception("Authentication Uri не задан");
					}

					if (!Equals(Storage.GetValue<string>(cKeyKey), Key)
						|| !Equals(Storage.GetValue<string>(cAppIDKey), AppID)
						|| !Equals(Storage.GetValue<string>(cAuthUriKey), AuthUri))
					{
						LiveTexClient.RemoveToken();
					}

					Storage.SetValue(cKeyKey, Key);
					Storage.SetValue(cAppIDKey, AppID);
					Storage.SetValue(cAuthUriKey, AuthUri);

					await LiveTexClient.Initialize(Key, AppID, AuthUri);

					App.RootFrame.Navigate(new Uri("/View/SelectServicePage.xaml", UriKind.Relative));
				});
		}
	}
}
