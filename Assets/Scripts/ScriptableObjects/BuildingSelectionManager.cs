using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSelectionManager", menuName = "Singletons/BuildingSelectionManager")]
public class BuildingSelectionManager : ScriptableObject
{
    public GameObject BuildingPrefab => m_buildingPrefab;
    private GameObject m_buildingPrefab;

    public bool Rotate => m_rotate;
    private bool m_rotate;
}
