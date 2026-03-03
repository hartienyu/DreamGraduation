using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//*****************************************
//创建人： Trigger 
//功能说明：销毁游戏物体
//***************************************** 
public class EffectDestory : MonoBehaviour
{
    public float destroyTime;//销毁时间

    void Start()
    {
        Destroy(gameObject, destroyTime);
    }

    void Update()
    {

    } 
}
