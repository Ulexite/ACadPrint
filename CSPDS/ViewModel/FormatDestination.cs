using System.ComponentModel;
using System.Runtime.CompilerServices;
using CSPDS.Annotations;
using CSPDS.Model;

namespace CSPDS.ViewModel
{
    public class FormatDestination: INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly string name;
        private readonly ModuleUI ui;
        
        private DestinationVariant destination;

        public FormatDestination(string name, ModuleUI ui)
        {
            this.name = name;
            this.ui = ui;
            this.destination = ui.DestiantionVariants.NotSelected;
        }

        public string Name => name;

        public DestinationVariant Destination
        {
            get => destination;
            set    
            {
                destination = value;
                OnPropertyChanged("Destination");                
                ui.UpdateDestinationForFormat(name, destination.Destination);
            }
        }

        public ModuleUI Ui => ui;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}