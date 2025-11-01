
using UnityEngine;
using UnityEngine.UI;

public class PhoneManager : MonoBehaviour
{
    public GameObject phoneUI;
    public Slider batteryBar;
    public float batteryDrainRate = 0.01f;
    public float chargeRate = 0.05f;
    public KeyCode toggleKey = KeyCode.Tab;
    public Transform cameraPivot;
    public Transform charger;
    public float lookThreshold = 15f;

    private float battery = 1f;
    private bool phoneActive = false;
    private bool isCharging = false;

    void Start()
    {
        UpdateBatteryUI();
        phoneUI.SetActive(false);
    }

    void Update()
    {
        HandleToggle();
        CheckIfLookingAtCharger();
        HandleBattery();
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

    void UpdateBatteryUI()
    {
        if (batteryBar != null)
            batteryBar.value = battery;
    }
}
