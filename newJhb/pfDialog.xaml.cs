using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Globalization;
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
    /// pfDialog.xaml 的交互逻辑
    /// </summary>
   
    public partial class pfDialog : Window
    {
        private Int64 _Id;
        private string _Zn;
        private WT _wt;                
        public pfDialog(Int64 wtID,string Zn)
        {
            InitializeComponent();
            _Id = wtID;
            _Zn=Zn;            
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {            
            using(jhbEntities ctx= new jhbEntities())
            {
                List<string> unit_info = (from g in ctx.WTs where g.站名 == _Zn select g.受理单位).Distinct().ToList();
                List<string> fix_info = (from g in ctx.WTs where g.站名 == _Zn select g.整改方案).Distinct().ToList();
                pdUnit.ItemsSource = unit_info.TakeWhile<string>(r=>r!=string.Empty);
                PlanFix.ItemsSource = fix_info.TakeWhile<string>(r=>r!=string.Empty);
                //根据编号(唯一)返回一条记录.
                _wt = (from g in ctx.WTs where g.问题编号 == _Id select g).First();                
                pdUnit.Text = _wt.受理单位;
                PlanFix.Text = _wt.整改方案;
                run_1.Text = string.Format("设备名称:   {0}",_wt.设备名称);
                run_2.Text = string.Format("存在问题:   {0}", _wt.存在问题);
                run_3.Text = string.Format("受理单位:   {0}", _wt.受理单位);
            }
            

        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {            
            if (!pf_input_valid())
            {
                return;
            }            
            //--------------- 验证完成后, 需要把日期格式统一转成约定格式
           
            //快照 输入字段
            DateTime? pd_dt = pdDate.SelectedDate.Value.Date;
            DateTime? plan_dt = planDate.SelectedDate.Value.Date;
            //---------------
            using(jhbEntities ctx= new jhbEntities())
            {
                var en_wt = (from g in ctx.WTs where g.问题编号 == _Id select g).Single(); //唯一记录., 记录编号检索
                var enty_wt = ctx.Entry(en_wt).Entity;                
                enty_wt.lastEdit = myFuc.getNowDateHashcode();
                enty_wt.受理单位 = pdUnit.Text;
                enty_wt.整改方案 = PlanFix.Text;
                enty_wt.OpTrack = "PF";
                enty_wt.rev1 = pd_dt.Value.ToString("yyyy年M月d日"); //记录最后一次派发日期.
                //--------------- 生成派发表.  同一天, 同站名, 同设备问题, 同受理单位 视为更新!  g.派发日期.Value - pd_dt.Value)
                var en_pf = from g in ctx.PFs
                            where g.站名 == _Zn && g.问题编号 == _Id && g.派发日期==pd_dt
                            select g;
                
                switch (en_pf.Count())
                {
                    case 0: //新生
                        PF nPf = new PF();
                        nPf.lastEdit = myFuc.getNowDateHashcode();
                        nPf.存在问题 = _wt.存在问题;
                        nPf.计划时间 = plan_dt;   //planDate.Text.ToString("yyyy-M-d")
                        nPf.派发日期 =pd_dt ;  // this.pdDate.Text;
                        nPf.设备名称 = _wt.设备名称;
                        nPf.受理单位 = pdUnit.Text;
                        nPf.天窗需求 = _wt.天窗需求;
                        nPf.问题编号 = _Id;
                        nPf.站名 = _Zn;
                        nPf.保留2 = PlanFix.Text;
                        ctx.PFs.Add(nPf);
                       // int x = await ctx.SaveChangesAsync();
                        //ctx.SaveChanges();
                        
                        break;
                    case 1: // 更新
                        var pf_entry_row = en_pf.Single();  //实体
                        var entry = ctx.Entry(pf_entry_row).Entity; // 入口
                        entry.lastEdit = myFuc.getNowDateHashcode();
                        entry.存在问题 = _wt.存在问题;  
                        entry.计划时间 = plan_dt;  //planDate.Text.ToString("yyyy-M-d")
                        
                        entry.设备名称 = _wt.设备名称;
                        entry.受理单位 = pdUnit.Text;
                        entry.天窗需求 = _wt.天窗需求;
                        entry.保留2 = PlanFix.Text;
                        //ctx.SaveChanges();
                        
                        break;

                   
                }
                

                //---------- 生成通知单表 -- 根据日期,站名,受理单位三个记录条件找到记录, 更新记录字段..
                var en_tzd = from g in ctx.TZDs
                             where g.派单日期 == pd_dt && g.站名 == _Zn && g.受理单位 == pdUnit.Text
                             select g;
                switch (en_tzd.Count())
                {
                    case 0:
                        // 插入新通知单
                        TZD new_tzd = new TZD();
                        new_tzd.派单日期 = pd_dt;
                        new_tzd.记录 = "\n"+"^"+_wt.设备名称+": "+ _wt.存在问题+" ("+plan_dt.Value.ToString("M月d日")+")";
                        new_tzd.lastEdit = myFuc.getNowDateHashcode();
                        new_tzd.受理单位 = pdUnit.Text;
                        new_tzd.站名 = _Zn;

                        ctx.TZDs.Add(new_tzd);

                        //ctx.SaveChanges();
                        break;
                    case 1:
                        //更新通知单
                        var tzd_enty_row = en_tzd.Single();
                        string oldRec = tzd_enty_row.记录;
                        if (oldRec.Contains(_wt.设备名称 + ": " + _wt.存在问题) == false)
	                        {
                                ctx.Entry(tzd_enty_row).Entity.记录 = oldRec  +"\n"+ "^" + _wt.设备名称 + ": " + _wt.存在问题;
                                ctx.Entry(tzd_enty_row).Entity.lastEdit = myFuc.getNowDateHashcode();
                                ctx.SaveChangesAsync();
	                        }                                                
                        break;
                    default:
                        Console.WriteLine("跑飞了....");
                        break;
                }
                ctx.SaveChangesAsync();
                //ctx.SaveChanges();

                g_var.g_chg_wt = enty_wt;

            }
            g_var.g_pf_flag = true;
            this.Close();

        }

        private bool pf_input_valid()
        {
            if (pdDate.SelectedDate == null || pdDate.Text == string.Empty)
            {
                Valid_Ani va1 = new Valid_Ani();
                Ani_pdDate.BeginAnimation(SolidColorBrush.ColorProperty, va1.color_Ani);
                return false;
            }
            if (pdUnit.Text == null || pdUnit.Text == string.Empty)
            {
                Valid_Ani va2 = new Valid_Ani();
                Ani_pdUnit.BeginAnimation(SolidColorBrush.ColorProperty, va2.color_Ani);
                return false;
            }
            if (planDate.SelectedDate == null || planDate.Text == string.Empty)
            {
                Valid_Ani va3 = new Valid_Ani();
                Ani_planDate.BeginAnimation(SolidColorBrush.ColorProperty, va3.color_Ani);
                return false;
            }
            if (PlanFix.Text == null || PlanFix.Text == string.Empty)
            {
                Valid_Ani va4 = new Valid_Ani();
                Ani_planFix.BeginAnimation(SolidColorBrush.ColorProperty, va4.color_Ani);
                return false;
            }
            return true;
        }  //验证函数
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void pdDate_GotFocus(object sender, RoutedEventArgs e)
        {
            Ani_pdDate.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        private void pdUnit_GotFocus(object sender, RoutedEventArgs e)
        {
            Ani_pdUnit.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        private void planDate_GotFocus(object sender, RoutedEventArgs e)
        {
            Ani_planDate.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

        private void PlanFix_GotFocus(object sender, RoutedEventArgs e)
        {
            Ani_planFix.BeginAnimation(SolidColorBrush.ColorProperty, null);
        }

    }
}
