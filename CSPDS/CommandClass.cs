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
            DxfObjectsWalker walker = new DxfObjectsWalker();
            ObjectDescriptionNode root = walker.WalkAllDocuments(Application.DocumentManager);
            AllObjectsView treeView = new AllObjectsView(root);

            saveToLog(walker.Types);
            saveToLog(root);
            Application.ShowModalWindow(treeView);
        }

        private void saveToLog(Dictionary<string, DxfTypesDescriptor> walkerTypes)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@".\types.log", true))
            {
                foreach (var typeDescriptor in walkerTypes.Values)
                {
                    file.WriteLine(typeDescriptor.Fqn);
                    file.WriteLine(typeDescriptor.AssemblyName);
                    file.WriteLine(typeDescriptor.AssemblyLocation);
                    file.WriteLine("propeties: ");
                    foreach (var prop in typeDescriptor.Propertyies)
                    {
                        file.WriteLine("   {0}={1}", prop.Item1, prop.Item2);
                    }

                    file.WriteLine("methods: ");
                    foreach (var meth in typeDescriptor.Methods)
                    {
                        file.WriteLine("   {0}", meth);
                    }
                    file.WriteLine("-----");
                }
            }
        }

        public void saveToLog(ObjectDescriptionNode root)
        {
            var frontire = new Stack<Tuple<ObjectDescriptionNode, int>>();
            frontire.Push(new Tuple<ObjectDescriptionNode, int>(root, 0));

            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@".\objectsTree.log", true))
            {
                while (frontire.Count != 0)
                {
                    var nextNode = frontire.Pop();
                    foreach (var child in nextNode.Item1.InnerObjects)
                    {
                        frontire.Push(new Tuple<ObjectDescriptionNode, int>(child, nextNode.Item2 + 1));
                    }

                    file.WriteLine(new String(' ', nextNode.Item2) + nextNode.Item1.Name);
                }
            }
        }
    }
}