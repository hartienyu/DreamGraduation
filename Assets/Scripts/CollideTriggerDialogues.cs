using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollideTriggerDialogues : MonoBehaviour
{
    private bool isTriggerDialogue = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool IsTriggerDialogue
    {
        get { return isTriggerDialogue; }
        private set { isTriggerDialogue = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        // 检查进入的是否是玩家
        if (other.CompareTag("Player"))
        {
            isTriggerDialogue = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // 玩家离开时重置（可选）
        if (other.CompareTag("Player"))
        {
            isTriggerDialogue = false;
        }
    }
}
