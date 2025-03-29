
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System.Xml;
using System;
using Random = UnityEngine.Random; // Добавьте эту строку
public class geneshnmaps : MonoBehaviour

{
    [Header("Основные настройки")]
    [SerializeField] private TextAsset biomeConfigXML;
    [SerializeField] private ItemManager itemManager;
    [SerializeField] private ListImage listImage;
    [SerializeField] private GameObject worldParent;
    [SerializeField] private int chunkWidth = 16;
    [SerializeField] private int worldHeight = 128;
    [SerializeField] private int renderDistance = 3;

    [Header("Настройки игрока")]
    [SerializeField] private Transform player;

    private Dictionary<string, BiomeTemplate> biomes = new Dictionary<string, BiomeTemplate>();
    private HashSet<int> generatedChunks = new HashSet<int>();
    private int seed;
    private int lastPlayerChunkX;
    private int lastY;
    public void button()
    {
        LoadBiomeTemplates();
        Initialize();
        GenerateInitialChunks();
    }

    private void Update()
    {
        if (player != null)
        {
            CheckPlayerPosition();
        }
    }

    private void LoadBiomeTemplates()
    {
        if (biomeConfigXML == null)
        {
            Debug.LogError("Biome config XML not assigned!");
            return;
        }

        try
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(biomeConfigXML.text);

            foreach (XmlNode biomeNode in xmlDoc.SelectNodes("Biomes/Biome"))
            {
                string biomeName = biomeNode.Attributes["name"]?.Value;
                if (string.IsNullOrEmpty(biomeName)) continue;

                BiomeTemplate biome = new BiomeTemplate
                {
                    name = biomeName,
                    baseHeight = ParseIntAttribute(biomeNode, "baseHeight", 64),
                    heightVariation = ParseIntAttribute(biomeNode, "heightVariation", 5),
                    temperature = ParseIntAttribute(biomeNode, "temperature", 15),
                    caveChance = ParseFloatAttribute(biomeNode, "caveChance", 0.3f),
                    heightValue = ParseFloatAttribute(biomeNode, "heightValue", 0.5f),
                    smoothness = ParseFloatAttribute(biomeNode, "smoothness",0.1f),
                    layers = new List<LayerTemplate>(),
                    ores = new List<OreTemplate>()
                };

                foreach (XmlNode layerNode in biomeNode.SelectNodes("Layer"))
                {
                    biome.layers.Add(new LayerTemplate
                    {
                        blockName = layerNode.Attributes["block"]?.Value ?? "stone",
                        thickness = ParseIntAttribute(layerNode, "thickness", 1),
                        isSurface = ParseBoolAttribute(layerNode, "isSurface", false)
                    });
                }

                foreach (XmlNode oreNode in biomeNode.SelectNodes("Ores/Ore"))
                {
                    biome.ores.Add(new OreTemplate
                    {
                        blockName = oreNode.Attributes["block"]?.Value ?? "stone",
                        frequency = ParseFloatAttribute(oreNode, "frequency", 0.1f),
                        threshold = ParseFloatAttribute(oreNode, "threshold", 0.7f),
                        minDepth = ParseIntAttribute(oreNode, "minDepth", 10),
                        maxDepth = ParseIntAttribute(oreNode, "maxDepth", worldHeight),
                        veinFrequency = ParseFloatAttribute(oreNode, "veinFrequency", 0.1f)
                    });
                }

                if (!biomes.ContainsKey(biomeName))
                {
                    biomes.Add(biomeName, biome);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading biomes: {e.Message}");
        }

        if (biomes.Count == 0)
        {
            biomes.Add("Default", GetDefaultBiome());
        }
    }

    private void Initialize()
    {
        seed = Random.Range(-100000, 100000);
        NameConverter._saveList = itemManager;
        generatedChunks.Clear();
        lastPlayerChunkX = int.MinValue;
    }

    private void GenerateInitialChunks()
    {
        GenerateChunk(0); // Центральный чанк

        for (int i = 1; i <= renderDistance; i++)
        {
            GenerateChunk(i * chunkWidth);
            GenerateChunk(-i * chunkWidth);
        }
    }

    private void CheckPlayerPosition()
    {
        int currentChunkX = Mathf.FloorToInt(player.position.x / chunkWidth) * chunkWidth;

        if (currentChunkX != lastPlayerChunkX)
        {
            lastPlayerChunkX = currentChunkX;
            GenerateSurroundingChunks(currentChunkX);
        }
    }

    private void GenerateSurroundingChunks(int centerChunkX)
    {
        for (int i = -renderDistance; i <= renderDistance; i++)
        {
            int chunkX = centerChunkX + i * chunkWidth;
            if (!generatedChunks.Contains(chunkX))
            {
                GenerateChunk(chunkX);
            }
        }
    }

    private void GenerateChunk(int chunkX)
    {
        if (generatedChunks.Contains(chunkX)) return;

        GameObject chunk = new GameObject($"Chunk_{chunkX}");
        chunk.transform.position = new Vector3(chunkX, 0, 0);
        chunk.transform.SetParent(worldParent.transform);

        BiomeTemplate biome = SelectBiomeForChunk(chunkX);
        GenerateTerrain(chunk, biome);

        generatedChunks.Add(chunkX);
    }

    private BiomeTemplate SelectBiomeForChunk(int chunkX)
    {
        float biomeValue = Mathf.PerlinNoise((chunkX + seed) * 0.1f, seed * 0.1f);

        if (biomeValue < 0.3f && biomes.ContainsKey("Plains"))
            return biomes["Plains"];
        if (biomeValue < 0.6f && biomes.ContainsKey("Forest"))
            return biomes["Forest"];
        if (biomes.ContainsKey("Mountains"))
            return biomes["Mountains"];

        return biomes["Default"];
    }

    private void GenerateTerrain(GameObject chunk, BiomeTemplate biome)
    {
        int chunkX = Mathf.FloorToInt(chunk.transform.position.x);

        for (int x = 0; x < chunkWidth; x++)
        {
            int globalX = chunkX + x;

            // Основной шум для высоты поверхности
            //float surfaceNoise = Mathf.PerlinNoise((globalX + seed) * biome.smoothness, seed / biome.heightValue);
            //int surfaceHeight = biome.baseHeight + Mathf.RoundToInt(biome.heightVariation * surfaceNoise);
            int surfaceHeight = biome.baseHeight + Mathf.RoundToInt(biome.heightValue * Mathf.PerlinNoise(globalX / biome.smoothness + seed, 0));
            if (surfaceHeight == 15)
            {
                lastY= surfaceHeight;
            }
            GenerateLayers(chunk, x, surfaceHeight, biome);
            GenerateCavesAndOres(chunk, x, surfaceHeight, globalX, biome);
        }
    }
    private void GenerateSmoothMountain(GameObject chunk, int startX, int peakX, int endX, int baseHeight, int peakHeight)
    {
        int chunkX = Mathf.FloorToInt(chunk.transform.position.x);

        for (int x = startX; x <= endX; x++)
        {
            int globalX = chunkX + x;
            int surfaceHeight;

            // Подъем до пика
            if (x <= peakX)
            {
                float t = (float)(x - startX) / (peakX - startX);
                surfaceHeight = Mathf.RoundToInt(Mathf.Lerp(baseHeight, peakHeight, t));
            }
            // Спуск после пика
            else
            {
                float t = (float)(x - peakX) / (endX - peakX);
                surfaceHeight = Mathf.RoundToInt(Mathf.Lerp(peakHeight, baseHeight, t));
            }

            // Сохраняем последнюю высоту для плавных переходов
            if (x == endX)
            {
                lastY = surfaceHeight;
            }

            GenerateLayers(chunk, x, surfaceHeight, biomes["Mountains"]);
        }
    }
    private void GenerateLayers(GameObject chunk, int x, int surfaceHeight, BiomeTemplate biome)
    {
        int currentY = surfaceHeight;

        foreach (LayerTemplate layer in biome.layers)
        {
            int thickness = layer.isSurface ? 1 : layer.thickness;

            for (int y = currentY; y > currentY - thickness && y >= 0; y--)
            {
                // Добавляем небольшую вариативность в границы слоёв
                if (y == currentY && Random.value < 0.3f && !layer.isSurface)
                    continue;

                CreateBlock(chunk, layer.blockName, x, y);
            }

            currentY -= thickness;
        }
    }
    private void GenerateCavesAndOres(GameObject chunk, int x, int surfaceHeight, int globalX, BiomeTemplate biome)
    {
        // Генерируем текстуру пещер для этого столбца
        Texture2D caveMap = manImage(0.1f, 0.5f, worldHeight);
        Texture2D oreMap = manImage(0.2f, 0.8f, worldHeight);

        int chunkX = Mathf.FloorToInt(chunk.transform.position.x);
        int localX = globalX - chunkX;

        for (int y = surfaceHeight - 3; y > 0; y--)
        {
            // Проверяем пиксель в текстуре пещер
            Color cavePixel = caveMap.GetPixel(localX, y);

            // Если пиксель черный - это пещера
            if (cavePixel == Color.black)
            {
                CreateBlock(chunk, "air", x, y);

                // Проверяем текстуру руд для этого места
                Color orePixel = oreMap.GetPixel(localX, y);

                // Если в текстуре руд черный пиксель - генерируем руду
                if (orePixel == Color.black)
                {
                    TryGenerateOreVein(chunk, x, y, globalX, biome);
                }
            }
            // Случайная генерация руд вне пещер
            else if (Random.value < 0.005f)
            {
                TryGenerateOreVein(chunk, x, y, globalX, biome);
            }
            else
            {
                CreateBlock(chunk, "stone", x, y);
            }
        }
    }

    public Texture2D manImage(float frequency, float threshold, int height)
    {
        Texture2D imageChunk = new Texture2D(chunkWidth, height);
        int SeedE = seed; // Используем общий seed мира для согласованности

        for (int x = 0; x < imageChunk.width; x++)
        {
            for (int y = 0; y < imageChunk.height; y++)
            {
                float v = Mathf.PerlinNoise(x * frequency + SeedE, y * frequency + SeedE);
                imageChunk.SetPixel(x, y, v > threshold ? Color.black : Color.white);
            }
        }
        imageChunk.Apply();
        return imageChunk;
    }

    private void TryGenerateOreVein(GameObject chunk, int x, int y, int globalX, BiomeTemplate biome)
    {
        foreach (OreTemplate ore in biome.ores)
        {
            // Проверяем глубину залегания руды
            if (y < ore.minDepth || y > ore.maxDepth) continue;

            // Шум для распределения руды
            float oreNoise = Mathf.PerlinNoise(
                (globalX + seed) * ore.veinFrequency,
                (y + seed) * ore.veinFrequency
            );

            if (oreNoise > ore.threshold)
            {
                GenerateOreCluster(chunk, x, y, ore.blockName);
                break;
            }
        }
    }

    private void GenerateOreCluster(GameObject chunk, int x, int y, string blockName)
    {
        int veinSize = Random.Range(3, 7);
        for (int i = 0; i < veinSize; i++)
        {
            int offsetX = Mathf.Clamp(x + Random.Range(-1, 2), 0, chunkWidth - 1);
            int offsetY = Mathf.Max(y + Random.Range(-1, 2), 0);

            CreateBlock(chunk, blockName, offsetX, offsetY);
        }
    }

    private void CreateBlock(GameObject chunk, string blockName, int x, int y)
    {
        if (blockName == "air") return;

        int blockId = NameConverter.FindItemIndexByName(blockName);
        if (blockId == -1) return;

        GameObject block = new GameObject(blockName);
        block.transform.SetParent(chunk.transform);
        block.transform.position = new Vector3(chunk.transform.position.x + x, y, 0);

        var renderer = block.AddComponent<SpriteRenderer>();
        renderer.sprite = listImage.imageL[blockId];

        block.AddComponent<BoxCollider2D>();
    }

    private BiomeTemplate GetDefaultBiome()
    {
        return new BiomeTemplate
        {
            name = "Default",
            baseHeight = 64,
            heightVariation = 5,
            temperature = 15,
            caveChance = 0.3f,
            layers = new List<LayerTemplate>
            {
                new LayerTemplate { blockName = "grass", thickness = 1, isSurface = true },
                new LayerTemplate { blockName = "dirt", thickness = 3, isSurface = false },
                new LayerTemplate { blockName = "stone", thickness = 60, isSurface = false }
            },
            ores = new List<OreTemplate>
            {
                new OreTemplate {
                    blockName = "coal_ore",
                    frequency = 0.1f,
                    threshold = 0.7f,
                    minDepth = 10,
                    maxDepth = 128,
                    veinFrequency = 0.1f
                }
            }
        };
    }

    private int ParseIntAttribute(XmlNode node, string attrName, int defaultValue)
    {
        if (node.Attributes[attrName] == null || !int.TryParse(node.Attributes[attrName].Value, out int result))
            return defaultValue;
        return result;
    }

    private float ParseFloatAttribute(XmlNode node, string attrName, float defaultValue)
    {
        if (node.Attributes[attrName] == null || !float.TryParse(node.Attributes[attrName].Value,
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out float result))
            return defaultValue;
        return result;
    }

    private bool ParseBoolAttribute(XmlNode node, string attrName, bool defaultValue)
    {
        if (node.Attributes[attrName] == null || !bool.TryParse(node.Attributes[attrName].Value, out bool result))
            return defaultValue;
        return result;
    }
}

[System.Serializable]
public class BiomeTemplate
{
    public string name;
    public int baseHeight;
    public int heightVariation;
    public int temperature;
    public float caveChance;
    public float heightValue;
    public float smoothness;
    public List<LayerTemplate> layers = new List<LayerTemplate>();
    public List<OreTemplate> ores = new List<OreTemplate>();
}

[System.Serializable]
public class LayerTemplate
{
    public string blockName;
    public int thickness;
    public bool isSurface;
}

[System.Serializable]
public class OreTemplate
{
    public string blockName;
    public float frequency;
    public float threshold;
    public int minDepth;
    public int maxDepth;
    public float veinFrequency;
}