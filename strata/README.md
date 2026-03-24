# STRATA

**Dig. Appraise. Sell. Go Deeper.**

A 2D digging/mining game with a dynamic economy, deep upgrade trees, and the thrill of unearthing lost civilizations layer by layer.

## How to Play

1. Open `index.html` in a modern browser (Chrome, Firefox, Safari, Edge)
2. Click **NEW GAME** to start
3. You can also serve it locally: `python3 -m http.server 8080` then visit `http://localhost:8080`

## Controls

### Movement
- **Arrow Keys** or **WASD** — Move left/right
- **Space** or **Up/W** — Jump
- **Down/S** — Dig downward
- **Shift + Left/Right** — Dig sideways

### Menus (Surface only)
- **M** — Open Market (sell items to factions)
- **U** — Open Upgrades (buy skill tree upgrades)
- **C** — Open Contracts (bounty board)
- **I** — Open Inventory (view your bag)
- **E** — Return to Surface (while underground)
- **F** — Toggle audio
- **Escape** — Close current menu

## Gameplay Loop

1. **Dig** — Descend into The Column, breaking through 4 distinct strata
2. **Collect** — Find items embedded in the terrain (watch for sparkles!)
3. **Return** — Press E to return to surface (or get forced back at 0 stamina)
4. **Sell** — Visit the Hollowmarket to sell items to 3 buyer factions
5. **Upgrade** — Spend gold on Excavation, Logistics, or Commerce skill trees
6. **Repeat** — Go deeper with better gear for rarer, more valuable finds

## The Strata

| Depth | Stratum | Theme |
|-------|---------|-------|
| 0-120m | The Dump | Modern refuse, scrap metal, electronics |
| 120-270m | The Foundation | Industrial era remains, steel, coal |
| 270-450m | The Catacombs | Medieval burial grounds, jewelry, weapons |
| 450-600m | The Ruin | Ancient civilization, gold artifacts, ritual objects |

## Economy

Three buyer factions with dynamic pricing:
- **The Curator** — Pays top price for historical artifacts
- **The Foundry** — Wants metals, ores, and machine parts
- **Finn's Fences** — Buys anything but pays less

Prices fluctuate based on supply/demand, market events, and your faction reputation.

## Tech

Pure HTML5 Canvas + vanilla JavaScript (ES6 modules). No frameworks, no build tools, no dependencies. Procedural audio via Web Audio API.
