using System;

namespace LiveTex.SampleApp
{
	internal class DisposeAction
		: IDisposable
	{
		private Action _action;

		public DisposeAction(Action action)
		{
			_action = action;
		}

		public void Dispose()
		{
			if (_action == null)
			{
				return; 
			}

			_action();
			_action = null;
		}
	}
}
