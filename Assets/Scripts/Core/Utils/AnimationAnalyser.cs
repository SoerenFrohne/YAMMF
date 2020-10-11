using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.YAMMF.CharacterControl;
using Core.YAMMF.TimeSeriesModel;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using BoneState = Core.YAMMF.TimeSeriesModel.BoneState;
using Object = UnityEngine.Object;

namespace Core.Utils
{
    public class AnimationAnalyser : MonoBehaviour
    {
        private struct FrameState
        {
            public int relativeFrame; // frame number relative to its animation
            public string animationName;
            public BoneState[] bones;
            public RootTrajectory rootTrajectory;
        }

        public Transform rootBone;
        public string animationsPath;

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
            }

            // Show after analysing
            if (!_analysed) return;
            GUI.Label(new Rect(Screen.width - 250, 72, 250, 32),
                "Rel. Frame: " + _frames[_currentFrame].relativeFrame + " (" +
                (_frames[_currentFrame].relativeFrame / (float) GetKeyframeLength()) + ")");
            GUI.Label(new Rect(Screen.width - 250, 24, 250, 32), "Clip: " + _frames[_currentFrame].animationName);

            if (GUI.Button(new Rect(Screen.width - 256, 224, 128, 32), "Previous Frame"))
            {
                _currentFrame = _currentFrame == 0 ? _frames.Count - 1 : _currentFrame - 1;
                SamplePose(_frames[_currentFrame].animationName, _frames[_currentFrame].relativeFrame);
            }

            if (GUI.Button(new Rect(Screen.width - 128, 224, 128, 32), "Next Frame"))
            {
                _currentFrame = (_currentFrame + 1) % (_frames.Count);
                SamplePose(_frames[_currentFrame].animationName, _frames[_currentFrame].relativeFrame);
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _frames = new List<FrameState>();
            //SamplePose(_animator.GetCurrentAnimatorClipInfo(0)[0].clip.name, 0);
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
            PopulateAnimator();
        }

        private void PopulateAnimator()
        {
            _controller = AnimatorController.CreateAnimatorControllerAtPath("Assets/tmp/animator.controller");

            AnimatorState lastState = new AnimatorState();
            AnimatorState currentState = new AnimatorState();
            for (int index = 0; index < _clips.Count; index++)
            {
                AnimationClip clip = _clips[index];
                if (index != 0) lastState = currentState;
                currentState = _controller.layers[0].stateMachine.AddState(clip.name);
                if (index != 0) lastState.AddTransition(currentState);
                currentState.motion = clip;
            }

            _animator.runtimeAnimatorController = _controller;
            Debug.Log(_animator.hasTransformHierarchy);
            Debug.Log(_animator.hasRootMotion);
        }


        private void SamplePose(string stateName, int frame)
        {
            //_animator.speed = 0;
            _animator.Play(stateName, 0, frame / (float) GetKeyframeLength());
        }

        private IEnumerator AnalyseClips()
        {
            int frameIndex = 0;
            foreach (string clipName in _clips.Select(clip => clip.name))
            {
                Debug.Log("Analysing " + clipName);

                // Get Bone Transforms
                if (rootBone == null) yield break;
                _boneTransforms = new List<Transform>();
                _boneTransforms.AddRange(rootBone.GetComponentsInChildren<Transform>());

                // Setup pose for each frame
                for (int i = 0; i <= GetKeyframeLength(); i++)
                {
                    FrameState frame = new FrameState {animationName = clipName};
                    SamplePose(clipName, i);
                    yield return new WaitForSeconds(.005f);

                    // Save data for each bone
                    frame.bones = new BoneState[_boneTransforms.Count];
                    frame.relativeFrame = i;
                    _frames.Add(frame);
                    for (int b = 0; b < _boneTransforms.Count; b++)
                    {
                        // Skip saving the last position for the first frame
                        if (_frames[frameIndex].bones[b] == null) _frames[frameIndex].bones[b] = new BoneState();

                        _frames[frameIndex].bones[b].lastPosition =
                            i == 0 ? _boneTransforms[b].position : _frames[frameIndex - 1].bones[b].currentPosition;
                        _frames[frameIndex].bones[b].currentPosition = _boneTransforms[b].position;
                        _frames[frameIndex].bones[b].upVector = _boneTransforms[b].up;
                        _frames[frameIndex].bones[b].forwardVector = _boneTransforms[b].forward;
                    }

                    // Save root data for each frame
                    Vector3 position = rootBone.position;
                    frame.rootTrajectory.lastPosition =
                        i == 0 ? Vector2.zero : _frames[frameIndex - 1].rootTrajectory.currentPosition;
                    frame.rootTrajectory.currentPosition = new Vector2(position.x, y: position.z);

                    frameIndex++;
                }


                Debug.Log("Finished analysing " + clipName);
            }

            frameIndex--;
            _currentFrame = frameIndex;
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
            foreach (BoneState bone in _frames[_currentFrame].bones)
            {
                if (_currentFrame == 0) return;
                Gizmos.DrawLine(bone.currentPosition, bone.currentPosition + bone.GetVelocity());
            }
        }

        private void DrawRootTrajectory()
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < _frames.Count; i++)
            {
                FrameState frame = _frames[i];
                Vector3 newPosition = new Vector3(frame.rootTrajectory.currentPosition.x, 0,
                    frame.rootTrajectory.currentPosition.y);
                Vector3 oldPosition = new Vector3(frame.rootTrajectory.lastPosition.x, 0,
                    frame.rootTrajectory.lastPosition.y);
                Gizmos.DrawLine(newPosition, oldPosition);
                Gizmos.DrawSphere(
                    new Vector3(frame.rootTrajectory.currentPosition.x, 0,
                        frame.rootTrajectory.currentPosition.y), .01f);
            }
        }

        private int GetKeyframeLength()
        {
            return (int) (GetCurrentClip().length * GetCurrentClip().frameRate);
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
            DrawRootTrajectory();
        }
    }
}