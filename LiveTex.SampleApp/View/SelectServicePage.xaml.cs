using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;

namespace LiveTex.SampleApp
{
	public partial class SelectServicePage
		: PhoneApplicationPage
	{
		public SelectServicePage()
		{
			InitializeComponent();

			DataContext = new SelectServiceViewModel();
		}

		private SelectServiceViewModel ViewModel => (SelectServiceViewModel)DataContext;

		protected override async void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);
			await ViewModel.NavigatedTo();
		}

		protected override async void OnNavigatedFrom(NavigationEventArgs e)
		{
			base.OnNavigatedFrom(e);
			await ViewModel.NavigatedFrom();
		}
	}
}