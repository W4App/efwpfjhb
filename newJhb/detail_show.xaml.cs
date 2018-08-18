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

namespace newJhb
{
    /// <summary>
    /// detail_show.xaml 的交互逻辑
    /// </summary>
    public partial class detail_show : Window
    {
        private Int64 _indx;
        private PF _pf;
        public detail_show(Int64 indx)
        {
            InitializeComponent();
            _indx = indx;
            _pf = new PF();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            

            using (jhbEntities ctx=new jhbEntities())
            {
                var pfs = from g in ctx.PFs where g.问题编号 == _indx orderby g.派发编号 descending select g;
                cnt.Text = string.Format("派发次数:  {0}",pfs.Count());
                foreach (var item in pfs)
                {
                    string tmpTx = string.Format("通知日期:{0}   计划处理日期:{1}    受理单位:  {2}",
                        item.派发日期.Value.ToString("yyyy年M月d日"),
                        item.计划时间.Value.ToString("yyyy年M月d日"),
                        item.受理单位
                        );
                    Run r=new Run(tmpTx);
                    Paragraph p = new Paragraph(r);
                    ListItem ls = new ListItem(p);
                    Lst.ListItems.Add(ls);
                }
            }
        }
    }
}
