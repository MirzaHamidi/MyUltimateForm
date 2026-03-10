using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class NoiseMapGeneratorMesh : MonoBehaviour
{
    [Header("Collision")]
    public bool[,] walkable;

    [Header("Plane Reference")]
    public GameObject plane;

    [Header("Map Size (cells)")]
    public int width = 64;
    public int height = 64;

    [Header("Mesh Height")]
    public float yOffset = 0.05f;

    [Header("Noise")]
    public bool randomizeSeedOnPlay = true;
    public int seed;
    public float scale = 25f;
    public Vector2 offset;

    [Header("Thresholds")]
    [Range(0f, 1f)] public float waterThreshold = 0.40f;
    [Range(0f, 1f)] public float dirtThreshold = 0.75f;

    [Header("Atlas Layout")]
    public int atlasColumns = 3;
    public int atlasRows = 1;

    [Tooltip("Atlas index: 0 = grass")]
    public int grassTileIndex = 0;

    [Tooltip("Atlas index: 1 = dirt")]
    public int dirtTileIndex = 1;

    [Tooltip("Atlas index: 2 = water")]
    public int waterTileIndex = 2;

    [Header("Player Spawn")]
    public Transform player;
    public int spawnSeedOffset = 999;

    private Bounds planeBounds;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();

        meshCollider = GetComponent<MeshCollider>();
        if (meshCollider == null)
            meshCollider = gameObject.AddComponent<MeshCollider>();

        if (!plane)
        {
            Debug.LogError("Plane atanmadı.");
            return;
        }

        if (randomizeSeedOnPlay)
            seed = Random.Range(int.MinValue, int.MaxValue);

        CachePlaneBounds();
        Generate();

        if (player != null)
        {
            Vector2Int candidate = GetRandomCell(spawnSeedOffset);
            Vector2Int safe = GetNearestWalkableCell(candidate);

            Vector3 spawnPos = CellToWorld(safe);
            player.position = spawnPos;
        }
    }

    void CachePlaneBounds()
    {
        Renderer r = plane.GetComponent<Renderer>();
        if (r == null)
        {
            Debug.LogError("Plane üzerinde Renderer yok.");
            return;
        }

        planeBounds = r.bounds;
    }

    [ContextMenu("Generate")]
    public void Generate()
    {
        if (!plane)
        {
            Debug.LogError("Plane atanmadı.");
            return;
        }

        if (scale <= 0.001f)
            scale = 0.001f;

        CachePlaneBounds();

        walkable = new bool[width, height];

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        System.Random rng = new System.Random(seed);
        float ox = rng.Next(-100000, 100000) + offset.x;
        float oy = rng.Next(-100000, 100000) + offset.y;

        float cellSizeX = planeBounds.size.x / width;
        float cellSizeZ = planeBounds.size.z / height;

        float startX = planeBounds.min.x;
        float startZ = planeBounds.min.z;
        float fixedY = planeBounds.max.y + yOffset;

        int vertexIndex = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float nx = (x + ox) / scale;
                float ny = (y + oy) / scale;
                float n = Mathf.PerlinNoise(nx, ny);

                bool isWater = n < waterThreshold;
                walkable[x, y] = !isWater;

                int tileIndex = PickTileIndex(n);

                float x0 = startX + x * cellSizeX;
                float x1 = x0 + cellSizeX;
                float z0 = startZ + y * cellSizeZ;
                float z1 = z0 + cellSizeZ;

                vertices.Add(new Vector3(x0, fixedY, z0)); // bottom-left
                vertices.Add(new Vector3(x1, fixedY, z0)); // bottom-right
                vertices.Add(new Vector3(x0, fixedY, z1)); // top-left
                vertices.Add(new Vector3(x1, fixedY, z1)); // top-right

                triangles.Add(vertexIndex + 0);
                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 1);

                triangles.Add(vertexIndex + 2);
                triangles.Add(vertexIndex + 3);
                triangles.Add(vertexIndex + 1);

                AddTileUVs(uvs, tileIndex);

                vertexIndex += 4;
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "ProceduralNoiseMesh";

        if (vertices.Count > 65000)
            mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetUVs(0, uvs);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = mesh;
    }

    void AddTileUVs(List<Vector2> uvs, int tileIndex)
    {
        float tileWidth = 1f / atlasColumns;
        float tileHeight = 1f / atlasRows;

        int col = tileIndex % atlasColumns;
        int row = tileIndex / atlasColumns;

        float uMin = col * tileWidth;
        float uMax = uMin + tileWidth;

        float vMax = 1f - (row * tileHeight);
        float vMin = vMax - tileHeight;

        uvs.Add(new Vector2(uMin, vMin)); // bottom-left
        uvs.Add(new Vector2(uMax, vMin)); // bottom-right
        uvs.Add(new Vector2(uMin, vMax)); // top-left
        uvs.Add(new Vector2(uMax, vMax)); // top-right
    }

    int PickTileIndex(float n)
    {
        if (n < waterThreshold) return waterTileIndex;
        if (n >= dirtThreshold) return dirtTileIndex;
        return grassTileIndex;
    }

    public Vector2Int GetRandomCell(int seedOffset = 0)
    {
        System.Random rng = new System.Random(seed + seedOffset);
        return new Vector2Int(rng.Next(0, width), rng.Next(0, height));
    }

    public Vector2Int GetNearestWalkableCell(Vector2Int start)
    {
        if (IsWalkable(start))
            return start;

        int maxR = Mathf.Max(width, height);

        for (int r = 1; r < maxR; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                Vector2Int c1 = new Vector2Int(start.x + dx, start.y + r);
                if (IsWalkable(c1)) return c1;

                Vector2Int c2 = new Vector2Int(start.x + dx, start.y - r);
                if (IsWalkable(c2)) return c2;
            }

            for (int dy = -r + 1; dy <= r - 1; dy++)
            {
                Vector2Int c3 = new Vector2Int(start.x + r, start.y + dy);
                if (IsWalkable(c3)) return c3;

                Vector2Int c4 = new Vector2Int(start.x - r, start.y + dy);
                if (IsWalkable(c4)) return c4;
            }
        }

        return new Vector2Int(width / 2, height / 2);
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        float cellSizeX = planeBounds.size.x / width;
        float cellSizeZ = planeBounds.size.z / height;

        float startX = planeBounds.min.x + cellSizeX * 0.5f;
        float startZ = planeBounds.min.z + cellSizeZ * 0.5f;
        float fixedY = planeBounds.max.y + yOffset;

        float worldX = startX + cell.x * cellSizeX;
        float worldZ = startZ + cell.y * cellSizeZ;

        return new Vector3(worldX, fixedY, worldZ);
    }

    public Vector2Int WorldToCell(Vector3 worldPos)
    {
        float normalizedX = Mathf.InverseLerp(planeBounds.min.x, planeBounds.max.x, worldPos.x);
        float normalizedZ = Mathf.InverseLerp(planeBounds.min.z, planeBounds.max.z, worldPos.z);

        int x = Mathf.Clamp(Mathf.FloorToInt(normalizedX * width), 0, width - 1);
        int y = Mathf.Clamp(Mathf.FloorToInt(normalizedZ * height), 0, height - 1);

        return new Vector2Int(x, y);
    }

    bool IsInsideMap(Vector2Int c)
    {
        return c.x >= 0 && c.y >= 0 && c.x < width && c.y < height;
    }

    bool IsWalkable(Vector2Int c)
    {
        return IsInsideMap(c) && walkable != null && walkable[c.x, c.y];
    }
}