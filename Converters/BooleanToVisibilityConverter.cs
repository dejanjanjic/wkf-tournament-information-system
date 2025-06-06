﻿using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WKFTournamentIS.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue)
            {
                if (parameter != null && parameter.ToString().Equals("Inverted", StringComparison.OrdinalIgnoreCase))
                {
                    return boolValue ? Visibility.Collapsed : Visibility.Visible;
                }
                return boolValue ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility visibilityValue)
            {
                if (parameter != null && parameter.ToString().Equals("Inverted", StringComparison.OrdinalIgnoreCase))
                {
                    return visibilityValue == Visibility.Collapsed;
                }
                return visibilityValue == Visibility.Visible;
            }
            return false;
        }
    }
}