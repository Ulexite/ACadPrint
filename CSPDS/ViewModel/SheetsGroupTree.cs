using System.Collections.Generic;
using System.Collections.ObjectModel;
using CSPDS.Utils;

namespace CSPDS.ViewModel
{
    public class SheetsGroupTree
    {
        private readonly GroupsSorter<SheetTreeNode> sorter;

        private readonly ObservableCollection<SheetsGroupTreeNode> groups =
            new ObservableCollection<SheetsGroupTreeNode>();

        private readonly Dictionary<string, SheetsGroupTreeNode> groupById =
            new Dictionary<string, SheetsGroupTreeNode>();

        private readonly SheetTreeNodeCache nodeCache;


        public SheetsGroupTree(string groupBy, SheetTreeNodeCache nodeCache)
        {
            this.sorter = new GroupsSorter<SheetTreeNode>(groupBy);
            this.nodeCache = nodeCache;
        }

        public ObservableCollection<SheetsGroupTreeNode> Groups => groups;

        public void Refill()
        {
            Dictionary<string, List<SheetTreeNode>> newGroups = sorter.Group(nodeCache.Values);

            foreach (var groupId in groupById.Keys)
            {
                if (!newGroups.ContainsKey(groupId))
                {
                    groups.Remove(groupById[groupId]);
                    groupById.Remove(groupId);
                }
            }

            foreach (var groupId in newGroups.Keys)
            {
                if (!groupById.ContainsKey(groupId))
                {
                    SheetsGroupTreeNode newGroup = new SheetsGroupTreeNode(groupId);
                    groups.Add(newGroup);
                    groupById.Add(groupId, newGroup);
                }

                groupById[groupId].Refill(newGroups[groupId]);
            }
        }
    }
}