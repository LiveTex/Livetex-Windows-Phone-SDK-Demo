using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SDK;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.ViewModel
{
	public class ViewModel
		: INotifyPropertyChanged
	{
		private readonly SynchronizationContext _syncContext;

		public ViewModel()
		{
			_syncContext = SynchronizationContext.Current;
		}

		private bool _isInitialized;
		public async Task NavigatedTo(object parameter = null)
		{
			if(!_isInitialized)
			{
				_isInitialized = true;

				if(InitializeClient)
				{
					await WrapRequest(async () => Client = await LiveTexClient.GetClient());
				}

				await Initialize(parameter);
			}

			await OnNavigatedTo();
		}

		protected ILiveTexClient Client { get; private set; }
		protected virtual bool InitializeClient => true;

		public async Task NavigatedFrom()
		{
			await OnNavigatedForm();
		}

		protected virtual Task OnNavigatedTo()
		{
			return Task.FromResult(true);
		}

		protected virtual Task OnNavigatedForm()
		{
			return Task.FromResult(true);
		}

		protected virtual Task Initialize(object parameter)
		{
			return Task.FromResult(true);
		}

		#region Busy

		private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			set
			{
				if (SetValue(ref _isBusy, value))
				{
					UpdateCommandsState();
				}
			}
		}

		private int _busyCounter;

		protected IDisposable BeginBusy()
		{
			var value = Interlocked.Increment(ref _busyCounter);

			if (value == 1)
			{
				IsBusy = true;
			}

			return new DisposeAction(() =>
			{
				if (Interlocked.Decrement(ref _busyCounter) <= 0)
				{
					IsBusy = false;
				}
			});
		}

		protected virtual void OnIsBusyChanged()
		{
			UpdateCommandsState();
		}

		#endregion

		#region Helpers

		protected async Task<bool> WrapRequest(Func<Task> action, bool markBusy = true)
		{
			using(markBusy ? BeginBusy() : null)
			{
				try
				{
					await action();
					return true;
				}
				catch (Exception ex)
				{
					var message = GetErrorMessage(ex.Unwrap());
					MessageBox.Show(message, "Ошибка", MessageBoxButton.OK);

					return false;
				}
			}
		}

		protected virtual string GetErrorMessage(Exception ex)
		{
			return ex is TransportException
				   ? "Ошибка сетевого соединения"
				   : ex.Message;
		}

		protected async Task SyncExecute(Action action)
		{
			if(action == null)
			{
				return;
			}

			var tcs = new TaskCompletionSource<bool>();

			_syncContext.Post(o => 
				{
					try
					{
						action();
						tcs.SetResult(true);
					}
					catch(Exception ex)
					{
						tcs.SetException(ex);
					}
				}, null);

			await tcs.Task;
		}

		#endregion
		
		#region OnPropertyChanged

		protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
		{
			return SetValue(ref field, value, null, propertyName);
		}

		protected bool SetValue<T>(ref T field, T value, Action successHandler, [CallerMemberName] string propertyName = null)
		{
			if (EqualityComparer<T>.Default.Equals(field, value))
			{
				return false;
			}

			field = value;
			OnPropertyChanged(propertyName);
			successHandler?.Invoke();

			return true;
		}

		public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			_syncContext.Post(o => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)), null);
		}

		#endregion

		#region Commands management

		private readonly ReaderWriterLockSlim _busyRelatedCommandsLock = new ReaderWriterLockSlim();
		private readonly List<WeakReference<ICommandEx>> _busyRelatedCommands = new List<WeakReference<ICommandEx>>();

		private void UpdateCommandsState()
		{
			using (_busyRelatedCommandsLock.UpgradeableReadLock())
			{
				for (var index = _busyRelatedCommands.Count - 1; index >= 0; index--)
				{
					ICommandEx command;
					if (!_busyRelatedCommands[index].TryGetTarget(out command))
					{
						using (_busyRelatedCommandsLock.WriteLock())
						{
							_busyRelatedCommands.RemoveAt(index);
						}

						return;
					}

					command.RiseCanExecuteChanged();
				}
			}
		}

		protected TCommand GetCommand<TCommand>(ref TCommand command, Action execute, Func<bool> canExecute = null, bool busyRelated = true)
			where TCommand : class, ICommand
		{
			if (command != null)
			{
				return command;
			}

			if (!typeof(TCommand).GetTypeInfo().IsAssignableFrom(typeof(DelegateCommand).GetTypeInfo()))
			{
				throw new ArgumentException("Unsupported backfield type");
			}

			var fullCanExecute = canExecute;

			if (busyRelated)
			{
				fullCanExecute = () => !IsBusy && canExecute.With(h => h(), true);
			}

			command = new DelegateCommand(execute, fullCanExecute) as TCommand;

			if (busyRelated)
			{
				using (_busyRelatedCommandsLock.WriteLock())
				{
					_busyRelatedCommands.Add(new WeakReference<ICommandEx>((ICommandEx)command));
				}
			}

			return command;
		}

		protected TCommand GetCommand<TCommand>(ref TCommand command, Action<object> execute, Func<object, bool> canExecute = null, bool busyRelated = true)
			where TCommand : class, ICommand
		{
			if (command != null)
			{
				return command;
			}

			if (!typeof(TCommand).GetTypeInfo().IsAssignableFrom(typeof(DelegateCommand).GetTypeInfo()))
			{
				throw new ArgumentException("Unsupported backfield type");
			}

			var fullCanExecute = canExecute;

			if (busyRelated)
			{
				fullCanExecute = o => !IsBusy && canExecute.With(h => h(o), true);
			}

			command = new DelegateCommand(execute, fullCanExecute) as TCommand;

			if (busyRelated)
			{
				using (_busyRelatedCommandsLock.WriteLock())
				{
					_busyRelatedCommands.Add(new WeakReference<ICommandEx>((ICommandEx)command));
				}
			}

			return command;
		}

		protected TCommand GetAsyncCommand<TCommand>(ref TCommand command, Func<Task> execute, Func<bool> canExecute = null, bool busyRelated = true)
			where TCommand : class, ICommand
		{
			if (command != null)
			{
				return command;
			}

			if (!typeof(TCommand).GetTypeInfo().IsAssignableFrom(typeof(AsyncCommand).GetTypeInfo()))
			{
				throw new ArgumentException("Unsupported backfield type");
			}

			var fullCanExecute = canExecute;

			if (busyRelated)
			{
				fullCanExecute = () => !IsBusy && canExecute.With(h => h(), true);
			}

			command = new AsyncCommand(execute, fullCanExecute) as TCommand;

			if (busyRelated)
			{
				using (_busyRelatedCommandsLock.WriteLock())
				{
					_busyRelatedCommands.Add(new WeakReference<ICommandEx>((ICommandEx)command));
				}
			}

			return command;
		}

		protected TCommand GetAsyncCommand<TCommand>(ref TCommand command, Func<object, Task> execute, Func<object, bool> canExecute = null, bool busyRelated = true)
			where TCommand : class, ICommand
		{
			if (command != null)
			{
				return command;
			}

			if (!typeof(TCommand).GetTypeInfo().IsAssignableFrom(typeof(AsyncCommand).GetTypeInfo()))
			{
				throw new ArgumentException("Unsupported backfield type");
			}

			var fullCanExecute = canExecute;

			if (busyRelated)
			{
				fullCanExecute = o => !IsBusy && canExecute.With(h => h(o), true);
			}

			command = new AsyncCommand(execute, fullCanExecute) as TCommand;

			if (busyRelated)
			{
				using (_busyRelatedCommandsLock.WriteLock())
				{
					_busyRelatedCommands.Add(new WeakReference<ICommandEx>((ICommandEx)command));
				}
			}

			return command;
		}

		#endregion
	}
}
