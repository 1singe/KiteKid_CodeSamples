using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.IsolatedStorage;
using Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Windows;
using UX;
using Directory = System.IO.Directory;
using File = System.IO.File;

public class TrajectoryAnalyser : MonoBehaviour
{
    public InputAction quittingGame;

    private float time = 0f;
    private KiteController _kiteController;

    public float frequency = 0.5f;

    public List<Vector3> trackedPositions = new List<Vector3>();

    public void Start()
    {
        if((_kiteController = FindObjectOfType<KiteController>()) == null) Debug.LogError("Could not find KiteController");
        quittingGame.Enable();
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time >= frequency)
        {
            trackedPositions.Add(_kiteController.transform.position);
            time = 0f;
        }

        if (quittingGame.IsPressed())
        {
            Application.Quit();
        }
    }

    private void OnApplicationQuit()
    {
        SO_TrajectoryData data = new SO_TrajectoryData();
        data.trackedPositions = trackedPositions;
        var name = "PlayTest_" + System.DateTime.Now.Day + "_" + System.DateTime.Now.Month + "_" +
                   System.DateTime.Now.Year + "_" + System.DateTime.Now.Hour + "_" + System.DateTime.Now.Minute + "_" + System.DateTime.Now.Second;
        data.name = name;
        var path2 = Application.streamingAssetsPath + "/PathAnalysis/";
        if(!Directory.Exists(path2)) Directory.CreateDirectory(path2);
        File.WriteAllText(path2 + name + ".json", JsonUtility.ToJson(data));
    }
}
