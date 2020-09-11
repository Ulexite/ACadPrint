using CSPDS.Annotations;
using CSPDS.Model;

namespace CSPDS.ViewModel
{
    public class DestinationVariant
    {
        private readonly string name;
        [CanBeNull] private readonly Destination destination;

        public DestinationVariant(string name, [CanBeNull] Destination destination)
        {
            this.name = name;
            this.destination = destination;
        }

        public string Name => name;

        [CanBeNull]
        public Destination Destination => destination;
    }
}