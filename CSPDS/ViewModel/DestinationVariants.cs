using System.Collections.Generic;
using System.Collections.ObjectModel;
using CSPDS.Model;

namespace CSPDS.ViewModel
{
    public class DestinationVariants
    {
        private readonly ObservableCollection<DestinationVariant> variants =
            new ObservableCollection<DestinationVariant>();

        private readonly HashSet<string> destinationIds = new HashSet<string>();
        private readonly DestinationVariant not_selected;

        public DestinationVariants()
        {
            not_selected = new DestinationVariant("<       Не выбрано       >", null);
            variants.Add(not_selected);
        }

        public ObservableCollection<DestinationVariant> Variants => variants;

        public DestinationVariant NotSelected => not_selected;

        public void AddAll(IEnumerable<Destination> destinations)
        {
            foreach (var destination in destinations)
            {
                if (!destinationIds.Contains(destination.Id))
                {
                    destinationIds.Add(destination.Id);
                    variants.Add(new DestinationVariant(destination.Name, destination));
                }
            }
        }
    }
}