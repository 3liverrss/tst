using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{
    public static event System.Action OnFlash;
    public static event System.Action OnPhoto;
    public static event System.Action OnSiren;

    public GameObject phoneUI;
    public Slider batteryBar;
    public Button flashButton;
    public Button photoButton;
    public Button sirenButton;

    public float batteryDrainRate = 0.01f;
    public float chargeRate = 0.05f;

    public Light flashLight;
    public float flashIntensity = 12f;
    public float flashDuration = 0.08f;
    public float flashBatteryCost = 0.02f;

    public float photoBatteryCost = 0.03f;

    public AudioSource sirenSource;
    public AudioClip sirenClip;
    public float sirenBatteryCost = 0.05f;
    public float sirenCooldown = 10f;

    public KeyCode toggleKey = KeyCode.Tab;
    public Transform cameraPivot;
    public Transform charger;
    public float lookThreshold = 15f;

    private float battery = 1f;
    private float sirenTimer = 0f;
    private bool phoneActive = false;
    private bool isCharging = false;
    private TemperatureManager temperatureManager;

    void Start()
    {
        temperatureManager = FindFirstObjectByType<TemperatureManager>();

        if (flashButton != null)
            flashButton.onClick.AddListener(TriggerFlash);

        if (photoButton != null)
            photoButton.onClick.AddListener(TriggerPhoto);

        if (sirenButton != null)
            sirenButton.onClick.AddListener(TriggerSiren);

        UpdateBatteryUI();
        phoneUI.SetActive(false);

        if (flashLight != null)
            flashLight.intensity = 0f;
    }

    void Update()
    {
        if (temperatureManager != null && temperatureManager.isFrozen)
        {
            phoneActive = false;
            phoneUI.SetActive(false);

            if (flashLight != null)
                flashLight.intensity = 0f;

            return;
        }

        if (sirenTimer > 0f)
            sirenTimer -= Time.deltaTime;

        HandleToggle();
        CheckIfLookingAtCharger();
        HandleBattery();
        UpdateButtons();
    }

    void HandleToggle()
    {
        if (Input.GetKeyDown(toggleKey) && battery > 0)
        {
            phoneActive = !phoneActive;
            phoneUI.SetActive(phoneActive);
        }
    }

    void CheckIfLookingAtCharger()
    {
        if (cameraPivot == null || charger == null) return;

        Vector3 dirToCharger = (charger.position - cameraPivot.position).normalized;
        float angle = Vector3.Angle(cameraPivot.forward, dirToCharger);
        isCharging = angle < lookThreshold;
    }

    void HandleBattery()
    {
        if (!isCharging)
        {
            if (phoneActive)
                battery -= batteryDrainRate * Time.deltaTime;
        }
        else
        {
            battery += chargeRate * Time.deltaTime;
        }

        battery = Mathf.Clamp01(battery);
        UpdateBatteryUI();

        if (battery <= 0 && phoneActive)
        {
            phoneActive = false;
            phoneUI.SetActive(false);
        }
    }

    void UpdateButtons()
    {
        bool hasBattery = battery > 0f;

        if (flashButton != null)
            flashButton.interactable = hasBattery;

        if (photoButton != null)
            photoButton.interactable = hasBattery;

        if (sirenButton != null)
            sirenButton.interactable = hasBattery && sirenTimer <= 0f;
    }

    public void TriggerFlash()
    {
        if (!phoneActive || battery <= 0f)
            return;

        if (flashLight != null)
        {
            flashLight.intensity = flashIntensity;
            CancelInvoke(nameof(ResetFlash));
            Invoke(nameof(ResetFlash), flashDuration);
        }

        UseBattery(flashBatteryCost);

        OnFlash?.Invoke();
    }

    public void TriggerPhoto()
    {
        if (!phoneActive || battery <= 0f)
            return;

        UseBattery(photoBatteryCost);

        OnPhoto?.Invoke();
    }

    public void TriggerSiren()
    {
        if (!phoneActive || battery <= 0f || sirenTimer > 0f)
            return;

        if (sirenSource != null && sirenClip != null)
            sirenSource.PlayOneShot(sirenClip);

        sirenTimer = sirenCooldown;
        UseBattery(sirenBatteryCost);

        OnSiren?.Invoke();
    }

    void UseBattery(float amount)
    {
        battery -= amount;
        battery = Mathf.Clamp01(battery);
        UpdateBatteryUI();
    }

    void ResetFlash()
    {
        if (flashLight != null)
            flashLight.intensity = 0f;
    }

    void UpdateBatteryUI()
    {
        if (batteryBar != null)
            batteryBar.value = battery;
    }
}