using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK.Client;
using LiveTex.SDK.Sample;

namespace LiveTex.SampleApp.ViewModel
{
	public class RequestDialogViewModel
		: ViewModel
	{
		protected override async Task Initialize()
		{
			await WrapRequest(() => Task.WhenAll(RefreshDepartments(), RefreshEmployees(null)));
		}

		private string _userName;
		public string UserName 
		{
			get { return _userName; }
			set { SetValue(ref _userName, value); }
		}
		
		private string _userAge;
		public string UserAge
		{
			get { return _userAge; }
			set { SetValue(ref _userAge, value); }
		}

		private List<ListItemWrapper<Department>> _departments;
		public List<ListItemWrapper<Department>> Departments
		{
			get { return _departments; }
			private set { SetValue(ref _departments, value); }
		}

		private ListItemWrapper<Department> _department;
		public ListItemWrapper<Department> Department
		{
			get { return _department; }
			set { SetDepartment(value); }
		}

		private List<ListItemWrapper<Employee>> _employees;
		public List<ListItemWrapper<Employee>> Employees
		{
			get { return _employees; }
			private set
			{
				if(SetValue(ref _employees, value))
				{
					Employee = _employees.First();
				}
			}
		}

		private ListItemWrapper<Employee> _employee;
		public ListItemWrapper<Employee> Employee
		{
			get { return _employee; }
			set { SetValue(ref _employee, value); }
		}

		#region Commands

		private DelegateCommand _requestDialogCommand;
 		public DelegateCommand RequestDialogCommand
		{
			get
			{
				if(_requestDialogCommand == null)
				{
					_requestDialogCommand = new DelegateCommand(() => RequestDialog());
				}

				return _requestDialogCommand;
			}
		}

		#endregion

		private async Task RefreshDepartments()
		{
			List<Department> departments = null;

			try
			{
				departments = await Client.GetDepartmentsAsync("1");
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

			var result = new List<ListItemWrapper<Department>>()
			{
				new ListItemWrapper<Department>(null, "Не выбран")
			};

			if (departments != null)
			{
				result.AddRange(departments.Select(d => new ListItemWrapper<Department>(d, d.Name)));
			}

			Departments = result;
		}

		private async Task RefreshEmployees(string departmentId)
		{
			List<Employee> emploees = null;

			try
			{
				emploees = string.IsNullOrWhiteSpace(departmentId)
					? await Client.GetEmployeesAsync("1")
					: await Client.GetDepartmentEmployeesAsync(departmentId);
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
		
		private async Task SetDepartment(ListItemWrapper<Department> value)
		{
			if (SetValue(ref _department, value, "Department"))
			{
				await WrapRequest(async () =>
				{
					var newDepartmentID = value == null || value.SourceObject == null
						? null
						: value.SourceObject.Id;

					await RefreshEmployees(newDepartmentID);
				});
			}
		}

		private async Task RequestDialog()
		{
			var attributes = new DialogAttributes
			{
				Hidden = new Dictionary<string, string> { { "platform", "win"} }
			};

			if (!string.IsNullOrWhiteSpace(UserAge))
			{
				int age;
				if (!int.TryParse(UserAge, out age)
					|| age < 10
					|| age > 99)
				{
					MessageBox.Show("Возвраст должен быть числом от 10 до 99", "Ошибка", MessageBoxButton.OK);
					return;
				}

				attributes.Visible = new Dictionary<string, string> { { "Возраст", age.ToString(CultureInfo.InvariantCulture) } };
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

			var departmentID = Department == null || Department.SourceObject == null
				? null
				: Department.SourceObject.Id;

			var employeeID = Employee == null || Employee.SourceObject == null
				? null
				: Employee.SourceObject.EmployeeId;

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

			App.RootFrame.Navigate(new Uri("/View/DialogPage.xaml", UriKind.Relative));
		}
	}
}
