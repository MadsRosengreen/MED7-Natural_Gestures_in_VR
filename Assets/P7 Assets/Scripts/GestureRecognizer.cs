using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using System.IO;
using System.Linq;



[System.Serializable]
public struct StaticGesture
{
    public string name;
    public List<Vector3> fingerData;
}

// Contains the struct for the dynamic gesture list
//[System.Serializable]
//public struct DynamicGesture
//{
//    public string name;
//    public List<List<StaticGesture>> dynamicGestures;
//}


public class GestureRecognizer : MonoBehaviour
{
    public OVRSkeleton recSkeleton, rightHandSkeleton, leftHandSkeleton;
    public List<StaticGesture> gestures;
    private List<OVRBone> fingerBonesRec, fingerBonesRightH, fingerBonesLeftH;

    // partialGestures is used to temporarily store the handposes of a dynamic gesture ->
    // before going to dynamicGestures.
    //public List<StaticGesture> partialGestures;
    //public List<List<StaticGesture>> dynamicGestures = new List<List<StaticGesture>>();

    public float threshold = 0.1f;

    public bool debugMode = true;
    public bool hasStarted = false;
    public bool enablePointingLine = false;

    public bool backward = true;
    public bool forward = true;
    public bool lef = true;
    public bool righ = true;

    public float fastSpeed = 0.2f; // 0 - 1
    public float speed = 2f; // 0 - 3

    StaticGesture currentGesture;
    GestureLog gestureLog;

    private float loggingInterval = 0.5f; // In seconds
    private float loggingTimer = 0.0f;
    private bool loggingReady = true;   // Only log when this is true


    //public float recordInterval = 0.3f; // Used for dynamic recording of gestures

    public GameObject movableObject = null;
    private GameObject tempMovableObject = null;
    private GameObject character;

    RaycastHit hit;
    [SerializeField] LineRenderer lineRendererLeft, lineRendererRight;

    // Meant to store the string we print out as a .csv
    //private List<string> gestureLogs; 

    // Variables for TargetSelection()
    CharacterMovement targetScript;
    public float selectionTime = 1f;
    float currentSelectionTime = 0f;

    // Variables for ManipulateSelection()
    Vector3 oldPalmPos_right = Vector3.zero;
    Vector3 boxVelocity = Vector3.zero;
    public float goThreshold = 2;
    public float goSlowThreshold = .1f;
    public float maxSpeed = 0.5f;




    void Start()
    {
        character = GameObject.Find("Male 1");
        targetScript = character.GetComponent<CharacterMovement>();
        gestureLog = GameObject.Find("Logging").GetComponent<GestureLog>();
        StartCoroutine(DelayInitialize(InitializeSkeleton));
    }

    public IEnumerator DelayInitialize(Action actionToDo)
    {
        if (!debugMode)
        {
            while (!leftHandSkeleton.IsInitialized && !rightHandSkeleton.IsInitialized)
            {
                yield return null;
            }
            actionToDo.Invoke();
        }
        else if (debugMode)
        {
            while (!recSkeleton.IsInitialized)
            {
                yield return null;
            }
            actionToDo.Invoke();
        }

    }

    public void InitializeSkeleton()
    {
        // Populate the private list of fingerbones from the current hand we put in the skeleton
        if (!debugMode)
        {
            fingerBonesRightH = new List<OVRBone>(rightHandSkeleton.Bones);
            fingerBonesLeftH = new List<OVRBone>(leftHandSkeleton.Bones);
            hasStarted = true;
            Debug.Log("Left and right hand should be initialized");
        }
        else
        {
            fingerBonesRec = new List<OVRBone>(recSkeleton.Bones);
            hasStarted = true;
            Debug.Log("Rechand is initialized");
        }
    }


