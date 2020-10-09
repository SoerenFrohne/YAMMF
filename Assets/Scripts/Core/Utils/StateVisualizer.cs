using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.YAMMF.CharacterControl;
using Core.YAMMF.TimeSeriesModel;
using UnityEditor;
using UnityEngine;
using BoneState = Core.YAMMF.TimeSeriesModel.BoneState;

namespace Core.Utils
{
    public class StateVisualizer : MonoBehaviour
    {
        private struct FrameState
        {
            public BoneState[] bones;
            public RootTrajectory rootTrajectory;
        }

        private Animator _animator;
        public Transform rootBone;
        private List<Transform> _boneTransforms;
        private FrameState[] _frames;
        private AnimationClip _clip;
        private int currentFrame = 0;
        private bool analysed = false;

        private void OnGUI()
        {
            GUI.Box(new Rect(Screen.width - 256, 0, 256, 256), "Stats");
            GUI.Label(new Rect(Screen.width - 250, 24, 250, 32), "Clip: " + _clip.name);
            GUI.Label(new Rect(Screen.width - 250, 48, 250, 32), "Length: " + GetKeyframeLength() + " Frames");
            GUI.Label(new Rect(Screen.width - 250, 72, 250, 32),
                "Frame: " + currentFrame + " (" + (currentFrame / (float) GetKeyframeLength()) + ")");

            if (GUI.Button(new Rect(Screen.width - 128, 192, 128, 32), "Analyse Clip"))
            {
                StartCoroutine(AnalyseClip());
            }

            if (!analysed) return;
            GUI.Label(new Rect(Screen.width - 250, 96, 250, 32),
                "Bone currentPosition: " + _frames[currentFrame].bones[1].currentPosition);
            GUI.Label(new Rect(Screen.width - 250, 120, 250, 32),
                "Bone lastPosition: " + _frames[currentFrame].bones[1].lastPosition);

            if (GUI.Button(new Rect(Screen.width - 256, 224, 128, 32), "Previous Frame"))
            {
                currentFrame = currentFrame == 0 ? GetKeyframeLength() : currentFrame - 1;
                SamplePose(currentFrame);
            }

            if (GUI.Button(new Rect(Screen.width - 128, 224, 128, 32), "Next Frame"))
            {
                SamplePose((currentFrame + 1) % (GetKeyframeLength() + 1));
            }
        }

        private void Start()
        {
            _animator = GetComponent<Animator>();
            _clip = _animator.GetCurrentAnimatorClipInfo(0)[0].clip;
            SamplePose(0);
        }


        private void SamplePose(int frame)
        {
            currentFrame = frame;
            _animator.speed = 0;
            _animator.Play(_animator.GetCurrentAnimatorStateInfo(0).shortNameHash, 0,
                frame / (float) GetKeyframeLength());
        }

        private IEnumerator AnalyseClip()
        {
            // Get Bone Transforms
            if (rootBone == null) yield break;
            _boneTransforms = new List<Transform>();
            _boneTransforms.AddRange(rootBone.GetComponentsInChildren<Transform>());

            _frames = new FrameState[GetKeyframeLength() + 1];
            // Setup pose for each frame
            for (int i = 0; i <= GetKeyframeLength(); i++)
            {
                SamplePose(i);
                yield return new WaitForSeconds(.1f);
                // Save data for each bone
                _frames[i].bones = new BoneState[_boneTransforms.Count];
                for (int b = 0; b < _boneTransforms.Count; b++)
                {
                    // Skip saving the last position for the first frame
                    if (_frames[i].bones[b] == null) _frames[i].bones[b] = new BoneState();

                    _frames[i].bones[b].lastPosition = i == 0 ? Vector3.zero : _frames[i - 1].bones[b].currentPosition;
                    _frames[i].bones[b].currentPosition = _boneTransforms[b].position;
                    _frames[i].bones[b].upVector = _boneTransforms[b].up;
                    _frames[i].bones[b].forwardVector = _boneTransforms[b].forward;
                }

                // Save root data for each frame
                Vector3 position = rootBone.position;
                _frames[i].rootTrajectory.lastPosition =
                    i == 0 ? Vector2.zero : _frames[i - 1].rootTrajectory.currentPosition;
                _frames[i].rootTrajectory.currentPosition = new Vector2(position.x, y: position.z);
            }

            Debug.Log("Finished Analysing");
            analysed = true;
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
            foreach (BoneState bone in _frames[currentFrame].bones)
            {
                if (currentFrame == 0) return;
                Gizmos.DrawLine(bone.currentPosition, bone.currentPosition + bone.GetVelocity());
            }
        }

        private void DrawRootTrajectory()
        {
            Gizmos.color = Color.cyan;
            for (int i = 1; i < _frames.Length; i++)
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
            return (int) (_clip.length * _clip.frameRate);
        }

        private void OnDrawGizmos()
        {
            if (!analysed) return;
            DrawBones();
            DrawVelocities();
            DrawRootTrajectory();
        }
    }
}