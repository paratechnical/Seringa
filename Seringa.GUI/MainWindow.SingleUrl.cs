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
using Seringa.GUI.Static;
using System.Dynamic;

namespace Seringa.GUI
{
    public partial class MainWindow
    {
        #region Private
        #region Fields
        private XmlTreeViewItem _selectedTreeViewItem = null;
        private bool _stopCurrentActionSingleUrlTab = false;
        private IList<IInjectionStrategy> _injectionStrategies = null;
        private IList<Type> _concreteInjectionStrategyTypes = null;
        private IInjectionStrategy _currentInjectionStrategy = null;
        #endregion Fields

        #region Methods
        private void PopulateInjectionStrategies()
        {
            if (_injectionStrategies != null)
                _injectionStrategies.Clear();

            var interfaceType = typeof(IInjectionStrategy);
            var concreteTypes = AppDomain.CurrentDomain.GetAssemblies().ToList()
                .SelectMany(s => s.GetTypes())
                .Where(p => p != interfaceType && interfaceType.IsAssignableFrom(p));


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
            _currentIpObtainerStrategy = new Seringa.Engine.Implementations.IPObtainers.SimpleIPObtainerStrategy();
            UIHelpers.ClearTreeView(tvDs);
            cmbProxyType.SelectedValue = ProxyType.None;
            btnAutodetect.IsEnabled = false;
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
            //btnGetUrls.IsEnabled = true;

            //TODO: add treeview here
            btnExecuteCustomQuery.IsEnabled = true;
            btnTest.IsEnabled = true;
            btnAutodetect.IsEnabled = true;
            //btnAutodetect.IsEnabled = true;
            //cbDbms.IsEnabled = true;
            //cbPayloads.IsEnabled = true;
            //cbExploits.IsEnabled = true;
        }

        private void DisableAll()
        {
            //btnGetUrls.IsEnabled = true;

            //TODO: add treeview here
            btnAutodetect.IsEnabled = false;
            btnExecuteCustomQuery.IsEnabled = false;
            btnTest.IsEnabled = false;
            //btnAutodetect.IsEnabled = false;
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
            sb.Append(injectionStrategy != null ? injectionStrategy.GetType().Name : string.Empty);
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
            if (!string.IsNullOrEmpty(txtUrl.Text) && UrlHelpers.ValidUrl(txtUrl.Text))
            {
                if (_currentInjectionStrategy != null)
                {
                    _currentInjectionStrategy.Url = txtUrl.Text;

                    PopulateExploits(cbDbms.SelectedValue != null ? cbDbms.SelectedValue.ToString() : string.Empty,
                                    _currentInjectionStrategy);

                    ProxifyInjectionStrategy();
                }

                btnAutodetect.IsEnabled = true;
            }
            else
                btnAutodetect.IsEnabled = false;
        }

        private void ParameterChange()
        {
            if (!string.IsNullOrEmpty(txtUrl.Text) && UrlHelpers.ValidUrl(txtUrl.Text) && _currentInjectionStrategy != null &&
                cbDbms.SelectedValue != null && _currentInjectionStrategy.ExploitDetails != null)
            {
                EnableAll();
                ClearAll();
            }
            else
                DisableAll();
        }
        #endregion Methods

        #endregion Private

        #region Public
        public IInjectionStrategy CurrentInjectionStrategy
        {
            get
            {
                return _currentInjectionStrategy;
            }
        }
        #endregion Public

