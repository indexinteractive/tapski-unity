using System.Collections.Generic;
using UnityEngine;

public class WorldGenerator : MonoBehaviour
{
    #region Map Properties
    [Header("Generation Settings")]
    private const int MaxTrees = 300;
    private const int MaxCoins = 5;
    private const int MaxCheckpoints = 3;

    [Tooltip("The distance from the player that the map will be generated")]
    public int TileLookAhead = 10;
    [Tooltip("Extra tiles to generate in the X coordinate")]
    public int xOverdraw = 5;
    [Tooltip("Extra tiles to generate in the Y coordinate")]
    public int yOverdraw = 5;

    [Header("Map Properties")]
    public Transform Player;
    public int PlayerPathWidth = 3;
    #endregion

    #region Prefabs
    [Header("Prefabs")]
    public GameObject Coin;
    public GameObject SnowyTree;
    public GameObject PartialTree;
    public GameObject PineTree;
    public GameObject ChristmasTree;
    public GameObject Checkpoint;
    public GameObject Rock;
    public GameObject SmallSnowman;
    public GameObject BigSnowman;
    public GameObject WoodRamp;
    public GameObject SnowRamp;

    [Header("Debugging")]
    public GameObject PathStep;
    #endregion

    #region Private Fields
    /// <summary>
    /// The path laid out before the player where he can navigate without
    /// many obstacles (mainly trees)
    /// </summary>
    private List<GameObject> path;

    /// <summary>
    /// List of all basic objects in the game
    /// </summary>
    private List<GameObject> objects;

    private GameObject _parent;
    private Rect _worldBounds;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        _parent = new GameObject("_SnowWorld_");
        PopulateObjects();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(_worldBounds.center, _worldBounds.size);
    }
    #endregion

    #region Procedural Generation
    /// <summary>
    /// Fills the object lists with random objects
    /// </summary>
    [Button]
    private void PopulateObjects()
    {
        objects = new List<GameObject>();

        ///////// MOVE TO AWAKE
        _parent = new GameObject("_SnowWorld_");

        var ortho = Camera.main.orthographicSize;
        var cameraHalfWidth = ortho * Camera.main.aspect;
        var center = Camera.main.transform.position;

        _worldBounds = new Rect(
            Mathf.Floor(center.x - cameraHalfWidth) - xOverdraw,
            Mathf.Floor(center.y - ortho) - yOverdraw,
            Mathf.Ceil(cameraHalfWidth * 2) + (xOverdraw * 2),
            Mathf.Ceil(ortho * 2) + (yOverdraw * 2)
        );
        ////======///// MOVE TO AWAKE

        // Indexes[0-> (MAX_COINS - 1)] are coins
        for (int i = 0; i < 2 * MaxCoins; i++)
        {
            var coin = Instance(Coin, -1000, -1000);
            objects.Add(coin);
        }

        for (int i = 0; i < MaxCheckpoints; i++)
        {
            var checkpoint = Instance(Checkpoint, -1000, -1000);
            objects.Add(checkpoint);
        }

        // Create objects that will only have one instance offscreen
        objects.Add(Instance(Rock, -1000, -1000));
        objects.Add(Instance(WoodRamp, -1000, -1000));
        objects.Add(Instance(SnowRamp, -1000, -1000));
        objects.Add(Instance(SnowRamp, -1000, -1000));
        objects.Add(Instance(BigSnowman, -1000, -1000));
        objects.Add(Instance(SmallSnowman, -1000, -1000));

        int treeCount = 0;
        for (float y = _worldBounds.yMax - 1; y >= _worldBounds.yMin; y--)
        {
            // for (float x = _worldBounds.xMin; x <= _worldBounds.xMax; x++)
            float xStart = Player.position.x - (PlayerPathWidth / 2);
            float xEnd = Player.position.x + (PlayerPathWidth / 2);

            if (y < Player.position.y)
            {
                for (float x = xStart; x <= xEnd; x++)
                {
                    var p = Instance(PathStep, x, y);
                    path.Add(p);
                }
            }

            if (treeCount < MaxTrees)
            {
                AddTree(xStart, xEnd, y);
                treeCount++;
            }
        }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Adds a tree on either side of the given position
    /// </summary>
    /// <param name="y">World index [,y]</param>
    private void AddTree(float pathXStart, float pathXEnd, float y)
    {
        // Even numbers on the left, odds on the right
        float x = (Random.Range(0, 2) == 0)
            ? Random.Range(_worldBounds.xMin, pathXStart - 1)
            : Random.Range(pathXEnd + 1, _worldBounds.xMax);

        // Choose the tree type
        int chance = Random.Range(0, 100);
        GameObject treePrefab;

        if (chance < 5)
        {
            treePrefab = ChristmasTree;
        }
        else
        {
            chance = Random.Range(0, 3);

            switch (chance)
            {
                case 0: treePrefab = SnowyTree; break;
                case 2: treePrefab = PartialTree; break;
                default: treePrefab = PineTree; break;
            }
        }

        var tree = Instance(treePrefab, x, y);
        objects.Add(tree);
    }

    public GameObject Instance(GameObject prefab, float x, float y)
    {
        GameObject instance = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
        instance.transform.parent = _parent.transform;

        return instance;
    }

    public GameObject Instance(GameObject prefab, Vector2 position)
    {
        GameObject instance = Instantiate(prefab, position, Quaternion.identity);
        instance.transform.parent = _parent.transform;

        return instance;
    }
    #endregion
}
