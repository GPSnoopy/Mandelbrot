
namespace Mandelbrot
{
    internal static class Impl
    {
        public static void ComputeSingle(
            uint[,] iterations,
            int startScanline, int increment,
            double offsetX, double offsetY,
            double zoom,
            uint maxIterations,
            ref bool cancel)
        {
            int height = iterations.GetLength(0);
            int width = iterations.GetLength(1);

            for (int i = startScanline; i < height && !cancel; i += increment)
            {
                for (int j = 0; j < width && !cancel; ++j)
                {
                    const double maxSquaredMagnitude = 4;

                    var coord = GetPointCoordinate(j, i, width, height, offsetX, offsetY, zoom);
                    var x = (float) coord.X;
                    var y = (float) coord.Y;

                    iterations[i, j] = 0;

                    //    C = x + y.j
                    //   Z0 = 0
                    // Zn+1 = (Zn^2) + C
                    //
                    // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)

                    float zr = x;
                    float zi = y;

                    for (uint it = 1; it <= maxIterations; ++it)
                    {
                        float zr2 = zr * zr;
                        float zi2 = zi * zi;
                        float squaredMagnitude = (zr2 + zi2);

                        if (squaredMagnitude > maxSquaredMagnitude)
                        {
                            iterations[i, j] = it;
                            break;
                        }

                        float tr = zr2 - zi2 + x;

                        zi = 2 * zr * zi + y;
                        zr = tr;
                    }
                }
            }
        }

        public static void ComputeDouble(
            uint[,] iterations,
            int startScanline, int increment,
            double offsetX, double offsetY,
            double zoom,
            uint maxIterations,
            ref bool cancel)
        {
            int height = iterations.GetLength(0);
            int width = iterations.GetLength(1);

            for (int i = startScanline; i < height && !cancel; i += increment)
            {
                for (int j = 0; j < width && !cancel; ++j)
                {
                    const double maxSquaredMagnitude = 4;

                    var coord = GetPointCoordinate(j, i, width, height, offsetX, offsetY, zoom);
                    var x = coord.X;
                    var y = coord.Y;

                    iterations[i, j] = 0;

                    //    C = x + y.j
                    //   Z0 = 0
                    // Zn+1 = (Zn^2) + C
                    //
                    // IsMandelbrot <=> ( |Zn| <= 2 ) for any n (natural number)

                    double zr = x;
                    double zi = y;

                    for (uint it = 1; it <= maxIterations; ++it)
                    {
                        double zr2 = zr * zr;
                        double zi2 = zi * zi;
                        double squaredMagnitude = (zr2 + zi2);

                        if (squaredMagnitude > maxSquaredMagnitude)
                        {
                            iterations[i, j] = it;
                            break;
                        }

                        double tr = zr2 - zi2 + x;

                        zi = 2 * zr * zi + y;
                        zr = tr;
                    }
                }
            }
        }

        public static Coord2D GetPointCoordinate(
            double screenX, double screenY,
            int width, int height,
            double offsetX, double offsetY,
            double zoom)
        {
            double coordX =
                -(width / (height * zoom)) + offsetX
                                           + ((screenX * 2) / (height * zoom));

            double coordY =
                -1.0 / zoom + offsetY
                            + ((screenY * 2) / (height * zoom));

            return new Coord2D(coordX, coordY);
        }
    }
}