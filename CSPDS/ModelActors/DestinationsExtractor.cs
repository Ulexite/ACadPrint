using System.Collections;
using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using Autodesk.AutoCAD.DatabaseServices;
using CSPDS.Model;

namespace CSPDS.Actors
{
    public class DestinationsExtractor
    {
        private readonly PropertiesExractor propExtractor;
        private readonly DestinationCreator creator;

        public DestinationsExtractor(PropertiesExractor propExtractor, DestinationCreator creator)
        {
            this.propExtractor = propExtractor;
            this.creator = creator;
        }

        public IEnumerable<Destination> DestinationsFrom(Document doc)
        {
            Database db = doc.Database;
            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary settingsDict = (DBDictionary) tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);
                foreach (string key in ((IDictionary) settingsDict).Keys)
                {
                    DBObject obj = tr.GetObject(settingsDict.GetAt(key), OpenMode.ForRead);
                    var properties = propExtractor.PropertiesFrom(obj.ObjectId);
                    yield return creator.Create(doc, key, properties);
                }
            }
        }
    }
}