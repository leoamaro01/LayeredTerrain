using LayeredTerrain;
using LayeredTerrain.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

namespace LayeredTerrainUnity
{
    public class LayeredTerrainGenerator : MonoBehaviour
    {
        [SerializeField]
        private LayeredTerrainFeature[] terrainFeatures;
        [SerializeField]
        private float unitSize, terrainHeight;

        [SerializeField]
        private int chunkSize, seed, cacheExpandMargin;
        [SerializeField]
        private bool generateSeed;
        [SerializeField]
        private LayeredTerrainFeature heightsFeature, vegetationFeature, waterFeature;
        [SerializeField]
        private float vegetationThreshold, treeScattering;

        [SerializeField]
        private Material terrainMaterial;
        [SerializeField]
        private GameObject[] treePrefabs;

        [SerializeField]
        private Transform followPlayer;
        [SerializeField]
        private int chunkViewDistance, chunksGeneratedPerFrame;

        private Generator terrainGenerator;

        private Thread chunkGenThread;
        private Vector2Int playerChunkPosition;
        private ConcurrentQueue<(int x, int y, float[,] heights, TreeInstance[] trees)> generatedChunks = new ConcurrentQueue<(int x, int y, float[,] heights, TreeInstance[] trees)>();
        private ConcurrentQueue<(int x, int y)> toEnableChunks = new ConcurrentQueue<(int x, int y)>();
        private ConcurrentQueue<(int x, int y)> toDisableChunks = new ConcurrentQueue<(int x, int y)>();

        private ConcurrentDictionary<(int x, int y), GameObject> enabledChunks = new ConcurrentDictionary<(int x, int y), GameObject>();
        private ConcurrentDictionary<(int x, int y), GameObject> disabledChunks = new ConcurrentDictionary<(int x, int y), GameObject>();

        private void Start()
        {
            if (generateSeed)
                seed = Guid.NewGuid().GetHashCode();

            var chunkGenerationOptions = new ChunkGenerationOptions(chunkSize,
                                                                    chunkSize,
                                                                    seed,
                                                                    chunkViewDistance * 2,
                                                                    chunkViewDistance * 2,
                                                                    cacheExpandMargin);

            var features = from fAsset in terrainFeatures
                           let layerOptions = from lAsset in fAsset.featureLayers
                                              let depFeatures = from depFeature in lAsset._dependencyFeatureAssets
                                                                select depFeature.featureName
                                              let depLayers = from depLayer in lAsset._dependencyLayerAssets
                                                              select depLayer.layerName
                                              select new LayerOptions(lAsset.LayerCompute,
                                                                      lAsset.layerName,
                                                                      depFeatures.ToArray(),
                                                                      depLayers.ToArray())
                           let blends = from bAsset in fAsset.layerBlends
                                        select (Feature.LayerBlendFunction)bAsset.BlendFunction
                           let modifiers = from mAsset in fAsset.layerModifiers
                                           select (Feature.LayerModifierFunction)mAsset.ModifyLayer
                           select new FeatureOptions(fAsset.featureName,
                                                     layerOptions.ToArray(),
                                                     blends.ToArray(),
                                                     modifiers.ToArray(),
                                                     chunkGenerationOptions);

            terrainGenerator = new Generator(features.ToArray());

            var roundedPlayerPos = followPlayer.position / (unitSize * chunkSize);
            playerChunkPosition = new Vector2Int(Mathf.RoundToInt(roundedPlayerPos.x), Mathf.RoundToInt(roundedPlayerPos.z));

            chunkGenThread = new Thread(GenerateChunksAroundPlayer);
            chunkGenThread.Start();

        }

        private void Update()
        {
            var roundedPlayerPos = followPlayer.position / (unitSize * chunkSize);
            playerChunkPosition = new Vector2Int(Mathf.RoundToInt(roundedPlayerPos.x), Mathf.RoundToInt(roundedPlayerPos.z));

            for (var i = 0; i < chunksGeneratedPerFrame; i++)
            {
                if (!generatedChunks.TryDequeue(out var chunk))
                    break;

                var terrainObj = new GameObject();
                terrainObj.transform.position = new Vector3(chunk.x * unitSize * chunkSize, 0, chunk.y * unitSize * chunkSize);
                var terrain = terrainObj.AddComponent<Terrain>();
                terrain.materialTemplate = terrainMaterial;
                terrain.drawInstanced = true;
                terrain.drawTreesAndFoliage = true;

                var size = unitSize * chunkSize;
                var terrainData = new TerrainData
                {
                    heightmapResolution = chunkSize,
                    size = new Vector3(size, terrainHeight, size),
                };

                terrainData.SetHeights(0, 0, chunk.heights);

                terrainData.treePrototypes = treePrefabs.Select(t => new TreePrototype()
                {
                    prefab = t
                }).ToArray();

                terrainData.SetTreeInstances(chunk.trees, true);

                terrain.terrainData = terrainData;
                terrain.allowAutoConnect = true;

                enabledChunks[(chunk.x, chunk.y)] = terrainObj;
            }

            while (toEnableChunks.TryDequeue(out var chunk))
            {
                disabledChunks.Remove(chunk, out var chunkGO);
                chunkGO.SetActive(true);
                enabledChunks[chunk] = chunkGO;
            }

            while (toDisableChunks.TryDequeue(out var chunk))
            {
                enabledChunks.Remove(chunk, out var chunkGO);
                chunkGO.SetActive(false);
                disabledChunks[chunk] = chunkGO;
            }
        }

