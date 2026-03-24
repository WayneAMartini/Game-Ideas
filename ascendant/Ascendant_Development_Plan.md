# Ascendant — Development Plan

## Context

Building **Ascendant**, a hero simulator idle/clicker RPG for iPhone, based on the completed design doc at `ascendant/Ascendant_Design_Doc.md`. The game features 24 hero classes, tap combat, AFK idle progression, a 4-hero party system, and an ascension prestige loop across floating sky islands.

**Engine:** Unity (C#) | **Backend:** Firebase | **Platform:** iOS (portrait, iPhone)

---

## Project Structure

```
ascendant/
  Ascendant_Design_Doc.md          # (exists) Game design document
  unity-project/                    # Unity project root
    Assets/
      Scenes/
        BootScene.unity             # Init, splash, Firebase connect
        MainMenuScene.unity         # Title screen, account
        GameScene.unity             # Core gameplay (combat, party, islands)
        SummonScene.unity           # Gacha/summoning portal
      Scripts/
        Core/                       # Game loop, state machine, save system
        Combat/                     # Tap system, damage, abilities, auto-attack
        Heroes/                     # Hero classes, stats, skill trees
        Party/                      # 4-hero formation, synergies, combos
        Progression/                # Leveling, equipment, ascension, mastery
        Islands/                    # Biomes, stages, bosses, environmental FX
        Idle/                       # AFK vault, offline calculations, expeditions
        Economy/                    # Currencies, shop, gacha, IAP
        UI/                         # All UI controllers and views
        Backend/                    # Firebase integration (auth, saves, events)
        Audio/                      # Sound manager, music system
        Utils/                      # Helpers, extensions, object pooling
      Prefabs/                      # Hero prefabs, enemy prefabs, FX prefabs
      Art/                          # Sprites, animations, UI assets
      Audio/                        # SFX and music files
      Data/                         # ScriptableObjects for class defs, items, islands
      Plugins/                      # Firebase SDK, IAP plugin
    ProjectSettings/
    Packages/
```

---

## Phase 1: Project Foundation & Core Combat MVP

**Goal:** Tap an enemy, see damage numbers, kill it, advance to the next stage. One hero (Warrior) in a portrait-mode scene.

### 1.1 — Unity Project Setup
- Create Unity 2D project (URP for mobile performance)
- Configure for iOS: portrait lock, target iOS 16+, iPhone SE 3rd gen minimum
- Set up folder structure per above
- Import TextMeshPro, DOTween (free tweening library)
- Create `GameScene` with combat viewport (top 60%) and hero panel (bottom 40%)
- **Files:** Project config, `GameScene.unity`, `GameManager.cs`

### 1.2 — Tap Input System
- Detect taps in the combat viewport area
- Calculate tap damage using formula: `(BaseTapPower + HeroTapBonus) * MomentumMultiplier`
- Implement **Momentum system**: consecutive taps within 0.5s build stacks (max 100), +1% damage per stack, decay at 5 stacks/sec
- Floating damage numbers (TextMeshPro, object-pooled) that scale with damage
- Visual feedback: screen border glow intensity tied to Momentum
- **Files:** `TapInputController.cs`, `MomentumSystem.cs`, `DamageNumberPool.cs`

### 1.3 — Enemy System (Basic)
- `EnemyData` ScriptableObject: HP, ATK, DEF, gold drop, XP drop, affinity
- Enemy spawner: spawn wave of 3-5 enemies per stage
- Enemy health bars (simple UI slider above each enemy sprite)
- Death animation (fade + particle burst), loot drop
- **Files:** `EnemyData.cs` (SO), `Enemy.cs`, `EnemySpawner.cs`, `EnemyHealthBar.cs`

### 1.4 — Hero System (Warrior Only)
- `HeroData` ScriptableObject: base stats (ATK, DEF, HP, SPD), class ID, affinity, role
- `Hero.cs`: runtime hero instance with current HP, level, stats
- Warrior tap mechanic: every 5th tap triggers Shockwave (AoE 50% tap damage)
- Hero portrait UI at bottom: circular portrait with HP bar
- **Files:** `HeroData.cs` (SO), `Hero.cs`, `HeroPortrait.cs`, `WarriorTapMechanic.cs`

### 1.5 — Stage Progression
- Stage counter (Island 1, Stage 1/100)
- Clear all enemies in wave -> advance to next stage
- Gold + XP awarded per kill
- Basic progression: stages get harder (enemy HP/ATK scale per stage)
- **Files:** `StageManager.cs`, `ProgressionConfig.cs`

### 1.6 — Basic Auto-Attack
- Hero auto-attacks the nearest enemy at a fixed interval (based on SPD stat)
- Auto-attack damage = Hero ATK (separate from tap damage)
- Simple animation: hero sprite plays attack anim, projectile/slash VFX
- **Files:** `AutoAttackSystem.cs`

**Milestone:** Playable tap combat with one hero, enemies, stages, damage numbers, and gold/XP accumulation.

---

## Phase 2: 4-Hero Party & Starter Classes

**Goal:** Run 4 heroes simultaneously with the complete starter class kit (Warrior, Mage, Priest, Rogue).

### 2.1 — Party System
- `PartyManager.cs`: manages 4 hero slots in a 2x2 formation grid
- Frontline (+20% DEF, targeted first) / Backline (+10% damage) positioning
- 4 hero portraits along the bottom panel with HP bars
- Party swap UI: drag heroes between slots (between stages only)
- **Files:** `PartyManager.cs`, `FormationGrid.cs`, `PartyUI.cs`

### 2.2 — Ability System
- 3 ability slots per hero: Ability 1 (tap portrait), Ability 2 (double-tap), Ultimate (hold when charged)
- Cooldown timers displayed as radial overlays on portraits
- Ultimate charge bar (fills from dealing/taking damage)
- Auto-cast toggle per ability (90% effectiveness when auto)
- **Files:** `AbilitySystem.cs`, `Ability.cs`, `AbilitySlot.cs`, `CooldownUI.cs`

### 2.3 — Remaining Starter Classes
- **Mage** — Arcane Bolt tap mechanic (homing missiles, chain on kill, split at high Momentum), Mana Surge passive, Frost Nova / Blizzard / Absolute Zero abilities
- **Priest** — Holy Touch tap mechanic (auto-triage heal/damage), Divine Grace passive, Healing Light / Sacred Shield / Resurrection abilities
- **Rogue** — Shadow Strike tap mechanic (combo points -> Finishing Blow at 5 CP), Cloak of Shadows passive, Ambush / Smoke Bomb / Death's Embrace abilities
- Each class gets a `[ClassName]TapMechanic.cs` and ability ScriptableObjects
- **Files:** `MageTapMechanic.cs`, `PriestTapMechanic.cs`, `RogueTapMechanic.cs`, per-class ability SOs

### 2.4 — Damage Type System
- Physical / Magical / True damage types
- Enemy resistance/weakness: Physical strong vs Beasts/Constructs, Magical strong vs Armored/Ethereal
- Damage calculation pipeline: `RawDamage * TypeMultiplier * AffinityBonus * DefenseReduction`
- **Files:** `DamageCalculator.cs`, `DamageType.cs` (enum)

### 2.5 — Affinity System (Core)
- 6 affinities: Flame, Frost, Storm, Nature, Shadow, Radiance
- Advantage wheel: Flame > Nature > Storm > Frost > Flame; Shadow <-> Radiance mutual +30%
- Advantage = +30% damage dealt, -15% damage taken
- Visual indicators on hero/enemy sprites showing affinity (colored border/icon)
- **Files:** `AffinitySystem.cs`, `AffinityData.cs`

**Milestone:** 4 heroes fighting together with unique tap mechanics, abilities, and affinity interactions.

---

## Phase 3: Idle & AFK Systems

**Goal:** The game generates meaningful rewards while closed. Players return to a satisfying loot collection moment.

### 3.1 — AFK Vault
- On app close/background: record timestamp + current stage + party power
- On app open: calculate offline duration (capped at 10 hours)
- AFK rewards = `(StageGoldRate * 0.5) * OfflineHours` for gold, similar for XP
- 5% chance per hour of equipment drop (common/uncommon only)
- Vault UI: glowing chest at top-right of combat screen, tap to collect with loot cascade animation
- **Files:** `AFKVaultSystem.cs`, `OfflineCalculator.cs`, `AFKVaultUI.cs`

### 3.2 — Push Notifications
- Register for iOS push notifications via Unity's Mobile Notifications package
- 8-hour vault nearly full notification
- Daily quest reset notification
- Expedition complete notification
- Respect quiet hours (10PM-8AM default), max 2/day, all individually toggleable
- **Files:** `NotificationManager.cs`, `NotificationSettings.cs`

### 3.3 — Expeditions
- Heroes NOT in active party can be deployed on timed idle missions
- 4 expedition types: Resource (2h), Scouting (4h), Dungeon (8h), Rare (12h)
- Up to 3 simultaneous expeditions (unlock slots with progression)
- Expedition UI: deployment screen with hero selection + timer display
- **Files:** `ExpeditionManager.cs`, `ExpeditionData.cs` (SO), `ExpeditionUI.cs`

**Milestone:** Close the app, come back hours later, collect meaningful rewards. Expeditions running in background.

---

## Phase 4: Progression Systems

**Goal:** Heroes get stronger through leveling, gear, and skill trees. Clear sense of power growth.

### 4.1 — Leveling System
- XP -> Level curve (exponential scaling)
- Level cap 100 per Realm (Realm 1 = Lv100)
- Per-class stat growth rates (ATK/DEF/HP/SPD at +/++/+++ per level)
- Milestone levels (10, 25, 50, 75, 100) unlock ability upgrades
- Level-up VFX: gold burst, fanfare sound
- **Files:** `LevelingSystem.cs`, `XPCurve.cs` (SO), `ClassGrowthRates.cs` (SO)

### 4.2 — Equipment System
- 6 slots: Weapon, Armor, Helm, Accessory, Relic, Mount
- Rarity tiers: Common (60%) -> Uncommon (25%) -> Rare (10%) -> Epic (4%) -> Legendary (0.9%) -> Mythic (0.1%)
- `EquipmentData` ScriptableObject with stat rolls, rarity, set ID
- Equipment inventory UI: grid view, tap to equip, drag to compare
- Enhancement: spend gold + materials to upgrade (+1 to +15)
- **Equipment persists through Ascension** (critical design choice)
- **Files:** `EquipmentData.cs` (SO), `EquipmentSystem.cs`, `InventoryUI.cs`, `EnhancementSystem.cs`

### 4.3 — Skill Tree System
- 3 branches per class (defined in design doc)
- Visual node tree UI: pinch-zoom + tap to invest
- 1 Skill Point per level + bonus from island challenges
- Capstone ability at end of each branch
- Free respec on Ascension; gold cost otherwise
- Skill trees reset on Ascension (like levels)
- **Files:** `SkillTreeData.cs` (SO), `SkillTreeSystem.cs`, `SkillTreeUI.cs`, `SkillNode.cs`

### 4.4 — Class Mastery
- Per-class permanent progression (never resets)
- Track stages cleared + ascensions completed per class
- Tiers: Novice -> Apprentice -> Journeyman -> Expert -> Master -> Grandmaster
- Rewards: stat bonuses, alternate ability variants, skins, passive abilities
- **Files:** `ClassMasterySystem.cs`, `ClassMasteryData.cs` (SO), `MasteryUI.cs`

**Milestone:** Full hero progression loop — level up, equip gear, invest in skill trees, build class mastery.

---

## Phase 5: Sky Islands & Boss Encounters

**Goal:** The world map is navigable. Islands have biomes with environmental effects. Bosses are multi-phase active encounters.

### 5.1 — Island Map UI
- Vertical scrolling world map showing floating sky islands
- Current position = pulsing beacon
- Completed islands = checkmark, locked = dimmed
- Tap island for info panel: biome, affinity, boss, rewards
- Parallax cloud layers for depth
- **Files:** `IslandMapUI.cs`, `IslandNode.cs`, `IslandData.cs` (SO)

### 5.2 — Biome Environmental Effects
- Per-island effects (e.g., Ember Plateau: 1% HP/5s fire damage to non-Flame heroes)
- Environmental FX system: particle overlays, color grading per biome
- Affinity interaction: home affinity +25% damage, advantage +50%, disadvantage -20%
- **Files:** `BiomeEffectSystem.cs`, `BiomeData.cs` (SO), `EnvironmentalFX.cs`

### 5.3 — Mini-Boss System
- Every 10th stage = mini-boss with 1 mechanic from pool (Enrage, Shield Phase, Add Spawning, Ground Slam, Life Steal, Split, Reflect)
- Mechanic telegraph system: visual warning before dangerous attacks
- Tap-to-dodge prompts for avoidable attacks
- **Files:** `MiniBossController.cs`, `BossMechanic.cs` (base class), mechanic subclasses

### 5.4 — Island Boss System
- Stage 100 = multi-phase boss fight (active play required)
- Phase transitions with cinematic camera shifts
- Example: Emberlord Kael (3 phases as described in design doc)
- Boss loot table: guaranteed Rare+, chance for Epic/Legendary
- **Files:** `IslandBossController.cs`, `BossPhase.cs`, `BossLootTable.cs` (SO)

### 5.5 — Realm 1 Content (12 Islands)
- Create `IslandData` ScriptableObjects for all 12 Realm 1 islands
- Biome art: placeholder tilesets/backgrounds for 6 affinity themes
- Enemy variety: 3-4 enemy types per affinity (Beast, Humanoid, Construct, Ethereal variants)
- Realm Boss: The Radiant Guardian (4-phase cinematic fight)
- **Files:** 12 island SOs, enemy SOs per biome, Realm Boss controller

**Milestone:** Full Realm 1 playable — 12 islands, 1200 stages, environmental effects, mini-bosses every 10 stages, island bosses at stage 100, cinematic Realm Boss.

---

## Phase 6: Ascension Prestige System

**Goal:** Players can prestige (ascend), reset level/progress, gain permanent power, and push further.

### 6.1 — Ascension Flow
- Ascend option available when hero hits progression wall or reaches Island 12
- Reset: level -> 1, island -> 1, skill tree -> reset
- Keep: equipment, class mastery, pantheon progress
- Earn Ascension Shards based on highest island reached
- Ascension cinematic: hero rises through clouds
- **Files:** `AscensionSystem.cs`, `AscensionUI.cs`, `AscensionCinematic.cs`

### 6.2 — Ascension Tiers
- Mortal (0) -> Awakened (1st) -> Exalted (3rd) -> Mythic (5th) -> Demigod (10th)
- Each tier grants permanent base stat bonuses (+15% / +35% / +60%)
- Tier-specific unlocks (advanced skill branches, mythic gear evolution)
- Visual indicator on hero portrait showing current tier
- **Files:** `AscensionTier.cs`, `TierBonusSystem.cs`

### 6.3 — Ascension Skill Tree
- Permanent upgrade tree (persists across ALL ascensions)
- 4 branches: Power, Fortitude, Prosperity, Swiftness
- Spend Ascension Shards to unlock nodes
- **Files:** `AscensionSkillTree.cs`, `AscensionSkillTreeUI.cs`, `AscensionNode.cs` (SO)

### 6.4 — Demigod Retirement & Pantheon
- After 10 ascensions: Transcendence Trial (gauntlet fight)
- Hero permanently retired, becomes Demigod Patron
- Grants global passive buff (per-class, defined in design doc)
- Pantheon UI: celestial grid, 24 slots, milestone bonuses at 6/12/18/24 filled
- **Files:** `DemigodSystem.cs`, `PantheonUI.cs`, `TranscendenceTrial.cs`

**Milestone:** Complete prestige loop. Ascend heroes, gain permanent power, retire to Demigod, fill the Pantheon.

---

## Phase 7: Expanded Class Roster (20 More Classes)

**Goal:** Implement remaining 20 classes across Tiers 2-4 with unique tap mechanics and abilities.

### 7.1 — Class Architecture Refactor
- Ensure `ITapMechanic` interface and `BaseAbility` class support all class patterns
- Patterns needed: charge-and-release (Marksman), resource systems (Monk Chi, Reaper Souls, Berserker Rage), pet companions (Ranger), totems (Shaman), songs (Bard), familiar swapping (Summoner), potion cycling (Alchemist), ammo/reload (Gunslinger), minion management (Necromancer)
- **Files:** `ITapMechanic.cs`, `BaseAbility.cs`, resource system interfaces

### 7.2 — Tier 2 Classes (6)
- Marksman, Defender, Berserker, Druid, Thief, Shaman
- Each: TapMechanic, 3 abilities, passive, ScriptableObject data, portrait art
- Unlock trigger: reach Island 3

### 7.3 — Tier 3 Classes (7)
- Warlock, Ranger, Spell-Blade, Necromancer, Monk, Paladin, Bard
- Pet system for Ranger (Storm Wolf default + 3 unlockable pets)
- Totem system for Shaman (persistent battlefield objects)
- Song system for Bard (persistent aura + Crescendo mechanic)
- Unlock trigger: reach Island 6 or via Astral Summon

### 7.4 — Tier 4 Classes (7)
- Dragon-Hunter, Summoner, Alchemist, Chronomancer, Gunslinger, Warden, Reaper
- Familiar-swapping system for Summoner (4 familiars with swipe gesture)
- Potion-cycling system for Alchemist (4 rotating potion types)
- Ammo/Reload system for Gunslinger (6-shot clip + reload pause)
- Soul resource system for Reaper (20 max, spent on abilities)
- Unlock trigger: reach Island 10 or via Astral Summon

### 7.5 — Party Synergies & Combos
- Affinity synergies: 2/3/4 matching bonuses + Resonance Abilities for 4-of-a-kind
- Role synergies: Vanguard+Support, Striker+Ranger, etc.
- 13+ Combo Abilities (specific hero pairs trigger joint attacks when abilities used within 3s)
- Discoverable combo indicator ("?" when undiscovered pair is in party)
- **Files:** `SynergySystem.cs`, `ComboAbilityData.cs` (SO), `ResonanceAbility.cs`

**Milestone:** All 24 classes playable with unique mechanics. Full synergy and combo system active.

---

## Phase 8: Economy, Gacha & Monetization

**Goal:** Currencies flow, heroes are summoned, IAP works, Battle Pass is live.

### 8.1 — Currency System
- 6 currencies: Gold, Stardust, Ascension Shards, Aether Crystals, Class Tokens, Guild Coins
- Currency wallet UI: persistent top bar showing Gold + Stardust, expandable for all
- Earn/spend tracking for economy balancing
- **Files:** `CurrencyManager.cs`, `Wallet.cs`, `CurrencyUI.cs`

### 8.2 — Astral Summon (Gacha)
- Summon portal scene with glowing animation
- Single Pull (300 Stardust) and 10-Pull (2700 Stardust, guaranteed 1 Rare+)
- Rarity rates: Uncommon 60%, Rare 30%, Epic 8%, Legendary 2%
- Pity system: guaranteed Epic at 30, Legendary at 90 (counter carries across banners)
- Wishlist (5 classes, 2x rate boost)
- Duplicate protection -> Star Fragments for Star-Up
- Spark system: 200 pulls = choose any hero free
- Summon animation with rarity-based reveal VFX
- **Files:** `GachaSystem.cs`, `SummonUI.cs`, `PityCounter.cs`, `WishlistManager.cs`, `BannerData.cs` (SO)

### 8.3 — Star System
- 1-Star through 7-Star hero progression
- Star-Up costs: Star Fragments + materials (escalating)
- Visual star rating on hero portraits and detail screens
- **Files:** `StarSystem.cs`, `StarUpUI.cs`

### 8.4 — IAP Integration
- Unity IAP package for App Store
- Stardust packages ($0.99 - $99.99)
- Patron's Blessing monthly subscription ($4.99/mo)
- Battle Pass premium track ($9.99/season)
- Starter packs (one-time purchases)
- Receipt validation via Firebase Cloud Functions (server-authoritative)
- **Files:** `IAPManager.cs`, `StoreUI.cs`, Firebase Cloud Functions for validation

### 8.5 — Battle Pass
- 50 tiers, free track + premium track
- Pass XP from daily/weekly quests
- Rewards: Gold, materials, Scrolls (free) + Stardust, skins, guaranteed hero (premium)
- Season duration: ~6 weeks
- **Files:** `BattlePassSystem.cs`, `BattlePassUI.cs`, `BattlePassData.cs` (SO)

**Milestone:** Full economy loop. Players earn currencies, summon heroes, upgrade star ratings, purchase IAP.

---

## Phase 9: Firebase Backend & Social Features

**Goal:** Cloud saves, leaderboards, guilds, arena, and world boss — all powered by Firebase.

### 9.1 — Firebase Setup
- Firebase project creation + iOS app registration
- Firebase Auth (Game Center sign-in + anonymous fallback)
- Firestore for player data, guild data, leaderboards
- Cloud Functions for server-authoritative gacha rolls, IAP validation, anti-cheat
- Firebase Analytics for retention/progression tracking
- **Files:** Firebase config, `FirebaseManager.cs`, `AuthManager.cs`, `CloudSaveManager.cs`

### 9.2 — Cloud Save System
- Auto-save every 30s + on background/close
- Sync player state: heroes, inventory, progression, currencies, settings
- Conflict resolution: server timestamp wins
- Offline queue: actions queued locally, synced on reconnect
- **Files:** `SaveData.cs`, `CloudSyncSystem.cs`, `OfflineQueue.cs`

### 9.3 — Leaderboards
- Firestore collections for: Highest Island, Speed Ascension, Boss Damage, Pantheon Race, Arena Rank
- Weekly/all-time views
- Top 100 + player's personal rank
- **Files:** `LeaderboardManager.cs`, `LeaderboardUI.cs`

### 9.4 — Guild System
- Guilds of up to 30 members
- Guild Tech Tree (members contribute resources for guild-wide bonuses)
- Guild chat (Firestore real-time)
- Guild Expeditions: cooperative shared map, each member clears paths
- **Files:** `GuildManager.cs`, `GuildUI.cs`, `GuildTechTree.cs`, `GuildExpedition.cs`

### 9.5 — Arena (Async PvP)
- Build defense team (4 heroes, AI-controlled)
- Attack other players' defense teams
- Normalized stat system (compress whale advantage to ~10%)
- Bracket matchmaking, monthly seasons
- **Files:** `ArenaManager.cs`, `ArenaMatchmaker.cs`, `ArenaUI.cs`, `ArenaBattleSimulator.cs`

### 9.6 — World Boss (Weekly Event)
- Global HP pool (e.g., 100B HP) stored in Firestore
- 3 attempts/day per player
- Damage recorded on leaderboard
- 48-hour event window, tier-based rewards for all contributors
- **Files:** `WorldBossManager.cs`, `WorldBossUI.cs`, Cloud Functions for damage validation

**Milestone:** Full multiplayer stack. Cloud saves, guilds, arena, world boss, leaderboards.

---

## Phase 10: Events & Endgame Content

**Goal:** Recurring event content to keep players engaged long-term.

### 10.1 — Tower of Trials (Weekly)
- 50-floor roguelike tower with random buffs/debuffs per floor
- Separate from main campaign progression
- Floor milestone rewards + leaderboard
- **Files:** `TowerOfTrials.cs`, `TowerFloor.cs`, `TowerModifier.cs` (SO)

### 10.2 — Void Rifts (Bi-weekly)
- Time-limited challenge dungeons with combat modifiers
- Endgame-tuned difficulty
- Exclusive Aether Crystal rewards
- **Files:** `VoidRiftManager.cs`, `RiftModifier.cs` (SO)

### 10.3 — Seasonal Events
- 6 themed events rotating every ~6 weeks
- Event-specific islands, enemies, cosmetics, and bosses
- Event currency + shop
- Server-driven event config (no app update needed)
- **Files:** `SeasonalEventManager.cs`, `EventConfig.cs` (fetched from Firebase)

### 10.4 — Realm 2 & 3 Content
- 12 more islands each (Islands 13-24, 25-36)
- Realm 3: dual-affinity islands
- Infinite Ascension (Realm 4+): procedurally generated islands
- **Files:** Island SOs, enemy SOs, boss controllers per realm

**Milestone:** Full endgame loop. Weekly/bi-weekly events, seasonal content, 3 realms + infinite mode.

---

## Phase 11: Art, Audio & Polish

**Goal:** The game looks and sounds great. Polished UX. App Store ready.

### 11.1 — Art Pipeline
- Hero sprites: 24 classes x idle/attack/ability/death animations
- Enemy sprites: ~50 unique enemies across 6 affinity themes
- UI art: portraits, icons, buttons, panels matching design doc layout
- VFX: tap particles, ability effects, boss telegraphs, ascension cinematic
- Island backgrounds: 36 unique biome illustrations
- **Decision:** Commission artist / use asset store / AI-assisted concept art -> hand-painted final

### 11.2 — Audio
- SFX: tap impacts, ability sounds, UI clicks, level-up fanfare, boss phase transitions
- Music: per-biome ambient tracks (6 themes), boss fight music, menu theme
- Haptic feedback: customizable intensity (Off/Light/Standard/Strong)

### 11.3 — UI/UX Polish
- Match design doc layout: combat viewport (60%) + hero panel (40%) + bottom tab bar
- One-thumb design: all critical interactions reachable by right thumb
- Transitions: smooth screen transitions, DOTween animations
- Accessibility: VoiceOver, adjustable text size, colorblind modes, tap assist, auto-battle

### 11.4 — Performance Optimization
- Object pooling for damage numbers, projectiles, enemies, particles
- Sprite atlasing for draw call reduction
- Target 60 FPS on iPhone 12+, 30 FPS minimum on iPhone SE 3
- Memory budget: <300MB RAM
- Battery: <8%/hour active play

### 11.5 — App Store Submission
- App Store Connect setup, screenshots, description, age rating
- TestFlight beta distribution
- App Review compliance: transparent gacha rates, subscription disclosures
- Privacy policy (Firebase data collection disclosure)

**Milestone:** Polished, optimized, App Store approved.

---

## Build Order Summary

| Phase | What | Builds On |
|-------|------|-----------|
| 1 | Core Combat MVP (1 hero, tapping, enemies, stages) | Nothing — start here |
| 2 | 4-Hero Party + Starter Classes + Affinities | Phase 1 |
| 3 | AFK/Idle Systems (vault, notifications, expeditions) | Phase 1 |
| 4 | Progression (leveling, gear, skill trees, mastery) | Phase 2 |
| 5 | Sky Islands, Biomes, Bosses (Realm 1) | Phase 2 + 4 |
| 6 | Ascension Prestige (tiers, shards, demigod, pantheon) | Phase 4 + 5 |
| 7 | All 24 Classes + Synergies/Combos | Phase 2 (expand) |
| 8 | Economy, Gacha, IAP, Battle Pass | Phase 4 + 7 |
| 9 | Firebase Backend + Social (saves, guilds, arena) | Phase 8 |
| 10 | Events & Endgame (tower, rifts, seasons, realms 2-3) | Phase 5 + 9 |
| 11 | Art, Audio, Polish, App Store | All phases |

Phases 3 and 4 can run in parallel after Phase 2. Phase 7 can begin alongside Phase 5. Phase 11 (art) should start early and run continuously.

---

## Verification & Testing Strategy

- **Each phase ends with a playable build** tested on device via TestFlight
- **Unit tests** for: damage calculations, AFK offline math, gacha rates/pity, currency transactions, ascension shard calculations
- **Integration tests** for: Firebase auth/save/sync, IAP receipt validation
- **Device testing** on: iPhone SE 3 (low-end), iPhone 13 (mid), iPhone 15 Pro (high-end)
- **Performance profiling** with Unity Profiler + Xcode Instruments after Phases 1, 5, 7, 11
- **Economy simulation** spreadsheet to validate currency flow, progression pacing, F2P vs paid timelines

---

## First Implementation Step

**Start with Phase 1.1** — Create the Unity project, configure for iOS portrait mode, set up the folder structure, and build the empty GameScene with the combat viewport / hero panel layout.
