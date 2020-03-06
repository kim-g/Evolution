using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Evolution
{
    public class Biome
    {
        public Biome(Grid gridPanel)
        {
            GridPanel = gridPanel ?? throw new ArgumentNullException(nameof(gridPanel));
        }
        #region Внутренние параметры

        #endregion

        #region Внешние свойства
        /// <summary>
        /// Список всех особей
        /// </summary>
        public List<Species> Individuals { get; set; } = new List<Species>();
        public List<Species> ToAdd { get; set; } = new List<Species>();
        public List<Species> ToRemove { get; set; } = new List<Species>();

        public Grid GridPanel { get; set; }
        #endregion

        #region Внутренние методы
        private bool[] Direction(Species El, bool[] Out, Species Ind)
        {
            if (Ind.Margin.Left == El.Margin.Left && Ind.Margin.Top == El.Margin.Top - Ind.Height)
                Out[0] = false;
            if (Ind.Margin.Left == El.Margin.Left + El.Width && Ind.Margin.Top == El.Margin.Top - Ind.Height)
                Out[1] = false;
            if (Ind.Margin.Left == El.Margin.Left + El.Width && Ind.Margin.Top == El.Margin.Top)
                Out[2] = false;
            if (Ind.Margin.Left == El.Margin.Left + El.Width && Ind.Margin.Top == El.Margin.Top + El.Height)
                Out[3] = false;
            if (Ind.Margin.Left == El.Margin.Left && Ind.Margin.Top == El.Margin.Top + El.Height)
                Out[4] = false;
            if (Ind.Margin.Left == El.Margin.Left - Ind.Width && Ind.Margin.Top == El.Margin.Top + El.Height)
                Out[5] = false;
            if (Ind.Margin.Left == El.Margin.Left - Ind.Width && Ind.Margin.Top == El.Margin.Top)
                Out[6] = false;
            if (Ind.Margin.Left == El.Margin.Left - Ind.Width && Ind.Margin.Top == El.Margin.Top - Ind.Height)
                Out[7] = false;
            return Out;
        }
        #endregion

        #region Внешние методы
        /// <summary>
        /// Добавление новой особи
        /// </summary>
        /// <param name="New">Экземпляр вида (новая особь)</param>
        public void Add(Species New)
        {
            ToAdd.Add(New);
        }

        /// <summary>
        /// Уничтожение особи
        /// </summary>
        /// <param name="Old">Особь для уничтожения</param>
        public void Remove(Species Old)
        {
            ToRemove.Remove(Old);
        }

        /// <summary>
        /// Массовое добавление новых элементов после хода
        /// </summary>
        public void AddAll()
        {
            foreach (Species Ind in ToAdd)
            {
                Individuals.Add(Ind);
                Ind.MyBiome = this;
                GridPanel.Children.Add(Ind);
            }

            ToAdd = new List<Species>();
        }

        /// <summary>
        /// Массовое удаление новых элементов после хода
        /// </summary>
        public void RemoveAll()
        {
            foreach (Species Ind in ToAdd)
            {
                Individuals.Remove(Ind);
                GridPanel.Children.Remove(Ind);
            }

            ToRemove = new List<Species>();
        }

        /// <summary>
        /// Показывает свободные клетки около позиции
        /// </summary>
        /// <param name="Position">Элемент, относительно которого смотрим свободные клетки</param>
        /// <returns></returns>
        public bool[] IsNear(Species El)
        {
            bool[] Out = new bool[8] { true, true, true, true, true, true, true, true };

            if (El.Margin.Top - 1 < 0)
                Out[0] = false;
            if (El.Margin.Left + 1 > GridPanel.ActualWidth && El.Margin.Top - 1 < 0)
                Out[1] = false;
            if (El.Margin.Left + 1 > GridPanel.ActualWidth)
                Out[2] = false;
            if (El.Margin.Left + 1 > GridPanel.ActualWidth && El.Margin.Top + 1 > GridPanel.ActualHeight)
                Out[3] = false;
            if (El.Margin.Top + 1 > GridPanel.ActualHeight)
                Out[4] = false;
            if (El.Margin.Left - 1 < 0 && El.Margin.Top + 1 > GridPanel.ActualHeight)
                Out[5] = false;
            if (El.Margin.Left - 1 < 0)
                Out[6] = false;
            if (El.Margin.Left - 1 < 0 && El.Margin.Top - 1 < 0)
                Out[7] = false;

            foreach (Species Ind in Individuals)
            {
                Out = Direction(El, Out, Ind);
            }

            foreach (Species Ind in ToAdd)
            {
                Out = Direction(El, Out, Ind);
            }

            return Out;
        }

        /*public List<Species> Near(Species Obj)
        {

        }*/

        
        #endregion
    }
}
