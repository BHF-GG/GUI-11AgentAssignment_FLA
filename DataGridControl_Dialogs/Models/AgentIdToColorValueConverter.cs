using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Lab3
{
    /// <summary>
    /// IValueConverter indeholder to metoder, som skal implementeres: Convert og Convertback
    /// </summary>
    /// 

        // Exercise 4.2.1
    
    class AgentIdToColorValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Debug.Assert(targetType == typeof(Brush));
            string id = value as string;
            if (id == null)
                id = "";
            // 007 requires special treatment ...
            return (id == "007" ? Brushes.Blue : Brushes.Black);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

