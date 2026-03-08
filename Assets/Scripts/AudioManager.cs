using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class AudioManager : MonoBehaviour
{
    [Header("播放器组件")]
    public AudioSource bgmSource;
    public AudioSource sfxSource;

    [Header("背景音乐")]
    public AudioClip mainBGM;

    [Header("UI 音效文件")]
    public AudioClip hoverSound;
    public AudioClip clickSound;
    public AudioClip errorSound;

    [Header("需要绑定音效的按钮列表")]
    public Button[] uiButtons;

    void Start()
    {
        if (bgmSource != null && mainBGM != null)
        {
            bgmSource.clip = mainBGM;
            bgmSource.loop = true;
            bgmSource.Play();
        }

        if (sfxSource == null) sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        foreach (Button btn in uiButtons)
        {
            if (btn != null)
            {
                btn.onClick.AddListener(() => PlaySFX(clickSound));
                AddCustomEvents(btn);
            }
        }
    }

    private void AddCustomEvents(Button btn)
    {
        EventTrigger trigger = btn.gameObject.GetComponent<EventTrigger>();
        if (trigger == null) trigger = btn.gameObject.AddComponent<EventTrigger>();

        EventTrigger.Entry hoverEntry = new EventTrigger.Entry();
        hoverEntry.eventID = EventTriggerType.PointerEnter;
        hoverEntry.callback.AddListener((data) => { PlaySFX(hoverSound); });
        trigger.triggers.Add(hoverEntry);

        EventTrigger.Entry downEntry = new EventTrigger.Entry();
        downEntry.eventID = EventTriggerType.PointerDown;
        downEntry.callback.AddListener((data) =>
        {
            if (!btn.interactable) PlaySFX(errorSound);
        });
        trigger.triggers.Add(downEntry);
    }

    // ========== 关键修改：将 private 改为 public ==========
    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
}