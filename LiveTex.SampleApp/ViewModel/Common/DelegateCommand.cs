using System;

namespace LiveTex.SampleApp.ViewModel
{
	public class DelegateCommand
		: CommandBase
	{
		private readonly Action<object> _execute;
		private readonly Func<object, bool> _canExecute;

		public DelegateCommand(Action execute, Func<bool> canExecute = null)
		{
			Guard.NotNull(execute, "execute");

			_execute = o => execute();
			if (canExecute != null)
			{
				_canExecute = o => canExecute();
			}
		}

		public DelegateCommand(Action<object> execute, Func<object, bool> canExecute = null)
		{
			Guard.NotNull(execute);

			_execute = execute;
			_canExecute = canExecute;
		}

		public override bool CanExecute(object parameter)
		{
			if (_canExecute == null)
			{
				return true;
			}

			return _canExecute(parameter);
		}

		public override void Execute(object parameter)
		{
			_execute(parameter);
		}

		protected override bool IsCanExecuteSupported => _canExecute != null;
	}
}
