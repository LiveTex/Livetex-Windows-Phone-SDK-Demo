﻿using System;
using System.Collections.Specialized;
using System.ComponentModel;
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
			ViewModel.PropertyChanged += ViewModelPropertyChanged;
			ViewModel.NavigatedTo();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			ViewModel.Messages.CollectionChanged -= MessagesCollectionChanged;
			ViewModel.PropertyChanged -= ViewModelPropertyChanged;
			ViewModel.NavigatedFrom();
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

		private void ViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (viewAbuseMenuItem != null && ViewModel != null)
			{
				viewAbuseMenuItem.IsEnabled = ViewModel.IsAbuseAllowed;
			}
		}
		
		private void SendMessageClick(object sender, EventArgs e)
		{
			SendMessageAndHideKeyboard();
		}

		private void SendMessageAndHideKeyboard()
		{
			ViewModel.SendMessageCommand.Execute(null);

			// Hide keyboard
			Focus();
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