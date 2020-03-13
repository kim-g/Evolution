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
    /// Логика взаимодействия для Diagram.xaml
    /// </summary>
    public partial class Diagram : UserControl
    {
        public Diagram()
        {
            InitializeComponent();
        }

        #region Внутренние свойства

        #endregion

        #region Внешние свойства
        public List<double> Source { get; set; }
        #endregion

        #region Внутренние методы

        #endregion

        #region Внешние методы
        public void Update()
        {
            List<Point> NewItems = new List<Point>();
            double MaxValue = Source.Max();
            int AW = Convert.ToInt32(ActualWidth);
            if (Source.Count <= ActualWidth)
                for (int i = 0; i < Source.Count; i++)
                    NewItems.Add(new Point(i, ActualHeight - Source[i] / MaxValue * ActualHeight));
            else
                for (int i = 0; i < ActualWidth; i++)
                    NewItems.Add(new Point(i, ActualHeight - Source[Source.Count - AW + i] / MaxValue * ActualHeight));
            Line1.Points = new PointCollection(NewItems);
        }
        #endregion
    }
}
