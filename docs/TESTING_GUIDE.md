# Guia de Testes - Arquitetura SOLID + ECS

## 🧪 Por que a Arquitetura Facilita Testes

A arquitetura **SOLID + ECS** torna o código altamente testável:

- ✅ **Components** são simples DTOs (fácil de instanciar)
- ✅ **Systems** têm dependências injetáveis (fácil de mockar)
- ✅ **Factories** usam interfaces (fácil de substituir)
- ✅ **Registries** são singleton (fácil de resetar entre testes)

---

## 📋 Estrutura de Testes

### Organização Recomendada

```
Tests/
├── Core/
│   ├── EntityTests.cs
│   ├── GameWorldTests.cs
│   └── RegistryTests.cs
├── Components/
│   ├── HealthComponentTests.cs
│   └── TransformComponentTests.cs
├── Systems/
│   ├── MovementSystemTests.cs
│   ├── AISystemTests.cs
│   └── CollisionSystemTests.cs
├── Factories/
│   ├── EnemyFactoryTests.cs
│   └── WeaponFactoryTests.cs
├── Registries/
│   ├── EnemyRegistryTests.cs
│   └── WeaponRegistryTests.cs
└── Builders/
    └── EntityBuilderTests.cs
```

---

## 🔧 Setup de Testes

### Dependências Necessárias

```xml
<!-- Adicionar ao .csproj -->
<ItemGroup>
  <PackageReference Include="xunit" Version="2.4.2" />
  <PackageReference Include="xunit.runner.visualstudio" Version="2.4.5" />
  <PackageReference Include="Moq" Version="4.18.4" />
  <PackageReference Include="FluentAssertions" Version="6.11.0" />
</ItemGroup>
```

---

## 📝 Exemplos de Testes

### 1. Testando Components (Dados)

```csharp
using Xunit;
using FluentAssertions;
using CubeSurvivor.Components;

namespace CubeSurvivor.Tests.Components
{
    public class HealthComponentTests
    {
        [Fact]
        public void HealthComponent_InitializesWithMaxHealth()
        {
            // Arrange & Act
            var health = new HealthComponent(100f);

            // Assert
            health.MaxHealth.Should().Be(100f);
            health.CurrentHealth.Should().Be(100f);
        }

        [Fact]
        public void HealthComponent_CurrentHealth_CanBeModified()
        {
            // Arrange
            var health = new HealthComponent(100f);

            // Act
            health.CurrentHealth = 50f;

            // Assert
            health.CurrentHealth.Should().Be(50f);
            health.MaxHealth.Should().Be(100f); // MaxHealth não muda
        }

        [Fact]
        public void HealthComponent_IsAlive_ReturnsTrueWhenHealthAboveZero()
        {
            // Arrange
            var health = new HealthComponent(100f);
            health.CurrentHealth = 10f;

            // Act
            var isAlive = health.CurrentHealth > 0;

            // Assert
            isAlive.Should().BeTrue();
        }
    }
}
```

---

### 2. Testando Systems (Lógica)

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using CubeSurvivor.Systems;

namespace CubeSurvivor.Tests.Systems
{
    public class MovementSystemTests
    {
        [Fact]
        public void MovementSystem_UpdatesEntityPosition()
        {
            // Arrange
            var world = new GameWorld();
            var system = new MovementSystem();
            system.Initialize(world);

            var entity = world.CreateEntity("TestEntity");
            var transform = entity.AddComponent(new TransformComponent(new Vector2(0, 0)));
            var velocity = entity.AddComponent(new VelocityComponent(100f));
            velocity.Direction = new Vector2(1, 0); // Direita

            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1));

            // Act
            system.Update(gameTime);

            // Assert
            transform.Position.X.Should().Be(100f); // Moveu 100 pixels em 1 segundo
            transform.Position.Y.Should().Be(0f);
        }

        [Fact]
        public void MovementSystem_DoesNotMoveEntitiesWithoutVelocity()
        {
            // Arrange
            var world = new GameWorld();
            var system = new MovementSystem();
            system.Initialize(world);

            var entity = world.CreateEntity("StaticEntity");
            var transform = entity.AddComponent(new TransformComponent(new Vector2(100, 100)));
            // Sem VelocityComponent

            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(1));

            // Act
            system.Update(gameTime);

            // Assert
            transform.Position.Should().Be(new Vector2(100, 100)); // Não moveu
        }
    }
}
```

---

### 3. Testando Factories com Mocks

```csharp
using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Entities;
using CubeSurvivor.Game.Registries;

