using CubeSurvivor.Components;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Define uma área do mundo com suas conexões via portais.
    /// </summary>
    public sealed class AreaDefinition
    {
        /// <summary>
        /// ID único da área (ex: "A1", "A2")
        /// </summary>
        /// <summary>
        /// ID único da área (ex: "A1", "A2")
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// Nome de exibição da área (ex: "Floresta Inicial")
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// Largura da área em pixels
        /// </summary>
        public int WidthTiles { get; set; }
        
        /// <summary>
        /// Altura da área em pixels
        /// </summary>
        public int HeightTiles { get; set; }
        
        /// <summary>
        /// Conexões com outras áreas (direção -> ID destino)
        /// </summary>
        public Dictionary<PortalDirection, string> Connections { get; set; } = new();
        
        /// <summary>
        /// Caminho para o arquivo JSON de configuração desta área
        /// </summary>
        public string ConfigPath { get; set; }
    }
    
    /// <summary>
    /// Define o mundo completo com todas as áreas.
    /// </summary>
    public sealed class WorldMapDefinition
    {
        /// <summary>
        /// Lista de todas as áreas do mundo
        /// </summary>
        public List<AreaDefinition> Areas { get; set; } = new();
        
        /// <summary>
        /// Tamanho padrão das áreas em pixels
        /// </summary>
        public int DefaultAreaWidthTiles { get; set; } = 2000;
        
        public int DefaultAreaHeightTiles { get; set; } = 2000;
        
        /// <summary>
        /// Tamanho do tile em pixels
        /// </summary>
        public int TileSize { get; set; } = 32;
        
        /// <summary>
        /// ID da área inicial onde o player spawna
        /// </summary>
        public string StartAreaId { get; set; } = "A1";
    }
}