using System;
using System.Windows;
using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using LiveTex.SampleApp.Wrappers;
using Microsoft.Phone.Controls;
using GestureEventArgs = System.Windows.Input.GestureEventArgs;

namespace LiveTex.SampleApp
{
	public partial class OfflineConversationsPage
		: PhoneApplicationPage
	{
		public OfflineConversationsPage()
		{
			InitializeComponent();

			DataContext = new OfflineConversationsViewModel();
		}

		private OfflineConversationsViewModel ViewModel => (OfflineConversationsViewModel)DataContext;

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			await ViewModel.NavigatedTo();
		}

		protected override async void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			await ViewModel.NavigatedFrom();
		}

		private void NewConversationClick(object sender, EventArgs e)
		{
			ViewModel?.NewConversationCommand.ExecuteSafe();
		}

		private void ListTap(object sender, GestureEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			var conversation = element?.DataContext as OfflineConversationWrapper;

			if(conversation == null)
			{
				return;
			}

			ViewModel.OpenConversationCommand.ExecuteSafe(conversation.Conversation);
		}
	}
}