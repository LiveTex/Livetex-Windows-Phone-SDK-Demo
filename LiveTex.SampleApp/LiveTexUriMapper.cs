using System;
using System.Windows.Navigation;
using LiveTex.SampleApp.LiveTex;

namespace LiveTex.SDK.Sample
{
	internal class LiveTexUriMapper
		: UriMapperBase
	{
		public override Uri MapUri(Uri uri)
		{
			if(string.Equals(uri.OriginalString, "/StartPage.xaml"))
			{
				if(AppCredentials.IsSet)
				{
					return new Uri("/View/SelectServicePage.xaml", UriKind.Relative);
				}

				return new Uri("/View/MainPage.xaml", UriKind.Relative);
			}

			if(string.Equals(uri.OriginalString, "/StartPage.xaml?LiveTex=1"))
			{
				return new Uri("/View/DialogPage.xaml", UriKind.Relative);
			}

			return uri;
		}
	}
}