using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TicketMachine : MonoBehaviour
{
    public GameObject ticketPrefab; // Üretilecek biletin prefab'ý
    public Transform ticketSpawnPoint; // Biletin çýkacaðý yer
    public AudioSource printSound; // Bilet çýkarma sesi

    public void GenerateTicket()
    {
        if (ticketPrefab != null && ticketSpawnPoint != null)
        {
            // Bileti oluþtur
            GameObject spawnedTicket = Instantiate(ticketPrefab, ticketSpawnPoint.position, Quaternion.Euler(90, 90, 0));

            // Tag'i ayarla
            spawnedTicket.tag = "Ticket";

            // Ölçeði ayarla
            spawnedTicket.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);

            // Rigidbody ekle ve kinematik olarak baþlat
            Rigidbody rb = spawnedTicket.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = spawnedTicket.AddComponent<Rigidbody>();
            }
            rb.isKinematic = true; // Fizik etkilerini kapat

            // Collider ekle (eðer yoksa)
            if (spawnedTicket.GetComponent<Collider>() == null)
            {
                BoxCollider collider = spawnedTicket.AddComponent<BoxCollider>();
                collider.size = Vector3.one * 0.1f;
                collider.isTrigger = false;
            }

            // XRGrabInteractable ekle
            XRGrabInteractable grabInteractable = spawnedTicket.GetComponent<XRGrabInteractable>();
            if (grabInteractable == null)
            {
                grabInteractable = spawnedTicket.AddComponent<XRGrabInteractable>();
            }

            // Grab baþladýðýnda Rigidbody'yi kinematikten çýkar
            grabInteractable.selectEntered.AddListener((interactor) =>
            {
                rb.isKinematic = false; // Fizik etkilerini aç
            });

            // Ses çal
            if (printSound != null)
            {
                printSound.Play();
            }

            Debug.Log("Bilet üretildi, tag atandý ve XRGrabInteractable ile hazýr!");
        }
    }
}
