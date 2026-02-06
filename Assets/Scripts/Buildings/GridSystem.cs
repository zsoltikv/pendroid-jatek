using UnityEngine;
using UnityEngine.Tilemaps;

public class GridSystem : MonoBehaviour
{
    public static GridSystem instance;
    
    [Header("Grid Settings")]
    public int gridWidth = 50;
    public int gridHeight = 50;
    public float cellSize = 1f;
    public Vector2 gridOrigin = Vector2.zero;
    public Tilemap groundTilemap;
    
    [Header("Visual")]
    public bool showGrid = true;
    public Color gridColor = new Color(1, 1, 1, 0.2f);
    
    private bool[,] occupiedCells;
    private Building[,] buildingsOnGrid;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        InitializeGrid();
        if (groundTilemap == null)
        {
            groundTilemap = FindObjectOfType<Tilemap>();
        }
    }
    
    private void InitializeGrid()
    {
        occupiedCells = new bool[gridWidth, gridHeight];
        buildingsOnGrid = new Building[gridWidth, gridHeight];
    }
    
    public Vector3 GetWorldPosition(int x, int y)
    {
        return new Vector3(
            gridOrigin.x + x * cellSize,
            gridOrigin.y + y * cellSize,
            0
        );
    }
    
    public Vector2Int GetGridPosition(Vector3 worldPosition)
    {
        int x = Mathf.FloorToInt((worldPosition.x - gridOrigin.x) / cellSize);
        int y = Mathf.FloorToInt((worldPosition.y - gridOrigin.y) / cellSize);
        return new Vector2Int(x, y);
    }
    
    public bool IsPositionValid(int x, int y, int width, int height)
    {
        // Check if within bounds
        if (x < 0 || y < 0 || x + width > gridWidth || y + height > gridHeight)
            return false;
        
        // Check if cells are occupied
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (occupiedCells[x + dx, y + dy])
                    return false;
                
                // Check if all cells have grassland_center tile underneath
                Vector3 worldPos = GetWorldPosition(x + dx, y + dy);
                if (!IsGrasslandCenter(worldPos))
                    return false;
            }
        }
        
        return true;
    }
    
    public bool PlaceBuilding(Building building, int x, int y, int width, int height)
    {
        if (!IsPositionValid(x, y, width, height))
            return false;
        
        // Mark cells as occupied
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                occupiedCells[x + dx, y + dy] = true;
                buildingsOnGrid[x + dx, y + dy] = building;
            }
        }
        
        return true;
    }

    bool IsGrasslandCenter(Vector3 worldPos, string expectedName = "grassland_center")
    {
        if (groundTilemap == null) return false;

        Vector3Int cellPos = groundTilemap.WorldToCell(worldPos);
        var tile = groundTilemap.GetTile(cellPos);
        if (tile == null) return false;

        var data = new TileData();
        tile.GetTileData(cellPos, groundTilemap, ref data);

        if (data.sprite != null)
        {
            Debug.Log($"Tile sprite name at {cellPos}: {data.sprite.name}");
        }
        
        // Allow any grassland tile (center, left, right, top, bottom, etc.)
        return data.sprite != null && data.sprite.name.StartsWith("grassland");
    }
    
    public void RemoveBuilding(int x, int y, int width, int height)
    {
        for (int dx = 0; dx < width; dx++)
        {
            for (int dy = 0; dy < height; dy++)
            {
                if (x + dx < gridWidth && y + dy < gridHeight)
                {
                    occupiedCells[x + dx, y + dy] = false;
                    buildingsOnGrid[x + dx, y + dy] = null;
                }
            }
        }
    }
    
    public Building GetBuildingAt(int x, int y)
    {
        if (x < 0 || y < 0 || x >= gridWidth || y >= gridHeight)
            return null;
        
        return buildingsOnGrid[x, y];
    }
    
    private void OnDrawGizmos()
    {
        if (!showGrid) return;
        
        Gizmos.color = gridColor;
        
        // Draw vertical lines
        for (int x = 0; x <= gridWidth; x++)
        {
            Vector3 start = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y, 0);
            Vector3 end = new Vector3(gridOrigin.x + x * cellSize, gridOrigin.y + gridHeight * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
        
        // Draw horizontal lines
        for (int y = 0; y <= gridHeight; y++)
        {
            Vector3 start = new Vector3(gridOrigin.x, gridOrigin.y + y * cellSize, 0);
            Vector3 end = new Vector3(gridOrigin.x + gridWidth * cellSize, gridOrigin.y + y * cellSize, 0);
            Gizmos.DrawLine(start, end);
        }
    }
}
