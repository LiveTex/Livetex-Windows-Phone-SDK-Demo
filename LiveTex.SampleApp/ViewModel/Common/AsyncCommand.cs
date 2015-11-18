using System;
using System.Threading;
using System.Threading.Tasks;

namespace LiveTex.SampleApp.ViewModel
{
	public class AsyncCommand
		: CommandBase
	{
		private readonly Func<object, Task> _execute;
		private readonly Func<object, bool> _canExecute;

		public AsyncCommand(Func<Task> execute, Func<bool> canExecute = null)
		{
			Guard.NotNull(execute, nameof(execute));

			_execute = o => execute();

			if (canExecute != null)
			{
				_canExecute = o => canExecute();
			}
		}

		public AsyncCommand(Func<object, Task> execute, Func<object, bool> canExecute = null)
		{
			Guard.NotNull(execute, nameof(execute));

			_execute = execute;
			_canExecute = canExecute;
		}

		private int _executing;

		public override bool CanExecute(object parameter)
		{
			if (_executing != 0)
			{
				return false;
			}

			return _canExecute == null
			       || _canExecute(parameter);
		}

		public override void Execute(object parameter)
		{
			ExecuteAsync(parameter).LogAsyncError();
		}

		private async Task ExecuteAsync(object parameter)
		{
			var current = Interlocked.Exchange(ref _executing, 1);
			if (current == 1)
			{
				return;
			}

			RiseCanExecuteChanged();

			try
			{
				if (_canExecute != null
				    && !_canExecute(parameter))
				{
					return;
				}

				await _execute(parameter);
			}
			finally
			{
				Interlocked.Exchange(ref _executing, 0);
				RiseCanExecuteChanged();
			}
		}

		protected override bool IsCanExecuteSupported => _canExecute != null;
	}
}