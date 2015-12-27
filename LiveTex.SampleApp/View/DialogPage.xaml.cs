using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LiveTex.SampleApp
{
	public partial class DialogPage
		: PhoneApplicationPage
	{
		public DialogPage()
		{
			InitializeComponent();

			DataContext = new DialogViewModel();

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
			var page = d as DialogPage;

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

					_abuseMenuItem = ApplicationBar.MenuItems.OfType<ApplicationBarMenuItem>().FirstOrDefault(i => i.Text == "написать жалобу");
				}

				return _abuseMenuItem;
			}
		}

		private DialogViewModel ViewModel => (DialogViewModel)DataContext;

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.Messages.CollectionChanged += MessagesCollectionChanged;
			ViewModel.NavigatedTo().LogAsyncError();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			ViewModel.Messages.CollectionChanged -= MessagesCollectionChanged;
			ViewModel.NavigatedFrom().LogAsyncError();
		}

		protected override void OnBackKeyPress(CancelEventArgs e)
		{
			ViewModel.CloseDialogCommand.Execute(null);

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

		private void SendFileClick(object sender, EventArgs e)
		{
			ViewModel.SendFileCommand.ExecuteSafe();
		}

		private void AbuseClick(object sender, EventArgs e)
		{
			ViewModel.AbuseCommand.Execute(null);
		}

		private void CloseDialogClick(object sender, EventArgs e)
		{
			ViewModel.CloseDialogCommand.Execute(null);
		}

		private void GoodButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			ViewModel.VoteUpCommand.Execute(null);
		}

		private void BadButtonTap(object sender, System.Windows.Input.GestureEventArgs e)
		{
			ViewModel.VoteDownCommand.Execute(null);
		}
	}
}