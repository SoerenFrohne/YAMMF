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
        public Transform rootBone;
        public List<Transform> Bones { get; private set; }
        public List<AnimationClip> clips;
        
        
        private void Awake()
        {
            LoadBones();
            clips = AnimationUtils.LoadAnimationsFromPath("Assets/AnimationData/Animations/Locomotion");
            StartCoroutine(SamplePose(clips[0], 5));
        }


        public IEnumerator SamplePose(AnimationClip clip, int frame)
        {
            Debug.Log((float) frame / (clip.length * clip.frameRate));
            Debug.Log(clip.length * clip.frameRate);
            clip.SampleAnimation(gameObject, ((float) frame) / (clip.length * clip.frameRate));
            yield return new WaitForSeconds(0f);
        }
        
        private void LoadBones()
        {
            if (rootBone == null) return;
            Bones = new List<Transform>();
            Bones.AddRange(rootBone.GetComponentsInChildren<Transform>());
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
