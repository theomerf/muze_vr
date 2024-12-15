using UnityEngine;

public class ElevatorMove : MonoBehaviour
{
    public Transform elevator; // Asansör objesi
    public Transform targetObject; // Hedef pozisyon olarak atanacak obje
    public Transform player; // Oyuncu veya VR kamera
    public float moveSpeed = 2f; // Asansör hareket hýzý
    public Animator doorAnimator; // Kapý animatörü
    public string doorOpenTrigger = "Open"; // Animatördeki tetikleyici

    private bool isMoving = false;

    void Update()
    {
        if (isMoving && targetObject != null)
        {
            // Hedef pozisyonun yüksekliðine doðru hareket et
            Vector3 targetPosition = new Vector3(
                elevator.position.x,
                targetObject.position.y,
                elevator.position.z
            );

            // Asansörü hareket ettir
            elevator.position = Vector3.MoveTowards(
                elevator.position,
                targetPosition,
                moveSpeed * Time.deltaTime
            );

            // Hedefe ulaþtýðýnda iþlemi tamamla
            if (Vector3.Distance(elevator.position, targetPosition) < 0.01f)
            {
                isMoving = false;
                TriggerDoorOpen();
                DetachPlayer();
            }
        }
    }

    public void StartMove()
    {
        if (!isMoving)
        {
            isMoving = true;

            // Oyuncuyu asansörün child'ý yap
            if (player != null)
            {
                player.SetParent(elevator);
            }
            else
            {
                Debug.LogWarning("Oyuncu atanmadý!");
            }
        }
    }

    void TriggerDoorOpen()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetTrigger(doorOpenTrigger); // Kapýyý açan animasyon tetikleyici
        }
        else
        {
            Debug.LogWarning("Kapý animatörü atanmadý!");
        }
    }

    void DetachPlayer()
    {
        // Oyuncuyu asansörden ayýr
        if (player != null)
        {
            player.SetParent(null);
        }
    }
}
