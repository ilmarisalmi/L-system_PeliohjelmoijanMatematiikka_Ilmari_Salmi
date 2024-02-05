//Tree generator using L-System. Ilmari Salmi
//https://www.youtube.com/watch?v=J0LyZSgVKVc Making Mathematical Art with L-Systems
//https://youtu.be/tUbTGWl-qus L-Systems Unity Tutorial [2018.1]
//https://youtu.be/p319XzQTYmQ Unity3d Procedural Generation - L-System Procedurally Generated Trees And Editor Tools
//https://youtu.be/feNVBEPXAcE Procedural Plant Generation with L-Systems

using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class LSystemTrue : MonoBehaviour
{
    public int iterations = 2; // n of iterations
    public float angle = 20f; // angle of rotation
    public float length = 2f; // length of branch
    public float width = 0.8f; // width of branch
    public float widthFactor = 0.9f; // width factor
    public string axiom = "F"; // axiom

    // possible rules
    public string[] possibleRules = {
        "FF+[+F-F-F]-[-F+F+F]",
        "F+F-F-F+F",
        "FF+[+F-F]-[-F+F]",
        "F+F+[F-F]-[F+F]",
        "F-F+[F+F]-[F-F]"
    };


    public Material branchMaterial;
    public Material leafMaterial;

    private const char FORWARD = 'F'; // forward
    private const char NOTHING = 'f'; // nothing
    private const char PLUS = '+'; // plus
    private const char MINUS = '-'; // minus
    private const char BRANCH_LEFT = '['; // branch left
    private const char BRANCH_RIGHT = ']'; // branch right

    private Stack<State> stateStack;
    // state of the system that controls the drawing
    private struct State
    {
        public Vector3 position;
        public Vector3 direction;
        public float width;
        public GameObject branchObject;
    }

    private List<LineRenderer> lineRenderers;
    public Button randomizeButton;
    private GameObject currentParent;

    // counter for branch names
    private int branchCounter = 0;

    void Start()
    {
        lineRenderers = new List<LineRenderer>();
        randomizeButton.onClick.AddListener(GenerateRandomTree);

        GenerateAndDrawTree();

        FindBranchesWithoutChildren();
    }

    // generate random tree
    void GenerateRandomTree()
    {  
        // destroy old tree. dont destroy the gameobject itself, just its children. 
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }
        
        lineRenderers.Clear();
        branchCounter = 0; // reset branch counter


        // randomize NEW parameters
        iterations = Random.Range(2, 4);
        angle = Random.Range(10f, 40f);
        length = Random.Range(1f, 2f);


        // generate NEW tree
        GenerateAndDrawTree();
        FindBranchesWithoutChildren();
    }

    // generate L-System axiom in string format. for example "F -> F+F-F-F+F"
    string GenerateLSystem(string rule)
    {
        string result = axiom;

        // iterate L-System
        for (int i = 0; i < iterations; i++)
        {
            // replace each character with corresponding rule
            string temp = "";
            foreach (char c in result)
            {
                // if character is forward, replace it with the specified rule
                if (c == FORWARD)
                {
                    temp += rule;
                }
                // if character is not forward, just add it to the string
                else
                {
                    temp += c;
                }
            }
            result = temp;
        }
        Debug.LogWarning("TRUE L-System: " + result);
        return result;
    }

    // draw L-System
    void DrawLSystem(string lsystem)
    {
        State state = new State();

        state.position = new Vector3(0f, 0f, 0f);
        state.position = transform.position;

        state.direction = Vector3.up;
        state.width = width;
        stateStack = new Stack<State>();

        //foreach loop goes through each character in the L-System
        foreach (char c in lsystem)
        {
            //switch statement checks what character is in the L-System
            switch (c)
            {
                //FORWARD
                case FORWARD:
                    GameObject branchObject = new GameObject("Branch" + branchCounter);
                    branchCounter++;
                    //set parent of the branch object
                    if (currentParent != null)
                    {
                        branchObject.transform.parent = currentParent.transform;
                    }
                    else
                    {
                        branchObject.transform.parent = transform;
                    }

                    currentParent = branchObject;
                    state.branchObject = branchObject;

                    //LineRenderer
                    LineRenderer lineRenderer = branchObject.AddComponent<LineRenderer>();
                    lineRenderer.material = branchMaterial;
                    lineRenderer.startWidth = state.width;
                    lineRenderer.endWidth = state.width * widthFactor;

                    //set positions of the line renderer
                    lineRenderer.SetPosition(0, state.position);
                    state.position += state.direction * length;
                    lineRenderer.SetPosition(1, state.position);
                    lineRenderers.Add(lineRenderer);
                    state.width *= widthFactor;
                    break;

                //NOTHING
                case NOTHING:
                    //move forward without drawing
                    state.position += state.direction * length;
                    break;

                //PLUS
                case PLUS:
                    //rotate direction vector by angle
                    state.direction = Quaternion.AngleAxis(angle, Vector3.forward) * state.direction;
                    break;

                //MINUS
                case MINUS:
                    //rotate direction vector by -angle
                    state.direction = Quaternion.AngleAxis(-angle, Vector3.forward) * state.direction;
                    break;

                //LEFT
                case BRANCH_LEFT:
                    //push current state to the stack
                    stateStack.Push(state);
                    break;

                //RIGHT
                case BRANCH_RIGHT:
                    state = stateStack.Pop();
                    //set current parent to the parent of the state that was popped
                    if (stateStack.Count > 0)
                    {
                        currentParent = stateStack.Peek().branchObject;
                    }
                    //if there are no more states in the stack, set current parent to null
                    else
                    {
                        currentParent = null;
                    }
                    break;
            }
        }
    }

    // generate and draw tree using a random rule
    void GenerateAndDrawTree()
    {
        string randomRule = possibleRules[Random.Range(0, possibleRules.Length)];
        string lsystem = GenerateLSystem(randomRule);
        DrawLSystem(lsystem);
    }

    //find branches without children
    void FindBranchesWithoutChildren()
    {
        List<GameObject> branchesWithoutChildren = new List<GameObject>();
        foreach (var renderer in lineRenderers)
        {
            if (renderer.transform.childCount == 0)
            {
                //change line renderer material to leaf material
                renderer.material = leafMaterial;
            }
        }
        foreach (var branch in branchesWithoutChildren)
        {
            Debug.Log("Branch without children: " + branch.name + " and its position is: " + branch.transform.position);

        }
    }
}
