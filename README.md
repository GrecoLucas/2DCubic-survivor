# Cube Survivor

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

- **W** - Mover para cima
- **S** - Mover para baixo
- **A** - Mover para esquerda
- **D** - Mover para direita

## Arquitetura ECS

### Componentes
- **TransformComponent**: Posição, rotação e escala
- **SpriteComponent**: Cor e tamanho visual
- **VelocityComponent**: Velocidade de movimento
- **InputComponent**: Marca entidades controláveis pelo jogador

### Sistemas
- **InputSystem**: Processa input do teclado e atualiza velocidades
- **MovementSystem**: Aplica velocidade às posições
- **RenderSystem**: Renderiza todas as entidades visíveis

### Entidades
- **Player**: Quadrado azul controlado pelo jogador

## Próximas Expansões Possíveis

- Inimigos e IA
- Sistema de colisão
- Sistema de combate
- Power-ups e itens
- Sistema de câmera
- Efeitos visuais e partículas
- Sistema de ondas/waves
- Pontuação e UI
