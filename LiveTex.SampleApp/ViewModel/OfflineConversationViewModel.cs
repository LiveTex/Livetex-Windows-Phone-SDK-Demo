using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;
using Microsoft.Phone.Tasks;

namespace LiveTex.SampleApp.ViewModel
{
	public class OfflineConversationViewModel
		: ViewModel, ILiveTexEventsHandler
	{
		public OfflineConversationViewModel()
		{
			Messages = new ChatMessageCollection();
		}

		#region Model properties

		public ChatMessageCollection Messages { get; }

		public string ConversationID { get; private set; }

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
			set { SetValue(ref _messageText, value); }
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
		public DelegateCommand CloseDialogCommand => GetCommand(ref _closeDialogCommand, App.RootFrame.GoBack);

		private DelegateCommand _abuseCommand;
		public DelegateCommand AbuseCommand => GetCommand(ref _abuseCommand, Abuse, () => IsAbuseAllowed);
		
		#endregion

		private IDisposable _eventsSubscription;

		protected override async Task Initialize(object parameter)
		{
			await base.Initialize(parameter);
			ConversationID = parameter as string;
		}

		protected override async Task OnNavigatedTo()
		{
			if (string.IsNullOrEmpty(ConversationID))
			{
				MessageBox.Show("Пожалуйста повторите попытку", "Ошибка", MessageBoxButton.OK);
				App.RootFrame.GoBack();
				return;
			}

			var lastMessage = Messages.LastOrDefault(m => m.TimeStamp.HasValue && !string.IsNullOrWhiteSpace(m.MessageID));
			var newMessages = new List<OfflineMessage>();

			List<OfflineMessage> messages = null;
			var result = await WrapRequest(async () =>
			{
				messages = await Client.GetOfflineMessages(ConversationID);
			});

			if (result)
			{
				if(lastMessage == null)
				{
					newMessages.AddRange(messages);
				}
				else
				{
					foreach (var message in messages)
					{
						if(string.Equals(message.Id.ToString(), lastMessage.MessageID, StringComparison.Ordinal))
						{
							break;
						}

						if(message.Timestamp != null
						   && message.Timestamp < lastMessage.TimeStamp)
						{
							break;
						}

						newMessages.Add(message);
					}
				}
			}

			newMessages.Reverse();

			foreach (var message in newMessages)
			{
				Messages.Add(new ChatMessageWrapper(message));
			}

			_eventsSubscription = Client.SubscribeToEvents(this);

			await WrapRequest(async () =>
			{
				var conversations = await Client.GetOfflineConversations();
				var conversation = conversations?.FirstOrDefault(c => string.Equals(c.ConversationID, ConversationID, StringComparison.Ordinal));

				if(conversation == null)
				{
					return;
				}

				if(conversation.Status == OfflineConversationStatus.Open)
				{
					await SendMessage(LiveTexClient.OfflineMessage);
					LiveTexClient.OfflineMessage = null;
				}

				await HandleConversationState(conversation);
			});

			await base.OnNavigatedTo();
		}

		protected override async Task OnNavigatedForm()
		{
			_eventsSubscription?.Dispose();
			_eventsSubscription = null;

			await base.OnNavigatedForm();
		}

		private async Task SendMessage()
		{
			var message = MessageText;
			await SyncExecute(() => MessageText = "");

			await SendMessage(message);
		}

		private async Task SendMessage(string message)
		{
			if (string.IsNullOrWhiteSpace(message))
			{
				return;
			}

			message = message.Replace("\r", "\n");

			var messageWrapper = new ChatMessageWrapper(message);
			Messages.Add(messageWrapper);

			var result = await WrapRequest(async () => await Client.SendOfflineMessageAsync(ConversationID, message), false);
			if(result)
			{
				await SyncExecute(() =>
				{
					messageWrapper.SetMassageID(Guid.NewGuid().ToString());

					Messages.UpdateMessage(messageWrapper);
					Messages.MarkAsReceived(messageWrapper.MessageID);
				});
			}
			else
			{
				messageWrapper.MarkAsFailed();
			}
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
			if (e.TaskResult != TaskResult.OK)
			{
				return;
			}

			var messageWrapper = new ChatMessageWrapper("Файл: " + Path.GetFileName(e.OriginalFileName));
			Messages.Add(messageWrapper);

			await WrapRequest(async () =>
			{
				var result = await Client.SendOfflineFileAsync(ConversationID, Path.GetFileName(e.OriginalFileName), e.ChosenPhoto);
				if (result)
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

		private void Abuse()
		{
			App.RootFrame.Navigate(new Uri("/View/AbusePage.xaml", UriKind.Relative));
		}

		private async Task HandleConversationState(OfflineConversation conversation)
		{
			Guard.NotNull(conversation, nameof(conversation));

			ConversationActive = conversation.Status == OfflineConversationStatus.Open;
			IsAbuseAllowed = conversation.Status == OfflineConversationStatus.Open;

			if (conversation.Status == OfflineConversationStatus.Close)
			{
				EmployeeAvatar = null;
				EmployeeName = "Диалог закрыт";
				return;
			}

			if (conversation.Route == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Оператор не назначен";
				return;
			}

			Employee employee = null;
			try
			{
				employee = await Client.GetEmployeeAsync(conversation.Route.MemberID.ToString());
			}
			catch(Exception)
			{
				//ignore
			}
			
			if(employee == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Оператор не назначен";
				return;
			}

			EmployeeAvatar = employee.Avatar;
			EmployeeName = employee.Firstname + " " + employee.Lastname;
		}

		#region ILiveTexEventsHandler

		async void ILiveTexEventsHandler.Ban(string message)
		{
			await SyncExecute(() => MessageBox.Show(message, "Вы заблокированы", MessageBoxButton.OK));
		}

		async void ILiveTexEventsHandler.ReceiveOfflineMessage(OfflineMessage message)
		{
			await SyncExecute(() => Messages.Add(new ChatMessageWrapper(message)));
		}

		void ILiveTexEventsHandler.UpdateDialogState(DialogState dialogState)
		{
		}

		void ILiveTexEventsHandler.ReceiveFileMessage(FileMessage message)
		{
		}

		void ILiveTexEventsHandler.ReceiveTextMessage(TextMessage message)
		{
		}

		void ILiveTexEventsHandler.ConfirmTextMessage(string messageId)
		{
		}

		void ILiveTexEventsHandler.ReceiveHoldMessage(HoldMessage message)
		{
		}

		void ILiveTexEventsHandler.ReceiveTypingMessage(TypingMessage message)
		{
		}

		#endregion
	}
}
