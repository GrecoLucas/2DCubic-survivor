# Cube Survivor

<<<<<<< HEAD
Jogo 2D top-down desenvolvido com MonoGame seguindo os princípios **SOLID** e arquitetura **Entity-Component-System (ECS)** pura.

## 🎯 Características Principais

- ✅ **Arquitetura ECS Pura**: Separação clara entre dados (Components) e lógica (Systems)
- ✅ **Princípios SOLID**: Código extensível, modular e testável
- ✅ **Sistema de Registries**: Adicione novos elementos sem modificar código existente
- ✅ **Pattern Builder**: Construção fluente e limpa de entidades complexas
- ✅ **Fácil Extensão**: Adicione novos biomas, inimigos e armas em minutos

## 📚 Documentação

- **[Guia de Arquitetura](docs/ARCHITECTURE_GUIDE.md)**: Princípios SOLID, ECS e como estender o jogo
- **[Exemplos Práticos](docs/EXAMPLES.md)**: Código pronto para criar novos elementos
- **[Sistema de Construção](docs/CONSTRUCTION_SYSTEM.md)**: Como funciona o sistema de building
- **[Sistema de Anexos](docs/SOCKET_ATTACHMENT_SYSTEM.md)**: Sistema de sockets para armas
- **[Sistema de Mundo](docs/WORLD_SYSTEM.md)**: Biomas, recursos e spawn

## 🏗️ Estrutura do Projeto

```
CubeSurvivor/
├── Core/                      # Sistema ECS base
│   ├── Component.cs          # Classe base para componentes
│   ├── Entity.cs             # Classe de entidade
│   ├── GameSystem.cs         # Classe base para sistemas
│   ├── GameWorld.cs          # Gerenciador de entidades e sistemas
│   └── Registry/             # Sistema de registries genéricos
│       ├── IRegistry.cs
│       └── Registry.cs
│
├── Components/                # Componentes (APENAS DADOS)
│   ├── Combat/               # Componentes de combate
│   ├── Common/               # Componentes básicos (Transform, Sprite, etc)
│   ├── AI/                   # Componentes de IA
│   ├── Physics/              # Colisores e física
│   └── ...
│
├── Systems/                   # Sistemas (APENAS LÓGICA)
│   ├── Combat/               # Sistemas de combate
│   ├── Rendering/            # Sistemas de renderização
│   ├── World/                # Sistemas de mundo
│   ├── Input/                # Sistema de input
│   └── ...
│
├── Entities/                  # Factories para criar entidades
│   ├── Factories/
│   │   ├── IEnemyFactory.cs
│   │   ├── EnemyFactory.cs
│   │   ├── IWeaponFactory.cs
│   │   ├── WeaponFactory.cs
│   │   ├── IResourceFactory.cs
│   │   └── ResourceFactory.cs
│   └── Interfaces/
│
├── Game/
│   ├── Registries/           # Registries para extensibilidade
│   │   ├── BiomeRegistry.cs   # Registro de biomas
│   │   ├── EnemyRegistry.cs   # Registro de inimigos
│   │   └── WeaponRegistry.cs  # Registro de armas
│   └── ...
│
├── Builders/                  # Builders para criação fluente
│   ├── EntityBuilder.cs
│   └── GameWorldBuilder.cs
│
└── docs/                      # Documentação completa
    ├── ARCHITECTURE_GUIDE.md
    └── EXAMPLES.md
```

## 🚀 Início Rápido

### Requisitos

- .NET 8.0 ou superior
- MonoGame 3.8.1

### Executar o Jogo

```bash
dotnet restore
dotnet build
dotnet run
```

## 🎮 Controles

- **WASD**: Movimento
- **Mouse**: Mirar
- **Clique Esquerdo**: Atirar
- **E**: Coletar itens
- **I**: Abrir inventário
- **1-9**: Usar itens do inventário
- **Space**: Dash (se tiver o componente)

## ⚡ Adicionar Novos Elementos

### Adicionar Novo Inimigo (2 linhas de código!)

