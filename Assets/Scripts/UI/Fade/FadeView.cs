using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Game.UI.Fade
{
    public class FadeView : MonoBehaviour
    {
        [SerializeField] private Image m_Fade;
        [SerializeField] private TextMeshProUGUI m_FadeText;

        public void SetState(bool fade, float delay, string text = "")
        {
            m_FadeText.text = text;
            StopAllCoroutines();
            StartCoroutine(FadeTransition(fade ? 1f : 0f, delay));
        }

        private IEnumerator FadeTransition(float end, float delay)
        {
            Color fadeColor = m_Fade.color, textColor = m_FadeText.color;
            float start = fadeColor.a;

            for (float t = 0f; t < delay; t += Time.deltaTime)
            {
                fadeColor.a = textColor.a = Mathf.Lerp(start, end, t / delay);
                m_Fade.color = fadeColor;
                m_FadeText.color = textColor;
                yield return null;
            }

            fadeColor.a = textColor.a = end;
            m_Fade.color = fadeColor;
            m_FadeText.color = textColor;
        }
    }
}
