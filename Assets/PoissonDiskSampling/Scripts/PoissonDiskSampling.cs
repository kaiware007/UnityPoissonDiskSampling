using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSampling : MonoBehaviour {

    struct GridIndex
    {
        public int x;
        public int y;
    }

    public float minDist = 1;    // 最小半径
    public float width = 10;
    public float height = 10;
    public int recursiveCount = 30;
    public bool isDispGrid = true;

    private Vector2[,] grid;
    private bool[,] enableGrid;

    private float gridSize;
    private int gridWidth, gridHeight;

    private List<Vector2> processList = new List<Vector2>();    // 候補リスト
    private List<Vector2> sampleList = new List<Vector2>();     // 確定した座標リスト
    
    void Initialize()
    {
        Sample();
    }

    void Sample()
    {
        gridSize = minDist / Mathf.Sqrt(2f);
        gridWidth = Mathf.CeilToInt(width / gridSize);
        gridHeight = Mathf.CeilToInt(height / gridSize);
        Debug.Log("gridSize " + gridSize + " gridWidth " + gridWidth + " gridHeight " + gridHeight);

        grid = new Vector2[gridWidth, gridHeight];
        enableGrid = new bool[gridWidth, gridHeight];

        processList.Clear();
        sampleList.Clear();

        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                grid[x, y] = Vector2.one * -10000f;
                enableGrid[x, y] = false;
            }
        }

        Vector2 firstPoint = new Vector2(Random.value * width, Random.value * height);
        processList.Add(firstPoint);
        sampleList.Add(firstPoint);
        GridIndex idx = GetGridIndex(firstPoint.x, firstPoint.y);
        grid[idx.x, idx.y] = firstPoint;
        enableGrid[idx.x, idx.y] = true;

        int count = 0;
        //while ((processList.Count > 0) && (count < 100))
        while (processList.Count > 0)
        {
            Vector2 pos = PopRandomProcessList();

            for (int i = 0; i < recursiveCount; i++)
            {
                Vector2 newPos = GenerateRandomPointAround(ref pos, minDist);

                // グリッドの範囲内か？
                if (!IsInGrid(ref newPos)) continue;

                // 周辺グリッドにある点が範囲外なら追加
                if (!IsInNeighborhood(ref newPos, minDist))
                {
                    processList.Add(newPos);
                    sampleList.Add(newPos);
                    GridIndex newIdx = GetGridIndex(newPos.x, newPos.y);
                    grid[newIdx.x, newIdx.y] = newPos;
                    enableGrid[newIdx.x, newIdx.y] = true;
                    continue;
                }
            }
            count++;    // 無限ループ対策
        }
        Debug.Log("sampleCount " + sampleList.Count);
    }

    /// <summary>
    /// グリッドのインデックス取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    GridIndex GetGridIndex(float x, float y)
    {
        GridIndex idx;
        idx.x = Mathf.FloorToInt(x / gridSize);
        idx.y = Mathf.FloorToInt(y / gridSize);
        return idx;
    }

    /// <summary>
    /// 候補リストからランダムに座標を取り出す
    /// </summary>
    /// <returns></returns>
    Vector2 PopRandomProcessList()
    {
        int idx = Random.Range(0, processList.Count);
        Vector2 pos = processList[idx];
        processList.RemoveAt(idx);  // 削除
        return pos;
    }

    /// <summary>
    /// 指定座標の周辺のランダムな座標を返す
    /// </summary>
    /// <param name="p"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    Vector2 GenerateRandomPointAround(ref Vector2 p, float minDistance)
    {
        return p + Random.insideUnitCircle * Random.Range(minDistance, minDistance * 2f);
    }

    /// <summary>
    /// グリッドの範囲内か？
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    bool IsInGrid(ref Vector2 p)
    {
        return (p.x >= 0f) && (p.x <= width) && (p.y >= 0f) && (p.y <= height);
    }

    /// <summary>
    /// 周辺グリッドの点が範囲内か？
    /// </summary>
    /// <param name="p"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    bool IsInNeighborhood(ref Vector2 p, float minDistance)
    {
        GridIndex idx = GetGridIndex(p.x, p.y);
        const int D = 2;

        int startX = Mathf.Max(idx.x - D, 0);
        int endX = Mathf.Min(idx.x + D, gridWidth);
        int startY = Mathf.Max(idx.y - D, 0);
        int endY = Mathf.Min(idx.y + D, gridHeight);
        //Debug.Log("x " + p.x + " " + p.y + " [" + idx.x + "," + idx.y + "] startX " + startX + " endX " + endX + " startY " + startY + " endY " + endY);
        for (int x = startX; x < endX; x++)
        {
            for(int y = startY; y < endY; y++)
            {
                //if ((x == idx.x) && (y == idx.y)) continue;
                if (!enableGrid[x, y]) continue;

                float distance = Vector2.Distance(p, grid[x, y]);
                if (distance < minDistance)
                {
                    //Debug.Log("newPos " + p + " grid[" + x + "," + y + "] " + grid[x, y] + " distance " + distance + " < " + minDistance);
                    return true;
                }
            }
        }

        //Debug.Log("[" + idx.x + "," + idx.y + "] p " + p.x + " " + p.y);
        return false;
    }

    // Use this for initialization
    void Start () {
        Initialize();
    }
	
	// Update is called once per frame
	void Update () {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Sample();
        }
	}

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (isDispGrid)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    for (int y = 0; y < gridHeight; y++)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(new Vector3(x * gridSize, 0, y * gridSize), new Vector3(gridSize, 0, gridSize));
                    }
                }
            }

            for(int i = 0; i < sampleList.Count; i++)
            {
                Gizmos.color = Color.red;
                Vector3 pos = new Vector3(sampleList[i].x, 0, sampleList[i].y);
                Gizmos.DrawCube(pos, Vector3.one * 0.1f);
                //Gizmos.color = Color.gray;
                //Gizmos.DrawWireSphere(pos, minDist);
#if UNITY_EDITOR
                if (isDispGrid)
                {
                    UnityEditor.Handles.color = Color.gray;
                    UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, minDist);
                }
#endif
            }
        }
    }
}
