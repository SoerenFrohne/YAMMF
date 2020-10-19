using System;
using UnityEngine;

namespace Core.YAMMF.Analysing.Snapshots
{
    [Serializable]
    public class BoneSnapshot
    {
        public string name;

        public bool isRoot;
        
        public Vector3 parentPosition;
        
        /**
         * Position of the bone transformed into the root coordinate system of the character
         */
        public Vector3 currentPosition;

        public Vector3 lastPosition;

        /**
         * Velocities of the bone
         */
        public Vector3 velocity;

        /**
         * Each bone rotation is formulated by its pair of cartesian forward and up vectors to create an unambiguous
         * and continuous interpolation space [Zhang et al. 2018].
         */
        public Vector3 forwardVector;

        /**
         * Each bone rotation is formulated by its pair of cartesian forward and up vectors to create an unambiguous
         * and continuous interpolation space [Zhang et al. 2018].
         */
        public Vector3 upVector;
        

        public Vector3 GetVelocity()
        {
            return currentPosition - lastPosition;
        }
        
    }
}