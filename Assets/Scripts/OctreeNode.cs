using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//The best way that came up after my research on web is using Octree system. Apparently there are multiple octree systems exist like:
//* Standard representation
//* Block representation
//* Sibling child representation
//* Implicit node representations
//
//I have to say the system looks quiet complex. Ive never done such a thing before. I understand the basic concept and watched few tutorials about it. 
//In this project you will find 2 version of octree. first one i found is a bit simpler than the second one i wrote. 
//Nevertheless in both cases main concept is, have a cube, divide that cube to 8 nodes, and in each of those nodes divide them again to 8. System goes like that in depth. 
//Unfortunatley despite you gave me 5 days for the test, i have multiple test to do for different places so i only managed to do this on friday after work and most of the saturday.
//Also the reason i provided this code to you is to show you that i can get the job done. I look through the web and found the code and apply it. I wouldnt be able to write the code by myself
//of the top of my head. But i can find answers to the questions, research about them and get the result.
//
//The code provided below with few other scripts provide few interactions to the user
// You can move around with WASD
//You can look around with mouse
//You can instantiate objects with left mouse
//You can move them around by right mouse button
//You can delete them by pressing E
//You can push them by pressing R
//For deleting or pushing you need to highlight the object which happens when the middle of the screen targets the cubes
//
//Current code can give exception if the cubes spawned too close. Its trying to divide the nodes too small so unity gives errors. This can be overcome by adding a limit for min splitting amount.
public class OctreeNode
{
    //How many items can the octree nodes contain before giving birth to 8 smaller children? by splitting
    public static int MaxObjectLimit = 1;
    //Internal variable (Store the link to the root of the entire octree)
    private static OctreeNode _OctreeRoot;
    //Allows anyone to read the information about the root
    public static OctreeNode OctreeRoot
    {
        get
        {
            if (_OctreeRoot == null)
            {
                _OctreeRoot = new OctreeNode(null, Vector3.zero, 15f,new List<OctreeItem>());
            }
            return _OctreeRoot;
        }
    }
    //The gameobject in change of displaying the boundaries of this particular node
    GameObject OctantGo;
    //to be initialized upon the creation of this node
    LineRenderer OctantLineRenderer;

    //The length from the center of node Node to one of its walls
    public float HalfDimentionLength;
    //The center of this node
    private Vector3 Pos;
    //What octree node is the parent of this octree node?
    public OctreeNode Parent;
    //What items are contained in this particular octree node?
    public List<OctreeItem> ContainedItems = new List<OctreeItem>();
    //Will be available to store links to any future children
    private OctreeNode[] _ChildrenNodes = new OctreeNode[8];

    public OctreeNode[] ChildrenNodes
    {
        get { return _ChildrenNodes; }

    }
    public void EreaseChildrennodes()
    {
        _ChildrenNodes = new OctreeNode[8];
    }
    [RuntimeInitializeOnLoadMethod]
    static bool Init()
    {
        return OctreeRoot == null;
    }
    //Constructer
    public OctreeNode(OctreeNode parent, Vector3 ThisChildPos, float ThisChildHalfLength, List<OctreeItem> potential_items)
    {
        //tell this particular node who is its parent
        this.Parent = parent;
        //how far from the center of this node to its walls
        HalfDimentionLength = ThisChildHalfLength;
        //the coordinate of the center of this new node
        Pos = ThisChildPos;

        OctantGo = new GameObject();
        OctantGo.hideFlags = HideFlags.HideInHierarchy;
        OctantLineRenderer = OctantGo.AddComponent<LineRenderer>();
        //fill the coordinates for the line renderer
        FillCube_VisualizeCoords();

        foreach (OctreeItem item in potential_items)
        {
            //Check if the item really belongs to this particular node
            ProcessItem(item);
        }
    }
    //see if a given item should be stored in containeditems list of items for this particular node
    public bool ProcessItem(OctreeItem item)
    {
        //check if this particular node contains position
        if (ContainsItemPos(item.transform.position))
        {
            //Check if this node doesnt have children
            if (ReferenceEquals(ChildrenNodes[0], null))
            {
                PushItem(item);
                return true;
            }
            else
            {
                foreach (OctreeNode childNode in ChildrenNodes)
                {
                    if (childNode.ProcessItem(item))
                    {
                        return true;
                    }
                }
            }            
        }
        //end if the item is inside of this particular node
        return false;
    }
    //We know that an item should be accquired and contained by this node work on adding it
    private void PushItem(OctreeItem item)
    {
        //only add it to our list of contained items if its not mentioned yet within the list
        if (!ContainedItems.Contains(item))
        {
            ContainedItems.Add(item);
            item.MyOwnerNodes.Add(this);
        }
        if (ContainedItems.Count > MaxObjectLimit)
        {
            Split();
        }
    }

