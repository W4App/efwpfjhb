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
    /// xjWin.xaml 的交互逻辑
    /// </summary>
    public partial class xjWin : Window
    {
        private WT _wt;
        private PF _pf;
        

        private Int64 _wtid;
        private string _zn;

        List<string> x3man = new List<string>();
        List<string> x2state = new List<string>();
      
        public xjWin(Int64 wid,string zn)
        {
            InitializeComponent();
            _wt = new WT();
            _wtid = wid;
            _zn = zn;
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using (jhbEntities ctx= new jhbEntities())
            {
                _wt = (from g in ctx.WTs 
                       where g.问题编号 == _wtid && g.站名 == _zn 
                       select g).Single();
                try
                {
                    var tmp_pf = (from g in ctx.PFs
                                  where g.问题编号 == _wtid
                                  orderby g.派发编号 descending
                                  select g);
                    if (tmp_pf.Count()>0)
                    {
                        _pf = tmp_pf.First();
                    }
                    else
                    {
                        _pf=null;
                    }                                       
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }  

                        
                var man = (from g in ctx.WTs
                           where g.站名 == _zn
                           select g.整治负责人).Distinct();           
                              //Replace("\r", string.Empty).Replace("\n", string.Empty)
                var state = (from g in ctx.WTs where g.站名 == _zn select g.整治情况).Distinct();               
                foreach (var ix in man)
                {
                    if (ix!=null)
                    {
                        string tx = ix.Replace("\r", string.Empty).Replace("\n", string.Empty);
                        if (tx!=string.Empty)
                        {
                            x3man.Add(tx);   
                        }
                                                                     
                    }                    
                }                
                foreach (var iy in state)
                {
                    if (iy!=null)
                    {
                        string ty = iy.Replace("\r", string.Empty).Replace("\n", string.Empty);
                        if (ty!=string.Empty)
                        {
                            x2state.Add(ty); 
                        }
                        
                    }                    
                }
                x3.ItemsSource = x3man.Distinct();
                x2.ItemsSource = x2state.Distinct();
                wtTip.Content = string.Format("{0}站 => {1}: {2}",_wt.站名 , _wt.设备名称 , _wt.存在问题);
                if (_pf==null)
                {
                    pfTip.Content = "没有派发信息!";
                }
                else
                {
                    pfTip.Content = string.Format("[{0}]通知 [{1}] 处理.", _pf.派发日期.Value.ToString("yyyy年M月d日") , _pf.受理单位);
                }
                
            }
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (x2.Text==null||x2.Text==string.Empty)
            {
                Valid_Ani va = new Valid_Ani();
                Ani_x2.BeginAnimation(SolidColorBrush.ColorProperty, va.color_Ani);
                return;
            }
            if (x3.Text==null||x2.Text==string.Empty)
            {
                Valid_Ani vb = new Valid_Ani();
                Ani_x3.BeginAnimation(SolidColorBrush.ColorProperty, vb.color_Ani);
                return;
            }
            if (x4.SelectedDate==null||x4.Text==string.Empty)
            {
                Valid_Ani vc = new Valid_Ani();
                Ani_x4.BeginAnimation(SolidColorBrush.ColorProperty, vc.color_Ani);
                return;
            }
            using (jhbEntities ctx = new jhbEntities())
            {
                var wt = (from g in ctx.WTs where g.站名 == _zn && g.问题编号 == _wtid select g).Single();
                ctx.Entry(wt).Entity.销记时间 = x4.SelectedDate.Value.Date;
                ctx.Entry(wt).Entity.完成情况 = x1.Text;
                ctx.Entry(wt).Entity.整治情况 = x2.Text;
                ctx.Entry(wt).Entity.整治负责人 = x3.Text;
                ctx.Entry(wt).Entity.lastEdit = myFuc.getNowDateHashcode();
                ctx.Entry(wt).Entity.销记 = true;

                ctx.Entry(wt).Entity.OpTrack = "X";
                g_var.g_xj_flag = true; //窗口返回...
                g_var.g_chg_wt = ctx.Entry(wt).Entity;


                var pf = from g in ctx.PFs where g.站名 == _zn && g.问题编号 == _wtid select g;
                foreach (var pfitem in pf)
                {
                    ctx.Entry(pfitem).Entity.ver1 = x4.SelectedDate.Value.Date.ToString("M月d日"); //销记时间
                    ctx.Entry(pfitem).Entity.ver2 = x1.Text; //完成情况
                    ctx.Entry(pfitem).Entity.ver3 = x2.Text; //整治情况
                    ctx.Entry(pfitem).Entity.var4 = x3.Text; //负责人
                }

                ctx.SaveChanges();

            }
            this.Close();
        }  
        private void x2_GotFocus(object sender, RoutedEventArgs e)
        {
            x2.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        private void x3_GotFocus(object sender, RoutedEventArgs e)
        {
            x3.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        private void x4_GotFocus(object sender, RoutedEventArgs e)
        {
            x4.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Close();
        }

             
    }
}
