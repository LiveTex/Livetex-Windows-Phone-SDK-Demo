using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class RequestDialogViewModel
		: ViewModel
	{
		protected override async Task Initialize(object parameter)
		{
			LiveTexID = LiveTexClient.LiveTexID;
			await WrapRequest(RefreshDepartments);
		}

		private string _userName;
		public string UserName 
		{
			get { return _userName; }
			set { SetValue(ref _userName, value); }
		}
		
		private string _liveTexID;
		public string LiveTexID
		{
			get { return _liveTexID; }
			private set { SetValue(ref _liveTexID, value); }
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
			private set { SetValue(ref _departments, value, () => Department = _departments.Count == 2 ? _departments[1] : _departments.First()); }
		}

		private ListItemWrapper<Department> _department;
		public ListItemWrapper<Department> Department
		{
			get { return _department; }
			set { SetValue(ref _department, value); }
		}
		
		#region Commands

		private AsyncCommand _requestDialogCommand;
		public AsyncCommand RequestDialogCommand => GetAsyncCommand(ref _requestDialogCommand, RequestDialog);

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
				result.AddRange(departments.Where(d=>!string.Equals(d.Name, "default", StringComparison.OrdinalIgnoreCase)).Select(d => new ListItemWrapper<Department>(d, d.Name)));
			}

			Departments = result;
		}

		protected override string GetErrorMessage(Exception ex)
		{
			if (string.Equals(ex.Message, "Unable to select chat member.", StringComparison.OrdinalIgnoreCase))
			{
				return "Нет ни одного оператора онлайн";
			}

			return base.GetErrorMessage(ex);
		}

		private async Task RequestDialog()
		{
			var departmentID = Department?.SourceObject?.Id;

			var errors = new List<string>();
			Action<string, string> validateString = (value, field) => { if (string.IsNullOrWhiteSpace(value)) errors.Add($"Заполните поле '{field}'"); };
			
			validateString(UserName, "Имя");
			validateString(departmentID, "Отдел");
			validateString(Message, "Сообщение");

			if (errors.Any())
			{
				MessageBox.Show(string.Join(Environment.NewLine, errors), "Ошибка", MessageBoxButton.OK);
				return;
			}

			var attributes = new DialogAttributes
			{
				Hidden = new Dictionary<string, string> { { "platform", "win" } }
			};

			if (!string.IsNullOrWhiteSpace(UserName))
			{
				await WrapRequest(() => Client.SetVisitorNameAsync(UserName));

				if(attributes.Visible == null)
				{
					attributes.Visible = new Dictionary<string, string>();
				}

				attributes.Visible["Имя пользователя"] = UserName;
			}

			if (!await WrapRequest(() => Client.RequestDialogByDepartmentAsync(departmentID, attributes)))
			{
				return;
			}

			LiveTexClient.Message = Message;

			App.RootFrame.Navigate(new Uri("/View/DialogPage.xaml", UriKind.Relative));
		}
	}
}
