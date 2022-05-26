using UnityEngine;

namespace Game.UI.DragAndDrop
{
    public class DropPanel : MonoBehaviour
    {
        [SerializeField] private RectTransform m_DropParent;

        public bool Drop(Transform card)
        {
            if (!RectTransformUtility.RectangleContainsScreenPoint(transform as RectTransform, Input.mousePosition))
                return false;

            card.rotation = Quaternion.identity;
            card.SetParent(m_DropParent);
            return true;
        }
    }
}
