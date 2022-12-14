using System;
using System.Linq;
using System.Windows;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using RootsFinder;

namespace Intagrating
{
    public partial class MainWindow
    {
        private static double dx = 0.001;
        public double LeftLimit => double.Parse(ATextBox.Text);
        private double rightLimit => double.Parse(BTextBox.Text);
        private static int n = int.Parse(new string(dx.ToString().Replace(",", "").ToCharArray().Reverse().ToArray()));
        private FunctionExpression _functionExpression = new FunctionExpression();

        private readonly PlotModel _plotModel = new PlotModel
        {
            // Axes =
            // {
            //     new LinearAxis
            //     {
            //         Position = AxisPosition.Bottom,
            //         MinimumRange = dx*50
            //     }
            // }
        };

        public MainWindow()
        {
            InitializeComponent();
        }

        private double Smth(double a)
        {
            // return 2 * a;
            // return 3 * Math.Tan(a);
            return Math.Cos(a);
        }

        private void SetPlotModel()
        {
            IntagratePlotView.Model = _plotModel;
        }

        private double CentralRectangle(Func<double, double> f, double a, double b, int n)
        {
            var barSeries = new LinearBarSeries
            {
                BarWidth = n * n * n
            };


            var h = (b - a) / n;
            double sum = (f(a) + f(b)) / 2;
            for (var i = 1; i < n; i++)
            {
                var x = a + h * i;
                var y = f(x);
                if (y > 1000 || y < -1000)
                {
                    MessageBox.Show("Значения 'Y' слишком большие");
                    return 0.0;
                }

                sum += Math.Abs(y);
                barSeries.Points.Add(new DataPoint(x, y));
            }

            _plotModel.Series.Add(barSeries);

            SetPlotModel();
            var result = h * sum;
            SBox.Text = result.ToString();
            return result;
        }


        private double TrapeziaMethod(Func<double, double> f, double a, double b, int n)
        {
            double S = 0;
            var h = (b - a) / n;
            var areaSeries = new AreaSeries();

            for (double i = a; i < b; i += dx)
            {
                var x1 = i;
                var y1 = f(x1);
                var x2 = i + dx;
                var y2 = f(x2);

                if (y1 > 1000 || y1 < -1000)
                {
                    MessageBox.Show("Значения 'Y' слишком большие");
                    return 0.0;
                }

                areaSeries.Points.Add(new DataPoint(x1, y1));
                areaSeries.Points.Add(new DataPoint(x2, y2));
                areaSeries.Points.Add(new DataPoint(x2, 0));

                S += Math.Abs((y1 + y2) / 2);
            }

            S *= dx;
            _plotModel.Series.Add(areaSeries);
            SetPlotModel();
            SBox.Text = S.ToString();
            return S;
        }

        private double ParabolMethod(Func<double, double> f, double a, double b, int n)
        {
            double S = 0;

            for (double i = 0; i < b - a; i += dx)
            {
                var x1 = i;
                var y1 = f(x1);

                if (y1 > 1000 || y1 < -1000)
                {
                    MessageBox.Show("Значения 'Y' слишком большие");
                    return 0.0;
                }

                var x2 = i + dx;
                var y2 = f(x2);
                var xMid = (x1 + x2) / 2;
                var yMid = f(xMid);
                S += Math.Abs(dx / 3 * (y1 + 4 * yMid + y2));
            }

            S *= 0.5;
            SBox.Text = S.ToString();

            return S;
        }

        private void VisualizeButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (!_functionExpression.TryLoadFunction(FunctionTextBox.Text))
            {
                MessageBox.Show(
                    "Function expression is not valid!",
                    "Function parsing error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if (LeftLimit > rightLimit)
            {
                MessageBox.Show(
                    "Left limit bigger then rig",
                    "Function parsing error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            
            _functionExpression.CanCalculate = true;

            dx = double.Parse(DXTextBox.Text);
            n = int.Parse(new string(dx.ToString().Replace(",", "").ToCharArray().Reverse().ToArray()));
            _plotModel.Series.Clear();
            _plotModel.Series.Add(new FunctionSeries(_functionExpression.FunctionValue, LeftLimit, rightLimit, 0.001, ""));
            if (Square.IsChecked.Value)
            {
                CentralRectangle(_functionExpression.FunctionValue, LeftLimit, rightLimit, n);
            }

            if (Trapezia.IsChecked.Value)
            {
                TrapeziaMethod(_functionExpression.FunctionValue, LeftLimit, rightLimit, n);
            }

            if (LSM.IsChecked.Value)
            {
                ParabolMethod(_functionExpression.FunctionValue, LeftLimit, rightLimit, n);
            }

            _plotModel.InvalidatePlot(true);
            SetPlotModel();
        }
    }
}