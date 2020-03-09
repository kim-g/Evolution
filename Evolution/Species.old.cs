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
    /// Логика взаимодействия для Species.xaml
    /// </summary>
    public partial class Species : UserControl, IDisposable
    {
        #region Константы
        protected const double EnergyPerStep = 0.1;
        protected const int DNA_Count = 100;
        protected const int OtherElementsToMutate = 1;
        #endregion

        #region private параметры
        double energy;
        bool alife = true;
        Brush RectColor;
        Random RND = new Random();
        int left_speed = 1;
        int top_speed = 0;
        byte[] DNA = new byte[100];

        /// <summary>
        /// Список генов для DNA
        /// </summary>
        byte[] Genes = new byte[]
        { 
            // Базовые гены
            0x00, // - Пустой ген. Объект ничего не делает.
            0x01, // - Ген размножения. Позволяет создать потомка.

            // Гены питания 0x1
            0x10, // - Ген фотосинтеза. Получние энергии из солнечного света. + X2
            0x11, // - Ген хищнечиства. Получение энергии из съеденых особей. Получает их энергию +1 за биомассу.

            // Гены движения 0x2
            0x20, // - Ген движения. Позволяет переместиться по направлению скорости
            0x21, // - Ген изменения скорости. +1 к скорости вправо.
            0x22, // - Ген изменения скорости. +1 к скорости влево.
            0x23, // - Ген изменения скорости. +1 к скорости вверх.
            0x24, // - Ген изменения скорости. +1 к скорости вниз.

            // Гены чувств
            0x30, // - Ген Чувства. Особь оглядывается вокруг в поисках пищи. 
                  //   Скорость изменяется по направлению к жертве. Скорость максимальна.
                  //   Тратится по X1 энергии эа каждое очко изменения скорости.
            0x31 // - Ген Чувства. Особь оглядывается вокруг в поисках опасности. 
                  //   Скорость изменяется по направлению от хищника. Скорость максимальна.
                  //   Тратится по X1 энергии эа каждое очко изменения скорости.

        };
        private int _maxSpeed;

        /// <summary>
        /// Скорость перемещения особи по горизонтали
        /// </summary>
        protected int LeftSpeed
        {
            get => left_speed;
            set
            {
                int New = value;
                if (New > 0 && New > MaxSpeed) New = Convert.ToInt32(MaxSpeed);
                if (New < 0 && New < -MaxSpeed) New = Convert.ToInt32(-MaxSpeed);
                left_speed = New;
            }
        }

        /// <summary>
        /// Скорость перемещения особи по вертикали
        /// </summary>
        protected int TopSpeed
        {
            get => top_speed;
            set
            {
                int New = value;
                if (New > 0 && New > MaxSpeed) New = Convert.ToInt32(MaxSpeed);
                if (New < 0 && New < -MaxSpeed) New = Convert.ToInt32(-MaxSpeed);
                top_speed = New;
            }
        }

        /// <summary>
        /// Перемещение
        /// </summary>
        protected void Move()
        {
            double Left = Margin.Left + LeftSpeed * ActualWidth;
            if (Left < 0) Left += ((Grid)Parent).ActualWidth;
            if (Left > ((Grid)Parent).ActualWidth) Left -= ((Grid)Parent).ActualWidth;

            double Top = Margin.Top + TopSpeed * ActualHeight;
            if (Top < 0) Top += ((Grid)Parent).ActualHeight;
            if (Top > ((Grid)Parent).ActualHeight) Top -= ((Grid)Parent).ActualHeight;

            Margin = new Thickness(Left, Top, 0, 0);
        }

        /// <summary>
        /// Перемещение на одну клетку в указанном направлении
        /// </summary>
        protected void Move(byte Direction)
        {
            double L = 0, T = 0;

            switch (Direction)
            {
                case 0: L = 0; T = -1; break;
                case 1: L = 1; T = -1; break;
                case 2: L = 1; T = 0; break;
                case 3: L = 1; T = 1; break;
                case 4: L = 0; T = 1; break;
                case 5: L = -1; T = 1; break;
                case 6: L = -1; T = 0; break;
                case 7: L = -1; T = -1; break;
            }

            double Left = Margin.Left + L * 10;

            double Top = Margin.Top + T * 10;

            Margin = new Thickness(Left, Top, 0, 0);
        }
        #endregion

        #region Свойства класса
        /// <summary>
        /// Запас энергии особи
        /// </summary>
        public double Energy
        {
            get => energy;
            set
            {
                if (!alife) return;

                energy = value;
                if (energy < 0) Die();
                Colorize();
            }
        }

        public int MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                _maxSpeed = value;
                if (_maxSpeed < 0) _maxSpeed = 0;
            } }

        /// <summary>
        /// Биом, в котором находится вид. Служит для взаимодействия с другими организмами.
        /// </summary>
        public Biome MyBiome { get; set; }
        #endregion

        public Species()
        {
            InitializeComponent();

            for (int i = 0; i < DNA_Count - 3; i++)
                DNA[i] = 0x10;
            for (int i = DNA_Count - 3; i < DNA_Count; i++)
                DNA[i] = 0x01;
        }

        #region Внутренние методы
        /// <summary>
        /// Убивает особь, но оставляет её труп.
        /// </summary>
        private void Die()
        {
            alife = false;
        }

        /// <summary>
        /// Уничтожает особь или труп особи. 
        /// </summary>
        private void Destroy()
        {
            if (alife) Die();
            Dispose();
        }

        /// <summary>
        /// Раскрашивает особь в зависимости от параметра
        /// </summary>
        private void Colorize(string Param = "Energy")
        {
            if (!alife)
            {
                RectColor = new SolidColorBrush(Color.FromRgb(128, 128, 128));
                Rect.Fill = RectColor;
                return;
            }

            switch (Param)
            {
                case "Energy":
                    byte Red = 0;
                    byte Green = 0;
                    byte Blue = 0;

                    if (energy < 3)
                    {
                        Green = Convert.ToByte(energy / 3 * 255);
                        Red = 255;
                    }
                    else
                    {
                        Green = 255;
                        Red = energy > 6
                            ? Convert.ToByte(0)
                            : Convert.ToByte((6 - energy) / 3 * 255);
                    }
                    RectColor = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));
                    break;
            }

            Rect.Fill = RectColor;
        }

        /// <summary>
        /// Действие, выполняемое в зависимости от ДНК
        /// </summary>
        protected void Do()
        {
            byte Gene = DNA[RND.Next(100)];

            switch (Gene)
            {
                case 0x01:
                    Birth(); break;
                case 0x10:
                    PhotoSynthesis(); break;
                case 0x11:
                    Eat(); break;
            }
        }

        /// <summary>
        /// Создать новую особь.
        /// </summary>
        protected void Birth()
        {
            bool[] Free = MyBiome.IsNear(this);
            List<byte> FreeDir = new List<byte>();
            for (byte i = 0; i < 8; i++)
                if (Free[i]) FreeDir.Add(i);
            if (FreeDir.Count == 0) return;
            if (Energy < 2.5) return;
            Energy = (Energy - 1) / 2;

            Species NewSpecies = new Species()
            {
                Energy = Energy,
                Margin = Margin,
                MaxSpeed = MaxSpeed,
                Width = Width,
                Height = Height,
                HorizontalAlignment = HorizontalAlignment,
                VerticalAlignment = VerticalAlignment
            };
            
            NewSpecies.Mutate();
            NewSpecies.Move(FreeDir[RND.Next(FreeDir.Count)]);

            MyBiome.Add(NewSpecies);
        }

        /// <summary>
        /// Получает энергию методом фотосинтеза
        /// </summary>
        protected void PhotoSynthesis()
        {
            Energy += 2 * EnergyPerStep;
        }

        protected void Eat()
        {

        }

        /// <summary>
        /// Функция мутации. 
        /// </summary>
        protected void Mutate()
        {
            int Seed = RND.Next(DNA_Count + OtherElementsToMutate);
            if (Seed < DNA_Count)
                DNA[Seed] = NewGene();
            switch (Seed - DNA_Count)
            {
                case 0:
                    MaxSpeed += RND.Next(3) - 1; break;
            }
        }

        /// <summary>
        /// Выдаёт случайный ген
        /// </summary>
        /// <returns></returns>
        protected byte NewGene()
        {
            return Genes[RND.Next(Genes.Length)];
        }
        #endregion

        #region Внешние свойства
        /// <summary>
        /// Главный метод особи. Делает один заранее запланированный шаг
        /// </summary>
        public void Step()
        {
            Energy -= EnergyPerStep;
            if (!alife) return;
            Do();
        }

        /// <summary>
        /// Реализация интерфейса IDisposable
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
