using System.Collections.Generic;
using UnityEngine;

// todo: seperate UI from available building logic
// maybe this should be in BuildingSelectionManager and we just call methods on it?
public class GameUI : MonoBehaviour
{
    [SerializeField]
    private TileManager m_tileManager;

    [SerializeField]
    private BuildingSelectionManager m_buildingSelectionManager;

    [SerializeField]
    private ScoreManager m_scoreManager;

    [SerializeField]
    private GameObject m_vailidityPredictorPrefab;

    [SerializeField]
    private Color m_validPlacementColor;

    [SerializeField]
    private Color m_invalidPlacementColor;

    private void Awake()
    {
        m_validityPreditors = new();
        m_buildingSelectionManager.Rotate = false;
    }

    private void Update()
    {
        if(Input.GetButtonDown("RotateBuilding"))
        {
            m_buildingSelectionManager.Rotate = !m_buildingSelectionManager.Rotate;
        }

        if(m_buildingSelectionManager.BuildingPrefab != null)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Collider collider = hit.collider;
                Tile tile = collider.GetComponent<Tile>();
                if (tile != null)
                {
                    DrawPlacementValidityPredictor(tile);

                    if (Input.GetMouseButtonDown(0))
                    {
                        Debug.Log($"Tile - OnMouseDown: attempting to place {m_buildingSelectionManager.BuildingPrefab.name} at {tile.TileIndex}");
                        if (m_tileManager.TryPlaceBuilding(m_buildingSelectionManager.BuildingPrefab, m_buildingSelectionManager.Rotate, tile.TileIndex))
                        {
                            m_buildingSelectionManager.RepopulateAvailableBuildings();
                            OnBuildingSelected(m_buildingSelectionIndex);
                            m_scoreManager.RecalculateScore();
                        }
                    }
                }
            }
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginHorizontal();
        {
            GUILayout.BeginVertical();
            {
                int selection = DrawAvailableBuildingButtons();

                if (selection != m_buildingSelectionIndex)
                {
                    m_buildingSelectionIndex = selection;
                    OnBuildingSelected(m_buildingSelectionIndex);
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
        return GUILayout.SelectionGrid(m_buildingSelectionIndex, GetAvailableBuildingNames(), m_buildingSelectionManager.AvailableBuildingsCount);
    }

    private void DrawPlacementValidityPredictor(Tile tile)
    {
        GameObject buildingPrefab = m_buildingSelectionManager.BuildingPrefab;
        Building building = buildingPrefab.GetComponent<Building>();

        int buildingSize = building.BuildingSize.x * building.BuildingSize.y;

        ResizeValidityPredictors(buildingSize);

        SelectAndApplyValidityPredictorColor(buildingPrefab, tile);

        PositionValidityPredictors(tile);
    }

    private void ResizeValidityPredictors(int buildingSize)
    {
        while (buildingSize < m_validityPreditors.Count)
        {
            GameObject extraValidityPredictor = m_validityPreditors[0];
            Destroy(extraValidityPredictor);
            m_validityPreditors.RemoveAt(0);
        }

        while (buildingSize > m_validityPreditors.Count)
        {
            m_validityPreditors.Add(Instantiate(m_vailidityPredictorPrefab));
        }
    }

    private void SelectAndApplyValidityPredictorColor(GameObject buildingPrefab, Tile tile)
    {
        Color validityPredictorColor;

        if (m_tileManager.CanInstantiateBuilding(buildingPrefab, m_buildingSelectionManager.Rotate, tile.TileIndex))
        {
            validityPredictorColor = m_validPlacementColor;
        }
        else
        {
            validityPredictorColor = m_invalidPlacementColor;
        }

        foreach (GameObject validityPredictor in m_validityPreditors)
        {
            Renderer renderer = validityPredictor.GetComponent<Renderer>();
            renderer.material.SetColor("_Color", validityPredictorColor);
        }
    }

    private void PositionValidityPredictors(Tile tile)
    {
        GameObject buildingPrefab = m_buildingSelectionManager.BuildingPrefab;
        Building building = buildingPrefab.GetComponent<Building>();

        int outerLoopCount;
        int innerLoopCount;

        if(m_buildingSelectionManager.Rotate)
        {
            outerLoopCount = building.BuildingSize.y;
            innerLoopCount = building.BuildingSize.x;
        }
        else
        {
            outerLoopCount = building.BuildingSize.x;
            innerLoopCount = building.BuildingSize.y;
        }


        Vector2Int tileCoordinate = tile.TileIndex;
        int index = 0;

        for(int i = 0; i < outerLoopCount; ++i)
        {
            for (int j = 0; j < innerLoopCount; ++j)
            {
                m_validityPreditors[index].transform.position = new(i + tileCoordinate.x, 1f, j + tileCoordinate.y);
                ++index;
            }
        }
    }

    List<GameObject> m_validityPreditors;

    int m_buildingSelectionIndex;
}
