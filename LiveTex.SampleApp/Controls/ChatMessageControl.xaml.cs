using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Windows.System;
using LiveTex.SampleApp.Wrappers;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace LiveTex.SampleApp.Controls
{
	public partial class ChatMessageControl
		: UserControl
	{
		public ChatMessageControl()
		{
			InitializeComponent();
		}

		private void UriOnClick(object sender, RoutedEventArgs e)
		{
			Launcher.LaunchUriAsync(new Uri((string)viewUri.Content));
		}
	}
}
