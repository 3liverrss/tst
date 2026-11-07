using UnityEngine;
using UnityEngine.Events;

public enum EyeColorState
{
    Black,
    White,
    Red,
    Yellow
}

public class HomelessEyes : MonoBehaviour
{
    public Transform cameraPivot;
    public Renderer[] eyeRenderers;
    public float lookAngle = 30f;
    public float reactionTime = 4f;
    public bool failIfLookAwayEarly = true;

    public float sleepMin = 15f;
    public float sleepMax = 30f;
    [Range(0f, 1f)] public float wakeChance = 0.7f;
    public float successGrace = 0.75f;

    public UnityEvent onCaught;

    private TemperatureManager temperatureManager;
    private EyeColorState currentEyes = EyeColorState.Black;
    private bool wasLooking = true;
    private bool resolved = true;
    private bool caught = false;
    private float timer = 0f;
    private float sleepTimer = 0f;
    private float graceTimer = 0f;

    void OnEnable()
    {
        PhoneManager.OnFlash += HandleFlash;
        PhoneManager.OnPhoto += HandlePhoto;
    }

    void OnDisable()
    {
        PhoneManager.OnFlash -= HandleFlash;
        PhoneManager.OnPhoto -= HandlePhoto;
    }

    void Start()
    {
        temperatureManager = FindFirstObjectByType<TemperatureManager>();
        SetEyes(EyeColorState.Black);
    }

    void Update()
    {
        if (caught)
            return;

        if (temperatureManager != null && temperatureManager.isFrozen)
            return;

        if (sleepTimer > 0f)
            sleepTimer -= Time.deltaTime;

        if (graceTimer > 0f)
            graceTimer -= Time.deltaTime;

        bool looking = IsPlayerLookingAtMe();

        if (looking && !wasLooking)
            RollEyes();
        else if (!looking && wasLooking)
            HandleLookAway();

        wasLooking = looking;

        if (looking && !resolved)
        {
            timer -= Time.deltaTime;

            if (timer <= 0f)
                Catch("ran out of time while eyes were " + currentEyes);
        }
    }

    bool IsPlayerLookingAtMe()
    {
        Vector3 toMe = transform.position - cameraPivot.position;
        toMe.y = 0f;

        Vector3 forward = cameraPivot.forward;
        forward.y = 0f;

        if (toMe.sqrMagnitude < 0.0001f || forward.sqrMagnitude < 0.0001f)
            return false;

        return Vector3.Angle(forward, toMe) < lookAngle;
    }

    void RollEyes()
    {
        if (sleepTimer > 0f || Random.value > wakeChance)
        {
            SetEyes(EyeColorState.Black);
            resolved = true;
            return;
        }

        EyeColorState rolled = (EyeColorState)Random.Range(1, 4);

        SetEyes(rolled);

        resolved = false;
        timer = reactionTime;

        Debug.Log(gameObject.name + " woke up with " + rolled + " eyes");
    }

    void HandleLookAway()
    {
        if (resolved)
            return;

        if (currentEyes == EyeColorState.White)
            Succeed("looked away");
        else if (failIfLookAwayEarly)
            Catch("looked away while eyes were " + currentEyes);
    }

    void Succeed(string how)
    {
        resolved = true;
        SetEyes(EyeColorState.Black);
        sleepTimer = Random.Range(sleepMin, sleepMax);
        graceTimer = successGrace;

        Debug.Log(gameObject.name + " went to sleep for " + sleepTimer.ToString("F1") + "s (" + how + ")");
    }

    void HandleFlash()
    {
        HandlePhoneAction(EyeColorState.Yellow, "flash");
    }

    void HandlePhoto()
    {
        HandlePhoneAction(EyeColorState.Red, "photo");
    }

    void HandlePhoneAction(EyeColorState needed, string actionName)
    {
        if (caught || !IsPlayerLookingAtMe())
            return;

        if (graceTimer > 0f)
            return;

        if (!resolved && currentEyes == needed)
            Succeed(actionName);
        else
            Catch("used " + actionName + " while eyes were " + currentEyes);
    }

    void SetEyes(EyeColorState state)
    {
        currentEyes = state;

        Color c = Color.black;

        switch (state)
        {
            case EyeColorState.White:
                c = Color.white;
                break;
            case EyeColorState.Red:
                c = Color.red;
                break;
            case EyeColorState.Yellow:
                c = Color.yellow;
                break;
        }

        foreach (Renderer r in eyeRenderers)
        {
            if (r == null)
                continue;

            r.material.color = c;

            if (r.material.HasProperty("_EmissionColor"))
            {
                r.material.EnableKeyword("_EMISSION");
                r.material.SetColor("_EmissionColor", state == EyeColorState.Black ? Color.black : c * 2f);
            }
        }
    }

    void Catch(string reason)
    {
        if (caught)
            return;

        caught = true;
        Debug.LogWarning(gameObject.name + " caught you: " + reason);
        onCaught.Invoke();
    }
}