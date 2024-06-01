using UnityEngine;

public class SingletonContainer : MonoBehaviour
{
	[SerializeField]
	private TileManager m_tileManager;

    [SerializeField]
    private BuildingSelectionManager m_buildingSelectionManager;

    private void Awake()
    {
        m_tileManager.Init();
    }
}