```csharp
// 1. Registrar definição
EnemyRegistry.Instance.Register("tank", new EnemyDefinition
{
    Name = "Tank Enemy",
    Health = 200f,
    Damage = 15f,
    Speed = 100f,
    AttackCooldown = 2f,
    Width = 60f,
    Height = 60f,
    ColorR = 100,
    ColorG = 100,
    ColorB = 100
});

// 2. Usar!
var enemy = enemyFactory.CreateEnemy(world, position, "tank");
```

### Adicionar Nova Arma

```csharp
WeaponRegistry.Instance.Register("shotgun", new WeaponDefinition
{
    Name = "Shotgun",
    Damage = 15f,
    FireRate = 0.8f,
    BulletSpeed = 400f,
    BulletsPerShot = 5,
    Spread = 30f
});
```

### Adicionar Novo Bioma

```csharp
BiomeRegistry.Instance.Register("desert", new BiomeDefinition
{
    Type = BiomeType.Desert,
    AllowsEnemySpawns = true,
    TreeDensity = 5,
    GoldDensity = 20,
    TextureName = "desert.png"
});
```

**Veja exemplos completos em [docs/EXAMPLES.md](docs/EXAMPLES.md)**

## 🏛️ Princípios de Design

### SOLID

- **S**ingle Responsibility: Cada classe tem uma única responsabilidade
- **O**pen/Closed: Extensível via Registries sem modificar código
- **L**iskov Substitution: Components e Systems são intercambiáveis
- **I**nterface Segregation: Interfaces pequenas e específicas
- **D**ependency Inversion: Depende de abstrações, não implementações

### Entity-Component-System (ECS)

```
Entity = Container de Components
Component = Dados puros (sem lógica)
System = Lógica de processamento
```

**Exemplo:**
```csharp
// Criar entidade com Builder Pattern
var player = new EntityBuilder(world, "Player")
    .WithTransform(new Vector2(100, 100))
    .WithSprite(Color.Blue, 32, 32, RenderLayer.Entities)
    .WithVelocity(250f)
    .WithHealth(100f)
    .WithCollider(32, 32, ColliderTag.Player)
    .Build();
```

## 🎯 Funcionalidades Atuais

### Sistemas Implementados
- ✅ Sistema de Movimento
- ✅ Sistema de IA (inimigos perseguem jogador)
- ✅ Sistema de Combate (balas, dano, colisão)
- ✅ Sistema de Inventário
- ✅ Sistema de Construção
- ✅ Sistema de Biomas
- ✅ Sistema de Spawn de Recursos
- ✅ Sistema de Renderização (com camadas)
- ✅ Sistema de UI

### Elementos do Jogo
- ✅ Múltiplos tipos de inimigos (configuráveis)
- ✅ Múltiplas armas (configuráveis)
- ✅ Sistema de recursos (madeira, ouro)
- ✅ Sistema de consumíveis (maçãs, cérebros)
- ✅ Biomas diferentes (floresta, caverna)
- ✅ Zonas seguras
- ✅ Sistema de loot

## 📖 Aprenda Mais

### Para Iniciantes
1. Leia [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md) para entender os princípios
2. Veja [EXAMPLES.md](docs/EXAMPLES.md) para exemplos práticos
3. Experimente adicionar um novo inimigo seguindo os exemplos

### Para Avançados
- Crie novos sistemas complexos
- Desenvolva mods usando os Registries
- Contribua com novos padrões de design

## 🤝 Contribuindo

Contribuições são bem-vindas! Por favor:
1. Siga os princípios SOLID
2. Mantenha a arquitetura ECS pura
3. Adicione documentação para novos recursos
4. Use os Registries para extensibilidade

## 📝 Licença

Este projeto está sob a licença MIT.

## 🙏 Agradecimentos

- MonoGame Framework
- Comunidade ECS
- Princípios SOLID de Robert C. Martin
=======
A top-down survival game built with MonoGame featuring an in-game map editor, chunked world streaming, and data-driven gameplay.

## Features

### 🎮 Gameplay
- Top-down survival mechanics
- Enemy waves and resource gathering
- Inventory and crafting system
- Region-based spawning (no hardcoding)
- Huge maps with chunk streaming

