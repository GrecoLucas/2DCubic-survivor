using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de vida/saúde
    /// </summary>
    public class HealthComponent : Component
    {
        public float MaxHealth { get; set; }
        public float CurrentHealth { get; set; }
        public bool IsAlive => CurrentHealth > 0;

        public HealthComponent(float maxHealth)
        {
            MaxHealth = maxHealth;
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(float damage)
        {
            CurrentHealth -= damage;
            if (CurrentHealth < 0) CurrentHealth = 0;
        }

        public void Heal(float amount)
        {
            CurrentHealth += amount;
            if (CurrentHealth > MaxHealth) CurrentHealth = MaxHealth;
        }
    }
}