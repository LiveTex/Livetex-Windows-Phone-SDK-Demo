using System.Threading.Tasks;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class AbuseViewModel
		: ViewModel
	{
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
			set
			{
				if (SetValue(ref _message, value))
				{
					AbuseCommand.RiseCanExecuteChanged();
				}
			}
		}

		#endregion

		private DelegateCommand _abuseCommand;
		public DelegateCommand AbuseCommand
		{
			get
			{
				if(_abuseCommand == null)
				{
					_abuseCommand = new DelegateCommand(Abuse, IsAbuseAllowed);
				}

				return _abuseCommand;
			}
		}

		private bool IsAbuseAllowed()
		{
			return !string.IsNullOrWhiteSpace(Message);
		}

		private async void Abuse()
		{
			var result = await WrapRequest(() => Client.AbuseAsync(new Abuse { Contact = "", Message = Message }));
			if(result)
			{
				App.RootFrame.GoBack();
			}
		}

		protected override async Task OnNavigatedTo()
		{
			await WrapRequest(async () =>
			{
				var dialogState = await Client.GetDialogStateAsync();
				await HandleDialogState(dialogState);
			});

			await base.OnNavigatedTo();
		}

		private async Task HandleDialogState(DialogState dialogState)
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
	}
}
