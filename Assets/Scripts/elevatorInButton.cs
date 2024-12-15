using UnityEngine;

public class ElevatorInButton : MonoBehaviour
{
    // Asansörün GameObject'i
    public GameObject elevatorObject;  // Asansörün GameObject'ini buraya atayacaðýz
    // Oyuncu veya VR kamera GameObject'i
    public GameObject player;  // Oyuncu veya VR Kamera GameObject'i

    public void OnButtonPressed()
    {
        // ElevatorObject'in üzerindeki ElevatorMove component'ini al
        ElevatorMove elevatorMove = elevatorObject.GetComponent<ElevatorMove>();

        if (elevatorMove != null)
        {
            elevatorMove.StartMove();  // Asansörün hareket etmesini baþlat
            Debug.Log("Butona basýldý, asansör hareket etmeye baþladý.");
        }
        else
        {
            Debug.LogError("ElevatorMove script'i bulunamadý!");
        }
    }
}
