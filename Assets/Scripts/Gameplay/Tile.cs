using UnityEngine;

public class Tile : MonoBehaviour
{
	[SerializeField]
	private TileManager m_tileManager;

	[SerializeField]
	private BuildingSelectionManager m_buildingSelectionManager;

	[SerializeField]
	private ScoreManager m_scoreManager;

	public Vector2Int TileIndex => m_tileIndex;

	public void Initialize(int xIndex, int yIndex)
    {
		m_tileIndex = new(xIndex, yIndex);
	}

    public Building Building { set; get; }

	Vector2Int m_tileIndex;
}
