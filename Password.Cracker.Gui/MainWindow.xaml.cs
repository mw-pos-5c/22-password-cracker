#region usings

using System.Threading.Tasks;
using System.Windows;

using Microsoft.AspNetCore.SignalR.Client;

#endregion

namespace Password.Cracker.Gui
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        #region Constants and Fields

        readonly HubConnection connection;

        #endregion

        public MainWindow()
        {
            InitializeComponent();

            connection = new HubConnectionBuilder().WithUrl("http://localhost:5189/ws").Build();

            connection.Closed += async (error) =>
            {
                await Task.Delay(1000);
                await connection.StartAsync();
            };

            connection.On<string>("FoundPassword", (pw) => { Dispatcher.Invoke(PwFound, pw); });
            connection.On<double>("StatusUpdate", (value) => { Dispatcher.Invoke(StatusUpdate, value); });

            connection.StartAsync().ContinueWith((_) => Connected());
        }

        private bool IsConnected => connection.State == HubConnectionState.Connected;

        private void Connected()
        {
            if (!CheckAccess())
            {
                Dispatcher.Invoke(Connected);
                return;
            }

            statusLabel.Content = IsConnected
                ? "Connected"
                : "Unable to connect";
        }

        private void PwFound(string pw)
        {
            resultLabel.Content = pw;
        }

        private void StatusUpdate(double perCent)
        {
            progressBar.Value = perCent;
            resultLabel.Content = (perCent * 100) + " %";
        }

        #region Event Handlers

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (!IsConnected) return;

            string hash = hashLabel.Text;
            string alph = alpLabel.Text;

            if (!int.TryParse(lenLabel.Text, out int len))
            {
                len = 0;
            }

            connection.SendAsync("Crack", hash, alph, len);
        }

        #endregion
    }
}
