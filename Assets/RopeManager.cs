using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RopeManager : MonoBehaviour
{
    public List<Rope> ropes;
    public TMPro.TextMeshProUGUI lineText;
    private void FixedUpdate()
    {
        if (ropes != null)
        {
            NativeArray<JobHandle> allHandles = new NativeArray<JobHandle>(ropes.Count, Allocator.Temp);
            for (int i = 0; i < ropes.Count; i++)
            {
                allHandles[i] = ropes[i].SimulateBurst();
            }
            JobHandle.CompleteAll(allHandles);
        }
    }
    private void Update()
    {
        if (ropes != null)
        {
            for (int i = 0; i < ropes.Count; i++)
            {
                ropes[i].RopeUpdate();
            }
        }
    }

    public void AddRope()
    {
        ropes.Add(Instantiate(ropes[0], new Vector3(Random.Range(-5, 5f), Random.Range(-5, 5f), Random.Range(-5, 5f)), Quaternion.identity));
        ropes[ropes.Count - 1].target.transform.position = new Vector3(Random.Range(-5, 5f), Random.Range(-5, 5f), Random.Range(-5, 5f));
        ropes[ropes.Count - 1].transform.name = ropes.Count.ToString();
        lineText.text = ropes.Count.ToString();
    }
    public void Add10Ropes()
    {
        for (int i = 0; i < 10; i++)
        {
            AddRope();
        }
    }
    public void Add100Ropes()
    {
        for (int i = 0; i < 100; i++)
        {
            AddRope();
        }
    }
}
