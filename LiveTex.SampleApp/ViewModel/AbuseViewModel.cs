using System;
using System.Threading.Tasks;
using LiveTex.SDK;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class AbuseViewModel
		: ViewModel, ILiveTexEventsHandler
	{
		private IDisposable _eventsSubscription;

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

		private bool _conversationActive;
		public bool ConversationActive
		{
			get { return _conversationActive; }
			private set { SetValue(ref _conversationActive, value); }
		}

		private string _message;
		public string Message
		{
			get { return _message; }
			set { SetValue(ref _message, value, AbuseCommand.RiseCanExecuteChanged); }
		}

		private string _contact;
		public string Contact
		{
			get { return _contact; }
			set { SetValue(ref _contact, value, AbuseCommand.RiseCanExecuteChanged); }
		}

		#endregion

		private AsyncCommand _abuseCommand;
		public AsyncCommand AbuseCommand => GetAsyncCommand(ref _abuseCommand, Abuse, IsAbuseAllowed);
		
		private bool IsAbuseAllowed()
		{
			return ConversationActive
				&& !string.IsNullOrWhiteSpace(Message)
				&& !string.IsNullOrWhiteSpace(Contact);
		}

		private async Task Abuse()
		{
			if(!IsAbuseAllowed())
			{
				return;
			}

			using(BeginBusy())
			{
				var result = await WrapRequest(() => Client.AbuseAsync(new Abuse {Contact = Contact, Message = Message}));
				if(result)
				{
					App.RootFrame.GoBack();
				}
			}
		}

		protected override async Task OnNavigatedTo()
		{
			using(BeginBusy())
			{
				await WrapRequest(async () =>
				{
					var dialogState = await Client.GetDialogStateAsync();
					HandleDialogState(dialogState);
				});

				await base.OnNavigatedTo();

				_eventsSubscription = Client.SubscribeToEvents(this);
			}
		}

		protected override Task OnNavigatedForm()
		{
			_eventsSubscription?.Dispose();
			_eventsSubscription = null;
			
			return base.OnNavigatedForm();
		}

		private void HandleDialogState(DialogState dialogState)
		{
			ConversationActive = dialogState.State == DialogStates.ConversationActive;
			
			if (dialogState.Conversation == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Диалог закрыт";
				return;
			}

			if (dialogState.Employee == null)
			{
				EmployeeAvatar = null;
				EmployeeName = "Не назначен";
				return;
			}

			EmployeeAvatar = dialogState.Employee.Avatar;
			EmployeeName = dialogState.Employee.Firstname + " " + dialogState.Employee.Lastname;
		}

		void ILiveTexEventsHandler.Ban(string message)
		{
		}

		void ILiveTexEventsHandler.UpdateDialogState(DialogState dialogState)
		{
			HandleDialogState(dialogState);
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

		public void ReceiveOfflineMessage(OfflineMessage message)
		{
		}
	}
}
