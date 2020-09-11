using System;
using System.IO;
using System.Reflection;

namespace CSPDS
{
    public class Module
    {
        private readonly ModuleModel model;
        private readonly ModuleUI ui;

        private readonly PlottingAction plotting;
        //TODO:config
        
        public Module()
        {
            //DI for dependency injection
            model = new ModuleModel();
            plotting = new PlottingAction();
            ui = new ModuleUI(model,plotting);
            plotting.Ui = ui;
        }

        public void ShowUI()
        {
            ui.View();
        }
        
        public static string AssemblyDirectory
        {
            get
            {
                string codeBase = Assembly.GetExecutingAssembly().CodeBase;
                UriBuilder uri = new UriBuilder(codeBase);
                string path = Uri.UnescapeDataString(uri.Path);
                return Path.GetDirectoryName(path);
            }
        }
        
    }
}