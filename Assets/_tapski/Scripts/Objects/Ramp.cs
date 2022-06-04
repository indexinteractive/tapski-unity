using UnityEngine;

public class Ramp : MonoBehaviour
{
    #region Public Properties
    public GameObject CoinPrefab;

    [Tooltip("Number of coins to spawn")]
    public int CoinCount = 2;

    [Tooltip("Distance from the ramp to the spawnpoint of the coins")]
    public float CoinDistance = 1.5f;

    [Tooltip("Distance between the coins")]
    public float CoinGap = 1f;
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        GameObject coinParent = new GameObject("CoinParent");
        coinParent.transform.SetParent(transform);

        float totalWidth = (CoinCount - 1) * CoinGap;
        coinParent.transform.localPosition = new Vector3(-totalWidth / 2, -CoinDistance, 0);

        for (int i = 0; i < CoinCount; i++)
        {
            var coin = Instantiate(CoinPrefab, coinParent.transform);
            coin.transform.localPosition = new Vector3((i * CoinGap), 0, 0);

            coin.SetActive(true);
        }
    }
    #endregion
}
