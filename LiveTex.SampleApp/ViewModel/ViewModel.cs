using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using LiveTex.SampleApp.LiveTex;
using LiveTex.SDK;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.ViewModel
{
	public class ViewModel
		: INotifyPropertyChanged
	{
		private readonly SynchronizationContext _syncContext;
		private readonly ILiveTexClient _client;

		public ViewModel()
		{
			_syncContext = SynchronizationContext.Current;
			_client = LiveTexClient.Client;
		}

		protected ILiveTexClient Client
		{
			get { return _client; }
		}

		private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			set { SetValue(ref _isBusy, value); }
		}

		private bool _isInitialized;
		public async Task NavigatedTo()
		{
			if(!_isInitialized)
			{
				_isInitialized = true;
				await Initialize();
			}

			await OnNavigatedTo();
		}

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

		protected virtual Task Initialize()
		{
			return Task.FromResult(true);
		}
		
		#region Helpers

		protected async Task<bool> WrapRequest(Func<Task> action, bool markBusy = true)
		{
			if (markBusy)
			{
				IsBusy = true;
			}

			try
			{
				await action();

				return true;
			}
			catch (Exception ex)
			{
				var aggregate = ex as AggregateException;
				if (aggregate != null)
				{
					ex = aggregate.InnerException;
				}

				var message = ex is TransportException
					? "Ошибка сетевого соединения"
					: ex.Message;

				MessageBox.Show(message, "Ошибка", MessageBoxButton.OK);

				return false;
			}
			finally
			{
				if (markBusy)
				{
					IsBusy = false;
				}
			}
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
			if (Equals(field, value))
			{
				return false;
			}

			field = value;
			OnPropertyChanged(propertyName);

			return true;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			_syncContext.Post(o =>
			{
				var handler = PropertyChanged;
				if (handler != null)
				{
					handler(this, new PropertyChangedEventArgs(propertyName));
				}
			}, null);
		}

		#endregion
	}
}
