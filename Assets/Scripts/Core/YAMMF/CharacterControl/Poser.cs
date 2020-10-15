using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Core.YAMMF.TimeSeriesModel;
using UnityEngine;

namespace Core.YAMMF.CharacterControl
{
    public class Poser : MonoBehaviour
    {
        public int currentFrame;
        public int totalFrames;
        public Transform rootBone;
        public List<Transform> Bones { get; private set; }
        public Draw draw;
        private Animator _animator;


        private void Start()
        {
            _animator = GetComponent<Animator>();
            LoadBones();
        }


        public delegate void Draw(int frame);

        public void ResetToTPose()
        {
            
        }
        
        public void SamplePose(AnimationClip clip, int frame)
        {
            clip.SampleAnimation(gameObject, ((float) frame) / (clip.length * clip.frameRate));

        }
        
        private void SamplePose(AnimationClip clip, float time)
        {
            _animator.enabled = false;
            clip.SampleAnimation(gameObject, time);
        }

        
        private void LoadBones()
        {
            if (rootBone == null) return;
            Bones = new List<Transform>();
            Bones.AddRange(rootBone.GetComponentsInChildren<Transform>());
        }

        private void OnDrawGizmos()
        {
            draw?.Invoke(currentFrame);
        }

        private void DrawBones()
        {
            Gizmos.color = new Color(0, 0, 1, 1);
            foreach (Transform b in Bones)
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
    }
}
