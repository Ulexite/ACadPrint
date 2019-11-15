using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            ObjectDescriptionNode fileNode = new ObjectDescriptionNode("file:" + document.Name);
            

            var db = document.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                fileNode.InnerObjects.Add(fromObjectId(db.NamedObjectsDictionaryId, tr));
                fileNode.InnerObjects.Add(fromObjectId(db.BlockTableId, tr));
                fileNode.InnerObjects.Add(fromObjectId(db.LayerTableId, tr));
            }
            return fileNode;
        }

        private static ObjectDescriptionNode fromUserData(Hashtable userdata)
        {
            ObjectDescriptionNode node = new ObjectDescriptionNode("userdata");
            foreach (var key in userdata.Keys)
            {
                String kv = key.ToString() + ":" + userdata[key].ToString();
                node.InnerObjects.Add(new ObjectDescriptionNode(kv));
            }

            return node;
        }

        private static ObjectDescriptionNode fromObject(object obj, Transaction tr)
        {
            try
            {
                if (obj == null)
                    return new ObjectDescriptionNode("null", "null");


                if (obj is ObjectId)
                    return fromObjectId((ObjectId) obj, tr);
                
                if(obj is SymbolTable)
                    return fromSymbolTable(obj as SymbolTable, tr);
                if (obj is IDictionary)
                    return fromIDictionary(obj as IDictionary, tr);
                if (obj is IEnumerable)
                    return fromEnumerable((IEnumerable) obj, tr);
                if (HasProperty(obj, "Name"))
                    return fromProperties("named: ", obj, tr);
                return fromProperties("", obj, tr);
            }
            catch (Exception exception)
            {
                return new ObjectDescriptionNode("error:" + exception.StackTrace);
            }
        }

        public static bool HasProperty(object objectToCheck, string name)
        {
            var type = objectToCheck.GetType();
            return type.GetProperty(name) != null;
        }


        private static ObjectDescriptionNode fromEnumerable(IEnumerable list, Transaction tr)
        {
            ObjectDescriptionNode collection = fromProperties("collection: ", list, tr);
            foreach (var obj in list)
            {
                ObjectDescriptionNode childNode = fromObject(obj, tr);
                collection.InnerObjects.Add(childNode);
            }

            return collection;
        }
        private static ObjectDescriptionNode fromSymbolTable(SymbolTable list, Transaction tr)
        {
            ObjectDescriptionNode collection = new ObjectDescriptionNode(list.ToString());
            foreach (var obj in list)
            {
                ObjectDescriptionNode childNode = fromObject(obj, tr);
                collection.InnerObjects.Add(childNode);
            }

            return collection;
        }

        private static ObjectDescriptionNode fromProperties(String prefix, object obj, Transaction tr)
        {
            ObjectDescriptionNode objNode = new ObjectDescriptionNode(prefix + obj.GetType().ToString());
            foreach (var prop in obj.GetType().GetProperties())
            {
                ObjectDescriptionNode propNode = new ObjectDescriptionNode("prop = " + prop.Name);
                String propValue = "nope";
                try
                {
                    propValue = prop.GetValue(obj, null).ToString();
                }
                catch (Exception e)
                {
                    propValue = "error:" + e.Message;
                }

                ObjectDescriptionNode valueNode = new ObjectDescriptionNode("value = " +propValue);
                propNode.InnerObjects.Add(valueNode);
                objNode.InnerObjects.Add(propNode);
            }

            return objNode;
        }

        private static ObjectDescriptionNode fromObjectId(ObjectId id, Transaction tr)
        {
            var obj = tr.GetObject(id, OpenMode.ForRead);
            return fromObject(obj, tr);
        }

        private static ObjectDescriptionNode fromIDictionary(IDictionary dict, Transaction tr)
        {
            ObjectDescriptionNode dictNode = fromProperties("Dictionary: ", dict, tr);

            foreach (var key in dict.Keys)
            {
                var keyNode = new ObjectDescriptionNode(key.ToString());
                var valueNode = fromObject(dict[key], tr);
                dictNode.InnerObjects.Add(keyNode);
                keyNode.InnerObjects.Add(valueNode);
            }

            return dictNode;
        }
    }


}