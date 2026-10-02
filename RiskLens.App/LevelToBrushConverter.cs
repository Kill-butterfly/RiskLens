using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using RiskLens.Core.Models;

namespace RiskLens.App
{
    public class LevelToBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => value is SignalLevel level ? level switch
            {
                SignalLevel.Red => Brushes.IndianRed,
                SignalLevel.Yellow => Brushes.Khaki,
                _ => Brushes.LightGreen
            } : Brushes.Transparent;

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotSupportedException();
    }
}
