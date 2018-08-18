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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SQLite;
using System.Globalization;
using System.Data.Entity;
using System.Windows.Media.Animation;

namespace newJhb
{
    public partial class MainWindow : Window
    {
        private void SW_Click(object sender, RoutedEventArgs e)  //主面板格式化...
        {
            switch (all_sw.Header.ToString())
            {
                case ">>":
                    SW1.Visibility = System.Windows.Visibility.Visible;
                    SW2.Visibility = System.Windows.Visibility.Visible;
                    SW3.Visibility = System.Windows.Visibility.Visible;
                    SW4.Visibility = System.Windows.Visibility.Visible;
                    SW5.Visibility = System.Windows.Visibility.Visible;
                    SW6.Visibility = System.Windows.Visibility.Visible;
                    SW7.Visibility = System.Windows.Visibility.Visible;
                    all_sw.Header = "<<";
                    break;
                case "<<":
                    SW1.Visibility = System.Windows.Visibility.Collapsed;
                    SW2.Visibility = System.Windows.Visibility.Collapsed;
                    SW3.Visibility = System.Windows.Visibility.Collapsed;
                    SW4.Visibility = System.Windows.Visibility.Collapsed;
                    SW5.Visibility = System.Windows.Visibility.Collapsed;
                    SW6.Visibility = System.Windows.Visibility.Collapsed;
                    SW7.Visibility = System.Windows.Visibility.Collapsed;
                    all_sw.Header = ">>";
                    break;
            }
        }
        private void pfTreeInit() //树形界面初始化...
        {

            List<DateTime?> pfDt = (from g in AllPF orderby g.派发日期 descending select g.派发日期).ToList();
           
            foreach (var it in pfDt.Distinct())
            {
                TreeViewItem item = new TreeViewItem();
                List<PF> dtPfs = (from p in AllPF where p.派发日期.Value == it.Value select p).ToList(); //子集合
                //这里要携带足够的信息到下层.
                item.Tag = new mTreeInfo { flag = 1, Info = it.Value.ToString("yyyy-M-d"), listPf = dtPfs };
                item.Header = it.Value.ToString("yyyy-M-d");
                item.Items.Add("*");
                pfTree.Items.Add(item);
            }
        }
        private void TzdTreeInit() //树形界面初始化...
        {
           
            List<DateTime?> tzdDt = (from g in AllTZD orderby g.派单日期 descending select g.派单日期).ToList();
                     
            foreach (var it in tzdDt.Distinct())
            {
                TreeViewItem item = new TreeViewItem();
                List<TZD> dtTzds = (from t in AllTZD where t.派单日期.Value == it.Value select t).ToList(); //tzd子集
                item.Tag = new mTreeInfo { flag = 1, Info = it.Value.ToString("yyyy-M-d"), listTzd = dtTzds };
                item.Header = it.Value.ToString("yyyy-M-d");
                item.Items.Add("*");
                tzdTree.Items.Add(item);
            }
        }

        private void pfTreeExp(object sender, RoutedEventArgs e)  //树形界面展开事件处理....
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            mTreeInfo nodeInfo = item.Tag as mTreeInfo;
            if (nodeInfo == null)
            {
                return;
            }
            switch (nodeInfo.flag)
            {
                case 1: //日期
                    var x = (from a in nodeInfo.listPf where a.派发日期.Value.ToString("yyyy-M-d") == nodeInfo.Info select a.站名).Distinct();
                    foreach (var zn in x)
                    {
                        List<PF> znPfs = (from mmm in nodeInfo.listPf where mmm.站名 == zn select mmm).ToList();
                        TreeViewItem znitem = new TreeViewItem();
                        znitem.Tag = new mTreeInfo { flag = 2, Info = zn, listPf = znPfs };
                        znitem.Header = zn.ToString();
                        znitem.Items.Add("*");
                        item.Items.Add(znitem);
                    }

                    break;
                case 2: //站名
                    var y = (from b in nodeInfo.listPf where b.站名 == nodeInfo.Info select b.受理单位).Distinct();
                    foreach (var un in y)
                    {
                        List<PF> unPfs = (from u in nodeInfo.listPf where u.受理单位 == un select u).ToList();
                        TreeViewItem unItem = new TreeViewItem();
                        unItem.Tag = new mTreeInfo { flag = 3, Info = un, listPf = unPfs };
                        unItem.Header = un.ToString();
                        //unItem.Items.Add("*");
                        item.Items.Add(unItem);
                    }
                    break;
            }
        }

