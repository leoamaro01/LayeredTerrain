using System;
using Random = System.Random;
using Vector2 = System.Numerics.Vector2;

namespace LayeredTerrain.Utilities
{
    public static class NoiseLayerComputes
    {
        public static float[,] DiamondSquareNoise(int x, int y, int width, int height, int seed, float smoothness, float initialRandomRange, float clampMin, float clampMax)
        {
            if (width != height || ((width - 2) & (width - 1)) != 0)
                throw new ArgumentException("The Diamond-Square algorithm requires chunks to be square and their side a power of 2 minus 1.");

            // making width and height a power of 2
            width -= 1;
            height -= 1;

            // X and Y of the top left corner of the main diamond-square patch (size width;height)
            var baseX = x * width;
            var baseY = y * height;

            // top left and top right of the total area to be computing, counting surrounding diamond-square patches for margins.
            var topLeftX = baseX - width;
            var topLeftY = baseY - height;

            var bottomRightX = baseX + width * 2;
            var bottomRightY = baseY + height * 2;

            // +1 to include the bottom right coordinates
            var diamondSquarePatch = new float[1 + bottomRightX - topLeftX, 1 + bottomRightY - topLeftY];

            void SetPointHeight(int x, int y, float height)
            {
                if (x < topLeftX || x > bottomRightX || y < topLeftY || y > bottomRightY)
                    throw new IndexOutOfRangeException("Tried to set point height on non-existing coordinates in the diamond-square patch");

                diamondSquarePatch[x - topLeftX, y - topLeftY] = height;
            }
            float GetPointHeight(int x, int y)
            {
                if (x < topLeftX || x > bottomRightX || y < topLeftY || y > bottomRightY)
                    throw new IndexOutOfRangeException("Tried to set point height on non-existing coordinates in the diamond-square patch");

                return diamondSquarePatch[x - topLeftX, y - topLeftY];
            }

            // seeding the corners of the 9 patches
            for (var i = 0; i < 4; i++)
                for (var e = 0; e < 4; e++)
                {
                    var coordX = topLeftX + i * width;
                    var coordY = topLeftY + e * height;

                    var cornerSeed = Utils.GetCombinedSeed(coordX, coordY, seed);

                    var cornerValue = ((float)new Random(cornerSeed).NextDouble() - 0.5f) * initialRandomRange;

                    SetPointHeight(coordX, coordY, cornerValue);
                }

            var startX = topLeftX;
            var startY = topLeftY;
            var stopX = bottomRightX;
            var stopY = bottomRightY;

            var randomRange = initialRandomRange;

            var squareIncrement = width;

            while (squareIncrement > 1)
            {
                // Diamond
                for (var i = startX; i < stopX; i += squareIncrement)
                    for (var e = startY; e < stopY; e += squareIncrement)
                    {
                        var topLeft = GetPointHeight(i, e);
                        var topRight = GetPointHeight(i + squareIncrement, e);
                        var bottomLeft = GetPointHeight(i, e + squareIncrement);
                        var bottomRight = GetPointHeight(i + squareIncrement, e + squareIncrement);

                        var average = (topLeft + topRight + bottomLeft + bottomRight) / 4f;

                        var centerX = i + squareIncrement / 2;
                        var centerY = e + squareIncrement / 2;

                        var centerSeed = Utils.GetCombinedSeed(centerX, centerY, seed);
                        var randomValue = ((float)new Random(centerSeed).NextDouble() - 0.5f) * randomRange;

                        SetPointHeight(centerX, centerY, average + randomValue);
                    }

                // Square
                bool evenY = false;
                for (var e = startY + squareIncrement / 2; e < stopY; e += squareIncrement / 2)
                {
                    var offset = evenY ? -squareIncrement / 2 : 0;
                    evenY = !evenY;

                    for (var i = startX + offset + squareIncrement; i < stopX; i += squareIncrement)
                    {
                        var top = GetPointHeight(i, e - squareIncrement / 2);
                        var right = GetPointHeight(i + squareIncrement / 2, e);
                        var bottom = GetPointHeight(i, e + squareIncrement / 2);
                        var left = GetPointHeight(i - squareIncrement / 2, e);

                        var average = (top + right + bottom + left) / 4f;

                        var diamondSeed = Utils.GetCombinedSeed(i, e, seed);
                        var randomValue = ((float)new Random(diamondSeed).NextDouble() - 0.5f) * randomRange;

                        SetPointHeight(i, e, average + randomValue);
                    }
                }

                randomRange *= MathF.Pow(2f, -smoothness);
                startX += squareIncrement / 2;
                startY += squareIncrement / 2;
                stopX -= squareIncrement / 2;
                stopY -= squareIncrement / 2;
                squareIncrement /= 2;
            }

            var result = new float[width + 1, height + 1];

            //var totalIters = MathF.Log(width, 2) + 0.001f;

            //var theoreticalMax = initialRandomRange / 2f;
            //var currentProbability = 0.5f;
            //var range = initialRandomRange;
            //for (var i = 0; i < totalIters && currentProbability > theoreticalMaximumProbabilityThreshold; i++)
            //{
            //    theoreticalMax += initialRandomRange / 2f;
            //    range *= MathF.Pow(2f, -smoothness);
            //    currentProbability *= 0.5f;
            //}

            for (var i = 0; i <= width; i++)
                for (var e = 0; e <= height; e++)
                {
                    var point = GetPointHeight(baseX + i, baseY + e);

                    // Interpolate height values between 0 and 1
                    var value = (point - clampMin) / (clampMax - clampMin);

                    if (value > 1)
                        value = 1;
                    else if (value < 0)
                        value = 0;

                    result[i, e] = value;
                }

            return result;
        }

