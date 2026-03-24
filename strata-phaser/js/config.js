// STRATA - Game Configuration & Data
// ===================================
window.STRATA = window.STRATA || {};

(function(S) {
    'use strict';

    // ---- Display ----
    S.WIDTH = 960;
    S.HEIGHT = 640;
    S.TILE = 32;

    // ---- World ----
    S.WORLD_W = 80;
    S.WORLD_H = 210;
    S.SURFACE_Y = 6;

    // ---- Tile Types ----
    S.T = {
        AIR: 0,
        DIRT: 1,
        ROCK: 2,
        HARD_ROCK: 3,
        ITEM: 4,
        HAZARD_GAS: 5,
        HAZARD_WATER: 6,
        BEDROCK: 7,
        SURFACE_GRASS: 8,
        BUILDING: 9
    };

    // ---- Rarity ----
    S.RARITY = {
        COMMON:    { name: 'Common',    color: 0xAAAAAA, hex: '#AAAAAA', mult: 1.0,  weight: 60 },
        UNCOMMON:  { name: 'Uncommon',  color: 0x4FC3F7, hex: '#4FC3F7', mult: 1.5,  weight: 25 },
        RARE:      { name: 'Rare',      color: 0xAB47BC, hex: '#AB47BC', mult: 2.5,  weight: 12 },
        LEGENDARY: { name: 'Legendary', color: 0xFFD700, hex: '#FFD700', mult: 5.0,  weight: 3  }
    };

    // ---- Strata Definitions ----
    S.STRATA = [
        {
            id: 0, name: 'Surface', startY: 0, endY: S.SURFACE_Y,
            hardness: 0, hazards: [],
            colors: { base: 0x87CEEB, ground: 0x8B7355, sky: 0x4A90D9, dirt: 0x8B7355, bg: 0x4A90D9 }
        },
        {
            id: 1, name: 'The Dump', startY: S.SURFACE_Y, endY: 45,
            hardness: 1, hazards: ['gas', 'unstable'],
            colors: {
                base: 0x6B6B6B, accent1: 0x8B7355, accent2: 0xA0522D,
                dirt: 0x5C4033, bg: 0x3D3D3D
            },
            itemDensity: 0.04
        },
        {
            id: 2, name: 'The Foundation', startY: 45, endY: 95,
            hardness: 2, hazards: ['flood', 'rust'],
            colors: {
                base: 0x4A4A5A, accent1: 0x708090, accent2: 0x8B4513,
                dirt: 0x36454F, bg: 0x2A2A3A
            },
            itemDensity: 0.05
        },
        {
            id: 3, name: 'The Catacombs', startY: 95, endY: 155,
            hardness: 3, hazards: ['cavein', 'curse', 'darkness'],
            colors: {
                base: 0x2D1B2E, accent1: 0xE8E8E0, accent2: 0x6B3FA0,
                dirt: 0x1A0A1A, bg: 0x1A0A1A
            },
            itemDensity: 0.06
        },
        {
            id: 4, name: 'The Ruin', startY: 155, endY: 205,
            hardness: 4, hazards: ['traps', 'guardians'],
            colors: {
                base: 0x8B6914, accent1: 0xDAA520, accent2: 0xCD853F,
                dirt: 0x5C4A1E, bg: 0x2A1A0A
            },
            itemDensity: 0.07
        }
    ];

    // ---- Faction Definitions ----
    S.FACTIONS = {
        curator: {
            id: 'curator', name: 'The Curator', baseMult: 1.0,
            color: 0xE8D44D, hex: '#E8D44D',
            desc: 'Museum collector. Pays well for historical artifacts and complete sets.'
        },
        foundry: {
            id: 'foundry', name: 'The Foundry', baseMult: 1.0,
            color: 0xE07040, hex: '#E07040',
            desc: 'Industrial forge. Wants metals, ores, and machine parts.'
        },
        fences: {
            id: 'fences', name: "Finn's Fences", baseMult: 0.7,
            color: 0x70B070, hex: '#70B070',
            desc: 'Black market. Buys anything, no questions, but pays less.'
        }
    };

    // ---- Reputation Tiers ----
    S.REP_TIERS = [
        { min: 0,  name: 'Neutral',   bonus: 0.00 },
        { min: 20, name: 'Trusted',   bonus: 0.10 },
        { min: 40, name: 'Preferred', bonus: 0.20 },
        { min: 60, name: 'Partner',   bonus: 0.30 },
        { min: 80, name: 'Patron',    bonus: 0.40 }
    ];

    // ---- Item Definitions (36 items across 4 strata) ----
    S.ITEMS = [
        // Stratum 1: The Dump
        { id: 'scrap_metal',   name: 'Scrap Metal',    stratum: 1, rarity: 'COMMON',    baseValue: 5,   weight: 2, desc: 'Twisted bits of rusted metal',          buyers: { foundry: 1.2, fences: 0.8 }},
        { id: 'old_wire',      name: 'Copper Wire',    stratum: 1, rarity: 'COMMON',    baseValue: 8,   weight: 1, desc: 'Tarnished copper wiring',                buyers: { foundry: 1.3, fences: 0.9 }},
        { id: 'broken_phone',  name: 'Broken Phone',   stratum: 1, rarity: 'COMMON',    baseValue: 12,  weight: 1, desc: 'A shattered mobile device',              buyers: { foundry: 1.0, fences: 1.1 }},
        { id: 'glass_bottle',  name: 'Vintage Bottle', stratum: 1, rarity: 'COMMON',    baseValue: 6,   weight: 1, desc: 'An old glass bottle with faded label',   buyers: { curator: 1.1, fences: 0.7 }},
        { id: 'lost_wallet',   name: 'Lost Wallet',    stratum: 1, rarity: 'UNCOMMON',  baseValue: 25,  weight: 1, desc: 'Still has some old bills inside',        buyers: { fences: 1.5, curator: 0.8 }},
        { id: 'old_radio',     name: 'Vintage Radio',  stratum: 1, rarity: 'UNCOMMON',  baseValue: 35,  weight: 2, desc: 'A vacuum tube radio, intact',            buyers: { curator: 1.4, fences: 1.0 }},
        { id: 'circuit_board', name: 'Circuit Board',  stratum: 1, rarity: 'UNCOMMON',  baseValue: 20,  weight: 1, desc: 'Complex circuitry with gold traces',     buyers: { foundry: 1.5, fences: 0.9 }},
        { id: 'toy_robot',     name: 'Tin Toy Robot',  stratum: 1, rarity: 'RARE',      baseValue: 80,  weight: 1, desc: 'A rare vintage tin toy',                 buyers: { curator: 2.0, fences: 1.2 }},
        { id: 'safe_box',      name: 'Locked Safe',    stratum: 1, rarity: 'RARE',      baseValue: 120, weight: 3, desc: 'Heavy iron safe, still sealed',          buyers: { fences: 1.8, curator: 1.0 }},

        // Stratum 2: The Foundation
        { id: 'coal_chunk',     name: 'Coal Chunk',          stratum: 2, rarity: 'COMMON',   baseValue: 10,  weight: 2, desc: 'Dense black coal',                  buyers: { foundry: 1.3, fences: 0.6 }},
        { id: 'steel_beam',     name: 'Steel Beam Fragment', stratum: 2, rarity: 'COMMON',   baseValue: 15,  weight: 3, desc: 'A rusted steel I-beam section',      buyers: { foundry: 1.5, fences: 0.5 }},
        { id: 'machine_gear',   name: 'Machine Gear',        stratum: 2, rarity: 'COMMON',   baseValue: 18,  weight: 2, desc: 'A heavy brass gear',                 buyers: { foundry: 1.4, curator: 0.8 }},
        { id: 'iron_pipe',      name: 'Iron Pipe',           stratum: 2, rarity: 'COMMON',   baseValue: 12,  weight: 2, desc: 'Corroded iron plumbing',              buyers: { foundry: 1.2, fences: 0.7 }},
        { id: 'old_coin',       name: 'Old Currency',        stratum: 2, rarity: 'UNCOMMON', baseValue: 40,  weight: 1, desc: 'Coins from a forgotten era',          buyers: { curator: 1.6, fences: 1.2 }},
        { id: 'blueprint',      name: 'Factory Blueprint',   stratum: 2, rarity: 'UNCOMMON', baseValue: 55,  weight: 1, desc: 'Detailed industrial schematics',      buyers: { curator: 1.5, foundry: 1.3 }},
        { id: 'pressure_gauge', name: 'Pressure Gauge',      stratum: 2, rarity: 'UNCOMMON', baseValue: 30,  weight: 1, desc: 'A precision brass instrument',        buyers: { foundry: 1.2, curator: 1.3 }},
        { id: 'steam_engine',   name: 'Steam Engine Part',   stratum: 2, rarity: 'RARE',     baseValue: 150, weight: 3, desc: 'A beautiful piece of engineering',     buyers: { curator: 2.0, foundry: 1.5 }},
        { id: 'pocket_watch',   name: 'Gold Pocket Watch',   stratum: 2, rarity: 'RARE',     baseValue: 200, weight: 1, desc: 'Exquisite craftsmanship, still ticks', buyers: { curator: 1.8, fences: 1.5 }},

        // Stratum 3: The Catacombs
        { id: 'bone_fragment',  name: 'Ancient Bone',     stratum: 3, rarity: 'COMMON',    baseValue: 15,  weight: 1, desc: 'Yellowed bone from unknown remains',   buyers: { curator: 1.2, fences: 0.5 }},
        { id: 'clay_pottery',   name: 'Clay Pottery',     stratum: 3, rarity: 'COMMON',    baseValue: 20,  weight: 2, desc: 'Fragments of decorated pottery',       buyers: { curator: 1.4, fences: 0.6 }},
        { id: 'iron_nail',      name: 'Iron Nails',       stratum: 3, rarity: 'COMMON',    baseValue: 12,  weight: 1, desc: 'Hand-forged iron nails',               buyers: { foundry: 1.1, curator: 0.9 }},
        { id: 'leather_scrap',  name: 'Ancient Leather',  stratum: 3, rarity: 'COMMON',    baseValue: 18,  weight: 1, desc: 'Preserved leather fragments',          buyers: { curator: 1.2, fences: 0.8 }},
        { id: 'silver_ring',    name: 'Silver Ring',      stratum: 3, rarity: 'UNCOMMON',  baseValue: 65,  weight: 1, desc: 'A tarnished silver ring with a seal',  buyers: { curator: 1.5, fences: 1.3 }},
        { id: 'sealed_urn',     name: 'Sealed Urn',       stratum: 3, rarity: 'UNCOMMON',  baseValue: 75,  weight: 2, desc: 'An ornate sealed funeral urn',         buyers: { curator: 1.8, fences: 1.0 }},
        { id: 'rusty_sword',    name: 'Medieval Sword',   stratum: 3, rarity: 'UNCOMMON',  baseValue: 90,  weight: 2, desc: 'A corroded but recognizable blade',    buyers: { curator: 1.7, foundry: 1.0 }},
        { id: 'jeweled_cross',  name: 'Jeweled Cross',    stratum: 3, rarity: 'RARE',      baseValue: 250, weight: 1, desc: 'Gold cross set with rubies',           buyers: { curator: 2.2, fences: 1.5 }},
        { id: 'crown_fragment', name: 'Crown Fragment',   stratum: 3, rarity: 'LEGENDARY', baseValue: 500, weight: 1, desc: 'A piece of a royal crown',             buyers: { curator: 3.0, fences: 2.0 }},

        // Stratum 4: The Ruin
        { id: 'carved_stone',   name: 'Carved Stone Block', stratum: 4, rarity: 'COMMON',    baseValue: 25,  weight: 3, desc: 'Stone block with ancient carvings',  buyers: { curator: 1.3, foundry: 0.8 }},
        { id: 'clay_tablet',    name: 'Clay Tablet',        stratum: 4, rarity: 'COMMON',    baseValue: 30,  weight: 2, desc: 'Inscribed with unknown script',      buyers: { curator: 1.5, fences: 0.7 }},
        { id: 'bronze_tool',    name: 'Bronze Tool',        stratum: 4, rarity: 'COMMON',    baseValue: 22,  weight: 2, desc: 'A primitive but well-made tool',     buyers: { foundry: 1.3, curator: 1.2 }},
        { id: 'obsidian_shard', name: 'Obsidian Shard',     stratum: 4, rarity: 'COMMON',    baseValue: 28,  weight: 1, desc: 'Sharp volcanic glass fragment',      buyers: { foundry: 1.2, fences: 0.9 }},
        { id: 'gold_figurine',  name: 'Gold Figurine',      stratum: 4, rarity: 'UNCOMMON',  baseValue: 120, weight: 1, desc: 'A small golden deity figure',        buyers: { curator: 2.0, fences: 1.5 }},
        { id: 'jade_amulet',    name: 'Jade Amulet',        stratum: 4, rarity: 'UNCOMMON',  baseValue: 140, weight: 1, desc: 'Green jade carved with symbols',     buyers: { curator: 1.8, fences: 1.3 }},
        { id: 'temple_glyph',   name: 'Temple Glyph',       stratum: 4, rarity: 'UNCOMMON',  baseValue: 100, weight: 2, desc: 'Stone tablet with glowing symbols',  buyers: { curator: 2.0, fences: 0.8 }},
        { id: 'ritual_mask',    name: 'Ritual Mask',        stratum: 4, rarity: 'RARE',      baseValue: 350, weight: 2, desc: 'An ornate ceremonial mask',          buyers: { curator: 2.5, fences: 1.8 }},
        { id: 'void_crystal',   name: 'Void Crystal',       stratum: 4, rarity: 'LEGENDARY', baseValue: 800, weight: 1, desc: 'Pulses with otherworldly energy',    buyers: { curator: 2.5, fences: 2.5 }}
    ];

    // Build lookup maps
    S.ITEM_MAP = {};
    S.ITEMS.forEach(function(item) { S.ITEM_MAP[item.id] = item; });

    S.ITEMS_BY_STRATUM = {};
    S.ITEMS.forEach(function(item) {
        if (!S.ITEMS_BY_STRATUM[item.stratum]) S.ITEMS_BY_STRATUM[item.stratum] = [];
        S.ITEMS_BY_STRATUM[item.stratum].push(item);
    });

    // ---- Player Defaults ----
    S.PLAYER = {
        maxStamina: 100,
        staminaDrainDig: 1.5,
        staminaDrainMove: 0.08,
        staminaRecovery: 0.5,
        moveSpeed: 200,
        jumpVelocity: -400,
        gravity: 800,
        inventorySlots: 10,
        headlampRadius: 7,
        headlampAngle: Math.PI / 3,
        startGold: 50
    };

    // ---- Economy Config ----
    S.ECONOMY = {
        priceFluctuationRate: 0.02,
        demandDecayRate: 0.01,
        supplyImpactFactor: 0.05,
        eventDuration: 5
    };

    // ---- Market Events ----
    S.MARKET_EVENTS = [
        null,
        {
            name: "Founder's Festival",
            factionBoost: { curator: 1.5 },
            itemBoost: ['old_radio', 'pocket_watch', 'jeweled_cross', 'crown_fragment', 'ritual_mask'],
            itemMult: 1.3, color: 0xE8D44D
        },
        {
            name: 'Industrial Boom',
            factionBoost: { foundry: 1.4 },
            itemBoost: ['scrap_metal', 'steel_beam', 'machine_gear', 'coal_chunk', 'steam_engine'],
            itemMult: 1.3, color: 0xE07040
        },
        {
            name: 'Black Market Bonanza',
            factionBoost: { fences: 1.6 },
            itemBoost: ['lost_wallet', 'safe_box', 'old_coin'],
            itemMult: 1.3, color: 0x70B070
        },
        {
            name: 'Ancient Discovery',
            factionBoost: { curator: 1.8 },
            itemBoost: ['gold_figurine', 'jade_amulet', 'temple_glyph', 'void_crystal'],
            itemMult: 1.3, color: 0xC070E0
        }
    ];

    // ---- Upgrade Tree Definitions ----
    S.UPGRADE_TREES = {
        excavation: {
            name: 'Excavation', desc: 'How you dig',
            color: 0xE07040,
            tiers: [
                [{ id: 'basic_pick', name: 'Basic Pickaxe', cost: 0, branch: 'base', requires: null,
                   effects: { digSpeed: 1.0 }, desc: 'A simple iron pickaxe' }],
                [
                    { id: 'pneumatic_hammer', name: 'Pneumatic Hammer', cost: 150, branch: 'power', requires: 'basic_pick',
                      effects: { digSpeed: 2.0, itemDamageChance: 0.2 }, desc: '2x dig speed, 20% item damage risk' },
                    { id: 'archaeologist_kit', name: "Archaeologist's Kit", cost: 200, branch: 'precision', requires: 'basic_pick',
                      effects: { digSpeed: 0.8, itemQualityBonus: 1.5 }, desc: 'Slower but +50% item quality' },
                    { id: 'blast_charges', name: 'Blast Charges', cost: 175, branch: 'range', requires: 'basic_pick',
                      effects: { digSpeed: 1.5, aoeRadius: 1 }, desc: 'Clear 3x3 areas' }
                ],
                [
                    { id: 'thermal_lance', name: 'Thermal Lance', cost: 500, branch: 'power', requires: 'pneumatic_hammer',
                      effects: { digSpeed: 3.0, canDigHardRock: true }, desc: 'Cuts through anything' },
                    { id: 'laser_scalpel', name: 'Laser Scalpel', cost: 600, branch: 'precision', requires: 'archaeologist_kit',
                      effects: { digSpeed: 1.0, itemQualityBonus: 2.0, neverDamage: true }, desc: 'Never damages items' },
                    { id: 'tunnel_bore', name: 'Tunnel Bore', cost: 550, branch: 'range', requires: 'blast_charges',
                      effects: { digSpeed: 2.5, aoeRadius: 2 }, desc: 'Drills wide tunnels' }
                ],
                [
                    { id: 'worldbreaker', name: 'The Worldbreaker', cost: 2000, branch: 'power', requires: 'thermal_lance',
                      effects: { digSpeed: 5.0, aoeRadius: 3 }, desc: 'Destroys everything' },
                    { id: 'quantum_extractor', name: 'Quantum Extractor', cost: 2500, branch: 'precision', requires: 'laser_scalpel',
                      effects: { digSpeed: 2.0, itemQualityBonus: 3.0, neverDamage: true }, desc: 'Phase items out of rock' },
                    { id: 'the_colony', name: 'The Colony', cost: 2200, branch: 'range', requires: 'tunnel_bore',
                      effects: { digSpeed: 3.0, autoDig: true }, desc: 'Autonomous mini-drills' }
                ]
            ]
        },
        logistics: {
            name: 'Logistics', desc: 'How you carry and move',
            color: 0x4FC3F7,
            tiers: [
                [{ id: 'canvas_pack', name: 'Canvas Backpack', cost: 0, branch: 'base', requires: null,
                   effects: { inventorySlots: 10 }, desc: 'Basic 10-slot pack' }],
                [
                    { id: 'reinforced_pack', name: 'Reinforced Pack', cost: 120, branch: 'capacity', requires: 'canvas_pack',
                      effects: { inventorySlots: 18 }, desc: '18 inventory slots' },
                    { id: 'grapple_line', name: 'Grapple Line', cost: 150, branch: 'speed', requires: 'canvas_pack',
                      effects: { inventorySlots: 12, moveSpeed: 240 }, desc: 'Fast movement, 12 slots' },
                    { id: 'supply_drone', name: 'Supply Drone', cost: 200, branch: 'automation', requires: 'canvas_pack',
                      effects: { inventorySlots: 14, autoSend: true }, desc: 'Auto-sends items to surface' }
                ],
                [
                    { id: 'cargo_mech', name: 'Cargo Mech', cost: 450, branch: 'capacity', requires: 'reinforced_pack',
                      effects: { inventorySlots: 30, moveSpeed: 160 }, desc: '30 slots but slower' },
                    { id: 'pneumatic_tubes', name: 'Pneumatic Tubes', cost: 500, branch: 'speed', requires: 'grapple_line',
                      effects: { inventorySlots: 14, instantReturn: true }, desc: 'Instant return to surface' },
                    { id: 'conveyor_system', name: 'Conveyor System', cost: 550, branch: 'automation', requires: 'supply_drone',
                      effects: { inventorySlots: 20, autoSend: true }, desc: 'Continuous auto-hauling' }
                ],
                [
                    { id: 'pocket_dimension', name: 'Pocket Dimension', cost: 1800, branch: 'capacity', requires: 'cargo_mech',
                      effects: { inventorySlots: 50 }, desc: '50 slots - carry everything' },
                    { id: 'teleport_pad', name: 'Teleport Pad', cost: 2000, branch: 'speed', requires: 'pneumatic_tubes',
                      effects: { inventorySlots: 20, teleport: true }, desc: 'Teleport to surface and back' },
                    { id: 'logistics_ai', name: 'The Logistics AI', cost: 2200, branch: 'automation', requires: 'conveyor_system',
                      effects: { inventorySlots: 30, autoSend: true, autoSort: true }, desc: 'Fully automated logistics' }
                ]
            ]
        },
        commerce: {
            name: 'Commerce', desc: 'How you sell',
            color: 0xFFD700,
            tiers: [
                [{ id: 'market_stall', name: 'Market Stall', cost: 0, branch: 'base', requires: null,
                   effects: { sellBonus: 0 }, desc: 'Basic market access' }],
                [
                    { id: 'jewelers_loupe', name: "Jeweler's Loupe", cost: 130, branch: 'appraisal', requires: 'market_stall',
                      effects: { sellBonus: 0.1, revealItems: true }, desc: '+10% sale bonus' },
                    { id: 'silver_tongue', name: 'Silver Tongue', cost: 160, branch: 'negotiation', requires: 'market_stall',
                      effects: { sellBonus: 0.15 }, desc: '+15% on all sales' },
                    { id: 'trade_contacts', name: 'Trade Contacts', cost: 180, branch: 'network', requires: 'market_stall',
                      effects: { sellBonus: 0.05, extraBuyers: true }, desc: 'Access to more merchants' }
                ],
                [
                    { id: 'master_appraiser', name: 'Master Appraiser', cost: 500, branch: 'appraisal', requires: 'jewelers_loupe',
                      effects: { sellBonus: 0.25, instantAppraisal: true }, desc: '+25% bonus' },
                    { id: 'market_manipulation', name: 'Market Manipulation', cost: 600, branch: 'negotiation', requires: 'silver_tongue',
                      effects: { sellBonus: 0.3, canManipulate: true }, desc: '+30% bonus, manipulate prices' },
                    { id: 'auction_house', name: 'Auction House', cost: 650, branch: 'network', requires: 'trade_contacts',
                      effects: { sellBonus: 0.15, auction: true }, desc: 'Auctions with bidding wars' }
                ],
                [
                    { id: 'the_connoisseur', name: 'The Connoisseur', cost: 2000, branch: 'appraisal', requires: 'master_appraiser',
                      effects: { sellBonus: 0.4, showBestItem: true }, desc: '+40% bonus, find best items' },
                    { id: 'monopoly', name: 'Monopoly', cost: 2500, branch: 'negotiation', requires: 'market_manipulation',
                      effects: { sellBonus: 0.5 }, desc: '+50% on all sales' },
                    { id: 'trade_empire', name: 'The Trade Empire', cost: 2200, branch: 'network', requires: 'auction_house',
                      effects: { sellBonus: 0.3, passiveIncome: true }, desc: '+30% bonus, passive income' }
                ]
            ]
        }
    };

    // Build upgrade lookup
    S.UPGRADE_MAP = {};
    Object.keys(S.UPGRADE_TREES).forEach(function(treeId) {
        var tree = S.UPGRADE_TREES[treeId];
        tree.tiers.forEach(function(tier, tierIdx) {
            tier.forEach(function(upgrade) {
                upgrade.tree = treeId;
                upgrade.tier = tierIdx;
                S.UPGRADE_MAP[upgrade.id] = upgrade;
            });
        });
    });

    // ---- Utility Functions ----
    S.getStratumAt = function(tileY) {
        for (var i = S.STRATA.length - 1; i >= 0; i--) {
            if (tileY >= S.STRATA[i].startY) return S.STRATA[i];
        }
        return S.STRATA[0];
    };

    S.depthToMeters = function(tileY) {
        return Math.floor(Math.max(0, tileY - S.SURFACE_Y) * 3);
    };

    S.noise = function(x, y) {
        var n = Math.sin(x * 12.9898 + y * 78.233) * 43758.5453;
        return n - Math.floor(n);
    };

    S.hash = function(x, y) {
        return ((x * 73856093) ^ (y * 19349663)) >>> 0;
    };

    S.rollItem = function(stratumId) {
        var items = S.ITEMS_BY_STRATUM[stratumId];
        if (!items || items.length === 0) return null;
        var totalWeight = 0;
        var weighted = items.map(function(item) {
            var w = S.RARITY[item.rarity].weight;
            totalWeight += w;
            return { item: item, weight: w };
        });
        var roll = Math.random() * totalWeight;
        var cumulative = 0;
        for (var i = 0; i < weighted.length; i++) {
            cumulative += weighted[i].weight;
            if (roll <= cumulative) return weighted[i].item;
        }
        return weighted[weighted.length - 1].item;
    };

    S.colorToHex = function(color) {
        return '#' + ('000000' + color.toString(16)).slice(-6);
    };

    S.lerpColor = function(a, b, t) {
        var ar = (a >> 16) & 0xFF, ag = (a >> 8) & 0xFF, ab = a & 0xFF;
        var br = (b >> 16) & 0xFF, bg = (b >> 8) & 0xFF, bb = b & 0xFF;
        var r = Math.round(ar + (br - ar) * t);
        var g = Math.round(ag + (bg - ag) * t);
        var bl = Math.round(ab + (bb - ab) * t);
        return (r << 16) | (g << 8) | bl;
    };

    S.brighten = function(color, amount) {
        var r = Math.min(255, ((color >> 16) & 0xFF) + amount);
        var g = Math.min(255, ((color >> 8) & 0xFF) + amount);
        var b = Math.min(255, (color & 0xFF) + amount);
        return (r << 16) | (g << 8) | b;
    };

    S.darken = function(color, amount) {
        var r = Math.max(0, ((color >> 16) & 0xFF) - amount);
        var g = Math.max(0, ((color >> 8) & 0xFF) - amount);
        var b = Math.max(0, (color & 0xFF) - amount);
        return (r << 16) | (g << 8) | b;
    };

    // ---- Shared Game State ----
    S.state = {
        gold: 50,
        stamina: 100,
        maxStamina: 100,
        inventory: [],
        reputation: { curator: 0, foundry: 0, fences: 0 },
        purchasedUpgrades: ['basic_pick', 'canvas_pack', 'market_stall'],
        expedition: {
            number: 0,
            maxDepth: 0,
            itemsFound: 0,
            tilesDug: 0,
            startTime: 0,
            items: []
        },
        effects: {
            digSpeed: 1.0,
            inventorySlots: 10,
            moveSpeed: 200,
            sellBonus: 0,
            itemQualityBonus: 1.0,
            neverDamage: false,
            aoeRadius: 0,
            headlampRadius: 7,
            itemDamageChance: 0
        },
        // Auto-digger system
        autoDigEnabled: false,
        hiredDiggers: 0,  // number of extra hired diggers
        autoDiggerDepth: 0 // how deep auto-diggers have reached
    };

    // Auto-digger config
    S.AUTO_DIGGER = {
        enableCost: 500,       // cost to enable auto-dig for your main digger
        hireCosts: [300, 600, 1200, 2500, 5000], // escalating cost per additional digger (max 5)
        digInterval: 0.8,      // seconds between each auto-dig action
        itemFindRate: 0.04,    // chance to find an item per dig
        maxDiggers: 5
    };

    S.resetState = function() {
        S.state.gold = S.PLAYER.startGold;
        S.state.stamina = S.PLAYER.maxStamina;
        S.state.maxStamina = S.PLAYER.maxStamina;
        S.state.inventory = [];
        S.state.reputation = { curator: 0, foundry: 0, fences: 0 };
        S.state.purchasedUpgrades = ['basic_pick', 'canvas_pack', 'market_stall'];
        S.state.expedition = { number: 0, maxDepth: 0, itemsFound: 0, tilesDug: 0, startTime: 0, items: [] };
        S.state.effects = {
            digSpeed: 1.0, inventorySlots: 10, moveSpeed: 200, sellBonus: 0,
            itemQualityBonus: 1.0, neverDamage: false, aoeRadius: 0,
            headlampRadius: 7, itemDamageChance: 0
        };
        S.state.autoDigEnabled = false;
        S.state.hiredDiggers = 0;
        S.state.autoDiggerDepth = 0;
    };

    // Debug: press backtick (`) to add gold while testing
    if (typeof window !== 'undefined') {
        window.addEventListener('keydown', function(e) {
            if (e.key === '`' || e.key === '~') {
                S.state.gold += 1000;
                console.log('[DEBUG] +1000 gold. Total: ' + S.state.gold);
            }
        });
    }

    // Recalculate effects from purchased upgrades
    // Since all branches are accessible, effects stack: take the best value per stat
    // across all purchased upgrades (max for beneficial stats, min for penalties)
    S.recalcEffects = function() {
        var eff = S.state.effects;
        eff.digSpeed = 1.0;
        eff.inventorySlots = 10;
        eff.moveSpeed = 200;
        eff.sellBonus = 0;
        eff.itemQualityBonus = 1.0;
        eff.neverDamage = false;
        eff.aoeRadius = 0;
        eff.headlampRadius = 7;
        eff.itemDamageChance = 0;

        S.state.purchasedUpgrades.forEach(function(id) {
            var upg = S.UPGRADE_MAP[id];
            if (!upg) return;
            var fx = upg.effects;
            // Stack: take the best (highest) value for each beneficial stat
            if (fx.digSpeed !== undefined) eff.digSpeed = Math.max(eff.digSpeed, fx.digSpeed);
            if (fx.inventorySlots !== undefined) eff.inventorySlots = Math.max(eff.inventorySlots, fx.inventorySlots);
            if (fx.moveSpeed !== undefined) eff.moveSpeed = Math.max(eff.moveSpeed, fx.moveSpeed);
            if (fx.sellBonus !== undefined) eff.sellBonus += fx.sellBonus;
            if (fx.itemQualityBonus !== undefined) eff.itemQualityBonus = Math.max(eff.itemQualityBonus, fx.itemQualityBonus);
            if (fx.neverDamage) eff.neverDamage = true;
            if (fx.aoeRadius !== undefined) eff.aoeRadius = Math.max(eff.aoeRadius, fx.aoeRadius);
            // Item damage chance: if you have neverDamage, override to 0; otherwise take lowest
            if (fx.itemDamageChance !== undefined && !eff.neverDamage) {
                eff.itemDamageChance = Math.max(eff.itemDamageChance, fx.itemDamageChance);
            }
        });

        // neverDamage overrides any damage chance
        if (eff.neverDamage) eff.itemDamageChance = 0;
    };

})(window.STRATA);