        private void btnAutodetect_Click(object sender, RoutedEventArgs e)
        {
            bool findAllPossibleAttackVectors = true;//@TODO: this should be a setting and come from somewhere, maybe the UI
            bool vuln = false;
            string msg = string.Empty;
            IList<string> dbMgmtSystems = new List<string>();
            IList<ExploitDetails> exploits = new List<ExploitDetails>();
            List<dynamic> filters = null;
            dynamic filter = null;
            ProxyType proxyType = ProxyType.None;
            string url = txtUrl.Text;
            string fullProxyAdress = txtProxyFullAddress.Text;
            bool useProxy = (chkUseProxy.IsChecked != null) ? chkUseProxy.IsChecked.Value : false; ;

            ClearAll();
            DisableAll();

            if (cmbProxyType.SelectedValue != null)
                Enum.TryParse<ProxyType>(cmbProxyType.SelectedValue.ToString(), out proxyType);

            var th = new Thread(() =>
            {

                dbMgmtSystems = XmlHelpers.GetAllAttributeValuesFromDoc(FileHelpers.GetCurrentDirectory() + "\\xml\\exploits.xml",
                                                                                "exploit", "dbms");

                foreach (var injectionStrategy in _injectionStrategies)
                {
                    if (_stopCurrentActionSingleUrlTab)
                        break;

                    foreach (var dbMgmtSystem in dbMgmtSystems)
                    {
                        if (_stopCurrentActionSingleUrlTab)
                            break;

                        filters = new List<dynamic>();
                        filter = new ExpandoObject();
                        filter.AttributeName = "dbms"; filter.AttributeValue = dbMgmtSystem;
                        filters.Add(filter);
                        filter = new ExpandoObject();
                        filter.AttributeName = "injection-strategy"; filter.AttributeValue = injectionStrategy.GetType().Name;
                        filters.Add(filter);
                        exploits = XmlHelpers.GetObjectsFromXml<ExploitDetails>(FileHelpers.GetCurrentDirectory() + "\\xml\\exploits.xml", "exploit", 
                                                                                filters);

                        foreach (var exploit in exploits)
                        {
                            if (_stopCurrentActionSingleUrlTab)
                                break;
                            
                            //populate
                            injectionStrategy.ExploitDetails = exploit; injectionStrategy.Url = url;
                            if (useProxy)
                            {
                                injectionStrategy.UseProxy = true;
                                injectionStrategy.ProxyDetails = new ProxyDetails()
                                {
                                    FullProxyAddress = fullProxyAdress,
                                    ProxyType = proxyType
                                };
                            }

                            //test
                            //var superGigi = UrlHelpers.HexEncodeValue("gigi");

                            try
                            {
                                vuln = injectionStrategy.TestIfVulnerable();
                            }
                            catch (Exception ex)
                            {
                                //TODO: log this maybe?
                            }

                            //depopulate
                            injectionStrategy.ExploitDetails = null; injectionStrategy.Url = null;injectionStrategy.ProxyDetails = null;
                            injectionStrategy.UseProxy = false;

                            if (vuln)
                            {
                                msg += string.Format("Vulnerable using  the injection strategy: {0} with the exploit: {1}. Detected DBMS: {2}", 
                                                        injectionStrategy.DisplayName, exploit.UserFriendlyName, dbMgmtSystem)
                                        + Environment.NewLine;
                                if (!findAllPossibleAttackVectors)
                                    _stopCurrentActionSingleUrlTab = true;
                                else
                                    vuln = false;
                            }
                        }
                    }
                }

                if (string.IsNullOrEmpty(msg))
                    msg = "Not vulnerable given any available expoit";

                txtCustomQueryResult.Dispatcher.Invoke(
                                    System.Windows.Threading.DispatcherPriority.Normal,
                                    new Action(
                                    delegate()
                                    {
                                        txtCustomQueryResult.Text = msg;
                                    }
                                ));
                _stopCurrentActionSingleUrlTab = false;
                EnableAllFromOtherThread();
            });
            try
            {
                th.Start();
            }
            catch (Exception ex)
            {
                txtCustomQueryResult.Text = string.Format("Error: {0}",ex.Message);
            }
        }


