using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneTransitionController : MonoBehaviour
{
    public static SceneTransitionController Instance { get; private set; }

    [Header("기본 참조")]
    [SerializeField] Canvas transitionCanvas;
    [SerializeField] RectTransform tilesRoot;
    [SerializeField] GridLayoutGroup grid;
    [SerializeField] Image tilePrefab;

    [Header("격자 설정")]
    [SerializeField] int columns = 10;
    [SerializeField] int rows = 5;

    [Header("트렌지션 설정")]
    [SerializeField] float stepDelay = 0.03f;
    [SerializeField] float tileAnimTime = 0.12f;
    [SerializeField] float phaseGap = 0.05f;

    RectTransform[,] tiles;
    bool isBusy;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        DontDestroyOnLoad(transitionCanvas.gameObject);

        transitionCanvas.gameObject.SetActive(false);
        BuildTiles();
    }

    void OnValidate()
    {
        if (grid != null)
        {
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = columns;
            grid.spacing = Vector2.zero;
        }
    }

    void BuildTiles()
    {
        if (tilesRoot == null || grid == null || tilePrefab == null) return;

        for (int i = tilesRoot.childCount - 1; i >= 0; i--)
            DestroyImmediate(tilesRoot.GetChild(i).gameObject);

        UpdateCellSize();

        tiles = new RectTransform[rows, columns];

        for (int y = 0; y < rows; y++)
        for (int x = 0; x < columns; x++)
        {
            var img = Instantiate(tilePrefab, tilesRoot);
            img.name = $"Tile_{x}_{y}";
            var rt = img.rectTransform;
            rt.localScale = Vector3.zero;
            tiles[y, x] = rt;
        }
    }

    void UpdateCellSize()
    {
        float w = Screen.width / (float)columns;
        float h = Screen.height / (float)rows;
        grid.cellSize = new Vector2(w, h);
    }

    public void LoadScene(string sceneName)
    {
        if (isBusy) return;
        StartCoroutine(TransitionRoutine(sceneName));
    }

    IEnumerator TransitionRoutine(string sceneName)
    {
        isBusy = true;

        transitionCanvas.gameObject.SetActive(true);
        UpdateCellSize();

        yield return FillDiagonal();
        yield return new WaitForSecondsRealtime(phaseGap);

        var op = SceneManager.LoadSceneAsync(sceneName);
        while (!op.isDone) yield return null;

        yield return new WaitForSecondsRealtime(phaseGap);
        yield return ClearFromCenter();

        transitionCanvas.gameObject.SetActive(false);
        isBusy = false;
    }

    IEnumerator FillDiagonal()
    {
        for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
                tiles[y, x].localScale = Vector3.zero;

        int maxStep = (columns - 1) + (rows - 1);

        for (int step = 0; step <= maxStep; step++)
        {
            for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                if (x + y == step)
                    StartCoroutine(ScaleTile(tiles[y, x], Vector3.zero, Vector3.one, tileAnimTime));
            }

            yield return new WaitForSecondsRealtime(stepDelay);
        }

        yield return new WaitForSecondsRealtime(tileAnimTime);
    }

    IEnumerator ClearFromCenter()
    {
        float cx = (columns - 1) / 2f;
        float cy = (rows - 1) / 2f;

        float maxDist = 0f;
        for (int y = 0; y < rows; y++)
        for (int x = 0; x < columns; x++)
            maxDist = Mathf.Max(maxDist, Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy)));

        int steps = Mathf.CeilToInt(maxDist * 2f);

        for (int s = 0; s <= steps; s++)
        {
            float threshold = (s / (float)steps) * maxDist + 0.001f;

            for (int y = 0; y < rows; y++)
            for (int x = 0; x < columns; x++)
            {
                float d = Vector2.Distance(new Vector2(x, y), new Vector2(cx, cy));
                if (d <= threshold && tiles[y, x].localScale.x > 0.01f)
                    StartCoroutine(ScaleTile(tiles[y, x], Vector3.one, Vector3.zero, tileAnimTime));
            }

            yield return new WaitForSecondsRealtime(stepDelay);
        }

        yield return new WaitForSecondsRealtime(tileAnimTime);
    }

    IEnumerator ScaleTile(RectTransform rt, Vector3 from, Vector3 to, float time)
    {
        float t = 0f;
        rt.localScale = from;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.0001f, time);
            float eased = t * t * (3f - 2f * t);
            rt.localScale = Vector3.LerpUnclamped(from, to, eased);
            yield return null;
        }
        rt.localScale = to;
    }
}