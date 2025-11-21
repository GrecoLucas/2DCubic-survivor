# Guia de Migração - Nova Arquitetura SOLID + ECS

## 📋 Visão Geral

Este guia ajuda a migrar código existente para a nova arquitetura baseada em **SOLID** e **ECS puro**, com uso de **Registries**, **Factories** e **Builders**.

---

## ⚠️ Mudanças Importantes

### 1. Factories Agora Usam Registries

#### Antes (Antigo)
```csharp
// EnemyFactory criava inimigos hardcoded
public class EnemyFactory
{
    public Entity CreateEnemy(IGameWorld world, Vector2 position)
    {
        var enemy = world.CreateEntity("Enemy");
        enemy.AddComponent(new HealthComponent(50f)); // Valores fixos
        enemy.AddComponent(new SpriteComponent(Color.Red, 40f, 40f));
        return enemy;
    }
}
```

#### Agora (Novo)
```csharp
// EnemyFactory usa EnemyRegistry para configurações
public class EnemyFactory : IEnemyFactory
{
    public Entity CreateEnemy(IGameWorld world, Vector2 position, string enemyType = "default")
    {
        var definition = EnemyRegistry.Instance.Get(enemyType);
        var enemy = world.CreateEntity(definition.Name);
        enemy.AddComponent(new HealthComponent(definition.Health)); // Configurável
        // ... restante baseado em definition
        return enemy;
    }
}
```

**Como migrar:**
1. Extraia valores hardcoded para definições
2. Registre definições no Registry apropriado
3. Use o Registry na Factory

---

### 2. Interface IEnemyFactory Mudou

#### Antes
```csharp
Entity CreateEnemy(IGameWorld world, Vector2 position);
```

#### Agora
```csharp
Entity CreateEnemy(IGameWorld world, Vector2 position, string enemyType = "default");
```

**Como migrar:**
```csharp
// Código antigo
var enemy = enemyFactory.CreateEnemy(world, position);

// Código novo (compatível por padrão)
var enemy = enemyFactory.CreateEnemy(world, position); // Usa "default"
var enemy = enemyFactory.CreateEnemy(world, position, "fast"); // Ou especifica tipo
```

---

### 3. Novos Registries Disponíveis

#### Registrar Tipos Personalizados

**Inimigos:**
```csharp
// No início do jogo ou arquivo de inicialização
EnemyRegistry.Instance.Register("meu_inimigo", new EnemyDefinition
{
    Name = "Meu Inimigo",
    Health = 100f,
    Damage = 15f,
    Speed = 200f,
    AttackCooldown = 1.2f,
    Width = 45f,
    Height = 45f,
    ColorR = 255,
    ColorG = 100,
    ColorB = 0
});
```

**Armas:**
```csharp
WeaponRegistry.Instance.Register("minha_arma", new WeaponDefinition
{
    Name = "Minha Arma",
    Damage = 20f,
    FireRate = 0.4f,
    BulletSpeed = 600f,
    Width = 30f,
    Height = 8f
});
```

**Biomas:**
```csharp
BiomeRegistry.Instance.Register("meu_bioma", new BiomeDefinition
{
    Type = BiomeType.Forest,
    AllowsEnemySpawns = true,
    TreeDensity = 30,
    GoldDensity = 10,
    TextureName = "meu_bioma.png"
});
```

---

### 4. Builder Pattern para Entidades

#### Antes
```csharp
var player = world.CreateEntity("Player");
player.AddComponent(new TransformComponent(position));
player.AddComponent(new SpriteComponent(Color.Blue, 32, 32, RenderLayer.Entities));
player.AddComponent(new VelocityComponent(250f));
player.AddComponent(new HealthComponent(100f));
player.AddComponent(new ColliderComponent(32, 32, ColliderTag.Player));
```

#### Agora (Mais Limpo)
```csharp
var player = new EntityBuilder(world, "Player")
    .WithTransform(position)
    .WithSprite(Color.Blue, 32, 32, RenderLayer.Entities)
    .WithVelocity(250f)
    .WithHealth(100f)
    .WithCollider(32, 32, ColliderTag.Player)
    .Build();
```

