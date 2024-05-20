using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	[SerializeField]
	Vector2Int m_buildingSize;

	[SerializeField]
	BuildingProperty m_buildingProperty;

	[SerializeField]
	int m_villagerCapacity;

	[SerializeField]
	int m_productionCapacity;

	public enum BuildingProperty
    {
		House = 1 << 0,
		Road = 1 << 1,
		Employment = 1 << 2,
		FoodGenerating = 1 << 3,

		Farm = Employment | FoodGenerating,
	}

	public Vector2Int BuildingSize { get { return m_buildingSize; } }

	public BuildingProperty Property { get { return m_buildingProperty; } }

	public bool IsAtVillagerCapacity { get { return m_villagers.Count >= m_villagerCapacity; } }
	public bool IsAtProductionCapacity { get { return m_production >= m_productionCapacity; } }

	public void AddVillager(Villager villager)
	{
		if(!IsAtVillagerCapacity)
		{
			m_villagers.Add(villager);
		}
	}

	public void IncrementProductionCapacity()
    {
		if(!IsAtProductionCapacity)
        {
			m_production++;
		}
	}

	public static float GetTileCost(Building building)
	{
		if(building == null)
        {
			return 2f;
        }

		if (building. HasProperty(BuildingProperty.Road))
		{
			return 1f;
		}
		else
		{
			return 2f;
		}
	}

	public bool HasProperty(BuildingProperty property)
	{
		return (property & Property) == property;
	}

	private void Awake()
    {
		m_villagers = new(m_villagerCapacity);

		if(m_buildingProperty == BuildingProperty.House)
        {
			for(int i = 0; i < m_villagerCapacity; ++i)
            {
				m_villagers.Add(new Villager(this));
			}
        }
	}

	public IEnumerable<Villager> Villagers => m_villagers;

	private List<Villager> m_villagers;
	private int m_production;
}
