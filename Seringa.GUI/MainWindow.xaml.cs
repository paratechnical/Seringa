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
       
        #region General methods
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


        

        private void AddOutputToMsgBox( string text)
        {
            if (!gridMain.Dispatcher.CheckAccess())
            {

                gridMain.Dispatcher.Invoke(
                  System.Windows.Threading.DispatcherPriority.Normal,
                  new Action(
                    delegate()
                    {
                        MessageBox.Show(text);
                    }
                ));
            }
            else
            {
                MessageBox.Show(text);
            }
            
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
        #endregion General methods

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
