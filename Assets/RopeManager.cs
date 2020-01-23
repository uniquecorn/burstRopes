using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopeManager : MonoBehaviour
{
    public List<Rope> ropes;
    public TMPro.TextMeshProUGUI lineText;
    private void FixedUpdate()
    {
        if (ropes != null)
        {
            for (int i = 0; i < ropes.Count; i++)
            {
                ropes[i].RopeFixedUpdate();
            }
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
        ropes.Add(Instantiate(ropes[0], new Vector3(Random.Range(-5, 5f), Random.Range(-5, 5f), 0), Quaternion.identity));
        ropes[ropes.Count - 1].target.transform.position = new Vector3(Random.Range(-5, 5f), Random.Range(-5, 5f), 0);
        ropes[ropes.Count - 1].transform.name = ropes.Count.ToString();
        lineText.text = ropes.Count.ToString();
    }
}
