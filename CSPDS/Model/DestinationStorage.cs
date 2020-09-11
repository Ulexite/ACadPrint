using System.Collections.Generic;

namespace CSPDS.Model
{
    public class DestinationStorage : Dictionary<string, Destination>
    {
        //TODO: save/load;
        public void Fill(IEnumerable<Destination> list)
        {
            foreach (var destination in list)
            {
                if (!ContainsKey(destination.Id))
                    Add(destination.Id, destination);
            }
        }
    }
}