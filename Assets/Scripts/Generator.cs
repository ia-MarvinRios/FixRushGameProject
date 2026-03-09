using UnityEngine;

namespace FixRushGame
{
    public class Generator : MonoBehaviour
    {
        [SerializeField] GameObject _object;

        private void Start()
        {
            Interactable.OnInteract += GenerateObject;
        }

        private void OnDestroy()
        {
            Interactable.OnInteract -= GenerateObject;
        }

        void GenerateObject(Interactable obj, FixRushGame.AuxPlayer p)
        {
            if (obj == this.GetComponent<Interactable>())
            {
                GameObject item = Instantiate(_object, p.transform.position, Quaternion.identity);
                item.GetComponent<Pickable>()
                        .PickUp(item.GetComponent<Interactable>(), p);
            }
        }
    }
}
