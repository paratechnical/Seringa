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

            string url = txtSearchEngineUrl.Text.Trim();

            if (string.IsNullOrEmpty(url))
                return;

            var th = new Thread(() =>
            {
                string error = string.Empty;
                string output = string.Empty;

                var results = HtmlHelpers.GoogleSearch(url, ref error);

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