namespace CubeSurvivor.Tests.Factories
{
    public class EnemyFactoryTests : IDisposable
    {
        private readonly GameWorld _world;
        private readonly Mock<TextureManager> _textureManagerMock;
        private readonly EnemyFactory _factory;

        public EnemyFactoryTests()
        {
            // Setup
            _world = new GameWorld();
            _textureManagerMock = new Mock<TextureManager>();
            _factory = new EnemyFactory(_textureManagerMock.Object);

            // Registrar inimigo de teste
            EnemyRegistry.Instance.Register("test_enemy", new EnemyDefinition
            {
                Name = "Test Enemy",
                Health = 50f,
                Damage = 10f,
                Speed = 150f,
                AttackCooldown = 1f,
                Width = 40f,
                Height = 40f,
                ColorR = 255,
                ColorG = 0,
                ColorB = 0
            });
        }

        [Fact]
        public void CreateEnemy_CreatesEntityWithCorrectComponents()
        {
            // Arrange
            var position = new Vector2(100, 100);

            // Act
            var enemy = _factory.CreateEnemy(_world, position, "test_enemy");

            // Assert
            enemy.Should().NotBeNull();
            enemy.Name.Should().Be("Test Enemy");
            enemy.HasComponent<TransformComponent>().Should().BeTrue();
            enemy.HasComponent<HealthComponent>().Should().BeTrue();
            enemy.HasComponent<VelocityComponent>().Should().BeTrue();
            enemy.HasComponent<AIComponent>().Should().BeTrue();
            enemy.HasComponent<EnemyComponent>().Should().BeTrue();
        }

        [Fact]
        public void CreateEnemy_SetsCorrectHealthFromDefinition()
        {
            // Arrange
            var position = new Vector2(100, 100);

            // Act
            var enemy = _factory.CreateEnemy(_world, position, "test_enemy");
            var health = enemy.GetComponent<HealthComponent>();

            // Assert
            health.MaxHealth.Should().Be(50f);
            health.CurrentHealth.Should().Be(50f);
        }

        [Fact]
        public void CreateEnemy_ThrowsExceptionForUnregisteredType()
        {
            // Arrange
            var position = new Vector2(100, 100);

            // Act
            Action act = () => _factory.CreateEnemy(_world, position, "nonexistent");

            // Assert
            act.Should().Throw<ArgumentException>()
               .WithMessage("*not registered*");
        }

        public void Dispose()
        {
            // Cleanup - remover teste do registry
            EnemyRegistry.Instance.Unregister("test_enemy");
        }
    }
}
```

---

### 4. Testando Registries

```csharp
using Xunit;
using FluentAssertions;
using CubeSurvivor.Game.Registries;

namespace CubeSurvivor.Tests.Registries
{
    public class EnemyRegistryTests
    {
        [Fact]
        public void Register_AddsEnemyDefinition()
        {
            // Arrange
            var definition = new EnemyDefinition
            {
                Name = "Test Enemy",
                Health = 100f,
                Damage = 10f,
                Speed = 150f,
                AttackCooldown = 1f,
                Width = 40f,
                Height = 40f,
                ColorR = 255,
                ColorG = 0,
                ColorB = 0
            };

            // Act
            EnemyRegistry.Instance.Register("test", definition);

            // Assert
            EnemyRegistry.Instance.Contains("test").Should().BeTrue();
            var retrieved = EnemyRegistry.Instance.Get("test");
            retrieved.Should().Be(definition);

            // Cleanup
            EnemyRegistry.Instance.Unregister("test");
        }

        [Fact]
        public void Get_ThrowsExceptionForNonexistentKey()
        {
            // Act
            Action act = () => EnemyRegistry.Instance.Get("nonexistent_enemy");

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }

        [Fact]
        public void GetAllValues_ReturnsAllRegisteredEnemies()
        {
            // Arrange
            var def1 = new EnemyDefinition { Name = "Enemy1", Health = 50f };
            var def2 = new EnemyDefinition { Name = "Enemy2", Health = 100f };

            EnemyRegistry.Instance.Register("test1", def1);
            EnemyRegistry.Instance.Register("test2", def2);

            // Act
            var all = EnemyRegistry.Instance.GetAllValues();

            // Assert
            all.Should().Contain(new[] { def1, def2 });

            // Cleanup
            EnemyRegistry.Instance.Unregister("test1");
            EnemyRegistry.Instance.Unregister("test2");
        }
    }
}
```

---

### 5. Testando EntityBuilder

```csharp
using Xunit;
using FluentAssertions;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using CubeSurvivor.Builders;

