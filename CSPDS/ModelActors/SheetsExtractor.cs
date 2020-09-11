using System.Collections.Generic;
using System.Linq;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using CSPDS.Model;

namespace CSPDS.Actors
{
    /*
    * Умеет извлекать все рамки из документа.
    * Знает чем рамка отличается от других объектов
    * Определяется как замкнутая кривая (Autodesk.AutoCAD.DatabaseServices.ImpCurve),
    * имеющая свойство "Формат"
    */
    public class SheetsExtractor
    {
        //TODO: Конфигурация
        private readonly List<string> importantFields = new List<string>()
        {
            "Формат"
        };

        private readonly string sheetFQN = "Autodesk.AutoCAD.DatabaseServices.ImpCurve";

        private readonly PropertiesExractor propExtractor;
        private readonly SheetCreator sheetCreator;

        public SheetsExtractor(PropertiesExractor propExtractor, SheetCreator sheetCreator)
        {
            this.propExtractor = propExtractor;
            this.sheetCreator = sheetCreator;
        }

        public IEnumerable<Sheet> SheetsFrom(Document doc)
        {
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                BlockTable blockTable = (BlockTable) tr.GetObject(db.BlockTableId, OpenMode.ForRead);
                foreach (ObjectId recordId in blockTable)
                {
                    BlockTableRecord record = (BlockTableRecord) tr.GetObject(recordId, OpenMode.ForRead);

                    foreach (ObjectId objectId in record)
                    {
                        var obj = tr.GetObject(objectId, OpenMode.ForRead);
                        if (obj.GetType().FullName.Equals(sheetFQN))
                        {
                            var properties = propExtractor.PropertiesFrom(obj.ObjectId);

                            if (!importantFields.Except(properties.Keys).Any())
                                yield return sheetCreator.Create(doc, obj.Id, properties);
                        }
                    }
                }
            }
        }
    }
}