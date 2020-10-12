using System;
using System.Collections.Generic;
using System.IO;
using Core.YAMMF.CharacterControl;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ClipsAnalysingEditor : EditorWindow
    {
        public Poser poser;
        public string path = "Assets/Animations/Locomotion";

        private List<AnimationClip> _clips;

        // Add menu named "My Window" to the Window menu
        [MenuItem("YAMMF/Analyse Animations")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            ClipsAnalysingEditor window = (ClipsAnalysingEditor)GetWindow(typeof(ClipsAnalysingEditor));
        
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            path = EditorGUILayout.TextField("Path", path);
            poser = EditorGUILayout.ObjectField("Poser", poser, typeof(Poser), true) as Poser;

            if (GUILayout.Button("Load Animations"))
            {
                LoadAnimations();
            }
            
            if (GUILayout.Button("Sample Pose"))
            {
                poser.SamplePose(_clips[3], 3);
            }
            //groupEnabled = EditorGUILayout.BeginToggleGroup("Optional Settings", groupEnabled);
            //myBool = EditorGUILayout.Toggle("Toggle", myBool);
            //myFloat = EditorGUILayout.Slider("Slider", myFloat, -3, 3);
            //EditorGUILayout.EndToggleGroup();
        }
    
        private void LoadAnimations()
        {
            _clips = new List<AnimationClip>();
            
            // Load fbx files
            string[] fbxFilePaths = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);

            // Load assets (an asset can contain multiple clips) and filter for animation clips
            _clips = new List<AnimationClip>();
            foreach (string file in fbxFilePaths)
            {
                Debug.Log("Loading: " + file.Replace('\\', '/'));
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(file.Replace('\\', '/'));
                if (assets.Length <= 0) Debug.LogError("No Assets found at given path");

                foreach (Object o in assets)
                {
                    if (!(o is AnimationClip clip) || o.name.Equals("__preview__mixamo.com")) continue;
                    _clips.Add(clip);
                }
            }

            Debug.Log("Clips loaded: " + _clips.Count);
            //PopulateAnimator();
        }
    }
}