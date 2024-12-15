using UnityEngine;
using System.Collections;

public class Turnstile : MonoBehaviour
{
    public Transform rotatingPart; // Turnikenin dönen kýsmý
    public GameObject targetObject; // Pozisyon ve rotasyon atanacak hedef GameObject
    public float rotationAngle = 90f; // Döndürülecek açý
    public float rotationSpeed = 10f; // Dönüþ hýzý
    private bool isActivated = false;

    private Quaternion initialRotation; // Baþlangýç rotasyonu
    private Quaternion targetRotation;  // Hedef rotasyonu

    public void ActivateTurnstile()
    {
        if (!isActivated && rotatingPart != null && targetObject != null)
        {
            isActivated = true;

            // 1. Üzerinde olduðu nesnenin pozisyonunu targetObject'in pozisyonuna ata
            transform.position = targetObject.transform.position;

            // 2. Rotasyonu ayarla (x = 0, y = -90, z = 0)
            transform.rotation = Quaternion.Euler(0f, -90f, 0f);

            // 3. Turnikenin dönüþü için baþlangýç ve hedef rotasyonlarý belirle
            initialRotation = rotatingPart.rotation;
            targetRotation = initialRotation * Quaternion.Euler(0, rotationAngle, 0);

            StartCoroutine(RotateTurnstile());
        }
    }

    private IEnumerator RotateTurnstile()
    {
        float elapsedTime = 0f; // Geçen süre

        while (elapsedTime < (rotationAngle / rotationSpeed))
        {
            // Rotasyonu Lerp ile yumuþatýlmýþ bir þekilde döndür
            rotatingPart.rotation = Quaternion.Lerp(initialRotation, targetRotation, elapsedTime / (rotationAngle / rotationSpeed));

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Döndürme iþlemi tamamlandýðýnda hedef rotasyona tam olarak ayarla
        rotatingPart.rotation = targetRotation;

        Debug.Log("Turnike açýldý!");
    }
}
