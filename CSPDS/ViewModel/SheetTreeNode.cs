using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using CSPDS.Data;
using CSPDS.Model;
using CSPDS.Utils;

namespace CSPDS.ViewModel
{
    /*
     * Узел UI-дерева, соответствующий одной рамке
     * Его можно выбирать
     */
    public class SheetTreeNode:ISelectable, IPropertiesHolder
    {
        private bool? isSelected = false;
        private readonly Sheet sheetData;
        private readonly HashSet<SheetsGroupTreeNode> parents = new HashSet<SheetsGroupTreeNode>();
        private readonly string name;
        
        public event PropertyChangedEventHandler PropertyChanged;

        public SheetTreeNode(Sheet sheetData)
        {
            this.sheetData = sheetData;
            this.name = sheetData["name"];
        }

        public bool? IsSelectedInternal
        {
            get =>isSelected; 
            set =>isSelected = value;
        }

        public bool? IsSelected
        {
            get=>isSelected;
            set
            {
                isSelected = value;
                this.OnChangeSelected(value);
            }
        }

        public Sheet SheetData => sheetData;

        public string Name => name;

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<ISelectable> Childs()
        {
            return Enumerable.Empty<ISelectable>();
        }

        public IEnumerable<ISelectable> Parents()
        {
            return parents;
        }

        public void AddParent(SheetsGroupTreeNode parent)
        {
            parents.Add(parent);
        }
        public void RemoveParent(SheetsGroupTreeNode parent)
        {
            parents.Remove(parent);
        }

        public string Id => sheetData.Id;

        public string this[string key]
        {
            get
            {
                if (sheetData.ContainsKey(key))
                    return sheetData[key];
                return ""; //??
            }
        }
    }
}