namespace CubeSurvivor.Tests.Builders
{
    public class EntityBuilderTests
    {
        [Fact]
        public void EntityBuilder_BuildsEntityWithAllComponents()
        {
            // Arrange
            var world = new GameWorld();
            var position = new Vector2(100, 100);

            // Act
            var entity = new EntityBuilder(world, "TestEntity")
                .WithTransform(position)
                .WithSprite(Color.Red, 32, 32, RenderLayer.Entities)
                .WithVelocity(200f)
                .WithHealth(100f)
                .WithCollider(32, 32, ColliderTag.Player)
                .Build();

            // Assert
            entity.Should().NotBeNull();
            entity.Name.Should().Be("TestEntity");
            entity.HasComponent<TransformComponent>().Should().BeTrue();
            entity.HasComponent<SpriteComponent>().Should().BeTrue();
            entity.HasComponent<VelocityComponent>().Should().BeTrue();
            entity.HasComponent<HealthComponent>().Should().BeTrue();
            entity.HasComponent<ColliderComponent>().Should().BeTrue();
        }

        [Fact]
        public void EntityBuilder_WithTransform_SetsCorrectPosition()
        {
            // Arrange
            var world = new GameWorld();
            var position = new Vector2(50, 75);

            // Act
            var entity = new EntityBuilder(world, "TestEntity")
                .WithTransform(position)
                .Build();

            var transform = entity.GetComponent<TransformComponent>();

            // Assert
            transform.Position.Should().Be(position);
        }

        [Fact]
        public void EntityBuilder_SupportsFluentInterface()
        {
            // Arrange
            var world = new GameWorld();

            // Act
            var builder = new EntityBuilder(world, "Test");
            var result1 = builder.WithTransform(Vector2.Zero);
            var result2 = result1.WithHealth(100f);

            // Assert
            result1.Should().BeSameAs(builder);
            result2.Should().BeSameAs(builder);
        }
    }
}
```

---

### 6. Testando Integração de Sistemas

```csharp
using Xunit;
using FluentAssertions;
using Microsoft.Xna.Framework;
using CubeSurvivor.Core;
using CubeSurvivor.Components;
using CubeSurvivor.Systems;
using CubeSurvivor.Systems.Combat;

namespace CubeSurvivor.Tests.Integration
{
    public class CombatIntegrationTests
    {
        [Fact]
        public void Enemy_DamagesPlayer_OnCollision()
        {
            // Arrange
            var world = new GameWorld();
            
            // Criar player
            var player = world.CreateEntity("Player");
            player.AddComponent(new TransformComponent(new Vector2(100, 100)));
            player.AddComponent(new HealthComponent(100f));
            player.AddComponent(new ColliderComponent(32, 32, ColliderTag.Player));

            // Criar inimigo
            var enemy = world.CreateEntity("Enemy");
            enemy.AddComponent(new TransformComponent(new Vector2(100, 100))); // Mesma posição
            enemy.AddComponent(new EnemyComponent(10f, 1f)); // 10 de dano
            enemy.AddComponent(new ColliderComponent(32, 32, ColliderTag.Enemy));

            // Adicionar sistema de colisão
            var collisionSystem = new CollisionSystem();
            world.AddSystem(collisionSystem);

            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

            // Act
            world.Update(gameTime);

            // Assert
            var playerHealth = player.GetComponent<HealthComponent>();
            playerHealth.CurrentHealth.Should().BeLessThan(100f); // Sofreu dano
        }

        [Fact]
        public void DeathSystem_RemovesEntityWhenHealthReachesZero()
        {
            // Arrange
            var world = new GameWorld();
            
            var entity = world.CreateEntity("Dying Entity");
            var health = entity.AddComponent(new HealthComponent(10f));
            health.CurrentHealth = 0f; // Morto

            var deathSystem = new DeathSystem();
            world.AddSystem(deathSystem);

            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromSeconds(0.016));

            // Act
            world.Update(gameTime);

            // Assert
            entity.Active.Should().BeFalse(); // Entidade foi desativada
        }
    }
}
```

---

## 🎯 Boas Práticas de Testes

### ✅ DO (Faça)

1. **Teste uma coisa por vez**
```csharp
// ✅ BOM
[Fact]
public void HealthComponent_InitializesWithMaxHealth() { }

[Fact]
public void HealthComponent_CurrentHealthCanBeModified() { }

