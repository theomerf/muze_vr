using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.UI;

public class Security : MonoBehaviour
{
    public Transform playerResetPosition; // Oyuncunun geri ýþýnlanacaðý konum
    public Transform itemResetPosition; // Eþyalarýn geri ýþýnlanacaðý konum
    public Text warningText; // UI'daki uyarý metnini referansla
    public Image backgroundImage; // Yazýnýn arkasýndaki arka plan için Image
    public AudioSource securityAudio; // Güvenlik sesini çaldýrmak için referans
    public Component xrOrigin; // XR Origin nesnesini buraya atacaðýz
    public Transform securityCharacter; // Güvenlik karakterinin transformu
    public Animator securityAnimator; // Güvenlik karakterinin animatoru
    public float teleportDelay = 2f; // Güvenlik karakterinin teleport olma süresi (kick animasyonu sonrasý)

    // Güvenlik karakterinin ýþýnlanacaðý pozisyon
    public Transform securityTeleportPosition;

    public float securityTeleportDistance = 1.5f; // Güvenlik karakterinin oyuncuya ýþýnlanma mesafesi
    public Transform cameraOffset; // Kamera ofset nesnesi

    private Vector3 initialItemPosition;  // Eþyanýn baþlangýç pozisyonu
    private Quaternion initialItemRotation; // Eþyalarýn baþlangýç rotasý
    private XRGrabInteractable grabInteractable;

    private float grabTime = 0; // Eþyayý ne kadar süreyle tutuyor
    private bool isWarningShown = false; // Uyarý gösterilip gösterilmediðini kontrol et
    private bool isPlayerKicked = false; // Oyuncu atýldý mý?
    private bool isSecurityNear = false; // Güvenlik karakteri oyuncunun yanýnda mý?
    private bool isSecurityKickReady = false; // Güvenlik tekmeye hazýr mý?

