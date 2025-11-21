# Guia de Arquitetura e Extensibilidade

## Índice
1. [Princípios SOLID Implementados](#princípios-solid)
2. [Arquitetura ECS](#arquitetura-ecs)
3. [Como Adicionar Novos Elementos](#extensões)
   - [Adicionar Novo Bioma](#novo-bioma)
   - [Adicionar Novo Inimigo](#novo-inimigo)
   - [Adicionar Nova Arma](#nova-arma)
   - [Adicionar Novo Componente](#novo-componente)
   - [Adicionar Novo Sistema](#novo-sistema)

---

## Princípios SOLID Implementados {#princípios-solid}

### Single Responsibility Principle (SRP)
Cada classe tem **uma única responsabilidade**:
- **Components**: Apenas armazenam dados
- **Systems**: Apenas processam lógica específica
- **Factories**: Apenas criam entidades
- **Registries**: Apenas gerenciam registros

**Exemplo:**
```csharp
// ✅ BOM - Componente só armazena dados
public class HealthComponent : Component
{
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; }
}

// ❌ RUIM - Componente não deve ter lógica de jogo
public class HealthComponent : Component
{
    public void TakeDamage(float amount) { } // Isso deve estar em um System!
}
```

### Open/Closed Principle (OCP)
Classes estão **abertas para extensão, fechadas para modificação**:
- Registries permitem adicionar novos tipos sem modificar código existente
- Interfaces permitem novos comportamentos sem alterar implementações

**Exemplo:**
```csharp
// ✅ BOM - Adicionar novo inimigo sem modificar código
EnemyRegistry.Instance.Register("boss", new EnemyDefinition {
    Name = "Boss",
    Health = 500f,
    // ... configurações
});

// Não é necessário modificar EnemyFactory ou EnemySystem!
```

### Liskov Substitution Principle (LSP)
Subclasses podem substituir classes base sem quebrar funcionalidade:
- Todos os Components podem ser usados como `Component`
- Todos os Systems podem ser usados como `GameSystem`

### Interface Segregation Principle (ISP)
Interfaces são pequenas e específicas:
- `IEnemyFactory` - apenas criação de inimigos
- `IWeaponFactory` - apenas criação de armas
- `IRegistry<TKey, TValue>` - operações básicas de registro

### Dependency Inversion Principle (DIP)
Dependemos de **abstrações**, não de implementações concretas:
```csharp
// ✅ BOM - Depende de interface
public class EnemySpawnSystem
{
    private readonly IEnemyFactory _enemyFactory;
    
    public EnemySpawnSystem(IEnemyFactory enemyFactory) {
        _enemyFactory = enemyFactory;
    }
}

// ❌ RUIM - Depende de implementação concreta
public class EnemySpawnSystem
{
    private readonly EnemyFactory _enemyFactory; // Acoplamento forte!
}
```

---

## Arquitetura ECS {#arquitetura-ecs}

### Entity-Component-System Pattern

**Entity**: Container de componentes (apenas um ID e lista de componentes)
**Component**: Dados puros (sem lógica)
**System**: Lógica de processamento (opera sobre componentes)

```
┌─────────────┐
│   Entity    │
├─────────────┤
│ Components: │
│  • Transform│
│  • Sprite   │
│  • Health   │
│  • AI       │
└─────────────┘
       ↓
┌─────────────┐
│  Systems    │
├─────────────┤
│ AISystem    │ → Processa todas entities com AIComponent
│ RenderSys   │ → Processa todas entities com SpriteComponent
│ HealthSys   │ → Processa todas entities com HealthComponent
└─────────────┘
```

### Fluxo de Processamento
1. Entities são criadas com componentes
2. Systems processam entities que têm componentes específicos
3. Cada frame, `GameWorld.Update()` chama todos os systems

---

## Como Adicionar Novos Elementos {#extensões}

### Adicionar Novo Bioma {#novo-bioma}

#### 1. Adicionar tipo ao enum (se necessário)
```csharp
// src/Game/World/Biomes/BiomeType.cs
public enum BiomeType
{
    Unknown = 0,
    Forest = 1,
    Cave = 2,
    Desert = 3,  // ← NOVO
}
```

#### 2. Registrar no BiomeRegistry
```csharp
// Em qualquer lugar do código, antes de usar:
BiomeRegistry.Instance.Register("desert", new BiomeDefinition
{
    Type = BiomeType.Desert,
    AllowsEnemySpawns = true,
    TreeDensity = 5,      // Poucos recursos de madeira
    GoldDensity = 20,     // Mais recursos de ouro
    TextureName = "desert.png"
});
```

#### 3. Usar no arquivo world.json
```json
{
  "biomes": [
    {
      "x": 0,
      "y": 0,
      "width": 2000,
      "height": 2000,
      "type": "Desert",
      "allowsEnemySpawns": true,
      "treeDensity": 5,
      "goldDensity": 20,
      "texture": "desert.png"
    }
  ]
}
```

**Pronto!** Nenhuma modificação em código existente necessária.

---

### Adicionar Novo Inimigo {#novo-inimigo}

#### 1. Registrar definição do inimigo
```csharp
// No início do jogo ou em um arquivo de inicialização:
EnemyRegistry.Instance.Register("sniper", new EnemyDefinition
{
    Name = "Sniper Enemy",
    Health = 40f,
    Damage = 30f,          // Muito dano
    Speed = 80f,           // Lento
    AttackCooldown = 3f,   // Ataque lento mas poderoso
    Width = 35f,
    Height = 35f,
    ColorR = 0,
    ColorG = 150,
    ColorB = 0,
    TextureName = "sniper.png", // Opcional
    LootTable = new[]
    {
        new LootEntry { ItemType = "brain", DropChance = 0.3f },
        new LootEntry { ItemType = "gold", DropChance = 0.5f }
    }
});
```

#### 2. Spawnar o novo inimigo
```csharp
// Usar a factory com o tipo registrado:
var enemy = enemyFactory.CreateEnemy(world, position, "sniper");
```

**Sem modificações** em `EnemyFactory`, `EnemySystem`, ou qualquer outro código!

---

### Adicionar Nova Arma {#nova-arma}

#### 1. Registrar definição da arma
```csharp
WeaponRegistry.Instance.Register("machinegun", new WeaponDefinition
{
    Name = "Machine Gun",
    Damage = 5f,
    FireRate = 0.1f,       // Muito rápido
    BulletSpeed = 700f,
    BulletSize = 4f,
    Width = 28f,
    Height = 8f,
    TextureName = "machinegun.png",
    BulletsPerShot = 1,
    Spread = 5f            // Dispersão das balas
});
```

#### 2. Criar arma no mundo
```csharp
var weaponFactory = new WeaponFactory(textureManager);
var machinegun = weaponFactory.CreateWeapon(world, position, "machinegun");
```

---

### Adicionar Novo Componente {#novo-componente}

#### 1. Criar classe do componente
```csharp
// src/Components/Combat/ShieldComponent.cs
using CubeSurvivor.Core;

namespace CubeSurvivor.Components.Combat
{
    /// <summary>
    /// Componente para escudo que absorve dano
    /// Princípio: SRP - Apenas armazena dados do escudo
    /// </summary>
    public class ShieldComponent : Component
    {
        public float ShieldStrength { get; set; }
        public float MaxShieldStrength { get; }
        public float RechargeRate { get; set; }
        public float RechargeCooldown { get; set; }

        public ShieldComponent(float maxShieldStrength, float rechargeRate = 10f)
        {
            MaxShieldStrength = maxShieldStrength;
            ShieldStrength = maxShieldStrength;
            RechargeRate = rechargeRate;
            RechargeCooldown = 0f;
        }
    }
}
```

#### 2. Criar sistema para processar o componente
```csharp
// src/Systems/Combat/ShieldSystem.cs
using CubeSurvivor.Core;
using CubeSurvivor.Components.Combat;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.Combat
{
    /// <summary>
    /// Sistema que gerencia escudos
    /// Princípio: SRP - Responsável apenas por lógica de escudos
    /// </summary>
    public sealed class ShieldSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var entity in World.GetEntitiesWithComponent<ShieldComponent>())
            {
                var shield = entity.GetComponent<ShieldComponent>();

                // Recarregar escudo após cooldown
                if (shield.ShieldStrength < shield.MaxShieldStrength)
                {
                    shield.RechargeCooldown -= deltaTime;
                    
                    if (shield.RechargeCooldown <= 0)
                    {
                        shield.ShieldStrength = Math.Min(
                            shield.ShieldStrength + shield.RechargeRate * deltaTime,
                            shield.MaxShieldStrength
                        );
                    }
                }
            }
        }
    }
}
```

#### 3. Adicionar sistema ao GameWorld
```csharp
world.AddSystem(new ShieldSystem());
```

#### 4. Adicionar componente a entidades
```csharp
// Usando builder:
var player = new EntityBuilder(world, "Player")
    .WithTransform(new Vector2(100, 100))
    .WithHealth(100f)
    .WithComponent(new ShieldComponent(50f, rechargeRate: 15f))
    .Build();
```

---

### Adicionar Novo Sistema {#novo-sistema}

#### 1. Criar classe do sistema
```csharp
// src/Systems/Special/TeleportSystem.cs
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.Special
{
    /// <summary>
    /// Sistema que gerencia teleporte de entidades
    /// Princípio: SRP - Responsável apenas por lógica de teleporte
    /// </summary>
    public sealed class TeleportSystem : GameSystem
    {
        private readonly Rectangle _worldBounds;

        public TeleportSystem(Rectangle worldBounds)
        {
            _worldBounds = worldBounds;
        }

        public override void Update(GameTime gameTime)
        {
            foreach (var entity in World.GetEntitiesWithComponent<TeleportComponent>())
            {
                var teleport = entity.GetComponent<TeleportComponent>();
                var transform = entity.GetComponent<TransformComponent>();

                if (transform == null) continue;

                // Processar cooldown
                teleport.CurrentCooldown -= (float)gameTime.ElapsedGameTime.TotalSeconds;

                // Verificar se pode teleportar
                if (teleport.ShouldTeleport && teleport.CurrentCooldown <= 0)
                {
                    transform.Position = teleport.Destination;
                    teleport.ShouldTeleport = false;
                    teleport.CurrentCooldown = teleport.Cooldown;
                }
            }
        }
    }
}
```

#### 2. Criar componente associado (se necessário)
```csharp
// src/Components/Special/TeleportComponent.cs
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components
{
    public class TeleportComponent : Component
    {
        public Vector2 Destination { get; set; }
        public bool ShouldTeleport { get; set; }
        public float Cooldown { get; set; }
        public float CurrentCooldown { get; set; }

        public TeleportComponent(float cooldown = 5f)
        {
            Cooldown = cooldown;
            CurrentCooldown = 0f;
        }
    }
}
```

#### 3. Adicionar ao GameWorld
```csharp
world.AddSystem(new TeleportSystem(worldBounds));
```

---

## Padrões de Design Utilizados

### Factory Pattern
Usado para criar entidades de forma consistente:
- `EnemyFactory`
- `WeaponFactory`
- `ResourceFactory`

### Builder Pattern
Usado para construir entidades complexas:
```csharp
var boss = new EntityBuilder(world, "Boss")
    .WithTransform(position)
    .WithSprite(Color.DarkRed, 80, 80, RenderLayer.Entities)
    .WithHealth(1000f)
    .WithAI(100f)
    .WithEnemy(50f, 2f)
    .WithLootDrop(("brain", 1.0f), ("gold", 0.8f))
    .Build();
```

### Registry Pattern
Usado para registrar e recuperar definições:
- `EnemyRegistry`
- `WeaponRegistry`
- `BiomeRegistry`

### Singleton Pattern
Usado para registries globais:
```csharp
EnemyRegistry.Instance.Register("type", definition);
```

---

## Boas Práticas

### ✅ DO (Faça)
- Mantenha componentes **sem lógica** (apenas dados)
- Coloque toda lógica em **Systems**
- Use **Registries** para extensibilidade
- Use **Builders** para criação complexa
- Dependa de **interfaces**, não implementações
- Uma responsabilidade por classe
- Documente com comentários XML

### ❌ DON'T (Não Faça)
- Não adicione lógica em Components
- Não acople Systems diretamente
- Não hardcode valores (use Registries)
- Não crie dependências circulares
- Não misture responsabilidades
- Não ignore os princípios SOLID

---

## Exemplo Completo: Adicionando um Boss

```csharp
// 1. Registrar definição
EnemyRegistry.Instance.Register("dragon_boss", new EnemyDefinition
{
    Name = "Dragon Boss",
    Health = 2000f,
    Damage = 50f,
    Speed = 120f,
    AttackCooldown = 2.5f,
    Width = 100f,
    Height = 100f,
    ColorR = 200,
    ColorG = 0,
    ColorB = 0,
    TextureName = "dragon.png",
    LootTable = new[]
    {
        new LootEntry { ItemType = "gold", DropChance = 1.0f },
        new LootEntry { ItemType = "brain", DropChance = 1.0f }
    }
});

// 2. Criar o boss usando a factory
var enemyFactory = new EnemyFactory(textureManager);
var boss = enemyFactory.CreateEnemy(world, spawnPosition, "dragon_boss");

// 3. Adicionar componentes extras se necessário
boss.AddComponent(new ShieldComponent(500f));
boss.AddComponent(new XpComponent { XpValue = 1000 });

// Pronto! O boss funciona com todos os sistemas existentes automaticamente.
```

---

## Estrutura de Arquivos Recomendada

```
src/
├── Core/                    # Núcleo ECS
│   ├── Component.cs
│   ├── Entity.cs
│   ├── GameSystem.cs
│   ├── GameWorld.cs
│   └── Registry/
│       ├── IRegistry.cs
│       └── Registry.cs
│
├── Components/              # Todos os componentes (dados)
│   ├── Combat/
│   ├── Common/
│   ├── AI/
│   └── ...
│
├── Systems/                 # Todos os sistemas (lógica)
│   ├── Combat/
│   ├── Rendering/
│   ├── World/
│   └── ...
│
├── Entities/
│   └── Factories/           # Factories para criar entidades
│       ├── IEnemyFactory.cs
│       ├── EnemyFactory.cs
│       ├── IWeaponFactory.cs
│       ├── WeaponFactory.cs
│       └── ...
│
├── Game/
│   └── Registries/          # Registries para extensibilidade
│       ├── EnemyRegistry.cs
│       ├── WeaponRegistry.cs
│       └── BiomeRegistry.cs
│
└── Builders/                # Builders para construção
    ├── EntityBuilder.cs
    └── GameWorldBuilder.cs
```

---

## Conclusão

Esta arquitetura garante:
- ✅ **Extensibilidade**: Adicione novos elementos facilmente
- ✅ **Manutenibilidade**: Código organizado e claro
- ✅ **Testabilidade**: Componentes e sistemas independentes
- ✅ **Escalabilidade**: Suporta crescimento do projeto
- ✅ **SOLID**: Todos os princípios aplicados
- ✅ **ECS Puro**: Separação clara de dados e lógica
