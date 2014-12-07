using System.Windows.Navigation;
using LiveTex.SampleApp.ViewModel;
using Microsoft.Phone.Controls;

namespace LiveTex.SampleApp
{
	public partial class AbusePage
		: PhoneApplicationPage
	{
		public AbusePage()
		{
			InitializeComponent();
			DataContext = new AbuseViewModel();
		}

		private AbuseViewModel ViewModel
		{
			get { return (AbuseViewModel)DataContext; }
		}

		protected override void OnNavigatedTo(NavigationEventArgs e)
		{
			base.OnNavigatedTo(e);

			ViewModel.NavigatedTo();
		}
	}
}