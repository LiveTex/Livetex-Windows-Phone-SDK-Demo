using System;
using System.Globalization;
using System.Windows.Data;

namespace LiveTex.SampleApp.Converters
{
	public class UpdateTimeConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var timestamp = value as DateTime?;
			if(timestamp == null)
			{
				return null;
			}

			if(timestamp.Value.Date == DateTime.Today)
			{
				return timestamp.Value.ToString("hh:mm");
			}

			if(timestamp.Value.Date.Year == DateTime.Today.Year)
			{
				return timestamp.Value.ToString("dd.MM");
			}

			return timestamp.Value.ToString("dd.MM.yy");
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}