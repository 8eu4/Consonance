using UnityEngine;
using System.Collections;

public class ConductorHP : Health
{
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent<EnemyAttack>(out var attack))
        {
            TakeDamage(attack.damage);
            Destroy(collision.gameObject);
        }
    }

    protected override void Die()
    {
        StartCoroutine(DeathSequence());
    }

    IEnumerator DeathSequence()
    {
        Debug.Log("Conductor Mati. Menunggu respawn...");

        // Matikan visual
        Renderer[] renders = GetComponentsInChildren<Renderer>();
        foreach (var r in renders) r.enabled = false;

        // Matikan gerak
        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        var rb = GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        yield return new WaitForSeconds(2.0f);

        // Nyalakan visual kembali (penting sebelum respawn)
        foreach (var r in renders) r.enabled = true;

        Debug.Log("Memanggil Respawn Manager...");

        base.Die();

        var identity = GetComponent<PlayerIdentity>();
        if (identity != null)
        {
            identity.Die();
        }
        else
        {
            Debug.LogError("PlayerIdentity tidak ditemukan di Conductor!");
        }
    }
}