using UnityEngine;
using UnityEngine.InputSystem;

namespace FixRushGame
{
    public abstract class Player : MonoBehaviour
    {
        internal abstract Interactable FocusedObj { get; set; }
        internal abstract GameObject GrabbedObj { get; set; }
        internal AuxPlayer SelAux;

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
            if (_rb == null || SelAux == null)
            {
                FocusedObj = null;
                return;
            }

            const float FOV_THRESHOLD = 0.5f;
            const float DIST_WEIGHT = 0.1f;

            float bestScore = float.MinValue;
            Interactable best = null;

            Vector3 origin = _rb.position;
            Vector3 forward = _rb.transform.forward;

            var candidates = SelAux.FocusCandidates;

            for (int i = 0; i < candidates.Count; i++)
            {
                var interactable = candidates[i];
                if (!interactable) continue;

                Vector3 toObj = interactable.transform.position - origin;
                float distance = toObj.magnitude;
                if (distance <= 0.001f) continue;

                Vector3 dir = toObj / distance;
                float dot = Vector3.Dot(forward, dir);

                if (dot < FOV_THRESHOLD)
                    continue;

                float score = dot - distance * DIST_WEIGHT;

                if (score > bestScore)
                {
                    bestScore = score;
                    best = interactable;
                }
            }

            FocusedObj = best;
        }
    }
}
