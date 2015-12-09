using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Windows.Storage.Pickers;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;
using Microsoft.Phone.Tasks;

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
			set { SetValue(ref _messageText, value, () => SendTypingMessage().LogAsyncError()); }
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

		private AsyncCommand _sendMessageCommand;
		public AsyncCommand SendMessageCommand => GetAsyncCommand(ref _sendMessageCommand, SendMessage);

		private DelegateCommand _sendFileCommand;
		public DelegateCommand SendFileCommand => GetCommand(ref _sendFileCommand, SendFile);

		private DelegateCommand _closeDialogCommand;
		public DelegateCommand CloseDialogCommand => GetCommand(ref _closeDialogCommand, CloseDialog);

		private AsyncCommand _voteUpCommand;
		public AsyncCommand VoteUpCommand => GetAsyncCommand(ref _voteUpCommand, VoteUp, () => IsAbuseAllowed);

		private AsyncCommand _voteDownCommand;
		public AsyncCommand VoteDownCommand => GetAsyncCommand(ref _voteDownCommand, VoteDown, () => IsAbuseAllowed);

		private DelegateCommand _abuseCommand;
		public DelegateCommand AbuseCommand => GetCommand(ref _abuseCommand, Abuse, () => IsAbuseAllowed);

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

				if(messages == null
				   || messages.Count == 0)
				{
					break;
				}

				if(lastMessage == null)
				{
					newMessages.AddRange(messages);
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
				HandleDialogState(dialogState);

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
			if(_typingMessageInProgress == 1)
			{
				return;
			}

			_typingMessageInProgress = 1;

			await Task.Delay(1000);

			if(_typingMessageInProgress != 1)
			{
				return;
			}

			await WrapRequest(() => Client.TypingMessageAsync(new TypingMessage { Text = MessageText }), false);
			
			_typingMessageInProgress = 0;
		}

		private async Task SendMessage()
		{
			var message = MessageText;
			await SyncExecute(() => MessageText = "");

			await SendMessage(message);
		}

		private async Task SendMessage(string message)
		{
			_typingMessageInProgress = 0;

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

		private void SendFile()
		{
			var choser = new PhotoChooserTask();
			choser.Completed += ChoserCompleted;
			choser.ShowCamera = true;
			choser.Show();
		}

		private async void ChoserCompleted(object sender, PhotoResult e)
		{
			if(e.TaskResult != TaskResult.OK)
			{
				return;
			}

			var messageWrapper = new ChatMessageWrapper("Файл: " + Path.GetFileName(e.OriginalFileName));
			Messages.Add(messageWrapper);

			await WrapRequest(async () =>
			{
				var result = await Client.SendFileAsync(e.OriginalFileName, e.ChosenPhoto);
				if(result)
				{
					await SyncExecute(() =>
					{
						messageWrapper.SetMassageID(Guid.NewGuid().ToString());
						Messages.UpdateMessage(messageWrapper);
						Messages.MarkAsReceived(messageWrapper.MessageID);
					});
				}
			});
		}

		private async void CloseDialog()
		{
			var result = await WrapRequest(() => Client.CloseDialogAsync());

 			if(result)
			{
				App.RootFrame.GoBack();
			}
		}

		private async Task VoteUp()
		{
			var result = await WrapRequest(() => Client.VoteDialogAsync(VoteType.Good));

			if(result)
			{
				MessageBox.Show("Учтена оценка Хорошо", "оценка диалога", MessageBoxButton.OK);
			}
		}

		private async Task VoteDown()
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

		private void HandleDialogState(DialogState dialogState)
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

		async void ILiveTexEventsHandler.Ban(string message)
		{
			await SyncExecute(() => MessageBox.Show(message, "Вы заблокированы", MessageBoxButton.OK));
		}

		async void ILiveTexEventsHandler.UpdateDialogState(DialogState dialogState)
		{
			await SyncExecute(() => HandleDialogState(dialogState));
		}

		async void ILiveTexEventsHandler.ReceiveFileMessage(FileMessage message)
		{
			await SyncExecute(() =>
			{
				HideTypingMessage();
				Messages.Add(new ChatMessageWrapper(message));
			});
		}

		async void ILiveTexEventsHandler.ReceiveTextMessage(TextMessage message)
		{
			await SyncExecute(() =>
			{
				HideTypingMessage();
				Messages.Add(new ChatMessageWrapper(message));
			});
		}

		async void ILiveTexEventsHandler.ConfirmTextMessage(string messageId)
		{
			await SyncExecute(() =>
			{
				Messages.MarkAsReceived(messageId);
			});
		}

		async void ILiveTexEventsHandler.ReceiveHoldMessage(HoldMessage message)
		{
			await SyncExecute(() =>
			{
				HideTypingMessage();
				Messages.Add(new ChatMessageWrapper(message));
			});
		}

		async void ILiveTexEventsHandler.ReceiveTypingMessage(TypingMessage message)
		{
			await SyncExecute(() =>
			{
				HideTypingMessage();

				var wrapper = new ChatMessageWrapper(message);
				Messages.Add(wrapper);

				Task.Delay(5000).ContinueWith(t => SyncExecute(() => Messages.Remove(wrapper)));
			});
		}

		void ILiveTexEventsHandler.ReceiveOfflineMessage(OfflineMessage message)
		{
		}

		#endregion
	}
}
