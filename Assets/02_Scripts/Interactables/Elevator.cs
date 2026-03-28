//using NUnit;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//创建人： Trigger 
//功能说明：电梯
//***************************************** 
public class Elevator : MonoBehaviour
{
    public Transform[] floors;//电梯可以到达的楼层
    public float stopInterval=10;//电梯停止的时间
    public float moveTime = 5;//电梯移动到下一层所需要花费的整体时间
    private int currentFloorIndex =0;//当前所在楼层电梯的索引
    private bool isMoving = true;//是否正在移动
    private float moveStartTime;//开始移动的时间（用于时间检测的标准,上一次启动电梯的时间）
    private float stopTime;//上一次电梯停止时的时间节点
    public Transform[] doors;//三个楼层的电梯门
    public float doorMoveSpeed=5;//电梯门移动速度
    private float doorMoveTimer;//门移动计时器
    private bool doorIsMoving;//门是否正在移动
    private bool doorIsOpen;//当前门是准备开还是关
    private int currentDoorIndex=1;//当前需要关闭那层电梯门的索引
    private FirstPersonController fpc;//玩家引用
    void Start()
    {

    }

    void Update()
    {
        if (isMoving)//移动状态
        {
            float timeVal = Time.time - moveStartTime;//电梯启动后的时间总长
            if (timeVal< moveTime) //电梯正在移动
            {
                //电梯往哪移动（往哪一层移动）
                int nextFloorIndex = (currentFloorIndex + 1) % floors.Length;//计算要去哪一层的索引
                //起点，开始移动时所在楼层的位置
                Vector3 startPosition = floors[currentFloorIndex].position;
                //目标点，结束移动时所在楼层的位置
                Vector3 targetPositon = floors[nextFloorIndex].position;
                //差值比例
                float t = timeVal / moveTime;
                transform.position = Vector3.Lerp(startPosition,targetPositon,t);
            }
            else
            {
                //到达目标楼层,停留一段时间再次启动
                isMoving = false;
                //更新当前停止时的时间节点
                stopTime = Time.time;
                //电梯门打开
                doorIsMoving = true;
                doorIsOpen=true;
                //电梯开门时候的时间节点记录
                doorMoveTimer = Time.time;
                //当前要开那层的门的索引
                currentDoorIndex = (currentFloorIndex + 1) % floors.Length;
                if (fpc)
                {
                    fpc.lockMove = false;
                    fpc.transform.SetParent(null);
                }
            }
        }
        else
        {
            MoveElevator();
        }
        if (doorIsMoving)//电梯门移动逻辑
        {
            if (doorIsOpen)//在开
            {
                OpenOrCloseDoor(true);
            }
            else//在关
            {
                OpenOrCloseDoor(false);
            }
        }
    }
    /// <summary>
    /// 检测电梯状态，如果处于非移动状态，则开始移动
    /// </summary>
    private void MoveElevator()
    {
        if (!isMoving)//电梯处于非移动状态
        {
            if (Time.time- stopTime>= stopInterval)
            {
                //电梯门关闭
                doorIsMoving = true;
                doorIsOpen = false;
                //电梯开门时候的时间节点记录
                doorMoveTimer = Time.time;
                isMoving = true;//改为移动状态
                moveStartTime = Time.time;//更新标准,上一次启动电梯的时间
                currentFloorIndex = (currentFloorIndex + 1) % floors.Length;//当前移动移动到下一层，需要更新索引
                if (fpc)
                {
                    fpc.lockMove = true;
                    fpc.transform.SetParent(transform);
                }
            }
        }
    }
    /// <summary>
    /// 开关电梯门
    /// </summary>
    /// <param name="isOpen">开门还是关门</param>
    private void OpenOrCloseDoor(bool isOpen)
    {
        if (isOpen)//开门
        {
            if (Time.time- doorMoveTimer<1)//开门需要花费1s
            {
                //正在开门状态时，门需要移动
                Transform doorTrans = doors[currentDoorIndex];
                doorTrans.position -= doorTrans.right * doorMoveSpeed * Time.deltaTime;
            }
        }
        else//关门
        {
            if (Time.time - doorMoveTimer < 1)//开门需要花费1s
            {
                //正在开门状态时，门需要移动
                Transform doorTrans = doors[currentDoorIndex];
                doorTrans.position += doorTrans.right * doorMoveSpeed * Time.deltaTime;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag=="Player")
        {
            fpc= other.GetComponent<FirstPersonController>();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            fpc = null;
        }
    }
}
