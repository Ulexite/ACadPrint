using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Windows;
using CSPDS.Actors;
using CSPDS.Model;
using CSPDS.ViewModel;
using CSPDS.Views;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Size = System.Drawing.Size;

namespace CSPDS
{
    public class ModuleUI
    {
        //TODO: logger DI
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        //TODO: конфигурация
        private readonly List<Tuple<string, string>> grouping = new List<Tuple<string, string>>()
        {
            new Tuple<string, string>("По форматам", "Формат"),
            new Tuple<string, string>("По файлам", "shortFileName")
        };

        private readonly Dictionary<string, SheetsGroupTree> groups =
            new Dictionary<string, SheetsGroupTree>();

        private readonly SheetTreeNodeCache sheetNodeCache = new SheetTreeNodeCache();
        private readonly DestinationVariants destiantion_variants = new DestinationVariants();
        private readonly Formats formats;

        private readonly ModuleModel model;
        private readonly PlottingAction plotting;

        //AcUI:
        private static PaletteSet mainPalette;

        public ModuleUI(ModuleModel model, PlottingAction plotting)
        {
            this.model = model;
            this.plotting = plotting;
            formats = new Formats(this);
        }

        public void View()
        {
            InitPalette();
            Refresh();

            mainPalette.KeepFocus = true;
            mainPalette.Visible = true;
        }

        public DestinationVariants DestiantionVariants => destiantion_variants;


        public void FocusOnFile(Document document)
        {
            //TODO: nonStatic, DI!            
            if (!document.IsDisposed)
            {
                if (!document.IsActive)
                {
                    DocumentCollection docMgr = Application.DocumentManager;
                    if (!docMgr.DocumentActivationEnabled)
                        docMgr.DocumentActivationEnabled = true;

                    docMgr.MdiActiveDocument = document;
                }
            }
        }


        private void Refresh()
        {
            model.Refresh();

            destiantion_variants.AddAll(model.Destinations);
            formats.AddAll(model.Formats);
            sheetNodeCache.Refill(model.Sheets);

            foreach (var group in groups.Values)
            {
                group.Refill();
            }
        }

        private void InitPalette()
        {
            if (mainPalette is null)
            {
                mainPalette = new PaletteSet("Менеджер печати ЦПП");
                mainPalette.DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right);
                mainPalette.SetSize(new Size(800, 600));

                foreach (var groupRule in grouping)
                {
                    groups.Add(groupRule.Item2, new SheetsGroupTree(groupRule.Item2, sheetNodeCache));
                    ElementHost group = CreateHost(new SheetsGroup(groups[groupRule.Item2].Groups, this));
                    mainPalette.Add(groupRule.Item1, group);
                }

                mainPalette.Add("Настройки по форматам", CreateHost(new FormatDestinationView(formats, this)));
            }
        }

        private ElementHost CreateHost(UIElement control)
        {
            ElementHost host = new ElementHost();
            host.Dock = DockStyle.Fill;
            host.Child = control;
            host.AutoSize = true;
            return host;
        }


        public void UpdateDestinationForFormat(string name, Destination destination)
        {
            model.SetDestinationForFormat(name, destination);
            //TODO: Update other views?
        }

        public void PlotSelected()
        {
            PlotPlan plan = model.GetPlotPlan();
            //TODO: Refresh windows etc
            foreach (var sheet in sheetNodeCache.Values)
            {
                if (sheet.IsSelected == true)
                    plan.Add(sheet.SheetData);
            }
            
            plotting.Plot(plan);            
        }
    }
}