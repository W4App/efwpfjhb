using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
    /// ShowWts.xaml 的交互逻辑
    /// </summary>
    public partial class ShowWts : Window
    {
        List<WT> wts = new List<WT>();
        List<PF> pfs = new List<PF>();
        List<TZD> tzds = new List<TZD>();

        public ShowWts()
        {
            InitializeComponent();
        }

        private void showAll_Loaded(object sender, RoutedEventArgs e)
        {
           
            using (jhbEntities ctx = new jhbEntities())
            {
                wts = (from g in ctx.WTs orderby g.站名  select g).ToList();
                pfs = (from g in ctx.PFs orderby g.站名 select g).ToList();
                tzds = (from g in ctx.TZDs orderby g.站名 select g).ToList();

                showAll.ItemsSource = wts;
            }
        }       

        private void show_wt_Click(object sender, RoutedEventArgs e)
        {
            showAll.ItemsSource = wts;
        }

        private void show_pfs_Click(object sender, RoutedEventArgs e)
        {
            showAll.ItemsSource = pfs;
        }

        private void show_tzds_Click(object sender, RoutedEventArgs e)
        {
            showAll.ItemsSource = tzds;
        }

        private void wtFile_Click(object sender, RoutedEventArgs e)
        {
            string fn = g_var.getSaveDialog();
            if (fn == null)
            {
                return;
            }

            XElement AllDoc = new XElement("记录集", "");
            XElement doc = new XElement("re", "re");

            foreach (var it in wts)
            {
                string dt1 = string.Empty;
                if (it.销记时间 != null)
                {
                    dt1 = it.销记时间.Value.ToString("yyyy年M月d日");
                }
                doc =
                new XElement("Record",
                        new XElement("问题编号", it.问题编号),
                        new XElement("站名", it.站名),
                        new XElement("设备名称", it.设备名称),
                        new XElement("存在问题", it.存在问题),
                        new XElement("发现时间", it.发现时间),
                        new XElement("检查人", it.检查人),
                        new XElement("严重程度", it.严重程度),
                        new XElement("整改方案", it.整改方案),
                        new XElement("受理单位", it.受理单位),
                        new XElement("最后派发日期", it.rev1),
                        new XElement("销记时间", dt1),
                        new XElement("完成情况", it.完成情况),
                        new XElement("整治情况", it.整治情况),
                        new XElement("整治负责人", it.整治负责人)
                        );
                AllDoc.Add(doc);

            }
            AllDoc.Save(fn);
        }

        private void pfFile_Click(object sender, RoutedEventArgs e)
        {
            string fn = g_var.getSaveDialog();
            if (fn == null)
            {
                return;
            }

            XElement AllDoc = new XElement("记录集", "");
            XElement doc = new XElement("re", "re");

            foreach (var it in pfs)
            {
                string dt1 = string.Empty;
                string dt2 = string.Empty;
                if (it.派发日期 != null)
                {
                    dt1 = it.派发日期.Value.ToString("yyyy年M月d日");
                }
                if (it.计划时间!=null)
                {
                    dt2 = it.计划时间.Value.ToString("yyyy年M月d日");
                }
                doc =
                new XElement("Record",
                        new XElement("问题编号", it.问题编号),
                        new XElement("站名", it.站名),
                        new XElement("设备名称", it.设备名称),
                        new XElement("存在问题", it.存在问题),
                        new XElement("派发日期", dt1),
                        new XElement("受理单位", it.受理单位),
                        new XElement("计划时间", dt2),
                        new XElement("天窗需求", it.天窗需求)                      
                        );
                AllDoc.Add(doc);

            }
            AllDoc.Save(fn);
        }

        private void tzdFile_Click(object sender, RoutedEventArgs e)
        {
            string fn = g_var.getSaveDialog();
            if (fn == null)
            {
                return;
            }

            XElement AllDoc = new XElement("记录集", "");
            XElement doc = new XElement("re", "re");

            foreach (var it in tzds)
            {
                string dt1 = string.Empty;
                if (it.派单日期 != null)
                {
                    dt1 = it.派单日期.Value.ToString("yyyy年M月d日");
                }
                doc =
                new XElement("Record",                        
                        new XElement("站名", it.站名),
                        new XElement("派单日期", dt1),
                        new XElement("派发内容", it.记录),
                        new XElement("受理单位", it.受理单位),
                        new XElement("流水号", it.流水号)                       
                        );
                AllDoc.Add(doc);

            }
            AllDoc.Save(fn);
        }   

    }
}
