using UnityEngine;

public class SingletonContainer : MonoBehaviour
{
	[SerializeField]
	private TileManager m_tileManager;

    private void Awake()
    {
        m_tileManager.Init();
    }
}
