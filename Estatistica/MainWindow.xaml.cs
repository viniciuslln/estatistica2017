using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private double proxGauss; // Z2 (será o próximo valor calculado por Z2=sqrt(-2 log u1)sin(2 pi u2) )
        private bool disponivel; // Boolean que determina se proximo random pegará Z1 ou Z2
        private Random rng = new Random(); // Instância da classe Random principal (que gerará numeros da população)

        int tamPopulacao = 1000; // Tamanho da população
        int media = 12; // Média dos elementos da pupulação
        int variancia = 9; // Variância da população σ^2
        int quantAmostra = 20; // Quantidade de amostras
        int tamAmostra = 25; // Tamanho da amostra
        double erro; // Erro padão da amostra

        double[] populacao; // População
        List<double[]> amostras; // Lista com as Amostras

        public MainWindow()
        {
            InitializeComponent();
            
            inicializar();

        }
        private void inicializar()
        {
            // Calcula erro com base de 95% de confiança
            // 1,96 é valor de ... de acordo com z = 0,tamAmostra segundo tabela normal 
            erro = 1.96 * (Math.Sqrt(variancia) / Math.Sqrt(tamAmostra)); 
            // Inicializando população
            gerarPopulacao();
            // Coleta Amostras
            coletarAmostras();
            // Gera o Grafico
            gerarGrafico();
        }

        /// <summary>
        /// Gera números aleatórios segundo expressao de Box-Muller
        /// Z1 = sqrt(-2 log u1)cos(2 pi u2) )
        /// Z2 = sqrt(-2 log u1)sin(2 pi u2) )
        /// u1 e u2 são numeros aleatórios [0,1]
        /// a cada chamada do método será retornado primeiro Z1 e depois Z2
        /// <summary>
        /// <param name="media"> Média populacional</param>
        /// <param name="desvioPadrao"> Desvio padrão populacional</param>
        public double RandomGauss(double media, double desvioPadrao)
        {
            if (disponivel)
            {
                disponivel = false;
                return media + desvioPadrao * proxGauss;
            }

            double u1 = rng.NextDouble();
            double u2 = rng.NextDouble();
            double temp1 = Math.Sqrt(-2.0 * Math.Log(u1));
            double temp2 = 2.0 * Math.PI * u2;

            proxGauss = temp1 * Math.Sin(temp2);
            disponivel = true;
            var toReturn = temp1 * Math.Cos(temp2);
            return media + desvioPadrao * toReturn;
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

            //------------------------------------------
            // Atualiza interface
            StackPanelInformacoes.Children.Add(new TextBlock()
            {
                Text = "Média da População: " + mediaCalculada
            });
        }
        /// <summary>
        /// Gera 20 amostras aleatórias de tamanho tamAmostra com elementos da população
        /// <summary>
        private void coletarAmostras()
        {
            amostras = new List<double[]>();
            Random random = new Random();
            for (int i = 0; i < quantAmostra; i++)
            {
                HashSet<int> randomNumbers = new HashSet<int>();
                for (int j = 0; j < tamAmostra; j++)
                    while (!randomNumbers.Add(random.Next(0,999))) ;
                var amostra = new double[tamAmostra];
                var numeros = randomNumbers.ToList();
                for (int j = 0; j < tamAmostra; j++)
                    amostra[j] = populacao[numeros[j]];
                amostras.Add(amostra);
                
                //------------------------------------------
                // Atualiza interface
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
        public void gerarGrafico()
        {
            double porcentagemDeAmostrasNaMedia = 0.0;
            //Inicializa área do gráfico
            Chart1.ChartAreas.Clear();
            ChartArea aChartArea = new ChartArea("Intervalos");
            Chart1.ChartAreas.Add(aChartArea);

            // Serie das amostras 
            Series errorBarSeries = new Series("ErrorBar")
            {
                ChartType = SeriesChartType.ErrorBar,
                YValuesPerPoint = 3,
                ShadowOffset = 1,
            };
            errorBarSeries["ErrorBarCenterMarkerStyle"] = "Circle";

            // Para cada amostra, gera uma barra 
            // Calcula a média da amostra e seu intervalo de confiança
            for (int x = 1; x <= amostras.Count; x++)
            {
                var amostra = amostras[x-1];
                
                var mediaAmostral = calculaMedia(amostra);

                double erroSuperior = mediaAmostral + erro;
                double erroInferior = mediaAmostral - erro;
                
                errorBarSeries.Points.AddXY(x, mediaAmostral, erroInferior, erroSuperior);

                if (media >= erroInferior && media <= erroSuperior)
                    porcentagemDeAmostrasNaMedia++;
            }

            StackPanelInformacoesAmostra.Children.Add(new TextBlock()
            {
                Text = "Quantidade de intervalos que contem a média: " + porcentagemDeAmostrasNaMedia
            });
            StackPanelInformacoesAmostra.Children.Add(new TextBlock()
            {
                Text = "Porcentagem de intervalos que contem a média: " + (porcentagemDeAmostrasNaMedia / quantAmostra) * 100 +"%"
            });

            // Linha da média populacional
            StripLine stripline = new StripLine()
            {
                Interval = 0,
                IntervalOffset = media,
                StripWidth = 0.1,
                BackColor = System.Drawing.Color.Purple,
                BorderDashStyle = ChartDashStyle.Dot,
                Text = "μ"
            };
            
            Chart1.Series.Clear();
            Chart1.Series.Add(errorBarSeries);
            Chart1.ChartAreas["Intervalos"].AxisY.StripLines.Add(stripline);

        }
        
        /// <summary>
        /// Calcula média dos elementos de um array de double
        /// <summary>
        /// <param name="l"> Array de entrada </param>
        double calculaMedia(double[] l)
        {
            var media = 0.0;
            foreach (var x in l)
                media += x;
            return media / l.Length;
        }

        private void ButtonClickRecarregar(object sender, RoutedEventArgs e)
        {
            StackPanelInformacoes.Children.Clear();
            StackPanelInformacoesAmostra.Children.Clear();
            ListViewPopulação.ItemsSource = null;
            TreeViewAmostras.Items.Clear();
            tamPopulacao = Int32.Parse(TextboxTamPopulacao.Text);
            media = Int32.Parse(TextboxMedia.Text);
            variancia = Int32.Parse(TextboxVariancia.Text);
            inicializar();
        }

        private void clickLista(object sender, MouseButtonEventArgs e)
        {
            int i = TreeViewAmostras.Items.IndexOf(sender as TreeViewItem);
            var amostra = amostras[i];

            var mediaAmostral = calculaMedia(amostra);

            double erroSuperior = mediaAmostral + erro;
            double erroInferior = mediaAmostral - erro;

            //------------------------------------------
            // Atualiza interface
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

        //para campos aceitarem somente numeros
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }
    }
}
