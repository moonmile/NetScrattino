using NetScrattinoWin.Models;
using NetScrattinoWin.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetScrattinoWin
{
    /// <summary>
    /// MainWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Loaded += MainWindow_Loaded;
        }


        PinModel pinModel;
        MainViewModel vm;
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            pinModel = new PinModel();
            vm = new MainViewModel(pinModel);
            this.DataContext = vm;

            vm.SerialItems = System.IO.Ports.SerialPort.GetPortNames().ToList();

            if ( App.CommandLineArgs != null )
            {
                var port = App.CommandLineArgs[0];
                var index = vm.SerialItems.FindIndex( x => x == port);
                if ( index >= 0 )
                {
                    vm.SerialSelectedIndex = index;
                    ConnectClicked(null, null);
                }
            }
        }

        /// <summary>
        /// シリアル通信を更新
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void UpdateSerialClicked(object sender, RoutedEventArgs e)
        {
            vm.SerialItems = System.IO.Ports.SerialPort.GetPortNames().ToList();
        }

        System.Timers.Timer timer;
        NetScrattino.NetScrattinoSever server;

        /// <summary>
        /// Arduino Firmataに接続
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectClicked(object sender, RoutedEventArgs e)
        {
            var name = vm.SerialName;
            var arduino = new FirmataNET.Arduino();
            arduino.Connect(name);
            arduino.Open();
            pinModel.Arduino = arduino;

            timer = new System.Timers.Timer();
            timer.Interval = 200;
            timer.Elapsed += (_,__) => {
                vm.Update();
            };
            timer.Start();

            server = new NetScrattino.NetScrattinoSever();
            server.StartAsync(5410, arduino);

            btnConnect.Background = new SolidColorBrush(Colors.Green);
            btnConnect.Foreground = new SolidColorBrush(Colors.White);

        }
    }
}
