using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using CSPDS.Data;
using log4net;

namespace CSPDS.ViewModel
{
    /*
     * Узел UI-дерева, группирующий рамки по какому-либо свойству
     * Например - имя файла, формат.
     */
    public class SheetsGroupTreeNode : ISelectable
    {
        private static readonly ILog _log =
            LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        private bool? isSelected = false;
        private readonly string name;
        private readonly ObservableCollection<SheetTreeNode> sheets = new ObservableCollection<SheetTreeNode>();
        public event PropertyChangedEventHandler PropertyChanged;

        public string Name => name;

        public SheetsGroupTreeNode(string name)
        {
            this.name = name;
        }

        public bool? IsSelectedInternal
        {
            get => isSelected;
            set => isSelected = value;
        }

        public bool? IsSelected
        {
            get => isSelected;
            set
            {
                isSelected = value;
                this.OnChangeSelected(value);
            }
        }

        public void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public IEnumerable<ISelectable> Childs()
        {
            return sheets;
        }

        public ObservableCollection<SheetTreeNode> Sheets => sheets;

        public IEnumerable<ISelectable> Parents()
        {
            return Enumerable.Empty<ISelectable>();
        }


        public void Refill(List<SheetTreeNode> newSheets)
        {
            _log.DebugFormat("FillToGroup {0}", newSheets.Count);
            foreach (var sheet in newSheets)
            {
                sheet.AddParent(this);
                sheets.Add(sheet);
            }

            foreach (var sheet in sheets)
            {
                if (!newSheets.Contains(sheet))
                {
                    sheets.Remove(sheet);
                    sheet.RemoveParent(this);
                }
            }
            _log.DebugFormat("In Group {0}", sheets.Count);
            
        }
    }
}