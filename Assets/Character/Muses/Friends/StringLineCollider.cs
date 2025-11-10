using UnityEngine;

// Pastikan GameObject ini punya Rigidbody dan BoxCollider
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(BoxCollider))]
public class StringLineCollider : MonoBehaviour
{
    // Objek yang kita tembak/tempel
    private GameObject attachedTarget;

    // Script StringLineAttack yang memiliki collider ini
    private StringLineAttack ownerScript;

    /// <summary>
    /// Dipanggil oleh StringLineAttack untuk memberi tahu siapa pemilik
    /// dan siapa targetnya.
    /// </summary>
    public void Setup(GameObject target, StringLineAttack owner)
    {
        this.attachedTarget = target;
        this.ownerScript = owner;
    }

    // Ini adalah fungsi utamanya
    void OnTriggerEnter(Collider other)
    {
        // 1. Jika kita menabrak objek yang sedang kita tempel, abaikan.
        if (other.gameObject == attachedTarget)
        {
            return;
        }

        // 2. Jika kita menabrak si penembak (player), abaikan.
        if (ownerScript != null && other.gameObject == ownerScript.gameObject)
        {
            return;
        }

        // 3. Jika kita menabrak sesuatu yang lain (orang, tembok, peluru, dll)
        Debug.Log("Line collision detected with: " + other.name);

        // --- Di sini kamu bisa tambahkan logika ---
        // Contoh: Hancurkan garisnya jika kena sesuatu
        if (ownerScript != null)
        {
            // Panggil CancelAttack() di script utama
            ownerScript.CancelAttack();
        }
    }
}