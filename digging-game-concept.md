# STRATA
### Dig. Appraise. Sell. Go Deeper.

*A digging/mining game with a dynamic economy, deep upgrade trees, and the thrill of unearthing lost civilizations layer by layer.*

---

## Elevator Pitch

You are a prospector-merchant in a world built on layers. Beneath your feet lie compressed epochs -- from modern landfills to medieval ruins to ancient temples to primordial crystal caverns. Each stratum holds unique treasures. You dig down, extract what you find, haul it to the surface, and sell it to a cast of buyers who each want different things. The deeper you go, the rarer and more valuable the finds -- but the more dangerous and expensive the expedition. Your profits fuel upgrades across six branching skill trees, letting you specialize your operation however you want: brute-force drill rigs, surgical precision tools, automated logistics, or market manipulation.

What makes STRATA different: **the economy is the game**. You're not just mining to check a progress bar -- you're reading a live market, building buyer relationships, timing your sales, fulfilling contracts, and deciding whether to sell that rare fossil now or hold it until the museum curator's demand spikes next season.

---

## Genre & Platform

- **Genre:** Mining/Management Roguelite with Economy Sim elements
- **Perspective:** 2D side-view (dig downward), with a top-down surface hub (shop/market)
- **Target Platforms:** PC (Steam), Switch, Mobile (adapted)
- **Session Length:** 15-45 min per expedition, with long-term meta progression across runs
- **Multiplayer (stretch goal):** Shared market server -- other players' mining affects supply/demand

---

## The World

The game takes place in **The Column** -- a single, impossibly deep vertical slice of earth beneath the frontier town of **Hollowmarket**. Nobody knows how deep The Column goes. Miners who've gone past Stratum 7 don't always come back. Those who do come back rich.

### The Strata (Biome Layers)

Each stratum has a distinct visual identity, hazard profile, and loot table:

| Stratum | Name | Depth | Theme | Key Finds | Hazards |
|---------|------|-------|-------|-----------|---------|
| 1 | **The Dump** | 0-50m | Modern refuse, topsoil | Scrap metal, recyclables, lost wallets, old electronics | Unstable ground, gas pockets |
| 2 | **The Foundation** | 50-150m | Industrial era remains | Steel beams, coal, machine parts, old currency | Flooded tunnels, rusted pipes |
| 3 | **The Catacombs** | 150-300m | Medieval/colonial burial grounds | Bones, pottery, jewelry, weapons, sealed urns | Cave-ins, cursed traps, darkness |
| 4 | **The Ruin** | 300-500m | Ancient civilization temples | Carved stone, gold artifacts, glyphs, ritual objects | Pressure plates, guardian constructs |
| 5 | **The Fossil Bed** | 500-800m | Prehistoric layers | Dinosaur bones, amber, petrified wood, insect fossils | Extreme heat, magma pockets, gas |
| 6 | **The Crystal Deep** | 800-1200m | Geological wonder zone | Raw gems, rare crystals, geodes, prismatic ore | Crystal storms, electromagnetic interference |
| 7 | **The Hollow** | 1200m+ | Unknown/alien origin | Alien alloys, living stone, void shards, echo relics | Reality distortion, pressure, the unknown |

Each stratum is procedurally generated per expedition. The layout changes, but the depth bands and loot tables stay consistent. Deeper = rarer = more valuable, but also more stamina/fuel cost and more hazards.

---

## Core Gameplay Loop

```
EXPEDITION                          SURFACE
-----------                         --------
Choose depth target  -------->  Prep gear & supplies
Dig down through layers             |
Find items, avoid hazards           |
Manage stamina/fuel/inventory       |
Decide: go deeper or return?        |
      |                             |
      v                             v
Return to surface  ---------->  APPRAISE finds
                                SELL to buyers (or hold)
                                FULFILL contracts
                                CHECK market trends
                                UPGRADE equipment/skills
                                ACCEPT new contracts
                                     |
                                     v
                               START NEXT EXPEDITION
```

### The Dig Phase

