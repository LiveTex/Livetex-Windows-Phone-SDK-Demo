using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;

namespace LiveTex.SampleApp
{
	public partial class RequestDialogPage
		: PhoneApplicationPage
	{
		public RequestDialogPage()
		{
			InitializeComponent();

			DataContext = new RequestDialogViewModel();
		}

		private RequestDialogViewModel ViewModel
		{
			get { return (RequestDialogViewModel)DataContext; }
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.NavigatedTo();
		}

		protected override void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);

			if(e.NavigationMode == NavigationMode.New
				&& e.IsNavigationInitiator)
			{
				NavigationService.RemoveBackEntry();
			}
		}
	}
}