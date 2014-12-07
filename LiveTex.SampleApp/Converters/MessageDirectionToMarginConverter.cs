using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using LiveTex.SampleApp.Wrappers;

namespace LiveTex.SampleApp.Converters
{
	public class MessageDirectionToMarginConverter
		: IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var message = value as ChatMessageWrapper;
			if(message == null)
			{
				return new Thickness(0,0,0,0);
			}

			return message.IsIncomingMessage
				? new Thickness(0, 0, 96, 0)
				: new Thickness(96, 0, 0, 0);
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
