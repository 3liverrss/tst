using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class BottleSlot
{
    public GameObject bottleObject;
    public TMP_Text label;
}

public class DrunkTeacher : MonoBehaviour
{
    public Camera playerCamera;
    public GameObject questionRoot;
    public TMP_Text equationText;
    public BottleSlot[] bottles;
    public Renderer[] eyeRenderers;

    public int addSubMax = 50;
    public int timesTableMax = 12;

    public float firstAppearMin = 10f;
    public float firstAppearMax = 20f;
    public float answerTime = 10f;
    public float cooldownMin = 15f;
    public float cooldownMax = 30f;
    public float maxClickDistance = 50f;

    public AudioSource sfxSource;
    public AudioClip appearClip;
    public AudioClip breakClip;
    public GameObject breakEffectPrefab;

    public UnityEvent onSolved;
    public UnityEvent onCaught;

    private TemperatureManager temperatureManager;
    private bool questionActive = false;
    private bool caught = false;
    private int correctIndex = 0;
    private float timer = 0f;
    private float waitTimer = 0f;

    void Start()
    {
        temperatureManager = FindFirstObjectByType<TemperatureManager>();

        if (playerCamera == null)
            playerCamera = Camera.main;

        if (questionRoot != null)
            questionRoot.SetActive(false);

        SetEyes(false);

        waitTimer = Random.Range(firstAppearMin, firstAppearMax);
    }

    void Update()
    {
        if (caught)
            return;

        if (temperatureManager != null && temperatureManager.isFrozen)
            return;

        if (!questionActive)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
                StartQuestion();

            return;
        }

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            Catch("ran out of time");
            return;
        }

        if (Input.GetMouseButtonDown(0))
            HandleClick();
    }

    void StartQuestion()
    {
        if (bottles == null || bottles.Length == 0 || equationText == null)
            return;

        int a;
        int b;
        int answer;
        string text;

        int type = Random.Range(0, 4);

        if (type == 0)
        {
            a = Random.Range(1, addSubMax + 1);
            b = Random.Range(1, addSubMax + 1);
            answer = a + b;
            text = a + " + " + b + " = ?";
        }
        else if (type == 1)
        {
            a = Random.Range(2, addSubMax + 1);
            b = Random.Range(1, a);
            answer = a - b;
            text = a + " - " + b + " = ?";
        }
        else if (type == 2)
        {
            a = Random.Range(2, timesTableMax + 1);
            b = Random.Range(2, timesTableMax + 1);
            answer = a * b;
            text = a + " x " + b + " = ?";
        }
        else
        {
            b = Random.Range(2, timesTableMax + 1);
            answer = Random.Range(2, timesTableMax + 1);
            a = b * answer;
            text = a + " / " + b + " = ?";
        }

        equationText.text = text;

        correctIndex = Random.Range(0, bottles.Length);

        List<int> used = new List<int>();
        used.Add(answer);

        for (int i = 0; i < bottles.Length; i++)
        {
            int value = answer;

            if (i != correctIndex)
            {
                int wrong;
                int tries = 0;

                do
                {
                    wrong = answer + Random.Range(-6, 7);
                    tries++;
                }
                while ((wrong < 1 || used.Contains(wrong)) && tries < 50);

                used.Add(wrong);
                value = wrong;
            }

            if (bottles[i].label != null)
                bottles[i].label.text = value.ToString();

            if (bottles[i].bottleObject != null)
                bottles[i].bottleObject.SetActive(true);
        }

        if (questionRoot != null)
            questionRoot.SetActive(true);

        SetEyes(true);

        questionActive = true;
        timer = answerTime;

        PlaySound(appearClip);

        Debug.Log(gameObject.name + " asks: " + text + " (answer " + answer + ")");
    }

    void HandleClick()
    {
        if (playerCamera == null || PointerOverButton())
            return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, maxClickDistance))
            return;

        int clicked = GetBottleIndex(hit.transform);

        if (clicked < 0)
            return;

        BreakBottle(clicked);

        if (clicked == correctIndex)
            Solve();
        else
            Catch("smashed the wrong bottle");
    }

    int GetBottleIndex(Transform hitTransform)
    {
        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i].bottleObject == null)
                continue;

            Transform t = bottles[i].bottleObject.transform;

            if (hitTransform == t || hitTransform.IsChildOf(t))
                return i;
        }

        return -1;
    }

    bool PointerOverButton()
    {
        if (EventSystem.current == null)
            return false;

        PointerEventData data = new PointerEventData(EventSystem.current);
        data.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(data, results);

        foreach (RaycastResult r in results)
        {
            if (r.gameObject.GetComponentInParent<Button>() != null)
                return true;
        }

        return false;
    }

    void BreakBottle(int index)
    {
        GameObject bottle = bottles[index].bottleObject;

        if (breakEffectPrefab != null)
            Instantiate(breakEffectPrefab, bottle.transform.position, bottle.transform.rotation);

        bottle.SetActive(false);

        PlaySound(breakClip);
    }

    void Solve()
    {
        questionActive = false;
        waitTimer = Random.Range(cooldownMin, cooldownMax);

        SetEyes(false);
        HideQuestion();

        Debug.Log(gameObject.name + " is asleep and will be back in " + waitTimer.ToString("F1") + "s");

        onSolved.Invoke();
    }

    void HideQuestion()
    {
        if (questionRoot != null)
            questionRoot.SetActive(false);
    }

    void Catch(string reason)
    {
        if (caught)
            return;

        caught = true;
        questionActive = false;

        Debug.LogWarning(gameObject.name + " caught you: " + reason);

        onCaught.Invoke();
    }

    void SetEyes(bool awake)
    {
        Color c = awake ? Color.white : Color.black;

        foreach (Renderer r in eyeRenderers)
        {
            if (r == null)
                continue;

            r.material.color = c;

            if (r.material.HasProperty("_EmissionColor"))
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", awake ? c * 2f : Color.black);
            }
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }
}