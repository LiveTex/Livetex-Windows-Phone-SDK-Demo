using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class NewOfflineConversationViewModel
		: ViewModel
	{
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

		#region Commands

		private AsyncCommand _createOfflineConversationCommand;
		public AsyncCommand CreateOfflineConversationCommand => GetAsyncCommand(ref _createOfflineConversationCommand, CreateNewOfflineConversation);

		#endregion

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

			string conversationID = null;

			if (!await WrapRequest(async () => conversationID = await Client.CreateOfflineConversationAsync(contacts)))
			{
				return;
			}

			LiveTex.LiveTexClient.OfflineMessage = Message;
			
			App.RootFrame.Navigate(new Uri($"/View/OfflineConversationPage.xaml?id={Uri.EscapeUriString(conversationID)}", UriKind.Relative));
		}
	}
}
