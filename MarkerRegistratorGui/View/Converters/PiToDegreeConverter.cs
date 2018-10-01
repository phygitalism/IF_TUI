using System;
using System.Globalization;
using System.Windows.Data;

namespace MarkerRegistratorGui.View.Converters
{
	[ValueConversion(typeof(float), typeof(float))]
	public class PiToDegreeConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var pis = (float)value;

			return 180.0f * pis / Math.PI;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
