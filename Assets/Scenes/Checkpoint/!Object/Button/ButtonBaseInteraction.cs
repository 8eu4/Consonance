// ButtonBaseInteraction.cs
using UnityEngine;

[DisallowMultipleComponent]
public class ButtonBaseInteraction : MonoBehaviour
{
    [Header("References")]
    public Transform player;                 // drag player transform
    public GameObject interactionUI;         // UI hint (e.g. "[Button (E)]")
    public Transform buttonVisual;           // optional visual transform to animate press

    [Header("Button Settings")]
    public float triggerDistance = 2.5f;     // distance to allow press
    public KeyCode interactKey = KeyCode.E;  // key to press
    public float activeDuration = 2f;        // durasi ON sebelum auto-OFF
    public float cooldown = 0.3f;            // prevent double-press
    public float pressDepth = 0.08f;         // visual press distance (local)
    public float pressSpeed = 6f;            // anim speed

    [Header("Custom Behavior")]
    [Tooltip("Drag scripts that implement IButtonAction here")]
    public MonoBehaviour[] buttonActions;    // any MonoBehaviour that implements IButtonAction

    bool isPlayerNear = false;
    bool isActive = false;
    bool isCooling = false;

    Vector3 buttonDefaultLocalPos;

    void Start()
    {
        if (interactionUI != null) interactionUI.SetActive(false);
        if (buttonVisual != null) buttonDefaultLocalPos = buttonVisual.localPosition;
    }

    void Update()
    {
        if (player == null) return;

        isPlayerNear = Vector3.Distance(player.position, transform.position) <= triggerDistance;

        if (interactionUI != null) interactionUI.SetActive(isPlayerNear);

        if (!isPlayerNear) return;

        if (!isActive && !isCooling && Input.GetKeyDown(interactKey))
        {
            StartCoroutine(ActivateOnce());
        }
    }

    System.Collections.IEnumerator ActivateOnce()
    {
        // start cooldown immediately
        isCooling = true;
        isActive = true;

        // visual press down (start)
        if (buttonVisual != null)
            StartCoroutine(AnimatePress(true));

        // notify pressed
        foreach (var mb in buttonActions)
        {
            if (mb is IButtonAction a) a.OnButtonPressed();
        }

        // wait active duration
        float t = 0f;
        while (t < activeDuration)
        {
            t += Time.deltaTime;
            yield return null;
        }

        // notify released (OFF)
        foreach (var mb in buttonActions)
        {
            if (mb is IButtonAction a) a.OnButtonReleased();
        }

        // visual release
        if (buttonVisual != null)
            StartCoroutine(AnimatePress(false));

        isActive = false;

        // finish cooldown (allow re-press after cooldown time)
        yield return new WaitForSeconds(cooldown);
        isCooling = false;
    }

    System.Collections.IEnumerator AnimatePress(bool down)
    {
        Vector3 from = buttonVisual.localPosition;
        Vector3 to = down ? (buttonDefaultLocalPos + Vector3.down * pressDepth) : buttonDefaultLocalPos;
        float elapsed = 0f;
        float duration = 1f / pressSpeed;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            buttonVisual.localPosition = Vector3.Lerp(from, to, Mathf.SmoothStep(0f, 1f, elapsed / duration));
            yield return null;
        }

        buttonVisual.localPosition = to;
    }

    // optional: draw gizmo range
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}
