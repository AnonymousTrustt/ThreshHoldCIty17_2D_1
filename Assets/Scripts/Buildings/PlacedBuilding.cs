using UnityEngine;

public class PlacedBuilding : MonoBehaviour
{
    public BuildingDefinition Definition { get; private set; }
    public Vector2Int GridPosition { get; private set; }
    public Vector2Int Footprint => Definition != null ? Definition.footprint : Vector2Int.one;

    public void Initialize(BuildingDefinition definition, Vector2Int gridPosition, IsometricGridManager gridManager)
    {
        Definition = definition;
        GridPosition = gridPosition;
        gameObject.name = definition != null ? definition.buildingName : "Placed Building";

        if (gridManager != null)
        {
            Apply2DPositionAndSorting(gridManager);
        }
    }

    public void Apply2DPositionAndSorting(IsometricGridManager gridManager)
    {
        transform.position = gridManager.GridToWorld(GridPosition);

        int sortingOffset = Definition != null ? Definition.sortingOrderOffset : 0;
        int sortingOrder = gridManager.GetSortingOrderForGridPosition(GridPosition, sortingOffset);
        SpriteSortingUtility.ApplySortingOrder(gameObject, sortingOrder);
    }
}
