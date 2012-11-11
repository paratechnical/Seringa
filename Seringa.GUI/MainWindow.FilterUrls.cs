using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Threading;
using Seringa.Engine.Implementations.QueryRunners;
using Seringa.Engine.Utils;
using Seringa.Engine.DataObjects;

namespace Seringa.GUI
{
    public partial class MainWindow
    {
        private bool _stopCurActionFilterUrlsTab = false;

        private void btnCancelFilterUrls_Click(object sender, RoutedEventArgs e)
        {
            _stopCurActionFilterUrlsTab = true;
        }


        private void btnCheckUrls_Click(object sender, RoutedEventArgs e)
        {
            IList<string> vulnerableResults = new List<string>();
            IList<string> urlsToCheck = new List<string>();
            string[] separators = new string[] { Environment.NewLine };
            IList<PatternDetails> patterns = new List<PatternDetails>();
            string urlBatch = txtUrls.Text;

            btnCheckUrls.IsEnabled = false;
            txtProbablyVulnerableUrls.Clear();
            bool possiblyVulnerable = false;
            

            var th = new Thread(() =>
            {
                var queryRunner = new SimpleQueryRunner();

                if (!string.IsNullOrEmpty(urlBatch))
                    urlsToCheck = urlBatch.Split(separators, StringSplitOptions.RemoveEmptyEntries).ToList();

                foreach (var url in urlsToCheck)
                {
                    if (_stopCurActionFilterUrlsTab == true)
                        break;

                    possiblyVulnerable = false;

                    IList<string> possiblyVulnerableUrls = Seringa.Engine.Utils.UrlHelpers.GeneratePossibleVulnerableUrls(url);//TODO:multiple possible vulnerable urls

                    foreach(var possiblyVulnerableUrl in possiblyVulnerableUrls)
                    {
                        string pageHtml = string.Empty;

                        try
                        {
                            pageHtml = queryRunner.GetPageHtml(possiblyVulnerableUrl, null);//@TODO:proxify
                        }
                        catch (Exception ex)
                        {
                            //@TODO: Log Exception
                        }

                        patterns = XmlHelpers.GetObjectsFromXml<PatternDetails>(FileHelpers.GetCurrentDirectory() + "\\xml\\patterns.xml", "pattern", null);

                        foreach (var pattern in patterns)
                        {
                            if(pattern != null && !string.IsNullOrEmpty(pattern.Value))
                            if(pageHtml.IndexOf(pattern.Value) > -1)
                            {
                                possiblyVulnerable = true; 
                                break;
                            }
                        }

                        if (possiblyVulnerable)
                        {
                            gridFilterUrls.Dispatcher.Invoke(
                                System.Windows.Threading.DispatcherPriority.Normal,
                                new Action(
                                    delegate()
                                    {
                                        txtProbablyVulnerableUrls.Text += possiblyVulnerableUrl + Environment.NewLine;
                                    }));
                        }
                    }
                }

                _stopCurActionFilterUrlsTab = false;

                gridFilterUrls.Dispatcher.Invoke(
                    System.Windows.Threading.DispatcherPriority.Normal,
                    new Action(
                    delegate()
                    {
                        btnCheckUrls.IsEnabled = true;
                    }
                ));
            });
            th.Start();
        }
     

    }
}
