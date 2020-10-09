using UnityEngine;

namespace Core.YAMMF.TimeSeriesModel
{
    /**
     * The time-series model is predicting the state variables of the character, ball, etc. for a frame i+1 given
     * those in the current frame i. The complete input vector consists of five components:
     * CharacterState, ControlVariables, ConditioningFeatures, OpponentInformation and LocalMotionPhases.
     */
    public class Input
    {
        public CharacterState characterState;
        public ControlVariables controlVariables;
    }
}
