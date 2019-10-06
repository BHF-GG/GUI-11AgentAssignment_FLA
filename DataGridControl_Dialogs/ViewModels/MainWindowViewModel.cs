using AgentAssignment;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using System.Xml.Serialization;
using Lab3.Data;
using Lab3.ViewModels;
using Lab3.Views;
using Microsoft.Win32;

namespace Lab3
{
    public class MainWindowViewModel : BindableBase
    {
        private ObservableCollection<Agent> agents;
        string filePath = "";
        private string AppTitle = "Agent Assignments 5.3";

        //Bruges til filtrering af specialities
        private string filter;

        //Bruges til uret
        DispatcherTimer timer = new DispatcherTimer();


        public MainWindowViewModel()
        {
            agents = new ObservableCollection<Agent>()
            {
                //Indsættes ved debug
#if DEBUG
                new Agent("001", "Nina", "Assassination", "UpperVolta"),
                new Agent("007", "James Bond", "Martinis", "North Korea"),
                new Agent("008", "Jim Bond", "Bombs", "Denmark"),
                new Agent("009", "James BLOND", "Assassination", "North Korea"),
                new Agent("010", "James Bomb", "Bombs", "North Korea")

#endif
            };
            CurrentAgent = null;

            //Exercise 4.3.1: Lav comboboks ved specialities
            Specialities = new ObservableCollection<string>
            {
                "Assassination",
                "Bombs",
                "Martinis",
                "Low profile",
                "Spy"
            };

            //Bruges til ur
            timer.Interval = TimeSpan.FromSeconds(1);
            timer.Tick += new EventHandler(Timer_Tick);
            timer.Start();
        }

        Agent currentAgent = null;

        //Virker ikke
        Clock clock = new Clock();

        public Clock Clock
        {
            get => clock;
            set => clock = value;
        }

        void Timer_Tick(object sender, EventArgs e)
        {
            Clock.Update();
        }

        private string filename = "";
        public string Filename
        {
            get { return filename; }
            set
            {
                SetProperty(ref filename, value);
                RaisePropertyChanged("Title");
            }
        }

        public string Title
        {
            get
            {
                var s = "";
                if (Dirty)
                    s = "*";
                return filename + s + " - " + AppTitle;
            }
        }

        private bool dirty = false;
        public bool Dirty
        {
            get { return dirty; }
            set
            {
                SetProperty(ref dirty, value);
                RaisePropertyChanged("Title");
            }
        }

        ///     Exercise 4.3.1: Lav comboboks ved specialities
        ///     Jeg laver en property (Specialities) i ViewModel som returnerer en observableCollection
        ///     af string. Og denne collection initierer jeg i constructoren med nogle
        ///     passende strenge (specialities).
        ///

        ObservableCollection<string> specialities;
        public ObservableCollection<string> Specialities
        {
            get { return specialities; }
            set
            {
                SetProperty(ref specialities, value);
            }
        }


        public ObservableCollection<Agent> Agents
        {
            get { return agents; }
            set { SetProperty(ref agents, value); }
        }

        /// <summary>
        ///     Exercise 4.5.1:
        ///     Tilføjer label og combobox til valg af filter i toolbaren.
        ///     ItemsSource bindes til en property i ViewModel klassen som
        ///     returnerer en ReadOnly collection af specialities baseret
        ///     på specialities klassen. Men da brugeren her også skal
        ///     kunne vælge alle (det samme som ingen filter) tilføjes
        ///     ”All” som første element.
        /// </summary>
        public IReadOnlyCollection<string> FilterSpecialities
        {
            get
            {
                ObservableCollection<string> result = new ObservableCollection<string>();
                result.Add("All");
                result.AddRange(Specialities);
                return result;
            }
        }

        //Exercise 4.5.2: SelectedIndex bindes til en ny CurrentSpecialityIndex i ViewModel.
        

        int currentSpecialityIndex = 0;

        //Exercise 4.5.3:
        //  Selve filtreringen configureres
        //  i CurrentSpecialityIndex’s Setter – eller rettere i
        //  denne aktiverer CollectionViews filtreringsmekanisme.
        //  Dog ikke hvis selected index er 0 (= all). I dette
        //  tilfælde fjernes filterfunktionen:

        public int CurrentSpecialityIndex
        {
            get { return currentSpecialityIndex; }
            set
            {
                if (currentSpecialityIndex != value)
                {
                    ICollectionView cv = CollectionViewSource.GetDefaultView(Agents);
                    currentSpecialityIndex = value;
                    if (currentSpecialityIndex == 0)
                        cv.Filter = null; // Index 0 is no filter (show all)
                    else
                    {
                        filter = Specialities[(currentSpecialityIndex - 1)];
                        cv.Filter = CollectionViewSource_Filter;
                    }
                    RaisePropertyChanged();
                }
            }
        }

        //Exercise 4.5.4: Selve filter-funktionen (predicate) som indsættes i CollectionView’et:

