using System.Collections.Generic;
using UnityEngine;

// todo: seperate UI from available building logic
// maybe this should be in BuildingSelectionManager and we just call methods on it?
public class GameUI : MonoBehaviour
{
    [SerializeField]
    private BuildingSelectionManager m_buildingSelectionManager;

    [SerializeField]
    private ScoreManager m_scoreManager;

    private void Update()
    {
        if(Input.GetButtonDown("RotateBuilding"))
        {
            m_buildingSelectionManager.Rotate = !m_buildingSelectionManager.Rotate;
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                int selection = DrawAvailableBuildingButtons();

                if (selection != m_buildingSelection)
                {
                    m_buildingSelection = selection;
                    OnBuildingSelected(m_buildingSelection);
                }
            }
            GUILayout.EndVertical();

            GUILayout.BeginVertical();
            {
                GUILayout.Label($"Score {m_scoreManager.Score}");
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }

    private void OnBuildingSelected(int index)
    {
        m_buildingSelectionManager.BuildingPrefab = m_buildingSelectionManager.AvailableBuildings[index];
    }

    string[] GetAvailableBuildingNames()
    {
        string[] names = new string[m_buildingSelectionManager.AvailableBuildingsCount];

        for(int i = 0; i < m_buildingSelectionManager.AvailableBuildingsCount; ++i)
        {
            names[i] = m_buildingSelectionManager.AvailableBuildings[i].name;
        }

        return names;
    }

    private int DrawAvailableBuildingButtons()
    {
        return GUILayout.SelectionGrid(m_buildingSelection, GetAvailableBuildingNames(), m_buildingSelectionManager.AvailableBuildingsCount);
    }

    int m_buildingSelection;
}
