using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctreeItem : MonoBehaviour
{
    //What nodes are we inside. 
    public List<OctreeNode> MyOwnerNodes = new List<OctreeNode>();

    private Vector3 PrevPos;


    // Start is called before the first frame update
    void Start()
    {
        PrevPos = transform.position;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (transform.position != PrevPos)
        {
            RefreshOwner();
            PrevPos = transform.position;
        }
    }

    public void RefreshOwner()
    {
        OctreeNode.OctreeRoot.ProcessItem(this);
        //store nodes that keep containing the item.
        List<OctreeNode> survivedNodes = new List<OctreeNode>();
        //during the fucntion store any nodes that are no longer containing the items in this list
        List<OctreeNode> obsoluteNodes = new List<OctreeNode>();
        foreach (OctreeNode on in MyOwnerNodes)
        {
            if (!on.ContainsItemPos(transform.position))
            {
                obsoluteNodes.Add(on);
            }
            else
            {
                survivedNodes.Add(on);
            }
        }

        MyOwnerNodes = survivedNodes;

        foreach (OctreeNode item in obsoluteNodes)
        {
            item.Attempt_ReduceSubdivisions(this);
        }
    }
}
