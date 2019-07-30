using System;
using ILGPU;
using ILGPU.Runtime;
using ILGPU.Runtime.Cuda;

namespace Mandelbrot
{
    internal sealed class ImplCuda : IDisposable
    {
        public ImplCuda()
        {
            _context = new Context();
            _gpu = new CudaAccelerator(_context);
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

            var doubleOverhead = doublePrecision ? 32 : 1;
            var cores = _gpu.NumMultiprocessors * 128; // TODO ILGPU proper number of CUDA cores per multi processor.
            var maxPixelPerBatch = 200L * _gpu.ClockRate * cores / (doubleOverhead * maxIterations);

            var blockSizeX = 16;
            var blockSizeY = 8;
            var gridX = DivUp(width, blockSizeX);
            var gridY = DivUp(height, blockSizeY);
            var maxBlocks = DivUp(checked((int) maxPixelPerBatch), blockSizeX * blockSizeY);
            var maxBlocksY = DivUp(maxBlocks, gridX);

            // Precompute the pixels coordinates.
            double startX = -(width / (height * zoom)) + offsetX;
            double startY = -1.0 / zoom + offsetY;
            double scale = 2 / (height * zoom);

            // Batch the mandelbrot computation into sets of block so that we avoid hitting the Windows watchdog timer for CUDA (5 seconds on Windows 10).
            for (int blockOffsetY = 0; blockOffsetY < gridY; blockOffsetY += maxBlocksY)
            {
                if (cancel)
                {
                    break;
                }

                var lp = new GroupedIndex2(
                    new Index2(gridX, Math.Min(gridY, maxBlocksY)),
                    new Index2(blockSizeX, blockSizeY));

                var kernel = _gpu.LoadStreamKernel(doublePrecision
                    ? (Action<GroupedIndex2, ArrayView<uint>, int, int, int, double, double, double, uint>) DoubleKernel
                    : SingleKernel);

                kernel(lp, _deviceMemory.ToArrayView(), width, height, blockOffsetY * blockSizeY, startX, startY, scale, maxIterations);
            }

            fixed (uint* ptr = iterations)
            {
                using var viewArrayWrapper = ViewPointerWrapper.Create(new IntPtr(ptr));
                var arrayView = new ArrayView<uint>(viewArrayWrapper, 0, iterations.Length);

                _deviceMemory.CopyTo(arrayView, 0);
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
            GroupedIndex2 index,
            ArrayView<uint> iterations,
            int width, int height,
            int blockOffsetY,
            double startX, double startY, double scale,
            uint maxIterations)
        {

            var i = index.ComputeGlobalIndex().Y + blockOffsetY;
            var j = index.ComputeGlobalIndex().X;

            if (i < height && j < width)
            {
                var x = (float) (startX + j * scale);
                var y = (float) (startY + i * scale);

                const float maxSquaredMagnitude = 4;

                //    C = x + y.j
                //   Z0 = 0
                // Zn+1 = (Zn^2) + C
                //
                // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)

                var zr = x;
                var zi = y;
                uint it;

                for (it = 0; it < maxIterations; ++it)
                {
                    var zr2 = zr * zr;
                    var zi2 = zi * zi;
                    var squaredMagnitude = zr2 + zi2;

                    if (squaredMagnitude > maxSquaredMagnitude)
                    {
                        break;
                    }

                    var tr = zr2 - zi2 + x;

                    zi = 2 * zr * zi + y;
                    zr = tr;
                }

                iterations[i * width + j] = it == maxIterations ? 0 : it + 1;
            }
        }

        private static void DoubleKernel(
            GroupedIndex2 index,
            ArrayView<uint> iterations,
            int width, int height,
            int blockOffsetY,
            double startX, double startY, double scale,
            uint maxIterations)
        {

            var i = index.ComputeGlobalIndex().Y + blockOffsetY;
            var j = index.ComputeGlobalIndex().X;

            if (i < height && j < width)
            {
                var x = startX + j * scale;
                var y = startY + i * scale;

                const double maxSquaredMagnitude = 4;

                //    C = x + y.j
                //   Z0 = 0
                // Zn+1 = (Zn^2) + C
                //
                // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)

                var zr = x;
                var zi = y;
                uint it;

                for (it = 0; it < maxIterations; ++it)
                {
                    var zr2 = zr * zr;
                    var zi2 = zi * zi;
                    var squaredMagnitude = zr2 + zi2;

                    if (squaredMagnitude > maxSquaredMagnitude)
                    {
                        break;
                    }

                    var tr = zr2 - zi2 + x;

                    zi = 2 * zr * zi + y;
                    zr = tr;
                }

                iterations[i * width + j] = it == maxIterations ? 0 : it + 1;
            }
        }

        private static int DivUp(int dividend, int divisor)
        {
            return (dividend + divisor - 1) / divisor;
        }

        private readonly Context _context;
        private readonly CudaAccelerator _gpu;
        private MemoryBuffer<uint> _deviceMemory;
        private int _width;
        private int _height;
    }
}
