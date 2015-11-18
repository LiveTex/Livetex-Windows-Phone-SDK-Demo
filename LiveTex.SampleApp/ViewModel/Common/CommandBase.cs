using System;
using System.Threading;

namespace LiveTex.SampleApp.ViewModel
{
	public abstract class CommandBase
		: ICommandEx
	{
		public abstract bool CanExecute(object parameter);
		public abstract void Execute(object parameter);

		private EventHandler _canExecuteChanged;
		public event EventHandler CanExecuteChanged
		{
			add
			{
				if (!IsCanExecuteSupported)
				{
					return;
				}

				EventHandler comparand;
				var eventHandler = _canExecuteChanged;

				do
				{
					comparand = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref _canExecuteChanged, (EventHandler)Delegate.Combine(comparand, value), comparand);
				}
				while (eventHandler != comparand);
			}
			remove
			{
				if (!IsCanExecuteSupported)
				{
					return;
				}

				EventHandler comparand;
				var eventHandler = _canExecuteChanged;

				do
				{
					comparand = eventHandler;
					eventHandler = Interlocked.CompareExchange(ref _canExecuteChanged, (EventHandler)Delegate.Remove(comparand, value), comparand);
				}
				while (eventHandler != comparand);
			}
		}

		public void RiseCanExecuteChanged()
		{
			_canExecuteChanged?.Invoke(this, EventArgs.Empty);
		}

		protected abstract bool IsCanExecuteSupported { get; }
	}
}