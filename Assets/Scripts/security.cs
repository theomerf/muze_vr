using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class Security : MonoBehaviour
{
    public Transform playerResetPosition; // Oyuncunun geri ���nlanaca�� konum
    public Transform itemResetPosition; // E�yalar�n geri ���nlanaca�� konum
    public Text warningText; // UI'daki uyar� metnini referansla
    public Image backgroundImage; // Yaz�n�n arkas�ndaki arka plan i�in Image
    public AudioSource securityAudio; // G�venlik sesini �ald�rmak i�in referans
    public Component xrOrigin; // XR Origin nesnesini buraya ataca��z
    public Transform securityCharacter; // G�venlik karakterinin transformu
    public Animator securityAnimator; // G�venlik karakterinin animatoru
    public float teleportDelay = 2f; // G�venlik karakterinin teleport olma s�resi (kick animasyonu sonras�)

    // G�venlik karakterinin ���nlanaca�� pozisyon
    public Transform securityTeleportPosition;

    public float securityTeleportDistance = 1.5f; // G�venlik karakterinin oyuncuya ���nlanma mesafesi
    public Transform cameraOffset; // Kamera ofset nesnesi

    private Vector3 initialItemPosition;  // E�yan�n ba�lang�� pozisyonu
    private Quaternion initialItemRotation; // E�yalar�n ba�lang�� rotas�
    private XRGrabInteractable grabInteractable;

    private float grabTime = 0; // E�yay� ne kadar s�reyle tutuyor
    private bool isWarningShown = false; // Uyar� g�sterilip g�sterilmedi�ini kontrol et
    private bool isPlayerKicked = false; // Oyuncu at�ld� m�?
    private bool isSecurityNear = false; // G�venlik karakteri oyuncunun yan�nda m�?
    private bool isSecurityKickReady = false; // G�venlik tekmeye haz�r m�?

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }

        // E�yan�n ba�lang�� pozisyonunu kaydet
        initialItemPosition = transform.position;
        initialItemRotation = transform.rotation;

        // Ba�lang��ta uyar� text'ini ve arka plan� gizle
        if (warningText != null)
        {
            warningText.gameObject.SetActive(false);
        }
        if (backgroundImage != null)
        {
            backgroundImage.gameObject.SetActive(false);
        }
    }

    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        grabTime = Time.time;
        isWarningShown = false;
        isPlayerKicked = false;
        isSecurityNear = false; // G�venlik karakteri yak�n de�il
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        // E�yay� ba�lang�� pozisyonuna d�nd�r
        transform.position = initialItemPosition;
        transform.rotation = initialItemRotation;

        grabTime = 0;
        isWarningShown = false;
        isSecurityNear = false;
    }

    private void Update()
    {
        if (grabTime > 0 && !isPlayerKicked)
        {
            float heldTime = Time.time - grabTime;
            if (heldTime > 9)
            {
                securityAnimator.SetBool("isSitting", false);
                securityAnimator.SetBool("isIdle", true);
            }
            if (heldTime > 10 && !isWarningShown)
            {
                // Uyar� g�ster
                ShowWarning("L�tfen e�yay� yerine koyun!", Color.yellow);
                isWarningShown = true;

                // G�venlik karakterini hemen ���nla
                if (!isSecurityNear)
                {
                    TeleportSecurityToPlayer();
                }
            }
            else if (heldTime > 20)
            {
                // G�venlik karakterini yak�nla�t�r ve tekme animasyonunu ba�lat
                if (!isSecurityNear)
                {
                    TeleportSecurityToPlayer();
                }

                // Tekme animasyonuna 3 saniye bekleyerek ba�la
                if (isSecurityNear && !isSecurityKickReady)
                {
                    Invoke("StartKickAnimation", 0); // 3 saniye sonra tekme animasyonunu ba�lat
                    isSecurityKickReady = true;
                }

                // Oyuncuyu m�zeden at
                KickPlayer();
            }
        }
    }

    private void ShowWarning(string message, Color textColor)
    {
        if (warningText != null && backgroundImage != null)
        {
            warningText.text = message;
            warningText.color = textColor; // Yaz�n�n rengini de�i�tir
            warningText.gameObject.SetActive(true); // Text nesnesini g�r�n�r yap

            backgroundImage.gameObject.SetActive(true); // Arka plan� g�r�n�r yap
            backgroundImage.color = Color.black; // Arka plan rengini ayarla (iste�e ba�l�)

            // Arka plan� yaz�ya g�re biraz geni�let
            RectTransform rt = backgroundImage.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(warningText.preferredWidth + 20, warningText.preferredHeight + 10); // Padding eklemek i�in  

            Invoke("ClearWarning", 5f); // Mesaj� 5 saniye sonra temizle
        }
    }

    private void ClearWarning()
    {
        if (warningText != null && backgroundImage != null)
        {
            warningText.gameObject.SetActive(false); // Text nesnesini gizle
            backgroundImage.gameObject.SetActive(false); // Arka plan� gizle
        }
    }

    private void TeleportSecurityToPlayer()
    {
        isSecurityNear = true;

        // Kamera ofseti kullanarak g�venlik karakterini oyuncunun yan�na ���nla
        if (securityCharacter != null && xrOrigin != null && cameraOffset != null)
        {
            Transform xrOriginTransform = xrOrigin as Transform; // xrOrigin'i Transform'a cast et

            if (xrOriginTransform != null)
            {
                // Kamera y�n�ne bakarak g�venlik karakterini biraz mesafeye ���nla
                Vector3 forwardDirection = cameraOffset.forward.normalized; // Kamera ofsetinin y�n� (normalize edilmi�)
                Vector3 spawnPosition = cameraOffset.position + forwardDirection * securityTeleportDistance;

                // Y pozisyonunu sabit tutal�m
                spawnPosition.y = securityCharacter.position.y; // G�venlik karakterinin mevcut y�ksekli�i

                securityCharacter.position = spawnPosition;

                // G�venlik karakterini kameran�n tam tersine d�nd�rmek i�in
                Vector3 directionToPlayer = securityCharacter.position - xrOriginTransform.position; // G�venlik karakteri ile oyuncu aras�ndaki y�n
                directionToPlayer.y = 0; // Y eksenindeki fark� yok say

                if (directionToPlayer != Vector3.zero)
                {
                    // G�venlik karakterini kameraya bakacak �ekilde d�nd�r, ancak 180 derece d�nd�r (yani size bakacak)
                    securityCharacter.rotation = Quaternion.LookRotation(-directionToPlayer); // Y�n�n tersini alarak g�venlik karakterini size y�nlendiriyoruz
                }

                // G�venlik sesi
                if (securityAudio != null)
                {
                    securityAudio.gameObject.SetActive(true); // G�venlik sesini �al
                    Invoke("SoundOff", 3);
                }
            }
        }
    }



    private void SoundOff()
    {
        securityAudio.gameObject.SetActive(false);
    }


    private void StartKickAnimation()
    {
        // G�venlik tekme animasyonunu ba�lat
        if (securityAnimator != null)
        {
            securityAnimator.SetBool("isIdle", false);
            securityAnimator.SetBool("isKicking", true);
        }
    }

    private void KickPlayer()
    {
        isPlayerKicked = true;

        // Animasyonu tamamlayana kadar bekle (kick animasyon s�resi)
        if (securityAnimator != null)
        {
            // Tekme animasyonu bitti�inde oyuncuyu ���nla
            Invoke("TeleportPlayerAfterKick", teleportDelay); // Animasyon sonras� oyuncuyu ���nla
        }
    }

    private void TeleportPlayerAfterKick()
    {
        // Oyuncuyu XR Origin'in belirlenen reset konumuna ���nla
        if (xrOrigin != null && playerResetPosition != null)
        {
            xrOrigin.transform.position = playerResetPosition.position;
            xrOrigin.transform.rotation = playerResetPosition.rotation;
        }

        // E�yay� ba�lang�� pozisyonuna ���nla
        if (itemResetPosition != null)
        {
            transform.position = itemResetPosition.position;
            transform.rotation = itemResetPosition.rotation;
        }

        // Uyar�y� g�ster ve oyuncuyu m�zeden at
        ShowWarning("E�yay� yerine koymad���n�z i�in m�zeden ��kar�ld�n�z!", Color.red);

        // Tekme animasyonunu bitir ve g�venlik karakterini eski konumuna geri al
        if (securityAnimator != null)
        {
            securityAnimator.SetBool("isKicking", false);
            securityAnimator.SetBool("isIdle", true);
        }

        // G�venlik karakterini eski konumuna geri al
        if (securityTeleportPosition != null)
        {
            securityCharacter.position = securityTeleportPosition.position; // G�venlik karakterini yeni bir konuma ���nla
        }
    }
}
