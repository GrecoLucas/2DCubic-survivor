# 📚 Índice da Documentação - Cube Survivor

## 🎯 Comece Aqui

Novo no projeto? Siga esta ordem:

1. **[README.md](../README.md)** - Visão geral do projeto
2. **[QUICK_REFERENCE.md](QUICK_REFERENCE.md)** - Guia rápido de referência
3. **[ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md)** - Arquitetura detalhada
4. **[EXAMPLES.md](EXAMPLES.md)** - Exemplos práticos

---

## 📖 Documentação Completa

### Arquitetura e Design

#### [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md)
**O que é:** Guia completo sobre a arquitetura do projeto
**Quando usar:** Para entender os princípios SOLID e ECS implementados

**Conteúdo:**
- ✅ Princípios SOLID explicados com exemplos
- ✅ Arquitetura ECS detalhada
- ✅ Como adicionar novos biomas, inimigos, armas
- ✅ Como criar novos componentes e sistemas
- ✅ Padrões de design utilizados
- ✅ Boas práticas e anti-padrões
- ✅ Estrutura de arquivos recomendada

**Ideal para:** Entender a arquitetura antes de modificar código

---

#### [QUICK_REFERENCE.md](QUICK_REFERENCE.md)
**O que é:** Referência rápida para consulta durante desenvolvimento
**Quando usar:** Quando você já entende a arquitetura e precisa de um lembrete rápido

**Conteúdo:**
- ✅ Checklist SOLID
- ✅ Comandos rápidos para criar elementos
- ✅ Padrões de nomenclatura
- ✅ Erros comuns e como evitá-los
- ✅ Templates de código
- ✅ Dicas de debugging

**Ideal para:** Ter aberto enquanto programa

---

#### [EXAMPLES.md](EXAMPLES.md)
**O que é:** Exemplos práticos de código prontos para usar
**Quando usar:** Quando você quer implementar algo específico

**Conteúdo:**
- ✅ Inicialização completa do jogo
- ✅ Criar novo tipo de inimigo (boss com minions)
- ✅ Criar sistema de habilidades (dash)
- ✅ Criar mod/extensão (sistema de classes)
- ✅ Código completo, pronto para copiar e adaptar

**Ideal para:** Aprender fazendo com exemplos reais

---

### Sistemas Específicos

#### [CONSTRUCTION_SYSTEM.md](CONSTRUCTION_SYSTEM.md)
**O que é:** Documentação do sistema de construção
**Quando usar:** Ao trabalhar com construção de estruturas

**Conteúdo:**
- Sistema de building
- Componentes relacionados
- Como adicionar novos blueprints

---

#### [SOCKET_ATTACHMENT_SYSTEM.md](SOCKET_ATTACHMENT_SYSTEM.md)
**O que é:** Documentação do sistema de sockets e anexos
**Quando usar:** Ao trabalhar com armas e equipamentos anexados

**Conteúdo:**
- Sistema de sockets
- Como anexar armas ao jogador
- Renderização de attachments

---

#### [WORLD_SYSTEM.md](WORLD_SYSTEM.md)
**O que é:** Documentação do sistema de mundo
**Quando usar:** Ao trabalhar com biomas, spawn de recursos

**Conteúdo:**
- Sistema de biomas
- Spawn de recursos
- Zonas seguras
- Configuração de mundo via JSON

---

### Histórico e Referência

#### [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)
**O que é:** Resumo completo da refatoração SOLID + ECS
**Quando usar:** Para entender as mudanças arquiteturais principais

**Conteúdo:**
- Arquivos criados/modificados
- Princípios SOLID implementados
- Arquitetura ECS pura
- Métricas de impacto

---

#### [DEVELOPMENT_HISTORY.md](DEVELOPMENT_HISTORY.md)
**O que é:** Histórico consolidado de refatorações e melhorias
**Quando usar:** Para entender o que foi feito e quando

**Conteúdo:**
- Refatoração Profunda - Sistema de Mapas V2 + Editor
- Menu ESC (Pause Menu)
- Sistema de Construção
- Sistema de Mapas
- Editor de Mapas
- Estatísticas de refatoração

---

### Histórico e Referência

#### [REFACTORING_SUMMARY.md](REFACTORING_SUMMARY.md)
**O que é:** Resumo completo da refatoração SOLID + ECS
**Quando usar:** Para entender as mudanças arquiteturais principais

**Conteúdo:**
- Arquivos criados/modificados
- Princípios SOLID implementados
- Arquitetura ECS pura
- Métricas de impacto

---

#### [DEVELOPMENT_HISTORY.md](DEVELOPMENT_HISTORY.md)
**O que é:** Histórico consolidado de refatorações e melhorias
**Quando usar:** Para entender o que foi feito e quando

