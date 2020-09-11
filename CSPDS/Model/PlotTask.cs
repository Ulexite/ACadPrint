using CSPDS.Annotations;
using CSPDS.Utils;

namespace CSPDS.Model
{
    public class PlotTask : IPropertiesHolder
    {
        private readonly Sheet sheet;
        [CanBeNull] private readonly Destination destination;

        public PlotTask(Sheet sheet, [CanBeNull] Destination destination)
        {
            this.sheet = sheet;
            this.destination = destination;
        }

        public string this[string key] => sheet[key];

        public Sheet Sheet => sheet;

        [CanBeNull]
        public Destination Destination => destination;
    }
}