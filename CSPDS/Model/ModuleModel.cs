using System.Collections.Generic;
using Autodesk.AutoCAD.ApplicationServices;
using CSPDS.Actors;
using CSPDS.Annotations;
using CSPDS.Model;
using CSPDS.Utils;
using Application = Autodesk.AutoCAD.ApplicationServices.Core.Application;

namespace CSPDS
{
    /*
     * Runtime нашего плагина
     */

    public class ModuleModel
    {
        private readonly SheetCache sheets;

        private readonly DestinationStorage destinations;

        //TODO: save
        private readonly HashSet<string> knownFormats = new HashSet<string>();

        //Индекс для поиска и контроля что каждому формату задана только одна настройка печати
        private readonly Dictionary<string, Destination> destinationByFormat = new Dictionary<string, Destination>();


        private readonly SheetsExtractor sheetExtractor;

        private readonly DestinationsExtractor destinationsExtractor;
        private readonly GroupsSorter<Sheet> sorter;

        public ModuleModel()
        {
            //TODO: inject logger!
            //DI is for Dependency injection!
            sheets = new SheetCache();
            destinations = new DestinationStorage();
            //TODO:Конфигурация!
            sorter = new GroupsSorter<Sheet>("Формат");
            PropertiesExractor propertiesExractor = new PropertiesExractor();

            sheetExtractor = new SheetsExtractor(propertiesExractor, new SheetCreator(new BoundsCalculator()));
            destinationsExtractor = new DestinationsExtractor(propertiesExractor, new DestinationCreator());
        }

        public void Refresh()
        {
            HashSet<string> currentDocs = new HashSet<string>();
            foreach (Document document in Application.DocumentManager)
            {
                sheets.Refill(sheetExtractor.SheetsFrom(document));
                destinations.Fill(destinationsExtractor.DestinationsFrom(document));
                currentDocs.Add(document.Name);
            }

            List<Sheet> sheetToRemove = new List<Sheet>();
            foreach (var sheet in sheets.Values)
            {
                if (!currentDocs.Contains(sheet.FileName))
                    sheetToRemove.Add(sheet);
            }

            foreach (var sheet in sheetToRemove)
                sheets.Remove(sheet.Id);

            knownFormats.UnionWith(sorter.Group(sheets.Values).Keys);
        }
        
        

        public void SetDestinationForFormat(string format, Destination destination)
        {
            if (destinationByFormat.ContainsKey(format))
            {
                if (!destination.Id.Equals(destinationByFormat[format]))
                {
                    destinationByFormat[format].RemoveFormat(format);
                    destination.AddFormat(format);
                    destinationByFormat[format] = destination;
                }
            }
            else
            {
                destination.AddFormat(format);
                destinationByFormat.Add(format, destination);
            }
        }

        [CanBeNull]
        public Destination GetDestinationForFormat(string format)
        {
            if (destinationByFormat.ContainsKey(format))
                return destinationByFormat[format];
            return null;
        }

        [CanBeNull]
        public Destination GetDestinationForSheet(Sheet sheet)
        {
            if (!(sheet.DestinationId is null))
                return destinations[sheet.DestinationId];

            if (destinationByFormat.ContainsKey(sheet.Format))
                return destinationByFormat[sheet.Format];

            return null;
        }

        public PlotPlan GetPlotPlan()
        {
            return new PlotPlan(this);
        }

        public IEnumerable<Sheet> Sheets => sheets.Values;
        public IEnumerable<Destination> Destinations => destinations.Values;
        public IEnumerable<string> Formats => knownFormats;
    }
}