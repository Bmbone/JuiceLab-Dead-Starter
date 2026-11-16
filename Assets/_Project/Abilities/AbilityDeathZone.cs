using JuiceStudy.Arcade;
using UnityEngine;

namespace JuiceStudy.Abilities
{
    public sealed class AbilityDeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out AbilityBall ball)) ball.Lose();
        }
    }
}
