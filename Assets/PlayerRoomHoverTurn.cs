using UnityEngine;
using UnityEngine.UI;

public class PlayerRoomHoverTurn : MonoBehaviour
{
    public Transform cameraPivot;
    public float turnSpeed = 5f;
    public float arriveThreshold = 1f;

    public Image arrowLeft;
    public Image arrowRight;
    public Image arrowBack;

    private TemperatureManager temperatureManager;

    private int targetAngle = 0;
    private bool hasProcessedThisHover = false;

    void Start()
    {
        temperatureManager = FindFirstObjectByType<TemperatureManager>();

        targetAngle = 0;
        UpdateArrows();
    }

    void Update()
    {
        if (temperatureManager != null && temperatureManager.isFrozen)
        {
            arrowLeft.gameObject.SetActive(false);
            arrowRight.gameObject.SetActive(false);
            arrowBack.gameObject.SetActive(false);
            return;
        }

        HandleHoverInput();
        RotateTowardsTarget();
        UpdateArrowHighlights();
        SnapIfArrived();
    }

    void HandleHoverInput()
    {
        Vector3 mousePos = Input.mousePosition;

        bool hoveredLeft = arrowLeft.gameObject.activeSelf && mousePos.x < Screen.width * 0.1f;
        bool hoveredRight = arrowRight.gameObject.activeSelf && mousePos.x > Screen.width * 0.9f;
        bool hoveredBack = arrowBack.gameObject.activeSelf && mousePos.y < Screen.height * 0.1f;

        bool facingBehind = targetAngle == 180;

        int desired = targetAngle;

        if (hoveredLeft)
            desired = facingBehind ? 90 : -90;
        else if (hoveredRight)
            desired = facingBehind ? -90 : 90;
        else if (hoveredBack)
            desired = facingBehind ? 0 : 180;
        else
        {
            hasProcessedThisHover = false;
            return;
        }

        if (hasProcessedThisHover)
            return;

        float diff = Mathf.Abs(Mathf.DeltaAngle(targetAngle, desired));

        int newTarget = desired;

        if (diff > 179f && Mathf.Abs(targetAngle) == 90)
            newTarget = 0;

        if (newTarget != targetAngle)
        {
            targetAngle = newTarget;
            UpdateArrows();
        }

        hasProcessedThisHover = true;
    }

    void RotateTowardsTarget()
    {
        Quaternion tgt = Quaternion.Euler(0, targetAngle, 0);

        cameraPivot.localRotation = Quaternion.Lerp(
            cameraPivot.localRotation,
            tgt,
            Time.deltaTime * turnSpeed
        );
    }

    void SnapIfArrived()
    {
        Quaternion tgt = Quaternion.Euler(0, targetAngle, 0);

        if (Quaternion.Angle(cameraPivot.localRotation, tgt) <= arriveThreshold)
            cameraPivot.localRotation = tgt;
    }

    void UpdateArrowHighlights()
    {
        Vector3 mousePos = Input.mousePosition;

        arrowLeft.color =
            (arrowLeft.gameObject.activeSelf && mousePos.x < Screen.width * 0.1f)
            ? Color.white
            : new Color(1, 1, 1, 0.3f);

        arrowRight.color =
            (arrowRight.gameObject.activeSelf && mousePos.x > Screen.width * 0.9f)
            ? Color.white
            : new Color(1, 1, 1, 0.3f);

        arrowBack.color =
            (arrowBack.gameObject.activeSelf && mousePos.y < Screen.height * 0.1f)
            ? Color.white
            : new Color(1, 1, 1, 0.3f);
    }

    public void UpdateArrows()
    {
        arrowLeft.gameObject.SetActive(targetAngle != -90);
        arrowRight.gameObject.SetActive(targetAngle != 90);
        arrowBack.gameObject.SetActive(true);
    }
}