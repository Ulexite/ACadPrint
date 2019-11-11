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
                    var bt = tr.GetObject(db.BlockTableId, OpenMode.ForRead) as BlockTable;
//                    BlockTableRecord.ModelSpace
//                    foreach (DBObject obj in tr.GetAllObjects())
//                    {
//                        yield return new ObjectDescriptor(document.Name, obj.ToString());
//                    }
                }
                
                
                yield return new ObjectDescriptor(document.Name, "dummyObjectForDocument");
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
}