    void Update()
    {
        loggingTimer += Time.deltaTime;
        if (loggingTimer >= loggingInterval)
        {
            loggingReady = true;
            loggingTimer = 0;
        }
        // Disable these if you want to record new gestures
        if (RecognizedLeft().name == null && RecognizedRight().name != null)
        {
            currentGesture = RecognizedRight();
        }
        else if (RecognizedRight().name == null && RecognizedLeft().name != null)
        {
            currentGesture = RecognizedLeft();
        }
        else
        {
            currentGesture = RecognizedRight();
        }

        //Debug.Log(currentGesture.name);

        if (currentGesture.name == "R_Pointing1")
        {
            PointingSelect(fingerBonesRightH);
            lineRendererLeft.enabled = false;
            if (loggingReady)
            {
                gestureLog.Log("Point", currentGesture.name, hit.transform.name);
                loggingReady = false;
            }
        }
        else if (currentGesture.name == "L_Pointing1" || currentGesture.name == "L_Pointing2")
        {
            PointingSelect(fingerBonesLeftH);
            lineRendererRight.enabled = false;
            if (loggingReady)
            {
                gestureLog.Log("Point", currentGesture.name, hit.transform.name);
                loggingReady = false;
            }
        }
        else
        {
            lineRendererLeft.enabled = false;
            lineRendererRight.enabled = false;
        }

        if (currentGesture.name == "R_ThumbsUp" && movableObject != null)
        {
            DeselectObject();
            if (loggingReady)
            {
                gestureLog.Log("Ok", currentGesture.name, movableObject.name);
                loggingReady = false;
            }
        }
        if (currentGesture.name == "L_ThumbsUp" && movableObject != null)
        {
            DeselectObject();
            if (loggingReady)
            {
                gestureLog.Log("Ok", currentGesture.name, movableObject.name);
                loggingReady = false;
            }
        }

        ManipulateSelection();

        if (debugMode && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("I tried to record...");
            RecordGesture();
        }

    }

