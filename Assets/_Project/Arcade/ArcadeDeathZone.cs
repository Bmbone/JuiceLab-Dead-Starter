using UnityEngine;

namespace JuiceStudy.Arcade
{
    public sealed class ArcadeDeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out ArcadeBall ball)) ball.Lose();
        }
    }
}
