using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TileManager", menuName = "Singletons/TileManager")]
public class TileManager : ScriptableObject
{
	[SerializeField]
	GameObject m_tilePrefab;

	[SerializeField]
	Vector2Int m_gridSize;

	public Vector2Int GridSize { get { return m_gridSize; } }

	// do scriptable objects support ctors?
	public void Init()
    {
		m_tileGrid = new(m_gridSize.x);

		for (int i = 0; i < m_gridSize.x; ++i)
		{
			m_tileGrid.Add(new(m_gridSize.y));

			for (int ii = 0; ii < m_gridSize.y; ++ii)
			{
				m_tileGrid[i].Add(null);
			}
		}

		GenerateTiles();

		Physics.queriesHitTriggers = true;
	}

    public void GenerateTiles()
	{
		ResetTiles();

		for(int i = 0; i < m_gridSize.x; ++i)
        {
			for(int ii = 0; ii < m_gridSize.y; ++ii)
			{

				m_tileGrid[i][ii] = Instantiate(m_tilePrefab);

				InitializeTile(m_tileGrid[i][ii], i, ii);
			}
		}
	}

	public void ResetTiles()
    {
		for (int i = 0; i < m_gridSize.x; ++i)
		{
			for (int ii = 0; ii < m_gridSize.y; ++ii)
			{
				if(m_tileGrid[i][ii] != null)
                {
					Destroy(m_tileGrid[i][ii]);
				}
			}
		}
	}

	public void TryPlaceBuilding(GameObject buildingPrefab, bool rotate, Vector2Int tileIndex)
    {
		if(!CanInstantiateBuilding(buildingPrefab, rotate, tileIndex))
		{
			Debug.Log($"TileManager - TryPlaceBuilding: failed to place {buildingPrefab.name} at {tileIndex}");
			return;
        }

		InstantiateBuilding(buildingPrefab, rotate, tileIndex);
	}

	public void ReevaluateScore()
    {

    }

	private bool CanInstantiateBuilding(GameObject buildingPrefab, bool rotate, Vector2Int tileIndex)
    {
		if(buildingPrefab == null)
        {
			return false;
        }

		Building building = buildingPrefab.GetComponent<Building>();

		if (building == null)
		{
			return false;
		}

		Vector2Int buildingSize = rotate ? new(building.BuildingSize.y, building.BuildingSize.x) : building.BuildingSize;

		if (!IsBuildingWithinGridBounds(tileIndex, buildingSize))
		{
			return false;
		}

		if(DoTilesContainBuilding(tileIndex, buildingSize))
        {
			return false;
        }

		return true;
	}

	private bool IsBuildingWithinGridBounds(Vector2Int tileIndex, Vector2Int buildingSize)
    {
		if (tileIndex.x < 0 || tileIndex.y < 0 || tileIndex.x + buildingSize.x > m_gridSize.x || tileIndex.y + buildingSize.y > m_gridSize.y)
		{
			return false;
		}

		return true;
    }

	private bool DoTilesContainBuilding(Vector2Int tileIndex, Vector2Int buildingSize)
    {
		for(int i = tileIndex.x; i < tileIndex.x + buildingSize.x; ++i)
        {
			for (int ii = tileIndex.y; ii < tileIndex.y + buildingSize.y; ++ii)
			{
				Tile tile = m_tileGrid[i][ii].GetComponent<Tile>();

				if(tile.Building != null)
                {
					return true;
                }
			}
		}

		return false;
    }

	private void InstantiateBuilding(GameObject buildingPrefab, bool rotate, Vector2Int tileIndex)
    {
		GameObject building = Instantiate(buildingPrefab);
		building.transform.position = new(tileIndex.x, 1f, tileIndex.y);

		Building buildingComponent = building.GetComponent<Building>();

		Vector2Int buildingSize;
		if(rotate)
        {
			buildingSize = new(buildingComponent.BuildingSize.y, buildingComponent.BuildingSize.x);
			building.transform.rotation = Quaternion.Euler(new(0f, 90f, 0f));
		}
		else
        {
			buildingSize = buildingComponent.BuildingSize;
		}

		for (int i = tileIndex.x; i < tileIndex.x + buildingSize.x; ++i)
		{
			for (int ii = tileIndex.y; ii < tileIndex.y + buildingSize.y; ++ii)
			{
				Tile tile = m_tileGrid[i][ii].GetComponent<Tile>();
				tile.Building = building;
			}
		}
	}

	private void InitializeTile(GameObject tile, int xIndex, int yIndex)
    {
		tile.transform.position = new(xIndex, 0f, yIndex);
		Tile tileComponent = tile.GetComponent<Tile>();
		tileComponent.Initialize(xIndex, yIndex);
	}

	private List<List<GameObject>> m_tileGrid;
}
