using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkerRegistratorGui.View.Converters
{
	[ValueConversion(typeof(float), typeof(float), ParameterType = typeof(string))]
	public class MultiplyConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var arg = (float)value;
			var multiplier = float.Parse((string)parameter);

			return arg * multiplier;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
