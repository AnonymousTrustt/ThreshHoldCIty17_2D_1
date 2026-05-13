using UnityEngine;

[CreateAssetMenu(fileName = "BuildingDefinition", menuName = "Threshold City 17/Building Definition")]
public class BuildingDefinition : ScriptableObject
{
    [Header("Identity")]
    public string buildingId = "building_id";
    public string buildingName = "New Building";
    public BuildingSector sectorType;

    [Header("2D Sprite Prefab")]
    public GameObject prefab;
    public Vector2Int footprint = Vector2Int.one;

    [Tooltip("Extra sorting offset for tall sprites that need to render above or below their base tile.")]
    public int sortingOrderOffset;

    [Header("Cost")]
    public int cost = 100;

    [Header("Resource Effects When Placed")]
    public int energyChange;
    public int pollutionChange;
    public int happinessChange;
}
