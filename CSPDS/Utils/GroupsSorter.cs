using System.Collections.Generic;
using CSPDS.ViewModel;

namespace CSPDS.Utils
{
    public class GroupsSorter<T> where T:IPropertiesHolder
    {
        private readonly string propertyName;

        public GroupsSorter(string propertyName)
        {
            this.propertyName = propertyName;
        }

        public Dictionary<string, List<T>> Group(IEnumerable<T> items)
        {
            Dictionary<string, List<T>> groups = new Dictionary<string, List<T>>();
            foreach (T item in items)
            {
                string value = item[propertyName];
                if(!groups.ContainsKey(value))
                    groups.Add(value, new List<T>());
                
                groups[value].Add(item);
            }

            return groups;
        }
        
    }
}