using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Globalization;
using System.Windows.Media;
using System.ComponentModel;

namespace newJhb
{
    [ValueConversion(typeof(string),typeof(string))]
    class BlankDelConvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            if (value != null)
            {
                string tmp = (string)value;
                tmp = tmp.Replace("\r", string.Empty).Replace("\n", string.Empty);                                
                return tmp;
            }
            else
            {
                return value;
            }
                
            
        }
        public object ConvertBack(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            return value;
        }
    }
    [ValueConversion(typeof(string),typeof(SolidColorBrush))]
    class Str2Col:IValueConverter
    {
        public object Convert(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            if (value != null)
            {
                string tmp = (string)value;
                SolidColorBrush xCol = new SolidColorBrush();
                xCol.Color = Colors.Black;
                switch (tmp)
                {
                    case "严重":
                        xCol.Color = Colors.Red;
                        return xCol;
                    case "一般":
                        xCol.Color = Colors.Black;
                        return xCol;
                    case "观察":
                        xCol.Color = Colors.Green;
                        return xCol;  
                    case "轻微":
                        xCol.Color = Colors.Green;
                        return xCol;
                }
                return xCol;
            }
            else
            {
                return value;
            }


        }
        public object ConvertBack(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            return value;
        }  
    }
    [ValueConversion(typeof(Boolean), typeof(Boolean))]
    class bool2Neg:IValueConverter
    {
        public object Convert(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            Boolean tmp = (Boolean)value;
            return !tmp;
        }
        public object ConvertBack(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            return value;
        }

    }
    [ValueConversion(typeof(Boolean), typeof(string))]
    class bool2Str : IValueConverter
    {
        public object Convert(object value, Type targetType, object parm, CultureInfo culInfo)
        {

            Boolean tmp = (Boolean)value;
            if (tmp)
            {
                return "已销";
            }
            else
            {
                return "未销";
            }
            
        }
        public object ConvertBack(object value, Type targetType, object parm, CultureInfo culInfo)
        {
            return value;
        }

    }
    class SateBarTip :INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (PropertyChanged!=null)
            {
                PropertyChanged(this, e);
            }
        }

        private int _total;
        private int _query;
        private string _tip;
        private string _otherTip;
        private string _clock;

        public int Total
        {
            get
            {
                return _total;
            }
            set
            {
                _total = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Total"));
            }
        }

        public int Query
        {
            get
            {
                return _query;
            }
            set
            {
                _query = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Query"));
            }
        }

        public string Tip
        {
            get
            {
                return _tip;
            }
            set
            {
                _tip = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Tip"));
            }
        }

        public string OtherTip
        {
            get
            {
                return _otherTip;
            }
            set
            {
                _otherTip = value;
                OnPropertyChanged(new PropertyChangedEventArgs("OtherTip"));
            }
        }

        public string Clock
        {
            get
            {
                return _clock;
            }
            set
            {
                _clock = value;
                OnPropertyChanged(new PropertyChangedEventArgs("Clock"));
            }
        }

    }

}