        public static float[,] PerlinNoise(int x, int y, int width, int height, int seed, int resolution)
        {
            var baseX = x * (width - 1);
            var baseY = y * (height - 1);

            var gradMinX = (int)MathF.Floor((float)baseX / resolution) * resolution;
            var gradMinY = (int)MathF.Floor((float)baseY / resolution) * resolution;

            var gradMaxX = (int)MathF.Ceiling((float)(baseX + width) / resolution) * resolution;
            var gradMaxY = (int)MathF.Ceiling((float)(baseY + height) / resolution) * resolution;

            var gradientsMatrixWidth = (gradMaxX - gradMinX) / resolution + 1;
            var gradientsMatrixHeight = (gradMaxY - gradMinY) / resolution + 1;

            Vector2[,] gradients = new Vector2[gradientsMatrixWidth, gradientsMatrixHeight];

            for (var i = 0; i < gradientsMatrixWidth; i++)
                for (var e = 0; e < gradientsMatrixHeight; e++)
                {
                    var gradientPosX = gradMinX + i * resolution; ;
                    var gradientPosY = gradMinY + e * resolution;

                    var gradientSeed = Utils.GetCombinedSeed(gradientPosX, gradientPosY, seed);
                    var random = new Random(gradientSeed);

                    gradients[i, e] = Vector2.Normalize(new Vector2((float)random.NextDouble() - 0.5f, (float)random.NextDouble() - 0.5f)); ;
                }

            float[,] perlinMap = new float[width, height];

            var halfResSpace = 0.5f / resolution;
            var halfResBlock = new Vector2(halfResSpace, halfResSpace);

            for (var i = 0; i < gradientsMatrixWidth - 1; i++)
                for (var e = 0; e < gradientsMatrixHeight - 1; e++)
                {
                    var clockwiseGradients = new Vector2[] {
                        gradients[i, e],
                        gradients[i + 1, e],
                        gradients[i + 1, e + 1],
                        gradients[i, e + 1]
                    };

                    var gradPosX = gradMinX + i * resolution;
                    var gradPosY = gradMinY + e * resolution;

                    var startX = Math.Max(baseX, gradPosX);
                    var startY = Math.Max(baseY, gradPosY);

                    var stopX = Math.Min(baseX + width, gradPosX + resolution);
                    var stopY = Math.Min(baseY + height, gradPosY + resolution);

                    for (var cellX = startX; cellX < stopX; cellX++)
                        for (var cellY = startY; cellY < stopY; cellY++)
                        {
                            var normPosX = (float)(cellX - gradPosX) / resolution;
                            var normPosY = (float)(cellY - gradPosY) / resolution;

                            var pointCoords = new Vector2(normPosX, normPosY) + halfResBlock;

                            perlinMap[cellX - baseX, cellY - baseY] = GeneratePerlinPoint(pointCoords, clockwiseGradients);
                        }
                }

            return perlinMap;
        }

        private static float Dot(Vector2 a, Vector2 b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        private static float GeneratePerlinPoint(Vector2 coords, Vector2[] clockwiseGradients)
        {
            var clockwiseDots = new float[clockwiseGradients.Length];

            Vector2[] corners = new Vector2[] {
                new Vector2(0f, 0f),
                new Vector2(1f, 0f),
                new Vector2(1f, 1f),
                new Vector2(0f, 1f)
            };

            for (var i = 0; i < clockwiseGradients.Length; i++)
            {
                var offset = coords - corners[i];

                clockwiseDots[i] = Dot(offset, clockwiseGradients[i]);
            }

            var fadedX = Fade(coords.X);
            var fadedY = Fade(coords.Y);

            var xInterpolation0 = Interpolate(clockwiseDots[0], clockwiseDots[1], fadedX);
            var xInterpolation1 = Interpolate(clockwiseDots[3], clockwiseDots[2], fadedX);

            return (1 + Interpolate(xInterpolation0, xInterpolation1, fadedY)) * 0.5f;
        }

        private static float Fade(float t)
        {
            // 6t^5 + 15t^4 + 10t^3
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Interpolate(float a, float b, float t)
        {
            return a + t * (b - a);
        }
    }
}

