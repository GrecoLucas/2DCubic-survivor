# 🎮 Cube Survivor - Guia de Início Rápido

## 🚀 Você é novo aqui?

Escolha seu nível de experiência:

### 👶 Iniciante (Nunca vi este código)
```
1. Leia: README.md (5 min)
2. Veja: docs/QUICK_REFERENCE.md - Seção "Comandos Rápidos" (10 min)
3. Pratique: Adicione um novo inimigo usando EnemyRegistry (15 min)
```

### 🧑‍💻 Intermediário (Já conheço ECS)
```
1. Leia: docs/ARCHITECTURE_GUIDE.md (30 min)
2. Veja: docs/EXAMPLES.md - Seção "Boss com Minions" (20 min)
3. Pratique: Crie um novo componente e sistema (1 hora)
```

### 🧙 Avançado (Quero criar extensões)
```
1. Leia: docs/EXAMPLES.md - Seção "Criando um Mod" (30 min)
2. Veja: docs/ARCHITECTURE_DIAGRAM.md (15 min)
3. Pratique: Desenvolva um mod completo (2+ horas)
```

---

## 📚 Documentação por Objetivo

### "Quero entender a arquitetura"
➡️ **[ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md)**
- Princípios SOLID explicados
- Como funciona o ECS
- Estrutura de arquivos
- Padrões de design

### "Quero ver exemplos de código"
➡️ **[EXAMPLES.md](docs/EXAMPLES.md)**
- Inicialização do jogo
- Boss com minions
- Sistema de dash
- Criar um mod

### "Preciso de uma referência rápida"
➡️ **[QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md)**
- Checklists SOLID
- Comandos rápidos
- Erros comuns
- Templates

### "Quero adicionar algo novo"
➡️ Veja a tabela abaixo ⬇️

---

## ➕ Como Adicionar...

