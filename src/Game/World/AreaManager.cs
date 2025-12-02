using CubeSurvivor.Components;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CubeSurvivor.Game.Map
{
    /// <summary>
    /// Gerencia áreas do mundo e transições entre elas.
    /// </summary>
    public sealed class AreaManager
    {
        private readonly WorldMapDefinition _worldMap;
        private readonly IGameWorld _world;
        private readonly PortalFactory _portalFactory;
        private string _currentAreaId;
        private AreaDefinition _currentArea;
        private MapDefinition _currentMapDefinition;
        
        public string CurrentAreaId => _currentAreaId;
        public AreaDefinition CurrentArea => _currentArea;
        public MapDefinition CurrentMapDefinition => _currentMapDefinition;
        
        public AreaManager(WorldMapDefinition worldMap, IGameWorld world)
        {
            _worldMap = worldMap;
            _world = world;
            _portalFactory = new PortalFactory();
        }
        
        /// <summary>
        /// Carrega uma área e cria seus portais
        /// </summary>
        public MapDefinition LoadArea(string areaId, Vector2? spawnPosition = null)
        {
            _currentArea = _worldMap.Areas.FirstOrDefault(a => a.Id == areaId);
            if (_currentArea == null)
            {
                Console.WriteLine($"[AreaManager] Area {areaId} not found!");
                return null;
            }
            
            _currentAreaId = areaId;
            
            Console.WriteLine($"[AreaManager] === Loading Area: {areaId} ===");
            
            // Load map definition for this area
            MapDefinition mapDef;
            
            if (File.Exists(_currentArea.ConfigPath))
            {
                mapDef = MapLoader.Load(_currentArea.ConfigPath);
                Console.WriteLine($"[AreaManager] Loaded map from {_currentArea.ConfigPath}");
            }
            else
            {
                // Create default map for this area
                mapDef = MapLoader.CreateDefaultMap(
                    _currentArea.WidthTiles,
                    _currentArea.HeightTiles,
                    _worldMap.TileSize
                );
                
                // Save it for future use
                Directory.CreateDirectory(Path.GetDirectoryName(_currentArea.ConfigPath));
                MapSaver.Save(_currentArea.ConfigPath, mapDef);
                Console.WriteLine($"[AreaManager] Created and saved default map to {_currentArea.ConfigPath}");
            }
            
            _currentMapDefinition = mapDef;
            
            // Update AreaDefinition name if available in map definition
            if (!string.IsNullOrEmpty(mapDef.Name))
            {
                _currentArea.Name = mapDef.Name;
                Console.WriteLine($"[AreaManager] Updated area name to: {mapDef.Name}");
            }
            
            // Create portals for connections
            CreatePortals();
            
            return mapDef;
        }
        
        private void CreatePortals()
        {
            if (_currentArea == null || _currentMapDefinition == null)
                return;
            
            // MapWidth e MapHeight já estão em pixels
            int mapWidthPx = _currentMapDefinition.MapWidth;
            int mapHeightPx = _currentMapDefinition.MapHeight;
            
            Console.WriteLine($"[AreaManager] Creating portals for area {_currentAreaId}...");
            
            foreach (var connection in _currentArea.Connections)
            {
                var direction = connection.Key;
                var destinationId = connection.Value;
                
                Vector2 portalPosition;
                Vector2 relativeSpawnPos;
                
                switch (direction)
                {
                    case PortalDirection.Right:
                        // Portal on right edge, middle
                        portalPosition = new Vector2(mapWidthPx - 32, mapHeightPx / 2);
                        // Spawn on left side of destination
                        relativeSpawnPos = new Vector2(0.05f, 0.5f);
                        break;
                    
                    case PortalDirection.Left:
                        // Portal on left edge, middle
                        portalPosition = new Vector2(32, mapHeightPx / 2);
                        // Spawn on right side of destination
                        relativeSpawnPos = new Vector2(0.95f, 0.5f);
                        break;
                    
                    case PortalDirection.Up:
                        // Portal on top edge, middle
                        portalPosition = new Vector2(mapWidthPx / 2, 32);
                        // Spawn on bottom of destination
                        relativeSpawnPos = new Vector2(0.5f, 0.95f);
                        break;
                    
                    case PortalDirection.Down:
                        // Portal on bottom edge, middle
                        portalPosition = new Vector2(mapWidthPx / 2, mapHeightPx - 32);
                        // Spawn on top of destination
                        relativeSpawnPos = new Vector2(0.5f, 0.05f);
                        break;
                    
                    default:
                        continue;
                }
                
                _portalFactory.CreatePortal(
                    _world,
                    portalPosition,
                    destinationId,
                    direction,
                    relativeSpawnPos
                );
                
                Console.WriteLine($"[AreaManager]   Portal {direction} -> {destinationId} at {portalPosition}");
            }
        }
        
        /// <summary>
        /// Calcula posição de spawn baseada na posição relativa
        /// </summary>
        public Vector2 CalculateSpawnPosition(Vector2 relativePosition)
        {
            if (_currentMapDefinition == null)
                return Vector2.Zero;
            
            // MapWidth e MapHeight já estão em pixels
            int mapWidthPx = _currentMapDefinition.MapWidth;
            int mapHeightPx = _currentMapDefinition.MapHeight;
            
            return new Vector2(
                relativePosition.X * mapWidthPx,
                relativePosition.Y * mapHeightPx
            );
        }
        
        /// <summary>
        /// Remove todas as entidades da área atual (exceto o player)
        /// </summary>
        public void ClearCurrentArea()
        {
            Console.WriteLine($"[AreaManager] Clearing area {_currentAreaId}...");
            
            var entitiesToRemove = new List<Entity>();
            
            foreach (var entity in _world.GetAllEntities())
            {
                // Don't remove player
                if (entity.GetComponent<PlayerInputComponent>() != null)
                    continue;

                // Don't remove entities attached to player (like weapon visuals)
                var attachment = entity.GetComponent<AttachmentComponent>();
                if (attachment != null && attachment.Parent != null && attachment.Parent.GetComponent<PlayerInputComponent>() != null)
                    continue;
                
                entitiesToRemove.Add(entity);
            }
            
            foreach (var entity in entitiesToRemove)
            {
                _world.RemoveEntity(entity);
            }
            
            Console.WriteLine($"[AreaManager] Removed {entitiesToRemove.Count} entities");
        }
    }
}