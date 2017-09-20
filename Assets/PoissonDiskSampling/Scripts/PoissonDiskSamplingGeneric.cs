using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSamplingPositionData
{
    public bool enable = false;
    public Vector2 position;

    public PoissonDiskSamplingPositionData()
    {
        enable = false;
        position = Vector2.zero;
    }

    public virtual void SetPosition(Vector2 pos)
    {
        enable = true;
        position = pos;
    }

}

public abstract class PoissonDiskSamplingGeneric<T> where T : PoissonDiskSamplingPositionData, new()
{

    protected struct GridIndex
    {
        public int x;
        public int y;
    }

    public float minDist = 1;    // 最小半径
    public float width = 10;
    public float height = 10;
    public int recursiveCount = 30;

    protected T[,] grid;

    protected float gridSize_;
    protected int gridWidth_, gridHeight_;

    protected List<T> processList_ = new List<T>();    // 候補リスト
    protected List<T> sampleList_ = new List<T>();     // 確定した座標リスト

    public float gridSize { get { return gridSize_; } }
    public float gridWidth { get { return gridWidth_; } }
    public float gridHeight { get { return gridHeight_; } }
    public List<T> sampleList { get { return sampleList_; } }

    virtual protected void InitializeGrid()
    {
        for (int x = 0; x < gridWidth_; x++)
        {
            for (int y = 0; y < gridHeight_; y++)
            {
                grid[x, y] = new T();
            }
        }
    }

    protected virtual void SetPoint(T point)
    {
        processList_.Add(point);
        sampleList_.Add(point);
        GridIndex newIdx = GetGridIndex(point.position.x, point.position.y);
        grid[newIdx.x, newIdx.y] = point;
    }

    protected virtual T GeneratePoint(Vector2 pos)
    {
        T point = new T();
        point.SetPosition(pos);
        return point;
    }

    protected virtual void InitializeFirstPoint()
    {
        T firstPoint = GeneratePoint(new Vector2(Random.value * width, Random.value * height));
        SetPoint(firstPoint);
        //return firstPoint;
    }

    protected virtual float GetminDist(T point)
    {
        return minDist;
    }

    public virtual void Sample()
    {
        gridSize_ = minDist / Mathf.Sqrt(2f);
        gridWidth_ = Mathf.CeilToInt(width / gridSize_);
        gridHeight_ = Mathf.CeilToInt(height / gridSize_);
        Debug.Log("gridSize " + gridSize_ + " gridWidth " + gridWidth_ + " gridHeight " + gridHeight_);

        grid = new T[gridWidth_, gridHeight_];

        processList_.Clear();
        sampleList_.Clear();

        InitializeGrid();

        //T firstPoint = InitializeFirstPoint();
        InitializeFirstPoint();

        while (processList_.Count > 0)
        {
            T pos = PopRandomProcessList();
            float mds = GetminDist(pos);

            for (int i = 0; i < recursiveCount; i++)
            {
                T newPos = GenerateRandomPointAround(pos, mds);
                float newmds = GetminDist(newPos);

                // グリッドの範囲内か？
                if (!IsInGrid(newPos, newmds)) continue;

                // 周辺グリッドにある点が範囲外なら追加
                if (!IsInNeighborhood(newPos, newmds))
                {
                    SetPoint(newPos);
                    continue;
                }
            }
        }
    }

    /// <summary>
    /// グリッドのインデックス取得
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    protected GridIndex GetGridIndex(float x, float y)
    {
        GridIndex idx;
        idx.x = Mathf.FloorToInt(x / gridSize_);
        idx.y = Mathf.FloorToInt(y / gridSize_);
        return idx;
    }

    /// <summary>
    /// 候補リストからランダムに座標を取り出す
    /// </summary>
    /// <returns></returns>
    protected virtual T PopRandomProcessList()
    {
        int idx = Random.Range(0, processList_.Count);
        T pos = processList_[idx];
        processList_.RemoveAt(idx);  // 削除
        return pos;
    }

    /// <summary>
    /// 指定座標の周辺のランダムな座標を返す
    /// </summary>
    /// <param name="p"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    protected virtual T GenerateRandomPointAround(T p, float minDistance)
    {
        float rad = Random.value * 2f * Mathf.PI;
        float length = minDistance + Random.value * minDistance;
        Vector2 pos;
        pos.x = Mathf.Cos(rad) * length;
        pos.y = Mathf.Sin(rad) * length;
        T newP = GeneratePoint(p.position + pos);
        return newP;
    }

    /// <summary>
    /// グリッドの範囲内か？
    /// </summary>
    /// <param name="p"></param>
    /// <returns></returns>
    protected virtual bool IsInGrid(T p, float minDistance)
    {
        return (p.position.x >= 0f) && (p.position.x <= width) && (p.position.y >= 0f) && (p.position.y <= height);
    }

    /// <summary>
    /// POINT同士の距離の比較関数
    /// </summary>
    /// <param name="p"></param>
    /// <param name="p2"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    protected virtual bool IsInDistance(T p, T p2, float minDistance)
    {
        float distance = Vector2.Distance(p.position, p2.position);
        if (distance < minDistance)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 検索するグリッド数
    /// </summary>
    /// <returns></returns>
    protected virtual int GetSearchGridNum()
    {
        return 5;
    }

    /// <summary>
    /// 周辺グリッドの点が範囲内か？
    /// </summary>
    /// <param name="p"></param>
    /// <param name="minDistance"></param>
    /// <returns></returns>
    protected virtual bool IsInNeighborhood(T p, float minDistance)
    {
        GridIndex idx = GetGridIndex(p.position.x, p.position.y);
        int D = GetSearchGridNum();
        int startX = Mathf.Max(idx.x - D, 0);
        int endX = Mathf.Min(idx.x + D, gridWidth_);
        int startY = Mathf.Max(idx.y - D, 0);
        int endY = Mathf.Min(idx.y + D, gridHeight_);

        for (int x = startX; x < endX; x++)
        {
            for(int y = startY; y < endY; y++)
            {
                if (!grid[x, y].enable) continue;

                if(IsInDistance(p, grid[x, y], minDistance))
                {
                    return true;
                }
                //float distance = Vector2.Distance(p.position, grid[x, y].position);
                //if (distance < minDistance)
                //{
                //    return true;
                //}
            }
        }

        return false;
    }
}
