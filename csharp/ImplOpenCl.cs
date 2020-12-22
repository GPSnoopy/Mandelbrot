using System;
using System.Linq;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.OpenCL;

namespace Mandelbrot
{
    internal sealed class ImplOpenCl : IDisposable
    {
        public static bool IsSupported => CLAccelerator.CLAccelerators.Any();
        
        public ImplOpenCl()
        {
            _context = new Context();
            var id = CLAccelerator.AllCLAccelerators.First();
            _gpu = new CLAccelerator(_context, id);
            _doubleKernel = _gpu.LoadAutoGroupedStreamKernel<Index2, ArrayView<uint>, int, int, double, double, double, uint>(DoubleKernel);
            _singleKernel = _gpu.LoadAutoGroupedStreamKernel<Index2, ArrayView<uint>, int, int,float, float, float, uint>(SingleKernel);
        }

        public void Dispose()
        {
            _deviceMemory?.Dispose();
            _deviceMemory = null;
            _gpu.Dispose();
            _context.Dispose();
        }

        public unsafe void ComputeMandelbrotSet(
            uint[,] iterations,
            int width, int height,
            double offsetX, double offsetY, double zoom,
            uint maxIterations,
            bool doublePrecision,
            ref bool cancel)
        {
            Resize(width, height);

            // Precompute the pixels coordinates.
            double startX = -(width / (height * zoom)) + offsetX;
            double startY = -1.0 / zoom + offsetY;
            double scale = 2 / (height * zoom);

                if (doublePrecision)
                {
                    _doubleKernel(new Index2(width, height), _deviceMemory.View, width, height, startX, startY, scale, maxIterations);
                }
                else
                {
                    _singleKernel(new Index2(width, height), _deviceMemory.View, width, height, (float)startX, (float) startY, (float) scale, maxIterations);
                }

            fixed (uint* ptr = iterations)
            {
                using var viewArrayWrapper = ViewPointerWrapper.Create(new IntPtr(ptr));
                var arrayView = new ArrayView<uint>(viewArrayWrapper, 0, iterations.Length);

                _deviceMemory.CopyTo(arrayView, 0);
                _gpu.Synchronize();
            }
        }

        private void Resize(int width, int height)
        {
            if (_width != width || _height != height)
            {
                _deviceMemory?.Dispose();
                _deviceMemory = _gpu.Allocate<uint>(width * height);
                _width = width;
                _height = height;
            }
        }

        private static void SingleKernel(
            Index2 index,
            ArrayView<uint> iterations,
            int width, int height,
            float startX, float startY, float scale,
            uint maxIterations)
        {
            var j = index.X;
            var i = index.Y;

            var x0 = startX + j * scale;
            var y0 = startY + i * scale;

            //    C = x + y.j
            //   Z0 = 0
            // Zn+1 = (Zn^2) + C
            //
            // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)
            var x = 0.0f;
            var y = 0.0f;
            var x2 = 0.0f;
            var y2 = 0.0f;
            var iteration = 0u;
            do
            {
                x2 = x * x;
                y2 = y * y;
                var xtmp = x2 - y2 + x0;
                y = 2 * x * y + y0;
                x = xtmp;
            } while (x2 + y2 <= 4 && ++iteration < maxIterations);

            iterations[i * width + j] = iteration == maxIterations ? 0u : iteration;
        }

        public static void DoubleKernel(
            Index2 index,
            ArrayView<uint> iterations,
            int width, int height,
            double startX, double startY, double scale,
            uint maxIterations)
        {
            var j = index.X;
            var i = index.Y;

            var x0 = startX + j * scale;
            var y0 = startY + i * scale;

            
            //    C = x + y.j
            //   Z0 = 0
            // Zn+1 = (Zn^2) + C
            //
            // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)
            var x = 0.0;
            var y = 0.0;
            var x2 = 0.0;
            var y2 = 0.0;
            var iteration = 0u;
            do
            {
                x2 = x * x;
                y2 = y * y;
                var xtmp = x2 - y2 + x0;
                y = 2 * x * y + y0;
                x = xtmp;
            } while (x2 + y2 <= 4 && ++iteration < maxIterations);

            iterations[i * width + j] = iteration == maxIterations ? 0u : iteration;
        }

        private readonly Context _context;
        private readonly Accelerator _gpu;
        private MemoryBuffer<uint> _deviceMemory;
        private int _width;
        private int _height;
        private readonly Action<Index2, ArrayView<uint>, int, int, double, double, double, uint> _doubleKernel;
        private readonly Action<Index2, ArrayView<uint>, int, int, float, float, float, uint> _singleKernel;
    }
}