        private bool CollectionViewSource_Filter(object ag)
        {
            Agent agent = ag as Agent;
            return (agent.Speciality == filter);
        }



        public Agent CurrentAgent
        {
            get { return currentAgent; }
            set { SetProperty(ref currentAgent, value); }
        }

        // Opretter CurrentIndex
        int currentIndex = -1;

        public int CurrentIndex
        {
            get { return currentIndex; }
            set { SetProperty(ref currentIndex, value); }
        }



        /// <summary>
        ///     Exercise 4.4.2:
        ///     I eventhandleren for comboboxens
        ///     SelectionChanged event sættes SortDescription
        ///     for bindingens CollectionViewSource til den valgte værdi:
        /// </summary>

        Object _selectedSortOrder = "None";
        public Object SelectedSortOrder
        {
            get { return _selectedSortOrder; }
            set
            {
                SetProperty(ref _selectedSortOrder, value);
                ICollectionView cv = CollectionViewSource.GetDefaultView(Agents);
                var newSortOrder = value as ComboBoxItem;
                var sortDesc = new SortDescription(newSortOrder.Tag.ToString(), ListSortDirection.Ascending);
                if (cv != null)
                {
                    cv.SortDescriptions.Clear();
                    if (newSortOrder.Tag.ToString() != "None")
                        cv.SortDescriptions.Add(sortDesc);
                }
            }
        }


        //Trækker en fra CurrentIndex
        private void PreviusCommandExecute()
        {
            --CurrentIndex;
        }

        //Tjekker om CurrentIndex > 0 (true / false)
        private bool PreviusCommandCanExecute()
        {
            return CurrentIndex > 0;
        }



        //Opretter en Icommand
        ICommand _PreviusCommand;

        //Opretter en tilhørende property
        //Get funktionen tjekker om Previoscommand kan eksekvere, så kalder den 
        //PreviusCommandExecute og et eller andet med .OberservesProperty
        // -- se videoen
        public ICommand PreviusCommand
        {
            get
            {
                return _PreviusCommand ??
                       (_PreviusCommand = new DelegateCommand(
                               PreviusCommandExecute, PreviusCommandCanExecute)
                           .ObservesProperty(() => CurrentIndex));
            }
        }

        ICommand _nextCommand;

        public ICommand NextCommand
        {
            get
            {
                return _nextCommand ?? (_nextCommand = new DelegateCommand(
                           () => ++CurrentIndex,
                           () => CurrentIndex < (Agents.Count - 1)).ObservesProperty(() => CurrentIndex));
            }
        }

        ICommand _newCommand;

        public ICommand AddNewAgentCommand
        {
            get
            {
                return _newCommand ?? (_newCommand = new DelegateCommand(() =>
                {
                    var newAgent = new Agent();
                    var vm = new AgentWindowViewModel("Add new agent", newAgent)
                    {
                        Specialities = specialities
                    };
                    var dlg = new AgentWindow
                    {
                        DataContext = vm
                    };
                    if (dlg.ShowDialog() == true)
                    {
                        Agents.Add(newAgent);
                        CurrentAgent = newAgent;
                        CurrentSpecialityIndex = 0;
                        Dirty = true;
                    }
                }));
            }
        }

        ICommand _editCommand;
        public ICommand EditAgentCommand
        {
            get
            {
                return _editCommand ?? (_editCommand = new DelegateCommand(() =>
                           {
                               var tempAgent = CurrentAgent.Clone();
                               var vm = new AgentWindowViewModel("Edit agent", tempAgent)
                               {
                                   Specialities = specialities
                               };
                               var dlg = new AgentWindow
                               {
                                   DataContext = vm,
                                   Owner = App.Current.MainWindow
                               };
                               if (dlg.ShowDialog() == true)
                               {
                                   // Copy values back
                                   CurrentAgent.ID = tempAgent.ID;
                                   CurrentAgent.CodeName = tempAgent.CodeName;
                                   CurrentAgent.Speciality = tempAgent.Speciality;
                                   CurrentAgent.Assignment = tempAgent.Assignment;
                                   Dirty = true;
                               }
                           },
                           () => {
                               return CurrentIndex >= 0;
                           }
                       ).ObservesProperty(() => CurrentIndex));
            }
        }

        ICommand _deleteCommand;
        public ICommand DeleteAgentCommand => _deleteCommand ?? (_deleteCommand =
                                                  new DelegateCommand(DeleteAgent, DeleteAgent_CanExecute)
                                                      .ObservesProperty(() => CurrentIndex));


