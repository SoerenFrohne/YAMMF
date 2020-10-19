using System;
using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using Core.YAMMF.Analysing.Snapshots;
using Core.YAMMF.CharacterControl;
using Core.YAMMF.TimeSeriesModel;
using UnityEditor;
using UnityEngine;

namespace Core.YAMMF.Analysing.Analysers
{
    [CreateAssetMenu(fileName = "RootTrajectoryAnalyser", menuName = "YAMMF/Analysers/Root Trajectory")]
    public class RootTrajectoryAnalyser : Analyser
    {
        [Serializable]
        public class FrameSnapshot
        {
            public int key;
            public RootTrajectorySnapshot[] rootTrajectories;
        }

        public FrameSnapshot[] frames = new FrameSnapshot[0];
        public int samples = 13;

        public override IEnumerator ExtractData(AnimationClip clip, Poser poser, int frameIndex)
        {
            for (int f = 0; f <= clip.GetUpperBound(); f++)
            {
                // Global frame index
                int i = frameIndex + f;

                FrameSnapshot frameSnapshot = new FrameSnapshot
                    {key = i, rootTrajectories = new RootTrajectorySnapshot[0]};

                // Get sample frames
                for (int s = (int) (clip.frameRate / samples);
                    s <= clip.frameRate;
                    s += (int) (clip.frameRate / samples))
                {
                    // Setup pose for each valid frame and save root data
                    if (f + s >= clip.GetUpperBound()) continue;

                    poser.SamplePose(clip, f + s);
                    yield return new WaitForSeconds(0f);

                    Vector3 position = poser.rootBone.position;
                    RootTrajectorySnapshot trajectory = new RootTrajectorySnapshot
                    {
                        position = new Vector3(position.x, 0, position.z)
                    };
                    ArrayUtility.Add(ref frameSnapshot.rootTrajectories, trajectory);
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
            Gizmos.color = Color.yellow;
            if (!(GenerateSpline(frame) is List<Vector3> curve)) return;
            for (int i = 0; i < curve.Count - 1; i++)
            {
                Gizmos.DrawLine(curve[i], curve[i + 1]);
            }

            foreach (RootTrajectorySnapshot snapshot in frames[frame].rootTrajectories)
            {
                Gizmos.DrawSphere(snapshot.position, 0.005f);
            }
        }

        public override void ExportData(AnimationDataSet animationDataSet)
        {
            RootTrajectoryAnalyser tmp = Instantiate(this);
            AssetDatabase.AddObjectToAsset(tmp, animationDataSet);
        }

        private IEnumerable<Vector3> GenerateSpline(int frame, int stepsPerCurve = 4, float tension = 1)
        {
            List<Vector3> result = new List<Vector3>();
            RootTrajectorySnapshot[] controlPoints = frames[frame].rootTrajectories;
            for (int i = 0; i < controlPoints.Length - 1; i++)
            {
                Vector3 prev = i == 0 ? controlPoints[i].position : controlPoints[i - 1].position;
                Vector3 currStart = controlPoints[i].position;
                Vector3 currEnd = controlPoints[i + 1].position;
                Vector3 next = i == controlPoints.Length - 2
                    ? controlPoints[i + 1].position
                    : controlPoints[i + 2].position;

                for (int step = 0; step <= stepsPerCurve; step++)
                {
                    float t = (float) step / stepsPerCurve;
                    float tSquared = t * t;
                    float tCubed = tSquared * t;

                    Vector3 interpolatedPoint =
                        (-.5f * tension * tCubed + tension * tSquared - .5f * tension * t) * prev +
                        (1 + .5f * tSquared * (tension - 6) + .5f * tCubed * (4 - tension)) * currStart +
                        (.5f * tCubed * (tension - 4) + .5f * tension * t - (tension - 3) * tSquared) * currEnd +
                        (-.5f * tension * tSquared + .5f * tension * tCubed) * next;

                    result.Add(interpolatedPoint);
                }
            }

            return result;
        }
    }
}