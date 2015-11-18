using System;
using System.Windows;
using System.Windows.Controls;
using Windows.System;

namespace LiveTex.SampleApp.Controls
{
	public partial class ChatMessageControl
		: UserControl
	{
		public ChatMessageControl()
		{
			InitializeComponent();
		}

		private async void UriOnClick(object sender, RoutedEventArgs e)
		{
			try
			{
				var uri = viewUri.Tag as string;

				if(string.IsNullOrWhiteSpace(uri))
				{
					return;
				}

				await Launcher.LaunchUriAsync(new Uri(uri));
			}
			catch(Exception)
			{
				MessageBox.Show("Не удалось открыть ссылку", "Ошибка", MessageBoxButton.OK);
			}
		}
	}
}
