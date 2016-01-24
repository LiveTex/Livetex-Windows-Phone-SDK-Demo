using System;
using System.Threading.Tasks;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class MainViewModel
		: ViewModel
	{
		protected override bool InitializeClient => false;

		protected override Task Initialize(object parameter)
		{
			// Очищаем стек навигации, для удалени неактуальной истории работы с приложением
			while(App.RootFrame.RemoveBackEntry() != null)
			{
			}

			Key = AppCredentials.Key;
			AppID = AppCredentials.ApplicationID;
			AuthUri = AppCredentials.AuthServerUri;

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

		private AsyncCommand _setCredentialsCommand;
		public AsyncCommand SetCredentialsCommand => GetAsyncCommand(ref _setCredentialsCommand, SetCredentials);

		private async Task SetCredentials()
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

					LiveTexClient.RemoveToken();
					AppCredentials.Set(Key, AppID, AuthUri);

					var client = await LiveTexClient.GetClient();
					if(client != null)
					{
						AppCredentials.Save();
						App.RootFrame.Navigate(new Uri("/View/SelectServicePage.xaml", UriKind.Relative));
					}
				});
		}
	}
}