        private void GenerateChunksAroundPlayer()
        {
            var generatedChunks = new HashSet<(int x, int y)>();
            Vector2Int? lastPlayerPos = null;

            while (true)
            {
                Thread.Sleep(100);
                if (lastPlayerPos.HasValue && playerChunkPosition == lastPlayerPos)
                    continue;

                lastPlayerPos = playerChunkPosition;

                var viewedChunks = new HashSet<(int x, int y)>();

                for (var x = -chunkViewDistance; x < chunkViewDistance; x++)
                    for (var y = -chunkViewDistance; y < chunkViewDistance; y++)
                    {
                        var chunkPos = playerChunkPosition + new Vector2Int(x, y);
                        var posTuple = (chunkPos.x, chunkPos.y);

                        viewedChunks.Add(posTuple);

                        if (generatedChunks.Contains(posTuple))
                        {
                            if (enabledChunks.ContainsKey(posTuple) || toEnableChunks.Contains(posTuple))
                                continue;
                            else if (disabledChunks.ContainsKey(posTuple))
                                toEnableChunks.Enqueue(posTuple);
                            // Chunk was generated but hasn't been added to enabled chunks list
                            else
                                continue;
                        }

                        new Thread(GenerateChunk).Start(new Tuple<int, int>(chunkPos.x, chunkPos.y));

                        generatedChunks.Add((chunkPos.x, chunkPos.y));
                    }

                // disable entries not currently in view
                foreach (var enabled in enabledChunks.Keys)
                {
                    if (viewedChunks.Contains(enabled))
                        continue;
                    toDisableChunks.Enqueue(enabled);
                }
            }
        }

        private void GenerateChunk(object chunkPos)
        {
            var paramTuple = (Tuple<int, int>)chunkPos;
            var x = paramTuple.Item1;
            var y = paramTuple.Item2;

            float[,] heights = new float[chunkSize, chunkSize];

            float[,] chunkHeights, vegetationData, waterData;

            var treeInstances = new List<TreeInstance>();

            lock (terrainGenerator)
            {
                chunkHeights = terrainGenerator[heightsFeature.featureName][x, y];
                vegetationData = terrainGenerator[vegetationFeature.featureName][x, y];
                waterData = terrainGenerator[waterFeature.featureName][x, y];
            }


            for (var e = 0; e < chunkSize; e++)
                for (var i = 0; i < chunkSize; i++)
                {
                    heights[e, i] = chunkHeights[i, e];
                }

            var treeCells = Mathf.FloorToInt(chunkSize / treeScattering);

            var treeUnit = 1f / treeCells;

            for (var i = 0; i < treeCells; i++)
                for (var e = 0; e < treeCells; e++)
                {
                    var chunkX = Mathf.RoundToInt(((float)i / treeCells) * chunkSize);
                    var chunkY = Mathf.RoundToInt(((float)e / treeCells) * chunkSize);

                    if (vegetationData[chunkX, chunkY] > vegetationThreshold &&
                        waterData[chunkX, chunkY] < 0.5f)
                    {
                        var treeRandomSeed = Utils.GetCombinedSeed(x * treeCells + i, y * treeCells + e, seed);
                        var treeRandom = new System.Random(treeRandomSeed);

                        var treePosOffset = new Vector2(((float)treeRandom.NextDouble() - 0.5f) * treeUnit * 0.25f, ((float)treeRandom.NextDouble() - 0.5f) * treeUnit * 0.25f);

                        var treePrototypeIndex = treeRandom.Next(treePrefabs.Length);

                        var treeScale = 1 + ((float)treeRandom.NextDouble() - 0.5f) * 0.5f;

                        var rotation = (float)treeRandom.NextDouble() * 2f * Mathf.PI;

                        var pos = new Vector3(treeUnit * 0.5f + treeUnit * i + treePosOffset.x, 0, treeUnit * 0.5f + treeUnit * e + treePosOffset.y);

                        var instance = new TreeInstance()
                        {
                            heightScale = treeScale,
                            widthScale = treeScale,
                            color = Color.white,
                            lightmapColor = Color.white,
                            position = pos,
                            prototypeIndex = treePrototypeIndex,
                            rotation = rotation
                        };

                        treeInstances.Add(instance);
                    }
                }

            generatedChunks.Enqueue((x, y, heights, treeInstances.ToArray()));
        }
    }
}
