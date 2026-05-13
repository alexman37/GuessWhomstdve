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

    private static object connectionProcessor_lockObj = new object();

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
    public void Connect(NodeNub otherNub)
    {
        // These nodes connect - lock them together
        connected = otherNub;
        otherNub.connected = this;
    }

    // Break this node's connection if it exists
    public void Break()
    {
        if(connected != null)
        {
            owner.BreakLock(data.input, nubIndex);
            connected.owner.BreakLock(connected.data.input, connected.nubIndex);

            BreakOnlyConnections();
        }
    }

    public void BreakOnlyConnections()
    {
        if(connected != null)
        {
            connected.connected = null;
            connected = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        lock (connectionProcessor_lockObj)
        {
            // Both sides of the collision technically trigger, but we only need to check one
            if (!connected && data.input)
            {
                Debug.Log("****** A collision check");
                NodeNub otherNub = collision.gameObject.GetComponent<NodeNub>();
                if (otherNub != null && !otherNub.connected)
                {
                    // Must verify one node is input and the other is output first.
                    if (data.input != otherNub.data.input)
                    {
                        owner.CheckValidConnection(this, otherNub);
                        // TODO run for all nubs connected
                        //Connect(otherNub);
                    }
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
