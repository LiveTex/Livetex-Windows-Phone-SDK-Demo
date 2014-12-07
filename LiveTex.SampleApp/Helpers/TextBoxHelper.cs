using System.Windows;
using System.Windows.Controls;

namespace LiveTex.SampleApp.Helpers
{
	public static class TextBoxHelper
	{
		public static readonly DependencyProperty UpdateSourceOnTextChangedProperty = DependencyProperty.RegisterAttached(
			"UpdateSourceOnTextChanged", typeof (bool), typeof (TextBoxHelper), new PropertyMetadata(default(bool), OnUpdateSourceOnTextChangedChanged));

		public static void SetUpdateSourceOnTextChanged(DependencyObject element, bool value)
		{
			element.SetValue(UpdateSourceOnTextChangedProperty, value);
		}

		public static bool GetUpdateSourceOnTextChanged(DependencyObject element)
		{
			return (bool) element.GetValue(UpdateSourceOnTextChangedProperty);
		}

		private static void OnUpdateSourceOnTextChangedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			var textBox = d as TextBox;
			if(textBox == null)
			{
				return;
			}

			var attach = (bool)e.NewValue;

			textBox.TextChanged -= TextBoxTextChanged;
			if (attach)
			{ 
				textBox.TextChanged += TextBoxTextChanged; 
			}
		}

		private static void TextBoxTextChanged(object sender, TextChangedEventArgs e)
		{
			var textBox = sender as TextBox;
			if(textBox == null)
			{
				return;
			}

			var binding = textBox.GetBindingExpression(TextBox.TextProperty);
			if (binding != null)
			{
				binding.UpdateSource();
			}
		}
	}
}
