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

        private bool running = false;

        protected bool Running
        {
            get => running;
            set
            {
                running = value;
                if (running)
                {
                    Start.Content = "Остановить";
                    MoveTimer.Start();
                    UpdateTimer.Start();
                    return;
                }

                Start.Content = "Продолжить";
                MoveTimer.Stop();
                UpdateTimer.Stop();
            }
        }

        public MainWindow()
        {
            InitializeComponent();

            MainBiome.Initialize(800, 400);

            for (int i = 0; i < 1; i++)
                MainBiome.Add(new Species(MainBiome.RandomSeeds.Next(int.MaxValue), 
                    Convert.ToByte(i==40 ? 2 : 0))
                {
                    Energy = 10,
                    MaxSpeed = 2,
                    Position = new IntPoint() { X = i, Y = 0}
                });

            CountLine.Line_1_Source = MainBiome.CountList;
            CountLine.Line_2_Source = MainBiome.ProducerList;
            CountLine.Line_3_Source = MainBiome.MixedList;
            CountLine.Line_4_Source = MainBiome.PredatorList;

            MoveTimer = new System.Windows.Threading.DispatcherTimer();
            MoveTimer.Tick += new EventHandler(MoveTimerTick);
            MoveTimer.Interval = new TimeSpan(0, 0, 0, 0, 20);

            UpdateTimer = new System.Windows.Threading.DispatcherTimer();
            UpdateTimer.Tick += new EventHandler(UpdateTimerTick);
            UpdateTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);
        }

        private void MoveTimerTick(object sender, EventArgs e)
        {
            DateTime StartTime = DateTime.Now;

            MainBiome.Step();
            MainBiome.Show();

            Status.Content = "Время расчёта шага " + 
                DateTime.Now.Subtract(StartTime).TotalMilliseconds.ToString() + " мс.";

            StepN.Content = $"Шаг № {MainBiome.Time}.";
            Count.Content = $"Всего существ: {MainBiome.ElementsCount}";

            double MaxEnergy = Math.Round(MainBiome.MaximumEnergy, 1);
            Energy.Content = $"Максимальная энергия: {MaxEnergy}";

            CountLine.Update();
        }

        private void MutatonTB_TextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                MainBiome.Mutagen = Convert.ToDouble(((TextBox)sender).Text);
            }
            catch
            {
                ((TextBox)sender).Text = MainBiome.Mutagen.ToString();
            }
        }

        private void ShowEnergy_Checked(object sender, RoutedEventArgs e)
        {
            MainBiome.MethodOfColorization = 0;
        }

        private void ShowNutration_Checked(object sender, RoutedEventArgs e)
        {
            MainBiome.MethodOfColorization = 1;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            Running = !Running;
        }

        private void CheckBox_Click(object sender, RoutedEventArgs e)
        {
            MainBiome.PredatorEatPredator = ((CheckBox)sender).IsChecked == true;
        }

        private void CheckBox_Click_1(object sender, RoutedEventArgs e)
        {
            MainBiome.PredatorHavePhotosynthesys = ((CheckBox)sender).IsChecked == true;
        }

        private void CheckBox_Click_2(object sender, RoutedEventArgs e)
        {
            MainBiome.ProducerEatOther = ((CheckBox)sender).IsChecked == true;
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            MainBiome.ShowAll();
        }
    }
}
