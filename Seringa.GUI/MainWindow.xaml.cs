using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Seringa.Engine.Interfaces;
using System.Windows.Controls;
using System.Threading;
using System.Text;
using Seringa.Engine.Utils;
using System.Collections.ObjectModel;
using Seringa.Engine.Implementations.Proxy;
using Seringa.GUI.Extensions;
using Seringa.Engine.DataObjects;
using Seringa.Engine.Enums;
using System.IO;
using System.Xml.Linq;
using System.Windows.Data;
using System.Xml;
using Seringa.GUI.Helpers;

namespace Seringa.GUI
{

    //xmlHelpers - clasa care sa citeasca xml-ul cu payloads si pe ala cu exploits si sa poroduca query-uri in functie
    //de strategia de injectare
    //2 strategii de injectare: error based, union based 
    //2 dropdownuri injection strategy si exploit
    //inca un dropdown payloads
    //cand apesi execute generatedPayload sa apara rezultatele in customqueryresult redenumit query result
    //cred ca scot alea 3 prostii cu coloane si table de tot
    //mai bine pun un textarea cu un xml sa se vada xml-ul generat de query-uri care va fi harta bazei de date(structura)
    //vezi parametru add to map de pe xml payloads
    //trebuie sa fie ceva care sa se actualizeze in timp real pe gui pe masura ce e scris in xml
    //pt generarea xml-urilor ar fi marfa sa am asa ceva http://www.liquid-technologies.com/xmldatabinding/xml-schema-to-cs.aspx
    //ar fi o idee buna si de alt proiect open source
    //daca nu le fac to msxml tot cum scrie acolo

    //daca bagi adresa de proxy aiurea si il pornesti crapa

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Private
        #region Fields
        private XmlTreeViewItem _selectedTreeViewItem = null;
        private bool _stopCurrentAction = false;
        private IList<IInjectionStrategy> _injectionStrategies = null;
        private IList<Type> _concreteInjectionStrategyTypes = null;
        private IInjectionStrategy _currentInjectionStrategy = null;
        private IIPObtainerStrategy _currentIpObtainerStrategy = null;
        private PayloadDetails _currentPayload = null;
        private ExploitDetails _currentExploit = null;
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
                    var strategy = (IInjectionStrategy)Activator.CreateInstance(concreteType);
                    //@TODO: this value should come from application options
                    strategy.DetailedExceptions = true;
                    _injectionStrategies.Add(strategy);
                    _concreteInjectionStrategyTypes.Add(concreteType);
                }
            }
        }

        private void Initializations()
        {
            _injectionStrategies = new List<IInjectionStrategy>();
            _concreteInjectionStrategyTypes = new List<Type>();
            //DatabaseNames = new ObservableCollection<string>();
            //TableNames = new  ObservableCollection<string>();
            //ColumnNames = new ObservableCollection<string>();
            ////ItemsSource="{Binding Path=DatabaseNames}"
            //lbDatabases.ItemsSource = DatabaseNames;
            //lbTables.ItemsSource = TableNames;
            //lbColumns.ItemsSource = ColumnNames;
            _currentIpObtainerStrategy = new Seringa.Engine.Implementations.IPObtainers.CMyIPObtainerStrategy();
            UIHelpers.ClearTreeView(tvDs);
            cmbProxyType.SelectedValue = ProxyType.None;
        }

        private void ClearAll()
        {
            txtCustomQueryResult.Text = string.Empty;
            UIHelpers.ClearTreeView(tvDs);
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
            //TODO: add treeview here
            btnExecuteCustomQuery.IsEnabled = true;
            //cbDbms.IsEnabled = true;
            //cbPayloads.IsEnabled = true;
            //cbExploits.IsEnabled = true;
        }

        private void DisableAll()
        {
            //TODO: add treeview here
            btnExecuteCustomQuery.IsEnabled = false;
            //cbDbms.IsEnabled = false;
            //cbPayloads.IsEnabled = false;
            //cbExploits.IsEnabled = false;
        }

        private void PopulatePayloads(string dbms)
        {
            string xpath = "";
            StringBuilder sb = new StringBuilder();
            sb.Append("/payloads/payload[@dbms = \"");
            sb.Append(dbms);
            sb.Append("\"]");
            xpath = sb.ToString();

            cbPayloads.DataContext = XmlHelpers.GetValuesFromDocByXpath(FileHelpers.GetCurrentDirectory() + "\\xml\\payloads.xml",
                                                                    xpath, 
                                                                    "user-friendly-name");
        }

        private void PopulateExploits(string dbms, IInjectionStrategy injectionStrategy)
        {
            string xpath = "";
            StringBuilder sb = new StringBuilder();
            sb.Append("/exploits/exploit[@dbms = \"");
            sb.Append(dbms);
            sb.Append("\" and @injection-strategy = \"");
            sb.Append(injectionStrategy!=null?injectionStrategy.GetType().Name:string.Empty);
            sb.Append("\"]");
            xpath = sb.ToString();
             
            cbExploits.DataContext = XmlHelpers.GetValuesFromDocByXpath(FileHelpers.GetCurrentDirectory() + "\\xml\\exploits.xml",
                                                                            xpath, "user-friendly-name");
        }

        private void PopulateDbms()
        {
            cbDbms.DataContext = XmlHelpers.GetAllAttributeValuesFromDoc(FileHelpers.GetCurrentDirectory() + "\\xml\\payloads.xml", 
                                                                            "payload", "dbms");
        }

        private void UrlOrStrategyChange()
        {
            if (!string.IsNullOrEmpty(txtUrl.Text) && UrlHelper.ValidUrl(txtUrl.Text) && _currentInjectionStrategy != null)
            {
                _currentInjectionStrategy.Url = txtUrl.Text;
                _currentPayload = null;

                PopulateExploits(cbDbms.SelectedValue!=null?cbDbms.SelectedValue.ToString():string.Empty, 
                                _currentInjectionStrategy);

                ProxifyInjectionStrategy();
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
            PopulateDbms();
            DisableAll();
        }

        #endregion Constructors

        #region Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbCurrentInjectionStrategy.DataContext = _injectionStrategies.Select(i => i.DisplayName).ToList();
        }

        #endregion Events



    }
}
