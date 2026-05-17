using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QNode : MonoBehaviour
{
    GameObject rootObject;  // basic container for everything
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
    private NodeNub[] inputNubs;
    private NodeNub[] outputNubs;

    // Just how many entries in above arrays are non-null
    private int realInputTypes;
    private int realOutputTypes;
    private int realInputNodes;
    private int realOutputNodes;

    float contentWidth = 4;
    float nubWidth = 1;
    float nodeHeight = 1;
    float nodeSpacing = 0.23f;

    Vector2 gridPosition;

    bool isDragging = false;   // can also drag if locked to the selected node
    Vector3 offsetToMainDrag;
    bool draggingLocked;
    //public bool connectionLocked;

    // TODO! be smarter about when we actually have to recalculate this
    // all roots of all objects currently locked together with this node
    HashSet<GameObject> lockedRoots;
    HashSet<int> lockedIDs;

    // Non-usable
    Vector3 mouseDragPivot;

    public void setTypeArrays(QNodeCreateOrder creator)
    {
        verticalSize = creator.verticalSize;
        inputTypes = creator.inTypes;
        outputTypes = creator.outTypes;
        inputNodes = new QNode[verticalSize];
        outputNodes = new QNode[verticalSize];
        inputNubs = new NodeNub[verticalSize];
        outputNubs = new NodeNub[verticalSize];

        foreach (QNodeType nodeType in inputTypes)
        {
            if (nodeType != null) realInputTypes++;
        }
        foreach (QNodeType nodeType in outputTypes)
        {
            if (nodeType != null) realOutputTypes++;
        }
    }

    private void setSortingOrder(GameObject go)
    {
        go.GetComponent<SpriteRenderer>().sortingOrder = id * 2;
    }

    private bool isEmptyNub(NodeNub nub)
    {
        return nub == null || nub.data.mainType == QType.EOE;
    }


    // Check if two nodes, potentially multi-nodes, should be connected based on this nub connection
    public void CheckValidConnection(NodeNub inputNub, NodeNub otherNub)
    {
        // We already assured this is the input (right) node, and otherNub belongs to the output (left) node

        // If there's only one output nub, all we have to do is make sure the input nub matches type
        if(otherNub.owner.realOutputTypes == 1)
        {
            Debug.Log("[N] Fast-tracked checking");
            if(otherNub.data.mainType == inputTypes[inputNub.nubIndex].mainType)
            {
                inputNub.Connect(otherNub);
            }
        } 
        // Else, make sure every output nub either has a matching input nub in the right spot, or, nothing at all
        else
        {
            // For multi-nub connections: (output, input)
            List<(NodeNub, NodeNub)> nubPairs = new List<(NodeNub, NodeNub)>();

            QNodeType[] outputs = otherNub.owner.outputTypes;
            // Add this to output nub index to get the corresponding input nub index
            int mapOutputToInput = inputNub.nubIndex - otherNub.nubIndex;

            for (int o = 0; o < outputs.Length; o++)
            {
                // Must be either the same type, or one does not exist
                if (outputs[o] != null && outputs[o].mainType != QType.EOE)
                {
                    int inputIndex = o + mapOutputToInput;
                    if (inputIndex >= 0 && inputIndex < inputTypes.Length)
                    {
                        if (inputTypes[inputIndex] != null && inputTypes[inputIndex].mainType != QType.EOE)
                        {
                            if (inputTypes[inputIndex].mainType != outputs[o].mainType)
                            {
                                Debug.Log("[N] These multi-nodes do not connect in one or more crucial places");
                                return;
                            } else
                            {
                                nubPairs.Add((otherNub, inputNubs[inputIndex]));
                            }
                        }
                    }
                }
            }
            Debug.Log("[N] Passed all checks, these multi-nodes connect");
            foreach((NodeNub, NodeNub) pair in nubPairs)
            {
                Debug.Log("CONNECTIONED");
                pair.Item1.Connect(pair.Item2);
            }
        }

        // Ultimately, the one that's currently being moved should change position
        LockOnto(otherNub.owner, inputNub.nubIndex, otherNub.nubIndex);
    }

    /// <summary>
    /// Move this node into position so it aligns with another node
    /// </summary>
    public void LockOnto(QNode outputNode, int inputIndex, int outputIndex)
    {
        // Dragging no longer effects position until object is let go of
        // TODO: It should instead drag if moved sufficiently far away enough
        lastMoved.draggingLocked = true;
        // Case 1: Directly drag input into output
        if(lastMoved.id == id)
        {
            Debug.Log("CASE 1");
            Vector3 oldPos = rootObject.transform.position;
            Vector3 newPos = new Vector3(
                outputNode.rootObject.transform.position.x - (contentWidth + nubWidth) * -1,
                outputNode.rootObject.transform.position.y - (nodeHeight + nodeSpacing) * (outputIndex - inputIndex),
                0
            );
            rootObject.transform.position = newPos;
        }
        // Case 2: Directly drag output into input
        else if(lastMoved.id == outputNode.id)
        {
            Debug.Log("CASE 2");
            Vector3 oldPos = outputNode.rootObject.transform.position;
            Vector3 newPos = new Vector3(
                rootObject.transform.position.x - (contentWidth + nubWidth) * 1,
                rootObject.transform.position.y - (nodeHeight + nodeSpacing) * (outputIndex - inputIndex),
                0
            );
            outputNode.rootObject.transform.position = newPos;
        }
        // Case 3: Indirect dragging
        else
        {
            // 3.1: last moved is attached to input node
            Debug.Log("CASE 3: There are " + lastMoved.lockedIDs.Count);
            if(lastMoved.lockedIDs.Contains(id))
            {
                Debug.Log("CASE 3.1: Part of input");
                Vector3 oldPos = rootObject.transform.position;
                Vector3 newPos = new Vector3(
                    outputNode.rootObject.transform.position.x - (contentWidth + nubWidth) * -1,
                    outputNode.rootObject.transform.position.y - (nodeHeight + nodeSpacing) * (outputIndex - inputIndex),
                    0
                );
                rootObject.transform.position = newPos;
                lastMoved.rootObject.transform.position += (newPos - oldPos);
            }

            // 3.2: last moved is attached to output node
            else
            {
                Debug.Log("CASE 3.2: Part of output");
                Vector3 oldPos = outputNode.rootObject.transform.position;
                Vector3 newPos = new Vector3(
                    rootObject.transform.position.x - (contentWidth + nubWidth) * 1,
                    rootObject.transform.position.y - (nodeHeight + nodeSpacing) * (outputIndex - inputIndex),
                    0
                );
                outputNode.rootObject.transform.position = newPos;
                lastMoved.rootObject.transform.position += (newPos - oldPos);
            }
        }

        // Set input / output nodes
        inputNodes[inputIndex] = outputNode;
        outputNode.outputNodes[outputIndex] = this;
        /*if(draggingInput)
        {
            inputNodes[inputIndex] = stationaryTarget;
            stationaryTarget.outputNodes[outputIndex] = this;
        } else
        {
            stationaryTarget.inputNodes[inputIndex] = this;
            outputNodes[outputIndex] = stationaryTarget;
        }*/
    }

    public void BreakLock(NodeNub self, int idOfOther)
    {
        bool input = self.data.input;
        int index = self.nubIndex;
        if (input)
        {
            inputNodes[index] = null;
        }
        else
        {
            outputNodes[index] = null;
        }

        if (verticalSize > 1)
        {
            QNode[] nodesToCheck = input ? inputNodes : outputNodes;
            for(int i = 0; i < nodesToCheck.Length; i++)
            {
                QNode node = nodesToCheck[i];
                if(node.id == idOfOther)
                {
                    if (input)
                    {
                        inputNodes[i] = null;
                    }
                    else
                    {
                        outputNodes[i] = null;
                    }
                    self.BreakOnlyConnections();
                }
            }
        }
    }

    public GameObject generateVisual()
    {
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
                inputNubs[i] = inNub.GetComponent<NodeNub>();
                inputNubs[i].Init(this, inputType, i);
                inNub.transform.SetParent(root.transform);
                inNub.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(inNub);
            }

            for (int i = 0; i < outputTypes.Length; i++)
            {
                QNodeType outputType = outputTypes[i];
                GameObject outNub = NodeManager.instance.NubFromNodeType(outputType);
                outputNubs[i] = outNub.GetComponent<NodeNub>();
                outputNubs[i].Init(this, outputType, i);
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
                inputNubs[0] = inNub.GetComponent<NodeNub>();
                inputNubs[0].Init(this, inputTypes[0], 0);
                inNub.transform.SetParent(root.transform);
                inNub.transform.localPosition = new Vector3(epsilon, 0, 0);
                setSortingOrder(inNub);
            }

            if (outputTypes.Length > 0)
            {
                GameObject outNub = NodeManager.instance.NubFromNodeType(outputTypes[0]);
                outputNubs[0] = outNub.GetComponent<NodeNub>();
                outputNubs[0].Init(this, outputTypes[0], 0);
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
                Debug.Log("From " + fromLast.id + ": Look at " + qNode.id);
                if (!qNode.isDragging)
                {
                    lockedRoots.Add(qNode.rootObject);
                    lockedIDs.Add(qNode.id);
                    qNode.isDragging = true;
                    qNode.offsetToMainDrag = qNode.rootObject.transform.position - trueStart.rootObject.transform.position;
                    findLockedNodes(qNode, trueStart);
                }
            }
        }
        foreach (QNode qNode in fromLast.outputNodes)
        {
            if (qNode != null)
            {
                Debug.Log("From " + fromLast.id + ": Look at " + qNode.id);
                if (!qNode.isDragging)
                {
                    lockedRoots.Add(qNode.rootObject);
                    lockedIDs.Add(qNode.id);
                    qNode.isDragging = true;
                    qNode.offsetToMainDrag = qNode.rootObject.transform.position - trueStart.rootObject.transform.position;
                    findLockedNodes(qNode, trueStart);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        lockedRoots = new HashSet<GameObject>();
        lockedIDs = new HashSet<int>();
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
        Debug.Log("This is ID " + id);
        lastMoved = this;
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -501);
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePosition);
        screenPos.z = 0;
        mouseDragPivot = screenPos - transform.position;

        // calculate lockedRoots
        lockedRoots.Clear();
        lockedIDs.Clear();
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