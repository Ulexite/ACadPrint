using System.Collections.Generic;
using CSPDS.Model;

namespace CSPDS.ViewModel
{
    public class SheetTreeNodeCache : Dictionary<string, SheetTreeNode>
    {
        
        //TODO: genetric with sheetCAche
        public void Add(Sheet sheet)
        {
            if (!ContainsKey(sheet.Id))
            {
                Add(sheet.Id, new SheetTreeNode(sheet));
            }
        }

        public void Refill(IEnumerable<Sheet> sheets)
        {
            HashSet<string> sheetIds = new HashSet<string>();
            foreach (var sheet in sheets)
            {
                Add(sheet);
                sheetIds.Add(sheet.Id);
            }

            foreach (string oldId in Keys)
            {
                if (!sheetIds.Contains(oldId))
                    Remove(oldId);
            }
        }
    }
}