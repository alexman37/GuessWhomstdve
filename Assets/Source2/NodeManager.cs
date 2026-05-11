using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeManager : MonoBehaviour
{
    public static NodeManager instance;

    public Dictionary<int, QNode> graph = new Dictionary<int, QNode>();
    static int totalNodeCount = 0;
    const int maxTotalNodes = 10000;

    [SerializeField] public GameObject qNode_single;
    [SerializeField] public GameObject qNode_multi;

    [SerializeField] public GameObject fragment_multi_mid;
    [SerializeField] public GameObject fragment_multi_bottom;

    [SerializeField] GameObject qNode_nub;
    [SerializeField] Sprite[] nubSprites_in;
    [SerializeField] Sprite[] nubSprites_out;

    // Start is called before the first frame update
    void Start()
    {
        if (instance == null) instance = this;
        else Destroy(this);

        QNodeType qt1 = new QNodeType(QType.Characteristic, true, false, false);
        QNodeType qt2 = new QNodeType(QType.Subcat, false, false, false);
        QNodeType qt1a = new QNodeType(QType.Action, true, false, false);
        QNodeType qt2a = new QNodeType(QType.Completed_Action, false, false, false);
        QNodeType qt1b = new QNodeType(QType.Player, true, false, false);
        QNodeType qt2b = new QNodeType(QType.Bool, false, false, false);
        QNodeType qt1c = new QNodeType(QType.AnyType, true, false, false);
        QNodeType qt2c = new QNodeType(QType.EOE, false, false, false);
        //QNode q = new QNode(4, new QNodeType[] { qt1, qt1a, qt1b, qt1c }, new QNodeType[] { qt2, qt2a, qt2b, qt2c });
        //QNode q2 = new QNode(4, new QNodeType[] { qt1, qt1a, qt1b, qt1c }, new QNodeType[] { qt2, qt2a, qt2b, qt2c });
        QNodeCreateOrder q1 = new QNodeCreateOrder(4, new QNodeType[] { qt1, qt1a, qt1b, qt1c }, new QNodeType[] { qt2, qt2a, qt2b, qt2c });
        QNodeCreateOrder q2 = new QNodeCreateOrder(4, new QNodeType[] { qt1, qt1a, qt1b, qt1c }, new QNodeType[] { qt2, qt2a, qt2b, qt2c });
        AddNodeToGraph(q1);
        AddNodeToGraph(q2);
    }

    public void AddNodeToGraph(QNodeCreateOrder q)
    {
        // generate core part of node
        GameObject root;
        if (q.verticalSize > 1)
        {
            root = GameObject.Instantiate(qNode_multi);
        } else
        {
            root = GameObject.Instantiate(qNode_single);
        }
        QNode qNode = root.GetComponent<QNode>();
        qNode.setTypeArrays(q);

        // finish generation
        qNode.id = (++totalNodeCount) % maxTotalNodes;
        qNode.generateVisual();
        graph.Add(qNode.id, qNode);
    }

    public void RemoveNodeFromGraph(int id)
    {
        graph.Remove(id);
    }

    public GameObject NubFromNodeType(QNodeType qNodeType)
    {
        GameObject nub = GameObject.Instantiate(qNode_nub);
        SpriteRenderer rend = nub.GetComponent<SpriteRenderer>();

        // TODO something different if array, and if mandatory
        if(qNodeType.input)
        {
            rend.sprite = nubSprites_in[(int)qNodeType.mainType];
        } else
        {
            rend.sprite = nubSprites_out[(int)qNodeType.mainType];
        }

        return nub;
    }
}
