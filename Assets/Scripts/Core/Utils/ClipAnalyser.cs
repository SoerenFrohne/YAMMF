using System.Collections;
using System.Collections.Generic;
using Core.Utils;
using UnityEngine;
using UnityEditor;

// Analysing clip by its curves
public class ClipAnalyser : MonoBehaviour
{
    public AnimationClip _clip;
    public string boneName;
    public List<AnimationCurve> curves;

    private EditorCurveBinding[] _curveBindings;

    private void Start()
    {
        _curveBindings = AnimationUtility.GetCurveBindings(_clip);

        foreach (EditorCurveBinding curveBinding in _curveBindings)
        {
            Debug.Log(curveBinding.path + ", " + curveBinding.propertyName);
            curves.Add(AnimationUtility.GetEditorCurve(_clip, curveBinding));
        }

        Debug.Log("curve " + curves[1].keys.Length);
        curves[1].NormalizeCurve();
    }
}