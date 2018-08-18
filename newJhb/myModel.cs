using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Entity;
using System.Windows.Media.Animation;
using System.Data.SQLite;
using System.Windows.Media;
using System.Windows;
using System.Data.Entity.Infrastructure;
using Microsoft.Win32;

namespace newJhb
{

    static class g_var
    {
        //定时器事件...
        public static void timeDo(object o)
        {
          
            DateTime dt = DateTime.Now;
            g_var.Bar.Clock = dt.ToString("H:mm:ss  yyyy年M月d日");
        }
        //状态条..
        public static SateBarTip Bar = new SateBarTip();
        //- 记录2个时间查询结果,在不同的事件中使用.
        public static List<WT> start_Q = new List<WT>();
        public static List<WT> end_Q = new List<WT>();

        //----------
        public static bool g_pf_flag=false;   // 因为wpf的自定义对话框没有返回状态. 所以设置检查此标志来更新状态
        public static bool g_xj_flag=false;
        public static WT g_chg_wt = new WT(); // 改变的行记录. 用于刷新
        public  static void tableValid()
        {
            using (jhbEntities ctx= new jhbEntities())
            {
                Bar.Total = ctx.WTs.Count();

                foreach (var item in ctx.WTs)
                {
                    DateTime dtLast = DateTime.FromBinary(item.lastEdit.Value);
                    DateTime dtNow = DateTime.FromBinary(myFuc.getNowDateHashcode());
                    int days = myFuc.diffDays(dtLast,dtNow);
                    if ( Math.Abs(days)>30 )
	                 {
                         item.OpTrack = "•••";
                          
	                 }
                    else
                    {
                        item.OpTrack = days.ToString();
                    }
                     
                }
                ctx.SaveChanges();               
            }
        }
        public static string getSaveDialog()
        {
            SaveFileDialog fndialog = new SaveFileDialog();
            fndialog.AddExtension = true;
            fndialog.Filter = "pseudo excel (*.xls)|*.xls|xml files (*.xml)|*.xml |All files (*.*)|*.*";
            fndialog.DefaultExt = ".xls";
            fndialog.FilterIndex = 1;
            fndialog.OverwritePrompt = true;

            if (fndialog.ShowDialog().Value)
            {
               return fndialog.FileName;
            }
            return null;
        }
        //---------- 导出信息-----------
        //public static DateTime patchDate = new DateTime();
    }
    static class myFuc
    {
        public static List<string> tzdPageList = new List<string>();
        public static int Itsum = 13;



        public static string IsthisWeek(DateTime dt)
        {
            DateTime nw = DateTime.Now;
            int dayofWk = (int)nw.DayOfWeek;
            int day = diffDays(nw, dt);
            if (day - dayofWk > 7 || day<0)
            {
                return "*";
            }
            if (day - dayofWk > 0 && day - dayofWk <= 7)
            {
                return "上周";
            }
            else
            {
                return "本周";
            }
        }
        public static List<TZD> LoadTzds()
        {
            using (jhbEntities ctx= new jhbEntities())
            {
                return (from g in ctx.TZDs orderby g.流水号 descending select g).ToList();
            }
        }
        public static List<PF> LoadPfs()
        {
            using (jhbEntities ctx= new jhbEntities())
            {
                return (from g in ctx.PFs orderby g.派发编号 descending select g).ToList();
            }
        }
        public static List<WT> LoadWts()
        {

            using (jhbEntities ctx= new jhbEntities())
            {
                List<WT> tmp=(from g in ctx.WTs orderby g.问题编号 descending select g).ToList();
                g_var.Bar.Total = tmp.Count();
                return tmp;
            }
        }
       /// <summary>
        /// 日期函数: 相差的日子数 按照早晚指定参数,得正数...
        /// </summary>
        /// <param name="later"></param>
        /// <param name="early"></param>
        /// <returns></returns>
        public static int diffDays(DateTime later, DateTime early)
        {
            int y = later.Year - early.Year;
            int d = later.DayOfYear - early.DayOfYear;
            return y * 360 + d;

        }
        /// <summary>
        /// 传入 唯一索引 及 记录名
        /// </summary>
        
