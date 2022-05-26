using System.Collections.Generic;
using UnityEngine;
using Card.Model;

namespace Card.Arc
{
    public class CardsArc : MonoBehaviour
    {
        [SerializeField] private float m_Width;
        [SerializeField] private float m_Height;

        [SerializeField, HideInInspector] private Vector2 m_Center;
        [SerializeField, HideInInspector] private float m_Radius, m_AngleLeft, m_AngleRight, m_PlaceStep;

        private List<CardModel> _cards = new List<CardModel>();

        public void GetPositionAndRotation(float angle, out Vector2 anchoredPosition, out Quaternion rotation)
        {
            anchoredPosition = GetPositionOnArc(angle);
            rotation = Quaternion.Euler(0f, 0f, -angle * Mathf.Rad2Deg);
        }

        public void AddCards(IEnumerable<CardModel> card)
        {
            _cards.AddRange(card);
            UpdateCardsPositions();
        }

        public void RemoveCard(CardModel card)
        {
            _cards.Remove(card);
            UpdateCardsPositions();
        }

        private void UpdateCardsPositions()
        {
            for (int i = 0; i < _cards.Count; i++)
                _cards[i].InHandPosition = (i - _cards.Count * 0.5f + 0.5f) * m_PlaceStep;
        }

        private Vector2 GetPositionOnArc(float angle)
        {
            return m_Center + new Vector2(Mathf.Sin(angle), Mathf.Cos(angle)) * m_Radius;
        }

        private void OnValidate()
        {
            m_Radius = 0.125f * m_Width * m_Width / m_Height + 0.5f * m_Height;
            m_Center = new Vector2(0f, m_Height - m_Radius);
            m_AngleLeft = Mathf.Asin(-0.5f * m_Width / m_Radius);
            m_AngleRight = -m_AngleLeft;
            m_PlaceStep = (m_AngleRight - m_AngleLeft) / 6f;
        }

        private void OnDrawGizmos()
        {
            Gizmos.matrix = transform.localToWorldMatrix;

            Gizmos.color = Color.red;
            Gizmos.DrawSphere(Vector3.zero, 25f);

            float step = (m_AngleRight - m_AngleLeft) / 180f;
            for (float a = m_AngleLeft; a < m_AngleRight; a += step)
                Gizmos.DrawLine(GetPositionOnArc(a), GetPositionOnArc(a + step));

            for (int count = 5; count <= 6; count++)
            {
                Gizmos.color = count == 5 ? Color.blue : Color.green;
                for (int i = 0; i < count; i++)
                {
                    float a = (i - count * 0.5f + 0.5f) * m_PlaceStep;
                    Gizmos.DrawSphere(GetPositionOnArc(a), 25f);
                }
            }
        }
    }
}
