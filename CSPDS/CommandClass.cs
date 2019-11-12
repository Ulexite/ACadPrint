using System;
using Autodesk.AutoCAD.ApplicationServices.Core;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using CSPDS.AllObjectsTreeView;

[assembly: CommandClass(typeof(CSPDS.CommandClass))]

namespace CSPDS
{
    /// <summary>
    /// Данный класс содержит методы для непосредственной работы с AutoCAD
    /// </summary>
    public class CommandClass
    {
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