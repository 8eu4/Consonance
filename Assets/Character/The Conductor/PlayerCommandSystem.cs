using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;

public class PlayerCommandSystem : MonoBehaviour
{
    [Header("Settings")]
    public Camera playerCamera;
    public LayerMask groundLayer;
    
    [Tooltip("Seberapa jauh player bisa melihat NPC (meter)")]
    public float maxInteractionRange = 30f; 
    
    [Tooltip("Jarak maksimal untuk bisa melakukan 'Follow Me' (Tap E)")]
    public float closeRangeLimit = 5f; // Kalau lebih dari 5m, gabisa follow

    [Tooltip("Jarak berhenti NPC saat Follow Me")]
    public float followStoppingDistance = 2.5f;

    [Header("Input Settings")]
    public float holdDuration = 0.3f;

    [Header("Visuals")]
    public GameObject arrowPrefab;
    public TextMeshProUGUI promptText;
    
    [Header("UI Colors")]
    public Color normalColor = Color.white;
    public Color actionColor = Color.yellow;
    public Color cancelColor = new Color(1f, 0.3f, 0.3f); 
    public Color deathColor = Color.red;
    public float alertDuration = 0.8f;

    private enum CommandState { Idle, Aiming, MovingToPoint, FollowingPlayer, Cooldown }
    private CommandState currentState = CommandState.Idle;

    private GameObject currentArrow;
    private NavMeshAgent selectedNPC;
    private CanvasGroup uiCanvasGroup;
    private Coroutine fadeCoroutine;

    private float keyPressTimer = 0f;
    private bool isKeyHeld = false;
    private bool actionTriggered = false;

    void Start()
    {
        if(promptText != null)
        {
            uiCanvasGroup = promptText.GetComponent<CanvasGroup>();
            if (uiCanvasGroup == null) uiCanvasGroup = promptText.gameObject.AddComponent<CanvasGroup>();
            uiCanvasGroup.alpha = 0; 
        }
    }

    void Update()
    {
        if (currentState == CommandState.FollowingPlayer || currentState == CommandState.MovingToPoint)
        {
            if (!IsNPCValid()) 
            {
                HandleDeath(); 
                return;
            }
        }

        if (Input.GetKeyDown(KeyCode.Q) && currentState != CommandState.Cooldown)
        {
            CancelAllActions();
            return;
        }

        switch (currentState)
        {
            case CommandState.Idle:
                HandleIdleState();
                break;
            case CommandState.Aiming:
                HandleAimingState();
                break;
            case CommandState.FollowingPlayer:
                HandleFollowingState();
                break;
            case CommandState.MovingToPoint:
                HandleMovingState();
                break;
            case CommandState.Cooldown:
                break;
        }
    }

    // --- LOGIKA UTAMA: JARAK DEKAT vs JAUH ---
    void HandleIdleState()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        bool hitNPC = false;

        if (Physics.Raycast(ray, out hit, maxInteractionRange))
        {
            NavMeshAgent agent = hit.collider.GetComponentInParent<NavMeshAgent>();

            if (hit.collider.CompareTag("NPC") && agent != null && agent.enabled)
            {
                hitNPC = true;
                float distance = Vector3.Distance(transform.position, hit.point);
                bool isClose = distance <= closeRangeLimit;

                // 1. LOGIKA TAMPILAN UI
                if (Input.GetKey(KeyCode.E))
                {
                     // Feedback saat menahan tombol
                     ShowUI("Hold to <color=yellow>COMMAND</color>...", actionColor);
                }
                else
                {
                     if (isClose)
                     {
                         // Jarak Dekat: Muncul DUA pilihan
                         ShowUI("Tap <b>[E]</b> <color=yellow>FOLLOW</color>    Hold <b>[E]</b> <color=yellow>COMMAND</color>", normalColor);
                     }
                     else
                     {
                         // Jarak Jauh: Cuma muncul pilihan COMMAND
                         ShowUI("Hold <b>[E]</b> to <color=yellow>COMMAND</color>", normalColor);
                     }
                }

                // 2. LOGIKA INPUT
                
                // A. Mulai Tekan
                if (Input.GetKeyDown(KeyCode.E))
                {
                    keyPressTimer = 0f;
                    isKeyHeld = true;
                    actionTriggered = false;
                }

                // B. Sedang Menahan (HOLD -> COMMAND)
                // Ini bisa dilakukan dari Jauh maupun Dekat
                if (Input.GetKey(KeyCode.E))
                {
                    keyPressTimer += Time.deltaTime;

                    if (keyPressTimer >= holdDuration && !actionTriggered)
                    {
                        selectedNPC = agent;
                        StartAimingMode(); 
                        actionTriggered = true;
                    }
                }

                // C. Lepas Tombol (TAP -> FOLLOW)
                if (Input.GetKeyUp(KeyCode.E))
                {
                    // Jika dilepas cepat (Tap)
                    if (!actionTriggered)
                    {
                        // --- CEK JARAK DI SINI ---
                        if (isClose)
                        {
                            // Hanya bisa follow kalau DEKAT
                            selectedNPC = agent;
                            StartFollowingPlayer();
                            actionTriggered = true;
                        }
                        else
                        {
                            // Kalau JAUH tapi di-Tap: Kasih feedback error (Optional)
                            // ShowUI("Too Far to Follow!", cancelColor); 
                            // Atau diamkan saja.
                        }
                    }
                    
                    isKeyHeld = false;
                    keyPressTimer = 0f;
                }
            }
        }
        
