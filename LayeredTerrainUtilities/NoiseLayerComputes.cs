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

            var xInterpolation0 = Lerp(clockwiseDots[0], clockwiseDots[1], fadedX);
            var xInterpolation1 = Lerp(clockwiseDots[3], clockwiseDots[2], fadedX);

            return (1 + Lerp(xInterpolation0, xInterpolation1, fadedY)) * 0.5f;
        }

        private static float Fade(float t)
        {
            // 6t^5 + 15t^4 + 10t^3
            return t * t * t * (t * (t * 6 - 15) + 10);
        }

        private static float Lerp(float a, float b, float t)
        {
            return a + t * (b - a);
        }

        public static float[,] WorleyNoiseNormal(int x, int y, int width, int height, int seed, int cellWidth, int cellHeight, float minDistance, float maxDistance, bool inverse)
        {
            return WorleyNoise(x, y, width, height, seed, cellWidth, cellHeight, minDistance, maxDistance, inverse, (a, b) => (a - b).LengthSquared(), MathF.Sqrt);
        }

        public static float[,] WorleyNoiseWavy(int x, int y, int width, int height, int seed, int cellWidth, int cellHeight, float maxWaveAmplitudeDistance, float minDistance, float maxDistance, bool inverse, float waveFrequency, float waveAmplitude)
        {
            return WorleyNoise(x, y, width, height, seed, cellWidth, cellHeight, minDistance, maxDistance, inverse,
                (a, b) =>
                {
                    var dif = a - b;
                    var dist = dif.Length();
                    var cos = dif.X / dist;
                    var amplitude = waveAmplitude * (MathF.Min(dist, maxWaveAmplitudeDistance) / maxWaveAmplitudeDistance);
                    return dist + amplitude * MathF.Cos(waveFrequency * MathF.Acos(cos));
                }, d => d);
        }

        public static float[,] WorleyNoise(int x, int y, int width, int height, int seed, int cellWidth, int cellHeight, float minDistance, float maxDistance, bool inverse, Func<Vector2, Vector2, float> norm, Func<float, float> normDeSquare)
        {
            var baseX = x * (width - 1);
            var baseY = y * (height - 1);


            // Add a cell on every side to account for a margin
            var gridMinX = (int)MathF.Floor((float)baseX / cellWidth) * cellWidth - cellWidth;
            var gridMinY = (int)MathF.Floor((float)baseY / cellHeight) * cellHeight - cellHeight;

            var gridMaxX = (int)MathF.Ceiling((float)(baseX + width) / cellWidth) * cellWidth + cellWidth;
            var gridMaxY = (int)MathF.Ceiling((float)(baseY + height) / cellHeight) * cellHeight + cellHeight;

            var gridWidth = ((gridMaxX - gridMinX) / cellWidth);
            var gridHeight = ((gridMaxY - gridMinY) / cellHeight);

            var gridPoints = new Vector2[gridWidth, gridHeight];

            for (var i = 0; i < gridWidth; i++)
                for (var e = 0; e < gridHeight; e++)
                {
                    var cellX = gridMinX + i * cellWidth;
                    var cellY = gridMinY + e * cellHeight;

                    var cellSeed = Utils.GetCombinedSeed(cellX, cellY, seed);
                    var cellRandom = new Random(cellSeed);

                    gridPoints[i, e] = new Vector2(cellX + (float)cellRandom.NextDouble() * cellWidth, cellY + (float)cellRandom.NextDouble() * cellHeight);
                }

            var result = new float[width, height];

            for (var i = 1; i < gridWidth - 1; i++)
                for (var e = 1; e < gridHeight - 1; e++)
                {
                    var cellX = gridMinX + i * cellWidth;
                    var cellY = gridMinY + e * cellHeight;

                    var startX = Math.Max(baseX, cellX);
                    var startY = Math.Max(baseY, cellY);

                    var stopX = Math.Min(baseX + width, cellX + cellWidth);
                    var stopY = Math.Min(baseY + height, cellY + cellHeight);

                    for (var pointX = startX; pointX < stopX; pointX++)
                        for (var pointY = startY; pointY < stopY; pointY++)
                        {
                            var closestDistanceSquared = -1f;

                            var pointPos = new Vector2(pointX, pointY);

                            for (var h = -1; h <= 1; h++)
                                for (var v = -1; v <= 1; v++)
                                {
                                    var cellPointPos = gridPoints[i + h, e + v];

                                    var distSqr = norm(cellPointPos, pointPos);

                                    if (closestDistanceSquared < 0 || distSqr < closestDistanceSquared)
                                        closestDistanceSquared = distSqr;
                                }

                            var dist = normDeSquare(closestDistanceSquared);

                            float value;
                            if (dist < minDistance)
                                value = 0f;
                            else if (dist > maxDistance)
                                value = 1f;
                            else
                                value = (dist - minDistance) / (maxDistance - minDistance);

                            if (inverse)
                                value = 1 - value;

                            value = Fade(value);

                            result[pointX - baseX, pointY - baseY] = value;
                        }
                }

            return result;
        }

        public static float[,] WorleyNoiseNormalEdges(int x, int y, int width, int height, int seed, int cellWidth, int cellHeight, float cellJitter, float minDistance, float minDifThreshold, float maxDifThreshold, bool inverse)
        {
            return WorleyNoiseEdges(x, y, width, height, seed, cellWidth, cellHeight, cellJitter, minDistance, minDifThreshold, maxDifThreshold, inverse, (a, b) => (a - b).LengthSquared(), MathF.Sqrt);
        }

        public static float[,] WorleyNoiseWavyEdges(int x, int y, int width, int height, int seed, int cellWidth, int cellHeight, float cellJitter, float maxWaveAmplitudeDistance, float minDistance, float minDifThreshold, float maxDifThreshold, bool inverse, float waveFrequency, float waveAmplitude)
        {
            return WorleyNoiseEdges(x, y, width, height, seed, cellWidth, cellHeight, cellJitter, minDistance, minDifThreshold, maxDifThreshold, inverse,
                (a, b) =>
                {
                    var dif = a - b;
                    var dist = dif.Length();
                    var cos = dif.X / dist;
                    var amplitude = waveAmplitude * (MathF.Min(dist, maxWaveAmplitudeDistance) / maxWaveAmplitudeDistance);
                    return dist + amplitude * MathF.Cos(waveFrequency * MathF.Acos(cos));
                }, d => d);
        }

        public static float[,] WorleyNoiseEdges(int x, int y, int width, int height, int seed, int cellWidth, int cellHeight, float cellJitter, float minDistance, float minDifThreshold, float maxDifThreshold, bool inverse, Func<Vector2, Vector2, float> norm, Func<float, float> normDeSquare)
        {
            var baseX = x * (width - 1);
            var baseY = y * (height - 1);


            // Add a cell on every side to account for a margin
            var gridMinX = (int)MathF.Floor((float)baseX / cellWidth) * cellWidth - cellWidth;
            var gridMinY = (int)MathF.Floor((float)baseY / cellHeight) * cellHeight - cellHeight;

            var gridMaxX = (int)MathF.Ceiling((float)(baseX + width) / cellWidth) * cellWidth + cellWidth;
            var gridMaxY = (int)MathF.Ceiling((float)(baseY + height) / cellHeight) * cellHeight + cellHeight;

            var gridWidth = ((gridMaxX - gridMinX) / cellWidth);
            var gridHeight = ((gridMaxY - gridMinY) / cellHeight);

            var gridPoints = new Vector2[gridWidth, gridHeight];

            for (var i = 0; i < gridWidth; i++)
                for (var e = 0; e < gridHeight; e++)
                {
                    var cellX = gridMinX + i * cellWidth;
                    var cellY = gridMinY + e * cellHeight;

                    var cellSeed = Utils.GetCombinedSeed(cellX, cellY, seed);
                    var cellRandom = new Random(cellSeed);

                    var xPos = (1 - cellJitter) * 0.5f + (float)cellRandom.NextDouble() * cellJitter;
                    var yPos = (1 - cellJitter) * 0.5f + (float)cellRandom.NextDouble() * cellJitter;

                    gridPoints[i, e] = new Vector2(cellX + xPos * cellWidth, cellY + yPos * cellHeight);
                }

            var result = new float[width, height];

            for (var i = 1; i < gridWidth - 1; i++)
                for (var e = 1; e < gridHeight - 1; e++)
                {
                    var cellX = gridMinX + i * cellWidth;
                    var cellY = gridMinY + e * cellHeight;

                    var startX = Math.Max(baseX, cellX);
                    var startY = Math.Max(baseY, cellY);

                    var stopX = Math.Min(baseX + width, cellX + cellWidth);
                    var stopY = Math.Min(baseY + height, cellY + cellHeight);

                    for (var pointX = startX; pointX < stopX; pointX++)
                        for (var pointY = startY; pointY < stopY; pointY++)
                        {
                            var closestDistanceSquared = -1f;
                            var secondClosestDistanceSquared = -1f;

                            var pointPos = new Vector2(pointX, pointY);

                            for (var h = -1; h <= 1; h++)
                                for (var v = -1; v <= 1; v++)
                                {
                                    var cellPointPos = gridPoints[i + h, e + v];

                                    var distSqr = norm(cellPointPos, pointPos);

                                    if (closestDistanceSquared < 0 || distSqr < closestDistanceSquared)
                                    {
                                        secondClosestDistanceSquared = closestDistanceSquared;
                                        closestDistanceSquared = distSqr;
                                    }
                                    else if (secondClosestDistanceSquared < 0 || distSqr < secondClosestDistanceSquared)
                                    {
                                        secondClosestDistanceSquared = distSqr;
                                    }
                                }

                            var dist = normDeSquare(closestDistanceSquared);
                            var secondDist = normDeSquare(secondClosestDistanceSquared);
                            var dif = MathF.Abs(secondDist - dist);

                            float value;

                            var difValue = (maxDifThreshold - dif) / (maxDifThreshold - minDifThreshold);
                            var distValue = (dist - minDistance) / (maxDifThreshold - minDifThreshold);
                            value = MathF.Min(distValue, difValue);
                            if (value >= 1f)
                                value = 1f;
                            else if (value <= 0f)
                                value = 0f;

                            if (inverse)
                                value = 1 - value;

                            value = Fade(value);

                            result[pointX - baseX, pointY - baseY] = value;
                        }
                }

            return result;
        }

    }
}

