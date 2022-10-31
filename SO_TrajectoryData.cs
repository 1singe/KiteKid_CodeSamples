using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UX;

public class SO_TrajectoryData : ScriptableObject
{
    public List<Vector3> trackedPositions = new List<Vector3>();
    public Color gizmoColor;
    public float pathWidth;

    public SO_TrajectoryData(TrajectoryData data)
    {
        trackedPositions = data.trackedPositions;
        gizmoColor = data.gizmoColor;
        pathWidth = data.pathWidth;
    }

    public SO_TrajectoryData()
    {
        trackedPositions = new List<Vector3>();
        gizmoColor = Color.black;
        pathWidth = 1;
    }
    
}
