using UnityEngine;

public class Tile : MonoBehaviour
{
	[SerializeField]
	private TileManager m_tileManager;

	[SerializeField]
	private GameObject m_buildingPrefab;

	public void Initialize(int xIndex, int yIndex)
    {
		m_tileIndex = new(xIndex, yIndex);
	}

    private void OnMouseDown()
    {
		Debug.Log($"Tile - OnMouseDown: attempting to place {m_buildingPrefab.name} at {m_tileIndex}");
		m_tileManager.TryPlaceBuilding(m_buildingPrefab, m_tileIndex);
	}

    public GameObject Building { set; get; }

	Vector2Int m_tileIndex;
}
