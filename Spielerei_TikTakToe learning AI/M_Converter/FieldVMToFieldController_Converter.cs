using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Spielerei_TikTakToe_learning_AI.Converter
{
    class FieldVMToFieldController_Converter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string pointView = "";
            int pointVM = (int)value;
            switch (pointVM)
            {
                case 1:
                    pointView = "X";
                    break;
                case -1:
                    pointView = "O";
                    break;
                case 0:
                    pointView = "";
                    break;
                default:
                    pointView = "?";
                    break;
            }
            return pointView;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
