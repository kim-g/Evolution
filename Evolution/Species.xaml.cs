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
        #region private параметры
        double energy;
        bool alife = true;
        #endregion

        #region Свойства класса
        /// <summary>
        /// Запас энергии особи
        /// </summary>
        public double Energy
        {
            get => energy;
            protected set
            {
                energy = value;
            }
        }
        #endregion

        public Species()
        {
            InitializeComponent();
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
        #endregion

        #region Внешние свойства
        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
