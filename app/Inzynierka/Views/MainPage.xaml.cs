using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

using Inzynierka.CommunicationService;
using Inzynierka.DatabaseAccess;
using Inzynierka.Exceptions;
using Inzynierka.Models;
using Inzynierka.Services;

using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Core.Preview;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

using WinUI = Microsoft.UI.Xaml.Controls;

using System.Text.Json;

namespace Inzynierka.Views
{
    public class Ints
    {
        public string a { get; set; }
        public string b { get; set; }
    }


    public sealed partial class MainPage : Page, INotifyPropertyChanged
    {
        private object _selectedItem;

        public object SelectedItem
        {
            get { return _selectedItem; }
            set { Set(ref _selectedItem, value); }
        }
        
        public ObservableCollection<FolderItem> Directories { get; } = new ObservableCollection<FolderItem>();


        public MainPage()
        {
            MainThreadDispatcherService.Initialize(Window.Current.Dispatcher);
            InitializeThings();
            InitializeComponent();

            SystemNavigationManagerPreview.GetForCurrentView().CloseRequested += App_CloseRequested;

        }


        private async void InitializeThings()
        {


            var o = JsonSerializer.Deserialize<Ints>("{\"a\":\"10\",\"b\":\"20\"}");

            var message = new MessageDialog(o.a);
            message.Commands.Add(new UICommand("Close"));
            message.DefaultCommandIndex = 0;
            message.CancelCommandIndex = 0;
            await message.ShowAsync();

            if (!BackendConctroller.Initialized)
            {
                try
                {
                    await BackendConctroller.Initialize(Window.Current.Dispatcher, DatabaseAccessService.DatabaseFilePath);
                }
                catch (BackendControllerInitializationException exception)
                {
                    var messageDialog = new MessageDialog("Error establishing connection with python: " + exception.Message);
                    messageDialog.Commands.Add(new UICommand("Close"));
                    messageDialog.DefaultCommandIndex = 0;
                    messageDialog.CancelCommandIndex = 0;
                    await messageDialog.ShowAsync();
                }
            }


        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void Set<T>(ref T storage, T value, [CallerMemberName]string propertyName = null)
        {
            if (Equals(storage, value))
            {
                return;
            }

            storage = value;
            OnPropertyChanged(propertyName);
        }

        private void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void TreeViewPage_ItemSelected(object sender, TreeViewItemSelectedEventArgs e)
        {
            if (e.Parameter is object)
            {
                imageGalleryPage.AccessFolder(e.Parameter);
            }
        }

        private async void imageGalleryPage_ImageClicked(object sender, EventArgs e)
        {
            await imageDetailPage.ShowAsync();
        }

        private void imageGalleryPage_AccessedFolderContetsChanged(object sender, EventArgs e)
        {

        }

        private void App_CloseRequested(object sender, SystemNavigationCloseRequestedPreviewEventArgs e)
        {
            BackendConctroller.SendCloseApp();
        }
    }
}