### 🗺️ Map System
- **Chunked storage** for massive worlds
- **Multi-layer** tiles and blocks
- **Region-based spawns** (Player, Enemy, Wood, Gold, SafeZones)
- **JSON serialization** for easy editing
- **Backward compatible** with legacy maps

### ✏️ In-Game Editor
- **Mouse-first UI** with left/right sidebars
- **Tools:** Brush, Eraser, Box Fill, Flood Fill, Picker, Regions
- **Live editing** with instant feedback
- **Undo/Redo** system (Ctrl+Z/Y)
- **Save & Exit** button
- **Region management** (create, focus, delete)

## Quick Start

```bash
# Build
dotnet build CubeSurvivor.csproj

# Run
dotnet run --project CubeSurvivor.csproj
```

## Controls

### Main Menu
- **Mouse-only** - Click buttons to navigate
- **Fullscreen** toggle in bottom-right
- **Play/Edit/New/Exit** buttons

### Editor
- **Left Sidebar:** Tools, Mode (Tiles/Blocks), Palette, Region Types
- **Right Sidebar:** Layers list, Regions list (with Focus/Delete)
- **Top Center:** "SAVE & EXIT" button
- **Left Mouse:** Paint/Place
- **Right Mouse:** Pan camera
- **WASD:** Move camera
- **Mouse Wheel:** Zoom
- **ESC:** Save and exit to menu
- **S:** Quick save
- **Ctrl+Z/Y:** Undo/Redo

### Gameplay
- **WASD:** Move
- **Mouse:** Aim
- **Left Click:** Shoot
- **E:** Pickup items
- **I:** Inventory

## Project Structure

```
src/
├── Game/
│   ├── Map/              # Map definition, loader, saver, streaming
│   ├── Editor/           # In-game editor (state, sidebar, tools)
│   ├── States/           # Game states (Menu, Play, Editor)
│   ├── Camera/           # Camera system
│   └── Configuration/    # Game config
├── Systems/              # ECS systems
│   ├── Core/             # Spawn, collision, etc.
│   ├── Rendering/        # Map & entity rendering
│   └── World/            # Resource spawns, harvesting
├── Components/           # ECS components
├── Entities/             # Entity factories
├── Inventory/            # Inventory system
└── Core/                 # ECS core, spatial hash
```

## Map Format
>>>>>>> c4d07b7 (new editor)

Maps are stored as JSON in `assets/maps/`:

```json
{
  "mapWidthTiles": 256,
  "mapHeightTiles": 256,
  "tileSizePx": 128,
  "chunkSizeTiles": 64,
  "tileLayers": [...],
  "blockLayers": [...],
  "regions": [
    {
      "id": "player_spawn_1",
      "type": "PlayerSpawn",
      "rectPx": [10000, 10000, 800, 800]
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
- `PlayerSpawn` - Where player starts
- `EnemySpawn` - Enemy spawning areas
- `WoodSpawn` - Wood resource spawning
- `GoldSpawn` - Gold resource spawning
- `SafeZone` - Safe areas (no enemies)

## Architecture

### ECS (Entity-Component-System)
Clean separation of data (Components) and logic (Systems).

### Chunk Streaming
Large maps divided into chunks. Only visible chunks are rendered. Blocks near camera spawn as ECS entities.

### Data-Driven
All spawns, regions, and map data loaded from JSON. Zero hardcoding.

### SOLID Principles
- Single Responsibility
- Open/Closed
- Liskov Substitution
- Interface Segregation
- Dependency Inversion

## Development

### Adding New Block Types
1. Add to `BlockType` enum in `MapDefinition.cs`
2. Add sprite/color in rendering system
3. Add to editor palette in `LeftSidebar.cs`
4. Update `WorldObjectFactory` if needed

### Adding New Region Types
1. Add to `RegionType` enum in `MapDefinition.cs`
2. Add spawn system logic if needed
3. Add to editor region picker in `LeftSidebar.cs`

## License

(Your license here)

## Credits

Built with:
- MonoGame
- C# / .NET 8
