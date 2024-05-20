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

    [SerializeField]
    private GameObject m_filterOverlayPrefab;

    private void Awake()
    {
        m_validityPreditors = new();
        m_buildingSelectionManager.Rotate = false;

        InitFilterOverlayGameObjects();
    }

    private void InitFilterOverlayGameObjects()
    {
        m_filterOverlay = new(m_tileManager.GridSize.x * m_tileManager.GridSize.y);

        for(int y = 0; y < m_tileManager.GridSize.y; ++y)
        {
            for(int x = 0; x < m_tileManager.GridSize.x; ++x)
            {
                GameObject filterOverlayObject = Instantiate(m_filterOverlayPrefab);
                filterOverlayObject.transform.position = new(x, 1f, y);
                filterOverlayObject.SetActive(false);
                m_filterOverlay.Add(filterOverlayObject);
            }
        }
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
                            OnBuildingSelected();
                            m_scoreManager.RecalculateScore();
                            UpdateFilter();
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
                int filterButtonSelection = DrawFilterButtons();
                if(filterButtonSelection != m_filterButtonSelection)
                {
                    m_filterButtonSelection = filterButtonSelection;
                    UpdateFilter();
                }
            }
            GUILayout.EndVertical();

            int selection = DrawAvailableBuildingButtons();

            if (selection != m_buildingSelectionIndex)
            {
                m_buildingSelectionIndex = selection;
                OnBuildingSelected();
            }

            GUILayout.Label($"Score {m_scoreManager.Score}");

        }
        GUILayout.EndHorizontal();
    }

    // this is bad...
    private int DrawFilterButtons()
    {
        string[] filters = { "None", "Food", "Employment (Workers)", "Employment (Workplace)", "Entertainment", "Religion" };
        return GUILayout.SelectionGrid(m_filterButtonSelection, filters, filters.Length);
    }

    private void UpdateFilter()
    {
        switch(m_filterButtonSelection)
        {
            case 0: // none
                {
                    foreach (GameObject filterObject in m_filterOverlay)
                    {
                        filterObject.SetActive(false);
                    }
                    return;
                }

            case 1: // food
                {
                    foreach (GameObject filterObject in m_filterOverlay)
                    {
                        filterObject.SetActive(true);
                    }

                    IEnumerable<IEnumerable<Tile>> tileGrid = m_tileManager.TileGrid;
                    foreach (IEnumerable<Tile> tileColumn in tileGrid)
                    {
                        foreach (Tile tile in tileColumn)
                        {
                            int tileIndex = m_scoreManager.TileCoordinateToIndex(tile.TileIndex);

                            Building building = tile.Building;
                            if (building == null)
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            if (!building.HasProperty(Building.BuildingProperty.House))
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            int fedVillagers = 0;

                            foreach(Villager villager in building.Villagers)
                            {
                                if(villager.HasFood)
                                {
                                    fedVillagers++;
                                }
                            }

                            float fedVillagersRatio = ((float) fedVillagers) / ((float) building.VillagerCount);

                            Color filterColor = Color.Lerp(m_invalidPlacementColor, m_validPlacementColor, fedVillagersRatio);

                            Renderer renderer = m_filterOverlay[tileIndex].GetComponent<Renderer>();
                            renderer.material.SetColor("_Color", filterColor);
                        }
                    }
                }

                return;

            case 2: // employment (workers)
                {
                    foreach (GameObject filterObject in m_filterOverlay)
                    {
                        filterObject.SetActive(true);
                    }

                    IEnumerable<IEnumerable<Tile>> tileGrid = m_tileManager.TileGrid;
                    foreach (IEnumerable<Tile> tileColumn in tileGrid)
                    {
                        foreach (Tile tile in tileColumn)
                        {
                            int tileIndex = m_scoreManager.TileCoordinateToIndex(tile.TileIndex);

                            Building building = tile.Building;
                            if (building == null)
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            if (!building.HasProperty(Building.BuildingProperty.House))
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            int workingVillagers = 0;

                            foreach (Villager villager in building.Villagers)
                            {
                                if (villager.HasJob)
                                {
                                    workingVillagers++;
                                }
                            }

                            float employmentRatio = ((float)workingVillagers) / ((float)building.VillagerCount);

                            Color filterColor = Color.Lerp(m_invalidPlacementColor, m_validPlacementColor, employmentRatio);

                            Renderer renderer = m_filterOverlay[tileIndex].GetComponent<Renderer>();
                            renderer.material.SetColor("_Color", filterColor);
                        }
                    }
                }

                return;

            case 3: // employment (workplace)
                {
                    foreach (GameObject filterObject in m_filterOverlay)
                    {
                        filterObject.SetActive(true);
                    }

                    IEnumerable<IEnumerable<Tile>> tileGrid = m_tileManager.TileGrid;
                    foreach (IEnumerable<Tile> tileColumn in tileGrid)
                    {
                        foreach (Tile tile in tileColumn)
                        {
                            int tileIndex = m_scoreManager.TileCoordinateToIndex(tile.TileIndex);

                            Building building = tile.Building;
                            if (building == null)
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            if (!building.HasProperty(Building.BuildingProperty.Employment))
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            float employmentRatio = building.EmploymentRatio;

                            Color filterColor = Color.Lerp(m_invalidPlacementColor, m_validPlacementColor, employmentRatio);

                            Renderer renderer = m_filterOverlay[tileIndex].GetComponent<Renderer>();
                            renderer.material.SetColor("_Color", filterColor);
                        }
                    }
                }
                
                return;

            // todo:
            case 4: // entertainment
                {
                    foreach (GameObject filterObject in m_filterOverlay)
                    {
                        filterObject.SetActive(true);
                    }

                    IEnumerable<IEnumerable<Tile>> tileGrid = m_tileManager.TileGrid;
                    foreach (IEnumerable<Tile> tileColumn in tileGrid)
                    {
                        foreach (Tile tile in tileColumn)
                        {
                            int tileIndex = m_scoreManager.TileCoordinateToIndex(tile.TileIndex);

                            Building building = tile.Building;
                            if (building == null)
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            if (!building.HasProperty(Building.BuildingProperty.House))
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            int entertainedVillagers = 0;

                            foreach (Villager villager in building.Villagers)
                            {
                                if (villager.HasEntertainment)
                                {
                                    entertainedVillagers++;
                                }
                            }

                            float entertainedRatio = ((float)entertainedVillagers) / ((float)building.VillagerCount);

                            Color filterColor = Color.Lerp(m_invalidPlacementColor, m_validPlacementColor, entertainedRatio);

                            Renderer renderer = m_filterOverlay[tileIndex].GetComponent<Renderer>();
                            renderer.material.SetColor("_Color", filterColor);
                        }
                    }
                }

                return;

            case 5: // food
                {
                    foreach (GameObject filterObject in m_filterOverlay)
                    {
                        filterObject.SetActive(true);
                    }

                    IEnumerable<IEnumerable<Tile>> tileGrid = m_tileManager.TileGrid;
                    foreach (IEnumerable<Tile> tileColumn in tileGrid)
                    {
                        foreach (Tile tile in tileColumn)
                        {
                            int tileIndex = m_scoreManager.TileCoordinateToIndex(tile.TileIndex);

                            Building building = tile.Building;
                            if (building == null)
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            if (!building.HasProperty(Building.BuildingProperty.House))
                            {
                                m_filterOverlay[tileIndex].SetActive(false);
                                continue;
                            }

                            int villagersThatHaveAChurch = 0;

                            foreach (Villager villager in building.Villagers)
                            {
                                if (villager.HasChurch)
                                {
                                    villagersThatHaveAChurch++;
                                }
                            }

                            float churchRatio = ((float)villagersThatHaveAChurch) / ((float)building.VillagerCount);

                            Color filterColor = Color.Lerp(m_invalidPlacementColor, m_validPlacementColor, churchRatio);

                            Renderer renderer = m_filterOverlay[tileIndex].GetComponent<Renderer>();
                            renderer.material.SetColor("_Color", filterColor);
                        }
                    }
                }

                return;
        }
    }

    private void OnBuildingSelected()
    {
        m_buildingSelectionManager.BuildingPrefab = m_buildingSelectionManager.AvailableBuildings[m_buildingSelectionIndex];
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

    int m_filterButtonSelection;

    List<GameObject> m_filterOverlay;
}
