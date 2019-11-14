using System;
using System.Collections.Generic;
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
        [CommandMethod("showallobjects", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.UsePickSet)]
        public void showAllObjects()
        {
            ObjectDescriptionNode root = AllDocumentsAllObjects.fromAllFiles();
            AllObjectsView treeView = new AllObjectsView(root);
            saveToLog(root);
            Application.ShowModalWindow(treeView);            
        }

        public void saveToLog(ObjectDescriptionNode root)
        {
            var frontire = new Stack<Tuple<ObjectDescriptionNode, int>>();
            frontire.Push(new Tuple<ObjectDescriptionNode, int>(root, 0));
            
            using (System.IO.StreamWriter file = 
                new System.IO.StreamWriter(@"..\objectsTree.log", true))
            {
                while (frontire.Count != 0)
                {
                    var nextNode = frontire.Pop();
                    foreach (var child in nextNode.Item1.InnerObjects)
                    {
                        frontire.Push(new Tuple<ObjectDescriptionNode, int>(child, nextNode.Item2+1));
                    }
                    
                    file.WriteLine(new String(' ' , nextNode.Item2) + nextNode.Item1.Name);
                }    
                
            }            
        }
        
    }
}