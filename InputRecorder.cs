using JetBrains.Annotations;
using Uduino;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;
using Event = AK.Wwise.Event;

public class InputRecorder : MonoBehaviour
{
    [CanBeNull] public VideoPlayer videoPlayer;
    
    [Header("Pause")]
    public bool paused;
    

    public RectTransform pauseUI;
    public float UIfadeTime;

    [Header("Calibration")] public RectTransform calibrationUI;
    
    [Space] [Header("Custom controller")]
    public float leftArduinoInput = 0f;
    public float rightArduinoInput = 0f;
    
    [Space]
    [Header("Inputs")]
    [Tooltip("There, you can map the button for the left input, it must be ANALOG")]
    public InputAction leftAction;
    [Tooltip("There, you can map the button for the right input, it must be ANALOG")]
    public InputAction rightAction;

    public InputAction pauseAction;

    [Space] [Header("Input Values Visualizer")]
    [Tooltip("From 0 to 1 : 0 if left input is not pressed, 1 if fully pressed")]
    public float left = 0f;
    [Tooltip("From 0 to 1 : 0 if right input is not pressed, 1 if fully pressed")]
    public float right = 0f;


    [HideInInspector]
    public Vector2 deltaInput;
    [HideInInspector]
    public Vector2 inputAcceleration;

    private JoyconManager _joyconManager;

    public HapticsProfile collisionVibration;

    public Event pauseEvent;
    public Event resumeEven;

    public bool canPause = false;

    private GameObject main;


    private InputValues inputs;
    
    public bool muteInputs = false;
    public void DataReceived(string data, UduinoDevice device)
    {
        if (device.name == "rightRotary")
        {
            InputValues.Instance.rightRawInput = int.Parse(data) - InputValues.Instance.rightRecalibrationCache;
        } else if (device.name == "leftRotary")
        {
            InputValues.Instance.leftRawInput = int.Parse(data) - InputValues.Instance.leftRecalibrationCache;
        }

        main = Camera.main.gameObject;
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (paused) return;
        if (!hasFocus)
        {
            Pause();
        }
        else
        {
            Unpause();
        }
    }


    public void ShowPause()
    {
        LeanTween.scale(pauseUI, Vector3.one, UIfadeTime).setEaseOutBack().setIgnoreTimeScale(true);
    }

    public void UnShowPause()
    {
        LeanTween.scale(pauseUI, Vector3.zero, UIfadeTime).setEaseInBack().setIgnoreTimeScale(true);
    }
    public void Pause()
    {
        if(videoPlayer != null && videoPlayer.clip != null && videoPlayer.isPlaying) videoPlayer.Pause();
        Time.timeScale = 0;
        pauseEvent.Post(main);
    }

    public void Unpause()
    {
        if(videoPlayer != null && videoPlayer.clip != null && videoPlayer.isPaused) videoPlayer.Play();
        Time.timeScale = 1;
        resumeEven.Post(main);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (!paused)
        {
            if (pauseStatus)
            {
                Pause();
            }
            else
            {
                Unpause();
            }
        }
    }


    
    void Start()
    {
        leftAction.Enable();
        rightAction.Enable();
        pauseAction.Enable();
        pauseUI.localScale = Vector3.zero;
        

    }

    void Update()
    {
        if (canPause)
        {
            if (pauseAction.WasPerformedThisFrame() ||
                (InputValues.Instance.rightJoycon != null && InputValues.Instance.rightJoycon.GetButtonDown(Joycon.Button.PLUS)))
            {
                
                if (!paused)
                {
                    Pause();
                    ShowPause();
                }
                else
                {
                    Unpause();
                    UnShowPause();
                }
                paused = !paused;
            }
            
        }
        
        
        left = leftAction.ReadValue<float>();
        right = rightAction.ReadValue<float>();
        leftArduinoInput = Mathf.Clamp(InputValues.Instance.leftRawInput - InputValues.Instance.leftRecalibrationCache, 0, InputValues.Instance.amplitude) / (float) InputValues.Instance.amplitude;
        rightArduinoInput = Mathf.Clamp(InputValues.Instance.rightRawInput - InputValues.Instance.rightRecalibrationCache, 0, InputValues.Instance.amplitude ) / (float) InputValues.Instance.amplitude;
        if (InputValues.Instance.usingCustom)
        {
            left = leftArduinoInput;
            right = rightArduinoInput;
        }

        (left, right) = muteInputs ? (0, 0) : (left, right);
    }

    public void Vibrate(float leftMultiplier, float rightMultiplier, HapticsProfile profile)
    {
        InputValues.Instance.leftJoycon?.SetRumble(profile.lowFrequencyValue, profile.highFrequencyValue, profile.amplitude * leftMultiplier, profile.time);
        InputValues.Instance.rightJoycon?.SetRumble(profile.lowFrequencyValue, profile.highFrequencyValue, profile.amplitude * rightMultiplier, profile.time);
    }
    public void Vibrate(float leftMultiplier, float rightMultiplier, HapticsProfile profile, int time)
    {
        InputValues.Instance.leftJoycon?.SetRumble(profile.lowFrequencyValue, profile.highFrequencyValue, profile.amplitude * leftMultiplier, time);
        InputValues.Instance.rightJoycon?.SetRumble(profile.lowFrequencyValue, profile.highFrequencyValue, profile.amplitude * rightMultiplier, time);
    }

    public void Vibrate(HapticsProfile profile)
    {
        InputValues.Instance.leftJoycon?.SetRumble(profile.lowFrequencyValue, profile.highFrequencyValue, profile.amplitude, profile.time);
        InputValues.Instance.rightJoycon?.SetRumble(profile.lowFrequencyValue, profile.highFrequencyValue, profile.amplitude, profile.time);
    }
}
