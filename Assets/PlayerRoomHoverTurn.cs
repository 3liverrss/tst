using UnityEngine;
using UnityEngine.UI;

public class PlayerRoomHoverTurn : MonoBehaviour
{
    public Transform cameraPivot;
    public float turnSpeed = 5f;

    public Image arrowLeft;
    public Image arrowRight;
    public Image arrowBack;

    private int currentAngle = 0;
    private int targetAngle = 0;

    void Update()
    {
        UpdateTargetAngle();
        cameraPivot.localRotation = Quaternion.Lerp(cameraPivot.localRotation,
            Quaternion.Euler(0, targetAngle, 0),
            Time.deltaTime * turnSpeed);

        UpdateArrowHighlights();
    }

    void UpdateTargetAngle()
    {
        Vector3 mousePos = Input.mousePosition;

        if (arrowLeft.gameObject.activeSelf && mousePos.x < Screen.width * 0.1f)
            targetAngle = -90;
        else if (arrowRight.gameObject.activeSelf && mousePos.x > Screen.width * 0.9f)
            targetAngle = 90;
        else if (arrowBack.gameObject.activeSelf && mousePos.y < Screen.height * 0.1f)
            targetAngle = 180;
        else
            targetAngle = 0;

        currentAngle = targetAngle;
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
