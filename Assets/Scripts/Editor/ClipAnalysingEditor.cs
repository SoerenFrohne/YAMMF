using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Utils;
using Core.YAMMF.Analysing;
using Core.YAMMF.CharacterControl;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace Editor
{
    public class ClipsAnalysingEditor : EditorWindow
    {
        public Poser poser;
        public string path = "Assets/Animations/Locomotion";
        public string output = "Assets/Animations/Locomotion";
        public Analyser[] analysers;

        private AnimationDataSet _animationDataSet;
        private List<AnimationClip> _clips;
        private bool _analysed;

        // Add menu named "My Window" to the Window menu
        [MenuItem("YAMMF/Analyse Animations")]
        private static void Init()
        {
            // Get existing open window or if none, make a new one:
            ClipsAnalysingEditor window =
                (ClipsAnalysingEditor) GetWindow(typeof(ClipsAnalysingEditor), false, "Analyse Animation Set");
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Label("Base Settings", EditorStyles.boldLabel);
            path = EditorGUILayout.TextField("Path", path);
            output = EditorGUILayout.TextField("Output", output);
            poser = EditorGUILayout.ObjectField("Poser", poser, typeof(Poser), true) as Poser;

            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty investigatablesProperty = so.FindProperty("analysers");

            EditorGUILayout.PropertyField(investigatablesProperty, true);
            so.ApplyModifiedProperties();


            GUILayout.BeginHorizontal();
            if (GUILayout.Button(_clips == null || _clips.Count == 0 ? "Load Animations" : "Update Animations"))
                LoadAnimations();

            EditorGUI.BeginDisabledGroup(_clips == null || _clips.Count == 0 || !EditorApplication.isPlaying);
            if (GUILayout.Button("Analyse Animations"))
            {
                this.StartCoroutine(AnalyseClips());
            }

            EditorGUI.EndDisabledGroup();
            GUILayout.EndHorizontal();

            if (!EditorApplication.isPlaying)
                GUILayout.Label("You must enter play mode to analyse animations", EditorStyles.helpBox);

            //----------------------------------------------------------------------------------------------------------
            EditorGUILayout.Space();

            if (_analysed == false) return;

            GUILayout.Label("Frame Control", EditorStyles.boldLabel);

            GUILayout.BeginHorizontal();
            
            if (GUILayout.Button("-10")) UpdatePoser(-10);

            if (GUILayout.Button("-1")) UpdatePoser(-1);

            GUILayout.Label("Frame: " + poser.currentFrame + "/" + poser.upperLimitFrame,
                new GUIStyle(GUI.skin.label) {alignment = TextAnchor.MiddleCenter});

            if (GUILayout.Button("+1")) UpdatePoser(1);

            if (GUILayout.Button("+10")) UpdatePoser(10);

            GUILayout.EndHorizontal();
            GUILayout.Label("Clip: " + GetClip(poser.currentFrame).name, EditorStyles.boldLabel);
        }

        private void LoadAnimations()
        {
            _clips = new List<AnimationClip>();
            int totalFrames = 0;

            // Load fbx files
            string[] fbxFilePaths = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);

            // Load assets (an asset can contain multiple clips) and filter for animation clips
            _clips = new List<AnimationClip>();
            foreach (string file in fbxFilePaths)
            {
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(file.Replace('\\', '/'));
                if (assets.Length <= 0) Debug.LogError("No Assets found at given path");

                foreach (Object o in assets)
                {
                    if (!(o is AnimationClip clip) || o.name.Equals("__preview__mixamo.com")) continue;
                    totalFrames += clip.GetFrameCount();
                    _clips.Add(clip);
                    Debug.Log("Clip added: " + clip.name + ", (Size: " + clip.GetFrameCount() + ")");
                }
            }

            Debug.Log(_clips.Count + " Clips loaded, " + totalFrames + " Frames to analyse");
        }

        private IEnumerator AnalyseClips()
        {
            if (poser == null) poser = (Poser) FindObjectOfType(typeof(Poser));
            int frameIndex = 0;

            ClearData();
            _animationDataSet = CreateInstance<AnimationDataSet>();
            AssetDatabase.CreateAsset(_animationDataSet, output + "/animSetData.asset");
            AssetDatabase.SaveAssets();

            // Analyse Clips
            foreach (AnimationClip clip in _clips)
            {
                string clipName = clip.name;
                Debug.Log("Analysing " + clipName);

                foreach (Analyser a in analysers)
                {
                    yield return this.StartCoroutine(a.ExtractData(clip, poser, frameIndex));
                }

                frameIndex += clip.GetFrameCount();
                Debug.Log("Finished analysing " + clipName);
            }

            // Update poser
            _analysed = true;
            poser.upperLimitFrame = frameIndex - 1;
            poser.currentFrame = frameIndex - 1;

            // Cumulate data, export it to an asset and refresh view
            foreach (Analyser a in analysers)
            {
                a.ExportData(_animationDataSet);
                poser.draw += a.Draw;
            }

            AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(_animationDataSet));
        }

        private IEnumerator LoadScene()
        {
            AsyncOperation asyncLoadLevel =
                SceneManager.LoadSceneAsync("Assets/Scenes/Analyser.unity", LoadSceneMode.Single);
            while (!asyncLoadLevel.isDone)
            {
                yield return null;
            }
        }

        private void ClearData()
        {
            foreach (Analyser analyser in analysers)
            {
                analyser.ClearData();
            }
        }

        private void OnEnable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private void OnDisable()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode) poser = (Poser) FindObjectOfType(typeof(Poser));
            else _analysed = false;
        }


        private void UpdatePoser(int delta)
        {
            if (poser == null) return;
            poser.currentFrame = Mathf.Clamp(poser.currentFrame + delta, 0, poser.upperLimitFrame);
            poser.SamplePose(GetClip(poser.currentFrame), ToLocalTime(poser.currentFrame));
        }


        private AnimationClip GetClip(int frame)
        {
            int i = 0;
            foreach (AnimationClip clip in _clips)
            {
                i += clip.GetFrameCount();
                if (frame < i) return clip;
            }

            return null;
        }


        private float ToLocalTime(int frame)
        {
            int i = 0;
            int lastLength = 0;
            foreach (AnimationClip clip in _clips)
            {
                i += clip.GetFrameCount();
                if (frame < i) return (frame - lastLength) / (float) clip.GetUpperBound();
                lastLength += clip.GetFrameCount();
            }

            return 0f;
        }
    }
}