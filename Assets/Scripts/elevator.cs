using UnityEngine;

public class ElevatorButton : MonoBehaviour
{
    // Asansörün Animator'ünü referans alýyoruz
    public Animator elevatorAnimator;
    public string animationTrigger = "open";  // Animator trigger parametre ismi

    // Kapý animasyonunu tetiklemek için fonksiyon
    public void OpenElevatorDoors()
    {
        // Animator'dan trigger'ý tetikle
        if (elevatorAnimator != null)
        {
            elevatorAnimator.SetTrigger(animationTrigger);  // Trigger'ý çalýþtýr
            Debug.Log("Asansör kapýlarý açýlýyor!");
        }
        else
        {
            Debug.LogError("Animator referansý eksik!");
        }
    }
}