        public static void del_rec(Int64 id, string table_name)
        {
            if (MessageBox.Show("确定删除本条记录?","警告!",MessageBoxButton.YesNo,MessageBoxImage.Warning,MessageBoxResult.No)==MessageBoxResult.No)
            {
                return;
            }
            using (jhbEntities ctx = new jhbEntities()) 
            {
                switch (table_name)
                {
                    case "wt":
                         var e1 = (from g in ctx.WTs where g.问题编号 == id select g).First();
                         ctx.WTs.Remove(e1);
                         ctx.SaveChanges();
                        break;
                    case "pf":
                         var e2 = (from g in ctx.PFs where g.派发编号 == id select g).First();
                        //删除派发, 记录站名,派发日期,存在问题  转去删除以派发日期为条件的tzd里面的 存在问题.
                         var enTzd = (from g in ctx.TZDs
                                      where g.派单日期.Value == e2.派发日期.Value && g.站名 == e2.站名
                                      orderby g.流水号 descending
                                      select g).First();
                         
                         string tmp_wtdetail = enTzd.记录.Replace("\n"+"^" + e2.设备名称 + ": " + e2.存在问题, "");

                       

                         if (tmp_wtdetail.Replace("\n",string.Empty)==string.Empty||tmp_wtdetail==null)
                         {
                             ctx.TZDs.Remove(enTzd);
                         }
                         else
                         {
                             ctx.Entry(enTzd).Entity.记录 = tmp_wtdetail;
                         }

                         ctx.PFs.Remove(e2);
                         ctx.SaveChanges();
                        break;
                    case "tzd":
                         var e3 = (from g in ctx.TZDs where g.流水号 == id select g).First();
                         //根据tzd里面的站名和派单日期. 删除派发的记录项目.
                         var enPf = from g in ctx.PFs
                                    where g.站名 == e3.站名 && g.派发日期.Value == e3.派单日期.Value && g.受理单位==e3.受理单位
                                    select g;
                         foreach (var item in enPf)
                         {
                             ctx.PFs.Remove(item);
                         }
                         ctx.TZDs.Remove(e3);                         
                         ctx.SaveChanges();
                        break;
                }
            }
          
        }                     
        public static long getNowDateHashcode()
        {
            DateTime dt = DateTime.Now.Date;

            return dt.ToBinary();
        }        
        /// <summary>
        /// 通知单内容处理, 实现序号生成, 行数统计. 如果行数大于15,  就强制缩减字号. 确保一页搞定 
        /// </summary>
        /// <param name="tzdRec"></param>
        /// <returns></returns>
        public static string tzdStringFmt(string tzdRec,out int rowCnt)
        {
            int cnt = tzdRec.Count(i => i == '^'); //派发的时候插入的格式化字符.每格条目都有一个
            int s = 0;

            for (int i = 0; i < cnt; i++)   //把条目的第一个"^"符号前面插入序号.
            {
                int pos = tzdRec.IndexOf("^", s);
                tzdRec = tzdRec.Insert(pos, (i + 1).ToString());

                s = pos + (i + 1).ToString().Length + 1;
            }            
            rowCnt = cnt;  // 输出计数, 预留未使用
            return tzdRec;
            //return tzdRec.Replace("^", ". ");  // 格式化, 把"^"替换成圆点和一个空格.
        }

        public static List<string>  Tzd2List(string tzd)
        {
           // Console.WriteLine(tzd);
            string[] tmpArr = tzd.Split('^');
            
            for (int i = 1; i < tmpArr.Count(); i++)
            {
                tmpArr[i] = tmpArr[i].Insert(0, (i).ToString()+". ");
               // Console.WriteLine(tmpArr[i]);
            }
            return  tmpArr.ToList();
        }
    }
    /// <summary>
    /// 树形界面初始化用 结构
    /// </summary>
    class mTreeInfo   // 传递树形结构所需的类. 记录数节点的类型, 显示信息. 派发列表, 通知单列表
    {
        public int flag { get; set; } //1->日期, 2->站名,3->受理单位, 0->无效
        public string Info { get; set; }
        public List<PF> listPf { get; set; } // 记录节点的内容, 传递到事件. 由此处理,显示之类的
        public List<TZD> listTzd { get; set; } // 和上面相同 
        // 考虑. 这2个列表不大, 本身在内存中, 不需要读取数据库, 所以记录到节点信息中.
    }
    class wtReInfo :WT  // 这个类派生自WT类, 只是为了输入记录用, 提供诸如信息提示, 重复记录显示之类的功能.这些事WT所没有的.
    {
        //------自动匹配信息----- 一切为了减少输入工作量! 所以, 所有的输入都会尽力的简单好用.
        //站名关联检查人,设备名称 
        private List<string> _str_checker; // 检查人输入提示
        private List<string> _str_eqName;  //设备名输入提示
        private List<string> _str_unit;     // 配合单位名称输入提示.   
        
