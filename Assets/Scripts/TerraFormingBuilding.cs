using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TerraformingBuilding : Building
{
    [Header("Terraforming Settings")]
    public int terraformRadius = 2;
    public TileBase grassTile;
    public Tilemap groundTilemap;

    private bool hasTransformed = false;

    private void Start()
    {
        if (groundTilemap == null)
        {
            groundTilemap = GameObject.FindObjectsByType<Tilemap>(FindObjectsSortMode.None).FirstOrDefault();
        }

        if (grassTile == null)
        {
            grassTile = Resources.Load<TileBase>("Tiles/Grass");
        }
    }

    private void Update()
    {
        if (isConstructed && !hasTransformed)
        {
            TransformTerrain();
            hasTransformed = true;
        }
    }

    private void TransformTerrain()
    {
        if (groundTilemap == null || grassTile == null)
        {
            Debug.LogWarning("Tilemap vagy Grass tile nincs beállítva!");
            return;
        }

        Vector3 buildingPos = transform.position;

        Vector3Int centerCell = groundTilemap.WorldToCell(buildingPos);

        Debug.Log($"Terraforming started at {centerCell}");

        for (int x = -terraformRadius; x <= terraformRadius; x++)
        {
            for (int y = -terraformRadius; y <= terraformRadius; y++)
            {
                Vector3Int targetCell = new Vector3Int(centerCell.x + x, centerCell.y + y, 0);

                TileBase existingTile = groundTilemap.GetTile(targetCell);

                if (existingTile != null)
                {
                    if (!IsGrassTile(existingTile, targetCell))
                    {
                        groundTilemap.SetTile(targetCell, grassTile);
                        Debug.Log($"Transformed tile at {targetCell} to Grass");
                    }
                }
            }
        }

        Debug.Log("Terraforming complete!");
    }

    private bool IsGrassTile(TileBase tile, Vector3Int position)
    {
        if (tile == null) return false;

        var data = new TileData();
        tile.GetTileData(position, groundTilemap, ref data);

        if (data.sprite != null)
        {
            return data.sprite.name.ToLower().Contains("grass");
        }

        return false;
    }

    private void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = new Color(0, 1, 0, 0.3f);

        float size = (terraformRadius * 2 + 1) * 1f;
        Gizmos.DrawCube(transform.position, new Vector3(size, size, 0.1f));
    }
}