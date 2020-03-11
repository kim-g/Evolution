using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace Evolution
{
    public class Species
    {
        #region Константы
        protected const double EnergyPerStep = 0.1;
        protected const int DNA_Count = 100;
        protected const int OtherElementsToMutate = 2;
        #endregion

        #region private параметры
        double energy;
        bool alife = true;
        bool destroying = false;
        Random RND;
        int left_speed = 0;
        int top_speed = 0;
        byte[] DNA = new byte[100];
        int sensitivity = 0;

        /// <summary>
        /// Список генов для DNA
        /// </summary>
        byte[] Genes = new byte[]
        { 
            // Базовые гены
            0x00, // - Пустой ген. Объект ничего не делает.
            0x01,0x01,0x01,0x01,0x01,0x01, // - Ген размножения. Позволяет создать потомка.

            // Гены питания 0x1
            0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10,0x10, // - Ген фотосинтеза. Получние энергии из солнечного света. + X2
            0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11,0x11, // - Ген хищнечиства. Получение энергии из съеденых особей. Получает их энергию +1 за биомассу.

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
        /// Положение на карте биома
        /// </summary>
        protected IntPoint position;
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

                if (energy != value) Changed = true;
                energy = value;
                if (energy < 0) Die();
            }
        }

        /// <summary>
        /// Максимальная скорость, которую может развить особь.
        /// </summary>
        public int MaxSpeed
        {
            get => _maxSpeed;
            set
            {
                _maxSpeed = value;
                if (_maxSpeed < 0) _maxSpeed = 0;
            }
        }

        /// <summary>
        /// Чувствительность особи.
        /// </summary>
        public int Sensitivity
        {
            get => sensitivity;
            set
            {
                sensitivity = value;
                if (sensitivity < 0) sensitivity = 0;
            }
        }

        /// <summary>
        /// Биом, в котором находится вид. Служит для взаимодействия с другими организмами.
        /// </summary>
        public Biome MyBiome { get; set; }

        /// <summary>
        /// Положение слева на карте биома 
        /// </summary>
        public int Left
        {
            get => position.X;
            set
            {
                position.X = value;
                if (position.X < 0) position.X += MyBiome.TableWidth;
                if (position.X >= MyBiome.TableWidth) position.X -= MyBiome.TableWidth;
            }
        }

        /// <summary>
        /// Положение сверху на карте биома 
        /// </summary>
        public int Top
        {
            get => position.Y;
            set
            {
                position.Y = value;
                if (position.Y < 0) position.Y = 0;
                if (position.Y >= MyBiome.TableHeight) position.Y = MyBiome.TableHeight-1;
            }
        }

        /// <summary>
        /// Положение на карте биома
        /// </summary>
        public IntPoint Position { get => position; set => position = value; }

        /// <summary>
        /// Тип питания
        /// </summary>
        public byte Nutrition { get; protected set; }
        #endregion

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="RandomSeed">Зерно для рандомайзера</param>
        public Species(int RandomSeed)
        {
            RND = new Random(RandomSeed);

            for (int i = 0; i < DNA_Count - 5; i++)
                DNA[i] = 0x10;
            for (int i = DNA_Count - 5; i < DNA_Count; i++)
                DNA[i] = 0x01;
            Nutrition = SetNutrition();
        }

        #region Внутренние методы

        /// <summary>
        /// Перемещение
        /// </summary>
        protected void Move()
        {
            MyBiome.Move(this, new IntPoint()
            {
                X = Left + LeftSpeed,
                Y = Top + TopSpeed
            });
            Energy -= EnergyPerStep * (Math.Abs(LeftSpeed) + Math.Abs(TopSpeed));
        }

        /// <summary>
        /// Перемещение на одну клетку в указанном направлении
        /// </summary>
        protected void Move(byte Direction)
        {
            MyBiome.Move(this, new IntPoint()
            {
                X = Left + Biome.Directions[Direction, 0],
                Y = Top + Biome.Directions[Direction, 1]
            });
        }

        /// <summary>
        /// Передвинуть особь до добавления в биом.
        /// </summary>
        /// <param name="Direction"></param>
        protected void MoveOutOfBiome(byte Direction)
        {
            Left += Biome.Directions[Direction, 0];
            Top += Biome.Directions[Direction, 1];
            Changed = true;
        }

        /// <summary>
        /// Убивает особь, но оставляет её труп.
        /// </summary>
        private void Die()
        {
            Changed = true;
            MyBiome.Change(Position);
            alife = false;
        }

        /// <summary>
        /// Уничтожает особь или труп особи. 
        /// </summary>
        private void Destroy()
        {
            destroying = true;
            if (alife) Die();
            MyBiome.Remove(this);
            MyBiome.Change(Position);
            Dispose();
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
                case 0x20:
                    Move(); break;
                case 0x21:
                    LeftSpeed++; break;
                case 0x22:
                    LeftSpeed--; break;
                case 0x23:
                    TopSpeed--; break;
                case 0x24:
                    TopSpeed++; break;
                case 0x30:
                    SenseFoodAll(); break;

            }
        }

        /// <summary>
        /// Создать новую особь.
        /// </summary>
        protected void Birth()
        {
            bool[] Free = MyBiome.Direction(this);
            List<byte> FreeDir = new List<byte>();
            for (byte i = 0; i < 8; i++)
                if (Free[i]) FreeDir.Add(i);
            if (FreeDir.Count == 0) return;
            if (Energy < 2.5) return;
            Changed = true;
            Energy = (Energy - 1) / 2;

            Species NewSpecies = new Species(MyBiome.RandomSeeds.Next(int.MaxValue))
            {
                Energy = Energy,
                MaxSpeed = MaxSpeed,
                DNA = DNA,
                Changed = true,
                MyBiome = MyBiome,
                Position = Position
            };

            NewSpecies.Mutate();
            NewSpecies.MoveOutOfBiome(FreeDir[RND.Next(FreeDir.Count)]);

            MyBiome.Add(NewSpecies);
        }

        /// <summary>
        /// Получает энергию методом фотосинтеза
        /// </summary>
        protected void PhotoSynthesis()
        {
            Energy += 5 * EnergyPerStep;
            Changed = true;
        }

        protected void Eat()
        {
            List<Species> Near = MyBiome.Near(this);

            if (Near.Count == 0) return;

            Species Victim = Near[RND.Next(Near.Count)];
            Energy += Victim.Energy + 1;
            Victim.Eaten();
            Changed = true;
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
                case 1:
                    Sensitivity += RND.Next(3) - 1; break;
            }

            Nutrition = SetNutrition();
        }

        /// <summary>
        /// Выдаёт случайный ген
        /// </summary>
        /// <returns></returns>
        protected byte NewGene()
        {
            return Genes[RND.Next(Genes.Length)];
        }

        /// <summary>
        /// Определяет ближайшую жертву независимо от её качеств и стремися к ней.
        /// </summary>
        protected void SenseFoodAll()
        {
            Energy -= EnergyPerStep * Sensitivity;

            int dist = MyBiome.TableWidth * MyBiome.TableHeight;
            List<Species> Clothest = new List<Species>(); 

            foreach (Species El in MyBiome.Sense(this))
            {
                int D = Distation(El);
                if (dist > D)
                {
                    Clothest = new List<Species>();
                    dist = D;
                    Clothest.Add(El);
                    continue;
                }
                if (dist == D) Clothest.Add(El);
            }

            if (Clothest.Count == 0) return;

            Species Aim;
            if (Clothest.Count == 1) Aim = Clothest[0];
            else
            {
                Aim = Clothest.OrderBy(x => Math.Abs(Math.Abs(x.Left - Left) - Math.Abs(x.Top - Top))).ToArray()[0];
            }
            LeftSpeed = GoTo(Left, Aim.Left);
            TopSpeed = GoTo(Top, Aim.Top);
        }

        /// <summary>
        /// Установить тип питания
        /// </summary>
        /// <returns></returns>
        protected byte SetNutrition()
        {
            byte PhS = 0;
            byte Predator = 0;

            foreach (byte i in DNA)
                switch (i)
                {
                    case 0x10: PhS++; break;
                    case 0x11: Predator++; break;
                }

            if (Math.Abs(PhS - Predator) * 100 / (PhS + Predator) < 10) return 1;
            if (PhS > Predator) return 0;
            return 2;
        }

        /// <summary>
        /// Вычисляет расстояние между двумя объектами
        /// </summary>
        /// <returns></returns>
        protected int Distation(Species Other)
        {
            return Math.Abs(Left - Other.Left) + Math.Abs(Top - Other.Top);
        }

        protected int GoTo(int My, int Other)
        {
            if (Math.Abs(My - Other) > MaxSpeed) return My - Other;
            if (My == Other) return 0;
            if (My > Other) return My - Other + 1;
            return My - Other - 1;
        }
        #endregion

        #region Внешние свойства
        /// <summary>
        /// Главный метод особи. Делает один заранее запланированный шаг
        /// </summary>
        public void Step()
        {
            if (destroying) return;
            Energy -= EnergyPerStep;
            if (!alife)
            {
                if (MyBiome.Free(MyBiome.MoveOne(Position, 4)))
                    Move(4);
                return;
            }
            Do();
        }

        /// <summary>
        /// Убивает особь
        /// </summary>
        public void Kill()
        {
            Die();
        }

        /// <summary>
        /// Особь съедается кем-либо
        /// </summary>
        /// <returns></returns>
        public void Eaten()
        {
            Destroy();
        }

        /// <summary>
        /// Флаг для отображения изменений.
        /// </summary>
        public bool Changed { get; set; } = true;

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
