using UnityEngine;

namespace Core.YAMMF.TimeSeriesModel
{
    /**
 * Control Variables are the variables used to guide the character to conduct various handball movements.
 * It consists of the following channels that are sampled in the past-to-current time window (-1 second to +1 second)
 * with n samples.
 */
    public class ControlVariables
    {
        public const int Samples = 13;

        /**
         * Horizontal Path of trajectory positions, directions and velocities
         */
        public RootTrajectory[] rootTrajectory = new RootTrajectory[Samples];

        public InteractionVector[] interactionVectors = new InteractionVector[Samples];

        public ActionLabel[] actionLabels = new ActionLabel[Samples];
    }

    public struct ActionLabel
    {
        enum Action
        {
            Idle,
            Move,
            Dribble,
            Hold,
            Shoot
        }
    }

    /**
     * A set of 3D pivot vectors and its derivative around the character, that together define
     * the dribbling direction, height and speed to direct a wide range of dynamic ball
     * interaction movements and maneuvers
     */
    public struct InteractionVector
    {
    }

    /**
     * The trajectory is computed by projecting the hip bone and
     * applying a Gaussian kernel on the root rotation to prevent overfitting to the data
     */
    public struct RootTrajectory
    {
        public Vector2 currentPosition;
        public Vector2 lastPosition;
        public Vector2 direction;

        /**
         * 
         */
        public Vector2 velocity;
    }
}