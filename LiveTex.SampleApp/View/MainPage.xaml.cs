using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;

namespace LiveTex.SampleApp
{
	public partial class MainPage
		: PhoneApplicationPage
	{
		public MainPage()
		{
			InitializeComponent();
			DataContext = new MainViewModel();
		}

		private MainViewModel ViewModel
		{
			get { return (MainViewModel)DataContext; }
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.NavigatedTo();
		}
	}
}