- **Movement:** You move freely left/right and dig downward (or sideways). Digging consumes tool durability and stamina.
- **Discovery:** Items are embedded in the terrain, partially visible as color/texture hints. Better surveying tools reveal more.
- **Extraction:** Some items require careful extraction (mini-game) to preserve their value. A shattered artifact is worth 10% of an intact one.
- **Hazards:** Water flooding, cave-ins, gas pockets, heat, hostile creatures (deeper strata), and structural collapse. You must reinforce tunnels or risk losing your path back.
- **The Tension:** You can always go deeper for better loot, but your stamina is draining, your pack is filling up, and the hazards are increasing. Push your luck or play it safe?

### The Surface Phase

- **Appraisal:** Unidentified items need to be appraised before sale. Better appraisal skills reveal hidden value (a "rusty sword" might actually be a legendary blade). Appraisal costs time/money -- or you can sell unappraised at a steep discount.
- **Selling:** Interact with multiple buyer types at the Hollowmarket (see Economy section).
- **Contracts:** Accept bounty-board contracts from buyers ("Bring me 3 intact ammonite fossils," "I need 500g of prismatic ore by end of week").
- **Upgrades:** Spend earnings on your skill trees and equipment.
- **Rest/Recover:** Stamina recovers between expeditions. Longer rest = full recovery but market shifts while you wait.

---

## The Economy System

This is the heart of what makes STRATA unique. The economy is not a simple "sell item for fixed price" system. It's a living, dynamic marketplace.

### Buyer Factions

Six factions operate in Hollowmarket. Each has different demands, price multipliers, and relationship benefits:

