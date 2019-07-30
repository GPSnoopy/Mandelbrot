using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Mandelbrot
{
    internal sealed class MandelbrotSet : IDisposable
    {
        public MandelbrotSet(int width, int height)
        {
            _lock = new object();
            _cancel = new bool[1];
            _cuda = new ImplCuda();
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
            _cuda?.Dispose();
        }

        public BackEnd BackEnd { get; set; }
        public Precision Precision { get; set; }
        public uint MaxIterations { get; set; }
        public double OffsetX { get; set; }
        public double OffsetY { get; set; }
        public double Zoom { get; set; }

        public uint[,] Iterations { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public int Threads
        {
            get => _threads;
            set { if (value < 1) throw new ArgumentException("threads cannot be less than one"); _threads = value; }
        }
        
        public void Resize(int width, int height)
        {
            if (width < 0) throw new ArgumentException("width cannot be negative");
            if (height < 0) throw new ArgumentException("height cannot be negative");

            Iterations = new uint[height, width];
            Width = width;
            Height = height;
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
            return Impl.GetPointCoordinate(screenX, screenY, Width, Height, OffsetX, OffsetY, Zoom);
        }

        private void ComputeSet(int startScanline, int increment)
        {
            try
            {
                switch (BackEnd)
                {
                    case BackEnd.Managed:
                        ComputeSetManaged(startScanline, increment);
                        break;

                    case BackEnd.Sse2:
                        ComputeSetSse2(startScanline, increment);
                        break;

                    case BackEnd.Avx:
                        ComputeSetAvx(startScanline, increment);
                        break;

                    case BackEnd.Cuda:
                        ComputeSetCuda();
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

        private void ComputeSetManaged(int startScanline, int increment)
        {
            if (Precision == Precision.Single)
                Impl.ComputeSingle(Iterations, startScanline, increment, OffsetX, OffsetY, Zoom, MaxIterations, ref _cancel[0]);
            else
                Impl.ComputeDouble(Iterations, startScanline, increment, OffsetX, OffsetY, Zoom, MaxIterations, ref _cancel[0]);
        }

        private void ComputeSetSse2(int startScanline, int increment)
        {
            if (Precision == Precision.Single)
                ImplSse.ComputeSingle(Iterations, startScanline, increment, OffsetX, OffsetY, Zoom, MaxIterations, ref _cancel[0]);
            else
                ImplSse.ComputeDouble(Iterations, startScanline, increment, OffsetX, OffsetY, Zoom, MaxIterations, ref _cancel[0]);
        }

        private void ComputeSetAvx(int startScanline, int increment)
        {
            if (Precision == Precision.Single)
                ImplAvx.ComputeSingle(Iterations, startScanline, increment, OffsetX, OffsetY, Zoom, MaxIterations, ref _cancel[0]);
            else
                ImplAvx.ComputeDouble(Iterations, startScanline, increment, OffsetX, OffsetY, Zoom, MaxIterations, ref _cancel[0]);
        }

        private void ComputeSetCuda()
        {
            _cuda.ComputeMandelbrotSet(Iterations, Width, Height, OffsetX, OffsetY, Zoom, MaxIterations, Precision == Precision.Double, ref _cancel[0]);
        }

        private readonly object _lock;
        private readonly bool[] _cancel;
        private readonly ImplCuda _cuda;
        private readonly List<Exception> _exceptions;
        private Thread[] _workerThreads;
        private int _threads;
    }
}
