using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ResetButton : MonoBehaviour
{
    [SerializeField] private float threshold = 0.1f;
    [SerializeField] private float deadZone = 0.025f;

    private bool isPressed;
    private Vector3 startPos;
    private ConfigurableJoint join;

    public UnityEvent onPressed, onReleased;

    private Vector3[] objectsPos = new Vector3[5];
    private Quaternion[] objectsRotation = new Quaternion[5];
    private GameObject[] objects = new GameObject[5];
    private GameObject[] sprites = new GameObject[5];
    private GameObject mrMan;
    private Vector3 mrManPos;

    CharacterMovement characterMovement;
    GestureRecognizer gestures;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i <= 4; i++)
        {
            objects[i] = GameObject.Find("object" + i);
            objectsPos[i] = objects[i].transform.position;
            objectsRotation[i] = objects[i].transform.rotation;

            sprites[i] = GameObject.Find("Sprite" + i);
            sprites[i].SetActive(false);
        }


        sprites[(int)Random.Range(0, 4.99f)].SetActive(true);
        startPos = this.transform.localPosition;
        join = GetComponent<ConfigurableJoint>();
        mrMan = GameObject.Find("Male 1");
        mrManPos = mrMan.transform.position;

        characterMovement = GameObject.Find("Male 1").GetComponent<CharacterMovement>();
        gestures = GameObject.Find("GestureDetector").GetComponent<GestureRecognizer>();
    }

    // Update is called once per frame
    void Update()
    {
        if(!isPressed && GetValue() + threshold >= 1)
        {
            Pressed();
        }
        if(isPressed && GetValue() - threshold <= 0)
        {
            Released();
        }
    }

    private float GetValue()
    {
        float value = Vector3.Distance(startPos, this.transform.localPosition) / join.linearLimit.limit;

        if(Mathf.Abs(value) < deadZone)
        {
            value = 0;
        }

        return Mathf.Clamp(value, -1f, 1f);
    }

    private void Pressed()
    {
        isPressed = true;
        for (int i = 0; i <= 4; i++)
        {
            objects[i].transform.position = objectsPos[i];
            objects[i].transform.rotation = objectsRotation[i];

            sprites[i].SetActive(false);
        }
        sprites[(int)Random.Range(0, 5)].SetActive(true);

        onPressed.Invoke();
        GameObject.Find("Logging").GetComponent<GestureLog>().SaveLog();
        mrMan.transform.position = mrManPos;
        gestures.movableObject = null;
        characterMovement.targNum = 5;
        characterMovement.walking = true;
        characterMovement.moveDirection(Vector3.zero, "idle");
    }

    private void Released()
    {
        isPressed = false;
        onReleased.Invoke();
    }
}
