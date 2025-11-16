namespace CubeSurvivor.Core
{
    /// <summary>
    /// Classe base para todos os componentes no sistema ECS
    /// </summary>
    public abstract class Component
    {
        public Entity Owner { get; set; }
        public bool Enabled { get; set; } = true;
    }
}
