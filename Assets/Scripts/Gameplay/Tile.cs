using UnityEngine;

public class Tile : MonoBehaviour
{
	[SerializeField]
	private TileManager m_tileManager;

	[SerializeField]
	private BuildingSelectionManager m_buildingSelectionManager;

	[SerializeField]
	private ScoreManager m_scoreManager;

	[SerializeField]
	private GameObject m_buildingPrefab;

	public Vector2Int TileIndex => m_tileIndex;

	public void Initialize(int xIndex, int yIndex)
    {
		m_tileIndex = new(xIndex, yIndex);
	}

	// todo: don't have this running every object
	// should be merged into the code that highlights placement. whenever we get around to that...
    private void OnMouseDown()
    {
		Debug.Log($"Tile - OnMouseDown: attempting to place {m_buildingPrefab.name} at {m_tileIndex}");
		if(m_tileManager.TryPlaceBuilding(m_buildingSelectionManager.BuildingPrefab, m_buildingSelectionManager.Rotate, m_tileIndex))
		{
			m_buildingSelectionManager.RepopulateAvailableBuildings();
			m_scoreManager.RecalculateScore();
		}
	}

    public Building Building { set; get; }

	Vector2Int m_tileIndex;
}
