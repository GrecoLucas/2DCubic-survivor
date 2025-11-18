using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Componente de XP para o jogador
    /// </summary>
    public class XpComponent : Component
    {
        public float CurrentXp { get; private set; }
        public float RequiredXp { get; private set; }
        public float BaseRequiredXp { get; private set; }

        public XpComponent(float baseRequiredXp = 100f)
        {
            BaseRequiredXp = baseRequiredXp;
            RequiredXp = baseRequiredXp;
            CurrentXp = 0f;
        }

        /// <summary>
        /// Adiciona XP. Retorna true se completou ao menos um ciclo (level-up).
        /// Ao completar, a barra reseta (subtrai o Required atual) e o Required aumenta 25%.
        /// </summary>
        public bool AddXp(float amount)
        {
            CurrentXp += amount;
            bool progressed = false;
            while (CurrentXp >= RequiredXp && RequiredXp > 0f)
            {
                CurrentXp -= RequiredXp;
                RequiredXp *= 1.25f; // aumenta 25% a cada ciclo
                progressed = true;
            }
            return progressed;
        }

        public float GetProgress()
        {
            return RequiredXp > 0f ? CurrentXp / RequiredXp : 0f;
        }
    }
}
