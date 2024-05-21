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

	[SerializeField]
	float m_improvementMultiplier;

	[SerializeField]
	int m_improvementRange;

	public enum BuildingProperty
	{
		House = 1 << 0,
		Road = 1 << 1,
		Employment = 1 << 2,
		FoodGenerating = 1 << 3,
		ProductionImprovement = 1 << 4,
		Entertainment = 1 << 5,
		Religion = 1 << 6,

		Farm = Employment | FoodGenerating,

		Windmill = Employment | ProductionImprovement,
		Tavern = Employment | Entertainment,
		Colosseum = Employment | Entertainment,
		Church = Employment | Religion,
		Park = Entertainment,
	}

	public float EmploymentRatio
	{
		get
		{
			if (m_villagerCapacity == 0)
			{
				return 1f;
			}

			return m_villagers.Count / m_villagerCapacity;
		}
	}

	public IEnumerable<Villager> Villagers => m_villagers;
	public int VillagerCount => m_villagers.Count;

	public Vector2Int BuildingSize { get { return m_buildingSize; } }

	public BuildingProperty Property { get { return m_buildingProperty; } }

	public Building ImprovementBuilding { get; set; }

	public float ImprovementMultiplier => m_improvementMultiplier;

	public int ImprovementRange => m_improvementRange;

	public bool IsAtVillagerCapacity { get { return m_villagers.Count >= m_villagerCapacity; } }

	public int ProductionCapacity
	{
		get
		{
			float improvementMultiplier = ImprovementBuilding ? ImprovementBuilding.ImprovementMultiplier : 1f;

			if (m_villagerCapacity == 0)
			{
				return (int) ((float)m_productionCapacity) * (int)improvementMultiplier;
			}

			float villagerRatio = ((float)m_villagers.Count) / ((float)m_villagerCapacity);
			float realProductionCapacity = ((float)m_productionCapacity) * villagerRatio;
			realProductionCapacity *= improvementMultiplier;

			return (int)realProductionCapacity;
		}
	}

	public bool IsAtProductionCapacity
	{
		get
		{
			return m_production >= ProductionCapacity;
		}
	}

	public void Reset()
	{
		if (HasProperty(BuildingProperty.House))
		{
			foreach (Villager villager in Villagers)
			{
				villager.Work = null;
				villager.FoodSource = null;
				villager.EntertainmentSource = null;
				villager.ReligionSource = null;
			}
        }
		else
        {
			m_villagers.Clear();
        }

		m_production = 0;
    }

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

	private List<Villager> m_villagers;
	private int m_production;
}
