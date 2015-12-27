using System;
using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;

namespace LiveTex.SampleApp
{
	public partial class NewOfflineConversationPage
		: PhoneApplicationPage
	{
		public NewOfflineConversationPage()
		{
			InitializeComponent();

			DataContext = new NewOfflineConversationViewModel();
		}

		private NewOfflineConversationViewModel ViewModel => (NewOfflineConversationViewModel)DataContext;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.NavigatedTo().LogAsyncError();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			if(e.NavigationMode == NavigationMode.New
				&& e.Uri.OriginalString.Contains("OfflineConversationPage.xaml")
				&& e.IsNavigationInitiator)
			{
				NavigationService.RemoveBackEntry();
			}
		}

		private void AppBarSendClick(object sender, EventArgs e)
		{
			Focus();
			Dispatcher.BeginInvoke(() => ViewModel.CreateOfflineConversationCommand.ExecuteSafe());
		}
	}
}