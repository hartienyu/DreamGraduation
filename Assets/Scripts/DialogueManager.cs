using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class DialogueLine
{
    public string speaker;
    public string content;
}

[System.Serializable]
public class DialogueData
{
    public DialogueLine[] dialoguePlot2_1;  // Plot 2.1 dialogue
    public DialogueLine[] dialoguePlot2_2;
    public DialogueLine[] dialoguePlot2_3;
    public DialogueLine[] dialoguePlot2_4;
}

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("UI ×é¼þÒýÓÃ")]
    public GameObject dialogueUI;
    // ÏÖÔÚÖ»ÐèÒªÒ»¸ö Text ×é¼þÁË
    public TextMeshProUGUI combinedText;

    private int currentPlotIndex = 1;
    private DialogueLine[] currentLines;
    private int currentLineIndex = 0;
    private bool isTalking = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Update()
    {
        if (isTalking && (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)))
        {
            ShowNextLine();
        }
    }

    // ½ÓÊÕ TextAsset (JSONÎÄ¼þ) ²¢¿ªÊ¼²¥·Å
    public void StartDialogue(TextAsset jsonFile)
    {
        if (isTalking || jsonFile == null) return;

        DialogueData data = JsonUtility.FromJson<DialogueData>(jsonFile.text);

        // ½âÎö JSON ÎÄ±¾Îª C# ¶ÔÏó
        string fieldName = "dialoguePlot2_" + currentPlotIndex; // n = 1,2,3,...
        var field = typeof(DialogueData).GetField(fieldName);
        if (field != null)
        {
            currentLines = field.GetValue(data) as DialogueLine[];

            if (currentLines != null && currentLines.Length > 0)
            {
                currentLineIndex = 0;
                isTalking = true;
                dialogueUI.SetActive(true);
                ShowNextLine();
                currentPlotIndex++;  // Plot index +1 every time this function is called (move to the next plot dialogue).
            }
        }
    }

    private void ShowNextLine()
    {
        if (currentLineIndex >= currentLines.Length)
        {
            EndDialogue();
            return;
        }

        DialogueLine line = currentLines[currentLineIndex];

        // ¡¾ºËÐÄÐÞ¸Äµã¡¿½«Ãû×ÖºÍÄÚÈÝºÏ²¢£¬²¢Ê¹ÓÃ¸»ÎÄ±¾¸øÃû×ÖÉÏÉ«
        // ÕâÀïÓÃÁË½ð»ÆÉ«(#FFD700)²¢½«Ãû×Ö¼Ó´Ö(<b>)£¬Äã¿ÉÒÔ¸ù¾ÝÐèÒªÐÞ¸Ä
        combinedText.text = $"<b><color=#FFD700>[{line.speaker}]</color></b>\n{line.content}";

        currentLineIndex++;
    }

    private void EndDialogue()
    {
        isTalking = false;
        dialogueUI.SetActive(false);

        // ¡¾ÐÂÔö¡¿¶Ô»°Ò»½áÊø£¬Á¢¿ÌÆô¶¯ 60 Ãëµ¹¼ÆÊ±
        if (CountdownTimer.Instance != null)
        {
            CountdownTimer.Instance.StartCountdown(60f); // ÕâÀïµÄ 60f ¿ÉÒÔ¸Ä³ÉÄãÏëÒªµÄÃëÊý
        }
    }
}