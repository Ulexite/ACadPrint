using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.ComponentModel;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.EditorInput;
using Autodesk.AutoCAD.Geometry;

//Autodesk
using Autodesk.AutoCAD.Runtime;
using CSPDS.AllObjectsTreeView;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;
using TypeDescriptor = System.ComponentModel.TypeDescriptor;

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

        [CommandMethod("showAllModules", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.UsePickSet)]
        public void listAllModules()
        {
            ModuleLister lister = new ModuleLister();
            var modules = lister.CollectModules(Process.GetCurrentProcess());
            saveToLog(modules);
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            ed.WriteMessage(modules.Count.ToString());
        }


        [CommandMethod("showobjectDeps", CommandFlags.Modal | CommandFlags.NoPaperSpace | CommandFlags.UsePickSet)]
        public void showEntityDeps()
        {
            DxfObjectsWalker walker = new DxfObjectsWalker();
            
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            PromptEntityResult prEntRes = ed.GetEntity("Select an Entity");

            if (prEntRes.Status == PromptStatus.OK)
            {
                ObjectDescriptionNode root = walker.WalkObjectOfDocument(Application.DocumentManager.MdiActiveDocument, prEntRes.ObjectId);
                AllObjectsView treeView = new AllObjectsView(root);
                saveToLog(walker.Types);
                saveToLog(root);
                Application.ShowModalWindow(treeView);
            }
        }
        
        [CommandMethod("SHOWCOMPROPS")]
        public static void ListComProps()
        {
            Document doc = Application.DocumentManager.MdiActiveDocument;
            Editor ed = doc.Editor;
            PromptEntityOptions peo = new PromptEntityOptions("\nSelect object: ");
            var res = ed.GetEntity(peo);
            if(res.Status != PromptStatus.OK)
                return;
            using(Transaction tr = doc.TransactionManager.StartOpenCloseTransaction())
            {
                Entity ent = (Entity) tr.GetObject(res.ObjectId, OpenMode.ForRead);
                
                var td = new Autodesk.AutoCAD.ComponentModel.TypeDescriptor(ent.GetType());
                var propProvs = td.GetPerInstancePropertyProviders();
                ed.WriteMessage(propProvs.Count.ToString());
                foreach (IPropertyProvider propProv in propProvs)
                {
                    var props = propProv.GetProperties(ent);
                    foreach (PropertyDescriptor prop in props)
                    {
                        try
                        {
                            object value = prop.GetValue(ent);
                            if(value != null)
                            {
                                ed.WriteMessage("\nfn {0}({1}) = {2}", prop.DisplayName, prop.Category,value.ToString());
                            }
                            
                        }
                        catch (Exception)
                        {
                            
                        }

                        
                    }
                    
                }

                tr.Commit();
            }
        }        
        private void saveToLog(List<Module> modules)
        {
            using (System.IO.StreamWriter file =
                new System.IO.StreamWriter(@".\modules.log", true))
            {
                foreach (var module in modules)
                {
                    file.WriteLine(module.ModuleName);
                    file.WriteLine(module.BaseAddress.ToString());
                    file.WriteLine("-----");
                }
                file.WriteLine("======");
            }
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

        [CommandMethod("ReadXData")]
        public static void ReadXData_Method()
        {
            Database db = HostApplicationServices.WorkingDatabase;
            Editor ed = Application.DocumentManager.MdiActiveDocument.Editor;
            try
            {
                PromptEntityResult prEntRes = ed.GetEntity("Select an Entity");
                string TestAppName = ed.GetString("AppName:").StringResult;
                ed.WriteMessage(prEntRes.ObjectId.ToString());
                if (prEntRes.Status == PromptStatus.OK)
                {
                    using (Transaction tr = db.TransactionManager.StartTransaction())
                    {
                        Entity ent = (Entity) tr.GetObject(prEntRes.ObjectId, OpenMode.ForRead);
                        ResultBuffer rb = ent.GetXDataForApplication(TestAppName);
                        if (rb != null)
                        {
                            TypedValue[] rvArr = rb.AsArray();
                            foreach (TypedValue tv in rvArr)
                            {
                                switch ((DxfCode) tv.TypeCode)
                                {
                                    case DxfCode.ExtendedDataRegAppName:
                                        string appName = (string) tv.Value;
                                        ed.WriteMessage("\nXData of appliation name (1001) {0}:", appName);
                                        break;
                                    case DxfCode.ExtendedDataAsciiString:
                                        string asciiStr = (string) tv.Value;
                                        ed.WriteMessage("\n\tAscii string (1000): {0}", asciiStr);
                                        break;
                                    case DxfCode.ExtendedDataLayerName:
                                        string layerName = (string) tv.Value;
                                        ed.WriteMessage("\n\tLayer name (1003): {0}", layerName);
                                        break;
                                    case DxfCode.ExtendedDataBinaryChunk:
                                        Byte[] chunk = tv.Value as Byte[];
                                        string chunkText = Encoding.ASCII.GetString(chunk);
                                        ed.WriteMessage("\n\tBinary chunk (1004): {0}", chunkText);
                                        break;
                                    case DxfCode.ExtendedDataHandle:
                                        ed.WriteMessage("\n\tObject handle (1005): {0}", tv.Value);
                                        break;
                                    case DxfCode.ExtendedDataXCoordinate:
                                        Point3d pt = (Point3d) tv.Value;
                                        ed.WriteMessage("\n\tPoint (1010): {0}", pt.ToString());
                                        break;
                                    case DxfCode.ExtendedDataWorldXCoordinate:
                                        Point3d pt1 = (Point3d) tv.Value;
                                        ed.WriteMessage("\n\tWorld point (1011): {0}", pt1.ToString());
                                        break;
                                    case DxfCode.ExtendedDataWorldXDisp:
                                        Point3d pt2 = (Point3d) tv.Value;
                                        ed.WriteMessage("\n\tDisplacement (1012): {0}", pt2.ToString());
                                        break;
                                    case DxfCode.ExtendedDataWorldXDir:
                                        Point3d pt3 = (Point3d) tv.Value;
                                        ed.WriteMessage("\n\tDirection (1013): {0}", pt3.ToString());
                                        break;
                                    case DxfCode.ExtendedDataControlString:
                                        string ctrStr = (string) tv.Value;
                                        ed.WriteMessage("\n\tControl string (1002): {0}", ctrStr);
                                        break;
                                    case DxfCode.ExtendedDataReal:
                                        double realValue = (double) tv.Value;
                                        ed.WriteMessage("\n\tReal (1040): {0}", realValue);
                                        break;
                                    case DxfCode.ExtendedDataDist:
                                        double dist = (double) tv.Value;
                                        ed.WriteMessage("\n\tDistance (1041): {0}", dist);
                                        break;
                                    case DxfCode.ExtendedDataScale:
                                        double scale = (double) tv.Value;
                                        ed.WriteMessage("\n\tScale (1042): {0}", scale);
                                        break;
                                    case DxfCode.ExtendedDataInteger16:
                                        Int16 int16 = (short) tv.Value;
                                        ed.WriteMessage("\n\tInt16 (1070): {0}", int16);
                                        break;
                                    case DxfCode.ExtendedDataInteger32:
                                        Int32 int32 = (Int32) tv.Value;
                                        ed.WriteMessage("\n\tInt32 (1071): {0}", int32);
                                        break;
                                    default:
                                        ed.WriteMessage("\n\tUnknown XData DXF code.");
                                        break;
                                }
                            }
                        }
                        else
                            ed.WriteMessage("The entity does not have the {0} XData.", TestAppName);

                        tr.Commit();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ed.WriteMessage(ex.ToString());
            }
        }
    }
}