// ❌ RUIM - testa várias coisas
[Fact]
public void HealthComponent_WorksCorrectly() { }
```

2. **Use nomes descritivos**
```csharp
// ✅ BOM
[Fact]
public void CreateEnemy_ThrowsException_WhenTypeNotRegistered() { }

// ❌ RUIM
[Fact]
public void Test1() { }
```

3. **Siga padrão AAA (Arrange, Act, Assert)**
```csharp
[Fact]
public void Example()
{
    // Arrange - Setup
    var entity = new Entity();
    
    // Act - Ação
    entity.AddComponent(new HealthComponent(100f));
    
    // Assert - Verificação
    entity.HasComponent<HealthComponent>().Should().BeTrue();
}
```

4. **Limpe após os testes (IDisposable)**
```csharp
public class MyTests : IDisposable
{
    public MyTests()
    {
        // Setup antes de cada teste
    }

    public void Dispose()
    {
        // Cleanup após cada teste
        EnemyRegistry.Instance.Unregister("test_enemy");
    }
}
```

---

### ❌ DON'T (Não Faça)

1. **Não teste implementações, teste comportamentos**
```csharp
// ❌ RUIM - testa implementação interna
[Fact]
public void List_UsesArrayInternally() { }

// ✅ BOM - testa comportamento
[Fact]
public void AddComponent_IncreasesComponentCount() { }
```

2. **Não crie dependências entre testes**
```csharp
// ❌ RUIM - Teste2 depende de Teste1
[Fact]
public void Test1_CreatesEnemy() { /* cria enemy "test" */ }

[Fact]
public void Test2_UsesEnemy() { /* usa enemy "test" criado em Test1 */ }

// ✅ BOM - Cada teste é independente
[Fact]
public void Test1() { /* setup próprio */ }

[Fact]
public void Test2() { /* setup próprio */ }
```

---

## 🔍 Cobertura de Testes

### Objetivos

- **Components**: 100% (são simples DTOs)
- **Systems**: 80-90% (lógica principal)
- **Factories**: 80-90% (criação)
- **Registries**: 90-100% (operações críticas)

### Ferramentas

```bash
# Instalar ferramenta de cobertura
dotnet tool install --global coverlet.console

# Gerar relatório de cobertura
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Ver relatório HTML
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport
```

---

## 📊 Exemplo de Suite Completa

```csharp
namespace CubeSurvivor.Tests
{
    public class GameplayTests : IDisposable
    {
        private GameWorld _world;
        private Entity _player;
        private IEnemyFactory _enemyFactory;

        public GameplayTests()
        {
            // Setup compartilhado
            _world = new GameWorld();
            
            // Criar player
            _player = new EntityBuilder(_world, "Player")
                .WithTransform(new Vector2(1000, 1000))
                .WithHealth(100f)
                .WithVelocity(250f)
                .WithCollider(32, 32, ColliderTag.Player)
                .Build();

            // Setup factory
            _enemyFactory = new EnemyFactory();

            // Registrar inimigo de teste
            EnemyRegistry.Instance.Register("test", new EnemyDefinition
            {
                Name = "Test",
                Health = 50f,
                Damage = 10f,
                Speed = 150f,
                AttackCooldown = 1f,
                Width = 40f,
                Height = 40f,
                ColorR = 255,
                ColorG = 0,
                ColorB = 0
            });
        }

        [Fact]
        public void Player_TakesDamage_WhenHitByEnemy() { }

        [Fact]
        public void Enemy_Dies_WhenHealthReachesZero() { }

        [Fact]
        public void Enemy_DropsLoot_OnDeath() { }

        [Fact]
        public void Player_CanPickup_DroppedItems() { }

        public void Dispose()
        {
            EnemyRegistry.Instance.Unregister("test");
        }
    }
}
```

---

## 🚀 Executando Testes

```bash
# Executar todos os testes
dotnet test

# Executar testes de uma classe específica
dotnet test --filter "FullyQualifiedName~EnemyFactoryTests"

# Executar com verbose
dotnet test --verbosity detailed

# Executar com cobertura
dotnet test /p:CollectCoverage=true
```

---

## 📚 Recursos Adicionais

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions](https://fluentassertions.com/)
- [Moq Quickstart](https://github.com/moq/moq4/wiki/Quickstart)
- [Test-Driven Development](https://en.wikipedia.org/wiki/Test-driven_development)

---

**Arquivo:** `docs/TESTING_GUIDE.md`
**Versão:** 1.0.0
