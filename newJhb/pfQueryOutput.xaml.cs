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
using System.Xml.Linq;

namespace newJhb
{
    /// <summary>
    /// pfQueryOutput.xaml 的交互逻辑
    /// </summary>
    public partial class pfQueryOutput : Window
    {

        public List<PF> all_pf = new List<PF>(); // 加载所有的派发项.

        public List<PF> znList = new List<PF>();
        public List<PF> s_list = new List<PF>();
        public List<PF> e_list = new List<PF>();

        public List<PF> tmp_list = new List<PF>();

        public pfQueryOutput()
        {
            InitializeComponent();
        }

        private void Button_ResetQuery(object sender, RoutedEventArgs e)
        {
            znList = all_pf;
            s_list = all_pf;
            e_list = all_pf;
            tmp_list = all_pf;
           
            showSelectPfGrid.ItemsSource = all_pf;
            start_time.SelectedDate = null;
            end_time.SelectedDate = null;
            pfZn.SelectedIndex = -1;
        }

        private void start_time_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {            
            if (start_time.SelectedDate != null)
            {
                s_list = (from g in all_pf where g.派发日期.Value >= start_time.SelectedDate.Value select g).ToList();
                tmp_list = tmp_list.Intersect(s_list).Intersect(e_list).ToList();
                showSelectPfGrid.ItemsSource = tmp_list;
                
            }

        }

        private void end_time_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            
            if (end_time.SelectedDate != null)
            {
                e_list = (from g in all_pf where g.派发日期.Value < end_time.SelectedDate.Value select g).ToList();
                tmp_list = tmp_list.Intersect(s_list).Intersect(e_list).ToList();
                showSelectPfGrid.ItemsSource = tmp_list;
            }

        }

        private void init_zhname_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (pfZn.SelectedIndex==-1)
            {
                return;
            }
            znList = (from g in all_pf where g.站名 == pfZn.SelectedValue.ToString() select g).ToList();
            tmp_list =all_pf.Intersect(znList).ToList();
            showSelectPfGrid.ItemsSource = tmp_list;
            start_time.SelectedDate = null;
            end_time.SelectedDate = null;

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (jhbEntities ctx = new jhbEntities())
            {
                all_pf = ctx.PFs.ToList();
                pfZn.ItemsSource = (from g in all_pf select g.站名).Distinct().ToList();
                znList = all_pf;
                s_list = all_pf;
                e_list = all_pf;
                tmp_list = all_pf;
                
                showSelectPfGrid.ItemsSource = all_pf;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            List<PF> tab =(List<PF>)showSelectPfGrid.ItemsSource;
            if (tab==null)
            {
                return;
            }
            PF[] Arr = tab.ToArray();
            
            string fn = g_var.getSaveDialog();

            if (fn == null)
            {
                return;
            }

            XElement AllDoc = new XElement("记录集", "");
            XElement doc = new XElement("re", "re");
            for (int i = 0; i < Arr.Count(); i++)
            {
                string pfDate = string.Empty;
                string pfUint = Arr[i].站名 + "信号工区";
                string planDate = string.Empty;
                //string finishDate = string.Empty;
                string workItem = Arr[i].设备名称 + ":  " + Arr[i].存在问题;

                if (Arr[i].派发日期 != null)
                {
                    pfDate = Arr[i].派发日期.Value.ToString("yyyy年M月d日");
                }
                if (Arr[i].计划时间!=null)
                {
                    planDate = Arr[i].计划时间.Value.ToString("yyyy年M月d日");
                }

                doc =
                new XElement("Record",
                        new XElement("序号", (i + 1).ToString()),
                        new XElement("下发日期", pfDate),
                        new XElement("车站", Arr[i].站名),
                        new XElement("作业内容", workItem),
                        new XElement("计划作业日期", planDate),
                        new XElement("施工单位", pfUint),
                        new XElement("配合单位", Arr[i].受理单位),
                        new XElement("配合单位签认日期"),
                        new XElement("完成日期", Arr[i].ver1),
                        new XElement("作业人员", Arr[i].var4),
                        new XElement("盯控干部"),
                        new XElement("完成情况", Arr[i].ver2),
                        new XElement("配合单位是否签字")
                        );
                AllDoc.Add(doc);

            }    
            AllDoc.Save(fn);

        }
    }
}
