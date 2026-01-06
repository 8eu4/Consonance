using UnityEngine;
using UnityEngine.AI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

public class PlayerCommandSystem : MonoBehaviour
{
    [Header("--- SECURITY (AUTO) ---")]
    [Tooltip("Script ini hanya aktif jika object ini memiliki Tag 'Player'.")]
    public string requiredTag = "Player";

    [Header("Settings")]
    public Camera playerCamera; // Masukkan Main Camera
    public LayerMask groundLayer;
    
    [Header("Target Tags")]
    public string[] allowableTags = { "Domi", "Remi", "NPC" }; 
    
    [Header("Movement Speed")]
    public float npcSpeed = 8.0f; 
    public float npcAcceleration = 60.0f;

    [Header("Interaction Settings")]
    public float maxInteractionRange = 50f; 
    public float closeContactDistance = 5f; 
    public float followStoppingDistance = 2.5f;

    [Header("Input Settings")]
    public float holdDuration = 0.25f; 

    [Header("Visuals")]
    public GameObject arrowPrefab;
    public TextMeshProUGUI promptText;
    
    [Header("UI Colors")]
    public Color idleColor = Color.white;
    public Color actionColor = Color.yellow;
    public Color stopColor = new Color(1f, 0.5f, 0.5f); 

    // Internal Variables
    private List<NavMeshAgent> activeFollowers = new List<NavMeshAgent>(); 
    private NavMeshAgent hoveredNPC;        
    private NavMeshAgent commandTarget;     
    
    private GameObject currentArrow;
    private CanvasGroup uiCanvasGroup;
    private Coroutine fadeCoroutine;

    private float keyPressTimer = 0f;
    private bool isHolding = false;     
    private bool isCommanding = false;  

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
        // -------------------------------------------------------------
        // 1. SATPAM TAG (AUTO SWITCH SAFETY)
        // -------------------------------------------------------------
        // Cek: Apakah object ini (Conductor) masih punya tag "Player"?
        // Kalau kamu lagi mainin Domi, tag Conductor pasti berubah, jadi ini jadi FALSE.
        if (!gameObject.CompareTag(requiredTag))
        {
            // Matikan visual jika mendadak ganti karakter
            if(uiCanvasGroup != null && uiCanvasGroup.alpha > 0) HideUI();
            if(currentArrow != null && currentArrow.activeSelf) currentArrow.SetActive(false);
            
            // Reset status input biar ga nyangkut
            isHolding = false;
            isCommanding = false;
            
            // Stop proses update di sini. Script "Tidur".
            return; 
        }
        // -------------------------------------------------------------

        // 2. CLEANUP (Hapus NPC mati)
        activeFollowers.RemoveAll(agent => agent == null || !agent.isActiveAndEnabled);

        // 3. UPDATE SQUAD MOVEMENT
        foreach (var follower in activeFollowers)
        {
            if (follower != null && follower.isOnNavMesh)
            {
                follower.speed = npcSpeed;
                follower.acceleration = npcAcceleration;
                
                // --- PERBAIKAN DI SINI ---
                
                // Kita hitung manual jarak follower ke "Conductor/Player"
                float distToPlayer = Vector3.Distance(follower.transform.position, transform.position);

                // Pastikan stopping distance sesuai settingan script
                follower.stoppingDistance = followStoppingDistance;

                // LOGIKA BARU:
                // Jika jarak LEBIH JAUH dari batas berhenti -> Jalan mendekat
                // Tambahkan sedikit buffer (+ 0.5f) biar dia gak maju-mundur di perbatasan
                if (distToPlayer > followStoppingDistance + 0.5f)
                {
                    follower.isStopped = false;
                    follower.SetDestination(transform.position);
                }
                // Jika sudah DEKAT -> Stop total biar gak geter
                else if (distToPlayer <= followStoppingDistance)
                {
                    if (!follower.isStopped)
                    {
                        follower.isStopped = true;
                        follower.velocity = Vector3.zero; // Matikan sisa momentum
                        follower.ResetPath(); // Hapus jalur biar gak maksa jalan
                    }
                }
            }
        }

