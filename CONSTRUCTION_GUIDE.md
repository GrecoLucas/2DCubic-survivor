# 🔨 Guia Rápido: Sistema de Construção

## Como Construir

### Passo 1: Pegue o Hammer 🔨
- Hammer está localizado no **outro lado do mapa** (coordenadas 2500, 1500)
- Apenas caminhe até ele para pegar automaticamente
- Ele vai aparecer no seu inventário

### Passo 2: Colete Madeira 🪵
- Há **2 pilhas de 5 madeiras** próximas ao spawn inicial (1200, 900) e (1500, 1100)
- **Nova madeira spawna a cada 30 segundos** automaticamente no mapa
- Você precisa de **4 madeiras** para construir 1 caixa

### Passo 3: Construa! 🏗️
1. Certifique-se de ter **Hammer** e pelo menos **4 Wood** no inventário
2. **Clique com o botão direito do mouse** onde deseja construir
3. A construção só funciona se:
   - ✅ Você está **dentro de 300 pixels** do local
   - ✅ O local está **livre** (sem paredes, caixas ou outras obstruções)
   - ✅ Você tem **4 Wood** no inventário

### O que você pode construir?
- **Caixas destrutíveis** (50 HP cada)
- Bloqueiam movimento de jogador e inimigos
- Bloqueiam balas
- Podem ser destruídas atirando nelas

## Texturas Necessárias

Coloque estas imagens em `assets/`:
- `hammer.png` (32x32 pixels) ✅ JÁ EXISTE
- `wood.png` (32x32 pixels) ⚠️ CRIAR

Se não houver textura, o jogo usa cores:
- Hammer: Cinza (#A9A9A9)
- Wood: Marrom (#8B5A2B)

## Troubleshooting

### "Não consigo construir nada!"

**Verificações:**
1. ✅ Você tem o Hammer no inventário?
   - Abra o inventário (tecla I) e verifique
   - Se não tiver, vá até (2500, 1500) pegar

2. ✅ Você tem pelo menos 4 madeiras?
   - Abra o inventário e conte as madeiras
   - Se não tiver, procure pickups ou espere 30s para novo spawn

3. ✅ Você está clicando com o botão DIREITO?
   - Left-click = Atirar
   - **Right-click = Construir**

4. ✅ Você está perto o suficiente?
   - Máximo 300 pixels de distância
   - Tente clicar mais perto de onde você está

5. ✅ O local está livre?
   - Não construa em cima de paredes
   - Não construa em cima de outras caixas
   - Tente um espaço aberto

### Mensagens de Debug

Abra o console e procure por:
- `[PlayerInput] ✓ Build solicitado` → Input funcionou
- `[Construction] ✓ Caixa construída` → **SUCESSO!**
- `[Construction] ⚠ Precisa de Hammer` → Falta hammer
- `[Construction] ⚠ Precisa de 4 Wood` → Falta madeira
- `[Construction] ⚠ Muito longe` → Clique mais perto
- `[Construction] ⚠ Posição bloqueada` → Local ocupado

## Configs (GameConfig.cs)

```csharp
PlayerBuildRange = 300f;           // Alcance de construção
WoodPerCrate = 4;                   // Madeira por caixa
WoodSpawnIntervalSeconds = 30f;     // Intervalo de spawn
```

## Configuração do Mapa (world1.json)

```json
{
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
    },
    {
      "x": 1500,
      "y": 1100,
      "type": "wood",
      "amount": 5
    }
  ],
  "woodSpawnRegions": [
    {
      "x": 500,
      "y": 500,
      "width": 3000,
      "height": 3000,
      "maxActiveWood": 20
    }
  ]
}
```

## Teste Rápido

1. Execute o jogo
2. Vá até (2500, 1500) e pegue o Hammer
3. Colete as 2 pilhas de madeira (total 10 wood)
4. Volte próximo ao spawn
5. **Right-click** em um espaço vazio
6. Deve aparecer uma caixa marrom!

**Se funcionar:** ✅ Sistema completo!
**Se não funcionar:** Verifique o console para ver qual validação falhou.

