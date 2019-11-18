using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Autodesk.AutoCAD.ApplicationServices;
using interop = System.Runtime.InteropServices;
using Autodesk.AutoCAD.DatabaseServices;
using Autodesk.AutoCAD.Windows.OPM;
using Autodesk.AutoCAD.Runtime;
using Exception = System.Exception;

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
        private List<string> visitedObjects = new List<string>();

        private List<string> walkableTypes = new List<string>()
        {
            "Autodesk.AutoCAD.DatabaseServices.ObjectId",
            "Autodesk.AutoCAD.DatabaseServices.ResultBuffer",
            "Autodesk.AutoCAD.DatabaseServices.HyperLinkCollection",
            "System.String",
            "System.Double",
            "System.Int32",
            "System.Collections.ArrayList",
            "System.Object",
            "System.Guid",
            "System.Boolean",
            "System.__ComObject",
        };

        private Dictionary<string, List<string>> walkableProperties = new Dictionary<string, List<string>>()
        {
            {"Autodesk.AutoCAD.DatabaseServices.TypedValue", new List<string>() {"Value"}}
        };

        public Dictionary<string, DxfTypesDescriptor> Types => types;

        private Transaction tr;

        public DxfObjectsWalker()
        {
            Assembly.LoadFrom("asdkOPMNetExt.dll");
        }

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
                fileNode.InnerObjects.Add(WalkObject(db.LinetypeTableId));
                fileNode.InnerObjects.Add(WalkObject(db.NamedObjectsDictionaryId));
                fileNode.InnerObjects.Add(WalkObject(db.BlockTableId));
            }

            tr = null;
            return fileNode;
        }

        public ObjectDescriptionNode WalkObjectOfDocument(Document document, object obj)
        {
            ObjectDescriptionNode root = new ObjectDescriptionNode("root", "");
            using (Transaction transaction = document.TransactionManager.StartTransaction())
            {
                tr = transaction;
                root.Add(WalkObject(obj));
            }

            tr = null;
            return root;
        }

        private ObjectDescriptionNode WalkObject(object obj)
        {
            if (obj == null)
                return forNull();

            try
            {
                if (obj is string)
                    return new ObjectDescriptionNode(obj as string, "str");
                if (obj is byte[])
                    return new ObjectDescriptionNode(ByteArrayToString(obj as byte[]), "bytes");

                WalkType(obj);
                if (obj.GetType().FullName.Equals("System.__ComObject") )
                    return WalkComObject(obj);
                if (obj is Guid)
                    return WalkGUID((Guid)obj);
                
                if (obj is RegAppTableRecord)
                    return WalkRegAppTableRecord(obj as RegAppTableRecord);
                if (obj is BlockTableRecord)
                    return WalkBlockTableRecord(obj as BlockTableRecord);
                if (obj is BlockReference)
                    return WalkBlockReference(obj as BlockReference);

                if (obj is ObjectId)
                    return WalkOjectId((ObjectId) obj);
                if (obj is TypedValue)
                    return WalkTypedValue((TypedValue) obj);
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

        private ObjectDescriptionNode WalkGUID(Guid o)
        {
            var node = WalkLeafObject(o);
            node.Add(Type.GetTypeFromCLSID(o).FullName, "Type");
            return node;
        }

        private ObjectDescriptionNode WalkComObject(object obj)
        {
            var unknown = interop.Marshal.GetIUnknownForObject(obj);
            var objNode = new ObjectDescriptionNode(unknown.ToString(), unknown.GetType().FullName);
                
            return objNode;
        }

        private ObjectDescriptionNode WalkBlockReference(BlockReference blockReference)
        {
            var brNode = WalkLeafObject(blockReference);
            try
            {
                brNode.Add(WalkObject(blockReference.AttributeCollection));
            }
            catch (Exception e)
            {
                brNode.Add(forError(e));
            }
            try
            {
                brNode.Add(WalkObject(blockReference.DynamicBlockReferencePropertyCollection));
            }
            catch (Exception e)
            {
                brNode.Add(forError(e));
            }
            try
            {
                brNode.Add(WalkObject(blockReference.DynamicBlockTableRecord));
            }
            catch (Exception e)
            {
                brNode.Add(forError(e));
            }
            return brNode;
        }

        private ObjectDescriptionNode WalkBlockTableRecord(BlockTableRecord blockTableRecord)
        {
            var btrNode = WalkEnumerable(blockTableRecord);
            var brIds = blockTableRecord.GetBlockReferenceIds(false, false);
            foreach (var brId in brIds)
            {
                try
                {
                    btrNode.Add(WalkObject(brId));
                }
                catch (Exception exception)
                {
                    btrNode.Add(forError(exception));
                }
            }

            return btrNode;
        }


        private ObjectDescriptionNode WalkTypedValue(TypedValue typedValue)
        {
            var valueNode = new ObjectDescriptionNode("", typedValue.GetType().FullName);
            valueNode.Add(WalkObject(typedValue.Value));
            string typeName = "unknownType: " + typedValue.TypeCode;
            try
            {
                var type = (DxfCode) typedValue.TypeCode;
                typeName = type.ToString();
            }
            catch (Exception)
            {
            }

            valueNode.Add(new ObjectDescriptionNode(typeName, "DxfCode"));
            return valueNode;
        }

        private ObjectDescriptionNode WalkRegAppTableRecord(RegAppTableRecord app)
        {
            var appNode = WalkLeafObject(app);
            return appNode;
        }

        private string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        private ObjectDescriptionNode WalkLeafObject(object o)
        {
            string fullTypeName = o.GetType().FullName;
            var name = fullTypeName == o.ToString() ? "" : o.ToString();
            var node = new ObjectDescriptionNode(name, fullTypeName);
        

            foreach (var prop in o.GetType().GetProperties())
            {
                if (walkableTypes.Contains(prop.PropertyType.FullName) ||
                    (walkableProperties.ContainsKey(fullTypeName) &&
                     walkableProperties[fullTypeName].Contains(prop.Name)))
                    try
                    {
                        node.Add(prop.Name, "property").Add(WalkObject(prop.GetValue(o, null)));
                    }
                    catch (Exception exception)
                    {
                        node.Add(prop.Name, "property").Add(forError(exception));
                    }
            }

            if(o is RXObject)
                node.Add(WalkForDynamicProperties(o as RXObject));
            return node;
        }

        private ObjectDescriptionNode WalkForDynamicProperties(RXObject o)
        {
            var node = new ObjectDescriptionNode("dynamic properties");
            try
            {
                    Dictionary classDict = SystemObjects.ClassDictionary;
                    IPropertyManager2 opm = (IPropertyManager2) xOPM.xGET_OPMPROPERTY_MANAGER(o.GetRXClass());
                    
                    int propertyCount = 1;
                    unsafe
                    {
                        opm.GetDynamicPropertyCountEx(&propertyCount);
                        //opm.GetDynamicClassInfo()
                    }
                    
                    node = new ObjectDescriptionNode("dynamic properties ("+propertyCount+")");

                    for (int id = 0; id < propertyCount; ++id)
                    {
                        object value = null;
                        opm.GetDynamicProperty(id, out value);
                        if (value != null)
                        {
                            node.Add( WalkObject(value));
                        }
                        else
                        {
                            return node.Add( forNull());
                        }
                    }
                
            }
            catch (Exception exception)
            {
                return forError(exception);
            }

            return node;

        }

        private ObjectDescriptionNode WalkEnumerable(IEnumerable enumerable)
        {
            ObjectDescriptionNode node = WalkLeafObject(enumerable);
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
            string strIdDec = id.ToString().Trim().Replace("(", "").Replace(")", "");
            string strIdHex = Convert.ToInt64(strIdDec).ToString("X");

            bool visited = visitedObjects.Contains(strIdHex);
            ObjectDescriptionNode idNode = new ObjectDescriptionNode(strIdHex, visited ? "visited_id" : "id");

            idNode.Add(new ObjectDescriptionNode(DBObject.IsCustomObject(id).ToString(), "isCustom"));
            
            visitedObjects.Add(strIdHex);

            if (!visited)
                idNode.Add(WalkObject(tr.GetObject(id, OpenMode.ForRead)));
            return idNode;
        }


        private ObjectDescriptionNode forNull()
        {
            return new ObjectDescriptionNode("null", "null");
        }

        private ObjectDescriptionNode forError(Exception exception)
        {
            return new ObjectDescriptionNode(exception.Message + "::" + exception.StackTrace,
                exception.GetType().FullName + "::");
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

        public ObjectDescriptionNode Add(string walkObject, string fqn)
        {
            return this.Add(new ObjectDescriptionNode(walkObject, fqn));
        }
    }
}