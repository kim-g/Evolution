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

        /// <summary>
        /// Словарт цветов, обозначающих различные типы питания.
        /// </summary>
        protected Dictionary<byte, Brush> NutritionColors = new Dictionary<byte, Brush>();

        protected Brush Stroke = new SolidColorBrush(Colors.Black);

        private double mutagen = 10;

        /// <summary>
        /// Количество логических ядер процессора.
        /// </summary>
        private int Processors = Environment.ProcessorCount;

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

        /// <summary>
        /// Генератор случайных чисел для генерации ключей генераторов особей.
        /// </summary>
        public Random RandomSeeds = new Random();

        /// <summary>
        /// Количество активных (живых и мёртвых) элементов на карте.
        /// </summary>
        public int ElementsCount { get => individuals.Count + ToAdd.Count - ToRemove.Count; }

        /// <summary>
        /// Количество особей-продуцентов
        /// </summary>
        public int ProducerCount { get => individuals.Where(predicate: x =>
        {
            if (x == null) return false;
            return x.Nutrition == 0;
        }).Count(); }

        /// <summary>
        /// Количество особей со смешанным типом питания
        /// </summary>
        public int MixedCount { get => individuals.Where(x =>
        {
            if (x == null) return false;
            return x.Nutrition == 1;
        }).Count(); }

        /// <summary>
        /// Количество особей-хищников
        /// </summary>
        public int PredatorCount { get => individuals.Where(x =>
        {
            if (x == null) return false;
            return x.Nutrition == 2;
        }).Count(); }

        /// <summary>
        /// Список количества всех особей в каждый момент времени
        /// </summary>
        public List<double> CountList { get; private set; } = new List<double>();

        /// <summary>
        /// Список количества всех продуцентов в каждый момент времени
        /// </summary>
        public List<double> ProducerList { get; private set; } = new List<double>();

        /// <summary>
        /// Список количества всех особей со смешанным типом питания в каждый момент времени
        /// </summary>
        public List<double> MixedList { get; private set; } = new List<double>();

        /// <summary>
        /// Список количества всех хищников в каждый момент времени
        /// </summary>
        public List<double> PredatorList { get; private set; } = new List<double>();

        /// <summary>
        /// Выбор способа колоризации
        /// 0 - Раскрасить по энергии
        /// 1 - Раскрасить по способам питания
        /// </summary>
        public byte MethodOfColorization { get; set; } = 0;

        /// <summary>
        /// Процент мутагенности среды
        /// </summary>
        /// <returns></returns>
        public double Mutagen
        {
            get => mutagen;
            set
            {
                mutagen = value;
                if (mutagen < 0) mutagen = 0;
                if (mutagen > 100) mutagen = 100;
            }
        }

        /// <summary>
        /// Флаг, позволяющий хищником поедать других хищников
        /// </summary>
        public bool PredatorEatPredator { get; set; } = false;

        /// <summary>
        /// Флаг, позволяющий хищником фотосинтезировать при наличии подобных генов
        /// </summary>
        public bool PredatorHavePhotosynthesys { get; set; } = false;

        /// <summary>
        /// Флаг, позволяющий растениям поедать другие клетки при наличии соответствующих генов
        /// </summary>
        public bool ProducerEatOther { get; set; } = false;

        /// <summary>
        /// Номер текущего шага. 
        /// </summary>
        public int Time { get; private set; } = 0;

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

            NutritionColors[0] = new SolidColorBrush(Color.FromRgb(0, 255, 0));
            NutritionColors[1] = new SolidColorBrush(Color.FromRgb(0, 255, 255));
            NutritionColors[2] = new SolidColorBrush(Color.FromRgb(0, 0, 255));

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
        /// Раскрашивает клетку в зависимости от выбранного стиля
        /// </summary>
        /// <param name="Rect">Клетка</param>
        /// <param name="El">Связанная с клеткой особь</param>
        protected void Colorize(Rectangle Rect, Species El)
        {
            switch (MethodOfColorization)
            {
                case 0:
                    ColorizeEnergy(Rect, El.Energy); break;
                case 1:
                    ColorizeNutrition(Rect, El.Nutrition, El.Energy); break;
            }
        }

        /// <summary>
        /// Раскрашивает квадрат в зависимости от энергии
        /// </summary>
        /// <param name="Rect"></param>
        /// <param name="Energy"></param>
        protected void ColorizeEnergy(Rectangle Rect, double Energy)
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

        /// <summary>
        /// Раскрашивает квадрат в зависимости от энергии
        /// </summary>
        /// <param name="Rect">Квадрат для раскраски</param>
        /// <param name="Nutrition">Тип питания</param>
        /// <param name="Energy">Энергия (для определения трупа)</param>
        protected void ColorizeNutrition(Rectangle Rect, byte Nutrition, double Energy)
        {
            Rect.Stroke = Stroke;
            if (Energy <= 0)
            {
                Rect.Fill = EnergyColors[-1];
                return;
            }

            Rect.Fill = NutritionColors[Nutrition];
        }

        /// <summary>
        /// Последовательно делает шаг для определённого набора особей.
        /// </summary>
        /// <param name="Start"></param>
        /// <param name="Count"></param>
        protected void SubStep(int Start, int Count)
        {
            for (int i = Start; i < Start + Count; i++)
                if (individuals.Count > i)
                {
                    if (individuals[i] == null) continue;
                    individuals[i].Step();
                }
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
            if (Old == null) return;
            ToRemove.Add(Old);
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
            foreach (Species Ind in ToRemove)
            {
                if (Ind == null) continue;
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
            Time++;

            int Block = Convert.ToInt32(Math.Ceiling((double)individuals.Count / Processors));
            Task[] Tasks = new Task[Processors];
            for (int i = 0; i < Processors; i++)
            {
                int Start = i * Block;
                Tasks[i] = Task.Factory.StartNew(() => SubStep(Start, Block));
            }
            Task.WaitAll(Tasks);

            /*foreach (Species El in individuals)
                El.Step();*/
            AddAll();
            RemoveAll();

            CountList.Add(individuals.Count);
            ProducerList.Add(ProducerCount);
            MixedList.Add(MixedCount);
            PredatorList.Add(PredatorCount);
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
                            Colorize(Rectangles[i, j], Map[i, j]);
                            Map[i, j].Changed = false;
                        }
                        Changed[i, j] = false;
                        
                    }

            // Отображаем изменённые объекты
            foreach (Species El in individuals.Where(Obj =>
            {
                if (Obj == null) return false;
                return Obj.Changed;
            }))
            {
                Colorize(Rectangles[El.Left, El.Top], El);
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
                    else Colorize(Rectangles[i, j], Map[i, j]);
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

        /// <summary>
        /// Показывает наличие соседних объектов в зависимости от чувствителтьности объекта
        /// </summary>
        /// <param name="El">Объект-сенсор</param>
        /// <returns>Список соседних объектов</returns>
        public List<Species> Sense(Species El)
        {
            List<Species> Found= new List<Species>();

            for (int i = El.Left - El.Sensitivity; i <= El.Left + El.Sensitivity; i++)
                for (int j = El.Top - El.Sensitivity; j <= El.Top + El.Sensitivity; j++)
                {
                    if (j < 0 || j >= TableHeight) continue;
                    int I = i < 0 ? i + TableWidth : i;
                    I = I >= TableWidth ? I - TableWidth : I;
                    if (Map[I, j] != null) Found.Add(Map[I, j]);
                }

            return Found;
        }

        #endregion
    }

    public struct IntPoint
    {
        public int X;
        public int Y;
    }
}
