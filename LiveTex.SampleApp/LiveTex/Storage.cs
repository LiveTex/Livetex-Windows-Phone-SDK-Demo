using System.IO.IsolatedStorage;

namespace LiveTex.SampleApp.LiveTex
{
	internal static class Storage
	{
		public static T GetValue<T>(string key, T defaultValue = default(T))
		{
			var settings = IsolatedStorageSettings.ApplicationSettings;

			object value;

			if(settings.TryGetValue(key, out value))
			{
				return (T)value;
			}

			return defaultValue;
		}

		public static void SetValue<T>(string key, T value)
		{
			var settings = IsolatedStorageSettings.ApplicationSettings;
			if(settings.Contains(key))
			{
				settings[key] = value;
			}
			else
			{
				settings.Add(key, value);
			}

			settings.Save();
		}
	}
}