        // 4. CORE LOGIC
        PerformRaycast();
        HandleInput();
    }

    void PerformRaycast()
    {
        // Pastikan kamera ada (kadang kamera ikut pindah object)
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;
        hoveredNPC = null;

        if (Physics.Raycast(ray, out hit, maxInteractionRange))
        {
            NavMeshAgent agent = hit.collider.GetComponentInParent<NavMeshAgent>();
            if (agent != null && HasValidTag(agent.gameObject) && agent.enabled)
            {
                hoveredNPC = agent;
            }
        }
    }

    void HandleInput()
    {
        // A. MULAI TEKAN
        if (Input.GetKeyDown(KeyCode.F))
        {
            keyPressTimer = 0f;
            isHolding = false;
            isCommanding = false;
            commandTarget = null;
            if (hoveredNPC != null) commandTarget = hoveredNPC;
            else if (activeFollowers.Count > 0) commandTarget = null; 
        }

        // B. TAHAN
        if (Input.GetKey(KeyCode.F))
        {
            keyPressTimer += Time.deltaTime;
            if (keyPressTimer >= holdDuration)
            {
                isHolding = true;
                isCommanding = true;
                ShowCommandMarker(); 
            }
        }

        // C. LEPAS
        if (Input.GetKeyUp(KeyCode.F))
        {
            if (isHolding && isCommanding) ExecuteMoveCommand(); 
            else HandleTapAction();

            if (currentArrow != null) currentArrow.SetActive(false);
            isHolding = false;
            isCommanding = false;
            commandTarget = null;
        }

        // D. UPDATE UI
        if (!isCommanding) UpdateStatusUI();
    }

    void ShowCommandMarker()
    {
        Camera cam = playerCamera != null ? playerCamera : Camera.main;
        if (cam == null) return;

        Ray ray = cam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f, groundLayer))
        {
            if (currentArrow == null && arrowPrefab != null) currentArrow = Instantiate(arrowPrefab);
            
            if (currentArrow != null)
            {
                currentArrow.SetActive(true);
                currentArrow.transform.position = hit.point;
                currentArrow.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            }
            string targetName = (commandTarget != null) ? commandTarget.tag : "SQUAD";
            ShowUI($"Move {targetName}... Release [F]", actionColor);
        }
    }

    void ExecuteMoveCommand()
    {
        if (currentArrow == null || !currentArrow.activeSelf) return;
        Vector3 destination = currentArrow.transform.position;

        if (commandTarget != null)
        {
            MoveAgent(commandTarget, destination);
            if (activeFollowers.Contains(commandTarget)) activeFollowers.Remove(commandTarget);
            ShowUI($"{commandTarget.tag} Moving...", actionColor);
        }
        else if (activeFollowers.Count > 0)
        {
            foreach (var npc in activeFollowers) MoveAgent(npc, destination);
            ShowUI("Squad Moving...", actionColor);
        }
    }

    void HandleTapAction()
    {
        if (hoveredNPC != null)
        {
            // LOGIKA CANCEL PRIORITAS (Kalau lagi jalan -> STOP)
            bool isBusy = (hoveredNPC.velocity.magnitude > 0.1f) || activeFollowers.Contains(hoveredNPC) || hoveredNPC.hasPath;

            if (isBusy)
            {
                StopAgent(hoveredNPC);
                ShowUI($"{hoveredNPC.tag} Stopped.", stopColor);
            }
            else
            {
                float dist = Vector3.Distance(transform.position, hoveredNPC.transform.position);
                if (dist <= closeContactDistance)
                {
                    RecruitAgent(hoveredNPC);
                    ShowUI($"{hoveredNPC.tag} Following!", actionColor);
                }
                else
                {
                    ShowUI("Too Far. Hold [F] to Move.", stopColor);
                }
            }
        }
        else
        {
            StopAll();
        }
    }

    void StopAgent(NavMeshAgent npc)
    {
        if (activeFollowers.Contains(npc)) activeFollowers.Remove(npc);
        if (npc.isOnNavMesh && npc.isActiveAndEnabled)
        {
            npc.ResetPath();
            npc.velocity = Vector3.zero;
        }
    }

    void RecruitAgent(NavMeshAgent npc)
    {
        if (!activeFollowers.Contains(npc))
        {
            npc.speed = npcSpeed;
            npc.acceleration = npcAcceleration;
            activeFollowers.Add(npc);
        }
    }

    void StopAll()
    {
        if (activeFollowers.Count > 0)
        {
            foreach (var npc in activeFollowers)
            {
                if (npc != null && npc.isOnNavMesh) 
                {
                    npc.ResetPath();
                    npc.velocity = Vector3.zero;
                }
            }
            activeFollowers.Clear();
            ShowUI("All Units Stopped.", stopColor);
        }
    }

    void MoveAgent(NavMeshAgent agent, Vector3 pos)
    {
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.speed = npcSpeed;
            agent.acceleration = npcAcceleration;
            agent.stoppingDistance = 0.5f;
            agent.SetDestination(pos);
        }
    }

    void UpdateStatusUI()
    {
        if (hoveredNPC != null)
        {
            bool isBusy = (hoveredNPC.velocity.magnitude > 0.1f) || activeFollowers.Contains(hoveredNPC) || hoveredNPC.hasPath;
            string name = hoveredNPC.tag;

            if (isBusy)
            {
                ShowUI($"Tap [F] Stop {name}", stopColor);
            }
            else
            {
                float dist = Vector3.Distance(transform.position, hoveredNPC.transform.position);
                if (dist <= closeContactDistance)
                    ShowUI($"Tap [F] Follow {name} | Hold [F] Move", idleColor);
                else
                    ShowUI($"Hold [F] Command {name} Move", idleColor);
            }
        }
        else
        {
            if (activeFollowers.Count > 0)
                ShowUI($"Following: {activeFollowers.Count} Units", idleColor); 
            else
                HideUI();
        }
    }

    bool HasValidTag(GameObject obj)
    {
        foreach (string tag in allowableTags)
        {
            if (obj.CompareTag(tag)) return true;
        }
        return false;
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
            if (cg == null) yield break; 
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            yield return null;
        }
        if (cg != null) cg.alpha = targetAlpha;
        if (targetAlpha == 0 && promptText != null) promptText.text = "";
    }
}