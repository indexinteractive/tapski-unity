using UnityEngine;

public class CoinSpawner : MonoBehaviour
{
    #region Public Properties
    public GameObject CoinPrefab;

    [Tooltip("Number of coins to spawn")]
    public int CoinCount = 2;

    [Tooltip("Distance from the object to the spawnpoint of the coins")]
    public float CoinDistance = 1.5f;

    [Tooltip("Distance between the coins")]
    public float CoinGap = 1f;
    #endregion

    #region Private Fields
    public GameObject[] _coins;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _coins = new GameObject[CoinCount];

        GameObject coinParent = new GameObject("CoinParent");
        coinParent.transform.SetParent(transform);

        float totalWidth = (CoinCount - 1) * CoinGap;
        coinParent.transform.localPosition = new Vector3(-totalWidth / 2, -CoinDistance, 0);

        for (int i = 0; i < CoinCount; i++)
        {
            _coins[i] = Instantiate(CoinPrefab, coinParent.transform);
            _coins[i].transform.localPosition = new Vector3((i * CoinGap), 0, 0);

            _coins[i].SetActive(false);
        }
    }

    private void OnEnable()
    {
        for (int i = 0; i < CoinCount; i++)
        {
            _coins[i].SetActive(true);
        }
    }
    #endregion
}
