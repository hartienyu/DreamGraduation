using UnityEngine;
using UnityEngine.EventSystems; // 包含鼠标悬停和 UI 选中事件
using TMPro;

// 新增了 ISelectHandler 和 IDeselectHandler 接口
public class JitterText : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [Header("抖动设置")]
    public float jitterMultiplier = 1.5f; // 抖动幅度

    [Header("触发模式")]
    [Tooltip("勾选：只有鼠标悬停或被键盘选中时才会抖；不勾选：一直抖")]
    public bool onlyOnHoverOrSelect = false;    // 核心开关（稍微改了名字更符合现在的逻辑）

    private TMP_Text textMesh;
    private bool isHovering = false;      // 记录当前鼠标是否在上方
    private bool isSelected = false;      // 新增：记录当前是否被系统焦点选中

    void Start()
    {
        textMesh = GetComponentInChildren<TMP_Text>();
        if (textMesh == null)
        {
            Debug.LogError("【JitterText】没有找到 TMP_Text 组件！请检查挂载位置。");
        }
    }

    void Update()
    {
        if (textMesh == null) return;

        // 如果开启了条件触发，且当前【既没有悬停】也【没有被选中】，则跳出不抖动
        if (onlyOnHoverOrSelect && !isHovering && !isSelected) return;

        // 否则执行抖动逻辑
        ApplyJitter();
    }

    private void ApplyJitter()
    {
        textMesh.ForceMeshUpdate();
        TMP_TextInfo textInfo = textMesh.textInfo;

        for (int i = 0; i < textInfo.characterCount; i++)
        {
            TMP_CharacterInfo charInfo = textInfo.characterInfo[i];

            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;

            Vector3[] sourceVertices = textInfo.meshInfo[materialIndex].vertices;
            Vector2 randomOffset = Random.insideUnitCircle * jitterMultiplier;

            sourceVertices[vertexIndex + 0] += (Vector3)randomOffset;
            sourceVertices[vertexIndex + 1] += (Vector3)randomOffset;
            sourceVertices[vertexIndex + 2] += (Vector3)randomOffset;
            sourceVertices[vertexIndex + 3] += (Vector3)randomOffset;
        }

        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            textInfo.meshInfo[i].mesh.vertices = textInfo.meshInfo[i].vertices;
            textMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Vertices);
        }
    }

    // ================= 提取出的网格恢复方法 =================
    private void CheckAndRestoreMesh()
    {
        // 只有在开启条件触发，且彻底失去所有焦点（没悬停且没选中）时，才强制复原文字排版
        if (onlyOnHoverOrSelect && !isHovering && !isSelected && textMesh != null)
        {
            textMesh.ForceMeshUpdate();
        }
    }

    // ================= 鼠标事件回调 =================

    public void OnPointerEnter(PointerEventData eventData)
    {
        isHovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        CheckAndRestoreMesh();
    }

    // ================= 选中事件回调 (新增) =================

    public void OnSelect(BaseEventData eventData)
    {
        isSelected = true;
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isSelected = false;
        CheckAndRestoreMesh();
    }
}