# Guia do Sistema de Áreas e Mundos

Este guia explica como funciona o sistema de áreas do Cube Survivor, como expandir o mapa e editar as configurações de cada região.

## 1. Visão Geral
O mundo do jogo é modular. A estrutura global (quem conecta com quem) é definida visualmente em um arquivo de texto, enquanto os detalhes de cada área (inimigos, texturas, tamanho) ficam em arquivos JSON separados.

## 2. O Mapa Global (`assets/map.txt`)
O arquivo `assets/map.txt` define o layout do mundo. O jogo lê este arquivo como uma grade.

- **IDs de Área**: Cada caractere (letra ou número) representa uma área única.
- **Conexões**: Se dois caracteres diferentes estão lado a lado (horizontal ou vertical), o jogo cria automaticamente um **Portal** entre eles.
- **Vazio**: O caractere `.` (ponto) representa espaço vazio (sem área).

**Exemplo:**
```text
A1 A2
.. A3
```
Neste exemplo:
- `A1` conecta-se à direita com `A2`.
- `A2` conecta-se abaixo com `A3`.

## 3. Detalhes da Área (`assets/areas/ID.json`)
Para cada ID definida no mapa (ex: `A1`), o jogo procura um arquivo JSON correspondente em `assets/areas/A1.json`.

### Estrutura do JSON
```json
{
  "mapWidth": 2000,       // Largura total da área em pixels
  "mapHeight": 2000,      // Altura total da área em pixels
  "biomes": [             // Lista de biomas (texturas de chão e regras)
    {
      "area": { "x": 0, "y": 0, "width": 2000, "height": 2000 },
      "type": "Cave",             // Tipo lógico (Forest, Cave)
      "textureKey": "cave",       // Nome da textura em assets/textures (sem .png)
      "allowsEnemySpawns": true,  // Se inimigos nascem aqui
      "treeDensity": 0            // Densidade de árvores (se Forest)
    }
  ],
  "crates": [...],        // Caixas destrutíveis
  "safeZones": [...],     // Zonas seguras (paredes)
  "pickups": [...]        // Itens no chão (madeira, armas)
}
```

## 4. Texturas (`assets/textures/`)
O sistema carrega automaticamente todas as imagens desta pasta.
- **Como adicionar**: Basta colar um arquivo `.png` ou `.jpg` na pasta `assets/textures`.
- **Como usar**: No JSON da área, use o nome do arquivo (sem extensão) no campo `"textureKey"`.
    - Exemplo: Se você adicionou `lava.png`, use `"textureKey": "lava"`.

## 5. Passo a Passo: Criando uma Nova Área

1.  **Defina no Mapa**:
    - Abra `assets/map.txt`.
    - Adicione uma nova ID (ex: `B1`) adjacente a uma área existente.

2.  **Crie a Configuração**:
    - Vá em `assets/areas/`.
    - Crie um arquivo `B1.json` (copie de um existente para facilitar).

3.  **Personalize**:
    - Edite `B1.json` para definir o tamanho, texturas e inimigos.
    - Se quiser um chão novo, adicione a imagem em `assets/textures/` e atualize o `textureKey`.

4.  **Jogue**:
    - Inicie o jogo. O portal para a nova área aparecerá automaticamente na posição correta.
