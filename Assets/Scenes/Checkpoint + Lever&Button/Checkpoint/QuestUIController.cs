using UnityEngine;
using TMPro;

public class QuestUIController : MonoBehaviour
{
    public static QuestUIController Instance { get; private set; }

    [Header("UI Reference")]
    public TMP_Text currentQuestText;

    [Header("Default Quest")]
    [TextArea(2, 4)]
    public string defaultQuestText = "Begin your journey.";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject); // persist Quest UI across scenes
    }

    void Start()
    {
        ResetQuest();
    }

    public void SetQuest(string questText)
    {
        if (currentQuestText == null)
        {
            Debug.LogWarning("[QuestUI] currentQuestText is NULL");
            return;
        }

        currentQuestText.text = questText;
        Debug.Log("[QuestUI] Quest Updated: " + questText);
    }

    public void ResetQuest()
    {
        if (currentQuestText == null)
        {
            Debug.LogWarning("[QuestUI] currentQuestText is NULL");
            return;
        }

        currentQuestText.text = defaultQuestText;
        Debug.Log("[QuestUI] Quest Reset");
    }
}
