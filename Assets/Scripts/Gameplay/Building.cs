using UnityEngine;

public class Building : MonoBehaviour
{
	[SerializeField]
	Vector2Int m_buildingSize;

	public Vector2Int BuildingSize { get { return m_buildingSize; } }
}
