using UnityEngine;

public class SceneMechanicManager : MonoBehaviour
{
    [Header("--- CHARACTER PARENTS ---")]
    public GameObject remiObject; // Objek Remi di Hierarchy
    public GameObject domiObject; // Objek Domi di Hierarchy

    private void Start()
    {
        // Beri jeda sedikit agar Switch Character selesai loading duluan
        Invoke("ApplyMechanicsOnce", 0.2f);
    }

    void ApplyMechanicsOnce()
    {
        // PROSES REMI
        if (remiObject != null)
        {
            // Cari komponen secara spesifik agar tidak salah ambil script Move
            var sLine = remiObject.GetComponentInChildren<StringLineAttack>();
            var sBlow = remiObject.GetComponentInChildren<SuckBlow>();

            if (sLine) sLine.enabled = false; // Matikan String
            if (sBlow) sBlow.enabled = true;  // Nyalakan Suck Blow
        }

        // PROSES DOMI
        if (domiObject != null)
        {
            var sLine = domiObject.GetComponentInChildren<StringLineAttack>();
            var sBlow = domiObject.GetComponentInChildren<SuckBlow>();

            if (sLine) sLine.enabled = false; // Matikan String
            if (sBlow) sBlow.enabled = true;  // Nyalakan Suck Blow
        }
        
        Debug.Log("<color=green>[Mechanic]</color> Konfigurasi Senjata Berhasil & Dikunci.");
        
        // Hancurkan script ini setelah tugas selesai agar tidak membebani Update
        Destroy(this); 
    }
}