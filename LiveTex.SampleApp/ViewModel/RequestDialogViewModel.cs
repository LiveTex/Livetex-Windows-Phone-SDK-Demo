using System;
using System.Collections.Generic;
using System.Globalization;
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
			await WrapRequest(() => Task.WhenAll(RefreshDepartments(), RefreshEmployees(null)));
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
			private set { SetValue(ref _departments, value, () => Department = _departments.First()); }
		}

		private ListItemWrapper<Department> _department;
		public ListItemWrapper<Department> Department
		{
			get { return _department; }
			set
			{
				SetValue(ref _department, value);
				IsEmployeeSelectionAllowed = _department?.SourceObject == null;
			}
		}

		private bool _isDepartmentSelectionAllowed;
		public bool IsDepartmentSelectionAllowed
		{
			get { return _isDepartmentSelectionAllowed; }
			private set { SetValue(ref _isDepartmentSelectionAllowed, value); }
		}

		private List<ListItemWrapper<Employee>> _employees;
		public List<ListItemWrapper<Employee>> Employees
		{
			get { return _employees; }
			private set { SetValue(ref _employees, value, () => Employee = _employees.First()); }
		}

		private ListItemWrapper<Employee> _employee;
		public ListItemWrapper<Employee> Employee
		{
			get { return _employee; }
			set
			{
				SetValue(ref _employee, value);
				IsDepartmentSelectionAllowed = _employee?.SourceObject == null;
			}
		}

		private bool _isEmployeeSelectionAllowed;
		public bool IsEmployeeSelectionAllowed
		{
			get { return _isEmployeeSelectionAllowed; }
			private set { SetValue(ref _isEmployeeSelectionAllowed, value); }
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

		private async Task RefreshEmployees(string departmentId)
		{
			List<Employee> emploees = null;

			try
			{
				emploees = string.IsNullOrWhiteSpace(departmentId)
					? await Client.GetEmployeesAsync("online")
					: await Client.GetDepartmentEmployeesAsync(departmentId);
			}
			catch(AggregateException ex) when (ex.InnerException is ServiceUnavailableException)
			{
			}
			catch (ServiceUnavailableException)
			{
			}
			catch (AggregateException ex)
			{
				throw ex.Unwrap();
			}

			var result = new List<ListItemWrapper<Employee>>
			{
				new ListItemWrapper<Employee>(null, "Не выбран")
			};

			if (emploees != null)
			{
				result.AddRange(emploees.Select(e => new ListItemWrapper<Employee>(e, e.Firstname + " " + e.Lastname)));
			}

			Employees = result;
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
			var attributes = new DialogAttributes
			{
				Hidden = new Dictionary<string, string> { { "platform", "win"} }
			};

			if(string.IsNullOrWhiteSpace(Message))
			{
				MessageBox.Show("Заполните поле 'Сообщение'", "Ошибка", MessageBoxButton.OK);
				return;
			}

			if (!string.IsNullOrWhiteSpace(UserName))
			{
				await WrapRequest(() => Client.SetVisitorNameAsync(UserName));

				if(attributes.Visible == null)
				{
					attributes.Visible = new Dictionary<string, string>();
				}

				attributes.Visible["Имя пользователя"] = UserName;
			}

			var departmentID = Department?.SourceObject?.Id;
			var employeeID = Employee?.SourceObject?.EmployeeId;

			Task task;

			if (employeeID != null)
			{
				task = Client.RequestDialogByEmployeeAsync(employeeID, attributes);
			}
			else if (departmentID != null)
			{
				task = Client.RequestDialogByDepartmentAsync(departmentID, attributes);
			}
			else
			{
				task = Client.RequestDialogAsync(attributes);
			}

			if (!await WrapRequest(() => task))
			{
				return;
			}

			LiveTex.LiveTexClient.Message = Message;

			App.RootFrame.Navigate(new Uri("/View/DialogPage.xaml", UriKind.Relative));
		}
	}
}
