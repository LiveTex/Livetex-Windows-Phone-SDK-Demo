using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LiveTex.SampleApp
{
	public partial class OfflineConversationPage
		: PhoneApplicationPage
	{
		public OfflineConversationPage()
		{
			InitializeComponent();

			DataContext = new OfflineConversationViewModel();

			Loaded += UpdateAbuseMenuItemOnLoad;
		}

		#region Attached properties

		public static readonly DependencyProperty IsAbuseMenuVisibleProperty = DependencyProperty.RegisterAttached(
			"IsAbuseMenuVisible", typeof(bool), typeof(DialogPage), new PropertyMetadata(default(bool), OnIsAbuseMenuVisibleChanged));

		public static void SetIsAbuseMenuVisible(DependencyObject element, bool value)
		{
			element.SetValue(IsAbuseMenuVisibleProperty, value);
		}

		public static bool GetIsAbuseMenuVisible(DependencyObject element)
		{
			return (bool) element.GetValue(IsAbuseMenuVisibleProperty);
		}

		private static void OnIsAbuseMenuVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var page = d as OfflineConversationPage;
			
			if (page == null)
			{
				return;
			}

			if (e.NewValue is bool)
			{
				page.OnIsAbuseMenuVisibleChanged((bool)e.NewValue);
			}
		}

		private void OnIsAbuseMenuVisibleChanged(bool value)
		{
			if (AbuseMenuItem == null)
			{
				return;
			}

			AbuseMenuItem.IsEnabled = value;
		}

		private void UpdateAbuseMenuItemOnLoad(object sender, RoutedEventArgs e)
		{
			Loaded -= UpdateAbuseMenuItemOnLoad;

			if (AbuseMenuItem == null)
			{
				return;
			}

			AbuseMenuItem.IsEnabled = GetIsAbuseMenuVisible(this);
		}

		#endregion

		private ApplicationBarMenuItem _abuseMenuItem;
		private ApplicationBarMenuItem AbuseMenuItem
		{
			get
			{
				if (_abuseMenuItem == null)
				{
					if (ApplicationBar == null)
					{
						return null;
					}

					_abuseMenuItem = ApplicationBar.MenuItems.OfType<ApplicationBarMenuItem>().FirstOrDefault(i => i.Text == "оставить жалобу");
				}

				return _abuseMenuItem;
			}
		}

		private OfflineConversationViewModel ViewModel => (OfflineConversationViewModel)DataContext;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			string conversationID;
			NavigationContext.QueryString.TryGetValue("id", out conversationID);

			ViewModel.Messages.CollectionChanged += MessagesCollectionChanged;
			ViewModel.NavigatedTo(conversationID).LogAsyncError();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			ViewModel.Messages.CollectionChanged -= MessagesCollectionChanged;
			ViewModel.NavigatedFrom().LogAsyncError();
		}

		protected override void OnBackKeyPress(CancelEventArgs e)
		{
			ViewModel.CloseDialogCommand.ExecuteSafe();

			e.Cancel = true;
			base.OnBackKeyPress(e);
		}

		private void MessagesCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(e.Action == NotifyCollectionChangedAction.Add)
			{
				viewMessagesList.ScrollIntoView(e.NewItems[0]);
			}
		}

		private void SendMessageClick(object sender, EventArgs e)
		{
			SendMessageAndHideKeyboard();
		}

		private void SendMessageAndHideKeyboard()
		{
			ViewModel.SendMessageCommand.ExecuteSafe();

			// Hide keyboard
			Focus();
		}

		private void AbuseClick(object sender, EventArgs e)
		{
			ViewModel.AbuseCommand.ExecuteSafe();
		}

		private void CloseDialogClick(object sender, EventArgs e)
		{
			ViewModel.CloseDialogCommand.ExecuteSafe();
		}

		private void GoodButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			ViewModel.VoteUpCommand.ExecuteSafe();
		}

		private void BadButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			ViewModel.VoteDownCommand.ExecuteSafe();
		}
	}
}