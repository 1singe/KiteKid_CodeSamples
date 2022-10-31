using AK.Wwise;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;

public abstract class RailEvent : MonoBehaviour
{
    protected RailEventManager EventManager;
    protected string Icon = "T_0_empty_.png";
    public float triggerPosition;

    public bool scaling = true;
    private void Set()
    {
        if ((EventManager = FindObjectOfType<RailEventManager>()) == null)
            Debug.LogError("Could not find any RailEventManager in scene !");
        EventManager.Subscribe(this);
        transform.position = EventManager.cart.m_Path.EvaluatePositionAtUnit(triggerPosition, CinemachinePathBase.PositionUnits.Distance);
    }
    
    public void Start()
    {
        Set();
    }

    private void OnValidate()
    {
        Set();
    }

    #if UNITY_EDITOR
    public void OnDrawGizmos()
    {
        
        Color color = Selection.Contains(gameObject) ? Color.white : Color.grey;
        Gizmos.DrawIcon(transform.position, Icon, scaling, color);
        
    }
    #endif

    public abstract void Trigger();
    

}
