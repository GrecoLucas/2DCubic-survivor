# Guia Rápido de Referência - SOLID & ECS

## 📌 Checklist SOLID

### ✅ Antes de Criar uma Nova Classe

- [ ] A classe tem **uma única responsabilidade**?
- [ ] Posso estender sem modificar código existente?
- [ ] Estou dependendo de **interfaces**, não implementações?
- [ ] As interfaces são pequenas e específicas?
- [ ] A classe pode ser substituída por subclasses?

### ✅ Antes de Criar um Componente

- [ ] Contém **apenas dados** (sem métodos de lógica)?
- [ ] Herda de `Component`?
- [ ] Tem propriedades públicas?
- [ ] NÃO tem métodos `Update()`, `Process()`, etc?

**Exemplo Correto:**
```csharp
public class ShieldComponent : Component
{
    public float ShieldStrength { get; set; }
    public float MaxShieldStrength { get; }
    // ✅ Apenas dados!
}
```

**Exemplo ERRADO:**
```csharp
public class ShieldComponent : Component
{
    public void Recharge(float amount) { } // ❌ Lógica aqui!
    public void TakeDamage(float damage) { } // ❌ Isso vai no System!
}
```

### ✅ Antes de Criar um Sistema

- [ ] Herda de `GameSystem`?
- [ ] Implementa `Update(GameTime gameTime)`?
- [ ] Opera sobre componentes, NÃO modifica definições?
- [ ] Tem uma única responsabilidade?
- [ ] Recebe dependências via construtor (DI)?

**Exemplo:**
```csharp
public sealed class ShieldSystem : GameSystem
{
    public override void Update(GameTime gameTime)
    {
        foreach (var entity in World.GetEntitiesWithComponent<ShieldComponent>())
        {
            var shield = entity.GetComponent<ShieldComponent>();
            // Processar lógica aqui
        }
    }
}
```

---

## 🔑 Comandos Rápidos

### Criar Novo Inimigo
```csharp
EnemyRegistry.Instance.Register("tipo", new EnemyDefinition {
    Name = "Nome",
    Health = 100f,
    Damage = 10f,
    Speed = 150f,
    AttackCooldown = 1f,
    Width = 40f,
    Height = 40f,
    ColorR = 255, ColorG = 0, ColorB = 0
});
```

### Criar Nova Arma
```csharp
WeaponRegistry.Instance.Register("tipo", new WeaponDefinition {
    Name = "Nome",
    Damage = 10f,
    FireRate = 0.5f,
    BulletSpeed = 500f,
    Width = 25f,
    Height = 6f
});
```

### Criar Novo Bioma
```csharp
BiomeRegistry.Instance.Register("tipo", new BiomeDefinition {
    Type = BiomeType.Forest,
    AllowsEnemySpawns = false,
    TreeDensity = 40,
    GoldDensity = 0,
    TextureName = "texture.png"
});
```

### Criar Entidade com Builder
```csharp
var entity = new EntityBuilder(world, "Nome")
    .WithTransform(position)
    .WithSprite(color, width, height, layer)
    .WithVelocity(speed)
    .WithHealth(health)
    .WithCollider(width, height, tag)
    .Build();
```

### Adicionar Sistema ao Mundo
```csharp
world.AddSystem(new MeuSistema());
```

---

## 📋 Padrão de Nomenclatura

### Components
- Nome: `[Funcionalidade]Component`
- Exemplos: `HealthComponent`, `TransformComponent`, `AIComponent`
- Namespace: `CubeSurvivor.Components.[Categoria]`

### Systems
- Nome: `[Funcionalidade]System`
- Exemplos: `MovementSystem`, `RenderSystem`, `AISystem`
- Namespace: `CubeSurvivor.Systems.[Categoria]`

### Factories
- Nome: `[Tipo]Factory`
- Exemplos: `EnemyFactory`, `WeaponFactory`
- Interface: `I[Tipo]Factory`
- Namespace: `CubeSurvivor.Entities.Factories`

### Registries
- Nome: `[Tipo]Registry`
- Exemplos: `EnemyRegistry`, `WeaponRegistry`
- Namespace: `CubeSurvivor.Game.Registries`

---

## 🚨 Erros Comuns

### ❌ Lógica em Componentes
```csharp
// ERRADO
public class HealthComponent : Component
{
    public void TakeDamage(float amount)
    {
        CurrentHealth -= amount; // ❌ Lógica aqui!
    }
}

// CORRETO
public class HealthComponent : Component
{
    public float CurrentHealth { get; set; } // ✅ Apenas dados
}

// Lógica vai no HealthSystem
public class HealthSystem : GameSystem
{
    public void ApplyDamage(Entity entity, float amount)
    {
        var health = entity.GetComponent<HealthComponent>();
        health.CurrentHealth -= amount; // ✅ Lógica no System
    }
}
```

