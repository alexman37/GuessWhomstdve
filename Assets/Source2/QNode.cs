using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QNode : MonoBehaviour
{
    GameObject rootObject;                    // basic container for everything
    [SerializeField] GameObject coreContent;  // topmost content sprite, container for other content sprites
    [SerializeField] TextMeshProUGUI mainText;

    // keep track of which QNode we're currently interacting with
    // so when we do connections / lock-ons, we know which not to move
    public static QNode lastMoved;

    public int id;
    int verticalSize = 1;
    private string displayTxt = "DEFAULT_NODE";
    // TODO replace with NodeNub
    private QNodeType[] inputTypes;
    private QNodeType[] outputTypes;
    private QNode[] inputNodes;
    private QNode[] outputNodes;

    float contentWidth = 4;
    float nubWidth = 1;

    Vector2 gridPosition;

    bool isDragging = false;   // can also drag if locked to the selected node
    Vector3 offsetToMainDrag;
    bool draggingLocked;
    bool connectionLocked;

    // TODO! be smarter about when we actually have to recalculate this
    // all roots of all objects currently locked together with this node
    HashSet<GameObject> lockedRoots;

    // Non-usable
    Vector3 mouseDragPivot;

    public void setTypeArrays(QNodeCreateOrder creator)
    {
        verticalSize = creator.verticalSize;
        inputTypes = creator.inTypes;
        outputTypes = creator.outTypes;
        inputNodes = new QNode[verticalSize];
        outputNodes = new QNode[verticalSize];
    }

    private void setSortingOrder(GameObject go)
    {
        go.GetComponent<SpriteRenderer>().sortingOrder = id * 2;
    }

    /// <summary>
    /// Move this node into position so it aligns with another node
    /// </summary>
    public void LockOnto(QNode stationaryTarget, bool draggingInput, int inputIndex, int outputIndex)
    {
        // Dragging no longer effects position until object is let go of
        // TODO: It should instead drag if moved sufficiently far away enough
        draggingLocked = true;
        // Change position
        rootObject.transform.position = new Vector3(
            stationaryTarget.rootObject.transform.position.x - (contentWidth + nubWidth) * (draggingInput ? -1 : 1),
            stationaryTarget.rootObject.transform.position.y,
            0
        );
        // Set input / output nodes
        if(draggingInput)
        {
            inputNodes[inputIndex] = stationaryTarget;
            stationaryTarget.outputNodes[outputIndex] = this;
        } else
        {
            stationaryTarget.inputNodes[inputIndex] = this;
            outputNodes[outputIndex] = stationaryTarget;
        }
    }

    public void BreakLock(bool input, int index)
    {
        if(input)
        {
            inputNodes[index] = null;
        } else
        {
            outputNodes[index] = null;
        }
    }

    public GameObject generateVisual()
    {
        float nodeHeight = 1;
        float nodeSpacing = 0.23f;

        // If sprites use exact distances, they don't technically collide and so separation can be visible
        // We can move the sprites together by a teeny tiny amount to avoid this
        float epsilon = 0.00f;

        GameObject root = new GameObject(displayTxt);
        root.transform.position = gameObject.transform.position;

        GameObject rootContent = gameObject;
        setSortingOrder(rootContent);

        // multi core
        if (verticalSize > 1)
        {
            mainText.GetComponent<RectTransform>().parent.GetComponent<Canvas>().sortingOrder = id * 2 + 1;
            for (int i = 1; i < verticalSize - 1; i++)
            {
                GameObject nextCore = GameObject.Instantiate(NodeManager.instance.fragment_multi_mid, rootContent.transform);
                nextCore.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(nextCore);
            }

            GameObject lastCore = GameObject.Instantiate(NodeManager.instance.fragment_multi_bottom, rootContent.transform);
            lastCore.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * (verticalSize - 1), 0);
            setSortingOrder(lastCore);

            for (int i = 0; i < inputTypes.Length; i++)
            {
                QNodeType inputType = inputTypes[i];
                GameObject inNub = NodeManager.instance.NubFromNodeType(inputType);
                inNub.GetComponent<NodeNub>().Init(this, inputType, i);
                inNub.transform.SetParent(root.transform);
                inNub.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(inNub);
            }

            for (int i = 0; i < outputTypes.Length; i++)
            {
                QNodeType outputType = outputTypes[i];
                GameObject outNub = NodeManager.instance.NubFromNodeType(outputType);
                outNub.GetComponent<NodeNub>().Init(this, outputType, i);
                outNub.transform.SetParent(root.transform);
                outNub.transform.localPosition = new Vector3(contentWidth + nubWidth - epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(outNub);
            }
        }

        // single core
        else
        {
            mainText.GetComponent<RectTransform>().parent.GetComponent<Canvas>().sortingOrder = id * 2 + 1;
            if (inputTypes.Length > 0)
            {
                GameObject inNub = NodeManager.instance.NubFromNodeType(inputTypes[0]);
                inNub.GetComponent<NodeNub>().Init(this, inputTypes[0], 0);
                inNub.transform.SetParent(root.transform);
                inNub.transform.localPosition = new Vector3(epsilon, 0, 0);
                setSortingOrder(inNub);
            }

            if (outputTypes.Length > 0)
            {
                GameObject outNub = NodeManager.instance.NubFromNodeType(outputTypes[0]);
                outNub.GetComponent<NodeNub>().Init(this, outputTypes[0], 0);
                outNub.transform.SetParent(root.transform);
                outNub.transform.localPosition = new Vector3(contentWidth + nubWidth - epsilon, 0, 0);
                setSortingOrder(outNub);
            }
        }

        rootContent.transform.SetParent(root.transform);
        rootObject = root;
        return rootObject;
    }

    /// <summary>
    /// Recursive method to look through all locked input/output nodes and add them to lockedRoots set.
    /// Looks ugly, but in practice this should not take long at all
    /// </summary>
    void findLockedNodes(QNode fromLast, QNode trueStart)
    {
        foreach (QNode qNode in fromLast.inputNodes)
        {
            if(qNode != null)
            {
                Debug.Log("Input recognized: " + qNode.id);
                if (!qNode.isDragging)
                {
                    lockedRoots.Add(qNode.rootObject);
                    qNode.isDragging = true;
                    qNode.offsetToMainDrag = qNode.rootObject.transform.position - trueStart.rootObject.transform.position;
                    Debug.Log("Add to drag " + qNode.id);
                    findLockedNodes(qNode, trueStart);
                }
            }
        }
        foreach (QNode qNode in fromLast.outputNodes)
        {
            if (qNode != null)
            {
                Debug.Log("Output recognized: " + qNode.id);
                if (!qNode.isDragging)
                {
                    lockedRoots.Add(qNode.rootObject);
                    qNode.isDragging = true;
                    qNode.offsetToMainDrag = qNode.rootObject.transform.position - trueStart.rootObject.transform.position;
                    Debug.Log("Add to drag " + qNode.id);
                    findLockedNodes(qNode, trueStart);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lockedRoots = new HashSet<GameObject>();
    }

    private void Update()
    {
        if(isDragging)
        {
            rootObject.transform.position = lastMoved.rootObject.transform.position + offsetToMainDrag;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO clicking with other nodes
    }

    private void OnMouseDown()
    {
        lastMoved = this;
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -501);
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePosition);
        screenPos.z = 0;
        mouseDragPivot = screenPos - transform.position;

        // calculate lockedRoots
        lockedRoots.Clear();
        findLockedNodes(this, this);
    }

    private void OnMouseUp()
    {
        draggingLocked = false;

        isDragging = false;
        foreach(GameObject node in lockedRoots)
        {
            QNode qNode = node.GetComponentInChildren<QNode>();
            qNode.isDragging = false;
        }
    }

    private void OnMouseDrag()
    {
        if(!draggingLocked)
        {
            Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -500);
            Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePosition);
            screenPos.z = 0;
            rootObject.transform.position = screenPos - mouseDragPivot;
        }
    }
}


// Pass in this data when creating a QNode
public class QNodeCreateOrder
{
    public int verticalSize;
    public QNodeType[] inTypes;
    public QNodeType[] outTypes;

    public QNodeCreateOrder(int v, QNodeType[] inp, QNodeType[] outp)
    {
        verticalSize = v;
        inTypes = inp;
        outTypes = outp;
    }
}


public class QNodeType
{
    public QType mainType;
    public bool input;         // is input (if false, output)
    public bool isCollection;  // is multiple of this type
    public bool mandatory;     // must be supplied for the node using this to function
    
    public QNodeType(QType type, bool inp, bool coll, bool man)
    {
        mainType = type;
        input = inp;
        isCollection = coll;
        mandatory = man;
    }
}

public enum QType
{
    EOE,                // End of execution (output only)
    Action,             // Effect on a player
    Bool,               // T/F
    Characteristic,     // Characteristic (of a CPD)
    Player,             // Player in-game
    Subcat,             // Subcategory (of a CPD)
    Completed_Action,   // Already run action
    AnyType             // Any input type allowed (input only)
}