# 📜 Histórico de Desenvolvimento

Este documento consolida informações de refatorações e melhorias importantes realizadas no projeto.

---

## 🎯 Refatoração Profunda - Sistema de Mapas V2 + Editor

### Status: ✅ Completo

#### Mudanças Principais

1. **Escala Global (TileSize 32x32)**
   - TileSize padrão: 128 → 50 → 32 pixels
   - Player, tiles e blocos alinhados a 32x32
   - Colisões perfeitamente alinhadas na grelha

2. **Sistema de Regiões**
   - Suporte completo para múltiplos tipos de região
   - PlayerSpawn, EnemySpawn, GoldSpawn, WoodSpawn, AppleSpawn, TreeSpawn, ItemSpawn, SafeZone, Biome
   - Apenas 1 PlayerSpawn permitido (enforcement automático)
   - Editor completo com criação, seleção, edição e remoção

3. **Serialização de Regiões**
   - RectangleJsonConverter para serialização correta de áreas
   - Suporte a formato antigo (left/right/top/bottom) e novo (x/y/width/height)
   - Limpeza automática de regiões inválidas ao carregar

4. **Sistema de Layers**
   - True layered map stack: Tiles → ItemsLow → Blocks → ItemsHigh
   - Editor com seleção de layer ativa
   - Visibilidade por layer
   - Overlay mode para ver todas as layers

5. **Item Layers**
   - Suporte a itens colocados diretamente no mapa
   - ItemLayers (ItemsLow e ItemsHigh)
   - Remoção automática de itens quando coletados
   - Spawn de itens do mapa no PlayState

6. **Biome System**
   - BiomeSystem permite spawns quando não há biomes definidos
   - Suporte a biomas opcionais (não obrigatórios)

7. **Editor UI**
   - Region Palette com seleção de tipo
   - Erase mode para remover regiões
   - Input routing correto (UI consome eventos primeiro)
   - Scroll wheel funcional na palette

8. **Debug Instrumentation**
   - Logs extensivos em RegionTool, MapSaver, MapLoader, EditorRenderer, PlayState, EnemySpawnSystem
   - Rastreamento completo do pipeline: create → save → load → render → spawn

---

## 🎮 Menu ESC (Pause Menu)

### Status: ✅ Completo

**Componente:** `UIPauseMenu.cs`

**Funcionalidades:**
- Resume - Continuar jogo/editor
- Save - Guardar mapa (só no Editor)
- Main Menu - Voltar ao menu principal
- Exit Game - Sair do jogo

**Características:**
- Bloqueia todo o input quando aberto
- Overlay semi-transparente
- ESC abre/fecha (toggle)
- 100% mouse-friendly
- Auto-save no Editor antes de sair

---

## 🔨 Sistema de Construção

### Status: ✅ Completo

**Componentes:**
- `WoodItem` - Material para construção (max stack: 99)
- `HammerItem` - Ferramenta que habilita construção (max stack: 1)

**Fluxo:**
1. Pegar Hammer (localizado no mapa)
2. Coletar Madeira (4 unidades por caixa)
3. Clicar com botão direito para construir (dentro de 300 pixels, local livre)

**Sistemas:**
- `ConstructionSystem` - Processa construção de caixas
- `BuilderComponent` - Componente que habilita construção
- `WoodEntityFactory` - Cria entidades de madeira
- `WorldObjectFactory.CreateCrate()` - Cria caixas destrutíveis (50 HP)

---

## 🗺️ Sistema de Mapas

### Formato JSON

```json
{
  "mapWidth": 256,
  "mapHeight": 256,
  "tileSize": 32,
  "chunkSize": 64,
  "tileLayers": [...],
  "blockLayers": [...],
  "itemLayers": [...],
  "regions": [
    {
      "id": "player_spawn_1",
      "type": "PlayerSpawn",
      "area": {"x": 10, "y": 10, "width": 5, "height": 5},
      "meta": {}
    }
  ]
}
```

### Block Types
- `Empty` (0)
- `Wall` (1)
- `Crate` (2)
- `Tree` (3)
- `Rock` (4)

### Region Types
- `PlayerSpawn` - Onde o jogador inicia
- `EnemySpawn` - Áreas de spawn de inimigos
- `GoldSpawn` - Spawn de ouro
- `WoodSpawn` - Spawn de madeira
- `AppleSpawn` - Spawn de maçãs
- `TreeSpawn` - Spawn de árvores
- `ItemSpawn` - Spawn genérico de itens
- `SafeZone` - Zonas seguras (sem inimigos)
- `Biome` - Definição de biomas

---

## 🎨 Editor de Mapas

### Ferramentas
- **Brush** - Pintar tiles/blocks/items
- **Eraser** - Apagar tiles/blocks/items
- **BoxFill** - Preencher retângulo
- **FloodFill** - Preencher área contígua
- **Picker** - Copiar tile/block/item
- **Region** - Criar/editar regiões
- **SelectMove** - Selecionar e mover

### UI
- **Left Sidebar**: Tools, Layers, Palettes (Tiles/Blocks/Items), Region Palette
- **Right Sidebar**: Layers list, Regions list (com Focus/Delete), Region Meta Editor
- **Top Bar**: Save & Exit button
- **Canvas**: Área de edição com grid

### Controles
- **Left Mouse**: Paint/Place
- **Right Mouse**: Pan camera
- **WASD**: Move camera
- **Mouse Wheel**: Zoom
- **ESC**: Pause menu
- **S**: Quick save
- **Delete/Backspace**: Delete selected region

---

## 📊 Estatísticas de Refatoração

### Arquivos Modificados
- MapDefinition.cs
- MapLoader.cs
- MapSaver.cs
- MapRegistry.cs
- ChunkedTileMap.cs
- MapRenderSystem.cs
- EditorRenderer.cs
- EditorContext.cs
- RegionTool.cs
- LeftSidebar.cs
- RightSidebar.cs
- PlayState.cs
- EnemySpawnSystem.cs
- ResourceSpawnSystem.cs
- BiomeSystem.cs
- E muitos outros...

### Build Status
✅ **0 errors, 0 warnings**

---

## 🔄 Próximas Melhorias (Opcional)

1. **Editor de Itens**
   - Tab "Items" na LeftSidebar
   - Brush tool para colocar itens
   - Lista de itens na RightSidebar

2. **Edição Inline de Regiões**
   - Double-click para renomear
   - Dropdown para mudar tipo
   - Editor de meta key-value completo

3. **ItemSpawn Regions**
   - Sistema de spawn baseado em ItemSpawn regions
   - Meta: itemId, intervalSeconds, maxActive

4. **Migração de Mapas Antigos**
   - Tool para converter tileSize 128 → 32
   - Ajustar posições de regiões

---

**Última atualização:** 2024

