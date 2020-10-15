using System.Collections.Generic;
using UnityEngine;

namespace Core.YAMMF.TimeSeriesModel
{
    /**
     * The CharacterState represents the current state of the character at a frame i.
     */
    public class CharacterState
    {
        /**
         * Vector of all bones with its positions, velocities and rotations
         */
        public List<BoneSnapshot> bones;

        /**
         * Control Variables are used to guide the character to conduct various handball movements.
         */
        public ControlVariables controlVariables;

        /**
         * Holding information about ball movement and contacts
         */
        public ConditioningFeatures conditioningFeatures;

        /**
         * the state of the opponent character with respect to the user character.
         */
        public OpponentInformation opponentInformation;

        /**
         * Local Phase: Contact transitions between a bone and other objects/environments.
         * For each key bone (hands, feet, ball) 
         */
        //public List<LocalMotionPhase> localMotionPhases;
    }
}