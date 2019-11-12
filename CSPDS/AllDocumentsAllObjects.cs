using System;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CSPDS
{
    public class AllDocumentsAllObjects
    {
        public static IEnumerable<ObjectDescriptor> ListAllObjects()
        {
            foreach (Document document in Application.DocumentManager)
            {
                var db = document.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
//                    yield return new ObjectDescriptor(document.Name, bt.ToString());
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
//                    yield return new ObjectDescriptor(document.Name, lt.ToString());
                    DimStyleTable dt = tr.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
//                    yield return new ObjectDescriptor(document.Name, dt.ToString());

                    foreach (ObjectId btId in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                        yield return new ObjectDescriptor(document.Name, btr.Name);
                    }

                    foreach (ObjectId btId in lt)
                    {
                        LayerTableRecord ltr = (LayerTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                        yield return new ObjectDescriptor(document.Name, ltr.Name);
                    }

                    foreach (ObjectId btId in dt)
                    {
                        DimStyleTableRecord dtr = (DimStyleTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                        yield return new ObjectDescriptor(document.Name, dtr.Name);
                    }
                }
            }
        }

        public static ObjectDescriptionNode allObjectsToTree()
        {
            ObjectDescriptionNode root = new ObjectDescriptionNode("root", "root");

            foreach (Document document in Application.DocumentManager)
            {
                ObjectDescriptionNode fileNode = new ObjectDescriptionNode("file:" + document.Name, "");
                root.InnerObjects.Add(fileNode);

                fillFileNode(fileNode, document);
            }
            return root;
        }

        private static void fillFileNode(ObjectDescriptionNode fileNode, Document document)
        {
                var db = document.Database;
                using (Transaction tr = db.TransactionManager.StartTransaction())
                {
                    BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
                    ObjectDescriptionNode btNode = new ObjectDescriptionNode(bt.ToString(), "");
                    fileNode.InnerObjects.Add(btNode);
                    LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
                    ObjectDescriptionNode ltNode = new ObjectDescriptionNode(lt.ToString(), "");
                    fileNode.InnerObjects.Add(ltNode);
                    DimStyleTable dt = tr.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
                    ObjectDescriptionNode dtNode = new ObjectDescriptionNode(dt.ToString(), "");
                    fileNode.InnerObjects.Add(dtNode);

                    foreach (ObjectId btrId in bt)
                    {
                        BlockTableRecord btr = (BlockTableRecord) tr.GetObject(btrId, OpenMode.ForRead);
                        ObjectDescriptionNode recNode = new ObjectDescriptionNode(btr.Name, btr.Name);
                        btNode.InnerObjects.Add(recNode);

                        foreach (ObjectId id in btr)
                        {
                            var obj = tr.GetObject(id, OpenMode.ForRead);
                            ObjectDescriptionNode objNode = new ObjectDescriptionNode(obj.ToString(), "");
                            recNode.InnerObjects.Add(objNode);
                        }
                    }

                    foreach (ObjectId btId in lt)
                    {
                        LayerTableRecord ltr = (LayerTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                        ObjectDescriptionNode recNode = new ObjectDescriptionNode(ltr.Name, ltr.Name);
                        
                        ltNode.InnerObjects.Add(recNode);
                    }

                    foreach (ObjectId btId in dt)
                    {
                        DimStyleTableRecord dtr = (DimStyleTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                        ObjectDescriptionNode recNode = new ObjectDescriptionNode(dtr.Name, dtr.Name);
                        dtNode.InnerObjects.Add(recNode);
                    }
                }
        }
    }


    public class ObjectDescriptor
    {
        private readonly String documentName;
        private readonly String objectName;
        private readonly String fullName;

        public ObjectDescriptor(string documentName, string objectName)
        {
            this.documentName = documentName;
            this.objectName = objectName;
            this.fullName = documentName + ":" + objectName;
        }

        public string DocumentName => documentName;

        public string ObjectName => objectName;

        public string FullName => fullName;
    }

    public class ObjectDescriptionNode
    {
        private readonly String objectClassFQN;
        private readonly List<ObjectDescriptionNode> innerObjects = new List<ObjectDescriptionNode>();
        private readonly String description;

        public ObjectDescriptionNode(string objectClassFqn, string description)
        {
            objectClassFQN = objectClassFqn;
            this.description = description;
        }

        public string ObjectClassFqn => objectClassFQN;

        public List<ObjectDescriptionNode> InnerObjects => innerObjects;

        public string Description => description;
    }
}