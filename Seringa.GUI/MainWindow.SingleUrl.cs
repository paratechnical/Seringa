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

namespace Seringa.GUI
{
    public partial class MainWindow
    {
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
                    txtCustomQueryResult.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new Action(
                                delegate()
                                {
                                    msg += " (Exception: "+ex.Message+")";
                                }
                            ));
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

                _stopCurrentAction = false;
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
            _stopCurrentAction = true;
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

                for (int i = 0; i < total; i = i + _currentInjectionStrategy.NumberOfResultsPerRequest)
                {
                    if (_stopCurrentAction)
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

                _stopCurrentAction = false;
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
            ed = XmlHelpers.GetObjectFromXml<ExploitDetails>(FileHelpers.GetCurrentDirectory() + "\\xml\\exploits.xml",
                                                            "exploit",
                                                            cbExploits.SelectedValue != null ? cbExploits.SelectedValue.ToString() : string.Empty);
            if (_currentInjectionStrategy != null && ed != null)
            {
                _currentInjectionStrategy.ExploitDetails = ed;
                ParameterChange();
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
