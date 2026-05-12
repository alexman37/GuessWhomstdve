using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class NodeNub : MonoBehaviour
{
    public QNode owner;
    public QNodeType data;
    public int nubIndex;
    public NodeNub connected = null;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(QNode o, QNodeType d, int i)
    {
        owner = o;
        data = d;
        nubIndex = i;
    }

    // Connect this nub to another, linking both their owning nodes together
    private void Connect(NodeNub otherNub)
    {
        // These nodes connect - lock them together
        connected = otherNub;
        otherNub.connected = this;
        // The one that's currently being moved should change position
        if (otherNub.owner.id == QNode.lastMoved.id)
        {
            // You drag output into input
            otherNub.owner.LockOnto(owner, false, otherNub.nubIndex, nubIndex);
        }
        else
        {
            // You drag input into output
            owner.LockOnto(otherNub.owner, true, nubIndex, otherNub.nubIndex);
        }
    }

    // Break this node's connection if it exists
    private void Break()
    {
        if(connected != null)
        {
            owner.BreakLock(data.input, nubIndex);
            connected.owner.BreakLock(connected.data.input, connected.nubIndex);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Both sides of the collision technically trigger, but we only need to check one
        if(!connected && data.input)
        {
            NodeNub otherNub = collision.gameObject.GetComponent<NodeNub>();
            if (otherNub != null && !otherNub.connected)
            {
                if (otherNub.data.mainType == data.mainType && otherNub.data.input != data.input)
                {
                    Connect(otherNub);
                }
            }
        }
    }

    // Right clicks break existing node connections
    void OnMouseOver()
    {
        if(Input.GetMouseButtonDown(1)){
            Break();
        }
    }
}
