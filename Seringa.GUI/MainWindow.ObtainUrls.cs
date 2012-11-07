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
using Seringa.Engine.Implementations.QueryRunners;

namespace Seringa.GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool _stopCurActionObtainUrlsTab = false;

        private void btnStopCurActionObtainUrls_Click(object sender, RoutedEventArgs e)
        {
            _stopCurActionObtainUrlsTab = true;
        }

        private void btnGetUrls_Click(object sender, RoutedEventArgs e)
        {
            txtUrls.Clear();
            btnGetUrls.IsEnabled = false;

            int nrResults = 0;
            string url = txtSearchEngineUrl.Text.Trim();
            bool useProxy = (chkUseProxy.IsChecked != null) ? chkUseProxy.IsChecked.Value : false;
            ProxyType proxyType = ProxyType.None;
            string proxyFullAdress = string.Empty;
            if (cmbProxyType.SelectedValue != null)
                Enum.TryParse<ProxyType>(cmbProxyType.SelectedValue.ToString(), out proxyType);
            if(!string.IsNullOrEmpty(txtProxyFullAddress.Text))
                proxyFullAdress = txtProxyFullAddress.Text;
            if(!string.IsNullOrEmpty(txtNrResults.Text))
                int.TryParse(txtNrResults.Text, out nrResults);


            if (string.IsNullOrEmpty(url))
                return;

            var th = new Thread(() =>
            {
                string error = string.Empty;
                string output = string.Empty;
                ProxyDetails pd = null;
                IList<string> results = new List<string>();

                useProxy = false;//hardcode this for now so no errors are shown
                if (useProxy)
                    pd = new ProxyDetails()
                    {
                        FullProxyAddress = proxyFullAdress,
                        ProxyType = proxyType
                    };

                try
                {
                    results = HtmlHelpers.GoogleSearch(url, nrResults, pd, ref error);
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                }


                if (results.Count > 0)
                {
                    foreach (var result in results)
                    {
                        if (_stopCurActionObtainUrlsTab == true)
                            break;
                        gridObtainUrls.Dispatcher.Invoke(
                            System.Windows.Threading.DispatcherPriority.Normal,
                            new Action(
                                delegate()
                                {
                                    txtUrls.Text += result + Environment.NewLine;
                                }));
                    }
                }
                else
                {

                    if (!string.IsNullOrEmpty(error))
                        output = "Error: " + error;
                    else
                        output = "Google returned 0 results";
                    
                    gridObtainUrls.Dispatcher.Invoke(
                        System.Windows.Threading.DispatcherPriority.Normal,
                        new Action(
                            delegate()
                            {
                                txtUrls.Text += output + Environment.NewLine;
                            }));
                }

                _stopCurActionObtainUrlsTab = false;

                gridObtainUrls.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        btnGetUrls.IsEnabled = true;
                    }
                ));
            });
            th.Start();
        }
    }
}
