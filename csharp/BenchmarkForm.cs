using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Intrinsics.X86;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace Mandelbrot
{
    internal partial class BenchmarkForm : Form
    {
        public BenchmarkForm()
        {
            _set = new MandelbrotSet(1280, 960)
            {
                MaxIterations = 10000,
                OffsetX = -0.75,
                OffsetY = 0.0,
                Zoom = 4.0
            };

            _backEnds = new[]
            {
                BackEnd.Managed,
                BackEnd.Sse2,
                Avx.IsSupported ? BackEnd.Avx : (BackEnd?) null,
                BackEnd.Cuda
            }.Where(e => e.HasValue).Select(e => e.Value).ToArray();

            _maxThreads = Environment.ProcessorCount;
            _series = _backEnds.SelectMany(b => new[] { new Series(BackEndLabel.Get(b) + " 64"), new Series(BackEndLabel.Get(b) + " 32") }).ToArray();
            _lock = new object();
            _thread = new Thread(RunBenchmark) { Name = "Benchmark" };

            InitializeComponent();
            InitializeSeries();
            InitializeDataGrid();

            _chartMenuStrip.Enabled = false;
            _closeButton.Enabled = false;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _set.Dispose();
                components?.Dispose();
            }

            base.Dispose(disposing);
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            lock (_lock)
            {
                _cancel = true;
                _set.AbortComputeSet();
            }

            _thread.Join();
            _set.ResetAbort();
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            _dataGridView.ClearSelection();
            _thread.Start();
        }

        private void InitializeSeries()
        {
            _chart.Series.Clear();

            foreach (var series in _series)
            {
                _chart.Series.Add(series);
            }

            _chart.ApplyPaletteColors();

            var colors = _chart.Series.Select(s => s.Color).ToArray();

            for (int i = 0; i != _series.Length; i += 2)
            {
                var c = colors[i / 2];

                _series[i + 0].Color = c;
                _series[i + 1].Color = Color.FromArgb(128, c);
            }
        }

        private void InitializeDataGrid()
        {
            for (int i = 1; i <= _maxThreads; ++i)
            {
                var c = _dataGridView.Columns.Add(i.ToString(), i + " threads");
                var col = _dataGridView.Columns[c];

                col.HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
            }

            for (int i = 0; i != _series.Length; ++i)
            {
                var r = _dataGridView.Rows.Add();
                var row = _dataGridView.Rows[r];

                row.HeaderCell.Value = _series[i].Name;
                row.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;
            }

            var height = _dataGridView.ColumnHeadersHeight;

            foreach (DataGridViewRow row in _dataGridView.Rows)
            {
                height += row.Height;
            }

            _dataGridView.Height = height;
            _dataGridView.RowPrePaint += dataGridView_RowPrePaint;
        }

        private void RunBenchmark()
        {
            for (int b = 0; b != _backEnds.Length; ++b)
            {
                for (int t = 1; t <= _maxThreads; ++t)
                {
                    RunBenchmark(_backEnds[b], t, 2*b + 0);
                    RunBenchmark(_backEnds[b], t, 2*b + 1);
                }
            }

            lock (_lock)
            {
                if (!_cancel)
                {
                    Invoke((MethodInvoker)(() => 
                    {
                        _chartMenuStrip.Enabled = true;
                        _closeButton.Enabled = true;
                    }));
                }
            }
        }

        private void RunBenchmark(BackEnd backEnd, int threads, int seriesIndex)
        {
            lock (_lock)
            {
                if (_cancel)
                {
                    return;
                }
            }

            _set.BackEnd = backEnd;
            _set.Precision = seriesIndex % 2 == 0 ? Precision.Double : Precision.Single;
            _set.Threads = threads;

            var timer = Stopwatch.StartNew();
            _set.ComputeSet();
            var elapsed = timer.Elapsed;

            lock (_lock)
            {
                if (!_cancel)
                {
                    Invoke((MethodInvoker)(() =>
                    {
                        var point = new DataPoint(threads, elapsed.TotalMilliseconds)
                        {
                            ToolTip = $"{elapsed.TotalMilliseconds:F0} milliseconds"
                        };

                        _series[seriesIndex].Points.Add(point);
                        _dataGridView.Rows[seriesIndex].Cells[threads - 1].Value = elapsed.TotalMilliseconds.ToString("F0");
                    }));
                }
            }
        }

        private void closeButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        void dataGridView_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            // Help removing indicator/pointer in row header of datagridview
            // https://social.msdn.microsoft.com/Forums/windows/en-US/346e5839-1813-472b-8b3a-7344118819b3/help-removing-indicatorpointer-in-row-header-of-datagridview?forum=winformsdatacontrols

            e.PaintCells(e.ClipBounds, DataGridViewPaintParts.All);
            e.PaintHeader(
                DataGridViewPaintParts.Background |
                DataGridViewPaintParts.Border |
                DataGridViewPaintParts.Focus |
                DataGridViewPaintParts.SelectionBackground |
                DataGridViewPaintParts.ContentForeground);

            e.Handled = true;
        }

        private void saveChartMenuItem_Click(object sender, EventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = ".png",
                Filter = "PNG file (*.png)|*.png|All files (*.*)|*.*",
                FilterIndex = 1,
                OverwritePrompt = true,
                RestoreDirectory = true,
            };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    SaveChartAsHighResolution(saveFileDialog.FileName);
                    SaveCsvFile(Path.ChangeExtension(saveFileDialog.FileName, ".csv"));
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.ToString(), $"ERROR: {exception.Message}", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void SaveChartAsHighResolution(string fileName)
        {
            // Duplicate high resolution chart
            var chart = new Chart()
            {
                Visible = false,
                Width = 2560,
                Height = 1600,
                Palette = _chart.Palette
            };

            var chartArea = new ChartArea();
            chart.ChartAreas.Add(chartArea);


            // Clone the series
            foreach (var series in _series)
            {
                var newSeries = new Series(series.Name)
                {
                    Color = series.Color
                };

                chart.Series.Add(newSeries);

                foreach (var point in series.Points)
                {
                    newSeries.Points.Add(point);
                }
            }

            // Scale the fonts.
            const float fontScale = 3;

            var legend = new Legend()
            {
                Font = new Font(_chart.Legends[0].Font.OriginalFontName, _chart.Legends[0].Font.Size * fontScale)
            };

            chart.Legends.Add(legend);

            chart.ChartAreas[0].AxisX.LabelStyle.Font = new Font(chartArea.AxisX.LabelStyle.Font.OriginalFontName, chartArea.AxisX.LabelStyle.Font.Size * fontScale);
            chart.ChartAreas[0].AxisX.Title = "Threads";
            chart.ChartAreas[0].AxisX.TitleFont = new Font(chartArea.AxisX.TitleFont.OriginalFontName, chartArea.AxisX.TitleFont.Size * fontScale);
            chart.ChartAreas[0].AxisY.LabelStyle.Font = new Font(chartArea.AxisY.LabelStyle.Font.OriginalFontName, chartArea.AxisY.LabelStyle.Font.Size * fontScale);
            chart.ChartAreas[0].AxisY.Title = "Milliseconds";
            chart.ChartAreas[0].AxisY.TitleFont = new Font(chartArea.AxisY.TitleFont.OriginalFontName, chartArea.AxisY.TitleFont.Size * fontScale);

            chart.SaveImage(fileName, ChartImageFormat.Png);
        }

        private void SaveCsvFile(string fileName)
        {
            using (var writer = new StreamWriter(fileName))
            {
                writer.Write(",");
                writer.WriteLine(string.Join(",", _dataGridView.Columns.Cast<DataGridViewColumn>().Select(c => '"' + c.HeaderText + '"')));

                foreach (DataGridViewRow row in _dataGridView.Rows)
                {
                    writer.Write('"' + (string) row.HeaderCell.Value + '"' + ',');
                    writer.WriteLine(string.Join(",", row.Cells.Cast<DataGridViewCell>().Select(c => c.Value)));
                }

                // TODO Save CPUID and CUDA info (see AboutBox.cs)
            }
        }

        private readonly object _lock;
        private readonly Thread _thread;
        private bool _cancel;

        private readonly MandelbrotSet _set;
        private readonly BackEnd[] _backEnds;
        private readonly Series[] _series;
        private readonly int _maxThreads;
    }
}
