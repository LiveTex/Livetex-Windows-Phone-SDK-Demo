using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class DialogViewModel
		: ViewModel, ILiveTexEventsHandler
	{
		private readonly ChatMessageCollection _messages;

		public DialogViewModel()
		{
			_messages = new ChatMessageCollection();
		}

		public ChatMessageCollection Messages
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

		private bool _isAbuseAllowed;
		public bool IsAbuseAllowed
		{
			get { return _isAbuseAllowed; }
			private set { SetValue(ref _isAbuseAllowed, value); }
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
					_voteUpCommand = new DelegateCommand(VoteUp, () => ConversationActive);
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
					_voteDownCommand = new DelegateCommand(VoteDown, () => ConversationActive);
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
					_abuseCommand = new DelegateCommand(Abuse, () => ConversationActive);
				}

				return _abuseCommand;
			}
		}

		#endregion

		private IDisposable _eventsSubscription;

		protected override async Task OnNavigatedTo()
		{
			var lastMessage = Messages.LastOrDefault(m => m.TimeStamp.HasValue && !string.IsNullOrWhiteSpace(m.MessageID));
			var newMessages = new List<TextMessage>();

			short offeset = 0;

			while (true)
			{
				List<TextMessage> messages = null;
				var result = await WrapRequest(async () => 
				{
					messages = await Client.GetMessagesHistoryAsync(10, offeset);
					offeset += 10;
				});

				if(!result)
				{
					break;
				}

				if(lastMessage == null)
				{
					newMessages.AddRange(messages);

					// Should be removed when offset bug is fixed
					break;

					continue;
				}

				foreach (var textMessage in messages)
				{
					if (string.Equals(textMessage.Id, lastMessage.MessageID, StringComparison.Ordinal))
					{
						lastMessage = null;
						break;
					}

					if(textMessage.Timestamp != null
						&& textMessage.Timestamp < lastMessage.TimeStamp)
					{
						lastMessage = null;
						break;
					}

					newMessages.Add(textMessage);
				}

				if(lastMessage == null)
				{
					break;
				}

				// Should be removed when offset bug is fixed
				break;
			}

			newMessages.Reverse();

			foreach (var textMessage in newMessages)
			{
				Messages.Add(new ChatMessageWrapper(textMessage));
			}

			_eventsSubscription = Client.SubscribeToEvents(this);

			await WrapRequest(async () =>
			{
				var dialogState = await Client.GetDialogStateAsync();
				await HandleDialogState(dialogState);

				if(dialogState.State != DialogStates.NoConversation
					&& !string.IsNullOrWhiteSpace(LiveTexClient.Message))
				{
					await SendMessage(LiveTexClient.Message);
					LiveTexClient.Message = null;
				}
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

			await SendMessage(message);
		}

		private async Task SendMessage(string message)
		{
			Volatile.Write(ref _typingMessageInProgress, 0);

			if (string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			message = message.Replace("\r", "\n");

			var messageWrapper = new ChatMessageWrapper(message);
			Messages.Add(messageWrapper);

			await WrapRequest(async () =>
			{
				var textMessage = await Client.SendTextMessageAsync(message);
				if (textMessage != null)
				{
					await SyncExecute(() => 
					{
						messageWrapper.SetMassageID(textMessage.Id); 
						Messages.UpdateMessage(messageWrapper);
					});
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
			ConversationActive = dialogState.State != DialogStates.NoConversation;
			IsAbuseAllowed = dialogState.State == DialogStates.ConversationActive;
			
			if(dialogState.Conversation == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Диалог закрыт";
				return;
			}

			if(dialogState.Employee == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Оператор не назначен";
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
				Messages.MarkAsReceived(messageId);
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
