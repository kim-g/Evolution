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
        }

        #region Внутренние свойства
        protected Species source;
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
            }
        }
        #endregion

        #region Внутренние методы
        protected void UpdateSource()
        {

        }
        #endregion

        #region Внешние методы

        #endregion
    }
}
