using System.Collections.Generic;

namespace CSPDS.Model
{
    public class SheetCache : Dictionary<string, Sheet>
    {
        public void Refill(IEnumerable<Sheet> newState)
        {
            var newSheets = new Dictionary<string, Sheet>();
            foreach (Sheet sheet in newState)
            {
                newSheets.Add(sheet.Id, sheet);
                if (!ContainsKey(sheet.Id))
                    Add(sheet.Id, sheet);
                //TODO: else update oldSheet from newSheet;
            }

            foreach (string oldId in Keys)
            {
                if (!newSheets.ContainsKey(oldId))
                    Remove(oldId);
            }
        }
    }
}