    StaticGesture RecognizedRight()
    {
        StaticGesture currentGesture = new StaticGesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures.GetRange(0, 3))
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBonesRightH.Count; i++)
            {
                Vector3 currentData = rightHandSkeleton.transform.InverseTransformPoint(fingerBonesRightH[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerData[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }
                sumDistance += distance;
            }

            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }

        }
        return currentGesture;
    }

    StaticGesture RecognizedLeft()
    {
        StaticGesture currentGesture = new StaticGesture();
        float currentMin = Mathf.Infinity;

        foreach (var gesture in gestures.GetRange(3, 4))
        {
            float sumDistance = 0;
            bool isDiscarded = false;
            for (int i = 0; i < fingerBonesLeftH.Count; i++)
            {
                Vector3 currentData = leftHandSkeleton.transform.InverseTransformPoint(fingerBonesLeftH[i].Transform.position);
                float distance = Vector3.Distance(currentData, gesture.fingerData[i]);
                if (distance > threshold)
                {
                    isDiscarded = true;
                    break;
                }

                sumDistance += distance;
            }

            if (!isDiscarded && sumDistance < currentMin)
            {
                currentMin = sumDistance;
                currentGesture = gesture;
            }
        }
        return currentGesture;
    }



    void PointingSelect(List<OVRBone> fingerBones)
    {
        if (currentGesture.name == "R_Pointing1")
        {
            if (Physics.Raycast(fingerBones[8].Transform.position, fingerBones[8].Transform.right, out hit))
            {
                if (fingerBones == fingerBonesRightH && enablePointingLine)
                {
                    lineRendererRight.enabled = enablePointingLine;
                    lineRendererRight.SetPosition(0, fingerBones[8].Transform.position);
                    lineRendererRight.SetPosition(1, hit.point);
                }
                else if (fingerBones == fingerBonesLeftH && enablePointingLine)
                {
                    lineRendererLeft.enabled = enablePointingLine;
                    lineRendererLeft.SetPosition(0, fingerBones[8].Transform.position);
                    lineRendererLeft.SetPosition(1, hit.point);
                }

                if (hit.transform.tag == "MovableObject" && movableObject == null || movableObject.name != hit.transform.name && hit.transform.tag == "MovableObject")
                {
                    tempMovableObject = hit.transform.gameObject;
                    if (hit.transform.name == tempMovableObject.name)
                    {
                        currentSelectionTime += Time.deltaTime;
                        if (currentSelectionTime >= selectionTime)
                        {
                            movableObject = tempMovableObject;
                            lef = true;
                            righ = true;
                            backward = true;
                            forward = true;
                            targetScript.setTargetPos(movableObject);
                            character.GetComponent<CapsuleCollider>().enabled = true;
                            currentSelectionTime = 0;
                        }
                    }
                }

            }
        }
        else if (currentGesture.name == "L_Pointing1" || currentGesture.name == "L_Pointing2")
        {
            if (Physics.Raycast(fingerBones[8].Transform.position, -fingerBones[8].Transform.right, out hit))
            {
                if (fingerBones == fingerBonesRightH && enablePointingLine)
                {
                    lineRendererRight.enabled = enablePointingLine;
                    lineRendererRight.SetPosition(0, fingerBones[8].Transform.position);
                    lineRendererRight.SetPosition(1, hit.point);
                }
                else if (fingerBones == fingerBonesLeftH && enablePointingLine)
                {
                    lineRendererLeft.enabled = enablePointingLine;
                    lineRendererLeft.SetPosition(0, fingerBones[8].Transform.position);
                    lineRendererLeft.SetPosition(1, hit.point);
                }

                if (hit.transform.tag == "MovableObject" && movableObject == null || movableObject.name != hit.transform.name && hit.transform.tag == "MovableObject")
                {
                    tempMovableObject = hit.transform.gameObject;
                    if (hit.transform.name == tempMovableObject.name)
                    {
                        currentSelectionTime += Time.deltaTime;
                        if (currentSelectionTime >= selectionTime)
                        {
                            movableObject = tempMovableObject;
                            lef = true;
                            righ = true;
                            backward = true;
                            forward = true;
                            targetScript.setTargetPos(movableObject);
                            character.GetComponent<CapsuleCollider>().enabled = true;
                            currentSelectionTime = 0;
                        }
                    }
                }

            }
        }
    }


    void ManipulateSelection()
    {
        //Right Hand
        if (!targetScript.GetComponent<Animator>().GetBool("isWalking") && currentGesture.name == "R_Stop" && movableObject != null)
        {
            Vector3 palmDirection = -fingerBonesRightH[9].Transform.up;

            // For debugging the hand palmDirection
            //lineRendererLeft.enabled = true;
            //lineRendererLeft.SetPosition(0, fingerBonesRightH[9].Transform.position);
            //lineRendererLeft.SetPosition(1, fingerBonesRightH[9].Transform.position + palmDirection);

            Vector3 deltaPosition = fingerBonesRightH[9].Transform.position - oldPalmPos_right;
            oldPalmPos_right = fingerBonesRightH[9].Transform.position;

            Vector3 movementAlongPalmDirection = Vector3.Project(deltaPosition, palmDirection);

            Vector3 rightHandVelocity = movementAlongPalmDirection / Time.deltaTime;

            Vector3 velocity = rightHandVelocity * Vector3.Dot(palmDirection.normalized, rightHandVelocity.normalized);

            float handSpeed = Vector3.Magnitude(velocity);

            float handDirection = Vector3.Dot(palmDirection.normalized, rightHandVelocity.normalized);

            velocity.y = 0;

            Vector3 snappedVelocity = SnapDirection(velocity);


            //movableObject.transform.position += boxVelocity * Time.deltaTime; //hvis boxVelocity er mindre end 0.01 (?) make mrMan stop

            /*if (boxVelocity.x != 0 || boxVelocity.z != 0)
            {
                character.transform.position += boxVelocity * Time.deltaTime;
                targetScript.moveDirection(boxVelocity);
            }*/


            if (snappedVelocity.x > 0 && lef == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Left", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else if (snappedVelocity.x < 0 && righ == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Right", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else if (snappedVelocity.z < 0 && forward == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Forward", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else if (snappedVelocity.z > 0 && backward == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Back", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else
            {
                targetScript.moveDirection();
            }

            if (handDirection < 0)
            {
                return;
            }

            if (handSpeed > goThreshold)
            {
                boxVelocity = Vector3.ClampMagnitude(snappedVelocity.normalized * speed, maxSpeed) * 10; //fart when u go yeet
            }



            if (handSpeed > goSlowThreshold && handSpeed < goThreshold)
            {
                boxVelocity = Vector3.zero;
                /*movableObject.transform.position += snappedVelocity.normalized * 3 * Time.deltaTime; //fart when u go slo
                character.transform.position += snappedVelocity.normalized * 3 * Time.deltaTime;
                targetScript.moveDirection(snappedVelocity.normalized);*/

                if (snappedVelocity.x > 0 && lef == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Left", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else if (snappedVelocity.x < 0 && righ == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Right", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else if (snappedVelocity.z < 0 && forward == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Forward", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else if (snappedVelocity.z > 0 && backward == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Back", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else
                {
                    targetScript.moveDirection();
                }
            }
        }
        //Left Hand
        else if (!targetScript.GetComponent<Animator>().GetBool("isWalking") && currentGesture.name == "L_Stop" && movableObject != null)
        {
            Vector3 palmDirection = fingerBonesLeftH[9].Transform.up;

            // For debugging the hand palmDirection
            //lineRendererLeft.enabled = true;
            //lineRendererLeft.SetPosition(0, fingerBonesRightH[9].Transform.position);
            //lineRendererLeft.SetPosition(1, fingerBonesRightH[9].Transform.position + palmDirection);

            Vector3 deltaPosition = fingerBonesLeftH[9].Transform.position - oldPalmPos_right;
            oldPalmPos_right = fingerBonesLeftH[9].Transform.position;

            Vector3 movementAlongPalmDirection = Vector3.Project(deltaPosition, palmDirection);

            Vector3 leftHandVelocity = movementAlongPalmDirection / Time.deltaTime;

            Vector3 velocity = leftHandVelocity * Vector3.Dot(palmDirection.normalized, leftHandVelocity.normalized);

            float handSpeed = Vector3.Magnitude(velocity);

            float handDirection = Vector3.Dot(palmDirection.normalized, leftHandVelocity.normalized);

            velocity.y = 0;

            Vector3 snappedVelocity = SnapDirection(velocity);

            //movableObject.transform.position += boxVelocity * Time.deltaTime; //hvis boxVelocity er mindre end 0.01 (?) make mrMan stop

            /*if (boxVelocity.x != 0 || boxVelocity.z != 0)
            {
                character.transform.position += boxVelocity * Time.deltaTime;
                targetScript.moveDirection(boxVelocity);
            }*/



            if (snappedVelocity.x > 0 && lef == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Left", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else if (snappedVelocity.x < 0 && righ == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Right", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else if (snappedVelocity.z < 0 && forward == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Forward", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else if (snappedVelocity.z > 0 && backward == true)
            {
                if (loggingReady)
                {
                    gestureLog.Log("Back", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                    loggingReady = false;
                }
                character.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
                targetScript.moveDirection(boxVelocity);
                movableObject.transform.position += boxVelocity * Time.deltaTime * fastSpeed;
            }
            else
            {
                targetScript.moveDirection();
            }

            if (handDirection < 0)
            {
                return;
            }

            if (handSpeed > goThreshold)
            {
                boxVelocity = Vector3.ClampMagnitude(snappedVelocity.normalized * speed, maxSpeed) * 10; //fart when u go yeet
            }

            if (handSpeed > goSlowThreshold && handSpeed < goThreshold)
            {
                boxVelocity = Vector3.zero;
                /*movableObject.transform.position += snappedVelocity.normalized * 3 * Time.deltaTime; //fart when u go slo
                character.transform.position += snappedVelocity.normalized * 3 * Time.deltaTime;
                targetScript.moveDirection(snappedVelocity.normalized);*/

                if (snappedVelocity.x > 0 && lef == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Left", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else if (snappedVelocity.x < 0 && righ == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Right", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else if (snappedVelocity.z < 0 && forward == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Forward", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else if (snappedVelocity.z > 0 && backward == true)
                {
                    if (loggingReady)
                    {
                        gestureLog.Log("Back", currentGesture.name, movableObject.name, boxVelocity.ToString("F3"), snappedVelocity.ToString("F3"));
                        loggingReady = false;
                    }
                    movableObject.transform.position += snappedVelocity.normalized * speed * Time.deltaTime; //fart when u go slo
                    character.transform.position += snappedVelocity.normalized * speed * Time.deltaTime;
                    targetScript.moveDirection(snappedVelocity.normalized);
                }
                else
                {
                    targetScript.moveDirection();
                }
            }
        }
    }

    void DeselectObject()
    {
        movableObject = null;
        character.GetComponent<CapsuleCollider>().enabled = false;
        targetScript.moveDirection(Vector3.zero, "idle");
    }

    Vector3 SnapDirection(Vector3 direction)
    {
        Vector3 forward = new Vector3(0, 0, -1);
        Vector3 backward = new Vector3(0, 0, 1);
        Vector3 right = new Vector3(-1, 0, 0);
        Vector3 left = new Vector3(1, 0, 0);

        Vector3[] axes = { forward, backward, right, left };

        Vector3 result = axes[0];

        for (int i = 1; i < axes.Length; i++)
        {
            float currentBest = Vector3.Angle(direction, result);
            float current = Vector3.Angle(direction, axes[i]);
            if (current < currentBest)
            {
                result = axes[i];
            }
        }

        return result;
    }


    // This function records one gesture when called
    void RecordGesture()
    {
        // Creates new static gesture called g
        StaticGesture g = new StaticGesture();

        // Sets the name of the new gesture
        g.name = "New Gesture";

        // Creates a list of 3D vectors we use to save the individual bones' positions
        List<Vector3> fingerData = new List<Vector3>();

        foreach (var bone in fingerBonesRec)
        {
            // Adds each bone's position from the InverseTransformPoint(wrist) to the Vector3 list we just created
            fingerData.Add(recSkeleton.transform.InverseTransformPoint(bone.Transform.position));

        }

        // Sets gesture g's finger data equal to the fingerdata we just set above
        g.fingerData = fingerData;

        // Adds the gesture g to the list of gestures we loop through in Recognized()
        gestures.Add(g);

    }


    // This function records a gesture every x seconds
    //IEnumerator RecordDynamicGesture()
    //{
    //    for (int i = 0; i < 10; i++)
    //    {
    //        StaticGesture staticGesture = new StaticGesture();

    //        staticGesture.name = "Partial Gesture" + i;

    //        List<Vector3> fingerData = new List<Vector3>();
    //        List<DynamicGesture> dynamicGesture = new List<DynamicGesture>();

    //        foreach (var bone in fingerBones)
    //        {
    //            fingerData.Add(skeleton.transform.InverseTransformPoint(bone.Transform.position));
    //        }
    //        staticGesture.fingerData = fingerData;
    //        partialGestures.Add(staticGesture);

    //        yield return new WaitForSeconds(recordInterval);
    //    }
    //}

    // PUT BELOW CODE IN UPDATE IF YOU WANT TO RECORD A DYNAMIC GESTURE ->
    // AND SAVE IT TO THE DYNAMIC GESTURE LIST.
    // BEWARE: THE GESTURES AREN'T SAVED ACROSS PLAY-SESSIONS
    //if (debugMode && Input.GetKeyDown(KeyCode.Space))
    //    {
    //        StartCoroutine(nameof(RecordDynamicGesture));
    //    }
    //    else if (debugMode && Input.GetKeyUp(KeyCode.Space))
    //    {
    //        StopCoroutine(nameof(RecordDynamicGesture));

    //        dynamicGestures.Add(partialGestures);
    //        //partialGestures.Clear();      // Clears the partial gesture list to get ready for the next one

    //        // Just used to check if we could print all fingerdata.x's
    //        //for (int i = 0; i < 25; i++)
    //        //{
    //        //    Debug.Log(dynamicGestures[0][0].fingerData[i].x);
    //        //}
    //    }



    // This was created in an attempt to save the nested lists out ->
    // to a .csv file, but something is wrong with the function call ->
    // in the third for loop or the for loops themselves.

    //private void LoopGestures() // Change the parameter to fit
    //{
    //    for (int i = 0; i < dynamicGestures.Count; i++) // Use int i
    //    {
    //        for (int j = 0; j < dynamicGestures[i].Count; j++) // Use int j
    //        {
    //            for (int k = 0; k < 24; k++) // Use int k
    //            {
    //                GestureLog(
    //                        dynamicGestures[i][j].fingerData[k].x,
    //                        dynamicGestures[i][j].fingerData[k].y,
    //                        dynamicGestures[i][j].fingerData[k].z,
    //                        j,
    //                        i
    //                        );
    //                Debug.Log("Looping");
    //                // change list<list<gesture[k]>> to fit
    //            }
    //        }
    //    }
    //}

    // This is the function we attempted to call in the for loop above.
    // It was meant to save the each the coordinates of each bone ->
    // in each hand pose in each gesture as; gestureNumber(int), handPoseNumber(int), x, y, z.
    // Every handPose has 24 vectors, so this one would be repeated 24 times (different x, y, z) ->
    // before handPoseNumber increments.
    //public void GestureLog(float x = 0.0f, float y = 0.0f, float z = 0.0f, int innerList = 0, int outerList = 0)
    //{
    //    Debug.Log("LoopediLoop start");
    //    gestureLogs.Add(outerList
    //        + ";" + innerList
    //        + ";" + x
    //        + ";" + y
    //        + ";" + z);
    //    Debug.Log("LoopediLoop");
    //}
    //public void SaveGestureLog()
    //{
    //    Debug.Log("Saved Success");

    //    File.WriteAllLines("SavedGestures.csv", gestureLogs);
    //    Debug.Log("Saved Success");
    //}
}
