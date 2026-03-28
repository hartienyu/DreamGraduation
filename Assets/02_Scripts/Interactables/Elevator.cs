using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************
//创建人： Trigger 
//功能说明：电梯控制逻辑
//*****************************************
public class Elevator : MonoBehaviour
{
    public Transform[] floors;
    public float stopInterval = 10;
    public float moveTime = 5;
    private int currentFloorIndex = 0;
    private bool isMoving = true;
    private float moveStartTime;
    private float stopTime;
    public Transform[] doors;
    public float doorMoveSpeed = 5;
    private float doorMoveTimer;
    private bool doorIsMoving;
    private bool doorIsOpen;
    private int currentDoorIndex = 1;

    private PlayerMovement playerMovement;

    void Start()
    {
        moveStartTime = Time.time;
    }

    void Update()
    {
        if (isMoving)
        {
            float timeVal = Time.time - moveStartTime;
            if (timeVal < moveTime)
            {
                int nextFloorIndex = (currentFloorIndex + 1) % floors.Length;
                Vector3 startPosition = floors[currentFloorIndex].position;
                Vector3 targetPositon = floors[nextFloorIndex].position;
                float t = timeVal / moveTime;
                transform.position = Vector3.Lerp(startPosition, targetPositon, t);
            }
            else
            {
                isMoving = false;
                stopTime = Time.time;
                doorIsMoving = true;
                doorIsOpen = true;
                doorMoveTimer = Time.time;
                currentDoorIndex = (currentFloorIndex + 1) % floors.Length;

                if (playerMovement)
                {
                    playerMovement.canMove = true;
                    playerMovement.transform.SetParent(null);
                }
            }
        }
        else
        {
            MoveElevator();
        }

        if (doorIsMoving)
        {
            if (doorIsOpen)
            {
                OpenOrCloseDoor(true);
            }
            else
            {
                OpenOrCloseDoor(false);
            }
        }
    }

    private void MoveElevator()
    {
        if (!isMoving)
        {
            if (Time.time - stopTime >= stopInterval)
            {
                doorIsMoving = true;
                doorIsOpen = false;
                doorMoveTimer = Time.time;
                isMoving = true;
                moveStartTime = Time.time;
                currentFloorIndex = (currentFloorIndex + 1) % floors.Length;

                if (playerMovement)
                {
                    playerMovement.canMove = false;
                    playerMovement.transform.SetParent(transform);
                }
            }
        }
    }

    private void OpenOrCloseDoor(bool isOpen)
    {
        if (isOpen)
        {
            if (Time.time - doorMoveTimer < 1)
            {
                Transform doorTrans = doors[currentDoorIndex];
                doorTrans.position -= doorTrans.right * doorMoveSpeed * Time.deltaTime;
            }
        }
        else
        {
            if (Time.time - doorMoveTimer < 1)
            {
                Transform doorTrans = doors[currentDoorIndex];
                doorTrans.position += doorTrans.right * doorMoveSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = other.GetComponent<PlayerMovement>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerMovement = null;
        }
    }
}