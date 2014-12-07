using System;
using System.Windows.Input;

namespace LiveTex.SampleApp.ViewModel
{
	public class DelegateCommand
		: ICommand
	{
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		public DelegateCommand(Action execute, Func<bool> canExecute = null)
			: this(o => execute(), o => canExecute == null || canExecute())
		{
		}

		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			_execute = execute;
			_canExecute = canExecute;
		}

		public bool CanExecute(object parameter)
		{
			return _canExecute == null
				|| _canExecute(parameter);
		}

		public void Execute(object parameter)
		{
			if(!CanExecute(parameter))
			{
				return;
			}

			if(_execute != null)
			{
				_execute(parameter);
			}
		}

		public event EventHandler CanExecuteChanged;

		public void RiseCanExecuteChanged()
		{
			var handler = CanExecuteChanged;
			if(handler != null)
			{
				handler(this, EventArgs.Empty);
			}
		}
	}
}
