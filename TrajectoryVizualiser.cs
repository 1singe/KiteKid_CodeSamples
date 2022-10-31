using System.Collections;
using System.Collections.Generic;
using System.IO;
using Cinemachine;
using SimpleJSON;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UX;


public class TrajectoryVizualiser : MonoBehaviour
{
    public List<SO_TrajectoryData> trajectories = new List<SO_TrajectoryData>();


    private List<CinemachineSmoothPath> paths = new List<CinemachineSmoothPath>();
    
        
    public void ReadFiles()
    {
        var path = "Assets/Data/ScriptableObjects/UX/";
        DirectoryInfo dir = new DirectoryInfo(path + "JSON/");
        FileInfo[] files = dir.GetFiles("*.json");
        trajectories.Clear();
        foreach (FileInfo file in files)
        {
            string text = File.ReadAllText(path + "JSON/" + file.Name);
            SO_TrajectoryData trajectory = new SO_TrajectoryData(JsonUtility.FromJson<TrajectoryData>(text));
            trajectories.Add(trajectory);
            #if UNITY_EDITOR
                AssetDatabase.CreateAsset(trajectory , path + "GeneratedAssets/" + file.Name.Replace(file.Extension, "") + ".asset");
                AssetDatabase.SaveAssets();
            #endif
        }
    }
    public void CreateVisuals()
    {
        
        foreach (var trajectory in trajectories)
        {
            var newObj = new GameObject(trajectory.name);
            newObj.transform.parent = transform;
            CinemachineSmoothPath newPath = newObj.AddComponent<CinemachineSmoothPath>();
            newPath.m_Waypoints = new CinemachineSmoothPath.Waypoint[trajectory.trackedPositions.Count];
            newPath.m_Appearance.pathColor = new Color(trajectory.gizmoColor.r, trajectory.gizmoColor.g, trajectory.gizmoColor.b, 1);
            newPath.m_Appearance.width = trajectory.pathWidth;
            for (int i = 0; i < trajectory.trackedPositions.Capacity; i++)
            {
                newPath.m_Waypoints[i].position = trajectory.trackedPositions[i];
            }
            paths.Add(newPath);
        }
    }

    public void DeleteVisuals()
    {
        for (int i = transform.childCount; i >= 0; i--)
        {
            DestroyImmediate(transform.GetChild(i));
        }
        paths.Clear();
        trajectories.Clear();
    }
}