**Como migrar:**
- Use `EntityBuilder` para entidades complexas
- Mantém código antigo funcionando (não é obrigatório usar Builder)

---

### 5. Novas Interfaces de Factories

Foram criadas interfaces para todas as factories:

- `IEnemyFactory` - Criação de inimigos
- `IWeaponFactory` - Criação de armas
- `IResourceFactory` - Criação de recursos
- `IBulletFactory` - Criação de projéteis
- `IPlayerFactory` - Criação do jogador

**Como migrar:**
```csharp
// Antes
private EnemyFactory _enemyFactory;

// Agora (melhor para testes e DI)
private IEnemyFactory _enemyFactory;
```

---

## 🔄 Passo a Passo de Migração

### Etapa 1: Atualizar Referências de Factories

**Arquivos afetados:**
- `EnemySpawnSystem.cs`
- Qualquer código que use factories

**Ação:**
```csharp
// Trocar tipo concreto por interface
private readonly IEnemyFactory _enemyFactory;

// Atualizar chamadas que não especificam tipo
var enemy = _enemyFactory.CreateEnemy(world, position, "default");
```

---

### Etapa 2: Migrar Valores Hardcoded para Registries

**Exemplo: Inimigos customizados**

1. Identifique valores hardcoded:
```csharp
// Código antigo em algum lugar
var fastEnemy = world.CreateEntity("Fast Enemy");
fastEnemy.AddComponent(new HealthComponent(30f));
fastEnemy.AddComponent(new VelocityComponent(300f));
```

2. Crie definição no Registry:
```csharp
// No início do jogo
EnemyRegistry.Instance.Register("fast", new EnemyDefinition
{
    Name = "Fast Enemy",
    Health = 30f,
    Speed = 300f,
    // ... outras propriedades
});
```

3. Use a Factory:
```csharp
// Código novo
var fastEnemy = enemyFactory.CreateEnemy(world, position, "fast");
```

---

### Etapa 3: Adicionar Construtores com TextureManager

Algumas factories agora aceitam `TextureManager` opcional:

**Antes:**
```csharp
var factory = new EnemyFactory();
```

**Agora:**
```csharp
var factory = new EnemyFactory(textureManager); // Opcional
```

---

### Etapa 4: Usar Builders (Opcional)

Identifique criação de entidades complexas e simplifique com Builder:

**Antes:**
```csharp
var boss = world.CreateEntity("Boss");
boss.AddComponent(new TransformComponent(position));
boss.AddComponent(new SpriteComponent(Color.DarkRed, 80, 80, RenderLayer.Entities));
boss.AddComponent(new VelocityComponent(120f));
boss.AddComponent(new HealthComponent(1000f));
boss.AddComponent(new AIComponent(120f));
boss.AddComponent(new EnemyComponent(50f, 2f));
var lootDrop = new LootDropComponent();
lootDrop.AddLoot("gold", 1.0f);
boss.AddComponent(lootDrop);
```

**Agora:**
```csharp
var boss = new EntityBuilder(world, "Boss")
    .WithTransform(position)
    .WithSprite(Color.DarkRed, 80, 80, RenderLayer.Entities)
    .WithVelocity(120f)
    .WithHealth(1000f)
    .WithAI(120f)
    .WithEnemy(50f, 2f)
    .WithLootDrop(("gold", 1.0f))
    .Build();
```

---

## 🆕 Novos Recursos Disponíveis

### 1. Sistema de Registry Genérico

Você pode criar seus próprios registries:

```csharp
using CubeSurvivor.Core.Registry;

public class MyCustomDefinition
{
    public string Name { get; set; }
    public float Value { get; set; }
}

public class MyCustomRegistry : Registry<string, MyCustomDefinition>
{
    private static readonly Lazy<MyCustomRegistry> _instance = 
        new Lazy<MyCustomRegistry>(() => new MyCustomRegistry());

    public static MyCustomRegistry Instance => _instance.Value;

    private MyCustomRegistry() { }
}

// Uso
MyCustomRegistry.Instance.Register("key", new MyCustomDefinition { ... });
var def = MyCustomRegistry.Instance.Get("key");
```

---

### 2. EntityBuilder Extensível

Adicione seus próprios métodos ao Builder:

