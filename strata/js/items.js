import { RARITY } from './config.js';

// Item definitions for all 4 strata
// Each item: { id, name, stratum, rarity, baseValue, weight, description, buyers[] }
// buyers: which factions want this item and their interest multiplier

export const ITEM_DEFS = [
    // === STRATUM 1: The Dump (modern refuse) ===
    { id: 'scrap_metal', name: 'Scrap Metal', stratum: 1, rarity: 'COMMON', baseValue: 5, weight: 2, description: 'Rusty but recyclable', buyers: { foundry: 1.2, fences: 0.8 } },
    { id: 'old_wire', name: 'Copper Wire', stratum: 1, rarity: 'COMMON', baseValue: 8, weight: 1, description: 'Tangled copper wiring', buyers: { foundry: 1.3, fences: 0.9 } },
    { id: 'broken_phone', name: 'Broken Phone', stratum: 1, rarity: 'COMMON', baseValue: 12, weight: 1, description: 'Cracked screen, might have rare metals inside', buyers: { foundry: 1.0, fences: 1.1 } },
    { id: 'glass_bottle', name: 'Vintage Bottle', stratum: 1, rarity: 'COMMON', baseValue: 6, weight: 1, description: 'Old glass bottle, surprisingly intact', buyers: { curator: 1.1, fences: 0.7 } },
    { id: 'lost_wallet', name: 'Lost Wallet', stratum: 1, rarity: 'UNCOMMON', baseValue: 25, weight: 1, description: 'Still has some old bills inside', buyers: { fences: 1.5, curator: 0.8 } },
    { id: 'old_radio', name: 'Vintage Radio', stratum: 1, rarity: 'UNCOMMON', baseValue: 35, weight: 2, description: 'A vacuum tube radio, collectors love these', buyers: { curator: 1.4, fences: 1.0 } },
    { id: 'circuit_board', name: 'Circuit Board', stratum: 1, rarity: 'UNCOMMON', baseValue: 20, weight: 1, description: 'Contains trace amounts of gold and silver', buyers: { foundry: 1.5, fences: 0.9 } },
    { id: 'toy_robot', name: 'Tin Toy Robot', stratum: 1, rarity: 'RARE', baseValue: 80, weight: 1, description: 'Pristine vintage toy, highly collectible', buyers: { curator: 2.0, fences: 1.2 } },
    { id: 'safe_box', name: 'Locked Safe', stratum: 1, rarity: 'RARE', baseValue: 120, weight: 3, description: 'Heavy and sealed — who knows what\'s inside?', buyers: { fences: 1.8, curator: 1.0 } },

    // === STRATUM 2: The Foundation (industrial era) ===
    { id: 'coal_chunk', name: 'Coal Chunk', stratum: 2, rarity: 'COMMON', baseValue: 10, weight: 2, description: 'Dense anthracite coal', buyers: { foundry: 1.3, fences: 0.6 } },
    { id: 'steel_beam', name: 'Steel Beam Fragment', stratum: 2, rarity: 'COMMON', baseValue: 15, weight: 3, description: 'Industrial-grade structural steel', buyers: { foundry: 1.5, fences: 0.5 } },
    { id: 'machine_gear', name: 'Machine Gear', stratum: 2, rarity: 'COMMON', baseValue: 18, weight: 2, description: 'Precision-cut iron gear', buyers: { foundry: 1.4, curator: 0.8 } },
    { id: 'iron_pipe', name: 'Iron Pipe', stratum: 2, rarity: 'COMMON', baseValue: 12, weight: 2, description: 'Corroded but solid iron piping', buyers: { foundry: 1.2, fences: 0.7 } },
    { id: 'old_coin', name: 'Old Currency', stratum: 2, rarity: 'UNCOMMON', baseValue: 40, weight: 1, description: 'Coins from a forgotten era of industry', buyers: { curator: 1.6, fences: 1.2 } },
    { id: 'blueprint', name: 'Factory Blueprint', stratum: 2, rarity: 'UNCOMMON', baseValue: 55, weight: 1, description: 'Technical drawings of industrial machinery', buyers: { curator: 1.5, foundry: 1.3 } },
    { id: 'pressure_gauge', name: 'Pressure Gauge', stratum: 2, rarity: 'UNCOMMON', baseValue: 30, weight: 1, description: 'Brass instrument, still functional', buyers: { foundry: 1.2, curator: 1.3 } },
    { id: 'steam_engine_part', name: 'Steam Engine Part', stratum: 2, rarity: 'RARE', baseValue: 150, weight: 3, description: 'A rare component from an early steam engine', buyers: { curator: 2.0, foundry: 1.5 } },
    { id: 'gold_pocket_watch', name: 'Gold Pocket Watch', stratum: 2, rarity: 'RARE', baseValue: 200, weight: 1, description: 'Exquisite craftsmanship, still ticking', buyers: { curator: 1.8, fences: 1.5 } },

    // === STRATUM 3: The Catacombs (medieval/colonial) ===
    { id: 'bone_fragment', name: 'Ancient Bone', stratum: 3, rarity: 'COMMON', baseValue: 15, weight: 1, description: 'Weathered human remains', buyers: { curator: 1.2, fences: 0.5 } },
    { id: 'clay_pottery', name: 'Clay Pottery', stratum: 3, rarity: 'COMMON', baseValue: 20, weight: 2, description: 'Hand-crafted earthenware shard', buyers: { curator: 1.4, fences: 0.6 } },
    { id: 'iron_nail', name: 'Iron Nails', stratum: 3, rarity: 'COMMON', baseValue: 12, weight: 1, description: 'Hand-forged iron nails', buyers: { foundry: 1.1, curator: 0.9 } },
    { id: 'leather_scrap', name: 'Ancient Leather', stratum: 3, rarity: 'COMMON', baseValue: 18, weight: 1, description: 'Preserved leather fragment', buyers: { curator: 1.2, fences: 0.8 } },
    { id: 'silver_ring', name: 'Silver Ring', stratum: 3, rarity: 'UNCOMMON', baseValue: 65, weight: 1, description: 'Tarnished silver band with inscription', buyers: { curator: 1.5, fences: 1.3 } },
    { id: 'sealed_urn', name: 'Sealed Urn', stratum: 3, rarity: 'UNCOMMON', baseValue: 75, weight: 2, description: 'Ceramic urn, contents unknown', buyers: { curator: 1.8, fences: 1.0 } },
    { id: 'rusty_sword', name: 'Medieval Sword', stratum: 3, rarity: 'UNCOMMON', baseValue: 90, weight: 2, description: 'Corroded blade, once a knight\'s weapon', buyers: { curator: 1.7, foundry: 1.0 } },
    { id: 'jeweled_cross', name: 'Jeweled Cross', stratum: 3, rarity: 'RARE', baseValue: 250, weight: 1, description: 'Gold cross studded with garnets', buyers: { curator: 2.2, fences: 1.5 } },
    { id: 'crown_fragment', name: 'Crown Fragment', stratum: 3, rarity: 'LEGENDARY', baseValue: 500, weight: 1, description: 'A piece of a royal crown, radiating history', buyers: { curator: 3.0, fences: 2.0 } },

    // === STRATUM 4: The Ruin (ancient civilization) ===
    { id: 'carved_stone', name: 'Carved Stone Block', stratum: 4, rarity: 'COMMON', baseValue: 25, weight: 3, description: 'Stone with ancient carvings', buyers: { curator: 1.3, foundry: 0.8 } },
    { id: 'clay_tablet', name: 'Clay Tablet', stratum: 4, rarity: 'COMMON', baseValue: 30, weight: 2, description: 'Inscribed with unknown script', buyers: { curator: 1.5, fences: 0.7 } },
    { id: 'bronze_tool', name: 'Bronze Tool', stratum: 4, rarity: 'COMMON', baseValue: 22, weight: 2, description: 'Ancient crafting implement', buyers: { foundry: 1.3, curator: 1.2 } },
    { id: 'obsidian_shard', name: 'Obsidian Shard', stratum: 4, rarity: 'COMMON', baseValue: 28, weight: 1, description: 'Razor-sharp volcanic glass', buyers: { foundry: 1.2, fences: 0.9 } },
    { id: 'gold_figurine', name: 'Gold Figurine', stratum: 4, rarity: 'UNCOMMON', baseValue: 120, weight: 1, description: 'Small golden deity statue', buyers: { curator: 2.0, fences: 1.5 } },
    { id: 'jade_amulet', name: 'Jade Amulet', stratum: 4, rarity: 'UNCOMMON', baseValue: 140, weight: 1, description: 'Polished jade with mystical engravings', buyers: { curator: 1.8, fences: 1.3 } },
    { id: 'temple_glyph', name: 'Temple Glyph', stratum: 4, rarity: 'UNCOMMON', baseValue: 100, weight: 2, description: 'Intact wall glyph from a lost temple', buyers: { curator: 2.0, fences: 0.8 } },
    { id: 'ritual_mask', name: 'Ritual Mask', stratum: 4, rarity: 'RARE', baseValue: 350, weight: 2, description: 'Ceremonial mask of gold and jade', buyers: { curator: 2.5, fences: 1.8 } },
    { id: 'void_crystal', name: 'Void Crystal', stratum: 4, rarity: 'LEGENDARY', baseValue: 800, weight: 1, description: 'A crystal that seems to absorb light itself', buyers: { curator: 2.5, fences: 2.5 } }
];

// Build lookup by id
export const ITEMS_BY_ID = {};
for (const item of ITEM_DEFS) {
    ITEMS_BY_ID[item.id] = item;
}

// Get items by stratum
export function getItemsForStratum(stratumId) {
    return ITEM_DEFS.filter(i => i.stratum === stratumId);
}

// Get rarity data for an item
export function getItemRarity(item) {
    return RARITY[item.rarity];
}

// Roll a random item for a given stratum
export function rollItem(stratumId) {
    const stratumItems = getItemsForStratum(stratumId);
    if (stratumItems.length === 0) return null;

    // Weight by rarity: common 60%, uncommon 25%, rare 12%, legendary 3%
    const weights = {
        COMMON: 60,
        UNCOMMON: 25,
        RARE: 12,
        LEGENDARY: 3
    };

    const weighted = [];
    for (const item of stratumItems) {
        weighted.push({ item, weight: weights[item.rarity] || 10 });
    }

    const totalWeight = weighted.reduce((sum, w) => sum + w.weight, 0);
    let roll = Math.random() * totalWeight;

    for (const { item, weight } of weighted) {
        roll -= weight;
        if (roll <= 0) return item;
    }

    return stratumItems[0];
}
