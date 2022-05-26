using UnityEngine;
using UnityEngine.UI;
using Card.Arc;

namespace Game.UI
{
    public class UIManager : MonoBehaviour
    {
        [SerializeField] private CardsArc m_CardsArc;
        [SerializeField] private Fade.FadeView m_FadeView;
        [SerializeField] private Button m_TestButton;
        [SerializeField] private DragAndDrop.DropPanel m_DropPanel;

        public CardsArc CardsArc => m_CardsArc;
        public Fade.FadeView FadeView => m_FadeView;
        public Button TestButton => m_TestButton;
        public DragAndDrop.DropPanel DropPanel => m_DropPanel;
    }
}
