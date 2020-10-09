using System.Collections.Generic;
using System.Linq;
using Core.YAMMF.TimeSeriesModel;
using UnityEngine;

namespace Core.YAMMF.CharacterControl
{
    // Expand to character state
    public class Character : MonoBehaviour
    {
        public Transform rootBone;
        public List<Transform> Bones { get; private set; }
        public RootTrajectory rootTrajectory;
        public Animator animator;

        // Start is called before the first frame update
        private void Awake()
        {
            animator = GetComponent<Animator>();
            LoadBones();
        }
        
        
        public void LoadBones()
        {
            if (rootBone == null) return;
            Bones = new List<Transform>();
            Bones.AddRange(rootBone.GetComponentsInChildren<Transform>());
        }
    }
}
