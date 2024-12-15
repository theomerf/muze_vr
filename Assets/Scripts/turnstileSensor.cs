using UnityEngine;

public class TurnstileSensor : MonoBehaviour
{
    public Turnstile turnstileScript;
    public Turnstile turnstileScript2;// Turnike dönüþünü kontrol eden script
    public string validTicketTag = "Ticket"; // Doðru biletin tag'i

    private void OnTriggerEnter(Collider other)
    {
        // Eðer nesne doðru tag'e sahipse (bilet okutuluyorsa)
        if (other.CompareTag(validTicketTag))
        {
            Debug.Log("Bilet okundu, turnike açýlýyor!");

            // Turnikeyi döndür
            if (turnstileScript != null)
            {
                turnstileScript.ActivateTurnstile();
                turnstileScript2.ActivateTurnstile();
            }

            // Opsiyonel: Bileti yok et veya kullanýlmýþ hale getir
            Destroy(other.gameObject);
        }
    }
}
