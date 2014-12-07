using System;
using System.Collections.Specialized;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;

namespace LiveTex.SampleApp
{
	public partial class DialogPage
		: PhoneApplicationPage
	{
		public DialogPage()
		{
			InitializeComponent();

			DataContext = new DialogViewModel();
		}

		private DialogViewModel ViewModel
		{
			get { return (DialogViewModel)DataContext; }
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.Messages.CollectionChanged += MessagesCollectionChanged;
			ViewModel.NavigatedTo();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			ViewModel.Messages.CollectionChanged -= MessagesCollectionChanged;
			ViewModel.NavigatedFrom();
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
			ViewModel.SendMessageCommand.Execute(null);
		}

		private void ViewMessageTextKeyDown(object sender, KeyEventArgs e)
		{
			if (e.Key == Key.Enter)
			{
				e.Handled = true;
				ViewModel.SendMessageCommand.Execute(null);

				// Hide keyboard
				Focus();
			}
		}

		private void AbuseClick(object sender, EventArgs e)
		{
			ViewModel.AbuseCommand.Execute(null);
		}

		private void CloseDialogClick(object sender, EventArgs e)
		{
			ViewModel.CloseDialogCommand.Execute(null);
		}

		private void VoteUpClick(object sender, EventArgs e)
		{
			ViewModel.VoteUpCommand.Execute(null);
		}

		private void VoteDownClick(object sender, EventArgs e)
		{
			ViewModel.VoteDownCommand.Execute(null);
		}
	}
}