        private void btnTest_Click(object sender, RoutedEventArgs e)
        {
            ClearAll();
            DisableAll();


            var th = new Thread(() =>
            {
                bool urlVuln = false;
                string msg = string.Format(GeneralMessages.NotVulnerable, _currentInjectionStrategy.DisplayName);

                try
                {
                    urlVuln = _currentInjectionStrategy.TestIfVulnerable();
                }
                catch (Exception ex)
                {
                    msg += " (Exception: "+ex.Message+")";
                }
                    
                    
                if (urlVuln)
                    msg = string.Format(GeneralMessages.Vulnerable, _currentInjectionStrategy.DisplayName);
                
                txtCustomQueryResult.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new Action(
                                delegate()
                                {
                                    txtCustomQueryResult.Text = msg;
                                }
                            ));

                _stopCurrentActionSingleUrlTab = false;
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void cbCurrentInjectionStrategy_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            _currentInjectionStrategy = (IInjectionStrategy)_injectionStrategies[cbCurrentInjectionStrategy.SelectedIndex];
            UrlOrStrategyChange();
            //ParameterChange();
        }

        private void txtUrl_GotFocus(object sender, RoutedEventArgs e)
        {
            DisableAll();
        }

        private void txtUrl_LostFocus(object sender, RoutedEventArgs e)
        {
            UrlOrStrategyChange();
            //ParameterChange();
        }

        private void btnStopCurAction_Click(object sender, RoutedEventArgs e)
        {
            _stopCurrentActionSingleUrlTab = true;
        }

        private void txtCustomQuery_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentInjectionStrategy != null && !string.IsNullOrEmpty(txtCustomQuery.Text))
            {
                CurrentInjectionStrategy.PayloadDetails = new PayloadDetails();
                CurrentInjectionStrategy.PayloadDetails.Payload = txtCustomQuery.Text.Trim();
                CurrentInjectionStrategy.PayloadDetails.Dbms = cbDbms.SelectedValue.ToString();
                //can't tell if a new user provided query is expected to yield one or more results so we assume it yields multiple
                //results
                CurrentInjectionStrategy.PayloadDetails.ExpectedResultType = ExpectedResultType.Multiple;
                cbPayloads.SelectedItem = null;
            }
        }

