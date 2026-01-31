using FixRushGame;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace FixRushGame
{
    public abstract class Player : MonoBehaviour
    {
        internal List<Interactable> FocusCandidates = new List<Interactable>();
        internal Interactable FocusedObj;
        internal GameObject GrabbedObj;

        protected Rigidbody _rb;

        protected InputSystem_Actions _inputActions;
        protected InputAction _moveAction;

        protected abstract void EnablePlayerInputs();
        protected abstract void DisablePlayerInputs();

        /// <summary>
        /// Updates the currently focused interactable object based on the direction the object is facing.
        /// </summary>
        /// <remarks>This method selects the interactable object from the FocusCandidates collection that
        /// is most directly in front of the current object. The FocusedObj property is updated to reference this
        /// object, or set to null if no suitable candidate is found.</remarks>
        protected void UpdateFocused()
        {
            if (_rb == null)
            {
                FocusedObj = null;
                return;
            }

            const float FOV_THRESHOLD = 0.5f;   // ~120° (Don't touch)
            const float DIST_WEIGHT = 0.1f;

            float bestScore = float.MinValue;
            Interactable best = null;

            Vector3 origin = _rb.transform.position;
            Vector3 forward = _rb.transform.forward;

            foreach (var i in FocusCandidates)
            {
                if (!i) continue;

                Vector3 toObj = i.transform.position - origin;
                float distance = toObj.magnitude;

                Vector3 dir = toObj / distance; //Normalized
                float dot = Vector3.Dot(forward, dir);

                // Out of the FOV
                if (dot < FOV_THRESHOLD)
                    continue;

                float score = dot - (distance * DIST_WEIGHT);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = i;
                }
            }

            FocusedObj = best;
        }
    }
}
