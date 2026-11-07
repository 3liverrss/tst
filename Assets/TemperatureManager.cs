using UnityEngine;
using UnityEngine.UI;

public class TemperatureManager : MonoBehaviour
{
    public Image coldOverlay;
    public Image blanketOverlay;
    public Transform cameraPivot;

    public KeyCode blanketKey = KeyCode.Space;
    public float lookThreshold = 10f;
    public float delayBeforeCold = 5f;
    public float freezeTime = 25f;
    public float warmRate = 0.1f;
    public float blanketFadeSpeed = 4f;

    public bool isFrozen = false;

    float temperature = 1f;
    float blanketAlpha = 0f;
    float timePassed = 0f;

    void Start()
    {
        coldOverlay.gameObject.SetActive(true);

        Color coldColor = coldOverlay.color;
        coldColor.a = 0f;
        coldOverlay.color = coldColor;

        blanketOverlay.color = new Color(0f, 0f, 0f, 0f);
    }

    void Update()
    {
        if (isFrozen)
            return;

        timePassed += Time.deltaTime;

        float xRot = cameraPivot.localEulerAngles.x;

        if (xRot > 180f)
            xRot -= 360f;

        bool lookingStraight = Mathf.Abs(xRot) < lookThreshold;
        bool blanketOn = Input.GetKey(blanketKey) && lookingStraight;

        if (blanketOn)
            temperature += warmRate * Time.deltaTime;
        else if (timePassed > delayBeforeCold)
            temperature -= Time.deltaTime / freezeTime;

        temperature = Mathf.Clamp01(temperature);

        float coldProgress = 1f - temperature;

        Color coldColor = coldOverlay.color;
        coldColor.a = coldProgress;
        coldOverlay.color = coldColor;

        float targetAlpha = blanketOn ? 1f : 0f;

        blanketAlpha = Mathf.Lerp(
            blanketAlpha,
            targetAlpha,
            Time.deltaTime * blanketFadeSpeed
        );

        blanketOverlay.color = new Color(0f, 0f, 0f, blanketAlpha);

        if (temperature <= 0f)
            FreezePlayer();
    }

    void FreezePlayer()
    {
        isFrozen = true;

        Color coldColor = coldOverlay.color;
        coldColor.a = 1f;
        coldOverlay.color = coldColor;

        blanketOverlay.color = new Color(0f, 0f, 0f, 0f);
    }
}