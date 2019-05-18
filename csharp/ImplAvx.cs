using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;

namespace Mandelbrot
{
    internal static class ImplAvx
    {
        public static unsafe void ComputeSingle(
            uint[,] iterations,
            int startScanline, int increment,
            double offsetX, double offsetY,
            double zoom,
            uint maxIterations,
            ref bool cancel)
        {
            const int stride = 8;

            int height = iterations.GetLength(0);
            int width = iterations.GetLength(1);

            var maxIter = Vector256.Create((float) maxIterations);
            var limit = Vector256.Create(4.0f);
            var one = Vector256.Create(1.0f);
            var two = Vector256.Create(2.0f);

            float* results = stackalloc float[stride];

            for (int i = startScanline; i < height && !cancel; i += increment)
            {
                for (int j = 0; j < width && !cancel; j += stride)
                {
                    var c0 = Impl.GetPointCoordinate(j + 0, i, width, height, offsetX, offsetY, zoom);
                    var c1 = Impl.GetPointCoordinate(j + 1, i, width, height, offsetX, offsetY, zoom);
                    var c2 = Impl.GetPointCoordinate(j + 2, i, width, height, offsetX, offsetY, zoom);
                    var c3 = Impl.GetPointCoordinate(j + 3, i, width, height, offsetX, offsetY, zoom);
                    var c4 = Impl.GetPointCoordinate(j + 4, i, width, height, offsetX, offsetY, zoom);
                    var c5 = Impl.GetPointCoordinate(j + 5, i, width, height, offsetX, offsetY, zoom);
                    var c6 = Impl.GetPointCoordinate(j + 6, i, width, height, offsetX, offsetY, zoom);
                    var c7 = Impl.GetPointCoordinate(j + 7, i, width, height, offsetX, offsetY, zoom);

                    var cr = Vector256.Create((float) c0.X, (float) c1.X, (float) c2.X, (float) c3.X, (float) c4.X, (float) c5.X, (float) c6.X, (float) c7.X);
                    var ci = Vector256.Create((float) c0.Y, (float) c1.Y, (float) c2.Y, (float) c3.Y, (float) c4.Y, (float) c5.Y, (float) c6.Y, (float) c7.Y);
                    var zr = cr;
                    var zi = ci;
                    var it = Vector256.Create(0f);

                    for (;;)
                    {
                        var zr2 = Avx.Multiply(zr, zr);
                        var zi2 = Avx.Multiply(zi, zi);
                        var squaredMagnitude = Avx.Add(zr2, zi2);

                        var cond = Avx.And(
                            Avx.Compare(squaredMagnitude, limit, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling),
                            Avx.Compare(it, maxIter, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling));

                        if (Avx.MoveMask(cond) == 0)
                        {
                            Avx.Store(results, it);

                            if (j + 0 < width) iterations[i, j + 0] = (uint) results[0] % maxIterations;
                            if (j + 1 < width) iterations[i, j + 1] = (uint) results[1] % maxIterations;
                            if (j + 2 < width) iterations[i, j + 2] = (uint) results[2] % maxIterations;
                            if (j + 3 < width) iterations[i, j + 3] = (uint) results[3] % maxIterations;
                            if (j + 4 < width) iterations[i, j + 4] = (uint) results[4] % maxIterations;
                            if (j + 5 < width) iterations[i, j + 5] = (uint) results[5] % maxIterations;
                            if (j + 6 < width) iterations[i, j + 6] = (uint) results[6] % maxIterations;
                            if (j + 7 < width) iterations[i, j + 7] = (uint) results[7] % maxIterations;
                            break;
                        }

                        zi = Fma.MultiplyAdd(two, Avx.Multiply(zr, zi), ci);
                        zr = Avx.Add(Avx.Subtract(zr2, zi2), cr);
                        it = Avx.Add(it, Avx.And(one, cond));
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
            const int stride = 4;

            int height = iterations.GetLength(0);
            int width = iterations.GetLength(1);

            var maxIter = Vector256.Create((double) maxIterations);
            var limit = Vector256.Create(4.0);
            var one = Vector256.Create(1.0);
            var two = Vector256.Create(2.0);
            var results = stackalloc double[stride];

            for (int i = startScanline; i < height && !cancel; i += increment)
            {
                for (int j = 0; j < width && !cancel; j += stride)
                {
                    var c0 = Impl.GetPointCoordinate(j + 0, i, width, height, offsetX, offsetY, zoom);
                    var c1 = Impl.GetPointCoordinate(j + 1, i, width, height, offsetX, offsetY, zoom);
                    var c2 = Impl.GetPointCoordinate(j + 2, i, width, height, offsetX, offsetY, zoom);
                    var c3 = Impl.GetPointCoordinate(j + 3, i, width, height, offsetX, offsetY, zoom);

                    var cr = Vector256.Create(c0.X, c1.X, c2.X, c3.X);
                    var ci = Vector256.Create(c0.Y, c1.Y, c2.Y, c3.Y);
                    var zr = cr;
                    var zi = ci;
                    var it = Vector256.Create(0.0);

                    for (; ; )
                    {
                        var zr2 = Avx.Multiply(zr, zr);
                        var zi2 = Avx.Multiply(zi, zi);
                        var squaredMagnitude = Avx.Add(zr2, zi2);

                        var cond = Avx.And(
                            Avx.Compare(squaredMagnitude, limit, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling),
                            Avx.Compare(it, maxIter, FloatComparisonMode.OrderedLessThanOrEqualNonSignaling));

                        if (Avx.MoveMask(cond) == 0)
                        {
                            Avx.Store(results, it);

                            if (j + 0 < width) iterations[i, j + 0] = (uint)results[0] % maxIterations;
                            if (j + 1 < width) iterations[i, j + 1] = (uint)results[1] % maxIterations;
                            if (j + 2 < width) iterations[i, j + 2] = (uint)results[2] % maxIterations;
                            if (j + 3 < width) iterations[i, j + 3] = (uint)results[3] % maxIterations;
                            break;
                        }

                        zi = Fma.MultiplyAdd(two, Avx.Multiply(zr, zi), ci);
                        zr = Avx.Add(Avx.Subtract(zr2, zi2), cr);
                        it = Avx.Add(it, Avx.And(one, cond));
                    }
                }
            }
        }
    }
}