        if (!hitNPC) HideUI();
    }

    // ... Bagian bawah ini sama persis (Copy Paste saja) ...

    void HandleAimingState()
    {
        ShowUI("Select Location... \n<b>[E]</b> Confirm    <b>[Q]</b> Cancel", actionColor);
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            if (currentArrow != null) currentArrow.transform.position = hit.point;
            if (Input.GetKeyDown(KeyCode.E)) MoveNPCToPoint(hit.point);
        }
    }

    bool IsNPCValid()
    {
        if (selectedNPC == null) return false;
        if (!selectedNPC.gameObject.activeInHierarchy) return false;
        if (!selectedNPC.enabled) return false;
        return true;
    }

    void HandleDeath()
    {
        selectedNPC = null;
        if (currentArrow != null) Destroy(currentArrow);
        StartCoroutine(ShowMessageRoutine("NPC <color=red>DIED</color>", deathColor));
    }

    void HandleFollowingState()
    {
        ShowUI("Following You... \nPress <b>[Q]</b> to Stop", actionColor);
        if (selectedNPC != null) selectedNPC.SetDestination(transform.position);
    }

    void HandleMovingState()
    {
        ShowUI("NPC Moving... \nPress <b>[Q]</b> to Stop", normalColor);
        if (selectedNPC != null)
        {
            if (!selectedNPC.pathPending && selectedNPC.remainingDistance <= selectedNPC.stoppingDistance)
            {
                if (!selectedNPC.hasPath || selectedNPC.velocity.sqrMagnitude == 0f)
                {
                    HideUI();
                    selectedNPC = null;
                    currentState = CommandState.Idle;
                }
            }
        }
    }

    void StartFollowingPlayer()
    {
        currentState = CommandState.FollowingPlayer;
        selectedNPC.stoppingDistance = followStoppingDistance; 
        isKeyHeld = false;
        keyPressTimer = 0f;
    }

    void StartAimingMode()
    {
        currentState = CommandState.Aiming;
        if (currentArrow == null) currentArrow = Instantiate(arrowPrefab);
        currentArrow.SetActive(true);
        isKeyHeld = false;
        keyPressTimer = 0f;
    }

    void MoveNPCToPoint(Vector3 targetPos)
    {
        if (selectedNPC != null) 
        {
            selectedNPC.stoppingDistance = 0.1f; 
            selectedNPC.SetDestination(targetPos);
        }
        if (currentArrow != null) Destroy(currentArrow);
        currentState = CommandState.MovingToPoint;
    }

    void CancelAllActions()
    {
        if (selectedNPC != null)
        {
            selectedNPC.ResetPath(); 
            selectedNPC = null;
        }
        if (currentArrow != null) Destroy(currentArrow);
        StartCoroutine(ShowMessageRoutine("Order    <color=red>CANCELLED</color>", cancelColor));
    }

    IEnumerator ShowMessageRoutine(string msg, Color col)
    {
        currentState = CommandState.Cooldown;
        ShowUI(msg, col);
        yield return new WaitForSeconds(alertDuration); 
        HideUI();
        currentState = CommandState.Idle;
    }

    void ShowUI(string message, Color color)
    {
        if (promptText == null) return;
        promptText.text = message;
        promptText.color = color; 
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(uiCanvasGroup, 1f, 0.2f));
    }

    void HideUI()
    {
        if (uiCanvasGroup == null || uiCanvasGroup.alpha == 0) return;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeCanvasGroup(uiCanvasGroup, 0f, 0.3f));
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        cg.alpha = targetAlpha;
        if (targetAlpha == 0 && promptText != null) promptText.text = "";
    }
}