| Faction | What They Buy | Why They Pay | Relationship Perk |
|---------|---------------|-------------|-------------------|
| **The Curator** (Museum) | Artifacts, fossils, complete sets | Historical value | Set completion bonuses (2x-5x for full collections) |
| **Gemwright Guild** (Jewelers) | Raw gems, crystals, precious metals | Craft materials | Bulk order contracts, gem cutting service (increases value) |
| **The Foundry** (Blacksmiths/Engineers) | Metals, alloys, machine parts, ores | Industrial use | Equipment crafting discounts, custom tool orders |
| **Madame Nox** (Alchemist/Occultist) | Cursed items, strange minerals, void shards | Research | Potion buffs for expeditions, hazard resistance recipes |
| **Finn's Fences** (Black Market) | Anything, no questions asked | Resale | Instant cash, no appraisal needed, but lower prices. Tips on rare locations |
| **The Archive** (Scholars) | Glyphs, texts, maps, echo relics | Knowledge | Lore reveals (shows what's in deeper strata), survey data |

### Dynamic Pricing

- **Supply/Demand Curves:** Each item type has a base price that fluctuates on a weekly cycle. If you flood the market with copper ore, the price drops. If nobody's brought fossils in a while, the Curator pays double.
- **Seasonal Events:** Periodic events shift demand. "The Founder's Festival" increases demand for historical artifacts. "Crystal Fair" boosts gem prices. "War Footing" increases metal prices.
- **Market Board:** A visible board in Hollowmarket shows current price trends (up/down arrows), active contracts, and upcoming events. Savvy players read this before deciding what to dig for.
- **Price Memory:** You can track price history for items you've sold before, helping you time your sales.

### Contracts & Reputation

- **Contracts:** Each faction posts contracts on the bounty board. Completing contracts earns bonus money AND reputation with that faction.
- **Reputation Tiers:** Neutral -> Trusted -> Preferred -> Partner -> Patron. Each tier unlocks new benefits:
  - **Trusted:** 10% price bonus, access to tier-2 contracts
  - **Preferred:** 20% price bonus, exclusive inventory (potions, tools, information)
  - **Partner:** 30% price bonus, passive income from faction (they sell your goods to their network)
  - **Patron:** 40% price bonus, unique faction ability (e.g., The Foundry builds you a custom drill; The Archive gives you a deep-scan map)
- **Faction Tension:** Selling exclusively to one faction may upset others. Selling cursed items to Madame Nox makes the Curator suspicious. Balance relationships or specialize -- your call.

### The Stockpile

- You have a warehouse on the surface where you can store items instead of selling them immediately.
- **Hold for profit:** Store items when prices are low, sell when they spike.
- **Set assembly:** Collect pieces of artifact sets over multiple expeditions. Complete sets are worth 3-5x their individual values.
- **Warehouse capacity** is upgradeable (part of the Commerce skill tree).

---

## Upgrade Trees (6 Branches)

Each tree has 4 tiers with meaningful branching choices at each tier. You can't max everything in a single playthrough -- you have to specialize.

### 1. EXCAVATION (How you dig)

The tools of your trade. Each tier offers a choice between three philosophies: **Power** (dig faster, break anything), **Precision** (dig carefully, preserve item quality), or **Range** (dig wider areas at once).

```
Tier 1: Basic Pickaxe
  |
Tier 2 (choose one):
  |- [Power] Pneumatic Hammer -- 2x dig speed, but items have 20% damage chance
  |- [Precision] Archaeologist's Kit -- Slower dig, but items extracted at +1 quality tier
  |- [Range] Blast Charges -- Clear 3x3 areas, but destroys fragile items in blast zone
  |
Tier 3 (choose one, builds on Tier 2 choice):
  |- [Power] Thermal Lance -- Cuts through any material, melts ice/rock instantly
  |- [Power] Seismic Driver -- Shockwave clears vertical columns, reveals hidden chambers
  |- [Precision] Laser Scalpel -- Surgical extraction, never damages items
  |- [Precision] Resonance Pick -- Vibrates items free without breaking surrounding terrain
  |- [Range] Tunnel Bore -- Drills horizontal/vertical tunnels automatically
  |- [Range] Shaped Charges -- Precise explosions that clear custom-shaped areas
  |
Tier 4 (ultimate):
  |- [Power] The Worldbreaker -- Destroys entire stratum layers, massive yield but no quality control
  |- [Precision] Quantum Extractor -- Phases items out of rock without disturbing terrain at all
  |- [Range] The Colony -- Deploy autonomous mini-drills that dig on their own while you explore
```

### 2. SURVEY (How you find things)

Detection and planning tools. Branch between **Depth** (see further down), **Detail** (identify items before digging), or **Hazard** (detect and avoid dangers).

```
Tier 1: Miner's Intuition (subtle terrain color hints)
  |
Tier 2:
  |- [Depth] Echo Sounder -- Ping reveals item locations 2 strata below you
  |- [Detail] Spectral Lens -- See item type/rarity through terrain in current stratum
  |- [Hazard] Canary Drone -- Alerts you to hazards within 50m radius
  |
Tier 3:
  |- [Depth] Tectonic Mapper -- Full map of item locations 3 strata down; plan your route
  |- [Detail] X-Ray Scanner -- See exact item identity and value before extracting
  |- [Hazard] Seismic Predictor -- Predict cave-ins 30 seconds before they happen
  |
Tier 4:
  |- [Depth] The Oracle -- See the entire Column's layout before you dig. Plan the perfect run.
  |- [Detail] Quantum Appraisal -- Items are auto-appraised at extraction. No surface appraisal needed.
  |- [Hazard] Phase Shield -- Become temporarily intangible. Walk through hazards unharmed.
```

### 3. LOGISTICS (How you carry and move)

Getting your finds to the surface. Branch between **Capacity** (carry more), **Speed** (move faster), or **Automation** (let machines do the hauling).

```
Tier 1: Canvas Backpack (10 slots)
  |
Tier 2:
  |- [Capacity] Reinforced Pack -- 25 slots, can carry heavy items
  |- [Speed] Grapple Line -- Zip back to surface in seconds (but limited capacity)
  |- [Automation] Supply Drone -- Sends items to surface automatically every 5 min
  |
Tier 3:
  |- [Capacity] Cargo Mech -- 50 slots, walk slower but carry anything
  |- [Speed] Pneumatic Tube Network -- Install tubes as you dig; items shoot to surface instantly
  |- [Automation] Conveyor System -- Build permanent conveyor belts in your tunnels
  |
Tier 4:
  |- [Capacity] Pocket Dimension -- 200 slots. Carry a museum's worth of loot.
  |- [Speed] Teleport Pad -- Place a pad at depth; teleport to surface and back at will
  |- [Automation] The Logistics AI -- Fully automated sorting, hauling, and surface delivery
```

### 4. COMMERCE (How you sell)

Your market savvy. Branch between **Appraisal** (know what things are worth), **Negotiation** (get better prices), or **Network** (access more buyers and markets).

```
Tier 1: Market Stall (sell to whoever walks by)
  |
Tier 2:
  |- [Appraisal] Jeweler's Loupe -- Appraise items 50% faster, reveal hidden properties
  |- [Negotiation] Silver Tongue -- 15% price bonus on all sales
  |- [Network] Trade Contacts -- Unlock 2 additional traveling merchants who visit weekly
  |
Tier 3:
  |- [Appraisal] Master Appraiser -- Instant appraisal, can identify forgeries and cursed items
  |- [Negotiation] Market Manipulation -- Spend money to artificially inflate/deflate prices for 3 days
  |- [Network] Auction House -- Sell items via auction; chance of bidding wars = huge profits
  |
Tier 4:
  |- [Appraisal] The Connoisseur -- Automatically identifies the single most valuable item in each stratum
  |- [Negotiation] Monopoly -- Lock exclusive contracts with factions; they buy ONLY from you at 2x price
  |- [Network] The Trade Empire -- Passive income from a network of sub-merchants who sell your stockpile automatically at optimal prices
```

### 5. ENDURANCE (Your physical capabilities)

Your body and survival. Branch between **Stamina** (dig longer), **Resistance** (survive hazards), or **Recovery** (heal and bounce back).

```
Tier 1: Miner's Constitution (base stamina pool)
  |
Tier 2:
  |- [Stamina] Iron Lungs -- 50% more expedition stamina
  |- [Resistance] Thermal Suit -- Halves heat/cold damage
  |- [Recovery] Field Medkit -- Heal 30% stamina mid-expedition (once per run)
  |
Tier 3:
  |- [Stamina] Deep Breath -- Double stamina; dig for twice as long
  |- [Resistance] Hazmat Rig -- Immune to gas, acid, and radiation
  |- [Recovery] Second Wind -- When stamina hits 0, recover 25% and get 60 seconds of bonus time
  |
Tier 4:
  |- [Stamina] The Inexhaustible -- Triple stamina. Reach the deepest strata in a single run.
  |- [Resistance] Living Armor -- Passive regeneration; hazards that don't kill you make you stronger (+damage resist stacking)
  |- [Recovery] Phoenix Core -- If you "die" underground, auto-revive at half stamina once per expedition. Keep your loot.
```

### 6. SPECIALIZATION (Your identity)

Unlocked after reaching Stratum 4 for the first time. Choose a specialization that defines your playstyle for the rest of the run:

- **Archaeologist:** +50% artifact value, can reconstruct broken artifacts, museum set bonuses doubled. *For players who love collecting and completing sets.*
- **Geologist:** +50% mineral/gem value, can identify ore veins from the surface, gem cutting skill (refine raw gems into cut gems worth 3x). *For players who love resource optimization.*
- **Treasure Hunter:** +100% rare item find chance, can sense hidden chambers, lucky finds (small chance any item is upgraded to legendary). *For players who love the thrill of discovery.*
- **Engineer:** Can build permanent infrastructure (elevators, lighting, reinforced tunnels) that persists between expeditions. Automation upgrades cost 50% less. *For players who love building systems.*
- **Merchant Prince:** Start each day with insider market knowledge, can bribe factions for reputation, warehouse capacity doubled. *For players who love the economy game.*

---

## Prestige System: "The Deep Reset"

When you reach The Hollow (Stratum 7) and extract a **Void Core**, you can choose to perform a **Deep Reset**:

- Your upgrade trees reset to Tier 1
- Your money resets to a starting amount
- **You keep:** Your specialization, your faction reputation tiers, your lore/map knowledge, and a permanent passive bonus:
  - +5% dig speed (stacking, per reset)
  - +5% sale prices (stacking)
  - +1 starting inventory slot (stacking)
  - Access to a new "Void" item tier with items that only appear post-prestige

Each Deep Reset also unlocks a new feature:
- **Reset 1:** Void items appear in all strata (rare, extremely valuable)
- **Reset 2:** A 7th buyer faction appears: **The Void Scholars** (buy void items at premium prices)
- **Reset 3:** Stratum 8 is revealed: **The Abyss** (procedurally generated endgame content)
- **Reset 4+:** Increasingly powerful void relics and cosmetic unlocks

---

## Visual Style & Art Direction

### Aesthetic: "Cozy Industrial"

Think **Steamworld Dig** meets **Stardew Valley** meets **Papers, Please**.

- **Underground:** Rich, layered pixel art. Each stratum has a radically different color palette and tile set. The Dump is grays and rusted oranges. The Catacombs are dark purples and bone-white. The Crystal Deep is electric blues and prismatic rainbows. Parallax scrolling backgrounds give depth.
- **Surface (Hollowmarket):** Warm, inviting frontier-town aesthetic. Wooden market stalls, cobblestone streets, lantern light. NPCs bustle around. Your shop/warehouse is customizable.
- **UI:** Clean, readable. A market ticker runs along the top showing price trends. Your inventory is a satisfying grid. Items have hand-drawn portraits with rarity borders.
- **Lighting:** Underground sections use dynamic lighting -- your headlamp casts a cone of light, and the darkness beyond is genuinely dark. Deeper strata have bioluminescent elements, lava glow, crystal refractions.
- **Animation:** Chunky, satisfying dig animations. Dirt crumbles. Rocks crack. Items sparkle when revealed. Selling items has a satisfying "cha-ching" with coins cascading.

### Audio Direction

- **Underground:** Quiet. Dripping water. Echoing picks. The deeper you go, the more alien the ambient sounds become. Stratum 7 has unsettling, barely-audible whispers.
- **Surface:** Lively market sounds. Crowd chatter. A folksy/acoustic soundtrack that shifts with the time of day.
- **Selling:** Satisfying register sounds. Big sales get a fanfare. Market price changes have subtle audio cues (rising pitch = prices going up).

---

## What Makes STRATA Unique (Competitive Analysis)

| Game | What It Does Well | What STRATA Does Differently |
|------|-------------------|------------------------------|
| **Digseum** | Excavation + museum display loop, prestige system | STRATA has a dynamic market instead of fixed museum values; multiple buyer types instead of one destination; deeper upgrade branching |
| **Dig or Die** | Survival pressure, physics simulation, night defense | STRATA replaces combat survival with economic survival -- the pressure is market timing and contract deadlines, not monsters |
| **SteamWorld Dig** | Satisfying Metroidvania progression, gem selling | STRATA has a living economy instead of fixed shop prices; specialization paths instead of linear upgrades |
| **Dome Keeper** | Tense mine-then-defend loop, resource decisions | STRATA's tension comes from push-your-luck depth decisions and market timing rather than wave defense |
| **Motherload** | Classic dig-sell-upgrade loop, mineral combos | STRATA adds buyer relationships, dynamic pricing, contracts, and 6 branching upgrade trees instead of linear stat boosts |
| **A Game About Digging A Hole** | Pure satisfying digging, simple upgrade loop | STRATA keeps the satisfying dig feel but layers on meaningful economic decisions and long-term strategy |
| **Core Keeper** | Deep crafting, multiplayer, biome variety | STRATA focuses on economy/trading as the core loop rather than survival/crafting; solo-focused with optional shared market |

### The STRATA Difference, Summarized

1. **The economy IS the game.** Not an afterthought. Dynamic pricing, multiple buyer factions with relationships, contracts, market manipulation, stockpiling, and timing.
2. **Deep upgrade trees with real choices.** Six trees, each with three-way branches at every tier. You can't max everything -- you become YOUR kind of miner.
3. **Layered world with narrative discovery.** Each stratum tells a story. Artifact sets piece together the history of lost civilizations. The deeper you go, the weirder and more compelling the lore.
4. **Push-your-luck expedition design.** Every dig is a risk/reward calculation. Go deeper for riches or return safely with what you have?
5. **Prestige system that changes the game.** Each Deep Reset doesn't just make numbers bigger -- it adds new content, factions, and strata.

---

## Sample Play Session

> **Day 1:** You start with a basic pickaxe and 10-slot backpack. You dig into The Dump, pulling out scrap metal and an old pocket watch. Back on the surface, you sell the scrap to The Foundry (they need metal) and the watch to The Curator (they collect historical items). You earn enough to upgrade to a Pneumatic Hammer. You check the market board: gem prices are rising because nobody's been bringing crystals lately. You note this for later.
>
> **Day 5:** You've upgraded your Survey tree to Echo Sounder. Before digging, you ping the ground and see a cluster of items in The Catacombs. You pack extra stamina rations and head down. In The Catacombs, you find two pieces of a medieval armor set (3 of 5 needed for the set bonus). You also find a cursed amulet -- Madame Nox will pay well for that. On the way back, a cave-in nearly traps you. You barely escape with your haul.
>
> **Day 12:** You've specialized as a Merchant Prince. Your warehouse is stocked with gems you've been hoarding. The Crystal Fair event begins -- gem prices spike 80%. You sell your entire stockpile to the Gemwright Guild and make a fortune. You use the profits to unlock Tier 3 Commerce: the Auction House. Now you can sell rare items via auction, where bidding wars between factions can triple the sale price.
>
> **Day 30:** You've reached Stratum 6: The Crystal Deep. The items here are extraordinarily valuable but the electromagnetic interference scrambles your survey tools. You're digging blind. You find a prismatic geode worth more than everything you've sold combined. But you're almost out of stamina and the path back is long. Do you try to find more, or do you take the safe route home? You head back. At the surface, the Gemwright Guild and The Curator both want the geode. You put it up for auction. Bidding war. Final sale: 50,000 gold. You've never had this much money. You buy your Tier 4 Excavation upgrade: The Colony -- autonomous mini-drills that dig for you.
>
> **Day 45:** You've reached The Hollow. You extract a Void Core. The game offers you the Deep Reset. You accept. Everything resets... but you keep your Patron status with the Gemwright Guild, your Merchant Prince specialization, and now Void items -- shimmering, otherworldly relics -- appear in every stratum. The game has changed. Time to dig again.

---

## Development Scope

### MVP (Vertical Slice)
- Strata 1-4 playable
- 3 buyer factions (Curator, Foundry, Finn's Fences)
- 2 upgrade trees (Excavation, Logistics)
- Basic dynamic pricing
- 30-50 unique items
- Core dig/sell/upgrade loop functional

### Full Release
- All 7 strata + Stratum 8 (post-prestige)
- All 6 buyer factions
- All 6 upgrade trees
- Dynamic market with events and seasonal cycles
- 200+ unique items
- Contract system
- Prestige system (4+ resets)
- Full soundtrack and sound design

### Post-Launch (Stretch Goals)
- **Shared Market Mode:** Your sales affect a shared online market. Other players' supply/demand impacts prices.
- **Rival Miners:** NPC competitors who also sell to your buyers, creating market competition.
- **Custom Expeditions:** Daily/weekly challenge runs with special modifiers.
- **The Endless Abyss:** Procedurally generated infinite depth mode post-prestige.

---

## Research Sources

This concept was informed by analysis of the following games:
- [Digseum on Steam](https://store.steampowered.com/app/3361470/Digseum/)
- [Dig or Die on Steam](https://store.steampowered.com/app/315460/Dig_or_Die/)
- [SteamWorld Dig 2 Upgrades Guide](https://gamefaqs.gamespot.com/switch/206484-steamworld-dig-2/faqs/75903/upgrades)
- [Dome Keeper General Strategy Guide](https://steamcommunity.com/sharedfiles/filedetails/?id=2869939597)
- [Motherload Wiki](https://xgenstudios.fandom.com/wiki/Motherload)
- [A Game About Digging A Hole on Steam](https://store.steampowered.com/app/3244220/A_Game_About_Digging_A_Hole/)
- [Core Keeper Mining Guide](https://foreverclassicgames.com/features/2024/10/mining-guide-core-keeper)
- [Game Economy Design Fundamentals (Machinations.io)](https://machinations.io/articles/what-is-game-economy-design)