### ❌ Dependência de Implementação Concreta
```csharp
// ERRADO
public class EnemySpawnSystem
{
    private EnemyFactory _factory; // ❌ Dependência concreta
}

// CORRETO
public class EnemySpawnSystem
{
    private IEnemyFactory _factory; // ✅ Dependência de interface
}
```

### ❌ Hardcoding de Valores
```csharp
// ERRADO
var enemy = new Entity();
enemy.AddComponent(new HealthComponent { MaxHealth = 100f }); // ❌ Valor fixo

// CORRETO
var definition = EnemyRegistry.Instance.Get("goblin");
enemy.AddComponent(new HealthComponent { MaxHealth = definition.Health }); // ✅ Configurável
```

### ❌ Múltiplas Responsabilidades
```csharp
// ERRADO
public class GameSystem
{
    public void Update()
    {
        ProcessMovement(); // ❌
        ProcessRendering(); // ❌ Muitas responsabilidades
        ProcessAI();        // ❌
    }
}

// CORRETO - Um sistema por responsabilidade
public class MovementSystem : GameSystem { }
public class RenderSystem : GameSystem { }
public class AISystem : GameSystem { }
```

---

## 🎯 Template de Novo Feature

### 1. Criar Componente (Dados)
```csharp
// src/Components/[Categoria]/[Nome]Component.cs
using CubeSurvivor.Core;

namespace CubeSurvivor.Components.[Categoria]
{
    public class [Nome]Component : Component
    {
        public [Tipo] [Propriedade] { get; set; }
        // Apenas dados, sem lógica
    }
}
```

### 2. Criar Sistema (Lógica)
```csharp
// src/Systems/[Categoria]/[Nome]System.cs
using CubeSurvivor.Core;
using Microsoft.Xna.Framework;

namespace CubeSurvivor.Systems.[Categoria]
{
    public sealed class [Nome]System : GameSystem
    {
        public override void Update(GameTime gameTime)
        {
            foreach (var entity in World.GetEntitiesWithComponent<[Nome]Component>())
            {
                var comp = entity.GetComponent<[Nome]Component>();
                // Processar lógica
            }
        }
    }
}
```

### 3. Registrar Sistema
```csharp
// No Game1.cs ou GameInitializer
world.AddSystem(new [Nome]System());
```

### 4. Usar em Entidades
```csharp
entity.AddComponent(new [Nome]Component { ... });
```

---

## 📊 Fluxo de Dados ECS

```
1. CRIAÇÃO
   Entity → AddComponent(Component) → Dados armazenados

2. PROCESSAMENTO
   GameWorld.Update() → System.Update() → Processa Components

3. LEITURA
   System → GetEntitiesWithComponent<T>() → Processa cada Entity

4. MODIFICAÇÃO
   System → GetComponent<T>() → Modifica dados do Component
```

---

## 🔍 Debugging

### Ver todas entidades com um componente
```csharp
var entities = World.GetEntitiesWithComponent<HealthComponent>();
foreach (var entity in entities)
{
    Console.WriteLine($"Entity: {entity.Name}");
}
```

### Verificar se entidade tem componente
```csharp
if (entity.HasComponent<HealthComponent>())
{
    var health = entity.GetComponent<HealthComponent>();
    Console.WriteLine($"Health: {health.CurrentHealth}");
}
```

### Listar todos componentes de uma entidade
```csharp
foreach (var component in entity.GetAllComponents())
{
    Console.WriteLine($"Component: {component.GetType().Name}");
}
```

---

## 📚 Recursos Adicionais

- **[ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md)**: Guia completo de arquitetura
- **[EXAMPLES.md](EXAMPLES.md)**: Exemplos práticos e casos de uso
- **[SOLID Principles](https://en.wikipedia.org/wiki/SOLID)**: Referência externa
- **[ECS Pattern](https://en.wikipedia.org/wiki/Entity_component_system)**: Referência externa

---

## ⚡ Dicas de Produtividade

1. Use **Registries** para tudo que é configurável
2. Use **Builders** para entidades complexas
3. Mantenha Systems **pequenos e focados**
4. Teste com diferentes configurações via Registries
5. Documente com comentários XML
6. Use `sealed` em classes que não devem ser herdadas
7. Prefira composição sobre herança

---

**Mantido em**: `docs/QUICK_REFERENCE.md`