    private void Split()
    {
        //for every item which was contained in this spliting node
        //make the item forget about this particuler node. 
        foreach (OctreeItem item in ContainedItems)
        {
            item.MyOwnerNodes.Remove(this);
        }
        //point the vector towards the top right future child origin
        Vector3 positionVector = new Vector3(HalfDimentionLength / 2, HalfDimentionLength / 2, HalfDimentionLength / 2);
        
        for (int i = 0; i < 4; i++)
        {
            _ChildrenNodes[i] = new OctreeNode(this, Pos + positionVector, HalfDimentionLength / 2, ContainedItems);
            positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;
        }
        //point the vector towards the bottom right future child origin
        positionVector = new Vector3(HalfDimentionLength / 2, -HalfDimentionLength / 2, HalfDimentionLength / 2);

        for (int i = 4; i < 8; i++)
        {
            _ChildrenNodes[i] = new OctreeNode(this, Pos + positionVector, HalfDimentionLength / 2, ContainedItems);
            positionVector = Quaternion.Euler(0f, -90f, 0f) * positionVector;
        }

        ContainedItems.Clear();
    }

    public void Attempt_ReduceSubdivisions(OctreeItem escapedItem)
    {
        if (!ReferenceEquals(this, OctreeRoot) && !Siblings_ChildrenNodesPresent_TooManyItems())
        {
            //iterate thorugh this node and its 7 siblings then kill them
            foreach (OctreeNode item in Parent.ChildrenNodes)
            {
                item.KillNode(Parent.ChildrenNodes.Where(i=> !ReferenceEquals(i, this)).ToArray());
            }

            Parent.EreaseChildrennodes();

        }
        else//otherwise if there are children in siblings or there are too many items for the parent to potentially hold them
        {  //remove the item from the contained items of this particuar node
            ContainedItems.Remove(escapedItem);
            escapedItem.MyOwnerNodes.Remove(this);
        }
      
    }

    private void KillNode(OctreeNode[] obsoleteNodes)
    {
        //for every item in this about to be deleted obsolete node do
        foreach (OctreeItem item in ContainedItems)
        {
            //from such items owner node extract a list excluding all the siblings
            //of this obsolete node. Then re assign such list to the owner nodes of that item
            item.MyOwnerNodes = item.MyOwnerNodes.Except(obsoleteNodes).ToList();
            //and remove this node as well after removing its 7 siblings
            item.MyOwnerNodes.Remove(this);

            item.MyOwnerNodes.Add(Parent);

            Parent.ContainedItems.Add(item);
            GameObject.Destroy(OctantGo);
        }
       
        GameObject.Destroy(OctantGo);
    }