        private void tzdTreeExp(object sender, RoutedEventArgs e) //树形界面展开事件处理....
        {
            TreeViewItem item = (TreeViewItem)e.OriginalSource;
            item.Items.Clear();
            mTreeInfo nodeInfo = item.Tag as mTreeInfo;
            if (nodeInfo == null)
            {
                return;
            }
            switch (nodeInfo.flag)
            {
                case 1: //日期
                    var x = (from a in nodeInfo.listTzd where a.派单日期.Value.ToString("yyyy-M-d") == nodeInfo.Info select a.站名).Distinct();
                    foreach (var zn in x)
                    {
                        List<TZD> znTzds = (from mmm in nodeInfo.listTzd where mmm.站名 == zn select mmm).ToList();
                        TreeViewItem znitem = new TreeViewItem();
                        znitem.Tag = new mTreeInfo { flag = 2, Info = zn, listTzd = znTzds };
                        znitem.Header = zn.ToString();
                        znitem.Items.Add("*");
                        item.Items.Add(znitem);
                    }


                    break;
                case 2: //站名
                    var y = (from b in nodeInfo.listTzd where b.站名 == nodeInfo.Info select b.受理单位).Distinct();
                    foreach (var un in y)
                    {
                        List<TZD> unTzds = (from u in nodeInfo.listTzd where u.受理单位 == un select u).ToList();
                        TreeViewItem unItem = new TreeViewItem();
                        unItem.Tag = new mTreeInfo { flag = 3, Info = un, listTzd = unTzds };
                        unItem.Header = un.ToString();
                        //unItem.Items.Add("*");
                        item.Items.Add(unItem);
                    }
                    break;
            }
        }

