using System.Collections.Generic;
using UnityEngine;

// todo: seperate UI from available building logic
// maybe this should be in BuildingSelectionManager and we just call methods on it?
public class GameUI : MonoBehaviour
{
    private const int k_availableBuildingsCount = 7;

    // todo: score

    [SerializeField]
    private BuildingSelectionManager m_buildingSelectionManager;

    [SerializeField]
    List<GameObject> m_buildingPrefabs;

    private void Awake()
    {
        RepopulateAvailableBuildings();
    }

    private void Update()
    {
        if(Input.GetButtonDown("RotateBuilding"))
        {
            m_buildingSelectionManager.Rotate = !m_buildingSelectionManager.Rotate;
        }
    }

    private void OnGUI()
    {
        {
            GUILayout.BeginVertical();

            int selection = DrawAvailableBuildingButtons();

            if(selection != m_buildingSelection)
            {
                m_buildingSelection = selection;
                OnBuildingSelected(m_buildingSelection);
            }

            GUILayout.EndVertical();
        }
    }

    private void RepopulateAvailableBuildings()
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

    private void OnBuildingSelected(int index)
    {
        m_buildingSelectionManager.BuildingPrefab = m_availableBuildings[index];
    }

    string[] GetAvailableBuildingNames()
    {
        string[] names = new string[k_availableBuildingsCount];

        for(int i = 0; i < k_availableBuildingsCount; ++i)
        {
            names[i] = m_availableBuildings[i].name;
        }

        return names;
    }

    private void OnPlaceBuilding()
    {
        RepopulateAvailableBuildings();
    }

    private int DrawAvailableBuildingButtons()
    {
        return GUILayout.SelectionGrid(m_buildingSelection, GetAvailableBuildingNames(), k_availableBuildingsCount);
    }

    int m_buildingSelection;

    GameObject[] m_availableBuildings = new GameObject[k_availableBuildingsCount];
}
