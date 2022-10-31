using UnityEditor;
using UnityEngine;


public class EventCreator : Editor
{
    [MenuItem("GameObject/Rail Events/Camera Event", false, 10)]
    static void CreateCameraEvent(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("CameraEvent");
        go.AddComponent<CameraEvent>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
    
    [MenuItem("GameObject/Rail Events/Delegate Event", false, 10)]
    static void CreateDelegateEvent(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("DelegateEvent");
        go.AddComponent<DelegateEvent>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
    
    [MenuItem("GameObject/Rail Events/Wind Key Frame", false, 10)]
    static void CreateGameplayEvent(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("Wind Key Frame");
        go.AddComponent<WindKeyframe>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
    
    [MenuItem("GameObject/Rail Events/Wind Way", false, 10)]
    static void CreateWindWay(MenuCommand menuCommand)
    {
        // Create a custom game object
        GameObject go = new GameObject("Wind Way");
        go.AddComponent<WindWay>();
        // Ensure it gets reparented if this was a context click (otherwise does nothing)
        GameObjectUtility.SetParentAndAlign(go, menuCommand.context as GameObject);
        // Register the creation in the undo system
        Undo.RegisterCreatedObjectUndo(go, "Create " + go.name);
        Selection.activeObject = go;
    }
    
}
