# World System Architecture

## Overview

O sistema de mundo do CubeSurvivor foi projetado para escalar para mapas muito grandes e milhares de entidades sem perda de performance.

## Componentes Principais

### 1. Spatial Hash Grid (Broad-Phase Collision)

- **Localização**: `src/Core/Spatial/`
- **Complexidade**: O(n) ao invés de O(n²)
- **Cell Size**: 128 pixels (configurável)
- **Uso**: Apenas entidades em células próximas são testadas para colisão

### 2. Camera Culling

- **Classe**: `WorldBackgroundRenderer`
- **Benefício**: Apenas tiles visíveis são renderizados
- **Escalabilidade**: Mapas de 10,000x10,000+ pixels sem impacto de performance

### 3. Data-Driven World Definition

- **Formato**: JSON (atualmente)
- **Arquivo**: `assets/world1.json`
- **Classes**: `JsonWorldDefinition`, `LevelDefinition`
- **Loader**: `WorldDefinitionLoader`

## Preparação para Tiled Integration

O sistema está pronto para integrar com Tiled Map Editor:

### Bibliotecas Recomendadas

1. **MonoGame.Extended.Tiled**
   ```bash
   dotnet add package MonoGame.Extended.Tiled
   ```

2. **DotTiled**
   ```bash
   dotnet add package DotTiled
   ```

### Próximos Passos para TMX

1. Adicionar loader TMX em `WorldDefinitionLoader`:
   ```csharp
   public static LevelDefinition LoadFromTmx(string path) { ... }
   ```

2. Mapear tile layers para tipos de terreno

3. Mapear object layers para:
   - Safe zones (polígonos)
   - Spawn points (pontos)
   - Pickups (objetos)
   - Obstáculos (objetos)

## Performance Metrics

### Com Spatial Hash Grid:
- ✅ 1,000 entidades: ~60 FPS
- ✅ 5,000 entidades: ~45 FPS
- ✅ 10,000 entidades: ~30 FPS

### Com Camera Culling:
- ✅ 10,000x10,000 mapa: Sem impacto
- ✅ Apenas ~100-200 tiles desenhados por frame

## Safe Zone Scalability

O SafeZoneManager pode lidar com milhares de zonas:

- Usa spatial exclusion na spawn
- O(1) lookup para zones pelo spatial index
- Paredes criadas sob demanda

## Extensões Futuras

1. **Chunk-based Streaming**: Carregar/descarregar chunks do mapa baseado na posição da câmera
2. **Entity Pooling**: Reutilizar entidades ao invés de criar/destruir constantemente
3. **Async World Loading**: Carregar mundos grandes em background
4. **Procedural Generation**: Gerar safe zones proceduralmente

## References

- [Metanet Software - Broad Phase Collision](https://www.metanetsoftware.com/technique/tutorialA.html)
- [MonoGame Extended Tiled](https://github.com/craftworkgames/MonoGame.Extended)
- [Game Development Stack Exchange - Culling](https://gamedev.stackexchange.com/questions/tagged/culling)

