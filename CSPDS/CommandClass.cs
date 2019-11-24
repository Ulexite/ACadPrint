using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

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
        private static PaletteSet ps;


        [CommandMethod("ShowWPF")]
        public void showPallete()
        {
            try
            {
                ps = new PaletteSet("Форматы по файлам");
                ps.Size = new Size(400, 600);
                ps.DockEnabled = (DockSides) ((int) DockSides.Left + (int) DockSides.Right);
                BorderEnumerator enumerator = new BorderEnumerator();
                BorderByFiles borders = new BorderByFiles(enumerator.fromAllFiles());
                ElementHost host = new ElementHost();
                host.Dock = DockStyle.Fill;
                host.Child = borders;
                ps.Add("Форматы", host);
                ps.KeepFocus = true;
                ps.Visible = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
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