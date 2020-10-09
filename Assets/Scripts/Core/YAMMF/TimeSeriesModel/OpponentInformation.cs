using UnityEngine;

namespace Core.YAMMF.TimeSeriesModel
{
    public class OpponentInformation
    {
        public bool opponentIsInRadius;
   
        /**
    * vectors between position samples of the user and opponent trajectories, as well the direction and velocity
    * of the opponent trajectory, all in the time orientation of the ball in the next frame.
    */
        public Vector2 positionDelta;
        public Vector2 rotationDelta;
        public Vector2 velocityDelta;
    }
}
