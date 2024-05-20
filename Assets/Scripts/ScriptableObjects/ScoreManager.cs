using UnityEngine;

[CreateAssetMenu(fileName = "ScoreManager", menuName = "Singletons/ScoreManager")]
public class ScoreManager : ScriptableObject
{
	[SerializeField]
	private TileManager m_tileManager;


}
