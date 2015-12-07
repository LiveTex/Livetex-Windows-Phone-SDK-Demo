using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class OfflineConversationsViewModel
		: ViewModel
	{
		protected override async Task OnNavigatedTo()
		{
			await WrapRequest(RefreshConversationsList);
			await base.OnNavigatedTo();
		}

		#region Properties

		private bool _hasConversations;
		public bool HasConversations
		{
			get { return _hasConversations; }
			set { SetValue(ref _hasConversations, value); }
		}

		private List<OfflineConversationWrapper> _conversations;
		public List<OfflineConversationWrapper> Conversations
		{
			get { return _conversations; }
			set
			{
				if(SetValue(ref _conversations, value))
				{
					HasConversations = Conversations?.Any() ?? false;
				}
			}
		}

		#endregion

		#region Commands

		private DelegateCommand _openConversationCommand;
		public DelegateCommand OpenConversationCommand => GetCommand(ref _openConversationCommand, OpenConversation, o => o is OfflineConversation);

		private DelegateCommand _newConversationCommand;
		public DelegateCommand NewConversationCommand => GetCommand(ref _newConversationCommand, CreateNewConversation);

		#endregion

		private void OpenConversation(object obj)
		{
			var conversation = obj as OfflineConversation;
			if(conversation == null)
			{
				return;
			}

			App.RootFrame.Navigate(new Uri($"/View/OfflineConversationPage.xaml?id={Uri.EscapeUriString(conversation.ConversationID)}", UriKind.Relative));
		}

		public void CreateNewConversation()
		{
			App.RootFrame.Navigate(new Uri("/View/NewOfflineConversationPage.xaml", UriKind.Relative));
		}

		private async Task RefreshConversationsList()
		{
			var conversations = await Client.GetOfflineConversations();

			if(conversations == null)
			{
				Conversations = null;
				return;
			}

			var tasks = conversations.Select(OfflineConversationWrapper.CreateAsync);
			var array = await Task.WhenAll(tasks);
			Conversations = array.ToList();
		}
	}
}
