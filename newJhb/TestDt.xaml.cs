using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using System.Data.SQLite;

namespace newJhb
{
    /// <summary>
    /// TestDt.xaml 的交互逻辑
    /// </summary>
    public partial class TestDt : Window
    {
        DateTime old = DateTime.Now;
        public TestDt()
        {
            InitializeComponent();

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            DateTime f = DateTime.Parse("2017-8-2");
            DateTime t = DateTime.Parse("2017-9-2");
            DateTime l = DateTime.Parse("2017-8-31");

            int x = myFuc.diffDays(f, t);
            int y = myFuc.diffDays(l, t);

            Console.WriteLine(myFuc.diffDays(f,t));
            
            
            
        }
    }
}
