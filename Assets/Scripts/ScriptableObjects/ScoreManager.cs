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

    public int Score { private set; get; }

	public void RecalculateScore()
    {
        Score = 0;

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
                    // todo: reduce copy pasta
                    bool hasJob = false;
                    if(!villager.HasJob)
                    {
                        Building job = FindJob(distanceTable, tile.TileIndex);
                        if(job != null)
                        {
                            hasJob = true;

                            villager.Work = job;
                            job.AddVillager(villager);
                        }
                    }
                    else
                    {
                        hasJob = true;
                    }

                    if(hasJob)
                    {
                        Score += m_workerScore;

                        bool hasFood = false;
                        if(!villager.HasFood)
                        {
                            Building foodSource = FindFoodSource(distanceTable, tile.TileIndex);
                            if (foodSource != null)
                            {
                                hasFood = true;

                                villager.FoodSource = foodSource;
                                foodSource.IncrementProductionCapacity();
                            }
                        }
                        else
                        {
                            hasFood = true;
                        }

                        if(hasFood)
                        {
                            Score += m_foodScore;
                        }
                    }
                }
            }
        }
    }

    private Building FindJob(float[, ] distanceTable, Vector2Int startCoordinate)
    {
        return FindAvailableBuilding(distanceTable, startCoordinate, Building.BuildingProperty.Employment, true);
    }

    private Building FindFoodSource(float[, ] distanceTable, Vector2Int startCoordinate)
    {
        return FindAvailableBuilding(distanceTable, startCoordinate, Building.BuildingProperty.FoodGenerating, false);
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

    private int TileCoordinateToIndex(Vector2Int tileCoordinate)
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

    private Vector2Int IndexToTileCoordinate(int index)
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
