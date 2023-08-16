using System;
using System.Numerics;
using Cinemachine.Editor;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

[CustomEditor(typeof(Cutscene))]
public class CutsceneEditor : Editor
{
    public override void OnInspectorGUI()
    {
        Cutscene cutscene = target as Cutscene;
        
        if (GUILayout.Button("Open Editor")) CutsceneEditorWindow.Open(cutscene);

        // base.OnInspectorGUI();
    }
}

public class CutsceneEditorWindow : ExtendedEditorWindow
{
    int toolbarInt = 0;
    string[] toolbarStrings = {"Speech Bubble", "Move Object", "Set Animation", "Wait", "Screen Fade", "Start Battle", "Play Video", "Set Music"};
    private static Cutscene _cutscene;
    private static CutsceneEditorWindow _window;

    private Vector2 _actionScrollPos;
    private Vector2 _contentScrollPos;
    
    public static void Open(Cutscene cutscene)
    {
        _window = GetWindow<CutsceneEditorWindow>("Cutscene Editor");
        _window._serializedObject = new SerializedObject(cutscene);
        _cutscene = cutscene;
    }
    
    public void OnGUI()
    {
        GUILayout.Space(15);
        
        toolbarInt = GUILayout.SelectionGrid(toolbarInt, toolbarStrings, 4);

        GUILayout.Space(5);
        if (GUILayout.Button("Insert Above"))
        {
            switch (toolbarInt)
            {
                case 0:
                    _cutscene.AddAction(new DialogueAction(), selectedPropertyIndex);
                    break;
                case 1:
                    _cutscene.AddAction(new MoveObjAction(), selectedPropertyIndex);
                    break;
                case 2:
                    _cutscene.AddAction(new PlayAnimAction(), selectedPropertyIndex);
                    break;
                case 3:
                    _cutscene.AddAction(new WaitAction(), selectedPropertyIndex);
                    break;
                case 4:
                    _cutscene.AddAction(new ScreenFadeAction(), selectedPropertyIndex);
                    break;
                case 5:
                    _cutscene.AddAction(new BattleAction(), selectedPropertyIndex);
                    break;
                case 6:
                    _cutscene.AddAction(new VideoAction(), selectedPropertyIndex);
                    break;
                case 7:
                    _cutscene.AddAction(new MusicAction(), selectedPropertyIndex);
                    break;
            }
            _window._serializedObject = new SerializedObject(_cutscene);
        }
        if (GUILayout.Button("Insert Below"))
        {
            switch (toolbarInt)
            {
                case 0:
                    _cutscene.AddAction(new DialogueAction(), selectedPropertyIndex + 1);
                    break;
                case 1:
                    _cutscene.AddAction(new MoveObjAction(), selectedPropertyIndex + 1);
                    break;
                case 2:
                    _cutscene.AddAction(new PlayAnimAction(), selectedPropertyIndex + 1);
                    break;
                case 3:
                    _cutscene.AddAction(new WaitAction(), selectedPropertyIndex + 1);
                    break;
                case 4:
                    _cutscene.AddAction(new ScreenFadeAction(), selectedPropertyIndex + 1);
                    break;
                case 5:
                    _cutscene.AddAction(new BattleAction(), selectedPropertyIndex + 1);
                    break;
                case 6:
                    _cutscene.AddAction(new VideoAction(), selectedPropertyIndex + 1);
                    break;
                case 7:
                    _cutscene.AddAction(new MusicAction(), selectedPropertyIndex + 1);
                    break;
            }

            selectedPropertyIndex += 1;
            _window._serializedObject = new SerializedObject(_cutscene);
        }
        if (GUILayout.Button("Append to Bottom"))
        {
            switch (toolbarInt)
            {
                case 0:
                    _cutscene.AddAction(new DialogueAction());
                    break;
                case 1:
                    _cutscene.AddAction(new MoveObjAction());
                    break;
                case 2:
                    _cutscene.AddAction(new PlayAnimAction());
                    break;
                case 3:
                    _cutscene.AddAction(new WaitAction());
                    break;
                case 4:
                    _cutscene.AddAction(new ScreenFadeAction());
                    break;
                case 5:
                    _cutscene.AddAction(new BattleAction());
                    break;
                case 6:
                    _cutscene.AddAction(new VideoAction());
                    break;
                case 7:
                    _cutscene.AddAction(new MusicAction());
                    break;
            }

            selectedPropertyIndex = _cutscene.GetCutsceneLength - 1;
            _window._serializedObject = new SerializedObject(_cutscene);
        }

        GUILayout.Space(15);
        GUILayout.Label("Current Actions");

        _currentProperty = _serializedObject.FindProperty("CutsceneActions");
        
        EditorGUILayout.BeginHorizontal();
        
        EditorGUILayout.BeginVertical("box", GUILayout.Width(235), GUILayout.ExpandHeight(true));
        _actionScrollPos =
            EditorGUILayout.BeginScrollView(_actionScrollPos, GUILayout.ExpandHeight(true));
        DrawField("_triggerOnlyOnce", false);
        if (_cutscene._triggerOnlyOnce) DrawField("_ID", false);
        GUILayout.Space(15);
        DrawSidebar(_currentProperty);
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.BeginVertical("box", GUILayout.ExpandHeight(true));
        _contentScrollPos =
            EditorGUILayout.BeginScrollView(_contentScrollPos, GUILayout.ExpandHeight(true));
        if (selectedProperty != null)
        {
            DrawProperties(selectedProperty, true);
            GUILayout.Space(15);
            GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();

            GUI.enabled = selectedPropertyIndex > 0;
            if (GUILayout.Button("Move Up"))
            {
                _currentProperty.MoveArrayElement(selectedPropertyIndex, selectedPropertyIndex - 1);
                selectedPropertyIndex -= 1;
            }

            GUI.enabled = selectedPropertyIndex < _cutscene.GetCutsceneLength - 1;
            if (GUILayout.Button("Move Down"))
            {
                _currentProperty.MoveArrayElement(selectedPropertyIndex, selectedPropertyIndex + 1);
                selectedPropertyIndex += 1;
            }

            GUI.enabled = true;
            if (GUILayout.Button("Remove Action"))
            {
                var oldLength = _currentProperty.arraySize;
                _currentProperty.DeleteArrayElementAtIndex(selectedPropertyIndex);
                if (_currentProperty.arraySize == oldLength)
                    _currentProperty.DeleteArrayElementAtIndex(selectedPropertyIndex);
                selectedPropertyIndex = -70;
                selectedProperty = null;
            }
            GUILayout.EndHorizontal();
        }
        else
        {
            EditorGUILayout.LabelField("Select action to view details");
        }
        
        GUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
        
        EditorGUILayout.EndHorizontal();
        
        _serializedObject.ApplyModifiedProperties();
    }

    void DrawSelectedPropertiesPanel()
    {
        _currentProperty = selectedProperty;

        EditorGUILayout.BeginHorizontal("box");
        
        DrawField("PlayWithNext", true);
        
        EditorGUILayout.EndHorizontal();
    }
}
