using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;

namespace Mandelbrot
{
    internal sealed class MandelbrotSet : IDisposable
    {
        public MandelbrotSet(int width, int height)
        {
            _lock = new object();
            _cancel = new bool[1];
            //_cuda = new MandelbrotSetAlea();
            _exceptions = new List<Exception>();

            BackEnd = BackEnd.Managed;
            Precision = Precision.Double;
            MaxIterations = 100;
            OffsetX = 0;
            OffsetY = 0;
            Threads = 1;
            Zoom = 1;

            Resize(width, height);
        }

        public void Dispose()
        {
            //_cuda?.Dispose();
        }

        public BackEnd BackEnd { get; set; }
        public Precision Precision { get; set; }
        public uint MaxIterations { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double Zoom { get; set; }

        public uint[,] Iterations => _matrix;
        public int Width => _width;
        public int Height => _height;

        public int Threads
        {
            get => _threads;
            set { if (value < 1) throw new ArgumentException("threads cannot be less than one"); _threads = value; }
        }
        
        public void Resize(int width, int height)
        {
            if (width < 0) throw new ArgumentException("width cannot be negative");
            if (height < 0) throw new ArgumentException("height cannot be negative");

            _matrix = new uint[height, width];
            _width = width;
            _height = height;
        }

        public void ComputeSet()
        {
            lock (_lock)
            {
                if (_workerThreads != null) throw new InvalidOperationException("ComputeSet already executing");

                _workerThreads = new Thread[BackEnd == BackEnd.Cuda ? 1 : Threads];

                for (int i = 0; i != _workerThreads.Length; ++i)
                {
                    var startScanline = i;

                    _workerThreads[i] = new Thread(() => ComputeSet(startScanline, _workerThreads.Length)) { Name = "MandelbrotSet" };
                    _workerThreads[i].Start();
                }
            }

            foreach (var thread in _workerThreads)
            {
                thread.Join();
            }

            lock (_lock)
            {
                var exceptions = _exceptions.ToArray();

                _exceptions.Clear();
                _workerThreads = null;

                if (exceptions.Any())
                {
                    throw new AggregateException(exceptions);
                }
            }
        }

        public void AbortComputeSet()
        {
            lock (_lock)
            {
                _cancel[0] = true;
            }
        }

        public void ResetAbort()
        {
            lock (_lock)
            {
                _cancel[0] = false;
            }
        }
        
        public Coord2D GetPointCoordinate(double screenX, double screenY)
        {
            double coordX =
                -(Width / (Height * Zoom)) + OffsetX
                + ((screenX * 2) / (Height * Zoom));

            double coordY =
                -1.0 / Zoom + OffsetY
                + ((screenY * 2) / (Height * Zoom));

	        return  new Coord2D(coordX, coordY);
        }

        private void ComputeSet(int startScanline, int increment)
        {
            try
            {
                switch (BackEnd)
                {
                    case BackEnd.Managed:
                        if (Precision == Precision.Single)
                            ComputeSetManagedSingle(startScanline, increment);
                        else
                            ComputeSetManagedDouble(startScanline, increment);
                        break;

                    case BackEnd.Sse2:
                        ComputeSetSse2(startScanline, increment);
                        break;

                    case BackEnd.Avx:
                        ComputeSetAvx(startScanline, increment);
                        break;

                    case BackEnd.Cuda:
                        ComputeSetCuda(startScanline, increment);
                        break;
                }
            }

            catch (Exception exception)
            {
                lock (_lock)
                {
                    _exceptions.Add(exception);
                }
            }
        }

        private void ComputeSetManagedSingle(int startScanline, int increment)
        {
            for (int i = startScanline; i < Height && !_cancel[0]; i += increment)
            {
                for (int j = 0; j < Width && !_cancel[0]; ++j)
                {
                    const double maxSquaredMagnitude = 4;

                    var coord = GetPointCoordinate(j, i);
                    var x = (float) coord.X;
                    var y = (float) coord.Y;
                    
                    _matrix[i, j] = 0;

                    //    C = x + y.j
                    //   Z0 = 0
                    // Zn+1 = (Zn^2) + C
                    //
                    // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)

                    float zr = x;
                    float zi = y;

                    for (uint it = 1; it <= MaxIterations; ++it)
                    {
                        float zr2 = zr * zr;
                        float zi2 = zi * zi;
                        float squaredMagnitude = (zr2 + zi2);

                        if (squaredMagnitude > maxSquaredMagnitude)
                        {
                            _matrix[i, j] = it;
                            break;
                        }

                        float tr = zr2 - zi2 + x;

                        zi = 2 * zr * zi + y;
                        zr = tr;
                    }
                }
            }
        }

        private void ComputeSetManagedDouble(int startScanline, int increment)
        {
            for (int i = startScanline; i < Height && !_cancel[0]; i += increment)
            {
                for (int j = 0; j < Width && !_cancel[0]; ++j)
                {
                    const double maxSquaredMagnitude = 4;

                    var coord = GetPointCoordinate(j, i);
                    var x = coord.X;
                    var y = coord.Y;

                    _matrix[i, j] = 0;

                    //    C = x + y.j
                    //   Z0 = 0
                    // Zn+1 = (Zn^2) + C
                    //
                    // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)

                    double zr = x;
                    double zi = y;

                    for (uint it = 1; it <= MaxIterations; ++it)
                    {
                        double zr2 = zr * zr;
                        double zi2 = zi * zi;
                        double squaredMagnitude = (zr2 + zi2);

                        if (squaredMagnitude > maxSquaredMagnitude)
                        {
                            _matrix[i, j] = it;
                            break;
                        }

                        double tr = zr2 - zi2 + x;

                        zi = 2 * zr * zi + y;
                        zr = tr;
                    }
                }
            }
        }

        private unsafe void ComputeSetSse2(int startScanline, int increment)
        {
            fixed (uint* ptr = _matrix)
            fixed (bool* cancel = _cancel)
            {
                //if (Precision == Precision.Single)
                //    ComputeMandelbrotSetSingleSSE2(ptr, Width, Height, OffsetX, OffsetY, Zoom, MaxIterations, startScanline, increment, cancel);
                //else
                //    ComputeMandelbrotSetDoubleSSE2(ptr, Width, Height, OffsetX, OffsetY, Zoom, MaxIterations, startScanline, increment, cancel);
            }
        }

        private unsafe void ComputeSetAvx(int startScanline, int increment)
        {
            fixed (uint* ptr = _matrix)
            fixed (bool* cancel = _cancel)
            {
            //    if (Precision == Precision.Single)
            //        ComputeMandelbrotSetSingleAVX(ptr, Width, Height, OffsetX, OffsetY, Zoom, MaxIterations, startScanline, increment, cancel);
            //    else
            //        ComputeMandelbrotSetDoubleAVX(ptr, Width, Height, OffsetX, OffsetY, Zoom, MaxIterations, startScanline, increment, cancel);
            }
        }

        private unsafe void ComputeSetCuda(int startScanline, int increment)
        {
            if (startScanline != 0) throw new ArgumentException("startScanline must be equal to zero");
            if (increment != 1) throw new ArgumentException("increment must be equal to one");

            fixed (uint* ptr = _matrix)
            {
                //_cuda.ComputeMandelbrotSet(ptr, Width, Height, OffsetX, OffsetY, Zoom, MaxIterations, startScanline, increment, Precision == Precision.Double, ref _cancel[0]);
            }
        }

        //[DllImport("MandelbrotCpp.dll")]
        //private static extern unsafe void ComputeMandelbrotSetSingleSSE2(
        //    uint* iterations,
        //    int width, int height,
        //    double offsetX, double offsetY, double zoom,
        //    uint maxIterations,
        //    int startScanline, int increment,
        //    bool* cancel);

        //[DllImport("MandelbrotCpp.dll")]
        //private static extern unsafe void ComputeMandelbrotSetDoubleSSE2(
        //    uint* iterations,
        //    int width, int height,
        //    double offsetX, double offsetY, double zoom,
        //    uint maxIterations,
        //    int startScanline, int increment,
        //    bool* cancel);

        //[DllImport("MandelbrotAvx.dll")]
        //private static extern unsafe void ComputeMandelbrotSetSingleAVX(
        //    uint* iterations,
        //    int width, int height,
        //    double offsetX, double offsetY, double zoom,
        //    uint maxIterations,
        //    int startScanline, int increment,
        //    bool* cancel);

        //[DllImport("MandelbrotAvx.dll")]
        //private static extern unsafe void ComputeMandelbrotSetDoubleAVX(
        //    uint* iterations,
        //    int width, int height,
        //    double offsetX, double offsetY, double zoom,
        //    uint maxIterations,
        //    int startScanline, int increment,
        //    bool* cancel);

        private readonly object _lock;
        private readonly bool[] _cancel;
        //private readonly MandelbrotSetAlea _cuda;
        private readonly List<Exception> _exceptions;
        private Thread[] _workerThreads;
        private uint[,] _matrix;
        private int _width;
        private int _height;
        private int _threads;
    }
}
