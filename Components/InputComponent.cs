using CubeSurvivor.Core;

namespace CubeSurvivor.Components
{
    /// <summary>
    /// Marca uma entidade como controlada pelo jogador
    /// </summary>
    public class InputComponent : Component
    {
        public bool IsPlayerControlled { get; set; } = true;
    }
}
