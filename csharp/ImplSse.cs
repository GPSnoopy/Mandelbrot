using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mandelbrot
{
    internal static class ImplSse
    {
        public static unsafe void ComputeSingle(
            uint[,] iterations,
            int startScanline, int increment,
            double offsetX, double offsetY,
            double zoom,
            uint maxIterations,
            ref bool cancel)
        {
            const int stride = 4;

            int height = iterations.GetLength(0);
            int width = iterations.GetLength(1);

            var maxIter = Vector128.Create((float) maxIterations);
            var limit = Vector128.Create(4.0f);
            var one = Vector128.Create(1.0f);
            var two = Vector128.Create(2.0f);
            var results = stackalloc float[stride];

            for (int i = startScanline; i < height && !cancel; i += increment)
            {
                for (int j = 0; j < width && !cancel; j += stride)
                {
                    var c0 = Impl.GetPointCoordinate(j + 0, i, width, height, offsetX, offsetY, zoom);
                    var c1 = Impl.GetPointCoordinate(j + 1, i, width, height, offsetX, offsetY, zoom);
                    var c2 = Impl.GetPointCoordinate(j + 2, i, width, height, offsetX, offsetY, zoom);
                    var c3 = Impl.GetPointCoordinate(j + 3, i, width, height, offsetX, offsetY, zoom);

                    var cr = Vector128.Create((float) c0.X, (float) c1.X, (float) c2.X, (float) c3.X);
                    var ci = Vector128.Create((float) c0.Y, (float) c1.Y, (float) c2.Y, (float) c3.Y);
                    var zr = cr;
                    var zi = ci;
                    var it = Vector128.Create(0f);

                    for (;;)
                    {
                        var zr2 = Sse.Multiply(zr, zr);
                        var zi2 = Sse.Multiply(zi, zi);
                        var squaredMagnitude = Sse.Add(zr2, zi2);

                        var cond = Sse.And(
                            Sse.CompareLessThanOrEqual(squaredMagnitude, limit),
                            Sse.CompareLessThanOrEqual(it, maxIter));

                        if (Sse.MoveMask(cond) == 0)
                        {
                            Sse.Store(results, it);

                            if (j + 0 < width) iterations[i, j + 0] = (uint) results[0] % maxIterations;
                            if (j + 1 < width) iterations[i, j + 1] = (uint) results[1] % maxIterations;
                            if (j + 2 < width) iterations[i, j + 2] = (uint) results[2] % maxIterations;
                            if (j + 3 < width) iterations[i, j + 3] = (uint) results[3] % maxIterations;
                            break;
                        }

                        zi = Sse.Add(Sse.Multiply(two, Sse.Multiply(zr, zi)), ci);
                        zr = Sse.Add(Sse.Subtract(zr2, zi2), cr);
                        it = Sse.Add(it, Sse.And(one, cond));
                    }
                }
            }
        }

        public static unsafe void ComputeDouble(
            uint[,] iterations,
            int startScanline, int increment,
            double offsetX, double offsetY,
            double zoom,
            uint maxIterations,
            ref bool cancel)
        {
            const int stride = 2;

            int height = iterations.GetLength(0);
            int width = iterations.GetLength(1);

            var maxIter = Vector128.Create((double)maxIterations);
            var limit = Vector128.Create(4.0);
            var one = Vector128.Create(1.0);
            var two = Vector128.Create(2.0);
            var results = stackalloc double[stride];
            
            for (int i = startScanline; i < height && !cancel; i += increment)
            {
                for (int j = 0; j < width && !cancel; j += stride)
                {
                    var c0 = Impl.GetPointCoordinate(j + 0, i, width, height, offsetX, offsetY, zoom);
                    var c1 = Impl.GetPointCoordinate(j + 1, i, width, height, offsetX, offsetY, zoom);

                    var cr = Vector128.Create(c0.X, c1.X);
                    var ci = Vector128.Create(c0.Y, c1.Y);
                    var zr = cr;
                    var zi = ci;
                    var it = Vector128.Create(0.0);

                    for (;;)
                    {
                        var zr2 = Sse2.Multiply(zr, zr);
                        var zi2 = Sse2.Multiply(zi, zi);
                        var squaredMagnitude = Sse2.Add(zr2, zi2);

                        var cond = Sse2.And(
                            Sse2.CompareLessThanOrEqual(squaredMagnitude, limit),
                            Sse2.CompareLessThanOrEqual(it, maxIter));

                        if (Sse2.MoveMask(cond) == 0)
                        {
                            Sse2.Store(results, it);

                            if (j + 0 < width) iterations[i, j + 0] = (uint) results[0] % maxIterations;
                            if (j + 1 < width) iterations[i, j + 1] = (uint) results[1] % maxIterations;
                            break;
                        }

                        zi = Sse2.Add(Sse2.Multiply(two, Sse2.Multiply(zr, zi)), ci);
                        zr = Sse2.Add(Sse2.Subtract(zr2, zi2), cr);
                        it = Sse2.Add(it, Sse2.And(one, cond));
                    }
                }
            }
        }
    }
}
