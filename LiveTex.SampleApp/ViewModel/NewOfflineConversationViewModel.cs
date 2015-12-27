using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class NewOfflineConversationViewModel
		: ViewModel
	{
		protected override async Task Initialize(object parameter)
		{
			await WrapRequest(RefreshDepartments);
		}

		private string _userName;
		public string UserName 
		{
			get { return _userName; }
			set { SetValue(ref _userName, value); }
		}
		
		private string _userEmail;
		public string UserEmail
		{
			get { return _userEmail; }
			set { SetValue(ref _userEmail, value); }
		}

		private string _userPhone;
		public string UserPhone
		{
			get { return _userPhone; }
			set { SetValue(ref _userPhone, value); }
		}

		private string _message;
		public string Message
		{
			get { return _message; }
			set { SetValue(ref _message, value); }
		}

		private List<ListItemWrapper<Department>> _departments;
		public List<ListItemWrapper<Department>> Departments
		{
			get { return _departments; }
			private set { SetValue(ref _departments, value, () => Department = _departments.FirstOrDefault()); }
		}

		private ListItemWrapper<Department> _department;
		public ListItemWrapper<Department> Department
		{
			get { return _department; }
			set { SetValue(ref _department, value); }
		}

		#region Commands

		private AsyncCommand _createOfflineConversationCommand;
		public AsyncCommand CreateOfflineConversationCommand => GetAsyncCommand(ref _createOfflineConversationCommand, CreateNewOfflineConversation);

		#endregion

		private async Task RefreshDepartments()
		{
			List<Department> departments = null;

			try
			{
				departments = await Client.GetDepartmentsAsync("online");
			}
			catch (AggregateException ex)
			{
				if (!(ex.InnerException is ServiceUnavailableException))
				{
					throw ex.InnerException;
				}
			}
			catch (ServiceUnavailableException)
			{ }

			var result = new List<ListItemWrapper<Department>>
			{
				new ListItemWrapper<Department>(null, "Не выбран")
			};

			if (departments != null)
			{
				var newItems = departments
					.Where(d => !string.Equals(d.Name, "default", StringComparison.OrdinalIgnoreCase))
					.Select(d => new ListItemWrapper<Department>(d, d.Name));

                result.AddRange(newItems);
			}

			Departments = result;
		}

		private async Task CreateNewOfflineConversation()
		{
			var errors = new List<string>();
			Action<string, string> validateString = (value, field) => { if(string.IsNullOrWhiteSpace(value)) errors.Add($"Заполните поле '{field}'"); };

			validateString(UserName, "Имя");
			validateString(UserEmail, "Email");
			validateString(Message, "Сообщение");

			if(errors.Any())
			{
				MessageBox.Show(string.Join(Environment.NewLine, errors), "Ошибка", MessageBoxButton.OK);
				return;
			}

			var contacts = new List<Contact>();

			if(!string.IsNullOrWhiteSpace(UserName))
			{
				contacts.Add(new Contact {Type = ContactType.Name, Value = UserName});
			}

			if (!string.IsNullOrWhiteSpace(UserEmail))
			{
				contacts.Add(new Contact { Type = ContactType.Email, Value = UserEmail });
			}

			if (!string.IsNullOrWhiteSpace(UserPhone))
			{
				contacts.Add(new Contact { Type = ContactType.Phone, Value = UserPhone });
			}

			var departmentID = Department?.SourceObject?.Id;

			Task<string> task;
			string conversationID = null;

			if (departmentID != null)
			{
				task = Client.CreateOfflineConversationAsync(departmentID, contacts);
			}
			else
			{
				task = Client.CreateOfflineConversationAsync(contacts);
			}

			if (!await WrapRequest(async () => conversationID = await task))
			{
				return;
			}

			LiveTex.LiveTexClient.OfflineMessage = Message;
			
			App.RootFrame.Navigate(new Uri($"/View/OfflineConversationPage.xaml?id={Uri.EscapeUriString(conversationID)}", UriKind.Relative));
		}
	}
}
