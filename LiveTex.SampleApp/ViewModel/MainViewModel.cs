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

		protected override Task Initialize()
		{
			Key = Storage.GetValue(cKeyKey, Config.cKey);

			return Task.FromResult(true);
		}

		private string _key;
		public string Key
		{
			get { return _key; }
			set { SetValue(ref _key, value); }
		}

		private DelegateCommand _removeTokenCommand;
		public DelegateCommand RemoveTokenCommand
		{
			get
			{
				if (_removeTokenCommand == null)
				{
					_removeTokenCommand = new DelegateCommand(LiveTexClient.RemoveToken);
				}

				return _removeTokenCommand;
			}
		}

		private DelegateCommand _initializeClientCommand;
		public DelegateCommand InitializeClientCommand
		{
			get
			{
				if(_initializeClientCommand == null)
				{
					_initializeClientCommand = new DelegateCommand(() => InitializeClient());
				}

				return _initializeClientCommand;
			}
		}

		private async Task InitializeClient()
		{
			await WrapRequest(async () =>
				{
					if (string.IsNullOrWhiteSpace(Key))
					{
						throw new Exception("Код не задан");
					}

					if(!Equals(Storage.GetValue<string>(cKeyKey), Key))
					{
						LiveTexClient.RemoveToken();
					}

					Storage.SetValue(cKeyKey, Key);

					await LiveTexClient.Initialize(Key, Config.cApplicationID, Config.cAuthServiceUri);

					var dialogState = await LiveTexClient.Client.GetDialogStateAsync();

					if (dialogState.State == DialogStates.NoConversation)
					{
						App.RootFrame.Navigate(new Uri("/View/RequestDialogPage.xaml", UriKind.Relative));
					}
					else
					{
						App.RootFrame.Navigate(new Uri("/View/DialogPage.xaml", UriKind.Relative));
					}
				});
		}
	}
}
