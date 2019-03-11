using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Windows.ApplicationModel;
using Windows.ApplicationModel.AppService;
using Windows.Foundation.Collections;

namespace AppServiceTest.Systray
{
    class SystrayApplicationContext : ApplicationContext
    {
        private readonly AppServiceConnection _appServiceConnection;
        private bool _isAppServiceConnectionOpen;

        public SystrayApplicationContext()
        {

            var sendMessageMenuItem = new MenuItem("Send Message", SendMessageToApp);
            var launchMenuItem = new MenuItem("Launch", LaunchApp);
            var exitMenuItem = new MenuItem("Exit", ExitApp);
            launchMenuItem.DefaultItem = true;

            var notifyIcon = new NotifyIcon();
            notifyIcon.DoubleClick += LaunchApp;
            notifyIcon.Icon = Properties.Resources.IconMaPhone;
            notifyIcon.ContextMenu = new ContextMenu(new[]
            {
                sendMessageMenuItem, launchMenuItem, exitMenuItem
            });
            notifyIcon.Visible = true;

            _appServiceConnection = CreateAppServiceConnection();
        }
        public async void Initialise()
        {
            await StartAppServiceConnection();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _appServiceConnection?.Dispose();
            }
            base.Dispose(disposing);
        }

        private AppServiceConnection CreateAppServiceConnection()
        {
            var appServiceConnection = new AppServiceConnection
            {
                PackageFamilyName = Package.Current.Id.FamilyName,
                AppServiceName = "MyAppServiceName",
            };
            appServiceConnection.ServiceClosed += OnAppServiceConnectionServiceClosed;
            appServiceConnection.RequestReceived += OnAppServiceRequestReceived;
            return appServiceConnection;
        }

        private void OnAppServiceRequestReceived(AppServiceConnection sender, AppServiceRequestReceivedEventArgs args)
        {
            if (args.Request.Message.ContainsKey("forsystray"))
            {
                var message = args.Request.Message["forsystray"].ToString();
                MessageBox.Show(message);
            }
        }

        private void OnAppServiceConnectionServiceClosed(AppServiceConnection sender, AppServiceClosedEventArgs args)
        {
            _isAppServiceConnectionOpen = false;
            _appServiceConnection.RequestReceived -= OnAppServiceRequestReceived;
            _appServiceConnection.ServiceClosed -= OnAppServiceConnectionServiceClosed;
        }

        private async Task<bool> StartAppServiceConnection()
        {
            if (_isAppServiceConnectionOpen) return true;
            var connectionStatus = await _appServiceConnection.OpenAsync();
            _isAppServiceConnectionOpen = connectionStatus == AppServiceConnectionStatus.Success;
            if (!_isAppServiceConnectionOpen)
            {
                // TODO handle error
            }
            return _isAppServiceConnectionOpen;
        }

        private async void ExitApp(object sender, EventArgs e)
        {
            await DoSendMessageToApp("exit", null);
            Application.Exit();
        }

        private async void LaunchApp(object sender, EventArgs e)
        {
            var uri = new Uri("myapp://dosomething");
            await Windows.System.Launcher.LaunchUriAsync(uri);
        }

        private async void SendMessageToApp(object sender, EventArgs e)
        {
            await DoSendMessageToApp("foruwp", "Hello UWP!");
        }

        private async Task DoSendMessageToApp(string key, object value)
        {
            if (await StartAppServiceConnection())
            {
                var parameters = new ValueSet
                {
                    {key, value}
                };
                await _appServiceConnection.SendMessageAsync(parameters);
            }
        }
    }
}
