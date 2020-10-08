using UnityEngine;

namespace Core.YAMMF.TimeSeriesModel
{
    public class BoneState
    {
        public Vector3 position;
        public Vector3 velocity;

        /**
         * Each bone rotation is formulated by its pair of cartesian forward and up vectors to create an unambiguous
         * and continuous interpolation space [Zhang et al. 2018].
         */
        public Vector3 rotation;
    }
}