        private void btnExecuteCustomQuery_Click(object sender, RoutedEventArgs e)
        {
            txtCustomQueryResult.Clear();
            DisableAll();

            var th = new Thread(() =>
            {
                string result = string.Empty;

                int total = 0;

                try
                {
                    total = _currentInjectionStrategy.GetTotalNoOfCustomQueryResultRows();
                }
                catch (Exception ex)
                {
                    txtCustomQueryResult.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new Action(
                                delegate()
                                {
                                    txtCustomQueryResult.Text = ex.Message;
                                }
                            ));
                }

                if (_currentInjectionStrategy.NumberOfResultsPerRequest > 0)
                    for (int i = 0; i < total; i = i + _currentInjectionStrategy.NumberOfResultsPerRequest)
                    {
                        if (_stopCurrentActionSingleUrlTab)
                            break;
                        try
                        {
                            result = _currentInjectionStrategy.GetSingleCustomQueryResultRow(i);
                        }
                        catch (Exception ex)
                        {
                            result = ex.Message;
                        }
                        if (!string.IsNullOrEmpty(result))
                        {
                            #region map to ui

                            List<string> valuesToInsert = new List<string>();
                            if (result.Contains(Environment.NewLine))
                                valuesToInsert.AddRange(result.Split(new string[] { Environment.NewLine }, StringSplitOptions.None));
                            else
                                valuesToInsert.Add(result);

                            if (_currentInjectionStrategy.PayloadDetails != null &&
                                !string.IsNullOrEmpty(_currentInjectionStrategy.PayloadDetails.NodeToMapTo))
                            {
                                var xpath = XmlHelpers.CreateProperMapToNodeFinderXpath(_currentInjectionStrategy.PayloadDetails, _currentInjectionStrategy);
                                //var xpath = XmlHelpers.CreateProperMapToNodeCreatorXpath(_currentInjectionStrategy.PayloadDetails,
                                //    result);
                                var tagName = XmlHelpers.GetLastTagFromXpath(xpath);

                                XmlTreeViewItem newChildItem = null;
                                XmlTreeViewItem oldParentItem = null;

                                if (tagName == "databases")//@TODO: no more hardconding
                                    oldParentItem = UIHelpers.GetTreeViewRoot(tvDs);
                                else if (tagName == "db" || tagName == "table")//@TODO: no more hardconding
                                    oldParentItem = _selectedTreeViewItem;

                                if (oldParentItem != null)
                                    foreach (var value in valuesToInsert)
                                    {
                                        if (!string.IsNullOrEmpty(value))
                                        {
                                            tvDs.Dispatcher.Invoke(
                                                    System.Windows.Threading.DispatcherPriority.Normal,
                                                    new Action(
                                                    delegate()
                                                    {
                                                        newChildItem = UIHelpers.GetXmlTreeViewItemRec(oldParentItem,
                                                                                                        _currentInjectionStrategy.PayloadDetails.NodeToMapTo,
                                                                                                        value);
                                                    }
                                                ));
                                            if (newChildItem == null)
                                            {
                                                tvDs.Dispatcher.Invoke(
                                                    System.Windows.Threading.DispatcherPriority.Normal,
                                                    new Action(
                                                    delegate()
                                                    {
                                                        UIHelpers.XmlTreeViewAdd(oldParentItem, _currentInjectionStrategy.PayloadDetails.NodeToMapTo, value);
                                                    }
                                                ));
                                            }
                                        }
                                    }
                            }
                            #endregion map to ui

                            txtCustomQueryResult.Dispatcher.Invoke(
                                    System.Windows.Threading.DispatcherPriority.Normal,
                                    new Action(
                                    delegate()
                                    {
                                        txtCustomQueryResult.Text += result + Environment.NewLine;
                                    }
                                ));
                        }
                    }

                _stopCurrentActionSingleUrlTab = false;
                EnableAllFromOtherThread();
            });
            th.Start();
        }

        private void cmbProxyType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ProxifyObtainerStrategy();
            ProxifyInjectionStrategy();
        }

        private void cbExploits_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExploitDetails ed = null;

            if (cbExploits.SelectedItem != null)
            {
                ed = XmlHelpers.GetObjectFromXml<ExploitDetails>(FileHelpers.GetCurrentDirectory() + "\\xml\\exploits.xml",
                                                                "exploit",
                                                                cbExploits.SelectedItem.ToString());
                if (_currentInjectionStrategy != null && ed != null)
                {
                    _currentInjectionStrategy.ExploitDetails = ed;
                    ParameterChange();
                }
            }
        }

        private void cbPayloads_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            PayloadDetails pd = null;

            if (cbPayloads.SelectedItem != null)
            {
                pd = XmlHelpers.GetObjectFromXml<PayloadDetails>(FileHelpers.GetCurrentDirectory() + "\\xml\\payloads.xml",
                                                                "payload",
                                                                cbPayloads.SelectedItem.ToString());
                if (_currentInjectionStrategy != null && pd != null)
                {
                    _currentInjectionStrategy.PayloadDetails = pd;
                    txtCustomQuery.Text = pd.Payload;
                }
            }
        }

        private void cbDbms_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string dbms = string.Empty;
            dbms = cbDbms.SelectedValue != null ? cbDbms.SelectedValue.ToString() : string.Empty;
            PopulatePayloads(dbms);
            if (_currentInjectionStrategy != null)
                PopulateExploits(dbms, _currentInjectionStrategy);
        }

        private void chkMapResultsToFile_Checked(object sender, RoutedEventArgs e)
        {
            string mappingFile = txtMappingFile.Text.Trim();
            if (string.IsNullOrEmpty(mappingFile) || _currentInjectionStrategy == null)
            {
                chkMapResultsToFile.IsChecked = false;
                return;
            }

            if (chkMapResultsToFile.IsChecked.Value)
            {
                string error = string.Empty;
                XDocument doc = null;
                if (XmlHelpers.CreateOrLoadMappingFile(mappingFile, _currentInjectionStrategy,
                                                        cbDbms.SelectedValue != null ? cbDbms.SelectedValue.ToString() : string.Empty,
                                                        ref error,out doc))
                {
                    _currentInjectionStrategy.MappingFile = mappingFile;
                }
                else
                {
                    MessageBox.Show(error);
                }
            }
            else
                _currentInjectionStrategy.MappingFile = null;
        }

        private void btnChooseMappingFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            // Set filter for file extension and default file extension
            dlg.DefaultExt = ".xml";
            dlg.Filter = "Xml file (.xml)|*.xml|All Files|*.*";
            dlg.InitialDirectory = FileHelpers.GetCurrentDirectory() + "\\xml\\maps";
            // Display OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = dlg.ShowDialog();
            // Get the selected file name and display in a TextBox
            if (result == true)
            {
                // Open document
                string filename = dlg.FileName;
                txtMappingFile.Text = filename;
            }
        }

        private void btnOverrideCurrentSettings_Click(object sender, RoutedEventArgs e)
        {
            string mappingFile = txtMappingFile.Text.Trim();

            if (!string.IsNullOrEmpty(mappingFile))
            {
                if (!File.Exists(mappingFile))
                {
                    MessageBox.Show("Could not load file");
                    return;
                }

                string injectionStrategyTypeName = XmlHelpers.GetAttributeValueFromDoc<string>(mappingFile, "/map/injection-strategy", "name",
                                                                                                string.Empty);

                int injectionStrategyNrOriginalQueryCols = XmlHelpers.GetElementValueFromDoc<int>(mappingFile,
                                                                "/map/injection-strategy/columns/originalquery", 0);

                int injectionStrategyNrHtmlCols = XmlHelpers.GetElementValueFromDoc<int>(mappingFile,
                                                                "/map/injection-strategy/columns/resultinghtml", 0);

                string injectionStrategyColumnIndexes = XmlHelpers.GetElementValueFromDoc<string>(mappingFile,
                                                                "/map/injection-strategy/columns/indexes", string.Empty);


                string vulnerableUrl = XmlHelpers.GetElementValueFromDoc<string>(mappingFile, "/map/vulnerable-url", string.Empty);

                string dbms = XmlHelpers.GetAttributeValueFromDoc<string>(mappingFile, "/map/dbms", "name",
                                                                                                string.Empty);

                IInjectionStrategy strategy = _injectionStrategies.Where(i => i.GetType().Name == injectionStrategyTypeName).FirstOrDefault();
                if (strategy != null)
                {
                    cbCurrentInjectionStrategy.SelectedValue = strategy.DisplayName;
                }
                if (_currentInjectionStrategy != null)
                {
                    if (!string.IsNullOrEmpty(vulnerableUrl))
                    {
                        txtUrl.Text = vulnerableUrl;
                        UrlOrStrategyChange();
                        //ParameterChange();
                    }
                    _currentInjectionStrategy.NrColumnsInOriginalQuery = injectionStrategyNrOriginalQueryCols;
                    _currentInjectionStrategy.NumberOfResultsPerRequest = injectionStrategyNrHtmlCols;
                    _currentInjectionStrategy.ColumnIndexes = ListHelpers.CommaSeparatedValuesToList<int>(injectionStrategyColumnIndexes);
                }

                if (!string.IsNullOrEmpty(dbms))
                    cbDbms.SelectedValue = dbms;

                var databasesElem = XmlHelpers.GetXmlElementViaXpath(mappingFile, "/map/databases");
                if (databasesElem != null)
                {
                    var newRootElement = UIHelpers.ClearTreeView(tvDs);

                    UIHelpers.BuildNodes(newRootElement, databasesElem);

                    #region different approach
                    //    XmlDataProvider dataProvider = this.FindResource("xmlDataProvider") as XmlDataProvider;
                    //    var bindDoc = new XmlDocument();
                    //    var reader = databasesElem.CreateReader();
                    //    reader.MoveToContent();
                    //    bindDoc.LoadXml(reader.ReadOuterXml());
                    //    dataProvider.Document = bindDoc;
                    #endregion different approach
                }
            }

        }

        private void tvDs_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            _selectedTreeViewItem = ((XmlTreeViewItem)((TreeView)sender).SelectedItem);
            ContextMenu contextMenu = new ContextMenu();

            if (_selectedTreeViewItem == null)
                return;

            if (_selectedTreeViewItem.TagName == "table")
            {
                _currentInjectionStrategy.SelectedTable = _selectedTreeViewItem.Header.ToString();
                _currentInjectionStrategy.SelectedDb = ((XmlTreeViewItem)_selectedTreeViewItem.Parent).Header.ToString();

            }
            else if (_selectedTreeViewItem.TagName == "db")
            {
                _currentInjectionStrategy.SelectedTable = string.Empty;
                _currentInjectionStrategy.SelectedDb = _selectedTreeViewItem.Header.ToString();
            }


            MenuItem menuItem = new MenuItem { Header = "Insert" };
            menuItem.Click += OptionClick;
            contextMenu.Items.Add(new MenuItem().Header = "Copy");
            _selectedTreeViewItem.ContextMenu = contextMenu;
        }

        void OptionClick(object sender, RoutedEventArgs e)
        {
            TreeViewItem newChild = new TreeViewItem();
            TreeViewItem selected = new TreeViewItem();
            // Unboxing
            MenuItem menuItem = sender as MenuItem;
            newChild.Header = menuItem.Header;

            selected = (TreeViewItem)tvDs.SelectedItem;
            selected.Items.Add(newChild);
        }
    }
}
