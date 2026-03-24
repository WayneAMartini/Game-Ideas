# STRATA
### Dig. Appraise. Sell. Go Deeper.

A 2D mining/digging game built with Phaser 3. Descend through layers of compressed history, unearth treasures, and sell them to a cast of buyers in a dynamic marketplace.

## How to Play

Open `index.html` in Chrome (works via `file://` protocol - no server needed).

### The Loop

1. **Prep** - Check your gear, stamina, and market conditions
2. **Descend** - Dig down through 4 distinct strata
3. **Collect** - Find items embedded in the earth (watch for sparkles!)
4. **Return** - Press E to return to the surface when your bag is full or stamina is low
5. **Sell** - Visit the Market to sell items to faction buyers
6. **Upgrade** - Spend gold on 3 branching upgrade trees
7. **Contracts** - Accept bounties for bonus gold and reputation
8. **Repeat** - Each expedition generates a new world

### Controls

| Key | Action |
|-----|--------|
| WASD / Arrow Keys | Move & Jump |
| DOWN / S | Dig below you |
| SHIFT + Direction | Dig sideways |
| E | Return to surface (underground) |
| M | Market (surface) |
| U | Upgrades (surface) |
| C | Contracts (surface) |
| I | Inventory (anywhere) |
| ESC | Back / Close |

### The Strata

| Depth | Name | What You'll Find |
|-------|------|-----------------|
| 0-45 | **The Dump** | Scrap metal, electronics, vintage items |
| 45-95 | **The Foundation** | Industrial-era remains, coal, machine parts |
| 95-155 | **The Catacombs** | Medieval artifacts, pottery, jewelry |
| 155-205 | **The Ruin** | Ancient temple artifacts, gold figurines, void crystals |

### Buyer Factions

- **The Curator** - Pays well for historical artifacts and collections
- **The Foundry** - Wants metals, ores, and machine parts
- **Finn's Fences** - Buys anything, no questions asked, but pays less

### Economy

Prices are dynamic - supply and demand shift between expeditions. Market events (Founder's Festival, Industrial Boom, etc.) temporarily boost prices for certain items. Build reputation with factions for permanent price bonuses.

### Upgrade Trees

- **Excavation** - Dig faster (Power), preserve items (Precision), or clear areas (Range)
- **Logistics** - Carry more (Capacity), move faster (Speed), or automate hauling (Automation)
- **Commerce** - Better appraisal (Appraisal), higher prices (Negotiation), or more buyers (Network)

Each tier offers 3 mutually exclusive choices. You can't max everything - specialize!

## Tech Overview

- **Engine**: Phaser 3 (loaded from CDN)
- **Graphics**: 100% procedural - all textures generated via Phaser's Graphics API + `generateTexture()`
- **Audio**: Procedural via Web Audio API (no external audio files)
- **Physics**: Custom tile-based collision (not using Arcade Physics bodies)
- **Lighting**: RenderTexture with erase-based headlamp effect
- **Architecture**: Multiple script files with shared `STRATA` namespace (no ES6 modules)
- **No build tools**: Works directly from `file://` protocol

### File Structure

```
strata-phaser/
  index.html          - Entry point, loads Phaser CDN + all scripts
  js/
    config.js          - Game constants, item/strata/upgrade data, shared state
    textures.js        - Procedural texture generation (tiles, player, UI, particles)
    audio.js           - Web Audio API sound system
    systems.js         - Economy, contracts game logic
    boot-scene.js      - BootScene (texture gen) + MenuScene (title screen)
    game-scene.js      - GameScene (world gen, player, digging, HUD, lighting)
    surface-scenes.js  - Market, Upgrades, Contracts, Prep, Summary, Inventory scenes
    main.js            - Phaser game config and launch
```

### Game Data

- 36 unique items across 4 strata with 4 rarity tiers
- 3 buyer factions with dynamic pricing and reputation
- 3 upgrade trees with 4 tiers and 3 branches each (27 upgrades total)
- 4 market event types
- Procedural contract generation
