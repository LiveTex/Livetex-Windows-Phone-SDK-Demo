using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class DialogViewModel
		: ViewModel, ILiveTexEventsHandler
	{
		private readonly ObservableCollection<ChatMessageWrapper> _messages;

		public DialogViewModel()
		{
			_messages = new ObservableCollection<ChatMessageWrapper>();
		}

		public ObservableCollection<ChatMessageWrapper> Messages
		{
			get { return _messages; }
		}

		#region Model properties

		private string _employeeAvatar;
		public string EmployeeAvatar
		{
			get { return _employeeAvatar; }
			private set { SetValue(ref _employeeAvatar, value); }
		}

		private string _employeeName;
 		public string EmployeeName
		{
			get { return _employeeName; }
			private set { SetValue(ref _employeeName, value); }
		}

		private string _messageText;

		public string MessageText
		{
			get { return _messageText; }
			set
			{
				if (SetValue(ref _messageText, value))
				{
					SendTypingMessage();
				}
			}
		}

		private bool _conversationActive;
		public bool ConversationActive
		{
			get { return _conversationActive; }
			private set { SetValue(ref _conversationActive, value); }
		}

		#endregion

		#region Model commands

		private DelegateCommand _sendMessageCommand;
		public DelegateCommand SendMessageCommand
		{
			get
			{
				if(_sendMessageCommand == null)
				{
					_sendMessageCommand = new DelegateCommand(() => SendMessage(), () => !IsBusy);
				}

				return _sendMessageCommand;
			}
		}

		private DelegateCommand _closeDialogCommand;
		public DelegateCommand CloseDialogCommand
		{
			get
			{
				if(_closeDialogCommand == null)
				{
					_closeDialogCommand = new DelegateCommand(CloseDialog);
				}

				return _closeDialogCommand;
			}
		}

		private DelegateCommand _voteUpCommand;
		public DelegateCommand VoteUpCommand
		{
			get
			{
				if(_voteUpCommand == null)
				{
					_voteUpCommand = new DelegateCommand(VoteUp);
				}

				return _voteUpCommand;
			}
		}

		private DelegateCommand _voteDownCommand;
		public DelegateCommand VoteDownCommand
		{
			get
			{
				if(_voteDownCommand == null)
				{
					_voteDownCommand = new DelegateCommand(VoteDown);
				}

				return _voteDownCommand;
			}
		}

		private DelegateCommand _abuseCommand;
		public DelegateCommand AbuseCommand
		{
			get
			{
				if(_abuseCommand == null)
				{
					_abuseCommand = new DelegateCommand(Abuse);
				}

				return _abuseCommand;
			}
		}

		#endregion

		protected override async Task Initialize()
		{
			List<TextMessage> messages = null;
			var result = await WrapRequest(async () => messages = await Client.GetMessagesHistoryAsync(10, 0));

			if (result)
			{
				Messages.Clear();

				foreach (var textMessage in messages)
				{
					Messages.Add(new ChatMessageWrapper(textMessage));
				}
			}
		}

		private IDisposable _eventsSubscription;

		protected override async Task OnNavigatedTo()
		{
			_eventsSubscription = Client.SubscribeToEvents(this);
			
			await WrapRequest(async () =>
			{
				var dialogState = await Client.GetDialogStateAsync();
				await HandleDialogState(dialogState);
			});

			await base.OnNavigatedTo();
		}

		protected override async Task OnNavigatedForm()
		{
			if (_eventsSubscription != null)
			{
				_eventsSubscription.Dispose();
				_eventsSubscription = null;
			}

			await base.OnNavigatedForm();
		}

		private void HideTypingMessage()
		{
			var message = Messages.FirstOrDefault(m => m.MessageType == ChatMessageType.Typing);
			if (message != null)
			{
				Messages.Remove(message);
			}
		}

		private int _typingMessageInProgress;

		private async Task SendTypingMessage()
		{
			var current = Interlocked.Exchange(ref _typingMessageInProgress, 1);
			if(current == 1)
			{
				return;
			}

			await Task.Delay(1000);

			if(Volatile.Read(ref _typingMessageInProgress) != 1)
			{
				return;
			}

			await WrapRequest(() => Client.TypingMessageAsync(new TypingMessage { Text = MessageText }), false);
			
			Volatile.Write(ref _typingMessageInProgress, 0);
		}

		private async Task SendMessage()
		{
			var message = MessageText;
			await SyncExecute(() => MessageText = "");

			Volatile.Write(ref _typingMessageInProgress, 0);

			if (string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			var messageWrapper = new ChatMessageWrapper(message);
			Messages.Add(messageWrapper);

			await WrapRequest(async () =>
			{
				var textMessage = await Client.SendTextMessage(message);
				if (textMessage != null)
				{
					await SyncExecute(() => messageWrapper.SetMassageID(textMessage.Id));
				}
			}, false);
		}
		
		private async void CloseDialog()
		{
			var result = await WrapRequest(() => Client.CloseDialogAsync());

 			if(result)
			{
				App.RootFrame.GoBack();
			}
		}

		private async void VoteUp()
		{
			var result = await WrapRequest(() => Client.VoteDialogAsync(VoteType.Good));

			if(result)
			{
				MessageBox.Show("Учтена оценка Хорошо", "оценка диалога", MessageBoxButton.OK);
			}
		}

		private async void VoteDown()
		{
			var result = await WrapRequest(() => Client.VoteDialogAsync(VoteType.Bad));

			if (result)
			{
				MessageBox.Show("Учтена оценка Плохо", "оценка диалога", MessageBoxButton.OK);
			}
		}

		private void Abuse()
		{
			App.RootFrame.Navigate(new Uri("/View/AbusePage.xaml", UriKind.Relative));
		}

		private async Task HandleDialogState(DialogState dialogState)
		{
			ConversationActive = dialogState.State == DialogStates.ConversationActive;

			if(dialogState.Employee == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Не назначен";
				return;
			}

			EmployeeAvatar = dialogState.Employee.Avatar;
			EmployeeName = dialogState.Employee.Firstname + " " + dialogState.Employee.Lastname;
		}

		#region ILiveTexEventsHandler

		void ILiveTexEventsHandler.Ban(string message)
		{
			SyncExecute(() => MessageBox.Show(message, "Вы заблокированы", MessageBoxButton.OK));
		}

		void ILiveTexEventsHandler.UpdateDialogState(DialogState dialogState)
		{
			SyncExecute(() => HandleDialogState(dialogState));
		}

		void ILiveTexEventsHandler.ReceiveFileMessage(FileMessage message)
		{
			SyncExecute(() =>
			{
				HideTypingMessage();
				Messages.Add(new ChatMessageWrapper(message));
			});
		}

		void ILiveTexEventsHandler.ReceiveTextMessage(TextMessage message)
		{
			SyncExecute(() =>
			{
				HideTypingMessage();
				Messages.Add(new ChatMessageWrapper(message));
			});
		}

		void ILiveTexEventsHandler.ConfirmTextMessage(string messageId)
		{
			SyncExecute(() =>
			{
				var message = Messages.FirstOrDefault(m => string.Equals(m.MessageID, messageId, StringComparison.OrdinalIgnoreCase));
				if (message == null)
				{
					return;
				}

				message.MarkAsReceived();
			});
		}

		void ILiveTexEventsHandler.ReceiveHoldMessage(HoldMessage message)
		{
			SyncExecute(() =>
			{
				HideTypingMessage();
				Messages.Add(new ChatMessageWrapper(message));
			});
		}

		void ILiveTexEventsHandler.ReceiveTypingMessage(TypingMessage message)
		{
			SyncExecute(() =>
			{
				HideTypingMessage();

				var wrapper = new ChatMessageWrapper(message);
				Messages.Add(wrapper);

				Task.Delay(5000).ContinueWith(t => SyncExecute(() => Messages.Remove(wrapper)));
			});
		}

		#endregion
	}
}
