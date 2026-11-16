using JuiceStudy.Levels;
using UnityEngine;

namespace JuiceStudy.RelicGear
{
    /// <summary>Observes genuine ball/brick contacts without handling damage, velocity or scoring.</summary>
    [DisallowMultipleComponent]
    public sealed class BallContactRelay : MonoBehaviour
    {
        [SerializeField] private RelicGearFeedback feedback;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (!isActiveAndEnabled || feedback == null || collision.contactCount == 0) return;
            if (collision.gameObject.TryGetComponent(out LevelBrick _))
                feedback.NotifyBrickContact(collision.GetContact(0).normal);
        }
    }
}
