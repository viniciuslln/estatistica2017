using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.DataVisualization.Charting;
using System.Windows.Input;

namespace Estatistica
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private double _nextGauss;
        private bool _available;
        private Random _rng = new Random();

        int tamPopulacao = 1000;
        int media = 12;
        int variancia = 9;
        double erro;

        double[] populacao;
        List<double[]> amostras;

        public MainWindow()
        {
            InitializeComponent();

            //Inicializa área do gráfico
            ChartArea aChartArea = new ChartArea("Intervalos");
            Chart1.ChartAreas.Add(aChartArea);

            //Inicializando população
            erro = 1.96 * (Math.Sqrt(variancia) / Math.Sqrt(25));
            gerarPopulacao();
            coletarAmostras();
            gerarGrafico();


        }
        public double RandomGauss(double media, double desvioPadrao)
        {
            if (_available)
            {
                _available = false;
                return media + desvioPadrao * _nextGauss;
            }

            double u1 = _rng.NextDouble();
            double u2 = _rng.NextDouble();
            double temp1 = Math.Sqrt(-2.0 * Math.Log(u1));
            double temp2 = 2.0 * Math.PI * u2;

            _nextGauss = temp1 * Math.Sin(temp2);
            _available = true;
            var toReturn = temp1 * Math.Cos(temp2);
            return media + desvioPadrao * toReturn;
        }

        private void coletarAmostras()
        {
            amostras = new List<double[]>();
            Random random = new Random();
            for (int i = 0; i < 20; i++)
            {
                HashSet<int> randomNumbers = new HashSet<int>();
                for (int j = 0; j < 25; j++)
                    while (!randomNumbers.Add(random.Next(0,999))) ;
                var amostra = new double[25];
                var numeros = randomNumbers.ToList();
                for (int j = 0; j < 25; j++)
                    amostra[j] = populacao[numeros[j]];
                amostras.Add(amostra);
                var item = new TreeViewItem()
                {
                    Header = "Amostra " + (i + 1),
                };
                item.PreviewMouseLeftButtonDown += clickLista;
                item.Items.Add(new ListView()
                {
                    ItemsSource = amostra
                });
                TreeViewAmostras.Items.Add(item);
            }
        }

        private void clickLista(object sender, MouseButtonEventArgs e)
        {
            int i = TreeViewAmostras.Items.IndexOf(sender as TreeViewItem);
            var amostra = amostras[i];

            var mediaAmostral = calculaMedia(amostra);

            double erroSuperior = mediaAmostral + erro;
            double erroInferior = mediaAmostral - erro;

            StackPanelInformacoesAmostra.Children.Clear();
            StackPanelInformacoesAmostra.Children.Add(new TextBlock()
            {
                Text = "Média da Amostra: " + mediaAmostral
            });
            StackPanelInformacoesAmostra.Children.Add(new TextBlock()
            {
                Text = "Erro Superior: " + erroSuperior
            });
            StackPanelInformacoesAmostra.Children.Add(new TextBlock()
            {
                Text = "Erro ingerior: " + erroInferior
            });
        }

        public void gerarPopulacao()
        {
            populacao = new double[tamPopulacao];
            for (int i = 0; i < tamPopulacao; i++)
            {
                populacao[i] = RandomGauss(media, Math.Sqrt(variancia));
            }
            ListViewPopulação.ItemsSource = populacao;
            var mediaCalculada = calculaMedia(populacao);
            StackPanelInformacoes.Children.Add(new TextBlock()
            {
                Text = "Média da População: " + mediaCalculada
            });
        }

        public void gerarGrafico()
        {
            Series errorBarSeries = new Series("ErrorBar")
            {
                ChartType = SeriesChartType.ErrorBar,
                YValuesPerPoint = 3,
                ShadowOffset = 1,
            };
            errorBarSeries["ErrorBarCenterMarkerStyle"] = "Circle";
           // errorBarSeries["ErrorBarType"] = "StandardDeviation";


            for (int x = 1; x <= amostras.Count; x++)
            {
                var amostra = amostras[x-1];
                
                var mediaAmostral = calculaMedia(amostra);

                double erroSuperior = mediaAmostral + erro;
                double erroInferior = mediaAmostral - erro;
                
                errorBarSeries.Points.AddXY(x, mediaAmostral, erroInferior, erroSuperior);

            }

            Chart1.Series.Clear();
            Chart1.Series.Add(errorBarSeries);

            StripLine stripline = new StripLine()
            {
                Interval = 0,
                IntervalOffset = 12,
                StripWidth = 0.1,
                BackColor = System.Drawing.Color.Purple,
                BorderDashStyle = ChartDashStyle.Dot,
                Text = "μ"
            };
            Chart1.ChartAreas["Intervalos"].AxisY.StripLines.Add(stripline);

        }

        double calculaMedia(double[] l)
        {
            var media = 0.0;
            foreach (var x in l)
                media += x;
            return media / l.Length;
        }

    }
}
