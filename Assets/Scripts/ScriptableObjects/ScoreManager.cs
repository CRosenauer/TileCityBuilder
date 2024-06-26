using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ScoreManager", menuName = "Singletons/ScoreManager")]
public class ScoreManager : ScriptableObject
{
	[SerializeField]
	private TileManager m_tileManager;

    [SerializeField]
    private float m_distanceLimit;

    [SerializeField]
    private int m_workerScore;

    [SerializeField]
    private int m_foodScore;

    [SerializeField]
    private int m_churchScore;

    [SerializeField]
    private int m_entertainmentScore;

    public int Score { private set; get; }

	public void RecalculateScore()
    {
        Score = 0;

        ResetAllJobsAndServices();

        float[, ] distanceTable = AllPairShortestPath();

        foreach (List<Tile> tileColumn in m_tileManager.TileGrid)
        {
            foreach(Tile tile in tileColumn)
            {
                Building building = tile.Building;

                if(building == null)
                {
                    continue;
                }

                if(!building.HasProperty(Building.BuildingProperty.House))
                {
                    continue;
                }

                foreach(Villager villager in building.Villagers)
                {
                    if(TryFindJob(distanceTable, villager, tile.TileIndex))
                    {
                        Score += m_workerScore;
                    }

                    if(TryFindFoodSource(distanceTable, villager, tile.TileIndex))
                    {
                        Score += m_foodScore;
                    }

                    if(TryFindEntertainment(distanceTable, villager, tile.TileIndex))
                    {
                        Score += m_entertainmentScore;
                    }

                    if (TryFindReligionSource(distanceTable, villager, tile.TileIndex))
                    {
                        Score += m_churchScore;
                    }
                }
            }
        }
    }

    private void ResetAllJobsAndServices()
    {
        foreach(IEnumerable<Tile> tileColumn in m_tileManager.TileGrid)
        {
            foreach(Tile tile in tileColumn)
            {
                if(tile.Building == null)
                {
                    continue;
                }

                tile.Building.Reset();
            }
        }
    }

    private bool TryFindJob(float[,] distanceTable, Villager villager, Vector2Int tileIndex)
    {
        if (villager.HasJob)
        {
            return true;
        }

        Building job = FindJob(distanceTable, tileIndex);
        if (job != null)
        {
            villager.Work = job;
            job.AddVillager(villager);

            if(job.HasProperty(Building.BuildingProperty.ProductionImprovement))
            {
                // apply improvement multiplier to appropriate buildings
            }

            return true;
        }

        return false;
    }

    private bool TryFindFoodSource(float[,] distanceTable, Villager villager, Vector2Int tileIndex)
    {
        if(!villager.HasJob)
        {
            return false;
        }

        // can we run into the state where we have food but no job?
        if (villager.HasFood)
        {
            return true;
        }

        Building foodSource = FindFoodSource(distanceTable, tileIndex);
        if (foodSource != null)
        {
            villager.FoodSource = foodSource;
            foodSource.IncrementProductionCapacity();
            return true;
        }
        
        return false;
    }

    private bool TryFindEntertainment(float[,] distanceTable, Villager villager, Vector2Int tileIndex)
    {
        if (!villager.HasJob)
        {
            return false;
        }

        if(villager.HasEntertainment)
        {
            return true;
        }

        Building entertainment = FindEntertainment(distanceTable, tileIndex);
        if(entertainment != null)
        {
            villager.EntertainmentSource = entertainment;
            entertainment.IncrementProductionCapacity();
            return true;
        }

        return false;
    }

    private bool TryFindReligionSource(float[,] distanceTable, Villager villager, Vector2Int tileIndex)
    {
        if (!villager.HasJob)
        {
            return false;
        }

        if (villager.HasChurch)
        {
            return true;
        }

        Building church = FindChurch(distanceTable, tileIndex);
        if (church != null)
        {
            villager.ReligionSource = church;
            church.IncrementProductionCapacity();
            return church;
        }

        return false;
    }

    private Building FindJob(float[, ] distanceTable, Vector2Int startCoordinate)
    {
        return FindAvailableBuilding(distanceTable, startCoordinate, Building.BuildingProperty.Employment, true);
    }

    private Building FindFoodSource(float[, ] distanceTable, Vector2Int startCoordinate)
    {
        return FindAvailableBuilding(distanceTable, startCoordinate, Building.BuildingProperty.FoodGenerating, false);
    }

    private Building FindChurch(float[,] distanceTable, Vector2Int startCoordinate)
    {
        return FindAvailableBuilding(distanceTable, startCoordinate, Building.BuildingProperty.Religion, false);
    }

    private Building FindEntertainment(float[,] distanceTable, Vector2Int startCoordinate)
    {
        return FindAvailableBuilding(distanceTable, startCoordinate, Building.BuildingProperty.Entertainment, false);
    }

