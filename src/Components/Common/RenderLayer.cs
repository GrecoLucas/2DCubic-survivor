namespace CubeSurvivor.Components
{
    /// <summary>
    /// Define a ordem de renderização das entidades.
    /// Valores menores são renderizados primeiro (atrás).
    /// </summary>
    public enum RenderLayer
    {
        /// <summary>
        /// Camada do fundo/chão (renderizada primeiro)
        /// </summary>
        Background = 0,
        
        /// <summary>
        /// Itens no chão (acima do background, abaixo de tudo)
        /// </summary>
        GroundItems = 10,
        
        /// <summary>
        /// Efeitos visuais no chão
        /// </summary>
        GroundEffects = 20,
        
        /// <summary>
        /// Projéteis (balas, etc)
        /// </summary>
        Projectiles = 30,
        
        /// <summary>
        /// Entidades (jogador, inimigos)
        /// </summary>
        Entities = 40,
        
        /// <summary>
        /// Armas seguradas por entidades
        /// </summary>
        Weapons = 50,
        
        /// <summary>
        /// Efeitos visuais acima das entidades
        /// </summary>
        Effects = 60,
        
        /// <summary>
        /// Interface do usuário
        /// </summary>
        UI = 100
    }
}

