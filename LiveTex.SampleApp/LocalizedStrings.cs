using LiveTex.SampleApp.Resources;

namespace LiveTex.SDK.Sample
{
	/// <summary>
	/// Provides access to string resources.
	/// </summary>
	public class LocalizedStrings
	{
		private static readonly AppResources _localizedResources = new AppResources();

		public AppResources LocalizedResources => _localizedResources;
	}
}