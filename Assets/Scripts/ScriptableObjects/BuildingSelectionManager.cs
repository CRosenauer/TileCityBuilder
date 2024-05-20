using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSelectionManager", menuName = "Singletons/BuildingSelectionManager")]
public class BuildingSelectionManager : ScriptableObject
{
    public GameObject BuildingPrefab { get; set; }
    public bool Rotate { get; set; }
}
