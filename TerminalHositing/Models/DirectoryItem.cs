using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace TerminalHositing.Models
{
    public class DirectoryItem : ObservableObject
    {
        private string _name;
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public ObservableCollection<DirectoryItem> SubDirectories { get; set; }

        public DirectoryItem()
        {
            SubDirectories = new ObservableCollection<DirectoryItem>();
        }
    }
}
