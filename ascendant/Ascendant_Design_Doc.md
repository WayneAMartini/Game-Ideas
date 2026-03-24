# ASCENDANT

## Hero Simulator Idle/Clicker Game — Game Design Document

**Platform:** iOS (iPhone) | **Orientation:** Portrait | **Genre:** Idle/Clicker RPG Hero Simulator
**Target Audience:** Mobile RPG fans, idle game enthusiasts, hero collector players (ages 13+)

---

## Table of Contents

1. [Game Overview](#1-game-overview)
2. [Core Gameplay Loop](#2-core-gameplay-loop)
3. [Tap Combat System](#3-tap-combat-system)
4. [AFK & Idle Systems](#4-afk--idle-systems)
5. [Hero Classes — Full Roster of 24](#5-hero-classes--full-roster-of-24)
6. [Affinity System](#6-affinity-system)
7. [4-Hero Party System](#7-4-hero-party-system)
8. [Ascension Prestige System](#8-ascension-prestige-system)
9. [Sky Islands & Biomes](#9-sky-islands--biomes)
10. [Progression Systems](#10-progression-systems)
11. [Boss Encounters](#11-boss-encounters)
12. [Events & Endgame](#12-events--endgame)
13. [Economy & Currencies](#13-economy--currencies)
14. [Monetization](#14-monetization)
15. [UI/UX Design](#15-uiux-design)
16. [Narrative & Lore](#16-narrative--lore)
17. [Technical Considerations](#17-technical-considerations)

---

## 1. Game Overview

### Concept

**Ascendant** is a hero simulator idle/clicker game where players recruit, level, and ascend a roster of heroes through floating sky islands. Heroes literally ascend — climbing vertically through biome-themed islands, battling enemies, collecting loot, and eventually achieving demigod status through a satisfying prestige loop.

Players run a party of 4 heroes simultaneously, each potentially at different stages of their ascension journey. The game blends satisfying tap-based active combat with meaningful AFK/idle progression, deep hero class customization, and a strategic party synergy system.

### Pillars

- **Vertical Fantasy:** Heroes ascend upward through increasingly spectacular sky islands. Every prestige cycle sends them higher. The visual metaphor of ascension IS the game.
- **Class Identity:** 24 distinct hero classes, each with unique tap mechanics, skill trees, and playstyles. No two classes feel the same.
- **Active + Idle Harmony:** Tapping is rewarding and impactful when you're engaged. Idle progression is generous and meaningful when you're away. Both modes matter.
- **Prestige as Power Fantasy:** Ascending a hero to demigod status is a celebration, not a punishment. Each reset makes you visibly, dramatically stronger.
- **Party Strategy:** Running 4 heroes with synergies, affinity bonuses, and combo abilities creates emergent strategy in a genre often lacking it.

### Inspirations

| Game | What We Take |
|------|-------------|
| **AFK Arena** | Faction advantage system, idle reward collection UX, hero ascension tiers, visual polish |
| **Tap Titans 2** | Tight prestige loop (15-30 min cycles), skill tree depth, clan raids, tap satisfaction |
| **Clicker Heroes** | Exponential power growth feel, dual active/idle build paths, layered prestige |
| **Almost a Hero** | Hero personality & synergies, comedic tone potential, party composition depth |
| **Idle Heroes** | Deep hero investment (multiple upgrade axes per hero), event-driven engagement cadence |
| **Nonstop Knight** | Auto-combat elegance, portrait-mode UI minimalism, always-running feel |

---

## 2. Core Gameplay Loop

### Micro Loop (Minute-to-Minute)

```
Tap enemies for damage --> Heroes auto-attack alongside you --> Enemies drop gold/XP
--> Level up heroes --> Unlock/upgrade abilities --> Push to next stage --> Repeat
```

### Session Loop (15-30 Minutes)

```
Collect AFK rewards --> Level heroes with banked resources --> Push through stages
--> Hit a wall (boss you can't beat) --> Optimize gear/skills/party --> Break through
--> Eventually reach Ascension threshold --> Ascend (prestige) --> Start stronger
```

### Meta Loop (Days-to-Weeks)

```
Ascend heroes through Realms --> Retire heroes to Demigod status --> Build Pantheon
--> Unlock new classes --> Experiment with party compositions --> Push deeper Realms
--> Complete class mastery --> Chase Mythic gear --> Climb leaderboards
```

### Session Design

| Session Type | Duration | Activities |
|-------------|----------|------------|
| Quick Check-in | 1-2 min | Collect AFK rewards, start expeditions, check daily quests |
| Active Session | 15-30 min | Push stages, boss attempts, manage gear/skills, prestige cycle |
| Deep Session | 30-60 min | Event content, Tower of Trials, team optimization, crafting |

---

## 3. Tap Combat System

### Active Tapping

The screen's upper 60% is the combat viewport. Tapping anywhere in this zone deals **Tap Damage** to the current enemy target.

**Tap Damage Formula:**
```
Tap Damage = (Base Tap Power + Hero Tap Bonus) x Momentum Multiplier x Affinity Bonus
```

**Momentum System:**
- Consecutive taps within 0.5 seconds of each other build a **Momentum** counter
- Every 10 Momentum stacks grants +10% Tap Damage (caps at 100 stacks = +100%)
- Momentum decays at 5 stacks/second when you stop tapping
- Visual feedback: screen border glows brighter with Momentum; at max, gold sparks fly from each tap

**Per-Class Tap Mechanics:**
Each hero class modifies what tapping does (detailed in the class roster below). Examples:
- Warrior: taps trigger shockwaves
- Rogue: taps build combo points for a finishing blow
- Marksman: hold-to-charge for bigger damage
- Gunslinger: ultra-fast taps with a reload rhythm

### Hero Auto-Attacks

Each hero in the 4-hero party auto-attacks independently:
- Auto-attack speed varies by class (Berserker = fast, Defender = slow but heavy)
- Auto-attacks are the primary DPS source during idle play
- Active tapping supplements auto-attacks, roughly doubling overall DPS when engaged

### Ability System

Each hero has **3 Ability Slots**:

| Slot | Type | Activation |
|------|------|-----------|
| Ability 1 | Core class skill | Cooldown-based, tap hero portrait to activate |
| Ability 2 | Utility/Secondary | Cooldown-based, tap hero portrait to activate |
| Ultimate | Powerful finisher | Charges from dealing/taking damage; tap when ready |

- Abilities are positioned as 4 hero portraits along the bottom of the combat screen
- Tap a portrait once to fire Ability 1; double-tap for Ability 2; hold for Ultimate (when charged)
- **Auto-Cast Toggle:** Players can enable auto-cast for any ability, turning it into a passive that fires on cooldown
- Auto-cast is slightly less efficient than manual timing (90% effectiveness) to reward active play

### Damage Types

| Type | Strong Against | Weak Against |
|------|---------------|-------------|
| Physical | Beasts, Constructs | Ethereal, Armored |
| Magical | Armored, Ethereal | Beasts, Resistant |
| True | Nothing resists | Nothing is weak (rare, endgame) |

---

## 4. AFK & Idle Systems

### Offline Progression

When the app is closed, heroes continue fighting at **50% combat efficiency**:
- Stages do not advance (heroes farm the current stage repeatedly)
- Gold, XP, and materials accumulate in the **AFK Vault**
- AFK Vault caps at **10 hours** of accumulated rewards
- Push notification at 8 hours: *"Your heroes have been fighting! Claim 1.2M Gold and 340K XP before the vault overflows."*

### AFK Vault Collection

On app launch, the AFK Vault is the **first thing visible** — a floating chest at the top of the combat screen, glowing and pulsing. Tap to open and claim.

**AFK Vault Contents:**
- Gold (based on highest stage cleared x time)
- Hero XP (distributed to active party)
- Materials (common and uncommon crafting drops)
- Rare chance (~5%) of equipment drop from AFK farming
- Bonus multiplier if the player returns within 6 hours of last session

### Expeditions (Idle Hero Deployment)

Heroes **not** in the active 4-hero party can be sent on **Expeditions** — timed idle missions:

| Expedition Type | Duration | Reward |
|----------------|----------|--------|
| Resource Gathering | 2 hours | Gold + crafting materials |
| Scouting Mission | 4 hours | Island intel + Stardust |
| Dungeon Delve | 8 hours | Equipment + Class Tokens |
| Rare Expedition | 12 hours | Legendary material + Ascension Shards |

- Up to 3 expeditions running simultaneously
- Higher-level heroes on expeditions yield better rewards
- Expedition slots unlock as the player progresses (start with 1, max 3)

### Active vs. Idle Balance

| Metric | Active Play | Idle/AFK |
|--------|------------|----------|
| Stage Progression | Yes (push new stages) | No (farm current stage) |
| Gold/XP Rate | 2-3x multiplier | 1x base rate (at 50% efficiency) |
| Boss Encounters | Required (active only) | Skipped |
| Equipment Drops | Full drop table | Common/Uncommon only |
| Ability Optimization | Manual timing = 100% | Auto-cast = 90% |

**Design Philosophy:** Active play should feel significantly more rewarding, but a player who only logs in twice daily should still make meaningful progress. Never punish absence — always reward presence.

---

## 5. Hero Classes — Full Roster of 24

### Class Unlock Tiers

| Tier | Name | Classes | How to Unlock |
|------|------|---------|--------------|
| 1 | **Starter** | Warrior, Mage, Priest, Rogue | Available from game start |
| 2 | **Apprentice** | Marksman, Defender, Berserker, Druid, Thief, Shaman | Reach Island 3 |
| 3 | **Adept** | Warlock, Ranger, Spell-Blade, Necromancer, Monk, Paladin, Bard | Reach Island 6 or via Astral Summon |
| 4 | **Master** | Dragon-Hunter, Summoner, Alchemist, Chronomancer, Gunslinger, Warden, Reaper | Reach Island 10 or via Astral Summon |

### Role Categories

| Role | Description | Classes |
|------|-------------|---------|
| **Vanguard** | Frontline fighters and tanks | Warrior, Berserker, Defender, Monk, Paladin, Warden |
| **Striker** | Melee burst damage dealers | Rogue, Thief, Spell-Blade, Reaper |
| **Caster** | Ranged magical damage | Mage, Warlock, Necromancer, Chronomancer |
| **Ranger** | Ranged physical damage | Marksman, Ranger, Gunslinger |
| **Support** | Healers, buffers, utility | Priest, Druid, Shaman, Bard, Alchemist |
| **Specialist** | Niche roles with unique mechanics | Dragon-Hunter, Summoner |

---

### Tier 1 — Starter Classes

---

#### WARRIOR
*The Stalwart Blade of Dawn*

| | |
|-|-|
| **Role** | Vanguard |
| **Affinity** | Radiance |
| **Position** | Frontline |
| **Difficulty** | Beginner |

**Identity:** The quintessential hero — a balanced frontliner with reliable damage and moderate tankiness. The Warrior is the first hero every player receives, designed to teach core mechanics through straightforward, satisfying gameplay.

**Tap Mechanic — Valor Strike:**
Each tap deals bonus physical damage. Every 5th tap triggers a **Shockwave** that hits all enemies on screen for 50% of tap damage. The shockwave grows visually larger with Momentum stacks, creating a satisfying rhythmic loop: tap-tap-tap-tap-BOOM.

**Passive — Battle Rhythm:**
Auto-attack speed increases by 2% for every 10 seconds of continuous combat, stacking up to 20%. Resets when entering a new stage. Rewards sustained engagement and synergizes with longer boss fights.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Cleaving Strike** | Deals 200% ATK to all frontline enemies | 8s |
| **War Cry** | Increases party ATK by 15% for 10s | 20s |
| **Ascendant Blade** (Ult) | Leaps skyward and slams down for 800% ATK in a massive AoE; enemies hit are stunned for 2s | Charge |

**Skill Tree Paths:**
- **Blade:** Raw damage, cleave radius, critical strike chance. *For players who want the Warrior as a damage dealer.*
- **Shield:** Defense, counterattack chance, taunt duration. *Turns the Warrior into a tank.*
- **Commander:** Party-wide buffs, War Cry enhancements, leadership auras. *The supportive frontliner.*

---

#### MAGE
*Architect of Arcane Frost*

| | |
|-|-|
| **Role** | Caster |
| **Affinity** | Frost |
| **Position** | Backline |
| **Difficulty** | Beginner |

**Identity:** A glass cannon who commands devastating area-of-effect frost magic. The Mage teaches players about positioning (staying in the backline), resource management (mana), and the power of AoE damage.

**Tap Mechanic — Arcane Bolt:**
Each tap fires a homing magic missile that deals magic damage to the target. If the missile kills the target, it chains to one nearby enemy at 50% damage. At high Momentum, missiles split into 3 projectiles. Visually spectacular: glowing blue-white bolts with frost trails.

**Passive — Mana Surge:**
When mana is above 80%, ability cooldowns recover 30% faster and all magic damage is increased by 15%. Creates a decision: do you spam abilities for burst, or conserve mana for the surge bonus?

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Frost Nova** | Deals 150% ATK in an AoE circle; freezes all enemies hit for 1.5s | 10s |
| **Blizzard** | Channels a storm over 5s, dealing 50% ATK per second to all enemies and slowing them 30% | 25s |
| **Absolute Zero** (Ult) | Massive ice explosion dealing 1000% ATK to all enemies; frozen enemies take triple damage | Charge |

**Skill Tree Paths:**
- **Frost:** Freeze duration, shatter damage (bonus when hitting frozen targets), AoE radius. *Maximum crowd control.*
- **Arcane:** Raw spell damage, missile count, mana efficiency. *Pure damage output.*
- **Ether:** Mana regeneration, party magic damage buff, spell resistance aura. *Supportive caster.*

---

#### PRIEST
*Vessel of Divine Light*

| | |
|-|-|
| **Role** | Support |
| **Affinity** | Radiance |
| **Position** | Backline |
| **Difficulty** | Beginner |

**Identity:** The essential healer who channels holy light to keep the party alive. The Priest teaches players about team management — watching health bars, timing heals, and the value of keeping everyone alive. Also deals bonus damage to Shadow enemies.

**Tap Mechanic — Holy Touch:**
Taps alternate function: if any ally is below 50% HP, the tap heals the lowest-HP ally for 100% of tap power. If all allies are above 50% HP, the tap deals holy damage to the target enemy instead. This automatic triage creates a "whack-a-mole" healing rhythm that feels active without being stressful.

**Passive — Divine Grace:**
While the Priest is alive, all party members take 10% reduced damage. If the Priest dies, the buff lingers for 5 seconds before fading, giving the team a brief window.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Healing Light** | Heals all allies for 15% of their max HP | 8s |
| **Sacred Shield** | Places a shield on the lowest-HP ally absorbing damage equal to 20% of Priest's max HP for 8s | 15s |
| **Resurrection** (Ult) | Revives all fallen allies at 50% HP and grants the party invincibility for 3s | Charge |

**Skill Tree Paths:**
- **Restoration:** Heal power, HoT effects, overheal shields. *Maximum healing throughput.*
- **Protection:** Shield strength, damage reduction auras, cleanse effects. *Preventive healing.*
- **Wrath:** Holy damage, bonus vs Shadow/Undead enemies, smite effects. *The battle priest.*

---

#### ROGUE
*Shadow Dancer of the Unseen Edge*

| | |
|-|-|
| **Role** | Striker |
| **Affinity** | Shadow |
| **Position** | Frontline |
| **Difficulty** | Intermediate |

**Identity:** A high-skill-ceiling burst damage dealer built around combo points. The Rogue rewards fast, rhythmic tapping and strategic ability timing. She darts in and out of shadows, striking with lethal precision.

**Tap Mechanic — Shadow Strike:**
Each tap generates 1 **Combo Point** (max 5). At 5 Combo Points, the next tap triggers **Finishing Blow** — a devastating strike dealing 500% of normal tap damage that consumes all Combo Points. Combo Points decay after 3 seconds of not tapping, creating urgency. Visual feedback: combo point pips light up around the Rogue's portrait; Finishing Blow triggers a dramatic slash animation.

**Passive — Cloak of Shadows:**
15% chance to dodge any attack. When below 50% HP, dodge chance increases to 30%. Each dodge generates 1 Combo Point passively.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Ambush** | Teleport behind the highest-ATK enemy, deal 250% ATK, and gain 3 Combo Points | 10s |
| **Smoke Bomb** | Party gains 30% dodge chance for 5s; Rogue becomes untargetable for 3s | 18s |
| **Death's Embrace** (Ult) | 10 rapid strikes on the target, each dealing 150% ATK; if the target dies, cooldown resets | Charge |

**Skill Tree Paths:**
- **Assassination:** Finishing Blow damage, crit rate, crit damage multiplier. *Maximum burst.*
- **Subtlety:** Dodge chance, movement speed, untargetable duration. *Survivable skirmisher.*
- **Venom:** Poison DoTs on all attacks, weakening debuffs, AoE poison cloud. *Sustained damage over time.*

---

### Tier 2 — Apprentice Classes

---

#### MARKSMAN
*The Frost-Veined Sharpshooter*

| | |
|-|-|
| **Role** | Ranger |
| **Affinity** | Frost |
| **Position** | Backline |
| **Difficulty** | Intermediate |

**Identity:** Precision over speed. The Marksman delivers devastating single-target damage through charged shots and armor penetration. Where the Mage dominates crowds, the Marksman executes priority targets.

**Tap Mechanic — Aimed Shot:**
**Hold** the tap to charge a shot. Charge time: 0.5s (min) to 2s (max). Damage scales from 100% to 400% of tap power based on charge. Fully charged shots are guaranteed critical hits. Quick taps still work at base damage for rapid-fire play. This creates a unique rhythm compared to other classes — deliberate, satisfying single shots versus spam tapping.

**Passive — Steady Hand:**
Each consecutive hit on the same target increases damage by 5%, stacking up to 10 times (+50%). Switching targets resets the stacks. This makes the Marksman devastating against bosses but weaker in multi-enemy waves.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Piercing Shot** | Fires a shot that penetrates all enemies in a line, dealing 200% ATK and reducing armor by 20% for 8s | 10s |
| **Eagle Eye** | Marks a target for 10s; all party members deal 25% more damage to the marked target | 20s |
| **Killshot** (Ult) | A single devastating shot dealing 1500% ATK to one target; if the target is below 30% HP, damage is doubled | Charge |

**Skill Tree Paths:**
- **Sniper:** Charge speed, headshot multiplier, single-target damage. *The boss killer.*
- **Barrage:** Multi-shot unlocks, split-shot, rapid fire mode. *AoE capability at reduced per-target damage.*
- **Tactical:** Debuffs, armor shred, enemy vulnerability marks. *Force multiplier for the whole party.*

---

#### DEFENDER
*Frost-Forged Fortress*

| | |
|-|-|
| **Role** | Vanguard (Tank) |
| **Affinity** | Frost |
| **Position** | Frontline |
| **Difficulty** | Beginner |

**Identity:** The immovable object. Where the Warrior balances offense and defense, the Defender commits fully to protecting the party. Massive HP pool, damage reduction, and the ability to force enemies to attack her.

**Tap Mechanic — Shield Bash:**
Taps deal reduced damage (60% of normal tap power) but apply a stacking **Weakened** debuff on the target: -3% defense per stack, up to 15 stacks (-45% defense). This makes the Defender a setup class — she softens enemies for the party's damage dealers.

**Passive — Bulwark:**
Absorbs 20% of all damage dealt to adjacent party members (heroes in neighboring formation slots). The Defender literally takes hits for the team.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Taunt** | Forces all enemies to attack the Defender for 5s; gains 30% damage reduction during taunt | 12s |
| **Frost Shield** | Gains a shield equal to 25% max HP; enemies who attack the shield are slowed 20% | 18s |
| **Unbreakable** (Ult) | Becomes immune to damage for 5s; reflects 50% of incoming damage back to attackers | Charge |

**Skill Tree Paths:**
- **Fortress:** Max HP, damage reduction, healing received bonus. *The unkillable wall.*
- **Retribution:** Reflect damage, counterattack chance, thorns aura. *Punishes enemies for attacking.*
- **Sentinel:** Bulwark absorption %, party defense buffs, CC immunity aura. *The team protector.*

---

#### BERSERKER
*Flame-Blooded Fury*

| | |
|-|-|
| **Role** | Vanguard (Melee DPS) |
| **Affinity** | Flame |
| **Position** | Frontline |
| **Difficulty** | Advanced |

**Identity:** A high-risk, high-reward melee monster who grows more dangerous as she takes damage. The Berserker dances on the edge of death, converting pain into raw destructive power through a unique Rage resource.

**Tap Mechanic — Rampage:**
Taps build **Rage** (1 per tap, also gains Rage when taking damage). At 0-50 Rage, taps deal normal damage. At 50-80 Rage, taps deal 2x damage. At 80-100 Rage, taps deal 3x damage BUT each tap costs 2% of max HP. This creates an exhilarating risk dynamic — do you keep tapping at max rage for insane damage while watching your health plummet?

**Passive — Blood Frenzy:**
Attack speed increases by 2% per 10% of HP missing. At 10% HP, the Berserker attacks 18% faster. Synergizes brutally with the Rage system.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Reckless Charge** | Dashes through all enemies in a line, dealing 180% ATK to each; gains 30 Rage | 8s |
| **Blood Howl** | Sacrifices 15% current HP to grant +40% ATK to self and +20% ATK to all allies for 8s | 15s |
| **Cataclysm** (Ult) | Consumes ALL Rage; deals (100% + 10% per Rage consumed) ATK in a massive AoE; at 100 Rage, this is 1100% ATK | Charge |

**Skill Tree Paths:**
- **Fury:** Max Rage, Rage damage multipliers, Rage generation. *The berserker's berserker — max risk, max reward.*
- **Bloodlust:** Lifesteal on all attacks, HP recovery on kill, sustain. *Solves the self-damage problem.*
- **Titan:** Max HP, damage reduction at low HP, Rage generation from defense. *The tanky berserker.*

---

#### DRUID
*Keeper of the Verdant Cycle*

| | |
|-|-|
| **Role** | Support |
| **Affinity** | Nature |
| **Position** | Backline |
| **Difficulty** | Intermediate |

**Identity:** A versatile nature healer who sustains allies through regenerative HoTs (heals over time) and can shapeshift into animal forms for emergencies. The Druid rewards planning — her HoTs need to be placed before damage comes, not after.

**Tap Mechanic — Nature's Touch:**
Taps alternate: **Odd taps** place a Rejuvenation HoT on the lowest-HP ally (heals 3% max HP over 6s, stacks up to 3 times). **Even taps** grow Thorny Vines on the current enemy target (deals 50% tap damage over 4s, stacks up to 3 times). This creates a weaving rhythm — heal, damage, heal, damage — that makes the Druid feel uniquely active.

**Passive — Wild Growth:**
All healing done by the Druid increases by 5% every 10 seconds of continuous combat, stacking up to 50%. Resets on shapeshift or new stage. Rewards sustained fights (boss encounters).

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Regrowth** | Places a powerful HoT on all allies: heals 5% max HP per second for 8s | 15s |
| **Bear Form** | Shapeshifts into a bear for 10s: moves to frontline, gains 50% max HP, taunts enemies; cannot heal in this form | 25s |
| **Nature's Wrath** (Ult) | The battlefield erupts with nature magic: all enemies take 500% ATK nature damage; all allies are healed to full HP | Charge |

**Skill Tree Paths:**
- **Restoration:** HoT power, number of HoT stacks, group healing. *The dedicated healer.*
- **Feral:** Bear Form duration and power, Cat Form unlock (high DPS), shapeshift cooldown reduction. *The combat shapeshifter.*
- **Guardian:** Bear Form becomes default; permanent frontline with nature-based tanking. *The nature tank (before Warden is unlocked).*

---

#### THIEF
*Fortune's Favorite Scoundrel*

| | |
|-|-|
| **Role** | Striker |
| **Affinity** | Shadow |
| **Position** | Frontline |
| **Difficulty** | Beginner |

**Identity:** A resourceful scoundrel whose combat prowess is secondary to her true talent: making money. The Thief dramatically increases gold and resource income, making her essential for progression even when her raw damage lags behind other Strikers. She also steals enemy buffs.

**Tap Mechanic — Pickpocket:**
Each tap has a 20% chance to generate bonus gold (50% of the enemy's gold value). Critical taps (based on crit rate) additionally **steal** one random buff from the enemy and apply it to the Thief for 5 seconds. Gold coins visually fly out of enemies with satisfying "cha-ching" feedback.

**Passive — Lucky Break:**
+30% gold from all sources (combat, AFK, expeditions) while the Thief is in the active party. +10% equipment drop rate. This is the strongest passive economy buff in the game, making the Thief a staple for farming compositions.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Mug** | Strikes the target for 150% ATK and steals gold equal to 200% of the enemy's normal drop | 8s |
| **Evasion** | Gains 50% dodge for 5s; each dodge generates bonus gold | 14s |
| **Jackpot** (Ult) | All enemies on screen drop 500% bonus gold; 10% chance for each enemy to drop a piece of equipment | Charge |

**Skill Tree Paths:**
- **Plunder:** Gold multipliers, material drop rates, AFK reward bonuses. *Maximum economy.*
- **Shadow:** Stealth, dodge, crit rate — a secondary damage dealer. *The Rogue-lite.*
- **Sabotage:** Debuffs, disarm, armor strip. *The utility thief.*

---

#### SHAMAN
*Voice of the Living Storm*

| | |
|-|-|
| **Role** | Support |
| **Affinity** | Storm |
| **Position** | Backline |
| **Difficulty** | Intermediate |

**Identity:** A totem-planting spellcaster who creates persistent zones of power on the battlefield. The Shaman's unique mechanic is totem management — placing, upgrading, and combining totems for powerful battlefield control.

**Tap Mechanic — Lightning Bolt:**
Taps call down chain lightning that bounces between enemies. For each active totem on the field, the lightning gains one additional bounce. With 3 totems active, each tap's lightning chains 4 times. Satisfying electrical crackle with each chain.

**Passive — Elemental Harmony:**
For each active totem, all party members gain +5% to a random stat (ATK, DEF, SPD, or HP regen). With 3 totems, the party has 3 random buffs that rotate every 15 seconds.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Storm Totem** | Places a totem that deals 30% ATK/s to nearby enemies for 15s | 10s |
| **Healing Totem** | Places a totem that heals all allies for 3% max HP/s for 15s | 12s |
| **Elemental Overload** (Ult) | All active totems explode for 400% ATK each, then are immediately replaced with empowered versions (double effect for 10s) | Charge |

**Skill Tree Paths:**
- **Storm:** Damage totem power, lightning bolt chains, storm AoE. *Offensive support.*
- **Spirit:** Healing totem power, resurrection totem unlock, spirit link (share HP pool). *Dedicated healing.*
- **Earth:** Earthen totem (taunt + absorb), earthquake on totem placement, crowd control. *Defensive support.*

---

### Tier 3 — Adept Classes

---

#### WARLOCK
*Flame-Pact Sorcerer*

| | |
|-|-|
| **Role** | Caster |
| **Affinity** | Flame |
| **Position** | Backline |
| **Difficulty** | Advanced |

**Identity:** A dark bargainer who trades her own health for devastating fire and shadow magic. The Warlock has the highest potential DPS among casters but demands careful health management. She's the opposite of the Priest — sacrificing safety for power.

**Tap Mechanic — Soulfire:**
Each tap drains 1% of the Warlock's current HP but deals 150% of normal tap damage as dark fire. If healing hits the Warlock while Soulfire is active, 50% of the healing is converted to bonus damage on the next tap instead. This creates a fascinating synergy with healers — and a deadly tension without one.

**Passive — Dark Pact:**
All abilities cost HP instead of having cooldowns (each ability costs 10-20% current HP). In exchange, abilities deal 30% more damage and have no cooldown — limited only by the Warlock's remaining HP. This transforms ability management from cooldown-watching to health-budgeting.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Corruption** | Curses the target for 300% ATK over 10s; if the target dies while cursed, the curse spreads to 2 nearby enemies | HP cost |
| **Life Drain** | Deals 200% ATK and heals the Warlock for 100% of damage dealt | HP cost |
| **Hellfire** (Ult) | Rains fire across the entire battlefield for 5s, dealing 150% ATK/s to all enemies; Warlock takes 5% max HP per second during channel | Charge |

**Skill Tree Paths:**
- **Affliction:** DoT damage, curse spread, multi-target corruption. *Death by a thousand burns.*
- **Destruction:** Direct damage spells, fire AoE, critical spell damage. *Burst caster.*
- **Drain:** Lifesteal efficiency, HP threshold safety net, hybrid sustain. *Sustainable dark magic.*

---

#### RANGER
*Storm-Bonded Beastmaster*

| | |
|-|-|
| **Role** | Ranger |
| **Affinity** | Storm |
| **Position** | Backline (Pet in Frontline) |
| **Difficulty** | Intermediate |

**Identity:** A ranged fighter who shares the battlefield with a loyal beast companion. The Ranger is effectively two units in one — the Ranger attacks from the back while the Pet fights in melee. Managing both is the core skill expression.

**Tap Mechanic — Coordinated Strike:**
Taps command both Ranger AND Pet to focus the same target simultaneously. Rapid taps build a **Bond** meter (fills in ~15 taps). When full, the next tap triggers **Pack Attack** — both Ranger and Pet strike for 300% combined ATK with a guaranteed critical hit. The Bond meter provides a satisfying build-and-release rhythm.

**Passive — Beast Bond:**
The Pet gains 50% of the Ranger's ATK, DEF, and HP stats. If the Pet dies, it resurrects automatically after 30 seconds. While the Pet is dead, the Ranger gains +30% ATK (fighting in grief-fueled fury).

**Pet:** The default companion is a **Storm Wolf** (melee, moderate damage, fast attacks). Later, players can unlock alternate pets through gameplay:
- **Thunder Bear** — Slow, tanky, AoE slam attacks
- **Lightning Hawk** — Ranged, fast, crit-focused
- **Storm Serpent** — Poison + lightning hybrid, DoT-focused

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Hunter's Volley** | Fires 5 arrows at random enemies, each dealing 80% ATK | 8s |
| **Fetch!** | Sends Pet to attack the highest-HP enemy, dealing 200% ATK and taunting it for 5s | 14s |
| **Primal Fury** (Ult) | Both Ranger and Pet enter a frenzy for 8s: +50% ATK, +50% attack speed. Pet attacks generate healing for the Ranger. | Charge |

**Skill Tree Paths:**
- **Beastmaster:** Pet stats, Pet abilities, Pet revival speed. *Maximum companion power.*
- **Survival:** Ranger personal damage, traps, evasion. *Self-sufficient hunter.*
- **Pack:** Unlock a second minor pet, summon wild beasts. *Army of animals.*

---

#### SPELL-BLADE
*Tempest Sword*

| | |
|-|-|
| **Role** | Striker |
| **Affinity** | Storm |
| **Position** | Frontline |
| **Difficulty** | Advanced |

**Identity:** A hybrid melee-magic warrior who weaves lightning into swordplay. The Spell-Blade alternates between physical strikes and magical bursts, with each enhancing the other. This class has the highest skill ceiling among Strikers.

**Tap Mechanic — Arc Slash:**
Taps deal melee damage and build **Arcane Edge** charges (1 per tap, max 5). At 5 charges, the next tap fires a ranged **Energy Wave** that deals 300% ATK magic damage to all enemies in a line and consumes all charges. This creates a melee-melee-melee-melee-melee-RANGED rhythm that feels kinetic and powerful.

**Passive — Spellweave:**
Each melee hit increases the next spell's damage by 10% (max 5 stacks = +50%). Each spell cast increases the next melee hit's damage by 10% (max 5 stacks = +50%). Alternating perfectly between melee and magic yields permanent +50% damage to both. This rewards skilled play.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Thunder Slash** | Melee strike dealing 200% ATK physical + 200% ATK lightning damage | 8s |
| **Arcane Shield** | Absorbs 15% max HP in damage; when the shield breaks, it explodes for 150% ATK magic damage to nearby enemies | 16s |
| **Storm Surge** (Ult) | Channels lightning into the blade for 6s; all attacks deal hybrid physical+magic damage, attack speed doubles, and every hit chains lightning to one additional enemy | Charge |

**Skill Tree Paths:**
- **Tempest:** Lightning-enhanced melee, chain lightning on hit, storm AoE. *Melee-focused hybrid.*
- **Battlemage:** Balanced melee and spell power, Spellweave bonuses, Energy Wave enhancements. *The true hybrid.*
- **Mystic Knight:** Arcane Shield enhancements, magic resistance, spell-enhanced defense. *The magical frontliner.*

---

#### NECROMANCER
*Sovereign of the Restless Dead*

| | |
|-|-|
| **Role** | Caster |
| **Affinity** | Shadow |
| **Position** | Backline |
| **Difficulty** | Advanced |

**Identity:** A master of undeath who raises an ever-growing army from fallen enemies. The Necromancer's power scales with combat length — the longer the fight, the larger the skeleton army, the more overwhelming the damage. Uniquely, the Necromancer's minions persist between stages.

**Tap Mechanic — Death Grasp:**
Taps deal shadow damage. Killing an enemy with a tap has a 40% chance to raise it as a **Skeleton Minion** (max 5 minions). Minions auto-attack the nearest enemy for 30% of the Necromancer's ATK. Each minion has its own health bar (25% of Necromancer's max HP). This creates a snowball dynamic — more kills mean more minions mean more kills.

**Passive — Army of the Dead:**
Each active minion increases the Necromancer's damage by 3% and reduces damage taken by 2%. At max 5 minions: +15% damage, +10% damage reduction. Minions persist between stages (but decay after 60 seconds without combat).

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Raise Dead** | Instantly raises 2 Skeleton Minions regardless of kills; if at max minions, existing ones are empowered (+50% ATK) for 10s | 12s |
| **Death Coil** | Deals 250% ATK shadow damage to target; if it kills, heals the Necromancer for 20% max HP | 10s |
| **Undead Legion** (Ult) | Raises ALL dead enemies from the current fight as minions (beyond the normal cap) for 15s; army deals 50% ATK each per second as a swarming mass | Charge |

**Skill Tree Paths:**
- **Undeath:** Minion count cap, minion strength, minion abilities (Skeleton Archer, Zombie Tank). *Army commander.*
- **Decay:** AoE shadow damage, life drain, corpse explosion (dead enemies explode for AoE). *Direct damage caster.*
- **Lich:** Self-transformation into a Lich for 20s — massively increased stats, no minions during transformation. *Personal power over army.*

---

#### MONK
*Disciple of the Inner Tempest*

| | |
|-|-|
| **Role** | Vanguard |
| **Affinity** | Storm |
| **Position** | Frontline |
| **Difficulty** | Advanced |

**Identity:** A disciplined martial artist who channels inner storms through precise combat techniques. The Monk is built around combo chains — executing specific tap sequences for powerful finishers. She's the highest-skill-ceiling Vanguard, rewarding rhythm and timing over raw stats.

**Tap Mechanic — Flurry of Blows:**
Taps execute a **5-hit combo chain**: Jab (1) -> Cross (2) -> Hook (3) -> Uppercut (4) -> **Perfect Strike** (5). Each hit in the chain deals escalating damage (80%, 100%, 120%, 150%, 250% of tap power). Completing the full chain triggers **Perfect Strike** with a dramatic slow-motion impact. Missing the timing window (1 second between taps) resets the chain. The Monk rewards rhythmic, deliberate tapping over frantic spam.

**Passive — Inner Peace:**
Regenerates 2% max HP per second while not attacking (between stages or during brief pauses). **Chi** generation doubles after dodging an attack. Chi is spent on abilities — the Monk has no cooldowns, only Chi costs.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Tiger Palm** | Powerful strike dealing 200% ATK; generates 20 Chi on hit | 20 Chi |
| **Spinning Crane Kick** | AoE spin hitting all nearby enemies for 150% ATK each; heals 5% max HP per enemy hit | 40 Chi |
| **One Thousand Fists** (Ult) | Unleashes a barrage of 20 strikes over 3 seconds, each dealing 100% ATK; the final strike deals 500% ATK and stuns for 3s | 80 Chi |

**Skill Tree Paths:**
- **Iron Fist:** Strike damage, combo speed, Perfect Strike multiplier. *Maximum melee damage.*
- **Flowing River:** Dodge chance, counter-attack on dodge, Chi generation. *Evasion tank.*
- **Zen:** Self-healing, party auras, meditation (channel for massive HP regen + Chi). *The sustain fighter.*

---

#### PALADIN
*Crusader of the Eternal Dawn*

| | |
|-|-|
| **Role** | Vanguard (Tank/Support Hybrid) |
| **Affinity** | Radiance |
| **Position** | Frontline |
| **Difficulty** | Intermediate |

**Identity:** A holy crusader who simultaneously tanks and heals — the bridge between Defender and Priest. The Paladin sacrifices the Defender's pure tankiness and the Priest's raw healing power for the ability to do both at once. In 4-hero parties, the Paladin frees up a slot that would normally go to a dedicated healer.

**Tap Mechanic — Judgment:**
Each tap deals holy damage to the target AND heals the lowest-HP ally for 20% of the damage dealt. This creates a "heal-by-fighting" loop that feels heroic and proactive. Against Shadow-affinity enemies, healing is doubled.

**Passive — Divine Shield:**
Every 30 seconds, the Paladin automatically blocks the next hit that would reduce her below 20% HP, negating all damage from that hit and healing to 30% HP. This "cheat death" mechanic provides insurance against burst damage.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Consecration** | Sanctifies the ground for 8s; enemies standing in it take 50% ATK/s holy damage; allies standing in it heal 3% max HP/s | 15s |
| **Lay on Hands** | Instantly heals one ally to full HP; can target self | 30s |
| **Avenging Wrath** (Ult) | For 10s: all attacks deal 2x damage, all damage dealt heals the entire party for 30% of damage done, Paladin gains 50% damage reduction | Charge |

**Skill Tree Paths:**
- **Crusader:** Holy damage, Judgment healing multiplier, consecration damage. *The offensive paladin.*
- **Guardian:** Damage reduction, Divine Shield frequency, taunt abilities. *The tanky paladin.*
- **Templar:** Party healing, aura buffs, Lay on Hands cooldown reduction. *The support paladin.*

---

#### BARD
*Maestro of the Celestial Orchestra*

| | |
|-|-|
| **Role** | Support |
| **Affinity** | Radiance |
| **Position** | Backline |
| **Difficulty** | Intermediate |

**Identity:** A musical enchanter who maintains persistent "Song" auras that buff the party. The Bard's unique mechanic is **Song Management** — choosing which song to play and building toward powerful Crescendo moments. Unlike other supports, the Bard's power is always-on through songs, with tap engagement layered on top.

**Tap Mechanic — Power Chord:**
Taps send musical shockwaves at enemies, dealing magic damage. Every 8th tap triggers a **Crescendo** — the current active Song's effect is amplified by 200% for 5 seconds. A visual counter (musical notes) tracks progress toward each Crescendo.

**Songs (One Active at a Time):**
| Song | Effect |
|------|--------|
| **Ballad of Valor** | Party ATK +15% |
| **Hymn of Fortitude** | Party DEF +15%, HP regen +3%/s |
| **March of Haste** | Party attack speed +20%, ability cooldowns -15% |

Switching songs has a 3-second transition where no song is active. Crescendo during Ballad = +45% ATK for 5s. Crescendo during Hymn = +45% DEF and +9% HP regen for 5s. Crescendo during March = +60% attack speed for 5s.

**Passive — Encore:**
When an ally uses an ability, 10% chance to duplicate its effect at 50% power. This procs on all ally abilities, creating unexpected and exciting "double cast" moments.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Inspiring Melody** | Instantly triggers a Crescendo regardless of tap count | 15s |
| **Discordant Note** | Deals 200% ATK to target and silences it for 3s (prevents special attacks) | 12s |
| **Symphony of Ascension** (Ult) | All three Songs play simultaneously for 12s; Crescendo triggers every 4 taps instead of 8 | Charge |

**Skill Tree Paths:**
- **Ballad:** Healing songs, shield songs, party sustain. *The healing bard.*
- **War Drums:** Damage songs, attack speed, Crescendo damage. *The offensive bard.*
- **Hymn:** Debuff songs, enemy silencing, crowd control. *The controller.*

---

### Tier 4 — Master Classes

---

#### DRAGON-HUNTER
*Slayer of Leviathans*

| | |
|-|-|
| **Role** | Specialist |
| **Affinity** | Nature |
| **Position** | Flexible (Adapts) |
| **Difficulty** | Intermediate |

**Identity:** An elite monster slayer purpose-built for boss encounters. The Dragon-Hunter deals dramatically more damage to bosses and elite enemies, learns from each attempt, and carries an arsenal of traps and specialized ammunition. In regular stage clearing, she's merely competent; against bosses, she's unmatched.

**Tap Mechanic — Dragonbane Bolt:**
**Hold** to charge a heavy crossbow shot (similar to Marksman but with a twist): charge damage scales from 100% to 300% of tap power normally, but against Boss-type enemies, scaling goes to **500%**. Fully charged shots against bosses also apply a "Dragonbane" debuff: target takes +5% more damage from all sources for 8s (stacks 3x for +15%).

**Passive — Slayer's Instinct:**
+50% damage against bosses and elite enemies at all times. Additionally, each failed attempt against the same boss increases the Dragon-Hunter's damage against that boss by 5% (stacks up to +50%, persisting until the boss is killed). This means bosses that block progression become easier over time — a frustration valve.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Bear Trap** | Places a trap that immobilizes the first enemy to cross it for 4s and deals 300% ATK | 10s |
| **Exploit Weakness** | Scans the target, revealing its elemental weakness for 15s; all party damage of that element is doubled | 20s |
| **Dragon's Bane** (Ult) | A massive crossbow bolt imbued with dragon-slaying magic: 2000% ATK to a single target; if the target is a Boss, deals an additional 2000% ATK | Charge |

**Skill Tree Paths:**
- **Slayer:** Boss damage multipliers, Dragonbane debuff stacks, critical damage vs. bosses. *The ultimate boss killer.*
- **Trapper:** Multiple trap types, trap damage, crowd control duration. *Area denial specialist.*
- **Hunter:** Tracking debuffs, party damage buffs against marked targets, loot bonuses from bosses. *Force multiplier.*

---

#### SUMMONER
*Elemental Conductor*

| | |
|-|-|
| **Role** | Specialist |
| **Affinity** | Nature |
| **Position** | Backline |
| **Difficulty** | Advanced |

**Identity:** A conjurer who commands elemental familiars, with each familiar fundamentally changing the Summoner's combat role. The Summoner is the most flexible class in the game — a single Summoner can fill DPS, tank, or support roles depending on which familiar is active. Swapping familiars mid-fight is the core skill expression.

**Familiars (One Active at a Time):**
| Familiar | Role | Behavior |
|----------|------|----------|
| **Ember Sprite** | AoE DPS | Flies around casting fire AoE; Summoner's taps deal fire damage |
| **Frost Golem** | Tank | Stands in frontline, taunts, absorbs damage; Summoner's taps heal the Golem |
| **Storm Hawk** | Burst DPS | Dive-bombs targets for critical hits; Summoner's taps increase Hawk's crit rate |
| **Stone Sentinel** | CC/Defense | Creates rock walls, roots enemies; Summoner's taps generate shields for allies |

**Tap Mechanic — Command:**
Taps direct the active familiar to attack the targeted enemy. **Swipe left/right** on the Summoner's portrait to cycle familiars (2-second swap cooldown). Each swap triggers a **Transition Burst** — the departing familiar's farewell attack (Ember leaves a fire trail, Frost leaves a frost patch, etc.).

**Passive — Elemental Attunement:**
The Summoner gains 15% of the active familiar's strongest stat (ATK for Ember, DEF for Frost, SPD for Storm, HP for Stone). Familiars gain 50% of the Summoner's base stats.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Empower Familiar** | Active familiar gains +50% all stats for 10s | 15s |
| **Dual Summon** | Summons a second familiar for 8s (both active simultaneously) | 25s |
| **Primal Convergence** (Ult) | All four familiars manifest simultaneously for 10s; each attacks independently; Summoner's taps command all four at once for quadrupled tap effectiveness | Charge |

**Skill Tree Paths:**
- **Conjuration:** Familiar stats, familiar abilities, familiar persistence after swap. *Stronger individual familiars.*
- **Duality:** Dual Summon duration, reduced swap cooldown, transition burst damage. *Multi-familiar combat.*
- **Synergy:** Familiar buffs affect party members, party auras based on active familiar, shared familiar stats. *The team-oriented Summoner.*

---

#### ALCHEMIST
*Volatile Genius*

| | |
|-|-|
| **Role** | Support/DPS Hybrid |
| **Affinity** | Flame |
| **Position** | Backline |
| **Difficulty** | Intermediate |

**Identity:** A brilliant (and slightly unhinged) potion-crafter who throws concoctions that heal, explode, buff, or corrode. The Alchemist's unique mechanic is the **Brew Cycle** — potions automatically rotate, and timing your throws to match the right potion is the skill expression.

**Tap Mechanic — Volatile Toss:**
The Alchemist cycles through 4 potion types every 5 seconds: **Healing** (green) -> **Explosive** (red) -> **Buffing** (gold) -> **Corrosive** (purple). Tapping throws the current potion:
- **Healing:** Heals the lowest-HP ally for 10% max HP
- **Explosive:** Deals 200% ATK fire damage in an AoE
- **Buffing:** Grants +15% ATK to the party for 5s
- **Corrosive:** Deals 100% ATK and reduces target's DEF by 20% for 8s

A colored indicator on the portrait shows the current potion. Skilled players time their taps to specific potion windows.

**Passive — Transmutation:**
5% of gold earned while the Alchemist is in the party converts to crafting materials. Additionally, the Alchemist generates 1 random crafting material every 30 seconds of combat.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Mega Brew** | Throws an enhanced version of the current potion at 3x effectiveness | 12s |
| **Experimental Flask** | Throws a random potion with a random wild effect (could be amazing or bizarre — examples: party turns invisible, enemies shrink, gravity reverses for 3s) | 18s |
| **Magnum Opus** (Ult) | Throws ALL four potion types simultaneously as one super-potion: heals all allies to full, deals 800% ATK to all enemies, buffs party +30% all stats for 10s, reduces all enemy DEF by 40% for 10s | Charge |

**Skill Tree Paths:**
- **Pyromancy:** Explosive potion damage, fire DoTs, combustion chains. *The damage alchemist.*
- **Restoration:** Healing potion power, HoTs, cleansing potions. *The healing alchemist.*
- **Mutation:** Experimental Flask always has beneficial effects, wild potion types, random massive buffs. *Chaotic but powerful.*

---

#### CHRONOMANCER
*Weaver of Temporal Threads*

| | |
|-|-|
| **Role** | Caster |
| **Affinity** | Frost |
| **Position** | Backline |
| **Difficulty** | Advanced |

**Identity:** A time-bending mage who controls the pace of battle itself. The Chronomancer speeds up allies, slows enemies, and can even rewind damage. She doesn't deal the highest raw damage, but her ability to manipulate time creates overwhelming tactical advantages.

**Tap Mechanic — Temporal Bolt:**
Each tap slows the target by 5% (stacks up to 50%). At max slow stacks, the enemy is **Frozen in Time** for 3 seconds (completely immobilized and unable to attack). After being Frozen in Time, the enemy is immune to further slowing for 5 seconds. This creates a rhythmic cycle: slow, slow, slow... FREEZE... wait... slow, slow...

**Passive — Time Dilation:**
All party ability cooldowns are reduced by 10% while the Chronomancer is alive. This is a powerful party-wide buff that makes every teammate's abilities more available.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Haste** | Doubles one ally's attack speed for 8s; if used on self, taps generate 2 slow stacks instead of 1 | 12s |
| **Rewind** | Reverts the last 3 seconds of damage on all allies (heals them for damage taken in that window); does not affect enemies | 20s |
| **Temporal Collapse** (Ult) | Freezes ALL enemies in time for 5s; during this window, all party members attack at 3x speed; when the freeze ends, all accumulated damage is applied at once in a devastating burst | Charge |

**Skill Tree Paths:**
- **Acceleration:** Ally speed buffs, Haste power, attack speed auras. *Speed up your team.*
- **Entropy:** Enemy slow/freeze effects, time damage (enemies take damage while slowed), temporal DoTs. *Control and damage.*
- **Paradox:** Rewind enhancements, temporal echo (repeat the last ability used by any ally), timeline manipulation. *Time manipulation mastery.*

---

#### GUNSLINGER
*Dual-Barrel Daredevil*

| | |
|-|-|
| **Role** | Ranger |
| **Affinity** | Flame |
| **Position** | Backline |
| **Difficulty** | Intermediate |

**Identity:** The fastest attacker in the game — a dual-pistol daredevil who fills the screen with bullets, ricochet shots, and explosive rounds. Where the Marksman is about precision and patience, the Gunslinger is about speed and spectacle. She has a unique **Ammo/Reload** mechanic that creates a satisfying shoot-shoot-shoot-RELOAD rhythm.

**Tap Mechanic — Fan the Hammer:**
Taps fire both pistols simultaneously at extreme speed (no charge time). Every 6 shots triggers a mandatory **Reload** — a 1-second pause where the Gunslinger can't attack. The first shot after Reload is a guaranteed critical hit that deals 3x damage. Visually spectacular: muzzle flashes, bullet casings flying, damage numbers everywhere.

**Passive — Quick Draw:**
The first attack against each new enemy deals 3x damage. In multi-enemy waves, this means the Gunslinger excels at cleaning up weakened enemies with opening shots.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Trick Shot** | Fires a ricochet bullet that bounces between all enemies (up to 8), dealing 100% ATK to each | 8s |
| **Explosive Round** | Loads an explosive bullet; next tap deals 300% ATK in an AoE and ignites enemies for 100% ATK fire damage over 5s | 14s |
| **Dead Eye** (Ult) | Time slows to a crawl for 6s; Gunslinger fires freely without Reload; every shot is a critical hit; tap as fast as possible for maximum damage | Charge |

**Skill Tree Paths:**
- **Desperado:** Crit damage, fire rate, Reload speed reduction. *Maximum DPS.*
- **Trickshot:** Ricochet bounces, split shots, AoE bullet patterns. *Multi-target specialist.*
- **Demolition:** Explosive rounds, fire damage, enemy ignite effects. *Flame-infused firepower.*

---

#### WARDEN
*Ironbark Sentinel*

| | |
|-|-|
| **Role** | Vanguard (Tank) |
| **Affinity** | Nature |
| **Position** | Frontline |
| **Difficulty** | Intermediate |

**Identity:** Nature's chosen guardian, clad in living bark and woven stone. The Warden tanks through regeneration and crowd control rather than raw damage reduction. She creates zones of difficult terrain that slow and root enemies, controlling the battlefield's tempo.

**Tap Mechanic — Earthen Slam:**
Taps create shockwaves that deal damage AND root the target for 1 second. Rooted enemies take 20% more damage from all sources. Against groups, each tap only roots one enemy, but rapidly tapping can stagger-root an entire wave. The screen shakes slightly with each slam for tactile satisfaction.

**Passive — Living Armor:**
Regenerates 3% max HP per second at all times. When standing in her own terrain effects (from abilities), regeneration doubles to 6%/second. This makes the Warden nearly unkillable in prolonged fights, especially on nature-affinity islands.

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Overgrowth** | Creates a zone of tangling vines for 10s; enemies in the zone are slowed 40% and take 30% ATK/s nature damage | 15s |
| **Ironbark** | Gains 50% damage reduction and reflects 20% of damage taken as nature damage for 8s | 18s |
| **Wrath of the Wild** (Ult) | Massive roots erupt from the ground, rooting ALL enemies for 4s and dealing 600% ATK; the Warden heals to full HP; Overgrowth zone is placed at max size for free | Charge |

**Skill Tree Paths:**
- **Ironbark:** Damage reduction, thorn damage, HP pool. *The immovable tree.*
- **Rootwalker:** Root duration, terrain effects, crowd control. *Battlefield controller.*
- **Overgrowth:** Party healing aura, nature's embrace (shield allies), regeneration sharing. *The nature healer-tank.*

---

#### REAPER
*Death's Executioner*

| | |
|-|-|
| **Role** | Striker |
| **Affinity** | Shadow |
| **Position** | Frontline |
| **Difficulty** | Advanced |

**Identity:** Death's own instrument — a scythe-wielding executioner who harvests souls from the fallen to fuel increasingly devastating attacks. The Reaper's defining mechanic is the **Execute Threshold** and **Soul Collection** — she deals bonus damage to low-HP enemies and grows stronger with every kill. In long fights with many enemies, the Reaper becomes an unstoppable snowball of death.

**Tap Mechanic — Harvest:**
Taps deal normal damage to healthy enemies. Against enemies below 30% HP, tap damage is **tripled**. Killing an enemy with a tap collects a **Soul** (max 20 Souls stored). Visual: defeated enemies' spectral essence flows into the Reaper's scythe, which glows brighter with each Soul collected.

**Passive — Soul Collection:**
Each stored Soul increases ALL damage by 2% and ability damage by 5%. At 20 Souls: +40% all damage, +100% ability damage. Souls are spent to activate abilities (instead of cooldowns), creating a resource management game — do you hoard Souls for passive power or spend them on devastating abilities?

**Abilities:**
| Ability | Effect | Cooldown |
|---------|--------|----------|
| **Soul Scythe** | Sweeping AoE strike dealing 250% ATK to all enemies; execute threshold raised to 50% for this hit | 5 Souls |
| **Death Mark** | Marks an enemy for death; if the marked enemy dies within 10s, all Souls are restored to max AND the Reaper heals 30% max HP | 3 Souls |
| **Reaping Hour** (Ult) | For 8s: execute threshold raised to 50%, ALL kills grant Souls beyond the cap (up to 40), attack speed doubled; at the end, excess Souls explode for 50% ATK each in an AoE | 15 Souls |

**Skill Tree Paths:**
- **Execution:** Execute threshold %, execute damage multiplier, critical strike vs. low-HP targets. *The finisher.*
- **Reaving:** AoE soul collection, Soul Scythe enhancements, multi-target harvesting. *The wave clearer.*
- **Undying:** Spend Souls for survivability (heal on Soul expenditure), death prevention, Soul-fueled shields. *The sustained duelist.*

---

## 6. Affinity System

Every hero belongs to one of six **Affinities** — elemental/thematic alignments that determine biome advantages, synergy bonuses, and visual identity.

### The Six Affinities

| Affinity | Theme | Color | Visual Motif |
|----------|-------|-------|-------------|
| **Flame** | Fire, heat, destruction | Red/Orange | Embers, smoke, heat shimmer |
| **Frost** | Ice, cold, precision | Blue/White | Crystals, snowflakes, mist |
| **Storm** | Lightning, wind, speed | Purple/Yellow | Sparks, gusts, crackling energy |
| **Nature** | Earth, beasts, growth | Green/Brown | Leaves, vines, stone, fur |
| **Shadow** | Darkness, death, stealth | Black/Violet | Smoke, wisps, spectral glow |
| **Radiance** | Light, holy, divine | Gold/White | Halos, sunbeams, celestial glow |

### Affinity Advantage Wheel

```
        Flame
       /     \
    beats    loses to
     /           \
  Nature ------- Storm
     \           /
    loses to  beats
       \     /
        Frost

   Shadow <---> Radiance (mutual opposition)
```

- **Flame > Nature > Storm > Frost > Flame** (standard ring)
- **Shadow vs. Radiance** — each deals +30% damage to the other (high-risk matchup)
- Advantage grants **+30% damage dealt** and **-15% damage taken** against the weaker affinity

### Class Affinity Assignments

| Affinity | Classes (4 per affinity) |
|----------|------------------------|
| **Flame** | Berserker, Warlock, Gunslinger, Alchemist |
| **Frost** | Mage, Marksman, Defender, Chronomancer |
| **Storm** | Shaman, Ranger, Spell-Blade, Monk |
| **Nature** | Druid, Dragon-Hunter, Summoner, Warden |
| **Shadow** | Rogue, Necromancer, Thief, Reaper |
| **Radiance** | Warrior, Priest, Paladin, Bard |

### Biome Affinity Interaction

Each Sky Island has an affinity. Fighting on an island that matches your hero's affinity grants:
- **Home Affinity:** +25% damage dealt, -10% damage taken
- **Advantage Affinity:** +50% damage dealt, -20% damage taken
- **Disadvantage Affinity:** -20% damage dealt, +10% damage taken
- **Neutral:** No modifier

This makes party composition island-dependent — players swap heroes to exploit biome advantages.

---

## 7. 4-Hero Party System

### Formation

The party consists of 4 heroes arranged in a **2x2 grid**:

```
 [Front-Left]  [Front-Right]
 [Back-Left]   [Back-Right]
```

**Frontline (Row 1):** Heroes here have +20% DEF but are targeted first by enemies. Vanguard classes belong here.

**Backline (Row 2):** Heroes here deal +10% damage but are vulnerable to AoE and piercing attacks. Casters, Rangers, and Supports belong here.

**Adjacency:** Heroes in horizontally or vertically adjacent slots can trigger certain synergy effects and are affected by each other's aura abilities.

### Party Rules

- Minimum 1 hero, maximum 4
- Any combination of classes is allowed (no forced roles), but composition affects efficiency
- Heroes can be swapped in/out between stages (not during combat)
- Each hero slot progresses independently — Slot 1 might have a hero on their 3rd ascension while Slot 4 has a fresh recruit

### Synergy System

#### Affinity Synergies

Running multiple heroes of the same affinity grants scaling bonuses:

| Count | Bonus |
|-------|-------|
| 2 matching | +10% damage of that element |
| 3 matching | +10% damage + 15% defense on matching biome |
| 4 matching (full party) | +25% damage + 20% defense + unique **Resonance Ability** |

**Resonance Abilities (4-of-a-Kind):**
| Affinity | Resonance Ability |
|----------|------------------|
| Flame | **Inferno** — party attacks ignite all enemies, dealing 5% max HP/s burn |
| Frost | **Absolute Cold** — all enemies are permanently slowed 20%; frozen enemies shatter for AoE |
| Storm | **Tempest** — party attack speed +30%; lightning strikes random enemies every 2s |
| Nature | **Overgrowth** — party regenerates 5% max HP/s; terrain constantly spawns entangling roots |
| Shadow | **Void Shroud** — party dodges 25% of attacks; killed enemies have 20% chance to rise as minions |
| Radiance | **Divine Chorus** — party heals 3% max HP/s; enemies deal 15% less damage; party immune to debuffs |

#### Role Synergies

| Combination | Bonus |
|-------------|-------|
| At least 1 Vanguard + 1 Support | +10% party survivability (damage reduction) |
| At least 1 Striker/Caster + 1 Ranger | +10% party damage |
| 2 different Supports | +20% all healing done |
| 2 different Vanguards | +15% party defense |
| All 4 roles different | +5% all stats (the "balanced team" bonus) |

#### Combo Abilities

Specific hero **pairs** unlock unique **Combo Attacks** — powerful joint abilities that activate when both heroes in the pair use their abilities within 3 seconds of each other.

| Pair | Combo Name | Effect |
|------|-----------|--------|
| Warrior + Mage | **Arcane Cleave** | Enchanted blade wave dealing physical + magic damage to all enemies |
| Priest + Necromancer | **Soul Judgment** | Massive holy/dark explosion; heals party for damage dealt |
| Rogue + Marksman | **Pinpoint Ambush** | Guaranteed critical strike from both, +100% crit damage |
| Berserker + Druid | **Wild Rampage** | Berserker shapeshifts into a flame bear for 8s, dealing massive AoE |
| Defender + Paladin | **Unbreakable Wall** | Both gain immunity for 4s; all enemy aggro locked to them |
| Spell-Blade + Bard | **Harmonic Edge** | Every melee strike plays a damaging chord; every chord enhances the next strike |
| Monk + Reaper | **Final Judgment** | 10-hit combo where each strike executes enemies below escalating thresholds (5%, 10%, 15%...) |
| Shaman + Summoner | **Primal Call** | All totems and familiars merge into a massive elemental titan for 10s |
| Warlock + Chronomancer | **Temporal Curse** | Curse is applied to all enemies AND is rewound to apply again 3s later (double curse) |
| Dragon-Hunter + Ranger | **Apex Predator** | Both heroes and all pets focus one target for 500% combined damage per second for 5s |
| Thief + Gunslinger | **Heist** | Enemies drop 10x gold; Gunslinger fires celebratory rounds that deal bonus damage |
| Mage + Alchemist | **Volatile Arcana** | Spells become potions and potions become spells — both gain each other's strongest ability for 10s |
| Warden + Necromancer | **Cycle of Life and Death** | Roots grow from corpses; each root heals allies and spawns a nature minion |

*30+ more combos are discoverable through experimentation. Players see a "?" indicator when an undiscovered combo pair is in their party.*

### Recommended Starter Compositions

| Composition | Strategy |
|-------------|----------|
| Warrior + Mage + Priest + Rogue | **Classic Balanced** — Covers all roles, easy to learn. |
| Defender + Berserker + Priest + Marksman | **Fortress** — Tank-and-spank with heavy single-target DPS. |
| Paladin + Warlock + Rogue + Bard | **Dark Harmony** — Paladin tanks and off-heals, Bard buffs, Warlock and Rogue burst. |
| Thief + Gunslinger + Alchemist + Druid | **Gold Rush** — Maximum economy; Thief's Lucky Break + Alchemist's Transmutation = rich. |

---

## 8. Ascension Prestige System

### Overview

Ascension is the core prestige loop. Heroes push through Sky Islands, eventually reaching a point where progress stalls. At that point, the hero can **Ascend** — resetting their level and island progress in exchange for permanent power upgrades and progress toward Demigod status.

### The Ascension Cycle

```
Start at Island 1, Level 1
       |
Push through islands, level up, gear up
       |
Hit progression wall (can't beat current boss)
       |
Choose to ASCEND
       |
  Hero resets to Level 1, Island 1
  + Earns Ascension Shards based on highest island reached
  + Permanent Ascension Tier upgrade
  + Retains: Gear, Class Mastery, Pantheon progress
       |
Blast through previously difficult islands at 2-3x speed
       |
Push further than before --> New wall --> Ascend again
```

### Ascension Tiers

| Tier | Ascensions Required | Base Stat Bonus | Unlock |
|------|-------------------|----------------|--------|
| **Mortal** | 0 (default) | -- | Starting tier |
| **Awakened** | 1st Ascension | +15% all base stats | Ascension Skill Tree |
| **Exalted** | 3rd Ascension | +35% all base stats | Advanced skill tree branches |
| **Mythic** | 5th Ascension | +60% all base stats | Mythic gear evolution |
| **Demigod** | 10th Ascension | Hero retires permanently | Global passive buff |

### Ascension Shards

The primary prestige currency. Earned based on highest island reached before ascending:

| Highest Island | Shards Earned |
|---------------|---------------|
| Island 3 | 10 |
| Island 6 | 30 |
| Island 9 | 60 |
| Island 12 (Realm Boss) | 120 |
| Island 15+ (Realm 2) | 200+ (scaling) |

**Spent on:**
- **Ascension Skill Tree** — Permanent upgrades (see below)
- **Pantheon upgrades** — Enhancing Demigod buffs
- **Class Mastery** milestones — Permanent class-specific bonuses

### Ascension Skill Tree

A permanent upgrade tree that persists across ALL ascensions. Spending Ascension Shards here makes every subsequent run faster and more powerful.

**Branches:**
| Branch | Focus | Example Nodes |
|--------|-------|--------------|
| **Power** | Damage and combat stats | +5% All Damage, +10% Crit Rate, +10% Ability Damage |
| **Fortitude** | Survivability | +10% Max HP, +5% Damage Reduction, +3% HP Regen |
| **Prosperity** | Economy and drops | +15% Gold, +10% Drop Rate, +5% XP Gain |
| **Swiftness** | Speed and QoL | +10% Stage Clear Speed, Auto-Collect Idle Rewards, Quick Ascension option |

### Demigod Retirement

After 10 Ascensions, a hero can achieve **Demigod** status:

1. The hero fights one final **Transcendence Trial** — a gauntlet of increasingly difficult enemies themed to their affinity
2. Upon completion, a cinematic plays: the hero ascends beyond the sky islands into a celestial realm
3. The hero is **permanently retired** from the active party
4. The hero becomes a **Demigod Patron** in the Pantheon, granting a **Global Passive Buff** to ALL heroes forever

**Demigod Buffs by Class:**

| Class | Demigod Buff |
|-------|-------------|
| Warrior | +5% All Physical Damage |
| Mage | +5% All Magical Damage |
| Priest | +5% All Healing |
| Rogue | +3% Critical Strike Chance |
| Marksman | +5% Damage to Bosses |
| Defender | +5% All Damage Reduction |
| Berserker | +3% Attack Speed |
| Druid | +3% HP Regeneration |
| Thief | +10% Gold Income |
| Shaman | +3% Ability Cooldown Reduction |
| Warlock | +5% DoT Damage |
| Ranger | +5% Pet/Companion Damage |
| Spell-Blade | +3% Hybrid (Phys+Magic) Damage |
| Necromancer | +5% Summon Damage |
| Monk | +3% Dodge Chance |
| Paladin | +3% Healing Done as Damage |
| Bard | +3% Party Buff Effectiveness |
| Dragon-Hunter | +8% Boss Damage |
| Summoner | +5% Familiar/Summon Stats |
| Alchemist | +5% Potion/Consumable Effectiveness |
| Chronomancer | +3% Party Ability Cooldown Reduction |
| Gunslinger | +5% Ranged Attack Damage |
| Warden | +3% Max HP |
| Reaper | +5% Damage to Low-HP Enemies |

### The Pantheon

The Pantheon is a celestial grid — 24 slots, one per class. As heroes achieve Demigod status and fill Pantheon slots, **milestone bonuses** unlock:

| Pantheon Slots Filled | Milestone Bonus |
|----------------------|----------------|
| 6 (one per affinity) | **Harmony of Elements:** +10% damage on all affinity-advantaged islands |
| 12 (half roster) | **Ascendant's Blessing:** AFK rewards doubled |
| 18 (three quarters) | **Mythic Awakening:** All heroes start at Awakened tier instead of Mortal |
| 24 (complete) | **Eternal Pantheon:** Unlock Realm 4 (Infinite Ascension), permanent +25% all stats |

**Filling an entire Affinity (all 4 classes of one element) grants an additional Affinity Milestone:**
- All Flame Demigods: +15% fire damage globally
- All Frost Demigods: +15% ice damage globally
- All Storm Demigods: +15% lightning damage globally
- All Nature Demigods: +15% nature damage globally
- All Shadow Demigods: +15% shadow damage globally
- All Radiance Demigods: +15% holy damage globally

### Multi-Tier Party Play

The 4 hero slots progress independently. At any time, a player might have:
- **Slot 1:** Warrior at Mythic tier (8th ascension) — pushing through Realm 2
- **Slot 2:** Necromancer at Exalted tier (4th ascension) — mid Realm 1
- **Slot 3:** Bard at Awakened tier (1st ascension) — early Realm 1
- **Slot 4:** Gunslinger at Mortal tier (0 ascensions) — just unlocked and leveling

This creates interesting dynamics:
- Powerful heroes carry newer ones through content
- Players stagger ascensions to always have at least one strong hero
- Fresh heroes benefit from Pantheon buffs accumulated by retired Demigods

---

## 9. Sky Islands & Biomes

### World Structure

The sky stretches infinitely upward. Islands float at ascending altitudes, organized into **Realms**. Each Realm contains **12 islands**, and each island has **100 stages**.

```
REALM 3 — THE CELESTIAL HEIGHTS (Islands 25-36)
  |
REALM 2 — THE MIDDLE SKY (Islands 13-24)
  |
REALM 1 — THE LOWER SKY (Islands 1-12)
  |
THE SURFACE (Tutorial / Starting Area)
```

### Realm 1 — The Lower Sky

| # | Island Name | Affinity | Description | Environmental Effect |
|---|------------|----------|-------------|---------------------|
| 1 | **Verdant Shelf** | Nature | Lush floating grasslands with cascading waterfalls. Tutorial island. | Healing herbs spawn, granting +5% HP regen |
| 2 | **Ember Plateau** | Flame | Volcanic mesa with rivers of lava and ash clouds. | Fire damage over time (1% HP/5s) to non-Flame heroes |
| 3 | **Stormspire Reach** | Storm | Lightning-struck peaks with perpetual thunderstorms. | Random lightning strikes deal AoE damage to enemies (and occasionally allies) |
| 4 | **Frosthollow Drift** | Frost | Frozen cavern network inside a massive ice island. | Healing reduced by 30% for all units; Frost heroes immune |
| 5 | **Duskwood Crossing** | Shadow | Haunted dead forest shrouded in perpetual twilight. | Visibility reduced; enemies ambush more frequently; Shadow heroes see through the darkness |
| 6 | **Sunstone Bastion** | Radiance | Golden temple complex bathed in eternal sunrise. | All healing increased by 20%; Radiance heroes gain shields |
| 7 | **Wildthorn Expanse** | Nature | Overgrown jungle island with enormous flora. | Vines periodically root random heroes for 2s; Nature heroes immune |
| 8 | **Cinderfall Heights** | Flame | Magma rivers flowing between floating obsidian pillars. | Enemies gain fire shields; Flame heroes bypass them |
| 9 | **Galebreak Summit** | Storm | Wind-torn cliffs above the cloudline. | All attack speed increased by 15%; movement randomized by gusts |
| 10 | **Crystalveil Tundra** | Frost | Crystalline ice fields that refract light into rainbows. | Enemies have reflective ice armor (10% damage reflected); Frost heroes bypass |
| 11 | **Abyssal Hollow** | Shadow | Deep caverns descending into a floating island's core. | Darkness debuff: -20% accuracy for non-Shadow heroes |
| 12 | **Zenith Spire** | Radiance | The culmination of Realm 1: a radiant citadel in the clouds. | Radiance heroes deal +30% damage. The Realm Boss awaits. |

### Realm 2 — The Middle Sky

Islands 13-24 follow a similar 12-island structure with escalated themes:

| # | Island Name | Affinity | Description |
|---|------------|----------|-------------|
| 13 | **Ashfall Wastes** | Flame | A ruined civilization consumed by volcanic eruption |
| 14 | **Glacial Throne** | Frost | A palace of living ice where the frozen queen rules |
| 15 | **Thundervault** | Storm | A floating weapons forge powered by captured lightning |
| 16 | **Titan's Garden** | Nature | An island overgrown with sentient, building-sized plants |
| 17 | **Necropolis** | Shadow | A city of the dead where shadows have physical form |
| 18 | **Solarium** | Radiance | A mirror array that focuses starlight into searing beams |
| 19 | **Primal Caldera** | Flame | The heart of a living volcano that breathes |
| 20 | **Permafrost Depths** | Frost | Tunnels through an iceberg island where time moves slowly |
| 21 | **Eye of the Tempest** | Storm | The calm center of an eternal storm, surrounded by chaos |
| 22 | **World Tree Root** | Nature | A massive root system from a tree that spans multiple islands |
| 23 | **Void Breach** | Shadow | A crack in reality where shadow bleeds into the world |
| 24 | **Empyrean Gates** | Radiance | The gateway to the celestial realm — Realm 2's final challenge |

### Realm 3 — The Celestial Heights

Islands 25-36 are endgame content with dramatically increased difficulty and rewards. Environments become surreal and otherworldly — floating geometric shapes, inverted gravity, reality distortions. Each island combines TWO affinities, requiring versatile party compositions.

### Stage Structure (Per Island)

Each island has 100 stages:

| Stage Range | Content |
|------------|---------|
| 1-9 | Normal enemy waves (3-5 enemies per wave) |
| 10 | **Mini-Boss** — unique enemy with 1 special mechanic |
| 11-19 | Normal waves with increased difficulty |
| 20 | **Mini-Boss** (different from stage 10) |
| ... | Pattern continues |
| 90 | **Mini-Boss** (9th, toughest mini-boss) |
| 91-99 | Elite waves (stronger enemies, more abilities) |
| 100 | **Island Boss** — Multi-phase fight, requires active play |

---

## 10. Progression Systems

### Hero Leveling

| Aspect | Details |
|--------|---------|
| XP Source | Combat kills (active & idle), quest rewards, consumable XP items |
| Level Cap | 100 per Realm (Realm 1 = Lv100, Realm 2 = Lv200, Realm 3 = Lv300) |
| Stat Growth | Each level grants +ATK, +DEF, +HP, +SPD based on class growth rates |
| Milestone Levels | Levels 10, 25, 50, 75, 100 unlock new abilities or ability upgrades |

**Class Stat Growth Rates (Per Level):**

| Class | ATK | DEF | HP | SPD |
|-------|-----|-----|----|-----|
| Warrior | ++ | ++ | ++ | + |
| Mage | +++ | + | + | ++ |
| Priest | + | ++ | ++ | ++ |
| Rogue | +++ | + | + | +++ |
| Berserker | +++ | + | +++ | ++ |
| Defender | + | +++ | +++ | + |
| (etc.) | Varies per class identity | | | |

*+++ = high growth, ++ = medium growth, + = low growth*

### Skill Trees

Each class has **3 branching skill paths** (detailed per class above). Key design rules:
- Players earn **Skill Points** from leveling (1 per level) and completing island challenges (bonus)
- Total Skill Points available allow investing deeply in one path or spreading across two
- Fully completing one path unlocks a **Capstone Ability** — a powerful passive unique to that path
- Respec cost: Gold (affordable in early game, significant in late game). **Respec is free on Ascension.**
- Skill tree progress is **reset on Ascension** (like levels), but Ascension Tier unlocks keep advanced nodes available

### Equipment System

**6 Equipment Slots per Hero:**
| Slot | Type | Primary Stat |
|------|------|-------------|
| Weapon | Class-specific | ATK |
| Armor | Heavy/Light/Robe | DEF |
| Helm | Various | HP |
| Accessory | Ring/Amulet/Charm | Special stat (crit, speed, etc.) |
| Relic | Ancient artifact | Unique passive effect |
| Mount | Rideable companion | Movement speed + stat bonuses |

**Rarity Tiers:**
| Rarity | Color | Drop Rate | Stat Multiplier |
|--------|-------|-----------|----------------|
| Common | White | 60% | 1.0x |
| Uncommon | Green | 25% | 1.3x |
| Rare | Blue | 10% | 1.7x |
| Epic | Purple | 4% | 2.5x |
| Legendary | Orange | 0.9% | 4.0x |
| Mythic | Red | 0.1% (crafted) | 6.0x |

**Equipment Features:**
- **Set Bonuses:** Equipping 2/4/6 pieces of the same set grants escalating bonuses. Sets are themed by affinity and playstyle.
- **Enhancement:** Spend gold + materials to upgrade equipment (+1 through +15). Each enhancement grants flat stat increases.
- **Evolution:** Legendary items can be evolved to Mythic using rare materials (Aether Crystals + class-specific components). Mythic items have unique visual effects.
- **Persistence:** Equipment is **NOT reset on Ascension**. This is a critical design choice — gear is the primary persistent progression that makes each Ascension feel worthwhile. A hero ascending for the 5th time with full Legendary gear will demolish early islands.

### Class Mastery

A per-class progression track that advances by playing that class:

| Mastery Level | Requirement | Reward |
|--------------|-------------|--------|
| Novice | First hero of this class | Class unlocked |
| Apprentice | 100 stages cleared | +5% class-specific stat |
| Journeyman | 1,000 stages + 1 Ascension | Alternate ability variant unlock |
| Expert | 5,000 stages + 3 Ascensions | Class skin unlock + 10% class stat |
| Master | 10,000 stages + 5 Ascensions | Mastery passive ability |
| Grandmaster | All islands cleared + Demigod | Ultimate class cosmetic + title |

Class Mastery is **permanent** — it never resets. It represents the player's lifetime commitment to a class.

---

## 11. Boss Encounters

### Boss Tiers

| Type | Frequency | Mechanics | Active Play Required? |
|------|-----------|-----------|----------------------|
| **Mini-Boss** | Every 10 stages | 1 special mechanic | Recommended |
| **Island Boss** | Stage 100 of each island | Multi-phase, 2-3 mechanics | Required |
| **Realm Boss** | Island 12/24/36 | Cinematic, 3-4 phases, party-wide mechanics | Required |
| **World Boss** | Weekly event | Global HP pool, all players contribute | Optional (event) |

### Mini-Boss Mechanics Pool

Mini-bosses draw from a pool of mechanics, gaining more as islands increase in difficulty:

| Mechanic | Description |
|----------|-------------|
| **Enrage Timer** | Boss gains +10% ATK every 10s; must be killed quickly |
| **Shield Phase** | Boss becomes immune until shield is broken (weak to specific affinity) |
| **Add Spawning** | Boss summons minions that must be killed or they heal the boss |
| **Ground Slam** | AoE attack with a visual telegraph; tap to dodge (moves party) |
| **Life Steal** | Boss heals from attacks; must out-DPS the healing |
| **Split** | Boss splits into 2 weaker copies when reaching 50% HP |
| **Reflect** | Boss reflects damage for 3s; stop attacking during reflect |

### Island Boss Design

Island Bosses are multi-phase encounters that require active play:

**Example — Emberlord Kael (Island 2: Ember Plateau Boss)**

*Phase 1 (100-60% HP):* Basic melee attacks + periodic fire breath (dodge by tapping a "move" prompt). Enrages after 60 seconds.

*Phase 2 (60-30% HP):* Summons fire elementals. Players must kill the adds before they reach Kael and heal him. Kael's fire breath now leaves burning ground (standing in it deals continuous damage).

*Phase 3 (30-0% HP):* Kael absorbs all remaining elementals and grows massive. Attack speed doubles. Every 10 seconds, a massive fireball targets the hero with highest ATK — player must tap the fireball 10 times to deflect it.

### Realm Boss Design

Realm Bosses are cinematic events:

**The Radiant Guardian (Realm 1 Final Boss — Zenith Spire)**

A towering angelic construct that guards the boundary between Realms.

*Phase 1 — Judgment:* Tests each hero individually. One by one, each party member faces the Guardian alone for 15 seconds. Surviving all four tests unlocks Phase 2.

*Phase 2 — Wrath:* Full party fight. The Guardian attacks with all six affinities in rotation, requiring the party to exploit advantages and endure disadvantages.

*Phase 3 — Transcendence:* The Guardian merges with the Zenith Spire itself. The environment becomes the boss — columns of light, crumbling platforms, and energy beams. Players must tap specific glowing weak points while dodging environmental hazards.

*Phase 4 — Ascension:* At 10% HP, the Guardian offers a choice: mercy or destruction. Mercy grants a unique Radiance Relic. Destruction grants a unique Shadow Relic. (Both are Legendary quality with different effects.)

### World Boss (Weekly Event)

A massive creature threatens the sky islands. All players on the server contribute damage across the event window (48 hours):

- Each player can attempt the World Boss 3 times per day (energy-gated)
- Damage dealt is recorded on a leaderboard
- The boss has a global HP pool (e.g., 100 billion HP)
- When the boss is defeated, ALL contributors receive tier-based rewards (Top 1%, 5%, 10%, 25%, All)
- If the boss survives the 48-hour window, contributors still get partial rewards

---

## 12. Events & Endgame

### Recurring Events

| Event | Frequency | Duration | Description |
|-------|-----------|----------|-------------|
| **Tower of Trials** | Weekly | 7 days | Roguelike tower climb: 50 floors with random buffs/debuffs per floor. Unique rewards per floor milestone. Separate leaderboard. |
| **Void Rifts** | Bi-weekly | 3 days | Time-limited challenge dungeons with modifiers (e.g., "All enemies have double HP," "Abilities cost double"). Endgame-tuned difficulty. |
| **Arena Season** | Monthly | 30 days | Asynchronous PvP: build a defense team, attack other players' defense teams. Bracket-based matchmaking. Season rewards at month's end. |
| **World Boss** | Weekly | 48 hours | Server-wide cooperative boss fight (described above). |
| **Guild Expedition** | Bi-weekly | 5 days | Guild members cooperatively clear a shared map with branching paths. Each member clears their assigned paths. Pooled rewards. |

### Seasonal Events (Every 6 Weeks)

Themed content updates with exclusive rewards:

| Season | Theme | Exclusive Content |
|--------|-------|------------------|
| **Frost Festival** | Winter/Ice | Frost-themed island, ice cosmetics, Frozen Boss variant |
| **Ember Solstice** | Summer/Fire | Volcanic challenges, fire weapon skins, heat-wave modifiers |
| **Shadow Tide** | Halloween/Dark | Haunted island, undead enemy reskins, Shadow Relic quest |
| **Radiant Dawn** | Spring/Light | Cherry blossom island, healing-focused challenges, Radiance cosmetics |
| **Storm's Eye** | Anniversary | All-element gauntlet, guaranteed Legendary summon, retrospective rewards |
| **Nature's Call** | Harvest/Earth | Gathering-focused event, crafting bonuses, pet/companion skins |

### Endgame Systems

**For players who've completed all three Realms:**

**1. Infinite Ascension (Realm 4+)**
- Procedurally generated islands with increasing difficulty
- Each island combines random affinity + random environmental effects
- Leaderboard tracks highest island reached
- Exclusive cosmetic rewards at milestones (Island 50, 100, 200, etc.)

**2. Pantheon Completion**
- Ascending all 24 classes to Demigod status — estimated 6-12 months of play
- Completing the Pantheon is the game's ultimate achievement
- Unlocks "Eternal Mode" — heroes can be resummoned from the Pantheon as "Eternal" versions with all Demigod buffs built-in

**3. Mythic Gear Crafting**
- Collecting rare materials from Realm 3 and events to craft Mythic-tier equipment
- Each Mythic item has a unique visual and a powerful special effect
- Full Mythic sets per class unlock "Set Mastery" abilities

**4. Leaderboards**
| Leaderboard | Tracks |
|-------------|--------|
| Highest Island | Furthest island reached in Infinite Ascension |
| Speed Ascension | Fastest time to complete a full Ascension cycle |
| Boss Damage | Highest single-attempt damage on World Boss |
| Pantheon Race | First to complete full Pantheon |
| Arena Rank | PvP standing |

**5. Guild Systems**
- Guilds of up to 30 members
- Guild Tech Tree: members contribute resources to unlock guild-wide bonuses (e.g., +5% Gold, +5% XP)
- Guild Raids: cooperative multi-boss dungeons requiring coordinated party compositions
- Guild Wars: weekly inter-guild tournaments

---

## 13. Economy & Currencies

### Currency Overview

| Currency | Icon | Earn Rate | Primary Sources | Primary Uses |
|----------|------|-----------|----------------|-------------|
| **Gold** | Coin | Very Fast | Combat, AFK, quests | Hero leveling, gear enhancement, respec, basic shop |
| **Stardust** | Star | Slow | Achievements, events, daily rewards, IAP | Astral Summon, cosmetics, convenience items |
| **Ascension Shards** | Crystal | On Ascension | Ascending heroes | Ascension Skill Tree, Pantheon upgrades |
| **Aether Crystals** | Prism | Very Slow | Realm 2+ content, events | Mythic gear crafting, high-tier upgrades |
| **Class Tokens** | Emblem | Per-class | Playing that class, expeditions | Class Mastery, class skins, class-specific upgrades |
| **Guild Coins** | Shield | From guild activities | Guild Expeditions, Guild Raids | Guild shop, Guild Tech Tree contributions |

### Economy Balance Philosophy

- **Gold** should flow freely. Players should never feel gold-starved for basic leveling. Gold is the "feels good" currency.
- **Stardust** is the premium pacing lever. F2P players earn enough for ~2 Astral Summons per week; paying players accelerate this.
- **Ascension Shards** are earned entirely through gameplay. They should never be directly purchasable for cash (only accelerated via XP boosters that lead to faster ascensions).
- **Aether Crystals** are endgame prestige. They represent investment in the game's deepest systems and should be exciting to earn.

### Astral Summon (Gacha)

The hero acquisition system:

| Summon Type | Cost | Pool |
|-------------|------|------|
| Single Pull | 300 Stardust or 1 Summon Scroll | 1 hero at random rarity |
| 10-Pull | 2700 Stardust or 10 Summon Scrolls | 10 heroes, guaranteed 1 Rare+ |
| Premium Pull | 500 Stardust | Higher Legendary rate (3x) |

**Rarity Rates:**
| Rarity | Rate | Result |
|--------|------|--------|
| Uncommon | 60% | Hero at Uncommon tier (usable but needs upgrading) |
| Rare | 30% | Hero at Rare tier |
| Epic | 8% | Hero at Epic tier |
| Legendary | 2% | Hero at Legendary tier (highly powerful) |

**Player-Friendly Systems:**
- **Pity System:** Guaranteed Epic at 30 pulls, Guaranteed Legendary at 90 pulls. Pity counter carries across banners.
- **Wishlist:** Choose up to 5 classes to have boosted pull rates (2x rate for wishlisted classes).
- **Duplicate Protection:** Pulling a hero you already own grants **Star Fragments** for that class. Star Fragments are used to **Star-Up** (increase star rating) the existing hero, boosting base stats.
- **Spark System:** After 200 total pulls on a banner, choose ANY hero from the banner for free.

### Star System (Hero Rarity Progression)

Heroes range from 1-Star to 7-Star. Higher stars = higher base stats.

| Star Rating | How to Obtain |
|-------------|--------------|
| 1-Star | Common drops, tutorial rewards |
| 2-Star | Uncommon summon result |
| 3-Star | Rare summon result |
| 4-Star | Epic summon result, or Star-Up a 3-Star |
| 5-Star | Legendary summon result, or Star-Up a 4-Star |
| 6-Star | Star-Up a 5-Star (requires class-specific materials) |
| 7-Star | Star-Up a 6-Star (requires Aether Crystals) |

**Star-Up Cost:**
- 3-Star to 4-Star: 20 Star Fragments
- 4-Star to 5-Star: 50 Star Fragments + Rare materials
- 5-Star to 6-Star: 100 Star Fragments + Epic materials
- 6-Star to 7-Star: 200 Star Fragments + Aether Crystals

---

## 14. Monetization

### Philosophy

**Pay to accelerate, never pay to gate.** Every piece of content in the game is accessible to free players. Spending money makes the journey faster and more convenient, but never locks out experiences or creates insurmountable PvP advantages.

### Revenue Streams

#### 1. Stardust Packages (IAP)

| Package | Price | Contents |
|---------|-------|----------|
| Handful of Stars | $0.99 | 100 Stardust |
| Star Pouch | $4.99 | 550 Stardust (+10% bonus) |
| Star Chest | $9.99 | 1200 Stardust (+20% bonus) |
| Constellation | $24.99 | 3200 Stardust (+28% bonus) |
| Galaxy | $49.99 | 7000 Stardust (+40% bonus) |
| Supernova | $99.99 | 15000 Stardust (+50% bonus) |

#### 2. Monthly Subscription — Patron's Blessing

**$4.99/month:**
- Double AFK Vault capacity (20 hours instead of 10)
- 50 Stardust daily login bonus (1500/month value)
- Exclusive "Patron" avatar frame
- +1 Expedition slot (4 total)
- Instant AFK Vault collection (no cap warning)

#### 3. Battle Pass — Ascendant Pass

**Free Track + Premium Track ($9.99/season, ~6 weeks):**
- 50 tiers of rewards, earning Pass XP from daily/weekly quests
- Free track: Gold, materials, Summon Scrolls, Class Tokens
- Premium track: Stardust, exclusive skins, guaranteed hero summon at Tier 50, unique Relic at Tier 50, cosmetic mount at Tier 30

#### 4. Starter Packs (One-Time)

| Pack | Price | Contents |
|------|-------|----------|
| Adventurer's Kit | $2.99 | 300 Stardust + 5 Summon Scrolls + 500K Gold |
| Hero's Arsenal | $9.99 | 1000 Stardust + 10 Summon Scrolls + Rare Equipment Set + XP Boost (24h) |
| Champion's Legacy | $19.99 | 3000 Stardust + 20 Summon Scrolls + Epic Equipment Set + Gold Boost (7d) + Class Token Chest |
| Ascendant's Birthright | $49.99 | 8000 Stardust + 50 Summon Scrolls + Legendary Weapon + 30-Day Patron's Blessing + Exclusive Skin |

#### 5. Cosmetic Shop

- **Hero Skins:** 500-1500 Stardust each. Visual-only (no stat changes). Alternate fantasy outfits, seasonal themes, crossover designs.
- **Tap Effects:** 300 Stardust. Change the visual effect of tapping (fire sparks, ice shards, musical notes, etc.).
- **Island Themes:** 800 Stardust. Reskin your home island's visual aesthetic.
- **UI Themes:** 500 Stardust. Dark mode, nature theme, cosmic theme, etc.
- **Avatar Frames & Titles:** 200-500 Stardust. Cosmetic profile customization.

### F2P Balance Targets

| Metric | F2P | Low Spender ($5/mo) | Mid Spender ($20/mo) | Whale ($100+/mo) |
|--------|-----|---------------------|---------------------|-------------------|
| Summons/Week | ~2 | ~4 | ~8 | ~20+ |
| Progression Speed | 1x | 1.3x | 1.6x | 2x |
| Content Access | 100% | 100% | 100% | 100% |
| Cosmetics | Free rewards only | Some premium | Most premium | All premium |
| Arena Competitiveness | Top 25% achievable | Top 10% | Top 5% | Top 1% |
| Pantheon Completion | ~12 months | ~9 months | ~7 months | ~4 months |

### Ethical Monetization Rules

1. **No lootboxes with real-money-only items.** Everything in paid boxes can eventually be earned free.
2. **Pity system on ALL gacha.** No player should ever feel like they pulled 100 times for nothing.
3. **No pay-to-win PvP.** Arena uses a normalized stat system where gear/level advantages are compressed (whales have ~10% advantage, not 200%).
4. **No energy system for core gameplay.** Players can always play the main campaign. Energy only gates optional bonus content (World Boss attempts, Void Rifts).
5. **Transparent rates.** All summon rates displayed in-game, compliant with App Store requirements.

---

## 15. UI/UX Design

### Layout Philosophy

**One hand, one thumb, one screen at a time.** Every critical interaction must be reachable by a right thumb in portrait orientation. The UI should feel native to iOS — clean, responsive, and intuitive.

### Main Screen Layout

```
+----------------------------------+
|  [Dynamic Island / Status Bar]   |
|                                  |
|                                  |
|        COMBAT VIEWPORT           |
|     (heroes, enemies, FX)        |
|                                  |
|    [AFK Vault Indicator]         |
|                                  |
|----------------------------------|
|  [Hero 1] [Hero 2] [Hero 3] [Hero 4] |  <- Tap portraits for abilities
|  [HP bar] [HP bar] [HP bar] [HP bar] |
|----------------------------------|
| [Battle] [Heroes] [Islands] [Summon] [More] |  <- Bottom Tab Bar
+----------------------------------+
```

### Screen Breakdown

**Battle Screen (Home):**
- Top 60%: Combat viewport with heroes (left) vs. enemies (right)
- Tap anywhere in viewport to deal Tap Damage
- Damage numbers float up with size proportional to damage dealt
- Momentum meter along the left edge (vertical bar that fills up)
- Stage counter top-left: "Island 3 — Stage 47/100"
- AFK Vault: floating chest icon top-right; glows when rewards are waiting; tap to collect
- Hero portraits bottom: 4 circular portraits with HP bars beneath. Tap for Ability 1, double-tap for Ability 2, hold for Ultimate

**Heroes Screen:**
- Scrollable list of all owned heroes (sorted by power, level, or affinity)
- Tap a hero to open their detail panel:
  - Stats (ATK, DEF, HP, SPD, Crit, etc.)
  - Equipment slots (tap to equip/swap)
  - Skill Tree (3-branch visual tree, tap nodes to invest)
  - Class Mastery progress bar
  - Ascension Tier indicator

**Islands Screen:**
- Vertical scrolling world map showing sky islands
- Current position highlighted with a pulsing beacon
- Completed islands have a checkmark; locked islands are dimmed
- Tap an island to see: biome info, affinity, boss details, completion rewards
- Expedition panel: swipe left to access expedition deployment

**Summon Screen:**
- Astral Summon portal with glowing animation
- Single Pull and 10-Pull buttons prominently displayed
- Pity counter visible: "Next guaranteed Epic in 12 pulls"
- Wishlist management accessible via a star icon
- Banner rotation: featured/limited heroes on special banners

**More Menu:**
- Shop
- Guild
- Events (Tower, Void Rift, Arena, World Boss)
- Achievements
- Pantheon view
- Settings
- Profile / Social

### Interaction Patterns

| Action | Gesture |
|--------|---------|
| Deal Tap Damage | Tap combat viewport |
| Fire Hero Ability 1 | Tap hero portrait |
| Fire Hero Ability 2 | Double-tap hero portrait |
| Fire Ultimate | Hold hero portrait (when charged) |
| Collect AFK Vault | Tap vault icon |
| Switch tabs | Tap bottom tab |
| View hero details | Tap hero in roster |
| Equip gear | Drag-and-drop or tap slot + select |
| Navigate skill tree | Pinch-zoom + tap nodes |
| Switch Summoner familiar | Swipe left/right on portrait |
| Dodge boss attack | Tap dodge prompt (appears in viewport) |

### Visual Feedback Hierarchy

| Event | Feedback |
|-------|----------|
| Normal tap | Small damage number, minor particle burst |
| Critical hit | Large damage number (gold color), screen flash |
| Ability use | Hero animation, ability-specific VFX, sound effect |
| Ultimate | Full-screen VFX, camera zoom, dramatic slowdown |
| Combo Ability | Dual-hero animation, unique VFX, combo name text |
| Level Up | Gold burst from hero, level number floats up, fanfare sound |
| Boss defeated | Explosion VFX, loot cascade, victory fanfare |
| Ascension | Full-screen cinematic: hero rises through clouds, celestial light |
| Demigod | Extended cinematic: hero transforms, joins Pantheon, global buff notification |

### Notification Strategy

| Notification | Timing | Message Style |
|-------------|--------|---------------|
| AFK Vault nearly full | 8 hours after last collection | "Your heroes earned 2.4M Gold and 580K XP! Collect before the vault overflows." |
| Daily quests reset | Morning (user-configurable time) | "New daily quests available! Complete 3 for bonus Stardust." |
| Event starting | Event launch | "Tower of Trials opens today! Climb 50 floors for exclusive rewards." |
| Event ending | 6 hours before close | "Frost Festival ends tonight! Don't miss the final rewards." |
| Expedition complete | On completion | "Scout Mission complete! Your Rogue found 200 Stardust." |
| Boss ready | When power threshold is met | "You're strong enough to challenge Emberlord Kael! Tap to fight." |

**Notification Rules:**
- Maximum 2 push notifications per day
- All notification types toggleable individually in settings
- No notifications between 10 PM and 8 AM (user-configurable quiet hours)
- Notifications include specific reward amounts for increased tap-through rates

### Accessibility

- **VoiceOver support** for all UI elements
- **Adjustable text size** (3 tiers: Standard, Large, Extra Large)
- **Colorblind modes** — affinity colors have secondary indicators (icons + patterns)
- **Tap assist** — option to reduce required tap speed for combo mechanics
- **Auto-battle toggle** — full auto mode for players with motor difficulties
- **Haptic feedback** — customizable intensity (Off, Light, Standard, Strong)

---

## 16. Narrative & Lore

### Setting

The world of **Ascendant** exists on the surface of a shattered planet. Millennia ago, a cataclysm split the earth and flung fragments skyward. These fragments — the Sky Islands — float at varying altitudes, held aloft by ancient magic and leyline energy. The space between islands is The Drift — a luminous void of clouds, wind currents, and occasionally, falling debris.

### Central Myth

The **First Ascendant** was a mortal hero who climbed from the surface to the highest sky island and achieved godhood. Their power shattered the world below — whether intentionally or accidentally is debated. The Ascendant's lingering power sustains the floating islands but is slowly fading. The six Affinities are fragments of the First Ascendant's divided essence.

### Player's Role

The player is a **Skyward Commander** — a rare individual with the ability to sense and nurture the spark of ascension in others. They recruit heroes from across the sky islands and guide them on the path to demigod status. Each hero that achieves Demigod strengthens the Pantheon, which stabilizes the islands and pushes back the encroaching Void (the source of Shadow enemies).

### Lore Delivery

- **No mandatory cutscenes** — lore is 100% optional
- **Island Lore Fragments:** Collectible text entries found while clearing islands (like Destiny's Grimoire cards)
- **Hero Stories:** Each class has a 5-part backstory unlocked by raising Class Mastery
- **Boss Journals:** Defeating an Island Boss unlocks their lore entry, revealing their origin and connection to the world
- **Pantheon Dialogues:** Each Demigod hero delivers a final speech upon retirement, reflecting on their journey

### Tone

Earnest but not po-faced. The world takes its mythology seriously, but individual heroes have personality and humor. Think "Studio Ghibli meets Supergiant Games" — beautiful, wondrous environments with grounded, relatable characters. The Alchemist cracks jokes. The Berserker is secretly a poet. The Thief leaves IOUs in treasure chests she's already looted.

---

## 17. Technical Considerations

### Platform Requirements

| Spec | Requirement |
|------|------------|
| Platform | iOS 16.0+ |
| Devices | iPhone SE (3rd gen) and newer |
| Orientation | Portrait locked |
| Storage | ~500MB initial, ~1.5GB with all assets |
| Network | Required for: summoning, events, leaderboards, guild. Offline play supported for campaign/idle. |

### Performance Targets

| Metric | Target |
|--------|--------|
| Frame Rate | 60 FPS during combat; 30 FPS minimum on older devices |
| Load Time | < 3 seconds cold start; < 1 second between screens |
| Battery | < 8% per hour of active play; < 1% per hour idle (background) |
| Memory | < 300MB RAM active |

### Save System

- **Cloud Save:** Automatic sync via Game Center / custom account. Prevents loss on device change.
- **Local Save:** Automatic save every 30 seconds + on app background/close. Encrypted to prevent tampering.
- **Offline Queue:** Actions taken offline are queued and synced when connectivity returns.

### Backend Architecture

- **Server-Authoritative:** All gacha results, premium purchases, and leaderboard submissions validated server-side
- **Event System:** Server pushes event configurations; client renders them dynamically (no app update required for new events)
- **Anti-Cheat:** Save file integrity validation, tap-rate sanity checks, impossible-progression detection

### Analytics Hooks

Key metrics to track for live-ops:
- **D1/D7/D30 Retention:** Healthy targets are 40%/20%/10%
- **Session Length:** Target average 15 minutes, median 8 minutes
- **Prestige Rate:** How often players ascend; target 1-2x per day for active players
- **Summon Conversion:** Percentage of players who purchase Stardust after exhausting free summons
- **Progression Funnel:** Where players stall (which island, which boss) for difficulty tuning
- **Class Popularity:** Track pick rates to inform balance patches and cosmetic priorities

---

## Appendix A — Full Class Quick Reference

| # | Class | Role | Affinity | Tier | Tap Mechanic Summary |
|---|-------|------|----------|------|---------------------|
| 1 | Warrior | Vanguard | Radiance | Starter | Shockwave every 5th tap |
| 2 | Mage | Caster | Frost | Starter | Homing bolts that chain on kill |
| 3 | Priest | Support | Radiance | Starter | Auto-triage: heal allies or damage enemies |
| 4 | Rogue | Striker | Shadow | Starter | Combo points into Finishing Blow |
| 5 | Marksman | Ranger | Frost | Apprentice | Hold-to-charge precision shots |
| 6 | Defender | Vanguard | Frost | Apprentice | Shield Bash applies defense debuff |
| 7 | Berserker | Vanguard | Flame | Apprentice | Rage-fueled taps at increasing damage/HP cost |
| 8 | Druid | Support | Nature | Apprentice | Alternating HoTs and damage vines |
| 9 | Thief | Striker | Shadow | Apprentice | Pickpocket for bonus gold and buff steal |
| 10 | Shaman | Support | Storm | Apprentice | Chain lightning boosted by active totems |
| 11 | Warlock | Caster | Flame | Adept | Taps drain HP for 150% damage |
| 12 | Ranger | Ranger | Storm | Adept | Coordinated strikes building Bond meter |
| 13 | Spell-Blade | Striker | Storm | Adept | Melee charges into ranged Energy Wave |
| 14 | Necromancer | Caster | Shadow | Adept | Kill-taps raise skeleton minions |
| 15 | Monk | Vanguard | Storm | Adept | 5-hit combo chain into Perfect Strike |
| 16 | Paladin | Vanguard | Radiance | Adept | Taps deal damage AND heal lowest ally |
| 17 | Bard | Support | Radiance | Adept | Shockwaves building to Crescendo song amplification |
| 18 | Dragon-Hunter | Specialist | Nature | Master | Charged crossbow bolts; bonus vs bosses |
| 19 | Summoner | Specialist | Nature | Master | Command familiars; swipe to switch |
| 20 | Alchemist | Support/DPS | Flame | Master | Throw rotating potion types |
| 21 | Chronomancer | Caster | Frost | Master | Slow stacks leading to time freeze |
| 22 | Gunslinger | Ranger | Flame | Master | Ultra-fast dual-pistol fire with reload rhythm |
| 23 | Warden | Vanguard | Nature | Master | Shockwave taps that root enemies |
| 24 | Reaper | Striker | Shadow | Master | Triple damage to low-HP enemies; soul harvesting |

---

## Appendix B — Affinity Advantage Chart

```
              FLAME
            /       \
      beats           loses to
        /               \
   NATURE               STORM
        \               /
      loses to       beats
            \       /
              FROST

     SHADOW <=====> RADIANCE
          (mutual +30%)
```

**Full Interaction Table:**

| Attacker \ Defender | Flame | Frost | Storm | Nature | Shadow | Radiance |
|----|-------|-------|-------|--------|--------|----------|
| **Flame** | -- | -30% | -- | +30% | -- | -- |
| **Frost** | +30% | -- | -- | -- | -- | -- |
| **Storm** | -- | +30% | -- | -30% | -- | -- |
| **Nature** | -30% | -- | +30% | -- | -- | -- |
| **Shadow** | -- | -- | -- | -- | -- | +30% |
| **Radiance** | -- | -- | -- | -- | +30% | -- |

---

## Appendix C — Combo Ability Reference

| Hero A | Hero B | Combo Name | Effect Summary |
|--------|--------|-----------|---------------|
| Warrior | Mage | Arcane Cleave | Physical + magic AoE blade wave |
| Priest | Necromancer | Soul Judgment | Holy/dark explosion that heals the party |
| Rogue | Marksman | Pinpoint Ambush | Guaranteed double critical strike |
| Berserker | Druid | Wild Rampage | Berserker becomes a flame bear |
| Defender | Paladin | Unbreakable Wall | Dual immunity + forced aggro |
| Spell-Blade | Bard | Harmonic Edge | Melee strikes play damaging chords |
| Monk | Reaper | Final Judgment | 10-hit escalating execute combo |
| Shaman | Summoner | Primal Call | Totems + familiars merge into a titan |
| Warlock | Chronomancer | Temporal Curse | Curse applies twice via time rewind |
| Dragon-Hunter | Ranger | Apex Predator | Focused team burst with all companions |
| Thief | Gunslinger | Heist | 10x gold drop + bonus damage barrage |
| Mage | Alchemist | Volatile Arcana | Heroes swap signature abilities |
| Warden | Necromancer | Cycle of Life and Death | Corpses sprout healing roots and nature minions |

*30+ additional combos are discoverable in-game through experimentation.*

---

*Ascendant Game Design Document v1.0*
*Created March 2026*