    private Building FindAvailableBuilding(float[,] distanceTable, Vector2Int startCoordinate, Building.BuildingProperty buildingProperty, bool isVillagerCapacity)
    {
        int tileCount = GetTileCount();
        int startIndex = TileCoordinateToIndex(startCoordinate);

        if(startIndex == -1)
        {
            return null;
        }

        Building nearestBuilding = null;
        float nearestDistance = float.PositiveInfinity;

        for (int i = 0; i < tileCount; ++i)
        {
            float distanceToTile = distanceTable[startIndex, i];

            if (distanceToTile > m_distanceLimit || distanceToTile >= nearestDistance)
            {
                continue;
            }

            Vector2Int tileCoordinate = IndexToTileCoordinate(i);

            Tile tile = m_tileManager.GetTile(tileCoordinate);
            Building building = tile.Building;

            if (building == null)
            {
                continue;
            }

            if (!building.HasProperty(buildingProperty))
            {
                continue;
            }

            bool isAtCapacity = isVillagerCapacity ? building.IsAtVillagerCapacity : building.IsAtProductionCapacity;

            if (isAtCapacity)
            {
                continue;
            }

            nearestBuilding = building;
            nearestDistance = distanceToTile;
        }

        return nearestBuilding;
    }

    // todo: move to helpers
    public int TileCoordinateToIndex(Vector2Int tileCoordinate)
    {
        if(tileCoordinate.x >= m_tileManager.GridSize.x)
        {
            return -1;
        }
        if (tileCoordinate.y >= m_tileManager.GridSize.y)
        {
            return -1;
        }

        return tileCoordinate.x + m_tileManager.GridSize.x * tileCoordinate.y;
    }

    public Vector2Int IndexToTileCoordinate(int index)
    {
        return new Vector2Int(index % m_tileManager.GridSize.x, index / m_tileManager.GridSize.x);
    }

    private int GetTileCount()
    {
        return m_tileManager.GridSize.x * m_tileManager.GridSize.y;
    }

    // https://www.geeksforgeeks.org/floyd-warshall-algorithm-dp-16/
    private float[,] AllPairShortestPath()
    {
        float[,] distanceTable = GetInitializeDistanceTable();

        int tileCount = GetTileCount();

        for (int intermediateIndex = 0; intermediateIndex < tileCount; intermediateIndex++)
        {
            for (int startIndex = 0; startIndex < tileCount; startIndex++)
            {
                for (int endIndex = 0; endIndex < tileCount; endIndex++)
                {
                    if (distanceTable[startIndex, intermediateIndex] + distanceTable[intermediateIndex, endIndex] < distanceTable[startIndex, endIndex])
                    {
                        distanceTable[startIndex, endIndex] = distanceTable[startIndex, intermediateIndex] + distanceTable[intermediateIndex, endIndex];
                    }
                }
            }
        }

        return distanceTable;
    }

    private float[,] GetInitializeDistanceTable()
    {
        int tileCount = GetTileCount();

        float[,] distanceTable = new float[tileCount, tileCount];
        
        for(int i = 0; i < tileCount; ++i)
        {
            for(int j = 0; j < tileCount; ++j)
            {
                distanceTable[i, j] = float.PositiveInfinity;
            }
        }

        for (int xIndex = 0; xIndex < m_tileManager.GridSize.x; ++xIndex)
        {
            for (int yIndex = 0; yIndex < m_tileManager.GridSize.y; ++yIndex)
            {
                Vector2Int positionCoordinate = new(xIndex, yIndex);
                Vector2Int rightCoordinate = new(xIndex + 1, yIndex);
                Vector2Int upCoordinate = new(xIndex, yIndex + 1);

                int positionIndex = TileCoordinateToIndex(positionCoordinate);
                int rightIndex = TileCoordinateToIndex(rightCoordinate);
                int upIndex = TileCoordinateToIndex(upCoordinate);

                distanceTable[positionIndex, positionIndex] = 0;

                if (rightIndex != -1)
                {
                    distanceTable[positionIndex, rightIndex] = Building.GetTileCost(m_tileManager.GetTile(positionCoordinate).Building);
                    distanceTable[rightIndex, positionIndex] = Building.GetTileCost(m_tileManager.GetTile(rightCoordinate).Building);
                }

                if(upIndex != -1)
                {
                    distanceTable[positionIndex, upIndex] = Building.GetTileCost(m_tileManager.GetTile(positionCoordinate).Building);
                    distanceTable[upIndex, positionIndex] = Building.GetTileCost(m_tileManager.GetTile(upCoordinate).Building);

                }
            }
        }

        return distanceTable;
    }
}
