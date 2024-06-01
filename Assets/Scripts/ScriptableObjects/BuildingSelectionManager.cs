using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSelectionManager", menuName = "Singletons/BuildingSelectionManager")]
public class BuildingSelectionManager : ScriptableObject
{
    public int AvailableBuildingsCount => k_availableBuildingsCount;
    private const int k_availableBuildingsCount = 5;

    [SerializeField]
    List<GameObject> m_buildingPrefabs;

    public GameObject BuildingPrefab { get; set; }
    public bool Rotate { get; set; }
    
    public GameObject[] AvailableBuildings { get { return m_availableBuildings; } }

    public void RepopulateAvailableBuildings()
    {
        List<GameObject> buildingPrefabs = new();
        buildingPrefabs.AddRange(m_buildingPrefabs);

        for (int i = 0; i < k_availableBuildingsCount; ++i)
        {
            int buildingIndex = Random.Range(0, buildingPrefabs.Count);
            m_availableBuildings[i] = buildingPrefabs[buildingIndex];
            buildingPrefabs.RemoveAt(buildingIndex);
        }
    }

    GameObject[] m_availableBuildings = new GameObject[k_availableBuildingsCount];
}