**Conteúdo:**
- Refatoração Profunda - Sistema de Mapas V2 + Editor
- Menu ESC (Pause Menu)
- Sistema de Construção
- Sistema de Mapas
- Editor de Mapas
- Estatísticas de refatoração

---

## 🗺️ Guia de Navegação por Tarefa

### "Quero adicionar um novo inimigo"
1. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) → Seção "Criar Novo Inimigo"
2. [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) → Seção "Adicionar Novo Inimigo"
3. [EXAMPLES.md](EXAMPLES.md) → Seção "Criando um Novo Tipo de Inimigo"

### "Quero adicionar uma nova arma"
1. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) → Seção "Criar Nova Arma"
2. [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) → Seção "Adicionar Nova Arma"

### "Quero adicionar um novo bioma"
1. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) → Seção "Criar Novo Bioma"
2. [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) → Seção "Adicionar Novo Bioma"
3. [WORLD_SYSTEM.md](WORLD_SYSTEM.md) → Sistema de Biomas

### "Quero criar um novo sistema/componente"
1. [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) → Seções "Adicionar Novo Componente" e "Adicionar Novo Sistema"
2. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) → Seção "Template de Novo Feature"
3. [EXAMPLES.md](EXAMPLES.md) → Seção "Criando um Sistema de Habilidades"

### "Quero entender a arquitetura"
1. [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) → Início ao fim
2. [QUICK_REFERENCE.md](QUICK_REFERENCE.md) → Seção "Checklist SOLID"

### "Preciso de código de exemplo"
1. [EXAMPLES.md](EXAMPLES.md) → Todos os exemplos
2. Código fonte em `src/` → Ver implementações reais

### "Quero criar um mod"
1. [EXAMPLES.md](EXAMPLES.md) → Seção "Criando um Mod/Extensão"
2. [ARCHITECTURE_GUIDE.md](ARCHITECTURE_GUIDE.md) → Princípios de extensibilidade

---

## 📋 Checklists Úteis

### Antes de Começar a Programar
- [ ] Li o README.md?
- [ ] Entendo os princípios SOLID?
- [ ] Entendo a arquitetura ECS?
- [ ] Tenho o QUICK_REFERENCE.md aberto?

### Ao Adicionar Novo Código
- [ ] Segue os princípios SOLID?
- [ ] Components têm apenas dados?
- [ ] Systems têm apenas lógica?
- [ ] Usei Registries para extensibilidade?
- [ ] Documentei com comentários XML?

### Antes de Fazer Commit
- [ ] Código compila sem erros?
- [ ] Segue a estrutura de arquivos recomendada?
- [ ] Nomeação segue os padrões?
- [ ] Adicionei documentação se necessário?

---

## 🎓 Níveis de Proficiência

### Iniciante
**Você está aqui se:** Acabou de conhecer o projeto

**Leia:**
1. README.md
2. QUICK_REFERENCE.md (Seções de Checklist)
3. ARCHITECTURE_GUIDE.md (Seção "Princípios SOLID")

**Pratique:**
- Adicione um novo inimigo usando o EnemyRegistry
- Crie uma arma customizada

---

### Intermediário
**Você está aqui se:** Já adicionou elementos usando Registries

**Leia:**
1. ARCHITECTURE_GUIDE.md (completo)
2. EXAMPLES.md (Seções de Inicialização e Boss)

**Pratique:**
- Crie um novo componente e sistema
- Implemente uma habilidade para o jogador
- Modifique comportamentos existentes

---

### Avançado
**Você está aqui se:** Já criou componentes e sistemas

**Leia:**
1. EXAMPLES.md (completo)
2. Código fonte para entender implementações

**Pratique:**
- Crie um mod completo
- Desenvolva um novo subsistema (ex: crafting)
- Contribua com melhorias na arquitetura

---

## 🔗 Links Externos Úteis

- [Princípios SOLID](https://en.wikipedia.org/wiki/SOLID)
- [Entity-Component-System Pattern](https://en.wikipedia.org/wiki/Entity_component_system)
- [MonoGame Documentation](https://docs.monogame.net/)
- [C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)

---

## 📞 Precisa de Ajuda?

1. **Consultou a documentação?** Verifique o índice acima
2. **Procurou por exemplos?** Veja EXAMPLES.md
3. **Checou o código fonte?** Muitos exemplos estão implementados

---

## 🔄 Atualizações da Documentação

**Versão:** 1.0.0
**Data:** 2024
**Última atualização:** Implementação inicial completa de SOLID + ECS

---

**Dica:** Marque esta página como favorita para acesso rápido!
