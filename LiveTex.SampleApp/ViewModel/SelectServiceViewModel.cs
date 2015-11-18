using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class SelectServiceViewModel
		: ViewModel
	{
		private AsyncCommand _onlineDialogCommand;
		public AsyncCommand OnlineDialogCommand => GetAsyncCommand(ref _onlineDialogCommand, OpenOnlineDialog);

		private DelegateCommand _offlineConversationsCommand;
		public DelegateCommand OfflineConversationsCommand => GetCommand(ref _offlineConversationsCommand, OpenOfflineConversations);

		private async Task OpenOnlineDialog()
		{
			using(BeginBusy())
			{
				var dialogState = await Client.GetDialogStateAsync();

				if(dialogState.State == DialogStates.NoConversation)
				{
					App.RootFrame.Navigate(new Uri("/View/RequestDialogPage.xaml", UriKind.Relative));
				}
				else
				{
					App.RootFrame.Navigate(new Uri("/View/DialogPage.xaml", UriKind.Relative));
				}
			}
		}

		private void OpenOfflineConversations()
		{
			App.RootFrame.Navigate(new Uri("/View/OfflineConversationsPage.xaml", UriKind.Relative));
		}
	}
}
