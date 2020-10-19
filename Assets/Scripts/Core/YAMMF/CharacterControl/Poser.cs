using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Core.Utils;
using Core.YAMMF.TimeSeriesModel;
using UnityEditor.PackageManager.UI;
using UnityEngine;

namespace Core.YAMMF.CharacterControl
{
    public class Poser : MonoBehaviour
    {
        public bool analysed;
        public int currentFrame;
        public int upperLimitFrame;
        public Transform rootBone;
        public List<Transform> Bones { get; private set; }
        public Draw draw;
        private Animator _animator;


        private void Start()
        {
            _animator = GetComponent<Animator>();
            LoadBones();
        }

        public void SamplePose(List<Transform> bones)
        {
            for (int i = 0; i < bones.Count; i++)
            {
                Bones[i].position = bones[i].position;
                Bones[i].rotation = bones[i].rotation;
            }
        }
        
        public void SamplePose(AnimationClip clip, int frame)
        {
            clip.SampleAnimation(gameObject, ((float) frame) / clip.GetUpperBound());
        }

        public void SamplePose(AnimationClip clip, float time)
        {
            clip.SampleAnimation(gameObject, time);
        }

        private void LoadBones()
        {
            if (rootBone == null) return;
            Bones = new List<Transform>();
            Bones.AddRange(rootBone.GetComponentsInChildren<Transform>());
        }

        public delegate void Draw(int frame);

        private void OnDrawGizmos()
        {
            draw?.Invoke(currentFrame);
        }
    }
}