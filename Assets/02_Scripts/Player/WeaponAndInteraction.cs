using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//*****************************************
//创建人： Trigger 
//功能说明：玩家武器管理、开门交互与射击系统（已剥离移动和生命值）
//*****************************************
public class WeaponAndInteraction : MonoBehaviour
{
    private DoorController dc;
    public Transform caughtTrans;
    private bool beCaught;

    private WEAPONTYPE weaponType = WEAPONTYPE.FLASHLIGHT;
    public GameObject[] weaponGos;
    private Animator animator;
    public float attackCD;

    public GameObject flashLightGo;
    private bool usingFlashLight;

    private Dictionary<WEAPONTYPE, int> bag = new Dictionary<WEAPONTYPE, int>();
    private Dictionary<WEAPONTYPE, int> clip = new Dictionary<WEAPONTYPE, int>();
    public int clipFlashLigtBattery = 1;
    public int clipMaxSingleShootBullet = 5;
    public int clipMaxAutoShootBullet = 10;

    private bool isReloading;
    private float attackTimer;

    public Transform attackEffectTrans;
    public GameObject attackEffectGo;
    public float lerpSpeed;
    public float moveDistance = -0.2f;
    private float elapsedTime;
    private bool movingForward;
    private Vector3 initPos;
    private Vector3 targetPos;
    public Transform handTrans;
    private bool isPlayingMoveAnimation;

    public AudioSource soundAudio;
    public AudioClip flashLightSound;
    public AudioClip singleShootSound;
    public AudioClip autoShootSound;
    public AudioClip reloadSound;

    void Start()
    {
        animator = GetComponentInChildren<Animator>();

        bag.Add(WEAPONTYPE.FLASHLIGHT, 1);
        bag.Add(WEAPONTYPE.SINGLESHOT, 20);
        bag.Add(WEAPONTYPE.AUTO, 40);

        clip.Add(WEAPONTYPE.FLASHLIGHT, clipFlashLigtBattery);
        clip.Add(WEAPONTYPE.SINGLESHOT, clipMaxSingleShootBullet);
        clip.Add(WEAPONTYPE.AUTO, clipMaxAutoShootBullet);

        initPos = handTrans.localPosition;
        targetPos = initPos + new Vector3(0, 0, moveDistance);
    }

    void Update()
    {
        if (beCaught) return;

        Attack();
        ToggleDoor();
        ChangeWeapon();
        Reload();
        PlayGunAnimation();
    }

    private void UseFlashLight()
    {
        if (Input.GetMouseButtonDown(0))
        {
            usingFlashLight = !usingFlashLight;
            flashLightGo.SetActive(usingFlashLight);
            soundAudio.PlayOneShot(flashLightSound);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Door")) dc = other.GetComponent<DoorController>();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Door")) dc = null;
    }

    private void ToggleDoor()
    {
        if (Input.GetKeyDown(KeyCode.Space) && dc)
        {
            dc.ToggleDoor();
        }
    }

    public Vector3 BeCaught()
    {
        beCaught = true;
        return caughtTrans.position;
    }

    private void ChangeWeapon()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            usingFlashLight = false;
            flashLightGo.SetActive(usingFlashLight);

            weaponType++;
            weaponType = (WEAPONTYPE)(((int)weaponType) % 3);

            ChangeWeaponGameObject();
            animator.SetTrigger("ChangeWeapon");

            switch (weaponType)
            {
                case WEAPONTYPE.FLASHLIGHT:
                    attackCD = 0;
                    animator.SetBool("UseGun", false);
                    break;
                case WEAPONTYPE.SINGLESHOT:
                    attackCD = 0.2f;
                    animator.SetBool("UseGun", true);
                    break;
                case WEAPONTYPE.AUTO:
                    attackCD = 0.1f;
                    animator.SetBool("UseGun", true);
                    break;
            }
        }
    }

    private void ChangeWeaponGameObject()
    {
        for (int i = 0; i < weaponGos.Length; i++)
        {
            weaponGos[i].SetActive(false);
        }
        weaponGos[(int)weaponType].SetActive(true);
    }

    private int GetMaxClipSize(WEAPONTYPE type)
    {
        if (type == WEAPONTYPE.FLASHLIGHT) return clipFlashLigtBattery;
        if (type == WEAPONTYPE.SINGLESHOT) return clipMaxSingleShootBullet;
        return clipMaxAutoShootBullet;
    }

    private void Reload()
    {
        if (!Input.GetKeyDown(KeyCode.R) || isReloading) return;

        int maxCapacity = GetMaxClipSize(weaponType);

        if (clip[weaponType] < maxCapacity && bag[weaponType] > 0)
        {
            isReloading = true;
            soundAudio.PlayOneShot(reloadSound);
            animator.SetTrigger("Reload");
            Invoke("RecoverAttackState", 3f);

            int needNum = maxCapacity - clip[weaponType];

            if (bag[weaponType] >= needNum)
            {
                bag[weaponType] -= needNum;
                clip[weaponType] += needNum;
            }
            else
            {
                clip[weaponType] += bag[weaponType];
                bag[weaponType] = 0;
            }
        }
    }

    private void RecoverAttackState()
    {
        isReloading = false;
    }

    private void Attack()
    {
        if (isReloading) return;

        switch (weaponType)
        {
            case WEAPONTYPE.FLASHLIGHT:
                UseFlashLight();
                break;
            case WEAPONTYPE.SINGLESHOT:
                if (Input.GetMouseButtonDown(0)) GunAttack();
                break;
            case WEAPONTYPE.AUTO:
                if (Input.GetMouseButton(0)) GunAttack();
                break;
        }
    }

    private void GunAttack()
    {
        if (Time.time - attackTimer >= attackCD)
        {
            if (clip[weaponType] > 0)
            {
                clip[weaponType]--;
                attackTimer = Time.time;

                GameObject go = Instantiate(attackEffectGo, attackEffectTrans);
                go.transform.localPosition = Vector3.zero;

                isPlayingMoveAnimation = true;
                soundAudio.PlayOneShot(weaponType == WEAPONTYPE.SINGLESHOT ? singleShootSound : autoShootSound);

                RaycastHit hit;
                if (Physics.Raycast(attackEffectTrans.position, attackEffectTrans.forward, out hit, 5))
                {
                    if (hit.collider.CompareTag("Enemy"))
                    {
                        hit.collider.GetComponent<Enemy>().TakeDamage(4);
                    }
                }
            }
            else
            {
                Reload();
            }
        }
    }

    private void PlayGunAnimation()
    {
        if (!isPlayingMoveAnimation) return;

        if (movingForward) MoveGun(initPos);
        else MoveGun(targetPos);
    }

    private void MoveGun(Vector3 target)
    {
        handTrans.localPosition = Vector3.Lerp(handTrans.localPosition, target, elapsedTime / (attackCD / 2));
        elapsedTime += Time.deltaTime;

        if (Vector3.Distance(handTrans.localPosition, target) <= 0.01f)
        {
            movingForward = !movingForward;
            elapsedTime = 0;
            if (!movingForward) isPlayingMoveAnimation = false;
        }
    }
}

public enum WEAPONTYPE
{
    FLASHLIGHT,
    SINGLESHOT,
    AUTO
}