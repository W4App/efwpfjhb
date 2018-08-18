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
using System.Windows.Controls.Primitives;
using System.Data;
using System.Data.Entity.Infrastructure;
using System.ComponentModel;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.HPSF;
using System.IO;

namespace newJhb
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// 9.29日添加主表中Ver1 字段为 最后一次派发日期 字符串. 方便导出表
    /// </summary>
    public partial class MainWindow : Window
    {

        Timer rollAct = new Timer(g_var.timeDo, null, 0, 1000);

        List<WT> AllWT = new List<WT>();
        
        List<TZD> AllTZD = new List<TZD>();
        List<PF> AllPF = new List<PF>();

        List<WT> g_Query_res = new List<WT>();
        //------操作上下文变量
        long TodayBincode=0;
        DateTime mNow;
        //----混合查询 维恩图大法!
        List<WT> list_zn=new List<WT>(); 
        List<WT> list_unit=new List<WT>();
        List<WT> list_dowith=new List<WT>();
        List<WT> list_merge=new List<WT>();
        //---录入全局对象.
        storeWtRec g_wt_info_rec = new storeWtRec(); //记录存储  对象
        wtReInfo Rec_info;   //记录  对象
        // DatePicker 的特殊要求.........
        DatePickerTextBox dptx = new DatePickerTextBox();
        // 设备名称过滤.   很重要, 决定更新或新增
        int eq_name_count = -1;
       
        
        //数据库性能--  构造跟踪器. 集中处理数据库更新

        string Tcontent = string.Empty;
        //+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
        // 随机生成的临时文件, 记录, 之后清理!
        List<string> TempFile = new List<string>();
        public MainWindow()  //窗体构造....
        {
            InitializeComponent();
            TodayBincode = myFuc.getNowDateHashcode();
            mNow = DateTime.Now.Date;

            g_var.tableValid(); //清除和检查数据库内容.

            
        }
      
        

        private void Window_Loaded(object sender, RoutedEventArgs e)  //界面初始化
        {

            AllWT=  myFuc.LoadWts();
            AllPF = myFuc.LoadPfs();
            AllTZD = myFuc.LoadTzds();
            All_init();
            mainGrid.ItemsSource = (from g in AllWT where g.销记 == false select g).ToList();
            pfDetial.ItemsSource = AllPF;
            TzdDetial.ItemsSource = AllTZD;
            //状态条绑定.
            StatusTip.DataContext = g_var.Bar;
            StatusTip1.DataContext = g_var.Bar;

            
        }
        
       
       private void mainGrid_LoadingRow(object sender, DataGridRowEventArgs e) //主WT 行格式化...
       {
           WT wt = e.Row.DataContext as WT;

           //着色: 1, 最近修改的当天, 显示绿色, 近3天显示淡绿. 发现时间和今天差15天未动的显示红色 销记的显示灰色
           //--- wt.lastEdit.
           DateTime lastEd = DateTime.FromBinary(wt.lastEdit.Value).Date;
            //只介入 上次处理时间和今天比较.... 因为录入时, 就有个处理时间...

           e.Row.Header = wt.OpTrack;
           if (wt.销记==true)
           {
               e.Row.Background = new SolidColorBrush(Colors.Gray);
           }
           else
           {
               switch (myFuc.diffDays(lastEd, mNow))
               {
                   case 0:  
                   case 1:                                                
                       e.Row.Background = new SolidColorBrush(Colors.Aquamarine);                       
                       break;
                   //case -2:
                   //case -3:
                   //case -4:
                   //case -5:
                   //case -6:
                   //case -7:
                   //    e.Row.Background = new SolidColorBrush(Color.FromRgb(189, 252, 201));                      
                   //    break;
                   //case -8:
                   //case -9:
                   //case -10:
                   //case -11:
                   //case -12:
                   //case -13:
                   //case -14:
                   //    e.Row.Background = new SolidColorBrush(Color.FromRgb(255, 227, 132)); 
                   //    break;
                   case -15:
                   case -16:
                   case -17:
                   case -18:
                   case -19:
                   case -20:
                       e.Row.Background = new SolidColorBrush(Colors.IndianRed);  
                       break;

                   default:
                       if (e.Row.AlternationIndex == 0)
                       {
                           e.Row.Background = null;
                       }
                       else
                       {
                           e.Row.Background = new SolidColorBrush(Colors.Azure);
                       }
                       break;
               }

           }
           
          
       }
           
       private void All_init()  //系统初始化...
       {           
           this.init_zhname.ItemsSource = (from g in AllWT select g.站名).Distinct();           
           this.init_unit.ItemsSource = (from f in AllWT select f.受理单位).Distinct();
           this.Re_zn.ItemsSource = (from g in AllWT select g.站名).Distinct(); 
           this.pfSelect.ItemsSource = (from g in AllPF 
                                        orderby g.派发日期 descending
                                        select g.派发日期).Distinct();           
           this.tzdSelect.ItemsSource = (from g in AllTZD 
                                         orderby g.派单日期 descending
                                         select g.派单日期).Distinct();
           pfTreeInit();
           TzdTreeInit();
           //--- 查询初始
           list_zn = AllWT;
           list_dowith = AllWT;
           list_merge = AllWT;
           list_unit = AllWT;
           //输入界面 绑定初始化...
           Rec_info = g_wt_info_rec.getWtRec();  //数据点 记录器...
           Re_rows.DataContext = Rec_info;   //           
       }    
        //-------主窗事件处理--------------------
       private void MainGrid_Pf(object sender, RoutedEventArgs e)  // 派发操作
       {

           List<WT> oldItem = mainGrid.ItemsSource as List<WT>;
           if (oldItem == null)
           {
               return;
           }           
           WT xWt = (WT)mainGrid.SelectedValue; 
            
           pfDialog pfwin = new pfDialog(xWt.问题编号, xWt.站名);
           g_var.Bar.Tip = "正在派发";
           pfwin.Owner = this;
           pfwin.ShowDialog();
           if (g_var.g_pf_flag==true)
           {

               g_var.g_pf_flag = false;
               
               for (int i = 0; i < oldItem.Count(); i++)
               {
                   if (oldItem[i].问题编号==xWt.问题编号)
                   {
                       oldItem.Remove(oldItem[i]);
                       oldItem.Insert(i, g_var.g_chg_wt);
                   }
               }
               mainGrid.ItemsSource = oldItem;
               mainGrid.Items.Refresh();
               GridUpdate("pf");
               GridUpdate("tzd");
           }
            
       }
       private void MainGrid_xj(object sender, RoutedEventArgs e)  // 销记操作
       {
           
           List<WT> oldItem = mainGrid.ItemsSource as List<WT>;
           
           if (oldItem==null)
           {               
               return;
           }
           WT xWt = (WT)mainGrid.SelectedValue;
           xjWin xWin = new xjWin(xWt.问题编号,xWt.站名);
           g_var.Bar.Tip = "正在销记";
           xWin.Owner = this;
           xWin.ShowDialog();

           if (g_var.g_xj_flag==true)
           {
               g_var.g_xj_flag = false;

               for (int i = 0; i < oldItem.Count(); i++)
               {
                   if (oldItem[i].问题编号==xWt.问题编号)
                   {
                       oldItem.Remove(oldItem[i]);
                       oldItem.Add(g_var.g_chg_wt);
                   }
               }                             
               mainGrid.ItemsSource = oldItem;
               mainGrid.Items.Refresh();
              
           }

          
       }
       

       //----------主窗事件处理---------------
       

       private void Rec_zn_init(object sender, SelectionChangedEventArgs e) //站名改变事件  --方便录入
       {
           ComboBox cbx = (ComboBox)e.OriginalSource;
           if (cbx!=null && cbx.SelectedItem!=null)
           {    
               string zn=cbx.SelectedItem.ToString();
               Rec_info.setListInfo(zn);

               Re_eq.ItemsSource = Rec_info.str_eqName;
               Re_who.ItemsSource = Rec_info.str_checker;
               Re_unit.ItemsSource = Rec_info.str_unit;
               //录入时... 主窗置位到站名,未销记....
               set_main_rec_mode(zn, false); 
               Re_eq.LostFocus += Re_eq_LostFocus; //站名有了, 站名和问题名称 联动检测...
           }
           else
           {
               Re_eq.LostFocus -= Re_eq_LostFocus;
           }
           
           
       }
       private void Record_Click(object sender, RoutedEventArgs e) //录入处理 按键事件
       {
           
           if (Rec_info.站名==""||Rec_info.站名==null)
           {
              Valid_Ani va = new Valid_Ani();
               Ani_zn.BeginAnimation(SolidColorBrush.ColorProperty, va.color_Ani);
               Re_zn.GotFocus += Ani_Valid_gotFoc;
               return;
           }
           
           if (Rec_info.设备名称 == null || Rec_info.设备名称 == string.Empty)
           {
               Valid_Ani vd = new Valid_Ani();
               Ani_eqName.BeginAnimation(SolidColorBrush.ColorProperty, vd.color_Ani);
               Re_eq.GotFocus += Ani_Valid_gotFoc;
               return;
           }           
           if (Rec_info.存在问题==null||Rec_info.存在问题==string.Empty)
           {
               Valid_Ani vc = new Valid_Ani();
               Ani_eqFault.BeginAnimation(SolidColorBrush.ColorProperty, vc.color_Ani);
               Re_eqfault.GotFocus += Ani_Valid_gotFoc;
               return;
           }
           if (Rec_info.发现时间==null)
           {
               Valid_Ani vb = new Valid_Ani();
               dptx.Background.BeginAnimation(SolidColorBrush.ColorProperty, vb.color_Ani);
               dptx.GotFocus += PART_TextBox_GotFocus;
               return;
           }
           if (eq_name_count>0 && Rec_info.销记==false) //更新
           {
               wt_update();
           }
           else
           {
               wt_insert_new();
           }
          
           //GridUpdate("wt");
           AllWT = myFuc.LoadWts();
           mainGrid.ItemsSource = AllWT;

           init_zhname.ItemsSource = (from g in AllWT select g.站名).Distinct();
           init_unit.ItemsSource = (from f in AllWT select f.受理单位).Distinct();
           Re_zn.ItemsSource = (from g in AllWT select g.站名).Distinct(); 
           //插入或更新后, 全局记录器 重置!
           g_wt_info_rec.Re_reset(Rec_info.站名);
           Rec_info = g_wt_info_rec.getWtRec();
           
           Re_rows.DataContext = null;
           Re_rows.DataContext = Rec_info;

           edit_tips.Content = "存在问题";
           edit_tips.Foreground = new SolidColorBrush(Colors.Black);   

      }

      

     
     private void Ani_Valid_gotFoc(object sender, RoutedEventArgs e) //验证处理器... 恢复输入逻辑
       {

           if (sender is TextBox) //存在问题...
           {
               Ani_eqFault.BeginAnimation(SolidColorBrush.ColorProperty, null);
               Re_eqfault.GotFocus -= Ani_Valid_gotFoc;               
           }

           if (sender is ComboBox)
           {
               ComboBox cbx = (ComboBox)sender;
               switch ((string)cbx.Tag)
               {
                   case "Zn":                       
                       Ani_zn.BeginAnimation(SolidColorBrush.ColorProperty, null);
                       Re_zn.GotFocus -= Ani_Valid_gotFoc;
                       break;
                   case "eqName":
                       Ani_eqName.BeginAnimation(SolidColorBrush.ColorProperty, null);
                       Re_eq.GotFocus -= Ani_Valid_gotFoc;
                       break;
               }
           }                     
       }
     private void PART_TextBox_GotFocus(object sender, RoutedEventArgs e)
     {
         //干掉动画.
         dptx.Background.BeginAnimation(SolidColorBrush.ColorProperty, null);
         dptx.GotFocus -= PART_TextBox_GotFocus;        
     }

     private void PART_TextBox_Loaded(object sender, RoutedEventArgs e)
     {
         //这个具有特殊性, 第一次初始后, 需要记录这个对象, 才能在后面验证动画. 因为模板里面的名字引不出..
         dptx = (DatePickerTextBox)sender;
         dptx.Loaded -= PART_TextBox_Loaded;//干掉自己...
     }
     
     private void Re_eq_LostFocus(object sender, RoutedEventArgs e)  //检测库中存在的项,提供输入编辑信息
     {
            AllWT = myFuc.LoadWts();
            g_var.Bar.Tip = "正在录入";
             // 只有 站名+设备名+ 未销记的记录才有更新的必要.
             eq_name_count = (from g in AllWT where (g.设备名称 == Re_eq.Text && g.站名 == Rec_info.站名 && g.销记 == false) select g.设备名称).Count();
            
             if (eq_name_count>0)
             {
                 edit_tips.Content = "存在问题:(*请更新合并问题)";
                 edit_tips.Foreground = new SolidColorBrush(Colors.Red);
                 var entry = (from g in AllWT where (g.设备名称 == Re_eq.Text && Rec_info.站名 == g.站名 && g.销记 == false) select g).First();
                 Rec_info.存在问题 = entry.存在问题;
                 Rec_info.受理单位 = entry.受理单位;
                 Rec_info.问题编号 = entry.问题编号;
                 Rec_info.销记 = entry.销记;
                 Rec_info.发现时间 = entry.发现时间;
                 Rec_info.检查人 = entry.检查人;
                 Rec_info.天窗需求 = entry.天窗需求;
                 Rec_info.严重程度 = entry.严重程度;

                 Re_eq.LostFocus -= Re_eq_LostFocus; 
                  
             }
             else
             {                 
                 Rec_info.存在问题 = string.Empty;
                 Rec_info.受理单位 = string.Empty;
                 Rec_info.问题编号 = -1;
                 Rec_info.销记 = false;
                 Rec_info.发现时间 = null;
                 Rec_info.检查人 = string.Empty;
                 Rec_info.天窗需求 = string.Empty;
                 Rec_info.严重程度 = string.Empty;
                 
                 edit_tips.Content = "存在问题";
                 edit_tips.Foreground = new SolidColorBrush(Colors.Black);              
             }
             Re_rows.DataContext = null;
             Re_rows.DataContext = Rec_info;
             
                 
     }

    

     private void pfDetial_LoadingRow(object sender, DataGridRowEventArgs e)
     {
         PF wt = e.Row.DataContext as PF;
         if (wt==null)
         {
             return; 
         }
         if (myFuc.diffDays(DateTime.FromBinary(wt.lastEdit.Value),mNow)==0)
         {
             e.Row.Background = new SolidColorBrush(Colors.PaleGreen);
         }
         else
         {
             if (e.Row.AlternationIndex == 0)
             {
                 e.Row.Background = null;
             }
             else
             {
                 e.Row.Background = new SolidColorBrush(Colors.Azure);
             }
         }
     }

     private void TzdDetial_LoadingRow(object sender, DataGridRowEventArgs e)
     {
         TZD wt = e.Row.DataContext as TZD;
         if (wt==null)
         {
             return;
         }

         if (myFuc.diffDays(DateTime.FromBinary(wt.lastEdit.Value), mNow)== 0)
         {
             e.Row.Background = new SolidColorBrush(Colors.PaleGreen);
         }
         else
         {
             if (e.Row.AlternationIndex == 0)
             {
                 e.Row.Background = null;
             }
             else
             {
                 e.Row.Background = new SolidColorBrush(Colors.Azure);
             }
         }
     }

     private void pf_Edit(object sender, RoutedEventArgs e)
     {
        
     }

     private void pf_Del(object sender, RoutedEventArgs e)
     {
         PF pf = (PF)pfDetial.SelectedValue;
         myFuc.del_rec(pf.派发编号, "pf");
         GridUpdate("pf");
         GridUpdate("tzd");
     }

     private void tzd_Edit(object sender, RoutedEventArgs e)
     {

     }

     private void tzd_Del(object sender, RoutedEventArgs e)
     {
         TZD tzd = (TZD)TzdDetial.SelectedValue;
         myFuc.del_rec(tzd.流水号, "tzd");

         GridUpdate("tzd");
         GridUpdate("pf");
     }

     private void mainGrid_detail(object sender, RoutedEventArgs e)
     {
         WT xwt = (WT)mainGrid.SelectedValue;
         detail_show dtail = new detail_show(xwt.问题编号);
         dtail.Owner = this;
         dtail.ShowDialog();
     }        
     private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
     {
         if (e.OriginalSource is TabControl)
         {

             TabControl tb = (TabControl)e.OriginalSource;
             switch (tb.SelectedIndex)
             {  
                 case 0:
                     g_var.Bar.Tip = "查询";
                     list_zn = AllWT;
                     list_dowith = AllWT;
                     list_merge = AllWT;
                     list_unit = AllWT;
                     docWin.Visibility = Visibility.Collapsed;
                     docPF.Visibility = Visibility.Collapsed;
                     //...
                     view1.Visibility = Visibility.Visible;
                     break;
                 case 1:
                     g_var.Bar.Tip = "录入";
                     docWin.Visibility = Visibility.Collapsed;
                     docPF.Visibility = Visibility.Collapsed;
                     //...
                     view1.Visibility = Visibility.Visible;
                     break;
                 case 2:
                        g_var.Bar.Tip = "通知单";
                        view1.Visibility = Visibility.Collapsed;
                        docPF.Visibility = Visibility.Collapsed;
                        //...
                        docWin.Visibility = Visibility.Visible;                   
                        tzdTree.Items.Clear();
                        TzdTreeInit();
                        break;
                 case 3:
                        g_var.Bar.Tip = "整治表";
                      view1.Visibility = Visibility.Collapsed;
                      docWin.Visibility = Visibility.Collapsed;
                       //... 
                      docPF.Visibility = Visibility.Visible;
                      pfTree.Items.Clear();
                      pfTreeInit();

                      break;                
             }             
         }
         g_var.Bar.OtherTip = string.Empty;
     }

     private void docPfdataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
     {
         e.Row.Header  = e.Row.GetIndex() + 1;         
     }

     private void show_all(object sender, RoutedEventArgs e)
     {
         ShowWts swt = new ShowWts();
         swt.Show();
     }    
     private void savePf_MenuItem_Click(object sender, RoutedEventArgs e)
     {
         if (docPfdataGrid.ItemsSource==null)
         {
             return; 
         }
         HSSFWorkbook wk;
         string z_name="";
         string z_pfdate="";
         using (FileStream f = new FileStream(@"temp1.xls",FileMode.OpenOrCreate))
         {
             wk = new HSSFWorkbook(f);
             ISheet sh = wk.GetSheet("Sheet1");
             List<PF> x = (List<PF>)docPfdataGrid.ItemsSource;
             var zn = (from g in x select g.站名).First();
             var pftime = (from dd in x select dd.派发日期).First();
             z_name=zn;
             z_pfdate= pftime.HasValue==true ?  pftime.Value.ToString("yyyy年M月dd") : "unknown";

             IRow r = sh.GetRow(1);
             ICell c = r.GetCell(1);
             string note =
                 string
                 .Format("   {0}，经对{1}站结合部设备问题收集，存在下列问题，计划下周整治,请按计划时间准备人力、料具到现场共同配合",
                 pftime.Value.ToString("yyyy年M月d日"), zn);
             c.SetCellValue(note);   
             // 序号(3,0),设备名称(3,1),存在问题(3,2),整治方案(3,6),计划时间(3,7),整治情况(3.8),负责人(3.9).
             for (int i = 0; i < x.Count(); i++)
             {
                 
                 IRow row = sh.GetRow(3 + i);
                 row.GetCell(0).SetCellValue(i + 1);
                 row.GetCell(1).SetCellValue(x[i].设备名称);
                 row.GetCell(2).SetCellValue(x[i].存在问题);
                 row.GetCell(6).SetCellValue(x[i].保留2);
                 row.GetCell(7).SetCellValue(x[i].计划时间.Value);
             }

         }
         string tmpfn = System.IO.Path.GetTempFileName();
         string fn =System.IO.Path.ChangeExtension(tmpfn,"xls");
         FileInfo finfo = new FileInfo(tmpfn);
         finfo.MoveTo(fn);

         TempFile.Add(fn);
         using (FileStream s= new FileStream(fn,FileMode.OpenOrCreate))
         {
             wk.Write(s);
         }
         string curDir = System.Environment.CurrentDirectory;
         
         string saveName = string.Format(@"{0}/user/{1}站{2}整治表.xls",curDir, z_name, z_pfdate);

         using (FileStream savefn = new FileStream(saveName, FileMode.OpenOrCreate))
         {
             wk.Write(savefn);
         }
         Process.Start(fn);
         //string fn = g_var.getSaveDialog();
         //if (fn==null)
         //{
         //    return;
         //}
         //
         //
                          
         //XElement AllDoc = new XElement("记录集");
         //XElement doc = new XElement("re", "re");
         //foreach (var it in x)
         //{
         //    string dt1 = string.Empty;
         //    if (it.计划时间!=null)
         //    {
         //        dt1 = it.计划时间.Value.ToString("yyyy年M月d日");
         //    }
         //    doc =
         //       new XElement("Record",                        
         //               new XElement("设备名称", it.设备名称),
         //               new XElement("存在问题", it.存在问题),                        
         //               new XElement("整改方案", it.保留2),
         //               new XElement("计划时间", dt1),                        
         //               new XElement("整治情况"  ),
         //               new XElement("整治负责人")
         //               );
         //    AllDoc.Add(doc);
            
         //}
         //AllDoc.Save(fn);
     }

     private void main_grid_docXml(object sender, RoutedEventArgs e)
     {
         if (mainGrid.ItemsSource==null)
         {
             return;
         }
            
         
         List<WT> x = (List<WT>)mainGrid.ItemsSource;
         var zn = (from g in x select g.站名).First();
        // var dt = (from g in x select g.发现时间).First();
        // DateTime dt = DateTime.Now;

         //string fn = string.Format("./myDoc/{0}查询生成表.xml", dt.ToString("yyyy年M月d日"));

         string fn = g_var.getSaveDialog();
         if (fn==null)
         {
             return; 
         }
         
         XElement AllDoc = new XElement("记录集");
         XElement doc = new XElement("re", "re");
         
         foreach (var it in x)
         {
             string dt1=string.Empty;
             string dt2 =string.Empty;
             
             
             if (it.发现时间!=null)
             {
                 dt1 = it.发现时间.Value.ToString("yyyy年M月d日");
             }
             if (it.销记时间!=null)
             {
                 dt2 = it.销记时间.Value.ToString("yyyy年M月d日");
             }
             doc =
                new XElement("Record",
                        new XElement("站名",it.站名),
                        new XElement("设备名称", it.设备名称),
                        new XElement("存在问题", it.存在问题),
                        new XElement("发现时间",dt1),
                        new XElement("检查人",it.检查人),
                        new XElement("最后派发日期",it.rev1),
                        new XElement("整改方案", it.整改方案),
                        new XElement("销记",it.销记),
                        new XElement("销记时间", dt2),
                        new XElement("完成情况",it.完成情况),
                        new XElement("整治情况",it.整治情况),
                        new XElement("整治负责人",it.整治负责人)
                        );
             AllDoc.Add(doc);

         }
         AllDoc.Save(fn);
     }

     

     private void start_time_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
     {
         
         if (g_Query_res.Count()<1)
         {
             g_Query_res = myFuc.LoadWts();
         }
         if (start_time.SelectedDate!=null)
	        {
                if (Q_fx_flag.IsChecked==true)
                {
                    g_var.start_Q = (from g in g_Query_res
                                     where g.发现时间.Value >= start_time.SelectedDate.Value
                                     select g).ToList();
                }
                else
                {
                    g_var.start_Q = (from g in g_Query_res
                                     where g.销记时间!=null && g.销记时间.Value >= start_time.SelectedDate.Value
                                     select g).ToList();
                }
                 
                 if (g_var.end_Q.Count()<1)
                 {
                     mainGrid.ItemsSource = g_var.start_Q;
                     g_var.Bar.Query = g_var.start_Q.Count();
                 }
                 else
                 {
                     mainGrid.ItemsSource = g_var.start_Q.Intersect(g_var.end_Q).Distinct().ToList();
                     g_var.Bar.Query = g_var.start_Q.Intersect(g_var.end_Q).Distinct().Count();
                 }
                
                 
	        }
         
     }

     private void end_time_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
     {
         if (g_Query_res.Count()<1)
         {
             g_Query_res = myFuc.LoadWts();
         }
         if (end_time.SelectedDate!=null)
         {
             if (Q_fx_flag.IsChecked==true)
             {
                 g_var.end_Q = (from g in g_Query_res
                                where g.发现时间.Value <= end_time.SelectedDate.Value
                                select g).ToList();
             }
             else
             {
                 g_var.end_Q = (from g in g_Query_res
                                where g.销记时间!=null && g.销记时间.Value <= end_time.SelectedDate.Value
                                select g).ToList();
             }
             
             if (g_var.start_Q.Count()<1)
             {
                 mainGrid.ItemsSource = g_var.end_Q;
                 g_var.Bar.Query = g_var.end_Q.Count();
             }
             else
             {
                 mainGrid.ItemsSource = g_var.end_Q.Intersect(g_var.start_Q).Distinct().ToList();
                 g_var.Bar.Query = g_var.end_Q.Intersect(g_var.start_Q).Distinct().Count();
             }
             
            
         }
     }

     private void Like_Q_Click(object sender, RoutedEventArgs e)
     {
         if (g_Query_res.Count() < 1)
         {
             g_Query_res = myFuc.LoadWts();
         }
         string boxStr = Qu_like.Text.ToUpperInvariant();
         if (boxStr=="内容模糊查询"||boxStr==string.Empty||boxStr==" ")
         {
             return;
         }         
         List<WT> tmp = (from g in g_Query_res
                                where g.设备名称.Contains(boxStr) == true || g.存在问题.Contains(boxStr)
                                select g).ToList();
         mainGrid.ItemsSource = tmp;

         g_var.Bar.Query = tmp.Count();
         
         
     }

     private void Qu_like_GotFocus(object sender, RoutedEventArgs e)
     {
         if (Qu_like.Text=="内容模糊查询")
         {
             Qu_like.Text = "";
             Qu_like.Foreground = new SolidColorBrush(Colors.Black);
         }
     }

     private void Qu_like_LostFocus(object sender, RoutedEventArgs e)
     {
         if (Qu_like.Text=="")
         {
             Qu_like.Text = "内容模糊查询";
             Qu_like.Foreground = new SolidColorBrush(Colors.LightGray);
         }
     }
       
     private void tzdContent_TextChanged(object sender, TextChangedEventArgs e)
     {
         //
         tzdContent.Document.BringIntoView();
     }

     private void Button_Click(object sender, RoutedEventArgs e)
     {
         
         if (tzdContent.FontSize>30)
         {
             return;
         }
         tzdContent.FontSize++;
     }

     private void Button_Click_1(object sender, RoutedEventArgs e)
     {
         if (tzdContent.FontSize<10)
         {
             return;
         }
         tzdContent.FontSize--;
     }

     private void Button_Click_2(object sender, RoutedEventArgs e)
     {
         if (docPfdataGrid.FontSize>30)
         {
             return;
         }
         docPfdataGrid.FontSize++;
     }

     private void Button_Click_3(object sender, RoutedEventArgs e)
     {
         if (docPfdataGrid.FontSize<10)
         {
             return;
         }
         docPfdataGrid.FontSize--;
     }

     private void mainGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
     {
         WT x =(WT)mainGrid.SelectedValue;
         string pfSate = string.Empty;
         if (x!=null)
         {
             using (jhbEntities ctx =new jhbEntities())
             {
                 List<PF>  pfInfo = (from g in ctx.PFs
                              where g.问题编号 == x.问题编号
                              orderby g.派发日期 descending
                              select g).ToList();
                 if (pfInfo.Count()>0)
                 {
                     PF singlePf = (from g in pfInfo select g).First();                     
                     g_var.Bar.OtherTip = 
                         string.Format("{0}#记录最后一次派发日期:{1}({2})   共派发过{3}次!", 
                         singlePf.问题编号,
                         singlePf.派发日期.Value.ToString("yyyy年M月d日"),
                         myFuc.IsthisWeek(singlePf.派发日期.Value),
                         pfInfo.Count()
                         );
                 }
                 else
                 {
                     g_var.Bar.OtherTip = "未派发!";
                 }
                 
                
             }
              
         }
     }

     private void mainGrid_LostFocus(object sender, RoutedEventArgs e)
     {
         g_var.Bar.OtherTip = "";
     }

     private void Expander_Expanded(object sender, RoutedEventArgs e)
     {
         start_time.SelectedDate = null;
         end_time.SelectedDate = null;
     }

     private void pageNext(object sender, RoutedEventArgs e)
     {
         // --这里实现分页装载!!!
         tzdContent.Document.Blocks.Clear();
         string[] tmpStrARR = myFuc.tzdPageList.ToArray<string>();
         string textStr = string.Empty;
         for (int i = myFuc.Itsum; i < tmpStrARR.Count(); i++)
         {
             textStr = textStr + tmpStrARR[i];
         }
         tzdContent.AppendText(textStr); 
     }

     private void pagePre(object sender, RoutedEventArgs e)
     {
         tzdContent.Document.Blocks.Clear();
         string[] tmpStrARR = myFuc.tzdPageList.ToArray<string>();
         string textStr = string.Empty;
         for (int i = 0; i < myFuc.Itsum; i++)
         {
             textStr = textStr + tmpStrARR[i];
         }
         tzdContent.AppendText(textStr); 
     }

     private void ItemCount_SelectionChanged(object sender, SelectionChangedEventArgs e)
     {
         myFuc.Itsum = int.Parse(ItemCount.SelectedValue.ToString());
         //Console.WriteLine(myFuc.Itsum);
     }

     private void show_pf(object sender, RoutedEventArgs e)
     {
         pfQueryOutput pfWin = new pfQueryOutput();
         pfWin.Show();
     }

     private void Window_Closing(object sender, CancelEventArgs e)
     {
         foreach (var item in TempFile)
         {
             try
             {
                 System.IO.File.Delete(item);
             }
             catch (Exception)
             {
                 return;                     
             }
             
         }
     }

     private void saveTzd_MenuItem_Click(object sender, RoutedEventArgs e)
     {
         HSSFWorkbook wk;
         using (FileStream f=new FileStream(@"temp2.xls",FileMode.OpenOrCreate))
         {
             wk = new HSSFWorkbook(f);
             ISheet sh = wk.GetSheet("Sheet1");
             IRow r = sh.GetRow(1);
             r.GetCell(8).SetCellValue(tzddocDate.Text);
             r = sh.GetRow(2);
             r.GetCell(2).SetCellValue(tzddocUnit.Text);
             r.GetCell(5).SetCellValue(tzddoccz.Text);
             r = sh.GetRow(4);
             r.GetCell(0).SetCellValue(Tcontent);
         }  
         string fntmp=System.IO.Path.GetTempFileName(); // get temp file and create tmpfile.
         string fnXls = System.IO.Path.ChangeExtension(fntmp, "xls");   // rename to this filename            
         TempFile.Add(fnXls);  // record  this filename to delete it when app  close...
         FileInfo finfo = new FileInfo(fntmp);   //  rename .
         finfo.MoveTo(fnXls);        
         using (FileStream s= new FileStream(fnXls,FileMode.OpenOrCreate))
         {
             wk.Write(s);
         }
         string curDir = System.Environment.CurrentDirectory;         
         string tzdNameSave = string.Format(@"{0}/user/{1}站{2}配合作业通知单.xls", curDir, tzddoccz.Text, tzddocDate.Text);
         using (FileStream saveTzd = new FileStream(tzdNameSave, FileMode.OpenOrCreate))
         {
             wk.Write(saveTzd);
         }

         Process.Start(fnXls);         
     }       
    }          
}
