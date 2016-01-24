namespace LiveTex.SampleApp.LiveTex
{
	internal static class AppCredentials
	{
		private const string cKeyKey = "ApplicationKey";
		private const string cAppIDKey = "ApplicationID";
		private const string cAuthUriKey = "AuthServerUriKey";

		public static string Key { get; private set; }

		public static string ApplicationID { get; private set; }

		public static string AuthServerUri { get; private set; }

		public static void Set(string key, string applicationID, string authServerUri)
		{
			Key = key;
			ApplicationID = applicationID;
			AuthServerUri = authServerUri;

			Save();
		}

		public static bool IsSet => !string.IsNullOrWhiteSpace(Key) && !string.IsNullOrWhiteSpace(ApplicationID) && !string.IsNullOrWhiteSpace(AuthServerUri);

		public static void Load()
		{
			Key = Storage.GetValue(cKeyKey, Config.cKey);
			ApplicationID = Storage.GetValue(cAppIDKey, Config.cApplicationID);
			AuthServerUri = Storage.GetValue(cAuthUriKey, Config.cAuthServiceUri);
		}

		public static void Save()
		{
			Storage.SetValue(cKeyKey, Key);
			Storage.SetValue(cAppIDKey, ApplicationID);
			Storage.SetValue(cAuthUriKey, AuthServerUri);
		}
	}
}