using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoarderCollision : MonoBehaviour
{
    CharacterMovement movementScript;
    GestureRecognizer gestureRecognizer;





    // Start is called before the first frame update
    void Start()
    {
        movementScript = GameObject.Find("Male 1").GetComponent<CharacterMovement>();
        gestureRecognizer = GameObject.Find("GestureDetector").GetComponent<GestureRecognizer>();

    }



    private void OnTriggerEnter(Collider other)
    {
        /*if (collision.gameObject.name == "log_front" || collision.gameObject.name == "log_back" || collision.gameObject.name == "log_left" || collision.gameObject.name == "log_right" || collision.gameObject.tag == "MovableObject")
        {
            movementScript.moveDirection("stop");
            if()
        }*/
        //&& this.gameObject.name == "Male 1"

        if (!GameObject.Find("Male 1").GetComponent<Animator>().GetBool("isWalking"))
        {
            if (other.gameObject.name == "log_front")
            {
                gestureRecognizer.backward = false;
                movementScript.moveDirection();
            }

            if (other.gameObject.name == "log_left" || (this.gameObject.tag == "MovableObject" && other.gameObject.name == "leftCol"))// || (other.gameObject.tag == "MovableObject" ))
            {
                gestureRecognizer.lef = false;
                movementScript.moveDirection();
            }

            if (other.gameObject.name == "log_back" || (this.gameObject.tag == "MovableObject" && other.gameObject.name == "backCol"))// || (other.gameObject.tag == "MovableObject" ))
            {
                gestureRecognizer.forward = false;
                movementScript.moveDirection();
            }

            if (other.gameObject.name == "log_right" || (this.gameObject.tag == "MovableObject" && other.gameObject.name == "rightCol"))// || (other.gameObject.tag == "MovableObject"))
            {
                gestureRecognizer.righ = false;
                movementScript.moveDirection();
            }
        }
        /*if (other.gameObject.tag == "MovableObject")
        {
            gestureRecognizer.coll = true;
            movementScript.moveDirection();
        }*/
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.name == "log_front")// || other.gameObject.tag == "MovableObject")
        {
            gestureRecognizer.backward = true;
        }

        if (other.gameObject.name == "log_left" || (this.gameObject.tag == "MovableObject" && other.gameObject.name == "leftCol"))// || other.gameObject.tag == "MovableObject")
        {
            gestureRecognizer.lef = true;
        }

        if (other.gameObject.name == "log_back" || (this.gameObject.tag == "MovableObject" && other.gameObject.name == "backCol"))// || other.gameObject.tag == "MovableObject")
        {
            gestureRecognizer.forward = true;
        }

        if (other.gameObject.name == "log_right" || (this.gameObject.tag == "MovableObject" && other.gameObject.name == "rightCol"))// || other.gameObject.tag == "MovableObject")
        {
            gestureRecognizer.righ = true;
        }

        /*if(other.gameObject.tag == "MovableObject")
        {
            gestureRecognizer.coll = false;
        }*/
    }
}
