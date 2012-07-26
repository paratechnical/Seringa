using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Siringa.Engine.Interfaces;
using System.Reflection;
using System.Windows.Controls;
using System.Threading;
using System.Text;
using Siringa.Engine.Utils;
using System.Collections.ObjectModel;
using Siringa.GUI.Extensions;

namespace Siringa.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private
        #region Fields
        private bool _stopCurrentAction = false;
        private IList<IInjectionStrategy> _injectionStrategies = null;
        private IList<Type> _concreteInjectionStrategyTypes = null;
        private IInjectionStrategy _currentInjectionStrategy = null;
        #endregion Fields
        #region Methods
        private void PopulateInjectionStrategies()
        {
            if(_injectionStrategies != null)
                _injectionStrategies.Clear();

            var interfaceType = typeof(IInjectionStrategy);
            var concreteTypes = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => p!=interfaceType && interfaceType.IsAssignableFrom(p));


            foreach (var concreteType in concreteTypes)
            {
                if (!_concreteInjectionStrategyTypes.Contains(concreteType))
                {
                    _injectionStrategies.Add((IInjectionStrategy)Activator.CreateInstance(concreteType));
                    _concreteInjectionStrategyTypes.Add(concreteType);
                }
            }
        }

        private void Initializations()
        {
            _injectionStrategies = new List<IInjectionStrategy>();
            _concreteInjectionStrategyTypes = new List<Type>();
            DatabaseNames = new ObservableCollection<string>();
            TableNames = new  ObservableCollection<string>();
            ColumnNames = new ObservableCollection<string>();
            //ItemsSource="{Binding Path=DatabaseNames}"
            lbDatabases.ItemsSource = DatabaseNames;
            lbTables.ItemsSource = TableNames;
            lbColumns.ItemsSource = ColumnNames;
        }

        private void ClearAll()
        {
            txtCustomQueryResult.Text = string.Empty;
            txtVersion.Text = string.Empty;
            txtVulnerable.Text = string.Empty;
            txtUser.Text = string.Empty;
            DatabaseNames.Clear();
            TableNames.Clear();
            ColumnNames.Clear();
        }

        private void EnableAllFromOtherThread()
        {
            if (!gridSingleUrl.Dispatcher.CheckAccess())
            {

                gridSingleUrl.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        EnableAll();
                    }
                ));
            }
            else
            {
                EnableAll();
            }
        }

        private void EnableAll()
        {
            btnCheckIfVulnerable.IsEnabled = true;
            btnColumns.IsEnabled = true;
            btnDatabases.IsEnabled = true;
            btnDebugLast.IsEnabled = true;
            btnExecuteCustomQuery.IsEnabled = true;
            btnGetUser.IsEnabled = true;
            btnGetVersion.IsEnabled = true;
            btnTables.IsEnabled = true;
            bntCurDb.IsEnabled = true;
        }

        private void DisableAll()
        {
            btnCheckIfVulnerable.IsEnabled = false;
            btnColumns.IsEnabled = false;
            btnDatabases.IsEnabled = false;
            btnDebugLast.IsEnabled = false;
            btnExecuteCustomQuery.IsEnabled = false;
            btnGetUser.IsEnabled = false;
            btnGetVersion.IsEnabled = false;
            btnTables.IsEnabled = false;
            bntCurDb.IsEnabled = false;
        }

        private void UrlOrStrategyChange()
        {
            if (!string.IsNullOrEmpty(txtUrl.Text) && UrlHelper.ValidUrl(txtUrl.Text) && _currentInjectionStrategy != null)
            {
                _currentInjectionStrategy.Url = txtUrl.Text;
                EnableAll();
                ClearAll();
            }
            else
                DisableAll();
        }

        private string GenerateProperOutput(string textBoxContent, string text, bool append, bool newLineAfterText)
        {
            var sb = new StringBuilder();
            if (append)
                sb.Append(textBoxContent);
            sb.Append(text);
            if (newLineAfterText)
                sb.Append(Environment.NewLine);
            return sb.ToString();
        }

        private void AddOutputToTextBox(TextBox textBox,string text,bool append,bool newLineAfterText)
        {
            if (!textBox.Dispatcher.CheckAccess())
            {

                textBox.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        textBox.Text = GenerateProperOutput(textBox.Text, text, append, newLineAfterText);
                    }
                ));
            }
            else
            {
                textBox.Text = GenerateProperOutput(textBox.Text, text, append, newLineAfterText);
            }
        }

        #endregion Methods
        #endregion Private

        #region Public

        #region Dependency Properties
        public static readonly DependencyProperty DatabaseNamesProperty = DependencyProperty.Register("DatabaseNames", 
                                                                                                        typeof(ObservableCollection<String>), 
                                                                                                        typeof(MainWindow));

        public ObservableCollection<String> DatabaseNames
        {
            get {  return (ObservableCollection<String>)GetValue(DatabaseNamesProperty); }
            set { SetValue(DatabaseNamesProperty, value); }
        }

        public static readonly DependencyProperty TableNamesProperty = DependencyProperty.Register("TableNames",
                                                                                                        typeof(ObservableCollection<String>),
                                                                                                        typeof(MainWindow));

        public ObservableCollection<String> TableNames
        {
            get { return (ObservableCollection<String>)GetValue(TableNamesProperty); }
            set { SetValue(TableNamesProperty, value); }
        }

        public static readonly DependencyProperty ColumnNamesProperty = DependencyProperty.Register("ColumnNames",
                                                                                                        typeof(ObservableCollection<String>),
                                                                                                        typeof(MainWindow));

        public ObservableCollection<String> ColumnNames
        {
            get { return (ObservableCollection<String>)GetValue(ColumnNamesProperty); }
            set { SetValue(ColumnNamesProperty, value); }
        }

        #endregion


        public IInjectionStrategy CurrentInjectionStrategy
        {
            get
            {
                return _currentInjectionStrategy;
            }
        }
        #endregion Public

        #region Constructors

        public MainWindow()
        {
            InitializeComponent();
            Initializations();
            PopulateInjectionStrategies();
            DisableAll();
        }

        #endregion Constructors

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbCurrentInjectionStrategy.DataContext = _injectionStrategies.Select(i => i.DisplayName).ToList();
        }

        private void cbCurrentInjectionStrategy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _currentInjectionStrategy = (IInjectionStrategy)_injectionStrategies[cbCurrentInjectionStrategy.SelectedIndex];    
            UrlOrStrategyChange();
        }

        private void btCheckIfVulnerable_Click(object sender, RoutedEventArgs e)
        {
            DisableAll();
            var th = new Thread(() =>
            {
                string injectionResult = "No";

                if (_currentInjectionStrategy.TestIfVulnerable())
                    injectionResult = "Yes";

                AddOutputToTextBox(txtVulnerable, injectionResult, false, false);
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void btnGetVersion_Click(object sender, RoutedEventArgs e)
        {
            DisableAll();
            var th = new Thread(() =>
            {
                string injectionResult = _currentInjectionStrategy.GetDbVersion();
                AddOutputToTextBox(txtVersion, injectionResult, false, false);
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void bntCurDb_Click(object sender, RoutedEventArgs e)
        {
            DisableAll();
            var th = new Thread(() =>
            {
                string injectionResult = _currentInjectionStrategy.GetCurrentDbName();
                AddOutputToTextBox(txtCurDb, injectionResult, false, false);
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void btnGetUser_Click(object sender, RoutedEventArgs e)
        {
            DisableAll();
            var th = new Thread(() =>
            {
                string injectionResult = _currentInjectionStrategy.GetDbUserName();
                AddOutputToTextBox(txtUser, injectionResult, false, false);
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void txtUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            DisableAll();
        }

        private void txtUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            UrlOrStrategyChange();
        }

        private void btnDatabases_Click(object sender, RoutedEventArgs e)
        {
            DisableAll();
            var th = new Thread(() =>
            {
                string result = string.Empty;
                int total = _currentInjectionStrategy.GetTotalNoOfDbs();
                for (int i = 0; i < total; i++)
                {
                    if (_stopCurrentAction)
                        break;
                    result = _currentInjectionStrategy.GetSingleDatabaseName(i);
                    if (!string.IsNullOrEmpty(result))
                    {
                        lbDatabases.Dispatcher.Invoke(
                              System.Windows.Threading.DispatcherPriority.Normal,
                              new Action(
                                delegate()
                                {
                                    DatabaseNames.Add(result);
                                }
                            ));
                    }
                }
                _stopCurrentAction = false;
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void btnStopCurAction_Click(object sender, RoutedEventArgs e)
        {
            _stopCurrentAction = true;
        }


        private void btnTables_Click(object sender, RoutedEventArgs e)
        {
            DisableAll();
            var th = new Thread(() =>
            {
                string result = string.Empty;
                int total = _currentInjectionStrategy.GetTotalNoOfTables();
                for (int i = 0; i < total; i++)
                {
                    if (_stopCurrentAction)
                        break;
                    result = _currentInjectionStrategy.GetSingleTableName(i);
                    if (!string.IsNullOrEmpty(result))
                    {
                        lbTables.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new Action(
                                delegate()
                                {
                                    TableNames.AddOnUI(result);
                                }
                            ));
                    }
                }
                _stopCurrentAction = false;
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void lbDatabases_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            CurrentInjectionStrategy.SelectedDb = lbDatabases.SelectedValue.ToString();
        }

        #endregion Events

        
    }
}
