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
    /// Логика взаимодействия для InfoFrame.xaml
    /// </summary>
    public partial class InfoFrame : UserControl
    {
        public InfoFrame()
        {
            InitializeComponent();

            DNA_Colors.Add(0x00, new SolidColorBrush(Colors.White));    // Пустой ген
            DNA_Colors.Add(0x01, new SolidColorBrush(Colors.Orange));   // Ген размножения
            DNA_Colors.Add(0x10, new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0x00))); // Ген фотосинтеза
            DNA_Colors.Add(0x11, new SolidColorBrush(Colors.Blue));     // Ген хищничества
            DNA_Colors.Add(0x20, new SolidColorBrush(Color.FromRgb(0x00, 0xFF, 0xFF)));   // Ген движения
            DNA_Colors.Add(0x21,
                new LinearGradientBrush(
                    Colors.White,
                    Color.FromRgb(0x00, 0xFF, 0xFF),
                    90));                                               // Ген изменения скорости вправо
            DNA_Colors.Add(0x22,
                new LinearGradientBrush(
                    Colors.White,
                    Color.FromRgb(0x00, 0xFF, 0xFF),
                    270));                                              // Ген изменения скорости вниз
            DNA_Colors.Add(0x23,
                new LinearGradientBrush(
                    Colors.White,
                    Color.FromRgb(0x00, 0xFF, 0xFF),
                    0));                                              // Ген изменения скорости влево
            DNA_Colors.Add(0x24,
                new LinearGradientBrush(
                    Colors.White,
                    Color.FromRgb(0x00, 0xFF, 0xFF),
                    180));                                                // Ген изменения скорости вверх
            DNA_Colors.Add(0x30, new SolidColorBrush(Colors.Violet));   // Ген чувства еды
            DNA_Colors.Add(0x31, new SolidColorBrush(Colors.Yellow));   // Ген чувства опасности

            DNA_Lables.Add(0x00, "Пустой");    // Пустой ген
            DNA_Lables.Add(0x01, "Размножение");   // Ген размножения
            DNA_Lables.Add(0x10, "Фотосинтез"); // Ген фотосинтеза
            DNA_Lables.Add(0x11, "Поедание");     // Ген хищничества
            DNA_Lables.Add(0x20, "Двигаться");   // Ген движения
            DNA_Lables.Add(0x21, "Увеличить скорость влево");                                               // Ген изменения скорости вправо
            DNA_Lables.Add(0x22, "Увеличить скорость вправо");                                              // Ген изменения скорости вниз
            DNA_Lables.Add(0x23, "Увеличить скорость вверх");                                              // Ген изменения скорости влево
            DNA_Lables.Add(0x24, "Увеличить скорость вниз");                                                // Ген изменения скорости вверх
            DNA_Lables.Add(0x30, "Поиск еды");   // Ген чувства еды
            DNA_Lables.Add(0x31, "Поиск опасности");   // Ген чувства опасности


            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    DNA[i, j] = new Rectangle()
                    {
                        Stroke = BlackStroke,
                        Fill = DNA_Colors[0x00],
                        ToolTip = DNA_Lables[0x00], 
                        Visibility = Visibility.Visible,
                        Width = double.NaN,
                        Height = double.NaN
                    };

                    Grid.SetColumn(DNA[i, j], i);
                    Grid.SetRow(DNA[i, j], j);
                    DNA_Grid.Children.Add(DNA[i, j]);
                }
        }

        #region Внутренние свойства
        protected Species source;

        /// <summary>
        /// Массив для отображения ДНК
        /// </summary>
        protected Rectangle[,] DNA = new Rectangle[10, 10];

        protected readonly string[] NutritionType = new string[] { "фотосинтез", "смешанный", "хищничество" };

        #region Цвета
        protected Brush BlackStroke = new SolidColorBrush(Colors.Black);
        protected Dictionary<byte, Brush> DNA_Colors = new Dictionary<byte, Brush>();
        protected Dictionary<byte, string> DNA_Lables = new Dictionary<byte, string>();
        #endregion
        #endregion

        #region Внешние свойства
        public Species Source
        {
            get => source;
            set
            {
                source = value;
                if (source == null)
                {
                    Visibility = Visibility.Collapsed;
                    return;
                }

                UpdateSource();
                Visibility = Visibility.Visible;
            }
        }
        #endregion

        #region Внутренние методы
        protected void UpdateSource()
        {
            ID_Label.Content = $"ID: {Source.ID}";
            Start_TB.Text = $"Появился в цикле: {Source.Start}";
            Life_TB.Text = $"Время жизни: {Source.Life}";
            Energy_TB.Text = $"Энергия: {Source.Energy}";
            MaxSpeed_TB.Text = $"Макс. скорость: {Source.MaxSpeed}";
            Speed_TB.Text = $"Скорость: {Source.LeftSpeed} x {Source.TopSpeed}";
            Sensitivity_TB.Text = $"Чувствительность: {Source.Sensitivity}";
            Nutrition_TB.Text = $"Питание: {NutritionType[Source.Nutrition]}";

            for (int i = 0; i < 10; i++)
                for (int j = 0; j < 10; j++)
                {
                    DNA[i, j].Fill = DNA_Colors[Source.DNA[i * 10 + j]];
                    DNA[i, j].ToolTip = DNA_Lables[Source.DNA[i * 10 + j]];
                }
        }
        #endregion

        #region Внешние методы

        #endregion

        private void Grid_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }
    }
}
