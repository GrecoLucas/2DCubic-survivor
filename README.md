# Cube Survivor

Jogo 2D top-down desenvolvido com MonoGame seguindo os princípios **SOLID** e arquitetura **Entity-Component-System (ECS)** pura.

## 🎯 Características Principais

- ✅ **Arquitetura ECS Pura**: Separação clara entre dados (Components) e lógica (Systems)
- ✅ **Princípios SOLID**: Código extensível, modular e testável
- ✅ **Sistema de Registries**: Adicione novos elementos sem modificar código existente
- ✅ **Pattern Builder**: Construção fluente e limpa de entidades complexas
- ✅ **Fácil Extensão**: Adicione novos biomas, inimigos e armas em minutos
- ✅ **Editor de Mapas In-Game**: Crie e edite mapas com ferramentas completas
- ✅ **Sistema de Regiões**: Spawn baseado em regiões (sem hardcoding)
- ✅ **Chunked World Streaming**: Mapas grandes com streaming eficiente

## 📚 Documentação

Toda a documentação está em `docs/`:

- **[Índice Completo](docs/INDEX.md)**: Navegação por toda a documentação
- **[Guia de Arquitetura](docs/ARCHITECTURE_GUIDE.md)**: Princípios SOLID, ECS e como estender o jogo
- **[Exemplos Práticos](docs/EXAMPLES.md)**: Código pronto para criar novos elementos
- **[Referência Rápida](docs/QUICK_REFERENCE.md)**: Checklists e comandos rápidos
- **[Guia de Início Rápido](docs/QUICK_START.md)**: Para novos desenvolvedores
- **[Histórico de Desenvolvimento](docs/DEVELOPMENT_HISTORY.md)**: Refatorações e melhorias importantes
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
│
├── Components/                # Componentes (APENAS DADOS)
│   ├── Combat/               # Componentes de combate
│   ├── Common/               # Componentes básicos (Transform, Sprite, etc)
│   ├── AI/                   # Componentes de IA
│   └── Physics/              # Colisores e física
│
├── Systems/                   # Sistemas (APENAS LÓGICA)
│   ├── Combat/               # Sistemas de combate
│   ├── Rendering/            # Sistemas de renderização
│   ├── World/                # Sistemas de mundo
│   └── Input/                # Sistema de input
│
├── Game/
│   ├── Map/                  # Sistema de mapas (chunked, multi-layer)
│   ├── Editor/               # Editor de mapas in-game
│   ├── States/               # Estados do jogo (Menu, Play, Editor)
│   └── Registries/           # Registries para extensibilidade
│
└── docs/                      # Documentação completa
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

### Main Menu
- **Mouse-only** - Click buttons to navigate
- **Fullscreen** toggle in bottom-right
- **Play/Edit/New/Exit** buttons

### Editor
- **Left Sidebar:** Tools, Layers, Palettes (Tiles/Blocks/Items), Region Types
- **Right Sidebar:** Layers list, Regions list (with Focus/Delete), Region Meta Editor
- **Top Center:** "SAVE & EXIT" button
- **Left Mouse:** Paint/Place
- **Right Mouse:** Pan camera
- **WASD:** Move camera
- **Mouse Wheel:** Zoom
- **ESC:** Pause menu
- **S:** Quick save
- **Delete/Backspace:** Delete selected region

### Gameplay
- **WASD:** Move
- **Mouse:** Aim
- **Left Click:** Shoot
- **E:** Pickup items
- **I:** Inventory
- **1-9:** Use inventory items
- **Space:** Dash (if available)

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

## 🗺️ Sistema de Mapas

Maps are stored as JSON in `assets/maps/`:

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
- `PlayerSpawn` - Where player starts
- `EnemySpawn` - Enemy spawning areas
- `GoldSpawn` - Gold resource spawning
- `WoodSpawn` - Wood resource spawning
- `AppleSpawn` - Apple resource spawning
- `TreeSpawn` - Tree block spawning
- `ItemSpawn` - Generic item spawning
- `SafeZone` - Safe areas (no enemies)
- `Biome` - Biome definition

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
- ✅ Editor de Mapas In-Game
- ✅ Sistema de Regiões (spawn baseado em regiões)

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
1. Leia [README.md](README.md) para visão geral
2. Veja [QUICK_START.md](docs/QUICK_START.md) para começar rápido
3. Leia [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md) para entender os princípios
4. Veja [EXAMPLES.md](docs/EXAMPLES.md) para exemplos práticos
5. Experimente adicionar um novo inimigo seguindo os exemplos

### Para Avançados
- Crie novos sistemas complexos
- Desenvolva mods usando os Registries
- Contribua com novos padrões de design
- Veja [DEVELOPMENT_HISTORY.md](docs/DEVELOPMENT_HISTORY.md) para histórico de refatorações

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
