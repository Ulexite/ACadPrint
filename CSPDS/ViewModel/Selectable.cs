using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSPDS.Annotations;

namespace CSPDS.Data
{
    /*
     * Хелпер для чекбоксов в дереве
     */
    public interface ISelectable : INotifyPropertyChanged

    {
        bool? IsSelectedInternal { get; set; }
        bool? IsSelected { get; set; }
        void OnPropertyChanged([CallerMemberName] string propertyName = null);

        IEnumerable<ISelectable> Childs();
        IEnumerable<ISelectable> Parents();
    }

    public static class Selectable
    {
        //
        // Must Call from IsSelected=>set
        //
        public static void OnChangeSelected(this ISelectable obj, bool? value)
        {
            foreach (ISelectable child in obj.Childs())
            {
                child.SetSelectedValue(value);
            }

            foreach (ISelectable parent in obj.Parents())
            {
                parent.RecheckChilds();
            }
        }

        public static void SetSelectedValue(this ISelectable obj, bool? value)
        {
            obj.IsSelectedInternal = value;
            obj.OnPropertyChanged("IsSelected");
        }

        public static void RecheckChilds(this ISelectable obj)
        {
            bool SomeSelected = false;
            bool SomeNotSelected = false;
            foreach (ISelectable child in obj.Childs())
            {
                if (child.IsSelectedInternal == true)
                    SomeSelected = true;
                if (child.IsSelectedInternal == false)
                    SomeNotSelected = true;
            }

            if (SomeSelected)
            {
                if (SomeNotSelected)
                    obj.IsSelectedInternal = null;
                else
                    obj.IsSelectedInternal = true;
            }
            else
                obj.IsSelectedInternal = false;

            obj.OnPropertyChanged("IsSelected");
        }
    }
}