    private void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.AddListener(OnSelectEnter);
            grabInteractable.selectExited.AddListener(OnSelectExit);
        }

        // Eþyanýn baþlangýç pozisyonunu kaydet
        initialItemPosition = transform.position;
        initialItemRotation = transform.rotation;

        // Baþlangýçta uyarý text'ini ve arka planý gizle
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
        isSecurityNear = false; // Güvenlik karakteri yakýn deðil
    }

    private void OnSelectExit(SelectExitEventArgs args)
    {
        // Eþyayý baþlangýç pozisyonuna döndür
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
                // Uyarý göster
                ShowWarning("Lütfen eþyayý yerine koyun!", Color.yellow);
                isWarningShown = true;

                // Güvenlik karakterini hemen ýþýnla
                if (!isSecurityNear)
                {
                    TeleportSecurityToPlayer();
                }
            }
            else if (heldTime > 20)
            {
                // Güvenlik karakterini yakýnlaþtýr ve tekme animasyonunu baþlat
                if (!isSecurityNear)
                {
                    TeleportSecurityToPlayer();
                }

                // Tekme animasyonuna 3 saniye bekleyerek baþla
                if (isSecurityNear && !isSecurityKickReady)
                {
                    Invoke("StartKickAnimation", 0); // 3 saniye sonra tekme animasyonunu baþlat
                    isSecurityKickReady = true;
                }

                // Oyuncuyu müzeden at
                KickPlayer();
            }
        }
    }

    private void ShowWarning(string message, Color textColor)
    {
        if (warningText != null && backgroundImage != null)
        {
            warningText.text = message;
            warningText.color = textColor; // Yazýnýn rengini deðiþtir
            warningText.gameObject.SetActive(true); // Text nesnesini görünür yap

            backgroundImage.gameObject.SetActive(true); // Arka planý görünür yap
            backgroundImage.color = Color.black; // Arka plan rengini ayarla (isteðe baðlý)

            // Arka planý yazýya göre biraz geniþlet
            RectTransform rt = backgroundImage.GetComponent<RectTransform>();
            rt.sizeDelta = new Vector2(warningText.preferredWidth + 20, warningText.preferredHeight + 10); // Padding eklemek için  

            Invoke("ClearWarning", 5f); // Mesajý 5 saniye sonra temizle
        }
    }

    private void ClearWarning()
    {
        if (warningText != null && backgroundImage != null)
        {
            warningText.gameObject.SetActive(false); // Text nesnesini gizle
            backgroundImage.gameObject.SetActive(false); // Arka planý gizle
        }
    }

    private void TeleportSecurityToPlayer()
    {
        isSecurityNear = true;

        // Kamera ofseti kullanarak güvenlik karakterini oyuncunun yanýna ýþýnla
        if (securityCharacter != null && xrOrigin != null && cameraOffset != null)
        {
            Transform xrOriginTransform = xrOrigin as Transform; // xrOrigin'i Transform'a cast et

            if (xrOriginTransform != null)
            {
                // Kamera yönüne bakarak güvenlik karakterini biraz mesafeye ýþýnla
                Vector3 forwardDirection = cameraOffset.forward; // Kamera ofsetinin yönü
                Vector3 spawnPosition = cameraOffset.position + forwardDirection * securityTeleportDistance;

                // Y pozisyonunu sabit tutalým (current y position of security character)
                spawnPosition.y = securityCharacter.position.y; // Y koordinatýný sabit tut

                securityCharacter.position = spawnPosition;

                // Güvenlik karakterinin yüzünü kameraya doðru çevirelim (ters deðil)
                // Kameranýn yönünü doðru bir þekilde hesaplayalým
                Vector3 directionToFace = xrOriginTransform.position - securityCharacter.position;

                // Y eksenindeki farký sýfýrlayarak sadece X ve Z yönünde döndürme yapýyoruz
                directionToFace.y = 0;

                // Karakterin yönünü oyuncuya doðru döndürüyoruz
                securityCharacter.rotation = Quaternion.LookRotation(directionToFace);

                // Eðer güvenlik karakterinin yönü yine doðru olmayacaksa, alternatif olarak:
                // Güvenlik karakterinin kamera ofseti ile olan pozisyonunu da kontrol edebiliriz
                // Bu sayede güvenlik her zaman doðru yöne bakacaktýr.
                if (securityCharacter.rotation != Quaternion.LookRotation(directionToFace))
                {
                    securityCharacter.rotation = Quaternion.LookRotation(directionToFace);
                }

                // Güvenlik sesi
                if (securityAudio != null)
                {
                    securityAudio.gameObject.SetActive(true); // Güvenlik sesini çal
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
        // Güvenlik tekme animasyonunu baþlat
        if (securityAnimator != null)
        {
            securityAnimator.SetBool("isIdle", false);
            securityAnimator.SetBool("isKicking", true);
        }
    }

    private void KickPlayer()
    {
        isPlayerKicked = true;

        // Animasyonu tamamlayana kadar bekle (kick animasyon süresi)
        if (securityAnimator != null)
        {
            // Tekme animasyonu bittiðinde oyuncuyu ýþýnla
            Invoke("TeleportPlayerAfterKick", teleportDelay); // Animasyon sonrasý oyuncuyu ýþýnla
        }
    }

    private void TeleportPlayerAfterKick()
    {
        // Oyuncuyu XR Origin'in belirlenen reset konumuna ýþýnla
        if (xrOrigin != null && playerResetPosition != null)
        {
            xrOrigin.transform.position = playerResetPosition.position;
            xrOrigin.transform.rotation = playerResetPosition.rotation;
        }

        // Eþyayý baþlangýç pozisyonuna ýþýnla
        if (itemResetPosition != null)
        {
            transform.position = itemResetPosition.position;
            transform.rotation = itemResetPosition.rotation;
        }

        // Uyarýyý göster ve oyuncuyu müzeden at
        ShowWarning("Eþyayý yerine koymadýðýnýz için müzeden çýkarýldýnýz!", Color.red);

        // Tekme animasyonunu bitir ve güvenlik karakterini eski konumuna geri al
        if (securityAnimator != null)
        {
            securityAnimator.SetBool("isKicking", false);
            securityAnimator.SetBool("isIdle", true);
        }

        // Güvenlik karakterini eski konumuna geri al
        if (securityTeleportPosition != null)
        {
            securityCharacter.position = securityTeleportPosition.position; // Güvenlik karakterini yeni bir konuma ýþýnla
        }
    }
}
