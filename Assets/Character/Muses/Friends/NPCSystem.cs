using UnityEngine;

public class NPCSystem : MonoBehaviour
{
    bool player_detection = false;
    bool isFollowing = false;

    [Header("UI Interaksi")]
    public GameObject interactionUI; // drag UI Canvas/Panel ke sini lewat Inspector

    private DomiAI domiAI;
    private Transform player;

    void Start()
    {
        if (interactionUI != null)
            interactionUI.SetActive(false);

        // ambil komponen DomiAI dari parent (karena script ini ada di child trigger sphere)
        domiAI = GetComponentInParent<DomiAI>();
    }

    void Update()
    {
        if (player_detection && Input.GetKeyDown(KeyCode.E))
        {
            if (!isFollowing)
            {
                Debug.Log("AI mulai mengikuti player!");
                domiAI.SetTarget(player);
                isFollowing = true;
            }
            else
            {
                Debug.Log("AI berhenti mengikuti player!");
                domiAI.RemoveTarget();
                isFollowing = false;
            }
            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player_detection = true;
            player = other.transform;

            if (interactionUI != null)
                interactionUI.SetActive(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player_detection = false;

            if (interactionUI != null)
                interactionUI.SetActive(false);
        }
    }
}
