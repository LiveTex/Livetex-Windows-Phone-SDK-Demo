using System;
using System.Globalization;
using System.Windows.Data;
using LiveTex.SampleApp.Wrappers;
using LiveTex.SDK.Client;

namespace LiveTex.SampleApp.Converters
{
	public class ListItemWrapperConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var wrapper = value as ListItemWrapper;
			if(wrapper == null)
			{
				return "";
			}

			return wrapper.Description;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
