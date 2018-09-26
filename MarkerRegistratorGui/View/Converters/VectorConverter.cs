using System;
using System.Globalization;
using System.Numerics;
using System.Windows.Data;

namespace MarkerRegistratorGui.View.Converters
{
	[ValueConversion(typeof(Vector2), typeof(float), ParameterType = typeof(string))]
	public class VectorConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var vector = (Vector2)value;

			switch (parameter)
			{
				case string s when s.Equals("x", StringComparison.OrdinalIgnoreCase):
					return vector.X;
				case string s when s.Equals("y", StringComparison.OrdinalIgnoreCase):
					return vector.Y;
				default:
					throw new NotSupportedException();
			}
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotImplementedException();
	}
}
