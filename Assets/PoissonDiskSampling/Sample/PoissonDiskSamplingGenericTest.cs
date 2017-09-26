using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoissonDiskSamplingGenericTest : MonoBehaviour {

    public float minDist = 1;    // 最小半径
    public float width = 10;
    public float height = 10;
    public int recursiveCount = 30;

    public bool isDispGrid = true;
    
    PoissonDiskSamplingGenericSample sampling = new PoissonDiskSamplingGenericSample();
    List<GameObject> objectList = new List<GameObject>();

    // Use this for initialization
    void Start()
    {
        Sample();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Sample();
        }
    }

    void Sample()
    {
        for(int i = 0; i < objectList.Count; i++)
        {
            Destroy(objectList[i]);
        }
        objectList.Clear();

        sampling.minDist = minDist;
        sampling.width = width;
        sampling.height = height;
        sampling.recursiveCount = recursiveCount;
        sampling.Sample();

        for (int i = 0; i < sampling.sampleList.Count; i++)
        {
            Vector3 pos = new Vector3(sampling.sampleList[i].position.x, 0, sampling.sampleList[i].position.y);
            GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            obj.transform.position = pos;
            obj.transform.localScale = Vector3.one * minDist * 0.5f;
            objectList.Add(obj);
        }
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
        {
            if (isDispGrid)
            {
                for (int x = 0; x < sampling.gridWidth; x++)
                {
                    for (int y = 0; y < sampling.gridHeight; y++)
                    {
                        Gizmos.color = Color.white;
                        Gizmos.DrawWireCube(new Vector3(x * sampling.gridSize, 0, y * sampling.gridSize), new Vector3(sampling.gridSize, 0, sampling.gridSize));
                    }
                }
            }

            for (int i = 0; i < sampling.sampleList.Count; i++)
            {
                Gizmos.color = Color.red;
                Vector3 pos = new Vector3(sampling.sampleList[i].position.x, 0, sampling.sampleList[i].position.y);
                Gizmos.DrawCube(pos, Vector3.one * 0.1f);
                //Gizmos.color = Color.gray;
                //Gizmos.DrawWireSphere(pos, minDist);
#if UNITY_EDITOR
                if (isDispGrid)
                {
                    UnityEditor.Handles.color = Color.gray;
                    UnityEditor.Handles.DrawWireDisc(pos, Vector3.up, sampling.minDist);
                }
#endif
            }
        }
    }
}
