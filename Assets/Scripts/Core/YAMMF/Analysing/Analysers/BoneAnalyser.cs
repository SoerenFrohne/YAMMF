using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Core.YAMMF.CharacterControl;
using Core.YAMMF.TimeSeriesModel;
using UnityEditor;
using UnityEngine;

namespace Core.YAMMF.Analysing.Analysers
{
    [Serializable]
    public class FrameSnapshot
    {
        public int key;
        public BoneSnapshot[] bones;
    }

    [CreateAssetMenu(fileName = "BoneAnalyser", menuName = "YAMMF/Analysers/Bones")]
    public class BoneAnalyser : Investigatable
    {
        public FrameSnapshot[] frames = new FrameSnapshot[0];

        public override IEnumerator ExtractData(AnimationClip clip, Poser poser, int frameIndex)
        {
            Debug.Log(frames.Length);

            for (int f = 0; f <= clip.GetKeyframeLength(); f++)
            {
                // Global frame index
                int i = frameIndex + f;
                
                // Setup pose for each frame
                FrameSnapshot frameSnapshot = new FrameSnapshot {key = i, bones = new BoneSnapshot[poser.Bones.Count]};
                poser.SamplePose(clip, f);
                yield return new WaitForSeconds(0f);

                // Extract data for each bone
                for (int b = 0; b < poser.Bones.Count; b++)
                {
                    frameSnapshot.bones[b] = new BoneSnapshot();
                    Transform bone = poser.Bones[b];

                    frameSnapshot.bones[b].isRoot = bone == poser.rootBone;
                    frameSnapshot.bones[b].parent = bone.parent;
                    frameSnapshot.bones[b].name = bone.name;
                    frameSnapshot.bones[b].lastPosition = f == 0 ? bone.position : frames[i - 1].bones[b].currentPosition;
                    frameSnapshot.bones[b].currentPosition = bone.position;
                    frameSnapshot.bones[b].upVector = bone.up;
                    frameSnapshot.bones[b].forwardVector = bone.forward;
                }

                ArrayUtility.Add(ref frames, frameSnapshot);
            }
        }

        public override void ClearData()
        {
            frames = new FrameSnapshot[0];
        }

        public override void Draw(int frame)
        {
            Gizmos.color = new Color(0, 0, 1, 1);
            for (int i = 0; i < frames[frame].bones.Length - 1; i++)
            {
                if (frames[frame].bones[i].isRoot)
                {
                    Gizmos.DrawSphere(frames[frame].bones[i].currentPosition, .03f);
                }
                else
                {
                    Gizmos.DrawLine(frames[frame].bones[i].currentPosition, frames[frame].bones[i].parent.position);
                    Gizmos.DrawSphere(frames[frame].bones[i].currentPosition, .01f);
                }
            }
        }

        public override void ExportData(AnimationSetData animationSet)
        {
        }
    }
}