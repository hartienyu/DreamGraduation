using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
//*****************************************
//创建人： Trigger 
//功能说明：女鬼
//***************************************** 
public class FemaleGhost : MonoBehaviour
{
    private FirstPersonController fpc;
    private bool seeMe;//玩家是否在看女鬼
    private Animator animator;
    public GameObject femaleGhostLightGo;//女鬼的灯
    public AudioSource audioSouce;
    public AudioClip audioClip;
    public AudioClip turnHeadClip;//第一次扭头音效
    public AudioSource bgMusicSouce;//撞见女鬼时需要持续播放的音效

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (fpc!=null)//女鬼有目标开始行动
        {
            //取同一平面位置
            Vector3 targetPos = new Vector3(fpc.transform.position.x
                ,transform.position.y,fpc.transform.position.z);
            //盯着玩家看
            transform.LookAt(targetPos);
            if (seeMe)//如果是看女鬼的时候需要检测玩家是不是扭头不再看了
            {
                if (Vector3.Angle(transform.forward,fpc.transform.
                    forward)<90)//玩家正前方与女鬼正前方夹角小于90，玩家不看了
                {
                    seeMe = false;//状态设置为不看
                }
            }
            else//如果没有看女鬼，需要检测玩家是不是要偷看女鬼
            {
                if (Vector3.Angle(transform.forward,fpc.transform.forward)
                    >90)//玩家与女鬼夹角大于90，说明扭头看了
                {
                    seeMe = true;
                    transform.position += (targetPos - transform.
                        position) / 2;//女鬼更新位置
                    audioSouce.PlayOneShot(audioClip);
                    if (Vector3.Distance(transform.position,targetPos)<=1)
                    {
                        CatchPlayer();
                    }
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (fpc==null&&other.CompareTag("Player"))
        {
            fpc = other.GetComponent<FirstPersonController>();
            seeMe = true;
            audioSouce.PlayOneShot(turnHeadClip);
            transform.position += (new Vector3(fpc.transform.position.x
                , transform.position.y, fpc.transform.position.z) - transform.
                   position) / 2;//女鬼更新位置
            Invoke("PlayBGMusic",2.5f);
        }
    }
    /// <summary>
    /// 抓住玩家
    /// </summary>
    private void CatchPlayer()
    {
        animator.CrossFade("ReayEat",1);
        transform.position= fpc.BeCaught();
        femaleGhostLightGo.SetActive(true);
        Invoke("Replay",5);//延时5S重新加载游戏
    }
    /// <summary>
    /// 重玩
    /// </summary>
    private void Replay()
    {
        SceneManager.LoadScene(0);
    }
    /// <summary>
    /// 播放背景音乐
    /// </summary>
    private void PlayBGMusic()
    {
        bgMusicSouce.Play();
    }
}
