using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class WorldGenerator : MonoBehaviour
{
    #region Map Properties
    [Header("Generation Settings")]
    private const int MaxCoins = 5;
    private const int MaxCheckpoints = 3;

    [Header("Map Size")]
    [Tooltip("Extra tiles to generate in the X coordinate")]
    public int xOverdraw = 5;
    [Tooltip("Extra tiles to generate in the Y coordinate")]
    public int yOverdraw = 5;

    [Header("Player Properties")]
    public Transform Player;
    public int PlayerPathWidth = 3;

    public Rect WorldBounds
    {
        get
        {
            return new Rect(
                _camera.transform.position.x - _worldSize.x / 2,
                _camera.transform.position.y - _worldSize.y / 2,
                _worldSize.x,
                _worldSize.y
            );
        }
    }
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
    public GameObject PathPrefab;
    #endregion

    #region Private Fields
    private Camera _camera;

    /// <summary>
    /// The parent object for dynamically generated objects
    /// </summary>
    private GameObject _parent;

    private Vector2 _worldSize;
    #endregion

    #region Path Generation
    /// <summary>
    /// The path laid out before the player where he can navigate without
    /// many obstacles (mainly trees)
    /// </summary>
    private List<PathStep> _safePath = new List<PathStep>();

    /// <summary>
    /// Determines the direction the path will take. Values can be:
    /// -1: curves left
    ///  0: straight
    ///  1: curves right
    /// </summary>
    private int _pathDirection = 0;

    /// <summary>
    /// Tracks the number of steps elapsed since the direction of <see cref="_pathDirection"/>
    /// was changed.
    /// Increments for each step during a new generated block of world. Resets to 0 when
    /// a new block is generated <see cref="UpdatePathDirection"/>
    /// </summary>
    private int _stepsSinceLastChange = 0;

    /// <summary>
    /// Tracks the step in which <see cref="_pathDirection"/> will change again
    /// </summary>
    private int _nextStepChange = 0;

    /// <summary>
    /// The position of <see cref="Player"/> when the previous chunk of world was generated
    /// in <see cref="GenerateMoreWorld"/>. Used to determine when more world should be generated
    /// </summary>
    private int _lastUpdatePosition = 0;
    #endregion

    #region Object Lists
    /// <summary>
    /// List of all basic objects in the game
    /// </summary>
    private List<GameObject> _objects = new List<GameObject>();

    /// <summary>
    /// List of trees that are visible or not yet marked inactive by <see cref="DisableHiddenTrees"/>
    /// </summary>
    private List<GameObject> _activeTrees = new List<GameObject>();

    /// <summary>
    /// List of trees that are pending repositioning by <see cref="ActivateTrees"/>
    /// </summary>
    private Queue<GameObject> _inactiveTrees = new Queue<GameObject>();
    #endregion

    #region Unity Lifecycle
    private void OnEnable()
    {
        _parent = new GameObject("_SnowWorld_");

        _camera = Camera.main;
        Assert.IsNotNull(_camera, "[WorldGenerator] No main camera found");
        Assert.IsNotNull(PathPrefab, "[WorldGenerator] No path prefab found");

        var ortho = _camera.orthographicSize;
        var cameraHalfWidth = ortho * _camera.aspect;
        var center = _camera.transform.position;

        _worldSize = new Vector2(
            Mathf.Ceil(cameraHalfWidth * 2) + (xOverdraw * 2),
            Mathf.Ceil(ortho * 2) + (yOverdraw * 2)
        );

        PopulateObjects();
    }

    private void Update()
    {
        DisableHiddenTrees();

        int distanceSinceUpdate = (int)Mathf.Abs(Player.position.y - _lastUpdatePosition);
        if (distanceSinceUpdate > _camera.orthographicSize)
        {
            GenerateMoreWorld(distanceSinceUpdate, _safePath);
            _lastUpdatePosition = (int)Player.position.y;
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(WorldBounds.center, WorldBounds.size);
    }
#endif
    #endregion

    #region Randomization
    /// <summary>
    /// Returns a new <see cref="Vector3"/> position for a <see cref="PathStep"/> relative
    /// to the given previous (above in y-component) step <paramref name="previous"/>
    /// </summary>
    private Vector3 RandomizePathPosition(PathStep previous)
    {
        float offset = Random.Range(0, 0.5f) * _pathDirection;

        float x = previous.transform.position.x + offset;
        float y = previous.transform.position.y - 1;
        float z = previous.transform.position.z;

        return new Vector3(x, y, z);
    }

    /// <summary>
    /// Randomises a width for a path given the previous step <paramref name="previous"/>
    /// </summary>
    [System.Obsolete("Needs more work before use")]
    private float RandomPathWidth(PathStep previous)
    {
        float delta = Random.Range(0.5f, 1.5f);
        float width = previous.Width * delta;

        return Mathf.Clamp(width, PlayerPathWidth * 0.5f, PlayerPathWidth * 2);
    }

    /// <summary>
    /// Instantiates a new tree with the following percentages:
    /// - 5% chance of a christmas tree
    /// - 33% chance of a pine tree
    /// - 33% chance of a partial tree
    /// - 33% chance of a snowy tree
    /// </summary>
    private GameObject GenerateTree()
    {
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

        return Instance(treePrefab);
    }
    #endregion

    #region Procedural Generation
    /// <summary>
    /// Fills the object lists with random objects during initialization
    /// </summary>
    private void PopulateObjects()
    {
        for (int i = 0; i < 2 * MaxCoins; i++)
        {
            var coin = Instance(Coin);
            _objects.Add(coin);
        }

        for (int i = 0; i < MaxCheckpoints; i++)
        {
            var checkpoint = Instance(Checkpoint);
            _objects.Add(checkpoint);
        }

        // Create objects that will only have one instance offscreen
        _objects.Add(Instance(Rock));
        _objects.Add(Instance(WoodRamp));
        _objects.Add(Instance(SnowRamp));
        _objects.Add(Instance(SnowRamp));
        _objects.Add(Instance(BigSnowman));
        _objects.Add(Instance(SmallSnowman));

        for (float y = WorldBounds.yMax - 1; y >= WorldBounds.yMin * 2; y--)
        {
            Vector2 position = new Vector2(Player.position.x, y);
            PathStep newPath = PathStep.Create(PathPrefab, position, _parent, PlayerPathWidth);
            _safePath.Add(newPath);

            var tree = GenerateTree();
            RepositionTree(tree, newPath);
            _activeTrees.Add(tree);
        }
    }

    /// <summary>
    /// Generates more terrain in front of the player:
    /// - Moves items in <paramref name="path"/> from the back to the front of the list
    /// - Repositions and activates trees along the new steps of the path
    /// </summary>
    private void GenerateMoreWorld(int generateDistance, List<PathStep> path)
    {
        for (int i = 0; i < generateDistance; i++)
        {
            // Take a step from the very end of the path
            PathStep step = path[0];
            path.Remove(step);

            // Reposition that step in front of the current front
            PathStep front = path[path.Count - 1];
            RepositionPath(step, front);

            // Move dead objects back to the front of the screen (bottom)
            ActivateTrees(step);

            // Add the step to the end of the path
            path.Add(step);
            UpdatePathDirection();
        }
    }
    #endregion

    #region Object Pooling
    /// <summary>
    /// Iterates through <see cref="_activeTrees"/> and disables any that are above the camera Y bounds
    /// </summary>
    private void DisableHiddenTrees()
    {
        for (int i = _activeTrees.Count - 1; i >= 0; i--)
        {
            GameObject tree = _activeTrees[i];
            if (!ViewportHelper.IsAboveCameraView(_camera, tree.transform, 0.1f))
            {
                tree.SetActive(false);
                _inactiveTrees.Enqueue(tree);
                _activeTrees.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// Attempts to get a tree from <see cref="_inactiveTrees"/> and repositions it
    /// along the same row as the given <paramref name="pathStep"/>
    /// </summary>
    private void ActivateTrees(PathStep step)
    {
        if (_inactiveTrees.TryDequeue(out GameObject tree))
        {
            RepositionTree(tree, step);
            tree.SetActive(true);
            _activeTrees.Add(tree);
        }
    }
    #endregion

    #region Helpers
    /// <summary>
    /// Updates the direction of <see cref="_pathDirection"/> which will determine the position
    /// of steps added to <see cref="_safePath"/>.
    /// <see cref="_pathDirection"/> is only changed after a given number of calls to this method,
    /// which is determined by <see cref="_stepsSinceLastChange"/>. This is to prevent the path from
    /// changing direction too often, and create blocks of straights/curves
    /// </summary>
    private void UpdatePathDirection()
    {
        if (_stepsSinceLastChange == _nextStepChange)
        {
            _pathDirection = Random.Range(-1, 2);
            _nextStepChange = Random.Range(5, 17);
            _stepsSinceLastChange = 0;
        }

        _stepsSinceLastChange++;
    }

    /// <summary>
    /// Repositions the given path to the front (below) of the given previous step
    /// with a randomized position offset and width
    /// </summary>
    private void RepositionPath(PathStep currentStep, PathStep previousStep)
    {
        currentStep.transform.position = RandomizePathPosition(previousStep);
        // currentStep.Width = RandomPathWidth(previousStep);

        if (!currentStep.IsVisibleInCamera(_camera, 0.1f))
        {
            RecenterPath(currentStep);
        }
    }

    /// <summary>
    /// If the previous position is outside of the camera bounds, this method is called
    /// to recenter the path to the center of the screen
    /// </summary>
    private void RecenterPath(PathStep path)
    {
        path.transform.position = new Vector3(
            _camera.transform.position.x,
            path.transform.position.y,
            path.transform.position.z
        );
    }

    /// <summary>
    /// Repositions trees on either side of the given position
    /// </summary>
    private void RepositionTree(GameObject tree, PathStep step)
    {
        // Even numbers on the left, odds on the right
        float x = (Random.Range(0, 2) == 0)
            ? Random.Range(WorldBounds.xMin, step.LeftEdge - 0.5f)
            : Random.Range(step.RightEdge + 0.5f, WorldBounds.xMax);

        tree.transform.position = new Vector3(x, step.transform.position.y, tree.transform.position.z);
    }

    public GameObject Instance(GameObject prefab, float x = -1000, float y = 1000)
    {
        GameObject instance = Instantiate(prefab, new Vector2(x, y), Quaternion.identity);
        instance.transform.parent = _parent.transform;

        return instance;
    }
    #endregion
}
