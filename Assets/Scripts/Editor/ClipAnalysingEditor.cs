using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Utils;
using Core.YAMMF.Analysing;
using Core.YAMMF.CharacterControl;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ClipsAnalysingEditor : EditorWindow
    {
        public Poser poser;
        public string path = "Assets/Animations/Locomotion";
        public string output = "Assets/Animations/Locomotion";
        public Investigatable investigatables;

        private AnimationSetData _animationSetData;
        private List<AnimationClip> _clips;
        private bool _analysed;

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
            output = EditorGUILayout.TextField("Output", output);
            poser = EditorGUILayout.ObjectField("Poser", poser, typeof(Poser), true) as Poser;
            investigatables = EditorGUILayout.ObjectField("Analyser", investigatables, typeof(Investigatable), true) as Investigatable;

            if (GUILayout.Button("Load Animations"))
            {
                LoadAnimations();
            }
            
            if (GUILayout.Button("Analyse Animations"))
            {
                this.StartCoroutine(AnalyseClips());
            }
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
        }
        
        private IEnumerator AnalyseClips()
        {
            int frameIndex = 0;
            
            investigatables.ClearData();
            _animationSetData = CreateInstance<AnimationSetData>();
            AssetDatabase.CreateAsset(_animationSetData, output + "/animSetData.asset");
            AssetDatabase.SaveAssets();

            foreach (AnimationClip clip in _clips)
            {
                string clipName = clip.name;
                Debug.Log("Analysing " + clipName);

                yield return this.StartCoroutine(investigatables.ExtractData(clip, poser, frameIndex));
                frameIndex += clip.GetKeyframeLength() + 1;
                Debug.Log("Finished analysing " + clipName);
            } 
            investigatables.ExportData(_animationSetData);

            _analysed = true;
            poser.totalFrames = frameIndex;
            poser.currentFrame = frameIndex - 1;
            poser.draw += investigatables.Draw;
        }
    }
}