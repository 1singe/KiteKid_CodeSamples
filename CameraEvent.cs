using System;
using Cinemachine;
using UnityEditor;
using UnityEngine;


public class CameraEvent : RailEvent
{
    private CameraManager _cameraManager;
    [SerializeField] private CameraMode mode;

    [Space] [Header("SET MODE ONLY")] [SerializeField]
    private CinemachineVirtualCamera targetCamera;
    
    [Space] [Header("TRANSITION")]
    [SerializeField] private float transitionTime;
    [SerializeField] private CinemachineBlendDefinition.Style style = CinemachineBlendDefinition.Style.EaseInOut;
    
    [SerializeField] protected new string icon = "T_1_eye_.png";
    
    public new void Start()
    {
        base.Start();
        if ((_cameraManager = FindObjectOfType<CameraManager>()) == null)
            Debug.LogError("Could not find CameraManager in scene !");
    }

    public override void Trigger()
    {
        switch (mode)
        {
            case CameraMode.None:
                _cameraManager.Switch(_cameraManager.defaultCamera, transitionTime, style);
                break;
            case CameraMode.Set:
                _cameraManager.Switch(targetCamera, transitionTime, style);
                break;
            default:
                Debug.LogError("CameraMode is invalid", gameObject);
                break;
        }
    }
    
    #if UNITY_EDITOR
    public new void OnDrawGizmos()
    {
        base.Icon = icon;
        base.OnDrawGizmos();    
    }
    #endif
    

}
