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

        private void btnGetUrls_Click(object sender, RoutedEventArgs e)
        {
            txtUrls.Clear();
            string url = txtSearchEngineUrl.Text.Trim();

            if (string.IsNullOrEmpty(url))
                return;

            //var queryRunner = new SimpleQueryRunner();
            //var pageHtml = queryRunner.GetPageHtml(url, null);
            //var 
            //var stringResults = HtmlHelpers.GetMultipleAnswersFromHtml(pageHtml, string.Empty, ExploitDetails, DetailedExceptions);
            var results = HtmlHelpers.GoogleSearch(url);

            foreach (var result in results)
                txtUrls.Text += result + Environment.NewLine;
        }


    }
}