```csharp
public static class EntityBuilderExtensions
{
    public static EntityBuilder WithMyCustomComponent(
        this EntityBuilder builder, 
        float value)
    {
        return builder.WithComponent(new MyCustomComponent { Value = value });
    }
}

// Uso
var entity = new EntityBuilder(world, "Test")
    .WithTransform(position)
    .WithMyCustomComponent(42f)
    .Build();
```

---

### 3. ResourceFactory Unificada

Agora há uma factory única para todos os recursos:

```csharp
var resourceFactory = new ResourceFactory(textureManager);

// Criar diferentes tipos de recursos
var wood = resourceFactory.CreateResource(world, position, "wood");
var gold = resourceFactory.CreateResource(world, position, "gold");
var apple = resourceFactory.CreateResource(world, position, "apple");
var brain = resourceFactory.CreateResource(world, position, "brain");
```

---

## ✅ Checklist de Migração

Use esta checklist para verificar se migrou corretamente:

- [ ] Todas as factories usam interfaces (`IEnemyFactory`, etc)
- [ ] Valores hardcoded movidos para Registries
- [ ] Factories recebem `TextureManager` quando apropriado
- [ ] Código compila sem erros
- [ ] Testes passam (se houver)
- [ ] Documentação atualizada
- [ ] Novos tipos registrados nos Registries apropriados

---

## 🔧 Solução de Problemas

### Problema: "Enemy type 'xyz' not registered"

**Causa:** Tentando criar inimigo não registrado

**Solução:**
```csharp
// Registrar antes de usar
EnemyRegistry.Instance.Register("xyz", new EnemyDefinition { ... });
```

---

### Problema: Compilação falha em IEnemyFactory

**Causa:** Assinatura de método mudou

**Solução:**
```csharp
// Adicionar parâmetro enemyType com valor padrão
public Entity CreateEnemy(IGameWorld world, Vector2 position, string enemyType = "default")
```

---

### Problema: Valores padrão diferentes

**Causa:** Definições no Registry diferem dos valores hardcoded antigos

**Solução:**
1. Verifique valores no Registry
2. Atualize a definição ou
3. Crie nova definição com valores antigos

---

## 📊 Comparação: Antes vs Depois

### Adicionar Novo Inimigo

#### Antes (≈ 20 linhas)
```csharp
// Criar nova classe
public class TankEnemyFactory
{
    public Entity Create(IGameWorld world, Vector2 position)
    {
        var enemy = world.CreateEntity("Tank");
        enemy.AddComponent(new TransformComponent(position));
        enemy.AddComponent(new SpriteComponent(Color.Gray, 60, 60, RenderLayer.Entities));
        enemy.AddComponent(new VelocityComponent(100f));
        enemy.AddComponent(new AIComponent(100f));
        enemy.AddComponent(new EnemyComponent(20f, 2f));
        enemy.AddComponent(new HealthComponent(200f));
        enemy.AddComponent(new ColliderComponent(60, 60, ColliderTag.Enemy));
        return enemy;
    }
}
```

#### Depois (≈ 5 linhas)
```csharp
// Apenas registrar
EnemyRegistry.Instance.Register("tank", new EnemyDefinition
{
    Name = "Tank", Health = 200f, Damage = 20f, Speed = 100f,
    AttackCooldown = 2f, Width = 60f, Height = 60f,
    ColorR = 128, ColorG = 128, ColorB = 128
});

// Usar
var tank = enemyFactory.CreateEnemy(world, position, "tank");
```

**Benefícios:**
- ✅ 75% menos código
- ✅ Configurável via dados
- ✅ Sem criar nova classe
- ✅ Extensível sem recompilação

---

## 🎓 Próximos Passos

Após migrar:

1. **Leia a documentação completa:**
   - [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md)
   - [EXAMPLES.md](EXAMPLES.md)

2. **Experimente os novos recursos:**
   - Crie tipos personalizados via Registries
   - Use EntityBuilder para código mais limpo
   - Implemente novos sistemas seguindo SOLID

3. **Contribua:**
   - Documente novos padrões que descobrir
   - Compartilhe exemplos de uso
   - Sugira melhorias

---

**Data de Migração:** 2024
**Versão:** 1.0.0