    //true if the children nodes are present in siblings of this particuler obsolete node 
    //or if their total number of items is way too much for parent to accept
    private bool Siblings_ChildrenNodesPresent_TooManyItems()
    {
        //items contained in this obsolete node and the siblings
        List<OctreeItem> legacyItems = new List<OctreeItem>();

        //iterate throgh siblings and see if they have any children
        foreach (OctreeNode item in Parent.ChildrenNodes)
        {
            //if they do have children then return true
            if (!ReferenceEquals(item.ChildrenNodes[0], null))
            {
                Debug.Log("We have too many children");
                return true;
            }
            //add all items from the currently inspected sibling. Add only the items not already contained in our contained in our legacy items list.
            legacyItems.AddRange(item.ContainedItems.Where(i => !legacyItems.Contains(i)));
        }
        //too many items for the parent to hold. Dont get rid of siblings and this particular obsolete node.
        if (legacyItems.Count > MaxObjectLimit+1)
        {
            Debug.Log("We have too many items " + legacyItems.Count);
            return true;
        }

        //having looked at all the siblings and none of them containing child nodes. Thier items altogether could be held by parent. So get rid of this particular node and those sibling nodes.
        return false;
    }

    //Check if a given coordinate is inside of this particular node
    public bool ContainsItemPos(Vector3 itemPos)
    {
        if (itemPos.x > Pos.x + HalfDimentionLength || itemPos.x < Pos.x - HalfDimentionLength)
        {
            return false;
        }
        if (itemPos.y > Pos.y + HalfDimentionLength || itemPos.y < Pos.y - HalfDimentionLength)
        {
            return false;
        }
        if (itemPos.z > Pos.z + HalfDimentionLength || itemPos.z < Pos.z - HalfDimentionLength)
        {
            return false;
        }
        return true;
    }

    //Fill the coordinates for the line renderer
    private void FillCube_VisualizeCoords()
    {
        Vector3[] Cubecoords = new Vector3[8];
        Vector3 Corner = new Vector3(HalfDimentionLength, HalfDimentionLength, HalfDimentionLength);//top top right corner

        //populate the first half of the cube coords
        for (int x = 0; x < 4; x++)
        {
            Cubecoords[x] = Pos + Corner;
            Corner = Quaternion.Euler(0, 90, 0f) * Corner;
        }

        Corner = new Vector3(HalfDimentionLength, -HalfDimentionLength, HalfDimentionLength);//bot top right

        for (int x = 4; x < 8; x++)
        {
            //relative to the position of this particular node and not the worlds zeroth coordinate
            Cubecoords[x] = Pos + Corner;
            //rotate around the verical axis, pointing to the remaining corners of the cube
            Corner = Quaternion.Euler(0, 90, 0f) * Corner;
        }

        OctantLineRenderer.useWorldSpace = true;
        OctantLineRenderer.positionCount = (16);
        OctantLineRenderer.startWidth = 0.03f;
        OctantLineRenderer.endWidth = 0.03f;

        OctantLineRenderer.SetPosition(0, Cubecoords[0]);
        OctantLineRenderer.SetPosition(1, Cubecoords[1]);
        OctantLineRenderer.SetPosition(2, Cubecoords[2]);
        OctantLineRenderer.SetPosition(3, Cubecoords[3]);
        OctantLineRenderer.SetPosition(4, Cubecoords[0]);
        OctantLineRenderer.SetPosition(5, Cubecoords[4]);
        OctantLineRenderer.SetPosition(6, Cubecoords[5]);
        OctantLineRenderer.SetPosition(7, Cubecoords[1]);
        OctantLineRenderer.SetPosition(8, Cubecoords[5]);
        OctantLineRenderer.SetPosition(9, Cubecoords[6]);
        OctantLineRenderer.SetPosition(10, Cubecoords[2]);
        OctantLineRenderer.SetPosition(11, Cubecoords[6]);
        OctantLineRenderer.SetPosition(12, Cubecoords[7]);
        OctantLineRenderer.SetPosition(13, Cubecoords[3]);
        OctantLineRenderer.SetPosition(14, Cubecoords[7]);
        OctantLineRenderer.SetPosition(15, Cubecoords[4]);
    }
}
