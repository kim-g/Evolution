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
        System.Windows.Threading.DispatcherTimer MoveTimer, UpdateTimer;
        int Steps = 0;

        public MainWindow()
        {
            InitializeComponent();

            MainBiome.Initialize(800, 400);

            for (int i = 0; i < 80; i++)
                MainBiome.Add(new Species(MainBiome.RandomSeeds.Next(int.MaxValue))
                {
                    Energy = 10,
                    MaxSpeed = 2,
                    Position = new IntPoint() { X = i, Y = 0}
                });

            CountLine.Source = MainBiome.CountList;

            MoveTimer = new System.Windows.Threading.DispatcherTimer();
            MoveTimer.Tick += new EventHandler(MoveTimerTick);
            MoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            MoveTimer.Start();

            UpdateTimer = new System.Windows.Threading.DispatcherTimer();
            UpdateTimer.Tick += new EventHandler(UpdateTimerTick);
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            UpdateTimer.Start();

        }

        private void MoveTimerTick(object sender, EventArgs e)
        {
            DateTime StartTime = DateTime.Now;

            MainBiome.Step();
            MainBiome.Show();

            Status.Content = "Время расчёта шага " + 
                DateTime.Now.Subtract(StartTime).TotalMilliseconds.ToString() + " мс.";

            StepN.Content = $"Шаг № {++Steps}.";
            Count.Content = $"Всего существ: {MainBiome.ElementsCount}";

            CountLine.Update();
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            MainBiome.ShowAll();
        }
    }
}
