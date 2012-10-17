using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Seringa.Engine.Enums;
using Seringa.Engine.Implementations.Proxy;
using System.Windows;
using System.Threading;
using Seringa.Engine.Interfaces;

namespace Seringa.GUI
{
    public partial class MainWindow
    {
        #region Private
        #region Fields
        private IIPObtainerStrategy _currentIpObtainerStrategy = null;
        #endregion Fields
        #endregion Private

        private void btnCurIP_Click(object sender, RoutedEventArgs e)
        {
            {
                DisableAll();
                var th = new Thread(() =>
                {
                    string error = string.Empty;
                    string ip = _currentIpObtainerStrategy.GetIp(ref error);
                    if (string.IsNullOrEmpty(error))
                        AddOutputToTextBox(txtCurIP, ip, false, false);
                    else
                        AddOutputToMsgBox(error);
                    EnableAllFromOtherThread();
                });
                th.Start();
            }
        }

        private void chkUseProxy_Checked(object sender, RoutedEventArgs e)
        {
            ProxifyObtainerStrategy();
            ProxifyInjectionStrategy();
        }

        private void txtProxyFullAddress_LostFocus(object sender, RoutedEventArgs e)
        {
            ProxifyObtainerStrategy();
            ProxifyInjectionStrategy();
        }

        private void ProxifyObtainerStrategy()
        {
            if (chkUseProxy.IsChecked.Value)
            {
                ProxyType proxyType = ProxyType.None;
                if (cmbProxyType.SelectedValue != null)
                    Enum.TryParse<ProxyType>(cmbProxyType.SelectedValue.ToString(), out proxyType);

                if (_currentIpObtainerStrategy != null)
                {
                    _currentIpObtainerStrategy.UseProxy = chkUseProxy.IsChecked.Value;
                    if (_currentIpObtainerStrategy.UseProxy)
                        _currentIpObtainerStrategy.ProxyDetails = new ProxyDetails()
                        {
                            FullProxyAddress = txtProxyFullAddress.Text,
                            ProxyType = proxyType
                        };
                }
            }
            else
                _currentIpObtainerStrategy.ProxyDetails = null;

        }

        private void ProxifyInjectionStrategy()
        {
            ProxyType proxyType = ProxyType.None;
            if (cmbProxyType.SelectedValue != null)
                Enum.TryParse<ProxyType>(cmbProxyType.SelectedValue.ToString(), out proxyType);

            if (_currentInjectionStrategy != null)
            {
                _currentInjectionStrategy.UseProxy = chkUseProxy.IsChecked.Value;
                if (_currentInjectionStrategy.UseProxy)
                    _currentInjectionStrategy.ProxyDetails = new ProxyDetails()
                    {
                        FullProxyAddress = txtProxyFullAddress.Text,
                        ProxyType = proxyType
                    };
            }
        }

    }
}
