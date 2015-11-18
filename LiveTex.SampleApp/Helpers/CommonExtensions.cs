using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using LiveTex.SampleApp;

namespace System
{
	public static class CommonExtensions
	{
		public static void ExecuteSafe(this ICommand command, object parameter = null)
		{
			if(command == null)
			{
				return;
			}

			if(command.CanExecute(parameter))
			{
				command.Execute(parameter);
			}
		}

		public static TOut With<TIn, TOut>(this TIn obj, Func<TIn, TOut> func, TOut defaultValue = default(TOut))
		   where TIn : class
		{
			Guard.NotNull(func, nameof(func));

			return obj != null
				? func(obj)
				: defaultValue;
		}

		public static Exception Unwrap(this Exception ex)
		{
			var aggregateException = ex as AggregateException;
			if(aggregateException != null)
			{
				return aggregateException.InnerException.Unwrap();
			}

			return ex;
		}

		public static void LogAsyncError(this Task task)
		{
			task?.ContinueWith(t => Debug.WriteLine(t.Exception.ToString()), TaskContinuationOptions.OnlyOnFaulted);
		}

		public static IDisposable ReadLock(this ReaderWriterLockSlim readerWriterLock)
		{
			Guard.NotNull(readerWriterLock);

			readerWriterLock.EnterReadLock();

			return new DisposeAction(readerWriterLock.ExitReadLock);
		}

		public static IDisposable UpgradeableReadLock(this ReaderWriterLockSlim readerWriterLock)
		{
			Guard.NotNull(readerWriterLock);

			readerWriterLock.EnterUpgradeableReadLock();

			return new DisposeAction(readerWriterLock.ExitUpgradeableReadLock);
		}

		public static IDisposable WriteLock(this ReaderWriterLockSlim readerWriterLock)
		{
			Guard.NotNull(readerWriterLock);

			readerWriterLock.EnterWriteLock();

			return new DisposeAction(readerWriterLock.ExitWriteLock);
		}

		public static IDisposable WriteLockIfRequired(this ReaderWriterLockSlim readerWriterLock, ref IDisposable release)
		{
			Guard.NotNull(readerWriterLock);

			if (release != null)
			{
				if (!readerWriterLock.IsWriteLockHeld)
				{
					throw new Exception("Release handler is not null but the write lock did not held");
				}

				return release;
			}

			release = readerWriterLock.WriteLock();
			return release;
		}
	}
}
