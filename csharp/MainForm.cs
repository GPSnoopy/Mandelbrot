using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics.X86;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Mandelbrot
{
    internal partial class MainForm : Form
    {
        public MainForm()
        {
            _set = new MandelbrotSet(1, 1)
            {
                BackEnd = Avx.IsSupported ? BackEnd.Avx : BackEnd.Sse2,
                MaxIterations = 1001,
                OffsetX = -0.75,
                Threads = Environment.ProcessorCount
            };

            InitializeComponent();

            // Icon
            var png = Properties.Resources.Icon;
            Icon = Icon.FromHandle(png.GetHicon());

            // Backend combo-box.
            var items = new[]
            {
                new ComboBoxItem(BackEndLabel.Get(BackEnd.Managed), BackEnd.Managed),
                new ComboBoxItem(BackEndLabel.Get(BackEnd.Sse2), BackEnd.Sse2),
                Avx.IsSupported ? new ComboBoxItem(BackEndLabel.Get(BackEnd.Avx), BackEnd.Avx) : null,
                new ComboBoxItem(BackEndLabel.Get(BackEnd.Cuda), BackEnd.Cuda)
            }.Where(e => e != null).ToArray();

            // ReSharper disable once CoVariantArrayConversion
            _backEndComboBox.Items.AddRange(items);
            _backEndComboBox.SelectedItem = items.Single(e => (BackEnd) e.Value == _set.BackEnd);
            
            // Force size initialisation. Sometimes does not seem to happen otherwise.
            pictureBox_Resize(null, null);

            // Only redraw Mandelbrot set when there is no pending messages.
            Application.Idle += OnIdle;

            RefreshLabels();
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            _set.Dispose();
        }

        private void benchmarkButton_Click(object sender, EventArgs e)
        {
            if (_benchmarkForm == null)
            {
                _benchmarkForm = new BenchmarkForm() {Icon = Icon};
                _benchmarkForm.FormClosed += (s, a) => _benchmarkForm = null;
                _benchmarkForm.Show();
            }
            else
            {
                _benchmarkForm.Focus();
            }
        }

        private void aboutButton_Click(object sender, EventArgs e)
        {
            var about = new AboutBox();
            about.ShowDialog();
        }

        private void pictureBox_Resize(object sender, EventArgs e)
        {
            _set.Resize(_pictureBox.Width, _pictureBox.Height);
            _screenBuffer = new Rgb[_pictureBox.Height, _pictureBox.Width];
            _bitmap = new Bitmap(_pictureBox.Width, _pictureBox.Height, PixelFormat.Format24bppRgb);

            _postRenderOnIdle = true;
        }

        private void pictureBox_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseLeftX = e.X;
                _mouseLeftY = e.Y;
                _mouseLeftPressed = true;
            }

            if (e.Button == MouseButtons.Right)
            {
                SetScreenCoordOffsets(e.X, e.Y);
            }
        }

        private void pictureBox_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                _mouseLeftPressed = false;
            }
        }

        private void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            //_toolStripStatusLabel.Text = e.X + ", " + e.Y;

            if (_mouseLeftPressed)
            {
                var deltaX = _mouseLeftX - e.X;
                var deltaY = _mouseLeftY - e.Y;

                SetScreenCoordOffsets(_set.Width / 2.0 + deltaX, _set.Height / 2.0 + deltaY);

                _mouseLeftX = e.X;
                _mouseLeftY = e.Y;
            }
        }

        private void pictureBox_MouseWheel(object sender, MouseEventArgs e)
        {
            if (e.Delta > 0)
            {
                _set.Zoom *= 1.1;
            }
            else
            {
                _set.Zoom /= 1.1;
            }

            RefreshLabels();

            _postRenderOnIdle = true;
        }

        protected override bool ProcessKeyEventArgs(ref Message m)
        {
            if ((m.Msg == 0x102) || (m.Msg == 0x106)) // this is special - don't remove
            {
                char c = (char) m.WParam;

                var add = c == '+';
                var sub = c == '-';

                if (add)
                {
                    _set.MaxIterations += 1000;
                }

                if (sub)
                {
                    _set.MaxIterations = _set.MaxIterations > 1000 ? _set.MaxIterations - 1000 : 1;
                }

                if (add || sub)
                {
                    RefreshLabels();
                    _postRenderOnIdle = true;
                }
            }

            return base.ProcessKeyEventArgs(ref m);
        }

        private void maxIterationsTextBox_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

            if (char.IsDigit(e.KeyChar) && _maxIterationsTextBox.Text.Length >= 6)
            {
                e.Handled = true;
            }

            if (e.KeyChar == (char) Keys.Return)
            {
                _pictureBox.Focus();
                e.Handled = true;
            }
        }

        private void maxIterationsTextBox_Validated(object sender, EventArgs e)
        {
            if (_maxIterationsTextBox.Text.Length != 0)
            {
                _set.MaxIterations = Math.Max(1, uint.Parse(_maxIterationsTextBox.Text));
                _postRenderOnIdle = true;
            }

            RefreshLabels();
        }

        private void backEndComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            _set.BackEnd = (BackEnd) ((ComboBoxItem)_backEndComboBox.SelectedItem).Value;
            _postRenderOnIdle = true;
        }

        private void doublePrecisionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _set.Precision = _doublePrecisionCheckBox.Checked ? Precision.Double : Precision.Single;
            _postRenderOnIdle = true;
        }

        private void OnIdle(object sender, EventArgs e)
        {
            if (_postRenderOnIdle)
            {
                RefreshMandelbrotSet();
                _postRenderOnIdle = false;
            }
        }

        private void SetScreenCoordOffsets(double x, double y)
        {
            var coord = _set.GetPointCoordinate(x, y);

            _set.OffsetX = coord.X;
            _set.OffsetY = coord.Y;

            RefreshLabels();

            _postRenderOnIdle = true;
        }

        private void RefreshLabels()
        {
            //_offsetXTextBox.Text = _set.OffsetX.ToString();
            //_offsetYTextBox.Text = _set.OffsetY.ToString();
            _zoomTextBox.Text = _set.Zoom.ToString("E");
            _maxIterationsTextBox.Text = _set.MaxIterations.ToString();
            _doublePrecisionCheckBox.Checked = _set.Precision == Precision.Double;
        }

        private void RefreshMandelbrotSet()
        {
            var timer = Stopwatch.StartNew();

            _set.ComputeSet();

            _timeToRender = timer.Elapsed.TotalSeconds;
            timer.Restart();

            ConvertMandelbrotColor();

            _timeToColor = timer.Elapsed.TotalSeconds;
            timer.Restart();

            SwapBuffer();
            _timeToSwap = timer.Elapsed.TotalSeconds;

            _toolStripStatusLabel.Text = $"R: {_timeToRender}, C: {_timeToColor}, S: {_timeToSwap} [{_set.Width} x {_set.Height}]";
        }

        private void ConvertMandelbrotColor()
        {
            var iterations = _set.Iterations;

            Parallel.For(0, _set.Height, i =>
            {
                for (int j = 0; j != _set.Width; ++j)
                {
                    const int maxColor = 255;

                    var value = iterations[i, j];
                    var color = new Rgb();

                    //
                    // Black -> Red
                    color.Red += (byte)Math.Min(value, maxColor);
                    value -= Math.Min(value, maxColor);

                    // Red -> Yellow
                    color.Green += (byte)Math.Min(value, maxColor);
                    value -= Math.Min(value, maxColor);

                    // Yellow -> Green
                    color.Red -= (byte)Math.Min(value, maxColor);
                    value -= Math.Min(value, maxColor);

                    // Green -> Blue/Green
                    color.Blue += (byte)Math.Min(value, maxColor);
                    value -= Math.Min(value, maxColor);

                    // Blue/Green -> Blue
                    color.Green -= (byte)Math.Min(value, maxColor);
                    value -= Math.Min(value, maxColor);

                    // Blue -> Violet
                    color.Red += (byte)Math.Min(value, maxColor);
                    value -= Math.Min(value, maxColor);

                    while (value > 0)
                    {
                        //
                        // Violet -> Red
                        color.Blue -= (byte)Math.Min(value, maxColor);
                        value -= Math.Min(value, maxColor);

                        // Red -> Yellow
                        color.Green += (byte)Math.Min(value, maxColor);
                        value -= Math.Min(value, maxColor);

                        // Yellow -> Green
                        color.Red -= (byte)Math.Min(value, maxColor);
                        value -= Math.Min(value, maxColor);

                        // Green -> Blue/Green
                        color.Blue += (byte)Math.Min(value, maxColor);
                        value -= Math.Min(value, maxColor);

                        // Blue/Green -> Blue
                        color.Green -= (byte)Math.Min(value, maxColor);
                        value -= Math.Min(value, maxColor);

                        // Blue -> Violet
                        color.Red += (byte)Math.Min(value, maxColor);
                        value -= Math.Min(value, maxColor);
                    }

                    _screenBuffer[i, j] = color;
                }
            });
        }

        private unsafe void SwapBuffer()
        {
            var data = _bitmap.LockBits(new Rectangle(0, 0, _bitmap.Width, _bitmap.Height), ImageLockMode.ReadWrite, _bitmap.PixelFormat);
            var dst = (byte*) data.Scan0.ToPointer();

            fixed (Rgb* src = _screenBuffer)
            {
                for (int i = 0; i != _bitmap.Height; ++i)
                {
                    Memory.Copy(dst + i * data.Stride, src + i * _bitmap.Width, _bitmap.Width * 3);
                }
            }

            _bitmap.UnlockBits(data);
            _pictureBox.Image = _bitmap;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct Rgb
        {
            public byte Blue;
            public byte Green;
            public byte Red;
        }

        private readonly MandelbrotSet _set;
        private Rgb[,] _screenBuffer;
        private Bitmap _bitmap;

        private bool _postRenderOnIdle;
        private double _timeToRender;
        private double _timeToColor;
        private double _timeToSwap;

        private int _mouseLeftX;
        private int _mouseLeftY;
        private bool _mouseLeftPressed;

        private BenchmarkForm _benchmarkForm;
    }
}
