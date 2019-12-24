using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Internal.PropertyInspector;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using CSPDS.AllObjectsTreeView;
using CSPDS.Views;
using log4net;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = Autodesk.AutoCAD.BoundaryRepresentation.Exception;

[assembly: CommandClass(typeof(CSPDS.CommandClass))]

namespace CSPDS
{
    /// <summary>
    /// Данный класс содержит методы для непосредственной работы с AutoCAD
    /// </summary>
    public class CommandClass
    {
        private static PaletteSet pallete;
        private static ObservableCollection<FileDescriptor> fileDescriptors;

        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        BorderEnumerator enumerator = new BorderEnumerator();
        BorderByFiles borders;

        [CommandMethod("ShowCPP")]
        public void ShowPallete()
        {
            try
            {
                if (pallete is null)
                {
                    _log.Debug("Подготовка панели");
                    pallete = new PaletteSet("Печать ЦПП");
                    _log.Debug("Готовим TreeView");
                    fileDescriptors = enumerator.refreshBorderList(Application.DocumentManager);
                    borders = new BorderByFiles(fileDescriptors);
                    _log.Debug("Готовим ElementHost");
                    ElementHost host = new ElementHost();
                    host.Dock = DockStyle.Fill;
                    host.Child = borders;
                    pallete.DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right);
                    _log.Debug("Соединяем");
                    pallete.Add("Форматы по файлам", host);
                }
                else
                {
                    _log.Debug("Обновить список");
                    enumerator.refreshBorderList(Application.DocumentManager);
                    try
                    {
                        _log.Debug("Обновить TreeView");

                        borders.tvrObjects.Items.Refresh();
                    }
                    catch (Exception exception)
                    {
                        _log.Error("Не обновили", exception);
                    }

                    _log.Debug("Обновить TreeView UpdateLayout");
                    borders.tvrObjects.UpdateLayout();
                }

                pallete.KeepFocus = true;
                pallete.Visible = true;
            }
            catch (Exception e)
            {
                var ed = Application.DocumentManager.CurrentDocument.Editor;
                ed.WriteMessage(e.StackTrace);
            }
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

        [CommandMethod("CPPPDF")]
        public void CreatePdf()
        {
            fileDescriptors = enumerator.refreshBorderList(Application.DocumentManager);
            PrintManager pm = new PrintManager();
            pm.PdfAll(fileDescriptors[0]);
        }

        [CommandMethod("SHOWCUSTOMPROPS")]
        public static void ListCustomProps()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect object: ");
            var res = ed.GetEntity(peo);
            if (res.Status != PromptStatus.OK)
                return;

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(AssemblyDirectory + @"\customProprs.log", false))
            {
                file.WriteLine(String.Format("object id: {0:X}", res.ObjectId));
                IntPtr pUnknown = ObjectPropertyManagerPropertyUtility.GetIUnknownFromObjectId(res.ObjectId);
                file.WriteLine(String.Format("pUnknown :{0:X}", pUnknown));
                if (pUnknown != IntPtr.Zero)
                {
                    using (CollectionVector properties =
                        ObjectPropertyManagerProperties.GetProperties(res.ObjectId, false, false))
                    {
                        file.WriteLine(properties.Count());
                        if (properties.Count() != 0)
                        {
                            using (CategoryCollectable category = properties.Item(0) as CategoryCollectable)
                            {
                                CollectionVector props = category.Properties;
                                for (int i = 0; i < props.Count(); ++i)
                                {
                                    using (PropertyCollectable prop = props.Item(i) as PropertyCollectable)
                                    {
                                        if (prop != null)
                                        {
                                            object value = null;
                                            if (prop.GetValue(pUnknown, ref value) && value != null)
                                            {
                                                file.WriteLine(string.Format("{4} {3} {2}  {0}={1}", prop.Name, value,
                                                    prop.Description, prop.CollectableName, prop.DISP));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }


        [CommandMethod("showallobjects", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.UsePickSet)]
        public void showAllObjects()
        {
            ObjectDescriptionNode root = AllDocumentsAllObjects.allObjectsToTree();

            AllObjectsView treeView = new AllObjectsView(root);
            Application.ShowModalWindow(treeView);
        }
    }
}