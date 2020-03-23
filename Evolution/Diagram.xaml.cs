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
        public List<double> Line_1_Source { get; set; }
        public List<double> Line_2_Source { get; set; }
        public List<double> Line_3_Source { get; set; }
        public List<double> Line_4_Source { get; set; }
        #endregion

        #region Внутренние методы

        /// <summary>
        /// Формирует 2D график по заданным параметрам
        /// </summary>
        /// <param name="Line"></param>
        /// <param name="AW"></param>
        /// <param name="MaxValue"></param>
        /// <returns></returns>
        protected PointCollection GetLineData(List<double> Line, int AW, double MaxValue)
        {
            List<Point> NewItems = new List<Point>();
            if (Line.Count <= ActualWidth)
                for (int i = 0; i < Line.Count; i++)
                    NewItems.Add(new Point(i, ActualHeight - Line[i] / MaxValue * ActualHeight));
            else
                for (int i = 0; i < ActualWidth; i++)
                {
                    int j = Line.Count - AW + i;
                    j = j < Line.Count ? j : Line.Count - 1;
                    j = j >= 0 ? j : 0;
                    NewItems.Add(new Point(i, ActualHeight - Line[j] / MaxValue * ActualHeight));
                }
                    
            return new PointCollection(NewItems);
        }

        #endregion

        #region Внешние методы
        /// <summary>
        /// Обновляет график
        /// </summary>
        public void Update()
        {
            double MaxValue = new double[] { Line_1_Source.Max(),
                                             Line_2_Source.Max(),
                                             Line_3_Source.Max(),
                                             Line_4_Source.Max(),}.Max();
            int AW = Convert.ToInt32(ActualWidth);
            Line1.Points = GetLineData(Line_1_Source, AW, MaxValue);
            Line2.Points = GetLineData(Line_2_Source, AW, MaxValue);
            Line3.Points = GetLineData(Line_3_Source, AW, MaxValue);
            Line4.Points = GetLineData(Line_4_Source, AW, MaxValue);
        }
        #endregion
    }
}