        private void Button_Click_pfFresh(object sender, RoutedEventArgs e) //派发树 -  刷新按钮
        {
            pfTree.Items.Clear();
            pfTreeInit();
        }
        private void Button_Click_tzdFresh(object sender, RoutedEventArgs e) //通知单树 - 刷新按钮
        {
            tzdTree.Items.Clear();
            TzdTreeInit();
        }
        private void pftree_slt(object sender, RoutedEventArgs e)  //待处理的文档事件
        {

            TreeViewItem tvi = new TreeViewItem();
            tvi = e.OriginalSource as TreeViewItem;
            mTreeInfo tinf = tvi.Tag as mTreeInfo;
            List<PF> listPf = tinf.listPf as List<PF>;
            
            if (listPf != null)
            {
                //var lsitpf = from g in listPf select g;
                docPfdataGrid.FontSize = 16;
                docPfdataGrid.ItemsSource = listPf;
                     
                if (tinf.flag==3)
                {
                    var zn = (from g in listPf select g.站名).First();
                    var dt = (from g in listPf select g.派发日期).First();
                                               
                    doc_zz_tabHeader.Text = 
                        string.Format("        {0}，经对{1}站结合部设备问题收集，存在下列问题，计划下周整治,请按计划时间准备人力、料具到现场共同配合",dt.Value.ToString("yyyy年M月d日"), zn);
                   
                }                    
            }

        }
        private void tzdTree_slt(object sender, RoutedEventArgs e) //待处理的文档事件
        {
            TreeViewItem tvi = new TreeViewItem();
            tvi = e.OriginalSource as TreeViewItem;
            mTreeInfo tinf = tvi.Tag as mTreeInfo;
            List<TZD> listTzd = tinf.listTzd as List<TZD>;
            if (listTzd != null)
            {
                tzdContent.FontSize = 16;
                if (tinf.flag==3)
                {
                    var tzd = listTzd.First();
                    int count;

                    myFuc.tzdPageList = null;
                    TzdPageNext.IsEnabled = false;
                    TzdPagePre.IsEnabled = false;

                    myFuc.tzdPageList = myFuc.Tzd2List(tzd.记录);

                   
                    tzdContent.Document.Blocks.Clear();

                    count = myFuc.tzdPageList.Count();

                    tzddocZn.Text = tzd.站名 + "信号工区";
                    tzddocDate.Text = tzd.派单日期.Value.ToString("yyyy年M月d日");
                    tzddoccz.Text = tzd.站名;
                    tzddocUnit.Text = tzd.受理单位;

                    string[] FirstPage = myFuc.tzdPageList.ToArray();
                    string addText = string.Empty;
                    string addtext1 = string.Empty;
                    if (count>myFuc.Itsum)
                    {
                        TzdPageNext.IsEnabled = true;
                        TzdPagePre.IsEnabled = true;
                    }
                                            
                    for (int i = 0; i < FirstPage.Count(); i++)
                    {
                        addText = addText + FirstPage[i];
                        addtext1 = addtext1 + FirstPage[i];
                    }
                    tzdContent.AppendText(addText);
                    Tcontent = addtext1;

                   
                    
                                 
                }
          }
        }
        private void jd_tree_pf(object sender, RoutedEventArgs e)  //简单 树形Grid关联
        {
            tbiPf.Focus();
            pfSelect.Focus();
            TreeViewItem ntvi = e.OriginalSource as TreeViewItem;
            string headStr = ntvi.Header.ToString();
            if (headStr == "派发信息")
            {
                List<DateTime?> pfDt = (from g in AllPF orderby g.派发日期 descending select g.派发日期).ToList();
                
                this.pfSelect.ItemsSource = pfDt.Distinct();
                
                
                pfDetial.ItemsSource = AllPF;
            }
            else
            {
                var x = from g in AllPF where g.派发日期 == DateTime.Parse(headStr) select g;
                pfDetial.ItemsSource = x.ToList();
            }
        }
        private void jd_tree_tzd(object sender, RoutedEventArgs e) ////简单 树形Grid关联
        {
            tbiTzd.Focus();
            tzdSelect.Focus();
            TreeViewItem ntvi = e.OriginalSource as TreeViewItem;
            string headStr = ntvi.Header.ToString();
            if (headStr == "通知单信息")
            {
                List<DateTime?> tzdDt = (from g in AllTZD orderby g.派单日期 select g.派单日期).ToList();
                this.tzdSelect.ItemsSource = tzdDt.Distinct();
                TzdDetial.ItemsSource = AllTZD;
            }
            else
            {
                var x = from g in AllTZD where g.派单日期 == DateTime.Parse(headStr) select g;
                TzdDetial.ItemsSource = x.ToList();
            }
        }
        private void QueryCMB_Chg(object sender, SelectionChangedEventArgs e)  //查询主事件处理.
        {
            
            ComboBox cbx = (ComboBox)e.OriginalSource;
            
            if (cbx.SelectedItem is string) //站名 和 受理单位
            {
                switch (cbx.Tag.ToString())
                {
                    case "zhName":
                        string tmp_zn = cbx.SelectedItem.ToString();
                        list_zn = (from zn in AllWT where zn.站名 == tmp_zn select zn).ToList();
                        //站名决定受理单位...
                        init_unit.ItemsSource = (from f in AllWT where f.站名==tmp_zn select f.受理单位).Distinct().ToList();
                        break;
                    case "Unit":
                        string tmp_un = cbx.SelectedItem.ToString();
                        if (cbx.SelectedItem ==null)
                        {
                            Console.WriteLine("bug?");
                        }
                        list_unit = (from un in AllWT where un.受理单位 == tmp_un select un).ToList();
                        break;
                }
            }
            if (cbx.SelectedItem is ComboBoxItem)  //销记状态 和严重程度
            {
                ComboBoxItem cbi = (ComboBoxItem)cbx.SelectedItem;
                switch (cbx.Tag.ToString())
                {
                    case "doWith":
                        string tmp_do = cbi.Content.ToString();
                        Boolean tmp_flag = false;
                        if (tmp_do == "已销记")
                        {
                            tmp_flag = true;
                        }
                        list_dowith = (from dw in AllWT where dw.销记 == tmp_flag select dw).ToList();
                        break;
                    case "Merge":

                        string tmp_m = cbi.Content.ToString();                        
                        list_merge = (from mg in AllWT where mg.严重程度 == tmp_m select mg).ToList();
                        break;
                }
            }
            //----集中处理.
            var QuerData = (list_zn.Intersect(list_dowith).Intersect(list_merge).Intersect(list_unit)).Distinct().ToList();            
            mainGrid.ItemsSource = QuerData;
            g_Query_res = QuerData;

            g_var.Bar.Query = g_Query_res.Count();
        }
        
        private void Button_ResetQuery(object sender, RoutedEventArgs e)  //查询重置 --
        {
            //查询基于主表, 强制更新主表到最新数据库状态
            AllWT= myFuc.LoadWts();
            foreach (var item in sp_test.Children)
            {
                if (item is ComboBox)
                {
                    ComboBox cbx = (ComboBox)item;
                    cbx.SelectedIndex = -1;
                    mainGrid.ItemsSource = AllWT;
                    list_zn = AllWT;
                    list_dowith = AllWT;
                    list_merge = AllWT;
                    list_unit = AllWT;
              }
            }
            init_unit.ItemsSource = (from f in AllWT select f.受理单位).Distinct().ToList();

            g_var.Bar.Query = 0;
        }       
    }
}
