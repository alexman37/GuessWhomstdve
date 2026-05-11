using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QNode : MonoBehaviour
{
    [SerializeField] GameObject coreContent;
    [SerializeField] TextMeshProUGUI mainText;

    public int id;
    int verticalSize = 1;
    private string displayTxt;
    private QNodeType[] inputTypes;
    private QNodeType[] outputTypes;
    private QNode[] inputNodes;
    private QNode[] outputNodes;

    Vector2 gridPosition;
    bool connectionLocked;

    // Non-usable
    Vector3 mouseDragPivot;

    public void setTypeArrays(QNodeCreateOrder creator)
    {
        verticalSize = creator.verticalSize;
        inputTypes = creator.inTypes;
        outputTypes = creator.outTypes;
    }

    private void setSortingOrder(GameObject go)
    {
        go.GetComponent<SpriteRenderer>().sortingOrder = id * 2;
    }

    public GameObject generateVisual()
    {
        float nodeLength = 6;
        float nodeHeight = 1;
        float nodeSpacing = 0.23f;

        // If sprites use exact distances, they don't technically collide and so separation can be visible
        // We can move the sprites together by a teeny tiny amount to avoid this
        float epsilon = 0.00f;

        GameObject root = gameObject;
        setSortingOrder(root);

        // multi core
        if (verticalSize > 1)
        {
            mainText.GetComponent<RectTransform>().parent.GetComponent<Canvas>().sortingOrder = id * 2 + 1;
            for (int i = 1; i < verticalSize - 1; i++)
            {
                GameObject nextCore = GameObject.Instantiate(NodeManager.instance.fragment_multi_mid, root.transform);
                nextCore.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(nextCore);
            }

            GameObject lastCore = GameObject.Instantiate(NodeManager.instance.fragment_multi_bottom, root.transform);
            lastCore.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * (verticalSize - 1), 0);
            setSortingOrder(lastCore);

            for (int i = 0; i < inputTypes.Length; i++)
            {
                QNodeType inputType = inputTypes[i];
                GameObject inNub = NodeManager.instance.NubFromNodeType(inputType);
                inNub.transform.SetParent(root.transform);
                inNub.transform.localPosition = new Vector3(epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(inNub);
            }

            for (int i = 0; i < outputTypes.Length; i++)
            {
                QNodeType outputType = outputTypes[i];
                GameObject outNub = NodeManager.instance.NubFromNodeType(outputType);
                outNub.transform.SetParent(root.transform);
                outNub.transform.localPosition = new Vector3(nodeLength - 1 - epsilon, -1 * (nodeHeight + nodeSpacing - epsilon) * i, 0);
                setSortingOrder(outNub);
            }
        }

        // single core
        else
        {
            if (inputTypes.Length > 0)
            {
                GameObject inNub = NodeManager.instance.NubFromNodeType(inputTypes[0]);
                inNub.transform.SetParent(root.transform);
                inNub.transform.localPosition = new Vector3(epsilon, 0, 0);
                setSortingOrder(inNub);
            }

            if (outputTypes.Length > 0)
            {
                GameObject outNub = NodeManager.instance.NubFromNodeType(outputTypes[0]);
                outNub.transform.SetParent(root.transform);
                outNub.transform.localPosition = new Vector3(nodeLength - 1 - epsilon, 0, 0);
                setSortingOrder(outNub);
            }
        }

        return root;
    }

    // Start is called before the first frame update
    void Start()
    {
        //generateVisual();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // TODO clicking with other nodes
    }

    private void OnMouseDown()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -501);
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePosition);
        screenPos.z = 0;
        mouseDragPivot = screenPos - transform.position;
    }

    private void OnMouseDrag()
    {
        Vector3 mousePosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, -500);
        Vector3 screenPos = Camera.main.ScreenToWorldPoint(mousePosition);
        screenPos.z = 0;
        transform.position = screenPos - mouseDragPivot;
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