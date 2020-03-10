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

namespace Evolution
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        System.Windows.Threading.DispatcherTimer MoveTimer;

        public MainWindow()
        {
            InitializeComponent();

            MainBiome.Initialize(800, 400);

            MainBiome.Add(new Species()
            {
                Energy = 10,
                MaxSpeed = 2
            });


            MoveTimer = new System.Windows.Threading.DispatcherTimer();
            MoveTimer.Tick += new EventHandler(MoveTimerTick);
            MoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            MoveTimer.Start();

        }

        private void MoveTimerTick(object sender, EventArgs e)
        {
            DateTime StartTime = DateTime.Now;

            MainBiome.Step();
            MainBiome.Show();

            Status.Content = DateTime.Now.Subtract(StartTime).TotalMilliseconds.ToString();
        }
    }
}
