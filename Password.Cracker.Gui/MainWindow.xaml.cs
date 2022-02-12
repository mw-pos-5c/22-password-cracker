using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Microsoft.AspNetCore.SignalR.Client;

namespace Password.Cracker.Gui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        HubConnection connection;

        private bool IsConnected => connection.State == HubConnectionState.Connected;

        public MainWindow()
        {
            InitializeComponent();
            
            connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5189/ws")
                .Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                await connection.StartAsync();
            };
            
            connection.On<string>("FoundPassword",
                (pw) =>
                {
                    Dispatcher.Invoke(PwFound, pw);
                });
            connection.On<double>("StatusUpdate",
                (value) =>
                {
                    Dispatcher.Invoke(StatusUpdate, value);
                });
            
            
            connection.StartAsync().ContinueWith((_) => Connected());

        }

        private void StatusUpdate(double perCent)
        {
            progressBar.Value = perCent;
            resultLabel.Content = (perCent*100)+ " %";
        }

        private void PwFound(string pw)
        {
            resultLabel.Content = pw;
        }

        private void Connected()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(Connected);
                return;
            }
            
            statusLabel.Content = IsConnected ? "Connected" : "Unable to connect";
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            string hash = hashLabel.Text;
            string alph = alpLabel.Text;
            
            if (!int.TryParse(lenLabel.Text, out int len))
            {
                return;
            }

            connection.SendAsync("Crack", hash, alph, len);
        }
    }
}
