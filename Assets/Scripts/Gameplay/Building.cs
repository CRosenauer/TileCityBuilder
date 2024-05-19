using UnityEngine;

public class Building : MonoBehaviour
{
	[SerializeField]
	Vector2Int m_buildingSize;

	[SerializeField]
	BuildingProperty m_buildingProperty;

	public enum BuildingProperty
    {
		House,
		Road,
		Farm
    }

	public Vector2Int BuildingSize { get { return m_buildingSize; } }

	public BuildingProperty Property { get { return m_buildingProperty; } }
}
