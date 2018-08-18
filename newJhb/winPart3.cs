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

namespace newJhb
{
    public partial class MainWindow : Window
    {
        private void wt_insert_new()  //插入新记录
        {
            using (jhbEntities ctx = new jhbEntities())
            {

                WT wt_in = new WT();
                wt_in.站名 = Rec_info.站名.Trim();
                wt_in.设备名称 = Rec_info.设备名称.Trim();
                wt_in.存在问题 = Rec_info.存在问题;
                wt_in.发现时间 = Rec_info.发现时间.Value.Date;
                if (Rec_info.检查人!=null)
                {
                    wt_in.检查人 = Rec_info.检查人.Trim();
                }
                else
                {
                    wt_in.检查人 = string.Empty;
                }
                
                wt_in.天窗需求 = Rec_info.天窗需求;
                wt_in.严重程度 = Rec_info.严重程度;
                if (Rec_info.受理单位!=null)
                {
                    wt_in.受理单位 = Rec_info.受理单位.Trim();
                }
                else
                {
                    wt_in.设备名称 = string.Empty;
                }
                wt_in.备注 = Rec_info.备注;
                wt_in.销记 = false;
                wt_in.lastEdit = myFuc.getNowDateHashcode();
                wt_in.OpTrack = "NE";
                ctx.WTs.Add(wt_in);
                ctx.SaveChangesAsync();
                //Console.WriteLine("----插入新记录");
                //ctx.SaveChanges();                
            }
        }
        private void wt_update()  // 插入时候更新记录
        {
            using (jhbEntities ctx = new jhbEntities())
            {
                var x = (from g in ctx.WTs where g.问题编号==Rec_info.问题编号 select g).First();
                var enty = ctx.Entry(x).Entity;                
                enty.备注 = Rec_info.备注;
                enty.存在问题 = Rec_info.存在问题.Trim();
                enty.发现时间 = Rec_info.发现时间.Value.Date;
                if (Rec_info.检查人!=null)
                {
                    enty.检查人 = Rec_info.检查人.Trim();
                }

                if (Rec_info.受理单位!=null)
                {
                    enty.受理单位 = Rec_info.受理单位.Trim();
                }
                enty.天窗需求 = Rec_info.天窗需求;
                enty.销记 = Rec_info.销记;
                enty.严重程度 = Rec_info.严重程度;
                enty.lastEdit = TodayBincode;
                enty.OpTrack = "UP";
                //Console.WriteLine("更新---");
                ctx.SaveChangesAsync();
                //ctx.SaveChanges();
            }
        }

        private void GridUpdate(string tableName)  // 读数据库, 刷新相应的表
        {
            switch (tableName)
            {
                case "wt":
                    mainGrid.ItemsSource = null;
                    using (jhbEntities ctx1 = new jhbEntities())
                    {
                         AllWT = (from g in ctx1.WTs                                                
                                                orderby g.发现时间 descending
                                                select g).ToList();
                         mainGrid.ItemsSource = AllWT.Where(i=>i.销记==false).ToList();
                    }
                    break;
                case "pf":
                    pfDetial.ItemsSource = null;
                     using (jhbEntities ctx2= new jhbEntities())
                     {
                        AllPF = (from g in ctx2.PFs 
                                                orderby g.派发日期 descending 
                                                select g).ToList();
                        pfDetial.ItemsSource = AllPF;
                     }  
                    break;
                case "tzd":
                    TzdDetial.ItemsSource = null;
                    using(jhbEntities ctx3= new jhbEntities())
	                    {
		                    AllTZD = (from g in ctx3.TZDs 
                                                      orderby g.派单日期 descending 
                                                      select g).ToList();
                            TzdDetial.ItemsSource = AllTZD;
	                    }
                    break;                
            }
            List<DateTime?> pfDt = (from g in AllPF orderby g.派发日期 descending select g.派发日期).Distinct().ToList();
            this.pfSelect.ItemsSource = pfDt;
            List<DateTime?> tzdDt = (from g in AllTZD orderby g.派单日期 descending select g.派单日期).Distinct().ToList();
            this.tzdSelect.ItemsSource = tzdDt;           
        }
        private void set_main_rec_mode(string zn,bool flag) // 输入时 排列主窗显示
        {
            using (jhbEntities ctx = new jhbEntities())
            {
                var x = from g in ctx.WTs where g.站名 == zn && g.销记 == flag orderby g.问题编号 descending select g;
                mainGrid.ItemsSource = x.ToList();
            }
        }
        private void MainGrid_del(object sender, RoutedEventArgs e) //主记录删除
        {
            WT xWt = (WT)mainGrid.SelectedValue;

            myFuc.del_rec(xWt.问题编号, "wt");
            mainGrid.ItemsSource= myFuc.LoadWts();           
        }      
    }
}
