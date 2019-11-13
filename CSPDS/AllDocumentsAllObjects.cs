using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Runtime;
using Autodesk.AutoCAD.Windows.Data;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;
using Exception = System.Exception;

namespace CSPDS
{
    public class AllDocumentsAllObjects
    {
        public static ObjectDescriptionNode fromAllFiles()
        {
            ObjectDescriptionNode root = new ObjectDescriptionNode("root", "root");

            foreach (Document document in Application.DocumentManager)
            {
                root.InnerObjects.Add(fromDocument(document));
            }

            return root;
        }

        private static ObjectDescriptionNode fromDocument(Document document)
        {
            ObjectDescriptionNode fileNode = new ObjectDescriptionNode("file:" + document.Name, "");
            fileNode.InnerObjects.Add(fromUserData(document.UserData));

            var db = document.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                fileNode.InnerObjects.Add(blockTable(tr, db));
                fileNode.InnerObjects.Add(layerTable(tr, db));
                fileNode.InnerObjects.Add(dimTable(tr, db));
                fileNode.InnerObjects.Add(fromObjectId(db.NamedObjectsDictionaryId, tr));
                fileNode.InnerObjects.Add(fromObjectId(db.PlotStyleNameDictionaryId, tr));
                fileNode.InnerObjects.Add(fromObjectId(db.DataLinkDictionaryId, tr));
            }

            return fileNode;
        }

        private static ObjectDescriptionNode fromUserData(Hashtable userdata)
        {
            ObjectDescriptionNode node = new ObjectDescriptionNode("userdata", "");
            foreach (var key in userdata.Keys)
            {
                String kv = key.ToString() + ":" + userdata[key].ToString();
                node.InnerObjects.Add(new ObjectDescriptionNode(kv, ""));
            }

            return node;
        }

        private static ObjectDescriptionNode fromObjectId(ObjectId id, Transaction tr)
        {
            try
            {
                var obj = tr.GetObject(id, OpenMode.ForRead);

                if (obj is DBDictionary)
                    return fromDictionary(obj as DBDictionary, tr);
//            if (objectType.IsDerivedFrom(RXObject.GetClass(typeof(DBDictionary))))
//                return fromDictionary(obj as DBDictionary, tr);
//            RXObject.GetClass(typeof(SymbolTable));
//            RXObject.GetClass(typeof(SymbolTableRecord));
                return new ObjectDescriptionNode(obj.ToString(), "");
            }
            catch (Exception exception)
            {
                return new ObjectDescriptionNode("error:" + exception.Message, "");
            }
        }

        private static ObjectDescriptionNode fromDictionary(DBDictionary dict, Transaction tr)
        {
            ObjectDescriptionNode node =
                new ObjectDescriptionNode("Dictionary(" + dict.Count + "):" + dict.ToString(), "");

            foreach (var data in dict)
            {
                var key = new ObjectDescriptionNode(data.m_key, "");
                key.InnerObjects.Add(fromObjectId(data.m_value, tr));
            }

            return node;
        }

        static ObjectDescriptionNode dimTable(Transaction tr, Database db)
        {
            DimStyleTable dt = tr.GetObject(db.DimStyleTableId, OpenMode.ForRead) as DimStyleTable;
            ObjectDescriptionNode dtNode = new ObjectDescriptionNode(dt.ToString(), "");

            foreach (ObjectId btId in dt)
            {
                DimStyleTableRecord dtr = (DimStyleTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                ObjectDescriptionNode recNode = new ObjectDescriptionNode(dtr.Name, dtr.Name);
                dtNode.InnerObjects.Add(recNode);
            }

            return dtNode;
        }

        static ObjectDescriptionNode layerTable(Transaction tr, Database db)
        {
            LayerTable lt = tr.GetObject(db.LayerTableId, OpenMode.ForRead) as LayerTable;
            ObjectDescriptionNode ltNode = new ObjectDescriptionNode(lt.ToString(), "");

            foreach (ObjectId btId in lt)
            {
                LayerTableRecord ltr = (LayerTableRecord) tr.GetObject(btId, OpenMode.ForRead);
                ObjectDescriptionNode recNode = new ObjectDescriptionNode(ltr.Name, ltr.Name);

                ltNode.InnerObjects.Add(recNode);
            }


            return ltNode;
        }

        static ObjectDescriptionNode blockTable(Transaction tr, Database db)
        {
            BlockTable bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
            ObjectDescriptionNode btNode = new ObjectDescriptionNode(bt.ToString(), "");

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

            return btNode;
        }

        static ObjectDescriptionNode namedObjects(Transaction tr, Database db)
        {
            ObjectDescriptionNode node = new ObjectDescriptionNode("NamedObjectDicitonary", "");
            DBDictionary dictionary = tr.GetObject(db.NamedObjectsDictionaryId, OpenMode.ForRead) as DBDictionary;

            foreach (var dictItem in dictionary)
            {
                //dictItem
            }

            return node;
        }
    }


    public class ObjectDescriptionNode
    {
        private readonly String name;
        private readonly List<ObjectDescriptionNode> innerObjects = new List<ObjectDescriptionNode>();
        private readonly String description;

        public ObjectDescriptionNode(string name, string description)
        {
            this.name = name;
            this.description = description;
        }

        public string Name => name;

        public List<ObjectDescriptionNode> InnerObjects => innerObjects;

        public string Description => description;
    }
}