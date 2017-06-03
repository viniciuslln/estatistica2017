using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Estatistica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            //Inicializa área do gráfico
            ChartArea aChartArea = new ChartArea();
            Chart1.ChartAreas.Add(aChartArea);

            var xExemplo = new double[] { 2, 5.8, 6, 9.7, 15, 17.8 };
            var yExemplo = new double[] { 4, 12, 8, 11.9, 15.8, 10 };
            var erroExemplo = new double[] { 5, 5, 5, 5, 5, 5 };

            DesenharGrafico(xExemplo, yExemplo, erroExemplo);
        }

        public void DesenharGrafico(double [] x, double [] y, double [] erro)
        {
            Series errorBarSeries = new Series("ErrorBar")
            {
                ChartType = SeriesChartType.ErrorBar,
                YValuesPerPoint = 3,
                ShadowOffset = 1,
            };
            errorBarSeries["ErrorBarCenterMarkerStyle"] = "Circle";
            errorBarSeries["ErrorBarType"] = "StandardDeviation";
 
            for (int i = 0; i<x.Length; i++)
            {
                errorBarSeries.Points.AddXY(x[i], y[i], y[i] - erro[i], y[i] + erro[i]);
            }
            Chart1.Series.Clear();
            Chart1.Series.Add(errorBarSeries);
        }
    }
}
