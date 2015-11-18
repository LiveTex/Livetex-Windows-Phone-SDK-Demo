using System.Windows.Input;

namespace LiveTex.SampleApp.ViewModel
{
	public interface ICommandEx
		: ICommand
	{
		void RiseCanExecuteChanged();
	}
}