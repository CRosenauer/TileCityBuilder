using System.Collections.Generic;
using UnityEngine;

public class Building : MonoBehaviour
{
	[SerializeField]
	Vector2Int m_buildingSize;

	[SerializeField]
	BuildingProperty m_buildingProperty;

	[SerializeField]
	readonly int m_villagerCapacity;

	public enum BuildingProperty
    {
		House,
		Road,
		Farm
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

    public Vector2Int BuildingSize { get { return m_buildingSize; } }

	public BuildingProperty Property { get { return m_buildingProperty; } }

	public IEnumerable<Villager> Villagers => m_villagers;

	private List<Villager> m_villagers;
}
