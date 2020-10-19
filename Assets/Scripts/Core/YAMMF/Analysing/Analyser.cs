using System.Collections;
using Core.YAMMF.CharacterControl;
using UnityEngine;

namespace Core.YAMMF.Analysing
{
    /// <summary>
    /// Investigatables are specific data that can be extracted from an animation
    /// </summary>
    public abstract class Analyser : ScriptableObject
    {
        /// <summary>
        /// Extract data from an animation
        /// </summary>
        public abstract IEnumerator ExtractData(AnimationClip clip, Poser poser, int frameIndex);

        /// <summary>
        /// Clear saved data
        /// </summary>
        public abstract void ClearData();
        
        /// <summary>
        /// Visualizing the data on a character
        /// </summary>
        public abstract void Draw(int frame);

        public abstract void ExportData(AnimationDataSet animationDataSet);
    }
}
