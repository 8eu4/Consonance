using UnityEngine;
using UnityEngine.UI;
using TMPro; 
using System.Collections;

public class GuidebookTrigger : MonoBehaviour
{
    [Header("UI Guidebook (Hologram)")]
    public GameObject guidePanel;       
    public CanvasGroup guideCanvasGroup; 
    public float fadeDuration = 1.0f; // Saya lambatin dikit biar smooth

    [Header("Subtitle System (Wajib Diisi)")]
    public GameObject subtitlePanel;     // Panel Hitam
    public CanvasGroup subtitleCanvasGroup; // CanvasGroup di Panel Hitam
    public TMP_Text subtitleText;        // TextMeshPro-nya
    [TextArea(3, 5)]                    
    public string subtitleContent;       

    [Header("Visual Melayang")]
    public bool useFloatingEffect = true; 
    public float floatSpeed = 2.0f;       
    public float floatDistance = 10.0f;   

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip guideVoiceClip;    
    [Range(0f, 1f)] public float volume = 1.0f;

    [Header("Player Control")]
    public bool freezePlayer = true;      
    public GameObject playerObject;       
    public float extraReadingTime = 2.0f; 

    // Internal State
    private bool hasTriggered = false;
    private Vector3 originalUIPos; 
    private Vector3 lockedPosition;
    private Rigidbody cachedRb;
    private CharacterController cachedCc;
    private bool isFrozen = false;
    private bool isGuideActive = false;

    void Start()
    {
        // 1. Matikan Guidebook Awal
        if (guidePanel != null) 
        {
            guidePanel.SetActive(false);
            originalUIPos = guidePanel.GetComponent<RectTransform>().anchoredPosition;
            if (guideCanvasGroup != null) guideCanvasGroup.alpha = 0f;
        }

        // 2. Matikan Subtitle Awal (PENTING)
        if (subtitlePanel != null) 
        {
            subtitlePanel.SetActive(false);
            // Otomatis cari CanvasGroup kalau lupa di-drag
            if (subtitleCanvasGroup == null) subtitleCanvasGroup = subtitlePanel.GetComponent<CanvasGroup>();
            // Paksa Alpha 0 biar gak kaget munculnya
            if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = 0f;
        }
        
        // Cache Player
        if (playerObject != null)
        {
            cachedRb = playerObject.GetComponent<Rigidbody>();
            cachedCc = playerObject.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        // Floating Effect
        if (isGuideActive && useFloatingEffect && guidePanel != null)
        {
            float newY = originalUIPos.y + (Mathf.Sin(Time.time * floatSpeed) * floatDistance);
            guidePanel.GetComponent<RectTransform>().anchoredPosition = new Vector3(originalUIPos.x, newY, 0);
        }

        // Freeze Position
        if (isFrozen && playerObject != null)
        {
            if(cachedRb != null) cachedRb.transform.position = lockedPosition;
            else if(cachedCc != null) cachedCc.transform.position = lockedPosition;
            else playerObject.transform.position = lockedPosition;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!hasTriggered && (other.CompareTag("Player") || other.transform.root.CompareTag("Player")))
        {
            if (playerObject == null) playerObject = other.transform.root.gameObject;
            StartCoroutine(ShowSequence());
        }
    }

    IEnumerator ShowSequence()
    {
        hasTriggered = true;
        isGuideActive = true;

        // 1. FREEZE
        if (freezePlayer) ToggleFreeze(true);

        // 2. SETUP AWAL (Sebelum Muncul)
        // Reset Alpha ke 0 dulu biar ga nge-blink
        if (guideCanvasGroup != null) guideCanvasGroup.alpha = 0f;
        if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = 0f;
        
        // Isi Teks
        if (subtitleText != null) subtitleText.text = subtitleContent;

        // Nyalakan Object
        if (guidePanel != null) guidePanel.SetActive(true);
        if (subtitlePanel != null) subtitlePanel.SetActive(true);

        // 3. ANIMASI FADE IN (Looping Smooth)
        float timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            
            // Rumus SmoothStep biar lebih halus dari Lerp biasa
            float smoothAlpha = Mathf.SmoothStep(0f, 1f, progress);

            if (guideCanvasGroup != null) guideCanvasGroup.alpha = smoothAlpha;
            if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = smoothAlpha;
            
            yield return null;
        }
        // Kunci Alpha di 1
        if (guideCanvasGroup != null) guideCanvasGroup.alpha = 1f;
        if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = 1f;

        // 4. AUDIO
        float wait = 2.0f;
        if (audioSource != null && guideVoiceClip != null)
        {
            audioSource.PlayOneShot(guideVoiceClip, volume);
            wait = guideVoiceClip.length;
        }
        yield return new WaitForSeconds(wait + extraReadingTime);

        // 5. ANIMASI FADE OUT
        timer = 0;
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;
            float smoothAlpha = Mathf.SmoothStep(1f, 0f, progress);

            if (guideCanvasGroup != null) guideCanvasGroup.alpha = smoothAlpha;
            if (subtitleCanvasGroup != null) subtitleCanvasGroup.alpha = smoothAlpha;
            
            yield return null;
        }

        // 6. BERSIH-BERSIH
        if (guidePanel != null) guidePanel.SetActive(false);
        if (subtitlePanel != null) subtitlePanel.SetActive(false);
        if (subtitleText != null) subtitleText.text = "";
        
        isGuideActive = false;

        // 7. UNFREEZE
        if (freezePlayer) ToggleFreeze(false);
        
        // Matikan Trigger
        GetComponent<Collider>().enabled = false;
    }

    void ToggleFreeze(bool state) // True = Beku, False = Cair
    {
        if (playerObject == null) return;

        // Matikan script gerak
        MonoBehaviour[] scripts = playerObject.GetComponentsInChildren<MonoBehaviour>();
        foreach(var s in scripts) 
        {
            string n = s.GetType().Name;
            if (n.Contains("Move") || n.Contains("Controller") || n.Contains("Look"))
                if(s != this) s.enabled = !state;
        }

        // Kunci Fisik
        if (state)
        {
            if(cachedRb != null) lockedPosition = cachedRb.transform.position;
            else if(cachedCc != null) lockedPosition = cachedCc.transform.position;
            
            isFrozen = true;
            if(cachedRb != null) { cachedRb.isKinematic = true; cachedRb.linearVelocity = Vector3.zero; }
        }
        else
        {
            isFrozen = false;
            if(cachedRb != null) { cachedRb.isKinematic = false; }
        }
        
        // Animasi Idle
        Animator anim = playerObject.GetComponent<Animator>();
        if(anim == null) anim = playerObject.GetComponentInChildren<Animator>();
        if (anim != null && state) { anim.SetFloat("Speed", 0f); anim.Play("Idle"); }
    }
}       