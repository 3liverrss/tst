using UnityEngine;
using UnityEngine.UI;

public class TemperatureManager : MonoBehaviour
{
    public Image coldOverlay;
    public Image blanketOverlay;
    public Transform cameraPivot;
    public KeyCode blanketKey = KeyCode.Space;
    public float lookThreshold = 10f;
    public float coldRate = 0.05f;
    public float warmRate = 0.1f;
    public float blanketFadeSpeed = 4f;

    float temperature = 1f;
    float blanketAlpha = 0f;

    void Update()
    {
        float xRot = cameraPivot.localEulerAngles.x;
        if (xRot > 180f) xRot -= 360f;

        bool lookingStraight = Mathf.Abs(xRot) < lookThreshold;
        bool blanketOn = Input.GetKey(blanketKey) && lookingStraight;

        if (blanketOn)
            temperature += warmRate * Time.deltaTime;
        else
            temperature -= coldRate * Time.deltaTime;

        temperature = Mathf.Clamp01(temperature);
        coldOverlay.color = new Color(1, 1, 1, 1f - temperature);

        float targetAlpha = blanketOn ? 1f : 0f;
        blanketAlpha = Mathf.Lerp(blanketAlpha, targetAlpha, Time.deltaTime * blanketFadeSpeed);
        blanketOverlay.color = new Color(0, 0, 0, blanketAlpha);
    }
}
