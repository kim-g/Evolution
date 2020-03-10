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
    /// Логика взаимодействия для Biome.xaml
    /// </summary>
    public partial class Biome : UserControl
    {
        #region Константы
        /// <summary>
        /// Относительные смещения для направлений
        /// </summary>
        public static int[,] Directions = new int[8, 2]
        {
            {  0, -1 },
            {  1, -1 },
            {  1,  0 },
            {  1,  1 },
            {  0,  1 },
            { -1,  1 },
            { -1,  0 },
            { -1, -1 }
        };
        #endregion

        #region Внутренние параметры
        /// <summary>
        /// Список всех особей
        /// </summary>
        protected List<Species> individuals = new List<Species>();

        /// <summary>
        /// Список новых особей
        /// </summary>
        protected List<Species> ToAdd = new List<Species>();

        /// <summary>
        /// Список удаляемых особей
        /// </summary>
        protected List<Species> ToRemove = new List<Species>();

        /// <summary>
        /// Карта расположения особей.
        /// </summary>
        protected Species[,] Map;

        /// <summary>
        /// Массив элементов Rectangle лоя отображения резльтатов
        /// </summary>
        protected Rectangle[,] Rectangles;

        protected bool[,] Changed;

        /// <summary>
        /// Словарт цветов, обозначающих различные энергии.
        /// </summary>
        protected Dictionary<double, Brush> EnergyColors = new Dictionary<double, Brush>();

        protected Brush Stroke = new SolidColorBrush(Colors.Black);
        #endregion

        #region Внешние свойства

        /// <summary>
        /// Ширина биома в элементах
        /// </summary>
        public int TableWidth { get; private set; }

        /// <summary>
        /// Высота биома в элементах
        /// </summary>
        public int TableHeight { get; private set; }

        /// <summary>
        /// Размер элемента (особи) в биоме
        /// </summary>
        public double ElementSize { get; set; } = 10;

        public Random RandomSeeds = new Random();

        #endregion

        #region Конструкторы
        public Biome()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Инициализация биома
        /// </summary>
        public void Initialize(double width, double height)
        {
            // Создание поля для биома 
            TableWidth = Convert.ToInt32(Math.Floor(width / ElementSize));
            TableHeight = Convert.ToInt32(Math.Floor(height / ElementSize));

            Width = TableWidth * ElementSize;
            Height = TableHeight * ElementSize;

            Map = new Species[TableWidth, TableHeight];
            Changed = new bool[TableWidth, TableHeight];
            Rectangles = new Rectangle[TableWidth, TableHeight];

            for (int i = 0; i < TableWidth; i++)
                MainGrid.ColumnDefinitions.Add(new ColumnDefinition()
                { Width = new GridLength(10, GridUnitType.Pixel) });
            for (int i = 0; i < TableHeight; i++)
                MainGrid.RowDefinitions.Add(new RowDefinition()
                { Height = new GridLength(10, GridUnitType.Pixel) });

            for (int i = 0; i < TableWidth; i++)
                for (int j = 0; j < TableHeight; j++)
                {
                    Rectangles[i, j] = new Rectangle()
                    {
                        Width = double.NaN,
                        Height = double.NaN,
                        StrokeThickness = 1
                    };
                    Grid.SetColumn(Rectangles[i, j], i);
                    Grid.SetRow(Rectangles[i, j], j);
                    MainGrid.Children.Add(Rectangles[i, j]);
                    Changed[i, j] = false;
                }

            // Создание цветов энергий
            for (double i = 0; i < 6; i += 0.1)
            {
                byte Red = 0;
                byte Green = 0;
                byte Blue = 0;

                if (i < 3)
                {
                    Green = Convert.ToByte(i / 3 * 255);
                    Red = 255;
                }
                else
                {
                    Green = 255;
                    Red = i > 6
                        ? Convert.ToByte(0)
                        : Convert.ToByte((6 - i) / 3 * 255);
                }
                EnergyColors[i] = new SolidColorBrush(Color.FromRgb(Red, Green, Blue));
            }

            EnergyColors[6] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            EnergyColors[-1] = new SolidColorBrush(Color.FromRgb(200, 200, 200));

            ShowAll();
        }
        #endregion

        #region Внутренние методы

        /// <summary>
        /// Убирает квадрат.
        /// </summary>
        /// <param name="Rect"></param>
        protected void ShowNull(Rectangle Rect)
        {
            Rect.Fill = null;
            Rect.Stroke = null;
        }

        /// <summary>
        /// Раскрашивает квадрат в зависимости от энергии
        /// </summary>
        /// <param name="Rect"></param>
        /// <param name="Energy"></param>
        protected void Colorize(Rectangle Rect, double Energy)
        {
            Rect.Stroke = Stroke;
            if (Energy <= 0)
            {
                Rect.Fill = EnergyColors[-1];
                return;
            }

            for (double i = 0; i < 6; i += 0.1)
            {
                if (Energy >= i && Energy < i + 0.1)
                {
                    Rect.Fill = EnergyColors[i];
                    return;
                }
            }

            Rect.Fill = EnergyColors[6];
        }
        #endregion

        #region Внешние методы
        /// <summary>
        /// Добавление новой особи
        /// </summary>
        /// <param name="New">Экземпляр вида (новая особь)</param>
        public void Add(Species New)
        {
            if (Map[New.Left, New.Top] != null) return;

            New.Changed = true;
            ToAdd.Add(New);
            Map[New.Left, New.Top] = New;
            New.MyBiome = this;
        }

        /// <summary>
        /// Уничтожение особи
        /// </summary>
        /// <param name="Old">Особь для уничтожения</param>
        public void Remove(Species Old)
        {
            ToRemove.Remove(Old);
            Old.Kill();
            Map[Old.Left, Old.Top] = null;
            Changed[Old.Left, Old.Top] = true;
        }

        /// <summary>
        /// Массовое добавление новых элементов после хода
        /// </summary>
        public void AddAll()
        {
            foreach (Species Ind in ToAdd)
                individuals.Add(Ind);

            ToAdd = new List<Species>();
        }

        /// <summary>
        /// Массовое удаление новых элементов после хода
        /// </summary>
        public void RemoveAll()
        {
            foreach (Species Ind in ToAdd)
            {
                individuals.Remove(Ind);
                Ind.Dispose();
            }

            ToRemove = new List<Species>();
        }

        /// <summary>
        /// Показывает направление, в котором есть свободные клетки относительно особи
        /// </summary>
        /// <param name="El">Особь, относительно которой определяется направление</param>
        /// <returns></returns>
        public bool[] Direction(Species El)
        {
            bool[] Out = new bool[8];
            for (int i = 0; i < 8; i++)
            {
                IntPoint NewDir = MoveOne(El.Position, i);

                if (NewDir.Y < 0 || NewDir.Y > TableHeight) Out[i] = false;
                else Out[i] = Free(NewDir);
                
            }

            return Out;
        }

        /// <summary>
        /// Производит однократное изменение всех объектов биома.
        /// </summary>
        public void Step()
        {
            foreach (Species El in individuals)
                El.Step();
            AddAll();
            RemoveAll();
        }

        /// <summary>
        /// Показать все изменения на форме
        /// </summary>
        public void Show()
        {
            // Отображаем изменённые поля на карте
            for (int i = 0; i < TableWidth; i++)
                for (int j = 0; j < TableHeight; j++)
                    if (Changed[i, j])
                    {
                        if (Map[i, j] == null) ShowNull(Rectangles[i, j]);
                        else
                        {
                            Colorize(Rectangles[i, j], Map[i, j].Energy);
                            Map[i, j].Changed = false;
                        }
                        Changed[i, j] = false;
                        
                    }

            // Отображаем изменённые объекты
            foreach (Species El in individuals.Where(Obj => Obj.Changed))
            {
                Colorize(Rectangles[El.Left, El.Top], El.Energy);
                Changed[El.Left, El.Top] = false;
                El.Changed = false;
            }
        }

        /// <summary>
        /// Полностью обновить визуальное отображение биома.
        /// </summary>
        public void ShowAll()
        {
            for (int i = 0; i < TableWidth; i++)
                for (int j = 0; j < TableHeight; j++)
                {
                    if (Map[i, j] == null) ShowNull(Rectangles[i, j]);
                    else Colorize(Rectangles[i, j], Map[i, j].Energy);
                    Changed[i, j] = false;
                    if (Map[i, j] != null) Map[i, j].Changed = false;
                }
        }

        /// <summary>
        /// Отображает ближайшие к особи цели
        /// </summary>
        /// <param name="El">Особь</param>
        /// <param name="Distance">Расстояние</param>
        /// <returns></returns>
        public List<Species> Near(Species El, int Distance = 1)
        {
            List<Species> Out = new List<Species>();
            if (Distance < 1) return Out;

            for (int i = 0; i < 8; i++)
            {
                IntPoint NewPlace = MoveOne(El.Position, i);
                if (NewPlace.Y < 0 || NewPlace.Y >= TableHeight) continue;

                if (Map[NewPlace.X, NewPlace.Y] != null)
                    Out.Add(Map[NewPlace.X, NewPlace.Y]);
            }

            return Out;
        }

        /// <summary>
        /// Обозначить положение как изменённое
        /// </summary>
        /// <param name="Position"></param>
        public void Change(IntPoint Position)
        {
            Changed[Position.X, Position.Y] = true;
        }

        /// <summary>
        /// Показать, является ли клетка пустой
        /// </summary>
        /// <param name="Position"></param>
        /// <returns></returns>
        public bool Free(IntPoint Position)
        {
            if (Position.Y < 0 || Position.Y >= TableHeight) return false;
            return Map[Position.X, Position.Y] == null;
        }

        /// <summary>
        /// Сдвигает точку в указанном направлении на одно деление
        /// </summary>
        /// <param name="From">Начальная точка</param>
        /// <param name="Dir">Направление</param>
        /// <returns></returns>
        public IntPoint MoveOne(IntPoint From, int Dir)
        {
            IntPoint Out = new IntPoint()
            {
                X = From.X + Directions[Dir, 0],
                Y = From.Y + Directions[Dir, 1]
            };

            if (Out.X < 0) Out.X += TableWidth;
            if (Out.X >= TableWidth) Out.X -= TableWidth;

            return Out;
        }

        /// <summary>
        /// Переместить особь в новое место
        /// </summary>
        /// <param name="El">Особь</param>
        /// <param name="To">Место</param>
        public void Move(Species El, IntPoint To)
        {
            if (To.Y < 0 || To.Y >= TableHeight) return;
            if (To.X < 0) To.X += TableWidth;
            if (To.X >= TableWidth) To.X -= TableWidth;
            if (Map[To.X, To.Y] != null) return;

            Change(El.Position);
            Map[El.Position.X, El.Position.Y] = null;
            El.Position = To;
            Map[El.Position.X, El.Position.Y] = El;
            El.Changed = true;
        }

        #endregion
    }

    public struct IntPoint
    {
        public int X;
        public int Y;
    }
}
