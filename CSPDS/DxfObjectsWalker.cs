using System;
using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.Customization;
using Autodesk.AutoCAD.DatabaseServices;

namespace CSPDS
{
    public class DxfTypesDescriptor
    {
        private string fqn;
        private string assemblyName;
        private string assemblyLocation;
        private List<Tuple<string, string>> propertyies = new List<Tuple<string, string>>();
        private List<string> methods = new List<string>();

        public string Fqn => fqn;

        public string AssemblyName => assemblyName;

        public string AssemblyLocation => assemblyLocation;

        public List<Tuple<string, string>> Propertyies => propertyies;

        public List<string> Methods => methods;

        public DxfTypesDescriptor(Type type)
        {
            fqn = type.FullName;
            assemblyName = type.Assembly.FullName;
            assemblyLocation = type.Assembly.Location;

            foreach (var prop in type.GetProperties())
            {
                propertyies.Add(new Tuple<string, string>(prop.Name, prop.PropertyType.FullName));
            }

            foreach (var meth in type.GetMethods())
            {
                methods.Add(meth.Name);
            }
        }
    }

    public class DxfObjectsWalker
    {
        private Dictionary<string, DxfTypesDescriptor> types = new Dictionary<string, DxfTypesDescriptor>();

        private Dictionary<string, List<string>> walkableProperties = new Dictionary<string, List<string>>()
        {
            {"Autodesk.AutoCAD.DatabaseServices.TypedValue", new List<string>() {"Value"}}
        };

        public Dictionary<string, DxfTypesDescriptor> Types => types;

        private Transaction tr;

        public ObjectDescriptionNode WalkAllDocuments(DocumentCollection documentCollection)
        {
            ObjectDescriptionNode root = new ObjectDescriptionNode("root", "");
            foreach (Document document in documentCollection)
            {
                root.InnerObjects.Add(WalkDocument(document));
            }

            return root;
        }

        public ObjectDescriptionNode WalkDocument(Document document)
        {
            ObjectDescriptionNode fileNode = new ObjectDescriptionNode(document.Name, "file:");
            using (Transaction transaction = document.TransactionManager.StartTransaction())
            {
                tr = transaction;
                Database db = document.Database;
                fileNode.InnerObjects.Add(WalkObject(db));
                fileNode.InnerObjects.Add(WalkObject(db.NamedObjectsDictionaryId));
                fileNode.InnerObjects.Add(WalkObject(db.BlockTableId));
            }

            tr = null;
            return fileNode;
        }

        private ObjectDescriptionNode WalkObject(object obj)
        {
            if (obj == null)
                return forNull();

            try
            {
                if(obj is string)
                    return new ObjectDescriptionNode(obj as string, "str");
                WalkType(obj);
                if (obj is ObjectId)
                    return WalkOjectId((ObjectId) obj);
                if (obj is IDictionary)
                    return WalkDictionary(obj as IDictionary);
                if (obj is IEnumerable)
                    return WalkEnumerable(obj as IEnumerable);

                return WalkLeafObject(obj);
            }
            catch (Exception e)
            {
                return forError(e);
            }
        }

        private ObjectDescriptionNode WalkLeafObject(object o)
        {
            var node = new ObjectDescriptionNode("", o.GetType().FullName);
            string fullTypeName = o.GetType().FullName;
            if (walkableProperties.ContainsKey(fullTypeName))
                foreach (var prop in o.GetType().GetProperties())
                {
                    if (walkableProperties[fullTypeName].Contains(prop.Name))
                        try
                        {
                            node.Add(WalkObject(prop.GetValue(o, null)));
                        }
                        catch (Exception exception)
                        {
                            node.Add(forError(exception));
                        }
                }

            return node;
        }

        private ObjectDescriptionNode WalkEnumerable(IEnumerable enumerable)
        {
            ObjectDescriptionNode node = new ObjectDescriptionNode("list", enumerable.GetType().FullName);
            foreach (var obj in enumerable)
            {
                node.Add(WalkObject(obj));
            }

            return node;
        }

        private ObjectDescriptionNode WalkDictionary(IDictionary dict)
        {
            ObjectDescriptionNode dictNode = new ObjectDescriptionNode("dict", dict.GetType().FullName);
            foreach (var key in dict.Keys)
            {
                dictNode.Add(new ObjectDescriptionNode(key.ToString(), "key")).Add(WalkObject(dict[key]));
            }

            return dictNode;
        }

        private void WalkType(object o)
        {
            String typeName = o.GetType().FullName;

            if (!types.ContainsKey(typeName))
            {
                var typeDescriptor = new DxfTypesDescriptor(o.GetType());
                types.Add(typeName, typeDescriptor);
            }
        }

        private ObjectDescriptionNode WalkOjectId(ObjectId id)
        {
            ObjectDescriptionNode idNode = new ObjectDescriptionNode(id.ToString(), "id");
            idNode.InnerObjects.Add(WalkObject(tr.GetObject(id, OpenMode.ForRead)));
            return idNode;
        }


        private ObjectDescriptionNode forNull()
        {
            return new ObjectDescriptionNode("null", "null");
        }

        private ObjectDescriptionNode forError(Exception exception)
        {
            return new ObjectDescriptionNode(exception.StackTrace, exception.GetType().FullName);
        }
    }

    public class ObjectDescriptionNode
    {
        private readonly String name;
        private readonly List<ObjectDescriptionNode> innerObjects = new List<ObjectDescriptionNode>();
        private readonly String fqn;

        public ObjectDescriptionNode(string name, string fqn)
        {
            this.name = name;
            this.fqn = fqn;
        }

        public ObjectDescriptionNode(string name)
        {
            this.name = name;
            this.fqn = "";
        }

        public void Add(string name, Type type)
        {
            InnerObjects.Add(new ObjectDescriptionNode(name, type.FullName));
        }

        public string Name => fqn + ":" + name;

        public List<ObjectDescriptionNode> InnerObjects => innerObjects;

        public string Fqn => fqn;

        public ObjectDescriptionNode Add(ObjectDescriptionNode walkObject)
        {
            innerObjects.Add(walkObject);
            return walkObject;
        }
    }
}