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

    private int currentAngle = 0;
    private int targetAngle = 0;

    private int pendingAngle = 0;
    private bool hasProcessedThisHover = false;

    void Start()
    {
        targetAngle = currentAngle;
        UpdateArrows();
    }

    void Update()
    {
        HandleHoverInput();
        RotateTowardsTarget();
        UpdateArrowHighlights();
        CheckArrivalAndUpdateState();
    }

    void HandleHoverInput()
    {
        Vector3 mousePos = Input.mousePosition;

        bool hoveredLeft = arrowLeft.gameObject.activeSelf && mousePos.x < Screen.width * 0.1f;
        bool hoveredRight = arrowRight.gameObject.activeSelf && mousePos.x > Screen.width * 0.9f;
        bool hoveredBack = arrowBack.gameObject.activeSelf && mousePos.y < Screen.height * 0.1f;

        int desired = currentAngle;

        if (hoveredLeft) desired = -90;
        else if (hoveredRight) desired = 90;
        else if (hoveredBack) desired = 180;
        else
        {
            hasProcessedThisHover = false;
            return;
        }

        if (hasProcessedThisHover) return;

        float diff = Mathf.Abs(Mathf.DeltaAngle(currentAngle, desired));
        if (diff == 180f)
        {
            targetAngle = 0;
            pendingAngle = 0;
        }
        else
        {
            targetAngle = desired;
        }

        hasProcessedThisHover = true;
    }

    void RotateTowardsTarget()
    {
        Quaternion tgt = Quaternion.Euler(0, targetAngle, 0);
        cameraPivot.localRotation = Quaternion.Lerp(cameraPivot.localRotation, tgt, Time.deltaTime * turnSpeed);
    }

    void CheckArrivalAndUpdateState()
    {
        float angleDiff = Quaternion.Angle(cameraPivot.localRotation, Quaternion.Euler(0, targetAngle, 0));
        if (angleDiff <= arriveThreshold)
        {
            cameraPivot.localRotation = Quaternion.Euler(0, targetAngle, 0);
            if (currentAngle != targetAngle)
            {
                currentAngle = targetAngle;
                UpdateArrows();
            }
        }
    }

    void UpdateArrowHighlights()
    {
        Vector3 mousePos = Input.mousePosition;

        arrowLeft.color = (arrowLeft.gameObject.activeSelf && mousePos.x < Screen.width * 0.1f) ? Color.white : new Color(1, 1, 1, 0.3f);
        arrowRight.color = (arrowRight.gameObject.activeSelf && mousePos.x > Screen.width * 0.9f) ? Color.white : new Color(1, 1, 1, 0.3f);
        arrowBack.color = (arrowBack.gameObject.activeSelf && mousePos.y < Screen.height * 0.1f) ? Color.white : new Color(1, 1, 1, 0.3f);
    }

    public void UpdateArrows()
    {
        arrowLeft.gameObject.SetActive(currentAngle != -90);
        arrowRight.gameObject.SetActive(currentAngle != 90);
        arrowBack.gameObject.SetActive(currentAngle != 180 && currentAngle != -180);
    }
}
