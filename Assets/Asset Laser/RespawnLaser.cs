using UnityEngine;
using System.Collections;

public class RespawnLaser : MonoBehaviour
{
    public static RespawnLaser instance;

    [Header("--- SETTING ---")]
    public Transform currentCheckpoint; 
    
    [Header("--- VISUAL ---")]
    public CanvasGroup blackScreen; // Drag Panel hitam yg ada CanvasGroup-nya kesini
    public float fadeDuration = 0.5f; // Kecepatan layar gelap
    public float stayBlackDuration = 0.5f; // Berapa lama layar gelap total

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        // Pastikan layar bening pas mulai
        if (blackScreen != null) 
        {
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }
    }

    public void KillAndRespawn(GameObject player)
    {
        StartCoroutine(RespawnRoutine(player));
    }

    IEnumerator RespawnRoutine(GameObject player)
    {
        // 1. FADE IN (Layar jadi Gelap)
        if (blackScreen != null)
        {
            blackScreen.gameObject.SetActive(true);
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                blackScreen.alpha = t;
                yield return null;
            }
            blackScreen.alpha = 1; // Pastikan gelap total
        }

        // 2. PROSES RESPAWN (Saat layar gelap gulita)
        // Kita matikan player sebentar biar reset fisika/animasi
        player.SetActive(false);
        
        yield return new WaitForSeconds(stayBlackDuration);

        if (currentCheckpoint != null)
        {
            // Pindahkan posisi
            player.transform.position = currentCheckpoint.position;
            // Reset rotasi (menghadap arah checkpoint)
            player.transform.rotation = currentCheckpoint.rotation; 
        }

        // Hidupkan player lagi
        player.SetActive(true);

        // 3. FADE OUT (Layar jadi Terang lagi)
        if (blackScreen != null)
        {
            float t = 0;
            while (t < 1f)
            {
                t += Time.deltaTime / fadeDuration;
                blackScreen.alpha = 1f - t; // Kebalikan (1 ke 0)
                yield return null;
            }
            blackScreen.alpha = 0;
            blackScreen.gameObject.SetActive(false);
        }
    }
}