        private void DeleteAgent()
        {
            MessageBoxResult res = MessageBox.Show("Are you sure you want to delete agent " + CurrentAgent.CodeName +
                                                   "?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.Yes)
            {
                Agents.Remove(CurrentAgent);
                Dirty = true;
            }
        }

        private bool DeleteAgent_CanExecute()
        {
            if (Agents.Count > 0 && CurrentIndex >= 0)
                return true;
            else
                return false;
        }

        ICommand _closeAppCommand;
        public ICommand CloseAppCommand
        {
            get
            {
                return _closeAppCommand ?? (_closeAppCommand = new DelegateCommand(() =>
                {
                    Application.Current.MainWindow.Close();
                }));
            }
        }

        ICommand _closingCommand;
        public ICommand ClosingCommand
        {
            get
            {
                return _closingCommand ?? (_closingCommand = new
                           DelegateCommand<CancelEventArgs>(ClosingCommand_Execute));
            }
        }

        private void ClosingCommand_Execute(CancelEventArgs arg)
        {
            if (Dirty)
                arg.Cancel = UserRegrets();
        }

        private bool UserRegrets()
        {
            var regret = false;
            MessageBoxResult res = MessageBox.Show("You have unsaved data. Are you sure you want to close the application without saving data first?",
                "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.No)
            {
                regret = true;
            }
            return regret;
        }

        ICommand _SaveAsCommand;
        public ICommand SaveAsCommand
        {
            get { return _SaveAsCommand ?? (_SaveAsCommand = new DelegateCommand<string>(SaveAsCommand_Execute)); }
        }

        private void SaveAsCommand_Execute(string argFilename)
        {
            var dialog = new SaveFileDialog
            {
                Filter = "Agent assignment documents|*.agn|All Files|*.*",
                DefaultExt = "agn"
            };
            if (filePath == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                dialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                filePath = dialog.FileName;
                Filename = Path.GetFileName(filePath);
                SaveFile();
            }
        }

        ICommand _SaveCommand;
        public ICommand SaveCommand
        {
            get
            {
                return _SaveCommand ?? (_SaveCommand = new DelegateCommand(SaveFileCommand_Execute, SaveFileCommand_CanExecute)
                  .ObservesProperty(() => Agents.Count));
            }
        }

        private void SaveFileCommand_Execute()
        {
            SaveFile();
        }

        private bool SaveFileCommand_CanExecute()
        {
            return (filename != "") && (Agents.Count > 0);
        }

        private void SaveFile()
        {
            try
            {
                Repository.SaveFile(filePath, Agents);
                Dirty = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Unable to save file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }



        ICommand _NewFileCommand;
        public ICommand NewFileCommand
        {
            get { return _NewFileCommand ?? (_NewFileCommand = new DelegateCommand(NewFileCommand_Execute)); }
        }

        private void NewFileCommand_Execute()
        {
            MessageBoxResult res = MessageBox.Show("Any unsaved data will be lost. Are you sure you want to initiate a new file?", "Warning",
                MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No);
            if (res == MessageBoxResult.Yes)
            {
                Agents.Clear();
                Filename = "";
                Dirty = false;
            }
        }


        ICommand _OpenFileCommand;
        public ICommand OpenFileCommand
        {
            get { return _OpenFileCommand ?? (_OpenFileCommand = new DelegateCommand<string>(OpenFileCommand_Execute)); }
        }

        private void OpenFileCommand_Execute(string argFilename)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "Agent assignment documents|*.agn|All Files|*.*",
                DefaultExt = "agn"
            };
            if (filePath == "")
                dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            else
                dialog.InitialDirectory = Path.GetDirectoryName(filePath);

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                filePath = dialog.FileName;
                Filename = Path.GetFileName(filePath);
                try
                {
                    Repository.ReadFile(filePath, out ObservableCollection<Agent> tempAgents);
                    Agents = tempAgents;
                    Dirty = false;
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unable to open file", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        //Delopgave 2 - colorcommand
        SolidColorBrush newBrush = SystemColors.WindowBrush; // Default color

        private void ColorCommand_Execute(String colorStr)
        {
            SolidColorBrush newBrush = SystemColors.WindowBrush; // Default color

            if (colorStr != null)
        {
            if (colorStr != "Default")
            {
                newBrush = new SolidColorBrush((Color)
                    ColorConverter.ConvertFromString(colorStr));
                Application.Current.Resources["BackgroundBrush"] = newBrush;
            }
        }
    }

        ICommand _ColorCommand;

        public ICommand ColorCommand
        {
            get
            {
                return _ColorCommand ?? (_ColorCommand = new
                           DelegateCommand<String>(ColorCommand_Execute));
            }
        }

        //Udekommenteret: Magen til ovenstående blot med try catch

        //private void ColorCommand_Execute(String colorStr)
        //{
        //    SolidColorBrush newBrush = SystemColors.WindowBrush; // Default color

        //    try
        //    {
        //        if (colorStr != null)
        //        {
        //            if (colorStr != "Default")
        //                newBrush = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorStr));
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        MessageBox.Show("Unknown color name, default color is used", "Program error!");
        //    }

        //    Application.Current.Resources["BackgroundBrush"] = newBrush;
        //}

        
    }

}
