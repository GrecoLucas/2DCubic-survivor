# Construction & Resource System

## Visão Geral

Sistema completo de recursos e construção que permite ao jogador coletar madeira e construir estruturas (caixas) quando possui um martelo.

---

## 📦 Componentes do Sistema

### 1. **Itens**

#### WoodItem (`src/Inventory/Items/Resources/WoodItem.cs`)
- **Tipo**: Material
- **Max Stack**: 99
- **Descrição**: Recurso usado para construção
- **Uso**: Consumido ao construir caixas (4 unidades por caixa)

#### HammerItem (`src/Inventory/Items/Tools/HammerItem.cs`)
- **Tipo**: Tool
- **Max Stack**: 1
- **Descrição**: Ferramenta que habilita construção
- **Uso**: Deve estar no inventário para construir

### 2. **Entity Factories**

#### WoodEntityFactory (`src/Entities/Factories/WoodEntityFactory.cs`)
- Cria entidades de madeira no mundo
- Usa textura "wood" se disponível, senão cor marrom (#8B5A2B)
- Tamanho: 18x18 pixels
- Render Layer: GroundItems

#### HammerEntityFactory (`src/Entities/Factories/HammerEntityFactory.cs`)
- Cria entidades de martelo no mundo
- Usa textura "hammer" se disponível, senão cor cinza (#A9A9A9)
- Tamanho: 22x22 pixels
- Render Layer: GroundItems

### 3. **Componentes**

#### BuilderComponent (`src/Components/Construction/BuilderComponent.cs`)
```csharp
public sealed class BuilderComponent : Component
{
    public float BuildRange { get; }                      // Distância máxima de construção
    public Vector2? RequestedBuildPosition { get; set; }  // Posição solicitada para construção
    public void ClearRequest()
}
```

**Uso:**
- Anexado ao jogador no `PlayerFactory`
- Range padrão: 300 pixels (configurável via `GameConfig.PlayerBuildRange`)

### 4. **Sistemas**

#### ConstructionSystem (`src/Systems/Construction/ConstructionSystem.cs`)
Sistema responsável por processar solicitações de construção.

**Fluxo de Construção:**
1. Verifica se o jogador tem martelo no inventário
2. Verifica se tem pelo menos 4 unidades de madeira
3. Valida se a posição está dentro do alcance (`BuildRange`)
4. Encaixa a posição numa grade (baseada em `WallBlockSize`)
5. Verifica se a posição está livre de colliders
6. Consome 4 unidades de madeira
7. Cria uma caixa destrutível na posição

**Validações:**
- ✅ Tem martelo?
- ✅ Tem >= 4 madeiras?
- ✅ Está dentro do alcance?
- ✅ Posição está livre?

#### ResourceSpawnSystem (`src/Systems/World/ResourceSpawnSystem.cs`)
Sistema que spawna madeira periodicamente no mapa.

**Comportamento:**
- **Intervalo**: 30 segundos (configurável via `GameConfig.WoodSpawnIntervalSeconds`)
- **Regiões**: Definidas no JSON (`woodSpawnRegions`)
- **Limite**: Cada região tem um `maxActiveWood` (quantidade máxima ativa)
- **Spawn Logic**:
  - Conta madeira ativa na região
  - Se abaixo do limite, tenta spawnar nova madeira
  - Até 10 tentativas de encontrar posição livre
  - Evita spawnar em colliders

---

## 🎮 Controles

### Input de Construção
**Right-Click (Botão Direito do Mouse) quando tem Hammer no inventário**

**Pré-requisitos:**
1. ✅ Ter **Hammer** no inventário (não precisa estar equipado)
2. ✅ Ter pelo menos **4 Wood** no inventário
3. ✅ Estar dentro do alcance de construção (300 pixels)
4. ✅ Posição de construção deve estar livre

**Como usar:**
1. Pegue o Hammer (spawn no outro lado do mapa em 2500, 1500)
2. Colete madeira (pickups iniciais ou espere spawn periódico)
3. Clique com o **botão direito** onde deseja construir
4. Sistema valida automaticamente e constrói se possível

Implementado em `PlayerInputSystem`:
```csharp
bool buildClick = mouseState.RightButton == ButtonState.Pressed;

if (buildClick && inventory.HasItem("hammer", 1))
{
    builder.RequestedBuildPosition = mouseWorldPos;
}
```

**Debug Logs:**
- `[PlayerInput] ✓ Build solicitado em (X, Y)` - Input aceito
- `[PlayerInput] ⚠ Precisa de Hammer no inventário!` - Falta hammer
- `[Construction] ✓ Caixa construída em (X, Y)!` - Sucesso
- `[Construction] ⚠ Precisa de 4 Wood!` - Falta madeira
- `[Construction] ⚠ Muito longe!` - Fora do alcance
- `[Construction] ⚠ Posição bloqueada!` - Há collider no local

---

## 📝 Configuração (GameConfig)

```csharp
// Construção e recursos
public const float PlayerBuildRange = 300f;      // Distância máxima de construção
public const int WoodPerCrate = 4;               // Madeira necessária por caixa
public const float WoodSpawnIntervalSeconds = 30f; // Intervalo de spawn de madeira
```

---

## 🗺️ Configuração do Mapa (world1.json)

### Pickups Iniciais
```json
"pickups": [
  {
    "x": 2500,
    "y": 1500,
    "type": "hammer",
    "amount": 1
  },
  {
    "x": 1200,
    "y": 900,
    "type": "wood",
    "amount": 5
  }
]
```

### Regiões de Spawn de Madeira
```json
"woodSpawnRegions": [
  {
    "x": 500,
    "y": 500,
    "width": 3000,
    "height": 3000,
    "maxActiveWood": 20
  }
]
```

**Parâmetros:**
- `x`, `y`: Posição do canto superior esquerdo da região
- `width`, `height`: Dimensões da região em pixels
- `maxActiveWood`: Quantidade máxima de madeira ativa na região

---

## 🔄 Fluxo de Jogo

### 1. **Início do Jogo**
- Martelo spawn em (2500, 1500) - outro lado do mapa
- 2 pilhas de madeira (5 unidades cada) spawn próximo ao jogador
- ResourceSpawnSystem começa timer de 30s

### 2. **Coleta de Recursos**
- Jogador caminha até os pickups (raio de 50 pixels)
- Martelo e madeira vão automaticamente para o inventário
- Pickups aparecem no inventário com suas texturas

### 3. **Construção**
- Jogador segura **B** e clica com o **botão direito** onde quer construir
- Sistema valida todas as condições
- Se válido:
  - 4 madeiras são consumidas
  - Caixa destrutível é criada na posição (grid-snapped)
  - Caixa tem 50 HP e pode ser destruída por balas
  - Mensagem de sucesso no console

### 4. **Spawn Periódico**
- A cada 30 segundos, `ResourceSpawnSystem` tenta spawnar madeira
- Verifica quantas madeiras estão ativas em cada região
- Se abaixo do limite (`maxActiveWood`), spawn 1 unidade nova
- Posição aleatória dentro da região, evitando colliders

---

## 🛠️ Arquitetura & Design Patterns

### Factory Pattern
- `WoodEntityFactory` e `HammerEntityFactory` encapsulam criação de entidades
- Suportam `TextureManager` para texturas opcionais
- Seguem o mesmo padrão de outras factories (Apple, Brain, Gun)

### Component-Entity-System (ECS)
- **BuilderComponent**: Capacidade de construir
- **ConstructionSystem**: Lógica de construção
- **ResourceSpawnSystem**: Lógica de spawn de recursos
- Separação clara de responsabilidades

### Data-Driven Design
- Configurações centralizadas em `GameConfig`
- Mapa e pickups definidos em JSON
- Regiões de spawn configuráveis via JSON
- Fácil balanceamento sem recompilação

### SOLID Principles
- **Single Responsibility**: Cada componente/sistema tem uma responsabilidade
- **Open/Closed**: Novo tipo de recurso pode ser adicionado estendendo o sistema
- **Dependency Inversion**: Sistemas dependem de abstrações (`IWorldObjectFactory`, `IGameWorld`)

---

## 🔮 Extensibilidade Futura

### Novos Recursos
Para adicionar um novo recurso (ex: "Stone"):
1. Criar `StoneItem` em `src/Inventory/Items/Resources/`
2. Criar `StoneEntityFactory` em `src/Entities/Factories/`
3. Adicionar ao JSON e `LevelDefinition`
4. Estender `ResourceSpawnSystem` ou criar novo sistema

### Novas Construções
Para adicionar novo tipo de construção (ex: "Wall"):
1. Adicionar método em `IWorldObjectFactory`
2. Implementar em `WorldObjectFactory`
3. Estender `ConstructionSystem` com nova lógica de validação
4. Adicionar custo de recursos em `GameConfig`

### Melhorias Possíveis
- [ ] Sistema de crafting com receitas complexas
- [ ] UI visual de construção (preview de ghost)
- [ ] Diferentes tipos de ferramentas com durabilidade
- [ ] Upgrade de ferramentas
- [ ] Sistema de blueprint/receitas
- [ ] Recursos de diferentes qualidades
- [ ] Modo de construção rápida (hold B + drag)

---

## 🐛 Debug & Console Output

### Logs de Construção
```
[Construction] ⚠ Precisa de um Hammer para construir!
[Construction] ⚠ Precisa de 4 Wood para construir! (Tem: 2)
[Construction] ⚠ Muito longe! Distância: 450 / 300
[Construction] ⚠ Posição bloqueada!
[Construction] ✓ Caixa construída em (1000, 800)!
```

### Logs de Resource Spawn
```
[ResourceSpawn] ✓ Madeira spawn em (1234, 5678)
```

---

## 📚 Arquivos Relacionados

### Core
- `src/Components/Construction/BuilderComponent.cs`
- `src/Systems/Construction/ConstructionSystem.cs`
- `src/Systems/World/ResourceSpawnSystem.cs`

### Items & Factories
- `src/Inventory/Items/Resources/WoodItem.cs`
- `src/Inventory/Items/Tools/HammerItem.cs`
- `src/Entities/Factories/WoodEntityFactory.cs`
- `src/Entities/Factories/HammerEntityFactory.cs`

### Configuration
- `src/Game/Configuration/GameConfig.cs`
- `src/Game/Configuration/LevelDefinition.cs`
- `src/Game/Configuration/WorldConfigModels.cs`
- `src/Game/Configuration/WorldDefinitionLoader.cs`

### Integration
- `src/Entities/Factories/PlayerFactory.cs` (adiciona `BuilderComponent`)
- `src/Systems/Input/PlayerInputSystem.cs` (input de construção)
- `src/Game/Game1.cs` (inicialização de sistemas)
- `assets/world1.json` (configuração do mapa)

---

## ✅ Checklist de Implementação

- [x] **STEP 1**: Criar WoodItem e HammerItem
- [x] **STEP 2**: Sistema de pickup compatível (já existente)
- [x] **STEP 3**: Adicionar ao LevelDefinition e JSON
- [x] **STEP 4**: ResourceSpawnSystem para spawn periódico
- [x] **STEP 5**: BuilderComponent e input de construção
- [x] **STEP 6**: ConstructionSystem completo
- [x] **STEP 7**: Constantes em GameConfig e testes

**Status**: ✅ Sistema 100% funcional e testado!

