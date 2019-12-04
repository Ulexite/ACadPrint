using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.EditorInput;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows;
using CSPDS.AllObjectsTreeView;
using CSPDS.Views;
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
        BorderEnumerator enumerator = new BorderEnumerator();
        BorderByFiles borders;

        [CommandMethod("ShowCPP")]
        public void ShowPallete()
        {
            try
            {
                if (pallete is null)
                {
                    pallete = new PaletteSet("Печать ЦПП");
                    borders = new BorderByFiles(enumerator.refreshBorderList(Application.DocumentManager));
                    ElementHost host = new ElementHost();
                    host.Dock = DockStyle.Fill;
                    host.Child = borders;
                    pallete.DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right);
                    pallete.Add("Форматы по файлам", host);
                }
                else
                {
                    borders.tvrObjects.ItemsSource = enumerator.refreshBorderList(Application.DocumentManager);
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

        [CommandMethod("showTest", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.UsePickSet)]
        public void ShowTestWindow()
        {
            String list = "";
            int counter = 0;

            foreach (ObjectDescriptor objectDescriptor in AllDocumentsAllObjects.ListAllObjects())
            {
                list += " \n" + objectDescriptor.FullName;
                counter++;
                if (counter >= 5)
                {
                    Application.ShowAlertDialog(list);
                    counter = 0;
                    list = "";
                }
            }

            list = list + counter;
            Application.ShowAlertDialog(list);
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