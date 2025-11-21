# Exemplos Práticos de Uso

## Índice
1. [Inicialização do Jogo](#inicialização)
2. [Criando um Novo Tipo de Inimigo](#novo-inimigo)
3. [Criando um Novo Sistema de Habilidades](#sistema-habilidades)
4. [Criando um Mod/Extensão](#mod)

---

## Inicialização do Jogo {#inicialização}

### Exemplo de setup completo do GameWorld

```csharp
using CubeSurvivor.Builders;
using CubeSurvivor.Systems;
using CubeSurvivor.Systems.Combat;
using CubeSurvivor.Systems.Rendering;
using CubeSurvivor.Entities;
using CubeSurvivor.Game.Registries;

public class GameInitializer
{
    public void Initialize(Game game)
    {
        // 1. Registrar tipos customizados ANTES de criar o mundo
        RegisterCustomEnemies();
        RegisterCustomWeapons();
        RegisterCustomBiomes();

        // 2. Criar o mundo usando Builder Pattern
        var world = new GameWorldBuilder()
            // Core Systems
            .AddSystem(new MovementSystem())
            .AddSystem(new AISystem())
            .AddSystem(new CollisionSystem())
            
            // Combat Systems
            .AddSystem(new BulletSystem())
            .AddSystem(new DeathSystem())
            
            // World Systems
            .AddSystem(new BiomeSystem())
            .AddSystem(new EnemySpawnSystem(
                spawnArea: worldBounds,
                enemyFactory: new EnemyFactory(textureManager),
                spawnInterval: 2f,
                maxEnemies: 50,
                exclusionProvider: safeZoneManager,
                biomeSystem: biomeSystem
            ))
            
            // Rendering Systems (ordem importa!)
            .AddSystem(new RenderSystem(spriteBatch, camera))
            .AddSystem(new UISystem(spriteBatch, camera))
            .Build();

        // 3. Criar entidades iniciais
        CreatePlayer(world);
        CreateWorldObjects(world);
    }

    private void RegisterCustomEnemies()
    {
        // Inimigo voador
        EnemyRegistry.Instance.Register("flyer", new EnemyDefinition
        {
            Name = "Flying Enemy",
            Health = 40f,
            Damage = 8f,
            Speed = 200f,
            AttackCooldown = 0.9f,
            Width = 35f,
            Height = 35f,
            ColorR = 100,
            ColorG = 100,
            ColorB = 255,
            LootTable = new[]
            {
                new LootEntry { ItemType = "brain", DropChance = 0.15f }
            }
        });

        // Boss final
        EnemyRegistry.Instance.Register("final_boss", new EnemyDefinition
        {
            Name = "Final Boss",
            Health = 5000f,
            Damage = 100f,
            Speed = 150f,
            AttackCooldown = 3f,
            Width = 120f,
            Height = 120f,
            ColorR = 150,
            ColorG = 0,
            ColorB = 150,
            TextureName = "final_boss.png",
            LootTable = new[]
            {
                new LootEntry { ItemType = "gold", DropChance = 1.0f },
                new LootEntry { ItemType = "brain", DropChance = 1.0f }
            }
        });
    }

    private void RegisterCustomWeapons()
    {
        // Arco e flecha
        WeaponRegistry.Instance.Register("bow", new WeaponDefinition
        {
            Name = "Bow",
            Damage = 20f,
            FireRate = 1.2f,
            BulletSpeed = 600f,
            BulletSize = 8f,
            Width = 30f,
            Height = 6f,
            TextureName = "bow.png",
            BulletsPerShot = 1,
            Spread = 0f
        });

        // Lança-chamas
        WeaponRegistry.Instance.Register("flamethrower", new WeaponDefinition
        {
            Name = "Flamethrower",
            Damage = 3f,
            FireRate = 0.05f,  // Muito rápido
            BulletSpeed = 200f,
            BulletSize = 10f,
            Width = 32f,
            Height = 10f,
            TextureName = "flamethrower.png",
            BulletsPerShot = 3,
            Spread = 15f
        });
    }

    private void RegisterCustomBiomes()
    {
        BiomeRegistry.Instance.Register("volcano", new BiomeDefinition
        {
            Type = BiomeType.Cave, // Reusa tipo existente
            AllowsEnemySpawns = true,
            TreeDensity = 0,
            GoldDensity = 60,
            TextureName = "volcano.png"
        });
    }

    private void CreatePlayer(GameWorld world)
    {
        var player = new EntityBuilder(world, "Player")
            .WithTransform(new Vector2(2000, 2000))
            .WithSprite(Color.Blue, 32, 32, RenderLayer.Entities)
            .WithVelocity(250f)
            .WithHealth(100f)
            .WithCollider(32, 32, ColliderTag.Player)
            .WithComponent(new PlayerInputComponent())
            .WithComponent(new InventoryComponent(new PlayerInventory()))
            .Build();
    }

    private void CreateWorldObjects(GameWorld world)
    {
        var resourceFactory = new ResourceFactory(textureManager);
        
        // Spawnar alguns recursos iniciais
        for (int i = 0; i < 50; i++)
        {
            var position = GetRandomPosition();
            resourceFactory.CreateResource(world, position, "wood");
        }
    }
}
```

---

## Criando um Novo Tipo de Inimigo {#novo-inimigo}

### Cenário: Boss que spawna minions

#### Passo 1: Criar componente para o boss
```csharp
// src/Components/Combat/BossComponent.cs
using CubeSurvivor.Core;

namespace CubeSurvivor.Components.Combat
{
    /// <summary>
    /// Componente para bosses que podem spawnar minions
    /// </summary>
    public class BossComponent : Component
    {
        public string MinionType { get; set; }
        public float SpawnInterval { get; set; }
        public int MaxMinions { get; set; }
        public float CurrentSpawnTimer { get; set; }
        public int CurrentMinions { get; set; }

        public BossComponent(string minionType, float spawnInterval = 10f, int maxMinions = 5)
        {
            MinionType = minionType;
            SpawnInterval = spawnInterval;
            MaxMinions = maxMinions;
            CurrentSpawnTimer = 0f;
            CurrentMinions = 0;
        }
    }
}
```

#### Passo 2: Criar sistema para gerenciar bosses
```csharp
// src/Systems/Combat/BossSystem.cs
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using CubeSurvivor.Components.Combat;
using CubeSurvivor.Entities.Factories;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.Combat
{
    /// <summary>
    /// Sistema que gerencia comportamentos especiais de bosses
    /// </summary>
    public sealed class BossSystem : GameSystem
    {
        private readonly IEnemyFactory _enemyFactory;

        public BossSystem(IEnemyFactory enemyFactory)
        {
            _enemyFactory = enemyFactory;
        }

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var boss in World.GetEntitiesWithComponent<BossComponent>())
            {
                var bossComp = boss.GetComponent<BossComponent>();
                var transform = boss.GetComponent<TransformComponent>();
                
                if (transform == null) continue;

                // Atualizar timer de spawn
                bossComp.CurrentSpawnTimer += deltaTime;

                // Spawnar minion se possível
                if (bossComp.CurrentSpawnTimer >= bossComp.SpawnInterval &&
                    bossComp.CurrentMinions < bossComp.MaxMinions)
                {
                    SpawnMinion(bossComp, transform.Position);
                    bossComp.CurrentSpawnTimer = 0f;
                    bossComp.CurrentMinions++;
                }
            }
        }

        private void SpawnMinion(BossComponent bossComp, Vector2 bossPosition)
        {
            // Spawnar minion perto do boss
            var offset = new Vector2(
                (float)(new System.Random().NextDouble() * 200 - 100),
                (float)(new System.Random().NextDouble() * 200 - 100)
            );

            var minion = _enemyFactory.CreateEnemy(
                World, 
                bossPosition + offset, 
                bossComp.MinionType
            );

            // Adicionar componente para rastrear que é minion deste boss
            minion.AddComponent(new MinionComponent { BossEntity = boss });
        }
    }
}

// Componente auxiliar
public class MinionComponent : Component
{
    public Entity BossEntity { get; set; }
}
```

#### Passo 3: Registrar e usar o boss
```csharp
// Registrar definição do boss
EnemyRegistry.Instance.Register("necromancer", new EnemyDefinition
{
    Name = "Necromancer Boss",
    Health = 1500f,
    Damage = 30f,
    Speed = 100f,
    AttackCooldown = 2f,
    Width = 80f,
    Height = 80f,
    ColorR = 80,
    ColorG = 0,
    ColorB = 80,
    TextureName = "necromancer.png",
    LootTable = new[]
    {
        new LootEntry { ItemType = "gold", DropChance = 1.0f },
        new LootEntry { ItemType = "brain", DropChance = 0.5f }
    }
});

// Registrar minion
EnemyRegistry.Instance.Register("skeleton", new EnemyDefinition
{
    Name = "Skeleton",
    Health = 25f,
    Damage = 5f,
    Speed = 180f,
    AttackCooldown = 0.8f,
    Width = 30f,
    Height = 30f,
    ColorR = 200,
    ColorG = 200,
    ColorB = 200,
    LootTable = new[] { new LootEntry { ItemType = "brain", DropChance = 0.05f } }
});

// Criar o boss
var enemyFactory = new EnemyFactory(textureManager);
var boss = enemyFactory.CreateEnemy(world, spawnPosition, "necromancer");

// Adicionar comportamento de boss
boss.AddComponent(new BossComponent(
    minionType: "skeleton",
    spawnInterval: 8f,
    maxMinions: 6
));

// Adicionar BossSystem ao mundo
world.AddSystem(new BossSystem(enemyFactory));
```

---

## Criando um Sistema de Habilidades {#sistema-habilidades}

### Cenário: Sistema de dash para o jogador

#### Passo 1: Criar componente de dash
```csharp
// src/Components/Abilities/DashComponent.cs
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Components.Abilities
{
    public class DashComponent : Component
    {
        public float DashSpeed { get; set; }
        public float DashDuration { get; set; }
        public float DashCooldown { get; set; }
        
        public bool IsDashing { get; set; }
        public float CurrentDashTime { get; set; }
        public float CurrentCooldown { get; set; }
        public Vector2 DashDirection { get; set; }

        public DashComponent(float dashSpeed = 800f, float dashDuration = 0.2f, float dashCooldown = 2f)
        {
            DashSpeed = dashSpeed;
            DashDuration = dashDuration;
            DashCooldown = dashCooldown;
            CurrentCooldown = 0f;
        }

        public bool CanDash => !IsDashing && CurrentCooldown <= 0;
    }
}
```

#### Passo 2: Criar sistema de dash
```csharp
// src/Systems/Abilities/DashSystem.cs
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using CubeSurvivor.Components.Abilities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CubeSurvivor.Systems.Abilities
{
    public sealed class DashSystem : GameSystem
    {
        private KeyboardState _previousKeyboardState;

        public override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            var currentKeyboardState = Keyboard.GetState();

            foreach (var entity in World.GetEntitiesWithComponent<DashComponent>())
            {
                var dash = entity.GetComponent<DashComponent>();
                var transform = entity.GetComponent<TransformComponent>();
                var velocity = entity.GetComponent<VelocityComponent>();
                var input = entity.GetComponent<PlayerInputComponent>();

                if (transform == null || velocity == null) continue;

                // Atualizar cooldown
                if (dash.CurrentCooldown > 0)
                {
                    dash.CurrentCooldown -= deltaTime;
                }

                // Processar dash ativo
                if (dash.IsDashing)
                {
                    dash.CurrentDashTime += deltaTime;

                    // Mover na direção do dash
                    transform.Position += dash.DashDirection * dash.DashSpeed * deltaTime;

                    // Terminar dash
                    if (dash.CurrentDashTime >= dash.DashDuration)
                    {
                        dash.IsDashing = false;
                        dash.CurrentDashTime = 0f;
                        dash.CurrentCooldown = dash.DashCooldown;
                    }
                }
                // Iniciar dash (tecla Space)
                else if (input != null && 
                         currentKeyboardState.IsKeyDown(Keys.Space) && 
                         _previousKeyboardState.IsKeyUp(Keys.Space) &&
                         dash.CanDash)
                {
                    // Determinar direção baseada no movimento atual
                    Vector2 dashDirection = input.LastMoveDirection;
                    
                    if (dashDirection != Vector2.Zero)
                    {
                        dashDirection.Normalize();
                        dash.DashDirection = dashDirection;
                        dash.IsDashing = true;
                        dash.CurrentDashTime = 0f;
                    }
                }
            }

            _previousKeyboardState = currentKeyboardState;
        }
    }
}
```

#### Passo 3: Adicionar ao jogador
```csharp
// Criar jogador com habilidade de dash
var player = new EntityBuilder(world, "Player")
    .WithTransform(startPosition)
    .WithSprite(Color.Blue, 32, 32, RenderLayer.Entities)
    .WithVelocity(250f)
    .WithHealth(100f)
    .WithCollider(32, 32, ColliderTag.Player)
    .WithComponent(new PlayerInputComponent())
    .WithComponent(new DashComponent(
        dashSpeed: 1000f,
        dashDuration: 0.15f,
        dashCooldown: 1.5f
    ))
    .Build();

// Adicionar DashSystem ao mundo
world.AddSystem(new DashSystem());
```

---

## Criando um Mod/Extensão {#mod}

### Cenário: Mod que adiciona sistema de classes de personagem

#### Estrutura do Mod
```
Mods/
└── CharacterClasses/
    ├── Components/
    │   └── CharacterClassComponent.cs
    ├── Systems/
    │   └── CharacterClassSystem.cs
    ├── Registries/
    │   └── CharacterClassRegistry.cs
    └── CharacterClassesMod.cs
```

#### Código do Mod

```csharp
// Mods/CharacterClasses/Components/CharacterClassComponent.cs
using CubeSurvivor.Core;

namespace Mods.CharacterClasses.Components
{
    public class CharacterClassComponent : Component
    {
        public string ClassName { get; set; }
        public float HealthMultiplier { get; set; }
        public float SpeedMultiplier { get; set; }
        public float DamageMultiplier { get; set; }

        public CharacterClassComponent(string className)
        {
            ClassName = className;
        }
    }
}

// Mods/CharacterClasses/Registries/CharacterClassRegistry.cs
using CubeSurvivor.Core.Registry;

namespace Mods.CharacterClasses.Registries
{
    public class CharacterClassDefinition
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public float HealthMultiplier { get; set; } = 1.0f;
        public float SpeedMultiplier { get; set; } = 1.0f;
        public float DamageMultiplier { get; set; } = 1.0f;
        public string[] StartingItems { get; set; }
    }

    public sealed class CharacterClassRegistry : Registry<string, CharacterClassDefinition>
    {
        private static readonly Lazy<CharacterClassRegistry> _instance = 
            new Lazy<CharacterClassRegistry>(() => new CharacterClassRegistry());

        public static CharacterClassRegistry Instance => _instance.Value;

        private CharacterClassRegistry()
        {
            RegisterDefaultClasses();
        }

        private void RegisterDefaultClasses()
        {
            Register("warrior", new CharacterClassDefinition
            {
                Name = "Warrior",
                Description = "Tank class with high health",
                HealthMultiplier = 1.5f,
                SpeedMultiplier = 0.8f,
                DamageMultiplier = 1.2f,
                StartingItems = new[] { "sword", "shield" }
            });

            Register("ranger", new CharacterClassDefinition
            {
                Name = "Ranger",
                Description = "Fast class with ranged weapons",
                HealthMultiplier = 0.8f,
                SpeedMultiplier = 1.3f,
                DamageMultiplier = 1.1f,
                StartingItems = new[] { "bow" }
            });

            Register("mage", new CharacterClassDefinition
            {
                Name = "Mage",
                Description = "Magic user with high damage",
                HealthMultiplier = 0.7f,
                SpeedMultiplier = 1.0f,
                DamageMultiplier = 1.5f,
                StartingItems = new[] { "staff" }
            });
        }
    }
}

// Mods/CharacterClasses/Systems/CharacterClassSystem.cs
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using Mods.CharacterClasses.Components;
using Mods.CharacterClasses.Registries;
using Microsoft.Xna.Framework;

namespace Mods.CharacterClasses.Systems
{
    public sealed class CharacterClassSystem : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            // Aplicar multiplicadores de classe
            foreach (var entity in World.GetEntitiesWithComponent<CharacterClassComponent>())
            {
                var classComp = entity.GetComponent<CharacterClassComponent>();
                
                if (!CharacterClassRegistry.Instance.Contains(classComp.ClassName))
                    continue;

                var classDef = CharacterClassRegistry.Instance.Get(classComp.ClassName);

                // Aplicar multiplicadores uma vez (marcar com flag)
                if (classComp.HealthMultiplier == 0)
                {
                    ApplyClassBonuses(entity, classDef);
                    classComp.HealthMultiplier = classDef.HealthMultiplier;
                    classComp.SpeedMultiplier = classDef.SpeedMultiplier;
                    classComp.DamageMultiplier = classDef.DamageMultiplier;
                }
            }
        }

        private void ApplyClassBonuses(Entity entity, CharacterClassDefinition classDef)
        {
            // Modificar health
            var health = entity.GetComponent<HealthComponent>();
            if (health != null)
            {
                health.MaxHealth *= classDef.HealthMultiplier;
                health.CurrentHealth = health.MaxHealth;
            }

            // Modificar velocidade
            var velocity = entity.GetComponent<VelocityComponent>();
            if (velocity != null)
            {
                velocity.Speed *= classDef.SpeedMultiplier;
            }

            // Nota: Damage seria aplicado no WeaponComponent quando atirar
        }
    }
}

// Mods/CharacterClasses/CharacterClassesMod.cs
using CubeSurvivor.Core;
using Mods.CharacterClasses.Systems;

namespace Mods.CharacterClasses
{
    /// <summary>
    /// Mod que adiciona sistema de classes de personagem
    /// </summary>
    public static class CharacterClassesMod
    {
        public static void Initialize(GameWorld world)
        {
            // Adicionar sistema ao mundo
            world.AddSystem(new CharacterClassSystem());

            Console.WriteLine("[Mod] Character Classes loaded!");
        }

        public static void RegisterCustomClass(string id, CharacterClassDefinition definition)
        {
            Registries.CharacterClassRegistry.Instance.Register(id, definition);
        }
    }
}
```

#### Uso do Mod

```csharp
// No Game1.cs ou GameInitializer
using Mods.CharacterClasses;
using Mods.CharacterClasses.Components;

public void InitializeWithMods()
{
    var world = new GameWorldBuilder()
        // ... outros sistemas
        .Build();

    // Carregar mod
    CharacterClassesMod.Initialize(world);

    // Registrar classe customizada
    CharacterClassesMod.RegisterCustomClass("assassin", new CharacterClassDefinition
    {
        Name = "Assassin",
        Description = "Critical hits specialist",
        HealthMultiplier = 0.6f,
        SpeedMultiplier = 1.5f,
        DamageMultiplier = 2.0f,
        StartingItems = new[] { "dagger", "dagger" }
    });

    // Criar jogador com classe
    var player = new EntityBuilder(world, "Player")
        .WithTransform(startPosition)
        .WithHealth(100f)
        .WithVelocity(250f)
        .WithComponent(new CharacterClassComponent("warrior"))
        .Build();
}
```

---

## Conclusão

Estes exemplos demonstram:
- ✅ Como usar os **Registries** para adicionar conteúdo
- ✅ Como criar novos **Components** e **Systems**
- ✅ Como usar **Builders** para código limpo
- ✅ Como criar **Mods** que estendem o jogo
- ✅ Arquitetura **SOLID** e **ECS** na prática
