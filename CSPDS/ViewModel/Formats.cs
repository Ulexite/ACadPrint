using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace CSPDS.ViewModel
{
    public class Formats
    {
        private readonly ObservableCollection<FormatDestination> formats = new ObservableCollection<FormatDestination>();
        private readonly HashSet<string> format_names = new HashSet<string>();

        private readonly ModuleUI ui;

        public Formats(ModuleUI ui)
        {
            this.ui = ui;
        }

        public void AddAll(IEnumerable<string> new_formats)
        {
            foreach (var formatName in new_formats)
            {
                if (!format_names.Contains(formatName))
                {
                    format_names.Add(formatName);
                    formats.Add(new FormatDestination(formatName, ui));
                }
            }
        }

        public ObservableCollection<FormatDestination> FormatDestinations => formats;
    }
}