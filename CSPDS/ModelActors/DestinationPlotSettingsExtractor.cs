using Autodesk.AutoCAD.DatabaseServices;
using CSPDS.Model;

namespace CSPDS.Actors
{
    public class DestinationPlotSettingsExtractor
    {
        public PlotSettings SettingsFor(Destination destination)
        {
            Database db = destination.Db;
            PlotSettings plotSettingsForSheet = new PlotSettings(true);

            using (Transaction tr = db.TransactionManager.StartTransaction())
            {
                DBDictionary settingsDict =
                    (DBDictionary) tr.GetObject(db.PlotSettingsDictionaryId, OpenMode.ForRead);
                plotSettingsForSheet.CopyFrom(tr.GetObject((ObjectId) settingsDict.GetAt(destination.DictKey),
                    OpenMode.ForRead));
                return plotSettingsForSheet;
            }
        }
    }
}