        public List<string> str_checker { get { return _str_checker; } }
        public List<string> str_eqName { get { return _str_eqName; } }
        public List<string> str_unit { get { return _str_unit; } }                 
        public void  setListInfo(string zhName) //指定站名过滤 文本搜索.
        {
            using( jhbEntities ctx=new jhbEntities()  )
            {
                _str_checker = (from j in ctx.WTs where j.站名 == zhName select j.检查人).Distinct().ToList();               
                _str_eqName = (from j in ctx.WTs where j.站名 == zhName select j.设备名称).Distinct().ToList();
                _str_unit = (from j in ctx.WTs where j.站名 == zhName select j.受理单位).Distinct().ToList();
            }
        }
       
        public wtReInfo() //构造函数  
        {
            
            using (jhbEntities ctx = new jhbEntities()) 
            {
                _str_checker = (from j in ctx.WTs  select j.检查人).Distinct().ToList();               
                _str_eqName = (from j in ctx.WTs  select j.设备名称).Distinct().ToList();
                _str_unit = (from j in ctx.WTs select j.受理单位).Distinct().ToList();                
            }
        }
        
       

    }

    class storeWtRec  //输入处理的关键部分
    {
        private wtReInfo _Rec_info;
        public wtReInfo getWtRec()
        {
            return _Rec_info;
        }
        public storeWtRec()
        {
            _Rec_info = new wtReInfo();
            _Rec_info.销记 = false;
            _Rec_info.lastEdit = myFuc.getNowDateHashcode();
        }
        //调试---
        public void DebugPrint()
        {
            Console.WriteLine("|编号:{0}|站名:{1}|设备名称:{2}|存在问题:{3}|发现时间:{4}|检查人:{5}|天窗:{6}|程度:{7}|受理单位:{8}|备注:{9}",
                               _Rec_info.问题编号,
                               _Rec_info.站名,
                               _Rec_info.设备名称,
                               _Rec_info.存在问题,
                               _Rec_info.发现时间,
                               _Rec_info.检查人,
                                _Rec_info.天窗需求,
                               _Rec_info.严重程度,
                               _Rec_info.受理单位,
                               _Rec_info.备注
                                          
                                );
            Console.WriteLine("|销记:{0}|销记时间:{1}|整治人:{2}|lastedit:{3}",
                               _Rec_info.销记,
                               _Rec_info.销记时间,
                               _Rec_info.整治负责人,
                                _Rec_info.lastEdit  
                                );

        }
        public void Re_reset(string zn)
        {
            _Rec_info = new wtReInfo();
            _Rec_info.站名 = zn;
        }
                
    }
    class Valid_Ani //先搞个固定颜色吧.
    {
        private ColorAnimation _color_Ani;
        //private DoubleAnimation _db_Ani;
        public Valid_Ani()
        {
            _color_Ani = new ColorAnimation();
            colorAni_init();

           // _db_Ani = new DoubleAnimation();
        }
        public ColorAnimation color_Ani 
        { 
            get { return _color_Ani; } 
            //set { _color_Ani = value; } 
        }
        //public DoubleAnimation db_Ani
        //{
        //    get { return _db_Ani; }
        //    set { _db_Ani = value; }
        //}
        private void colorAni_init() //动画初始化
        {
            Color c = new Color();
            c = Colors.Violet;
            _color_Ani.From = Colors.White;
            _color_Ani.To = c;
            _color_Ani.Duration = TimeSpan.FromMilliseconds(400);
            _color_Ani.RepeatBehavior = new RepeatBehavior(3);
            
        }

    }
    
    

   
}
