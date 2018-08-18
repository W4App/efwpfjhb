using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Threading;


namespace newJhb
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
       // private List<>
        Mutex sgl = null;
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            bool isNew;
            sgl = new Mutex(true, "newJhb", out isNew);
            
                if (!isNew)
                {
                    this.Shutdown(); //阻止多实例.
                }
               
                
            

        }
        
       

        
    }
    
}
