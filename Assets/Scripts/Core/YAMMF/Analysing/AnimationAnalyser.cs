using System.Collections;
using System.Collections.Generic;
using System.IO;
using Core.Utils;
using Core.Utils.Extensions;
using Core.YAMMF.TimeSeriesModel;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Core.YAMMF.Analysing
{
    public class AnimationAnalyser : MonoBehaviour
    {
        private class FrameState
        {
            public float timeStamp;
            public string animationName;
            public BoneSnapshot[] bones;
            public Matrix4x4 rootState;
        }

        public Transform rootBone;
        public string animationsPath;
        [Tooltip("Samples per second")] public float sampleRate = 60f;

        private Animator _animator;
        private AnimatorController _controller;
        private List<AnimationClip> _clips;
        private List<Transform> _boneTransforms;
        private List<FrameState> _frames;
        private int _currentFrame = 0;
        private bool _analysed = false;

        private void OnGUI()
        {
            GUI.Box(new Rect(Screen.width - 256, 0, 256, 256), "Stats");
            GUI.Label(new Rect(Screen.width - 250, 48, 250, 32),
                "Frame: " + _currentFrame + "/" + (_frames.Count - 1));

            if (GUI.Button(new Rect(Screen.width - 256, 192, 128, 32), "Load AnimationClips"))
            {
                LoadAnimations();
            }

            if (GUI.Button(new Rect(Screen.width - 128, 192, 128, 32), "Analyse Clips"))
            {
                StartCoroutine(AnalyseClips());
                //Play();
            }

            // Show after analysing
            if (!_analysed) return;
            GUI.Label(new Rect(Screen.width - 250, 72, 250, 32), "Time: " + _frames[_currentFrame].timeStamp);
            GUI.Label(new Rect(Screen.width - 250, 24, 250, 32), "Clip: " + _frames[_currentFrame].animationName);

            if (GUI.Button(new Rect(Screen.width - 256, 224, 128, 32), "Previous Frame"))
            {
                _currentFrame = _currentFrame == 0 ? _frames.Count - 1 : _currentFrame - 1;
                SamplePose(_frames[_currentFrame].animationName, _frames[_currentFrame].timeStamp);
            }

            if (GUI.Button(new Rect(Screen.width - 128, 224, 128, 32), "Next Frame"))
            {
                _currentFrame = (_currentFrame + 1) % (_frames.Count);
                SamplePose(_frames[_currentFrame].animationName, _frames[_currentFrame].timeStamp);
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _frames = new List<FrameState>();
            
            // Get Bone Transforms
            _boneTransforms = new List<Transform>();
            _boneTransforms.AddRange(rootBone.GetComponentsInChildren<Transform>());
            
            _animator.speed = 0;
        }

        private void OnApplicationQuit()
        {
            // To-do: Clean up tmp-folder
        }

        private void LoadAnimations()
        {
            // Load fbx files
            string[] fbxFilePaths = Directory.GetFiles(animationsPath, "*.fbx", SearchOption.AllDirectories);

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

        private void SamplePose(string stateName, float time)
        {
            _animator.enabled = false;
            _clips.Find(clip => clip.name.Equals(stateName)).SampleAnimation(gameObject, time);
        }


        private IEnumerator AnalyseClips()
        {
            int frameIndex = 0;
            _currentFrame = frameIndex;

            foreach (AnimationClip clip in _clips)
            {
                string clipName = clip.name;
                Debug.Log("Analysing " + clipName);

                // Setup pose for each frame
                for (int f = 0; f <= clip.GetKeyframeLength(); f++)
                {
                    FrameState frame = new FrameState {animationName = clipName};
                    SamplePose(clipName, f / (float) clip.GetKeyframeLength());
                    yield return new WaitForSeconds(.00f);

                    // Save data for each bone
                    frame.bones = new BoneSnapshot[_boneTransforms.Count];

                    frame.timeStamp = f / (float) clip.GetKeyframeLength();

                    // Save root data for each frame
                    // frame.rootState = AnimationUtils.GetRootMotion(GetCurrentClip(), (int) frame.timeStamp * clip.GetKeyframeLength());

                    _frames.Add(frame);
                    for (int b = 0; b < _boneTransforms.Count; b++)
                    {
                        // Skip saving the last position for the first frame
                        if (_frames[frameIndex].bones[b] == null) _frames[frameIndex].bones[b] = new BoneSnapshot();

                        _frames[frameIndex].bones[b].lastPosition =
                            f == 0 ? _boneTransforms[b].position : _frames[frameIndex - 1].bones[b].currentPosition;
                        _frames[frameIndex].bones[b].currentPosition = _boneTransforms[b].position;
                        _frames[frameIndex].bones[b].upVector = _boneTransforms[b].up;
                        _frames[frameIndex].bones[b].forwardVector = _boneTransforms[b].forward;
                    }

                    frameIndex++;
                }

                Debug.Log("Finished analysing " + clipName);
            }

            frameIndex--;
            
            _currentFrame = 0;
            SamplePose(_clips[0].name, _frames[0].timeStamp);
            _analysed = true;
            Debug.Log(frameIndex);
        }


        private void DrawBones()
        {
            Gizmos.color = new Color(0, 0, 1, 1);
            foreach (Transform b in _boneTransforms)
            {
                if (b == rootBone)
                    Gizmos.DrawSphere(b.position, .03f);
                else
                {
                    Gizmos.DrawLine(b.position, b.parent.position);
                    Gizmos.DrawSphere(b.position, .01f);
                }
            }
        }

        private void DrawVelocities()
        {
            Gizmos.color = Color.red;
            for (int i = 0;
                i < _frames[_currentFrame].bones.Length;
                i++)
            {
                if (_currentFrame == 0) return;
                Gizmos.DrawLine(_boneTransforms[i].position,
                    _boneTransforms[i].position + _frames[_currentFrame].bones[i].GetVelocity());
            }
        }

        private void DrawRootTrajectory()
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < _frames.Count; i++)
            {
                Vector3 newPosition = MatrixExtension.ExtractTranslationFromMatrix(ref _frames[i].rootState);
                Vector3 oldPosition = MatrixExtension.ExtractTranslationFromMatrix(ref _frames[i - 1].rootState);
                Gizmos.DrawLine(newPosition, oldPosition);
                Gizmos.DrawSphere(newPosition, .01f);
            }
        }

        private AnimationClip GetCurrentClip()
        {
            return _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
        }

        private void OnDrawGizmos()
        {
            if (!_analysed) return;
            DrawBones();
            DrawVelocities();
            //DrawRootTrajectory();
        }
    }
}