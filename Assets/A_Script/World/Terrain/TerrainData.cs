using UnityEngine;

public class TerrainData : MonoBehaviour
{
    public static TerrainData Instance { get; private set; }
    public float[,] noiseMap;
    [SerializeField] float noiseFrequency = 1f;

    private void Start()
    {
        Instance = this;
        GenerateTerrain();
    }


    public void GenerateTerrain(bool isIsland = false)
    {
        int worldSize = World.Instance.WorldSize;

        noiseMap = new float[worldSize, worldSize];
        float scale = 0.05f;

        for (int y = 0; y < worldSize; y++)
            for (int x = 0; x < worldSize; x++)
            {
                float nx = x * scale;
                float ny = y * scale;

                float noise = FractalNoise(x * 0.01f, y * 0.01f, noiseFrequency);


                if (isIsland)
                {
                    float island = IslandMask(x, y, worldSize);
                    noiseMap[x, y] = noise * island;
                }
                else
                {
                    noiseMap[x, y] = noise;
                }
            }
    }

    float FractalNoise(float x, float y, float noiseFrequency)
    {
        float value = 0;
        float amplitude = 1;
        float maxValue = 0;
        float frequency = noiseFrequency;

        for (int i = 0; i < 4; i++)
        {
            value += Mathf.PerlinNoise(x * frequency, y * frequency) * amplitude;
            maxValue += amplitude;
            amplitude *= 2f;
            frequency *= .5f;
        }

        return value / maxValue;
    }
    float IslandMask(int x, int y, float worldSize)
    {
        float cx = worldSize / 2f;
        float cy = worldSize / 2f;

        float dx = (x - cx) / cx;
        float dy = (y - cy) / cy;

        float dist = Mathf.Sqrt(dx * dx + dy * dy);

        return Mathf.Clamp01(1f - dist);
    }

    public static TerrainType GetTerrain(float v)
    {
        if (v < 0.3f) return TerrainType.Water;
        else if (v < 0.5f) return TerrainType.Sand;
        else if (v < 0.7f) return TerrainType.Grass;
        else return TerrainType.Rocks;
    }
}
