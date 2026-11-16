using JuiceStudy.Abilities;
using JuiceStudy.Arcade;
using UnityEngine;

namespace JuiceStudy.Levels
{
    public sealed class LevelDeathZone : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.TryGetComponent(out LevelBall ball)) ball.Lose();
        }
    }
}
