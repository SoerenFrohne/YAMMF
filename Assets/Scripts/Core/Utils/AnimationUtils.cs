using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Core.Utils
{
    public static class AnimationUtils
    {
        public static List<AnimationClip> LoadAnimationsFromPath(string path)
        {
            // Load fbx files
            string[] fbxFilePaths = Directory.GetFiles(path, "*.fbx", SearchOption.AllDirectories);

            // Load assets (an asset can contain multiple clips) and filter for animation clips
            List<AnimationClip> clips = new List<AnimationClip>();
            foreach (string file in fbxFilePaths)
            {
                Debug.Log("Loading: " + file.Replace('\\', '/'));
                Object[] assets = AssetDatabase.LoadAllAssetsAtPath(file.Replace('\\', '/'));
                if (assets.Length <= 0) Debug.LogError("No Assets found at given path");

                foreach (Object o in assets)
                {
                    if (!(o is AnimationClip clip) || o.name.Equals("__preview__mixamo.com")) continue;
                    clips.Add(clip);
                }
            }

            Debug.Log("Clips loaded: " + clips.Count);
            return clips;
        }
        
        public static int GetKeyframeLength(AnimationClip clip)
        {
            return (int) (clip.length * clip.frameRate);
        }
        
        /// <summary>
        /// Root motion of (Mixamo) animations is stored in the first seven animation curves of a clip:
        /// [tx, ty, tz, rx, ry, rz, rw]
        /// </summary>
        /// <param name="clip">name of the <inheritdoc cref="AnimationClip"/></param>
        /// <param name="key">frame number</param>
        /// <returns></returns>
        public static Matrix4x4 GetRootMotion(AnimationClip clip, int key)
        {
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);

            float tx = AnimationUtility.GetEditorCurve(clip, curveBindings[0]).keys[key].value;
            float ty = AnimationUtility.GetEditorCurve(clip, curveBindings[1]).keys[key].value;
            float tz = AnimationUtility.GetEditorCurve(clip, curveBindings[2]).keys[key].value;
            float rx = AnimationUtility.GetEditorCurve(clip, curveBindings[3]).keys[key].value;
            float ry = AnimationUtility.GetEditorCurve(clip, curveBindings[4]).keys[key].value;
            float rz = AnimationUtility.GetEditorCurve(clip, curveBindings[5]).keys[key].value;
            float rw = AnimationUtility.GetEditorCurve(clip, curveBindings[6]).keys[key].value;

            return Matrix4x4.TRS(new Vector3(tx, ty, tz), new Quaternion(rx, ry, rz, rw), Vector3.one);
        }

        public static void NormalizeCurve(this AnimationCurve curve)
        {
            float sum = curve.keys.Sum(key => key.value);
            sum /= curve.keys.Length;

            
            Keyframe[] newKeys = curve.keys;
            for (int i = 0; i < curve.keys.Length; i++)
            {
                newKeys[i].value -= sum;
            }

            curve.keys = newKeys;
        }

        /// <summary>
        /// Get root motion transforms of a specific frame within an animation clip, but projected only on the XZ-axis.
        /// This is helpful when there are problems with y position offset.
        /// </summary>
        /// <param name="clip"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static Matrix4x4 GetFlatRootMotion(AnimationClip clip, int key)
        {
            EditorCurveBinding[] curveBindings = AnimationUtility.GetCurveBindings(clip);

            float tx = AnimationUtility.GetEditorCurve(clip, curveBindings[0]).keys[key].value;
            AnimationCurve y = AnimationUtility.GetEditorCurve(clip, curveBindings[1]);
            y.NormalizeCurve();
            float ty = y.keys[key].value;
            float tz = AnimationUtility.GetEditorCurve(clip, curveBindings[2]).keys[key].value;
            float rx = AnimationUtility.GetEditorCurve(clip, curveBindings[3]).keys[key].value;
            float ry = AnimationUtility.GetEditorCurve(clip, curveBindings[4]).keys[key].value;
            float rz = AnimationUtility.GetEditorCurve(clip, curveBindings[5]).keys[key].value;
            float rw = AnimationUtility.GetEditorCurve(clip, curveBindings[6]).keys[key].value;

            return Matrix4x4.TRS(new Vector3(tx, ty, tz), new Quaternion(rx, ry, rz, rw), Vector3.one);
        }
    }
    
}
