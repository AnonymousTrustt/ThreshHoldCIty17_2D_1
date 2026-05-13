using System.Collections.Generic;
using UnityEngine;

public class IsometricGridManager : MonoBehaviour
{
    [Header("2D Isometric Cell Size")]
    [Tooltip("World-space width of one diamond tile. For a 128x64 tile at 100 PPU, use 1.28.")]
    [SerializeField] private float cellWidth = 1.28f;

    [Tooltip("World-space height of one diamond tile. For a 128x64 tile at 100 PPU, use 0.64.")]
    [SerializeField] private float cellHeight = 0.64f;

    [Tooltip("Optional origin. If empty, this object's transform position is used.")]
    [SerializeField] private Transform gridOrigin;

    [Header("2D Sprite Depth")]
    [Tooltip("Base Z used for placed tiles/buildings. Keep this near 0 for a 2D orthographic camera.")]
    [SerializeField] private float baseZ = 0f;

    [Tooltip("Small Z offset per isometric row. This is only for depth layering, not gameplay movement.")]
    [SerializeField] private float zStepPerGridRow = 0.001f;

    [Tooltip("Multiplier used to convert grid position into SpriteRenderer sortingOrder.")]
    [SerializeField] private int sortingOrderStep = 10;

    [Header("Debug")]
    [SerializeField] private bool drawDebugGrid = true;
    [SerializeField] private int debugGridRadius = 10;
    [SerializeField] private Color debugGridColor = new Color(0f, 1f, 1f, 0.35f);

    private readonly Dictionary<Vector2Int, PlacedBuilding> occupiedCells = new Dictionary<Vector2Int, PlacedBuilding>();

    public float CellWidth => cellWidth;
    public float CellHeight => cellHeight;
    public float BaseZ => baseZ;

    public Vector2 GridToWorld2D(Vector2Int gridPosition)
    {
        return GridToWorld2D(gridPosition.x, gridPosition.y);
    }

    public Vector2 GridToWorld2D(int gridX, int gridY)
    {
        Vector2 origin = GetOrigin2D();

        float worldX = (gridX - gridY) * cellWidth * 0.5f;
        float worldY = (gridX + gridY) * cellHeight * 0.5f;

        return origin + new Vector2(worldX, worldY);
    }

    public Vector3 GridToWorld(Vector2Int gridPosition)
    {
        return GridToWorld(gridPosition.x, gridPosition.y);
    }

    public Vector3 GridToWorld(int gridX, int gridY)
    {
        Vector2 world2D = GridToWorld2D(gridX, gridY);
        return new Vector3(world2D.x, world2D.y, GetZDepthForGridPosition(new Vector2Int(gridX, gridY)));
    }

    public Vector2Int WorldToGrid(Vector2 worldPosition)
    {
        Vector2 origin = GetOrigin2D();
        Vector2 localPosition = worldPosition - origin;

        float halfWidth = cellWidth * 0.5f;
        float halfHeight = cellHeight * 0.5f;

        float projectedX = localPosition.x / halfWidth;
        float projectedY = localPosition.y / halfHeight;

        int gridX = Mathf.RoundToInt((projectedX + projectedY) * 0.5f);
        int gridY = Mathf.RoundToInt((projectedY - projectedX) * 0.5f);

        return new Vector2Int(gridX, gridY);
    }

    public Vector2Int WorldToGrid(Vector3 worldPosition)
    {
        return WorldToGrid(new Vector2(worldPosition.x, worldPosition.y));
    }

    public Vector3 SnapWorldToGrid(Vector3 worldPosition)
    {
        return GridToWorld(WorldToGrid(worldPosition));
    }

    public float GetZDepthForGridPosition(Vector2Int gridPosition)
    {
        return baseZ + (gridPosition.x + gridPosition.y) * zStepPerGridRow;
    }

    public int GetSortingOrderForGridPosition(Vector2Int gridPosition, int extraOffset = 0)
    {
        return -(gridPosition.x + gridPosition.y) * sortingOrderStep + extraOffset;
    }

    public bool IsCellOccupied(Vector2Int gridPosition)
    {
        return occupiedCells.ContainsKey(gridPosition);
    }

    public bool CanPlaceBuilding(Vector2Int originCell, Vector2Int footprint)
    {
        foreach (Vector2Int cell in GetCellsInFootprint(originCell, footprint))
        {
            if (IsCellOccupied(cell))
            {
                return false;
            }
        }

        return true;
    }

    public void MarkCellsOccupied(Vector2Int originCell, Vector2Int footprint, PlacedBuilding building)
    {
        foreach (Vector2Int cell in GetCellsInFootprint(originCell, footprint))
        {
            occupiedCells[cell] = building;
        }
    }

    public void ClearCells(Vector2Int originCell, Vector2Int footprint)
    {
        foreach (Vector2Int cell in GetCellsInFootprint(originCell, footprint))
        {
            occupiedCells.Remove(cell);
        }
    }

    public List<Vector2Int> GetCellsInFootprint(Vector2Int originCell, Vector2Int footprint)
    {
        List<Vector2Int> cells = new List<Vector2Int>();
        int width = Mathf.Max(1, footprint.x);
        int height = Mathf.Max(1, footprint.y);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                cells.Add(originCell + new Vector2Int(x, y));
            }
        }

        return cells;
    }

    public List<PlacedBuilding> GetPlacedBuildings()
    {
        HashSet<PlacedBuilding> uniqueBuildings = new HashSet<PlacedBuilding>(occupiedCells.Values);
        return new List<PlacedBuilding>(uniqueBuildings);
    }

    public void ClearAllPlacedBuildings(bool destroyGameObjects)
    {
        List<PlacedBuilding> buildings = GetPlacedBuildings();
        occupiedCells.Clear();

        if (!destroyGameObjects)
        {
            return;
        }

        foreach (PlacedBuilding building in buildings)
        {
            if (building != null)
            {
                Destroy(building.gameObject);
            }
        }
    }

    private Vector2 GetOrigin2D()
    {
        Vector3 origin = gridOrigin != null ? gridOrigin.position : transform.position;
        return new Vector2(origin.x, origin.y);
    }

    private void OnDrawGizmos()
    {
        if (!drawDebugGrid)
        {
            return;
        }

        Gizmos.color = debugGridColor;

        for (int x = -debugGridRadius; x <= debugGridRadius; x++)
        {
            for (int y = -debugGridRadius; y <= debugGridRadius; y++)
            {
                DrawDiamond(GridToWorld(x, y));
            }
        }
    }

    private void DrawDiamond(Vector3 center)
    {
        Vector3 left = center + new Vector3(-cellWidth * 0.5f, 0f, 0f);
        Vector3 right = center + new Vector3(cellWidth * 0.5f, 0f, 0f);
        Vector3 top = center + new Vector3(0f, cellHeight * 0.5f, 0f);
        Vector3 bottom = center + new Vector3(0f, -cellHeight * 0.5f, 0f);

        Gizmos.DrawLine(left, top);
        Gizmos.DrawLine(top, right);
        Gizmos.DrawLine(right, bottom);
        Gizmos.DrawLine(bottom, left);
    }
}