| O QUÊ | ONDE | TEMPO |
|-------|------|-------|
| **Novo Inimigo** | [QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md#criar-novo-inimigo) | 2 min |
| **Nova Arma** | [QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md#criar-nova-arma) | 2 min |
| **Novo Bioma** | [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md#novo-bioma) | 5 min |
| **Novo Componente** | [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md#novo-componente) | 15 min |
| **Novo Sistema** | [ARCHITECTURE_GUIDE.md](docs/ARCHITECTURE_GUIDE.md#novo-sistema) | 20 min |
| **Novo Mod** | [EXAMPLES.md](docs/EXAMPLES.md#mod) | 1-2 horas |

---

## 🗺️ Mapa da Documentação

```
📁 docs/
│
├─ 📄 INDEX.md ........................ Índice completo com navegação
├─ 📄 REFACTORING_SUMMARY.md .......... Resumo de tudo que foi feito
│
├─ 🏛️ ARQUITETURA
│  ├─ ARCHITECTURE_GUIDE.md ........... Guia completo (SOLID + ECS)
│  ├─ ARCHITECTURE_DIAGRAM.md ......... Diagramas visuais
│  └─ MIGRATION_GUIDE.md .............. Como migrar código antigo
│
├─ 💡 APRENDIZADO
│  ├─ QUICK_REFERENCE.md .............. Referência rápida
│  ├─ EXAMPLES.md ..................... Exemplos práticos
│  └─ TESTING_GUIDE.md ................ Como testar
│
└─ 📖 SISTEMAS ESPECÍFICOS
   ├─ CONSTRUCTION_SYSTEM.md .......... Sistema de construção
   ├─ SOCKET_ATTACHMENT_SYSTEM.md ..... Sistema de anexos
   └─ WORLD_SYSTEM.md ................. Sistema de mundo/biomas
```

---

## ⚡ Ações Rápidas

### Adicionar Inimigo Rápido (30 segundos)

```csharp
// Cole isto no início do seu código
EnemyRegistry.Instance.Register("meu_inimigo", new EnemyDefinition
{
    Name = "Meu Inimigo",
    Health = 100f,
    Damage = 15f,
    Speed = 200f,
    AttackCooldown = 1f,
    Width = 45f,
    Height = 45f,
    ColorR = 255,
    ColorG = 0,
    ColorB = 0
});

// Usar
var enemy = enemyFactory.CreateEnemy(world, position, "meu_inimigo");
```

### Criar Entidade Complexa (EntityBuilder)

```csharp
var boss = new EntityBuilder(world, "Boss")
    .WithTransform(position)
    .WithSprite(Color.DarkRed, 80, 80, RenderLayer.Entities)
    .WithHealth(500f)
    .WithVelocity(100f)
    .WithAI(100f)
    .WithEnemy(25f, 2f)
    .Build();
```

---

## 🎯 Princípios em 30 Segundos

### SOLID
- **S**ingle: Uma classe, uma responsabilidade
- **O**pen/Closed: Extensível sem modificar
- **L**iskov: Subclasses substituíveis
- **I**nterface Segregation: Interfaces pequenas
- **D**ependency Inversion: Dependa de abstrações

### ECS
- **Entity**: Container (ID + lista de componentes)
- **Component**: Apenas dados
- **System**: Apenas lógica

---

## 🔗 Links Diretos

- 📖 [Guia Completo de Arquitetura](docs/ARCHITECTURE_GUIDE.md)
- 💡 [Exemplos Práticos](docs/EXAMPLES.md)
- ⚡ [Referência Rápida](docs/QUICK_REFERENCE.md)
- 🗺️ [Índice Completo](docs/INDEX.md)
- 🔄 [Guia de Migração](docs/MIGRATION_GUIDE.md)
- 🧪 [Guia de Testes](docs/TESTING_GUIDE.md)
- 📊 [Diagramas](docs/ARCHITECTURE_DIAGRAM.md)

---

## ❓ FAQ

### "Por onde começo?"
➡️ Leia o [README.md](README.md), depois [QUICK_REFERENCE.md](docs/QUICK_REFERENCE.md)

### "Como adiciono um novo inimigo?"
➡️ Veja [QUICK_REFERENCE.md - Criar Novo Inimigo](docs/QUICK_REFERENCE.md)

### "Não entendo SOLID"
➡️ Leia [ARCHITECTURE_GUIDE.md - Princípios SOLID](docs/ARCHITECTURE_GUIDE.md#princípios-solid)

### "Quero ver código de exemplo"
➡️ Veja [EXAMPLES.md](docs/EXAMPLES.md)

### "Como testo meu código?"
➡️ Veja [TESTING_GUIDE.md](docs/TESTING_GUIDE.md)

### "Quebrei alguma coisa, e agora?"
➡️ Veja [QUICK_REFERENCE.md - Erros Comuns](docs/QUICK_REFERENCE.md)

---

## 🎓 Trilha de Aprendizado

```
Semana 1: Fundamentos
  └─ Leia README + QUICK_REFERENCE
  └─ Adicione 3 tipos de inimigos
  └─ Adicione 2 armas

Semana 2: Arquitetura
  └─ Leia ARCHITECTURE_GUIDE completo
  └─ Crie um novo componente + sistema
  └─ Implemente uma habilidade

Semana 3: Avançado
  └─ Leia EXAMPLES (Mods)
  └─ Crie um mod simples
  └─ Escreva testes

Semana 4: Contribuição
  └─ Melhore a documentação
  └─ Compartilhe seu mod
  └─ Ajude outros desenvolvedores
```

---

## 💪 Desafios

### Nível 1: Iniciante
- [ ] Adicione 5 tipos diferentes de inimigos
- [ ] Crie 3 armas com características únicas
- [ ] Adicione um novo bioma

### Nível 2: Intermediário
- [ ] Crie um sistema de dash para o jogador
- [ ] Implemente um boss que spawna minions
- [ ] Adicione power-ups temporários

### Nível 3: Avançado
- [ ] Desenvolva um mod de classes de personagem
- [ ] Crie um sistema de quests
- [ ] Implemente um sistema de crafting

---

## 🌟 Recursos Externos

- [Princípios SOLID](https://en.wikipedia.org/wiki/SOLID)
- [Entity-Component-System](https://en.wikipedia.org/wiki/Entity_component_system)
- [MonoGame Docs](https://docs.monogame.net/)
- [C# Best Practices](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

## 📞 Precisa de Ajuda?

1. **Consulte a documentação**
   - Use o índice: [INDEX.md](docs/INDEX.md)
   - Busque exemplos: [EXAMPLES.md](docs/EXAMPLES.md)

2. **Verifique os erros comuns**
   - [QUICK_REFERENCE.md - Erros Comuns](docs/QUICK_REFERENCE.md)

3. **Veja o código fonte**
   - Muitos exemplos em `src/`

---

**Dica:** Marque esta página nos favoritos! 🔖

**Última atualização:** 2024
**Versão:** 1.0.0
