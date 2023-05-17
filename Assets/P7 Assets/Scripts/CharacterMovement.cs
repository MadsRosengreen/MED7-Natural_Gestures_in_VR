using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    Animator animator;
    public GameObject[] selectedObject = new GameObject[5];
    public GameObject[] targets = new GameObject[5];
    public int targNum;
    public bool walking = true;
    public float speed = 70.0f;
    float step;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();


        //selectedObject = GameObject.FindGameObjectsWithTag("MovableObject");

        /* if (selectedObject[3] != null)
         {
             Debug.Log("2222");

             for (int i = 0; i <= 4; i++)
             {
                 Debug.Log("asdaw");
                 selectedObject [i] = GameObject.Find("object" + i);
                 targets[i] = selectedObject[i].transform.GetChild(0).gameObject;

             }
         }*/

        for (int i = 0; i <= 4; i++)
        {
            selectedObject[i] = GameObject.Find("object" + i);
            targets[i] = selectedObject[i].transform.GetChild(0).gameObject;

        }

    }


    private void OnTriggerStay(Collider other)
    {
        if(other.tag == "Target" && other.name == selectedObject[targNum].transform.GetChild(0).name)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrabbed", true);
        }
        
    }
    private void OnTriggerExit(Collider other)
    {
       
            animator.SetBool("isWalking", true);
            animator.SetBool("isGrabbed", false);
       
    }


    // Update is called once per frame
    void Update()
    {
        step = speed * Time.deltaTime;
        //animator.GetBool("isWalking");
        animator.GetBool("isGrabbed");


        if (animator.GetBool("isWalking") && targNum != 5)
        {
            Vector3 targMovement = new Vector3(targets[targNum].transform.position.x, 0, targets[targNum].transform.position.z);
            this.transform.position = Vector3.MoveTowards(this.transform.position, targMovement, step);
            //Vector3 direction = new Vector3 (this.transform.position.x, targets[targNum].transform.position.y - this.transform.position.y, this.transform.position.z);
            Vector3 direction = new Vector3(targets[targNum].transform.position.x - this.transform.position.x, 0 ,targets[targNum].transform.position.z - this.transform.position.z);
            Vector3 newDir = Vector3.RotateTowards(this.transform.rotation.eulerAngles, direction, 10, 10);
            this.transform.rotation = Quaternion.LookRotation(newDir);
        }
        else if(!walking && targNum != 5)
        {
            Vector3 direction = new Vector3(selectedObject[targNum].transform.position.x - this.transform.position.x, 0, selectedObject[targNum].transform.position.z - this.transform.position.z);
            Vector3 newDir = Vector3.RotateTowards(this.transform.rotation.eulerAngles, direction, 10, 10);
            this.transform.rotation = Quaternion.LookRotation(newDir);
            /*Vector3 direction = new Vector3(this.transform.position.x, selectedObject[targNum].transform.position.y - this.transform.position.y, this.transform.position.z);
            Vector3 newDir = Vector3.RotateTowards(this.transform.position, direction, 10, 0);
            this.transform.rotation = Quaternion.LookRotation(newDir);*/
        }

        if (Input.GetKeyDown("r"))
        {
            setTargetPos(selectedObject[0]);
        }
        if (Input.GetKeyDown("e"))
        {
            setTargetPos(selectedObject[1]);
        }
        if(Input.GetKeyDown("t"))
        {
            setTargetPos(selectedObject[2]);
        }
    }



    public void setTargetPos (GameObject target)
    {
        string tempName = target.transform.GetChild(0).gameObject.name;

        switch (tempName)
        {
            case "targ0":
                targNum = 0;
                break;
            case "targ1":
                targNum = 1;
                break;
            case "targ2":
                targNum = 2;
                break;
            case "targ3":
                targNum = 3;
                break;
            case "targ4":
                targNum = 4;
                break;

        }
        walking = false;
        moveDirection(Vector3.zero, "walking");
    }

    public void moveDirection (Vector3 direction = default(Vector3), string gesture = "")
    {
        if(direction.x < 0) //højre
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrabbed", false);
            animator.SetBool("isWForwards", false);
            animator.SetBool("isWBackwards", false);
            animator.SetBool("isWLeft", true);
            animator.SetBool("isWRight", false);
        }

        else if(direction.x > 0) //venstre
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrabbed", false);
            animator.SetBool("isWForwards", false);
            animator.SetBool("isWBackwards", false);
            animator.SetBool("isWLeft", false);
            animator.SetBool("isWRight", true);
        }

        else if (direction.z < 0) //frem
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrabbed", false);
            animator.SetBool("isWForwards", false);
            animator.SetBool("isWBackwards", true);
            animator.SetBool("isWLeft", false);
            animator.SetBool("isWRight", false);
        }

        else if (direction.z > 0) //tilbage
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrabbed", false);
            animator.SetBool("isWForwards", true);
            animator.SetBool("isWBackwards", false);
            animator.SetBool("isWLeft", false);
            animator.SetBool("isWRight", false);
        }

        else if (direction == Vector3.zero && gesture == "") //idle(grabbed)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isGrabbed", true);
            animator.SetBool("isWForwards", false);
            animator.SetBool("isWBackwards", false);
            animator.SetBool("isWLeft", false);
            animator.SetBool("isWRight", false);
        }

        else if (gesture == "walking") //gå (ikke grabbed)
        {
            animator.SetBool("isWalking", true);
            animator.SetBool("isGrabbed", false);
            animator.SetBool("isWForwards", false);
            animator.SetBool("isWBackwards", false);
            animator.SetBool("isWLeft", false);
            animator.SetBool("isWRight", false);
        }

        else //stop (idle)
        {
            StartCoroutine(goIdle());
        }

        //walk to object
        
        
        
        /*switch (gesture)
        {
            case "forwards":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", true);
                animator.SetBool("isWBackwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                this.transform.position += new Vector3(x, 0, z);
                break;
            case "backwards":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isWBackwards", true);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                this.transform.position += new Vector3(x, 0, z);
                break;
            case "left":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isWBackwards", false);
                animator.SetBool("isWLeft", true);
                animator.SetBool("isWRight", false);
                this.transform.position += new Vector3(x, 0, z);
                break;
            case "right":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isWBackwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", true);
                this.transform.position += new Vector3(x, 0, z);
                break;
            case "stop":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", true);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isWBackwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                this.transform.position += new Vector3(x, 0, z);
                break;
            case "walk":
                animator.SetBool("isWalking", true);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isWBackwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                break;
            case "idle":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isWBackwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                break;
            /*case "rotateRight":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isBForwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                break;
            case "rotateLeft":
                animator.SetBool("isWalking", false);
                animator.SetBool("isGrabbed", false);
                animator.SetBool("isWForwards", false);
                animator.SetBool("isBForwards", false);
                animator.SetBool("isWLeft", false);
                animator.SetBool("isWRight", false);
                break;*/
     }

    IEnumerator goIdle()
    {
        animator.SetBool("isWalking", false);
        //animator.SetBool("isGrabbed", true);
        animator.SetBool("isWForwards", false);
        animator.SetBool("isWBackwards", false);
        animator.SetBool("isWLeft", false);
        animator.SetBool("isWRight", false);
        yield return new WaitForSeconds(0.1f);
        animator.SetBool("isGrabbed", false);
        yield return null;

    }
}

