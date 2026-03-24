'use strict';
(function() {

// Game configuration constants
const TILE_SIZE = 32;
const WORLD_WIDTH = 60;  // tiles wide
const WORLD_HEIGHT = 200; // tiles tall (covers all 4 strata)
const SURFACE_HEIGHT = 5; // rows of sky/surface before underground

const CANVAS_WIDTH = 800;
const CANVAS_HEIGHT = 600;

// Strata definitions
const STRATA = [
    {
        id: 0,
        name: 'Surface',
        startDepth: 0,
        endDepth: SURFACE_HEIGHT,
        colors: { sky: '#87CEEB', ground: '#8B7355' },
        bgColor: '#87CEEB'
    },
    {
        id: 1,
        name: 'The Dump',
        startDepth: SURFACE_HEIGHT,
        endDepth: SURFACE_HEIGHT + 40,
        colors: {
            base: '#6B6B6B',
            accent1: '#8B7355',
            accent2: '#A0522D',
            dirt: '#5C4033'
        },
        bgColor: '#3D3D3D',
        hardness: 1,
        hazards: ['gas', 'unstable'],
        description: 'Modern refuse and topsoil'
    },
    {
        id: 2,
        name: 'The Foundation',
        startDepth: SURFACE_HEIGHT + 40,
        endDepth: SURFACE_HEIGHT + 90,
        colors: {
            base: '#4A4A5A',
            accent1: '#708090',
            accent2: '#8B4513',
            dirt: '#36454F'
        },
        bgColor: '#2A2A3A',
        hardness: 2,
        hazards: ['flood', 'rust'],
        description: 'Industrial era remains'
    },
    {
        id: 3,
        name: 'The Catacombs',
        startDepth: SURFACE_HEIGHT + 90,
        endDepth: SURFACE_HEIGHT + 150,
        colors: {
            base: '#2D1B2E',
            accent1: '#E8E8E0',
            accent2: '#6B3FA0',
            dirt: '#1A0A1A'
        },
        bgColor: '#1A0A1A',
        hardness: 3,
        hazards: ['cavein', 'curse', 'darkness'],
        description: 'Medieval burial grounds'
    },
    {
        id: 4,
        name: 'The Ruin',
        startDepth: SURFACE_HEIGHT + 150,
        endDepth: SURFACE_HEIGHT + 200,
        colors: {
            base: '#8B6914',
            accent1: '#DAA520',
            accent2: '#CD853F',
            dirt: '#5C4A1E'
        },
        bgColor: '#2A1A0A',
        hardness: 4,
        hazards: ['traps', 'guardians'],
        description: 'Ancient civilization temples'
    }
];

// Player defaults
const PLAYER = {
    startX: Math.floor(WORLD_WIDTH / 2),
    startY: SURFACE_HEIGHT - 2,
    maxStamina: 100,
    staminaDrainDig: 1.5,
    staminaDrainMove: 0.08,
    staminaRecoveryRate: 0.05,
    digSpeed: 1.0,
    moveSpeed: 5, // tiles per second
    inventorySlots: 10,
    headlampRadius: 7,
    headlampAngle: Math.PI / 3
};

// Economy
const ECONOMY = {
    priceFluctuationRate: 0.02,
    demandDecayRate: 0.01,
    supplyImpactFactor: 0.05,
    eventDuration: 5, // expeditions
    startingGold: 50
};

// Tile types
const TILES = {
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

// Game states
const GAME_STATES = {
    MENU: 'menu',
    SURFACE: 'surface',
    PREP: 'prep',
    DIGGING: 'digging',
    MARKET: 'market',
    UPGRADES: 'upgrades',
    CONTRACTS: 'contracts',
    EXPEDITION_SUMMARY: 'expedition_summary',
    INVENTORY: 'inventory'
};

// Rarity tiers
const RARITY = {
    COMMON: { name: 'Common', color: '#AAAAAA', multiplier: 1.0 },
    UNCOMMON: { name: 'Uncommon', color: '#4FC3F7', multiplier: 1.5 },
    RARE: { name: 'Rare', color: '#AB47BC', multiplier: 2.5 },
    LEGENDARY: { name: 'Legendary', color: '#FFD700', multiplier: 5.0 }
};

// Input handling - tracks keyboard state
const keys = {};
const justPressed = {};

function initInput() {
    window.addEventListener('keydown', (e) => {
        if (!keys[e.code]) {
            justPressed[e.code] = true;
        }
        keys[e.code] = true;
        // Prevent scrolling with arrow keys/space
        if (['ArrowUp', 'ArrowDown', 'ArrowLeft', 'ArrowRight', 'Space'].includes(e.code)) {
            e.preventDefault();
        }
    });

    window.addEventListener('keyup', (e) => {
        keys[e.code] = false;
    });
}

function isKeyDown(code) {
    return !!keys[code];
}

function isKeyJustPressed(code) {
    return !!justPressed[code];
}

function clearJustPressed() {
    for (const key in justPressed) {
        delete justPressed[key];
    }
}

// Mouse state
const mouse = { x: 0, y: 0, clicked: false, justClicked: false };

function initMouse(canvas) {
    canvas.addEventListener('mousemove', (e) => {
        const rect = canvas.getBoundingClientRect();
        mouse.x = e.clientX - rect.left;
        mouse.y = e.clientY - rect.top;
    });

    canvas.addEventListener('mousedown', (e) => {
        mouse.clicked = true;
        mouse.justClicked = true;
    });

    canvas.addEventListener('mouseup', () => {
        mouse.clicked = false;
    });
}

function getMouse() {
    return { ...mouse };
}

function clearMouseJustClicked() {
    mouse.justClicked = false;
}


// Item definitions for all 4 strata
// Each item: { id, name, stratum, rarity, baseValue, weight, description, buyers[] }
// buyers: which factions want this item and their interest multiplier

const ITEM_DEFS = [
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
const ITEMS_BY_ID = {};
for (const item of ITEM_DEFS) {
    ITEMS_BY_ID[item.id] = item;
}

// Get items by stratum
function getItemsForStratum(stratumId) {
    return ITEM_DEFS.filter(i => i.stratum === stratumId);
}

// Get rarity data for an item
function getItemRarity(item) {
    return RARITY[item.rarity];
}

// Roll a random item for a given stratum
function rollItem(stratumId) {
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


class ParticleSystem {
    constructor() {
        this.particles = [];
    }

    update(dt) {
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const p = this.particles[i];
            p.x += p.vx * dt;
            p.y += p.vy * dt;
            p.vy += (p.gravity || 80) * dt;
            p.life -= dt;
            p.alpha = Math.max(0, p.life / p.maxLife);

            if (p.life <= 0) {
                this.particles.splice(i, 1);
            }
        }
    }

    draw(ctx, camera) {
        const offset = camera.getOffset();
        for (const p of this.particles) {
            ctx.globalAlpha = p.alpha;
            ctx.fillStyle = p.color;
            const sx = p.x + offset.x;
            const sy = p.y + offset.y;
            ctx.fillRect(sx, sy, p.size, p.size);
        }
        ctx.globalAlpha = 1;
    }

    // Dig particles
    emitDig(tileX, tileY, color) {
        const cx = tileX * TILE_SIZE + TILE_SIZE / 2;
        const cy = tileY * TILE_SIZE + TILE_SIZE / 2;
        for (let i = 0; i < 8; i++) {
            this.particles.push({
                x: cx + (Math.random() - 0.5) * TILE_SIZE,
                y: cy + (Math.random() - 0.5) * TILE_SIZE,
                vx: (Math.random() - 0.5) * 100,
                vy: -Math.random() * 60 - 20,
                gravity: 120,
                size: 2 + Math.random() * 3,
                color: color,
                life: 0.3 + Math.random() * 0.4,
                maxLife: 0.7,
                alpha: 1
            });
        }
    }

    // Item found sparkle
    emitSparkle(tileX, tileY, color) {
        const cx = tileX * TILE_SIZE + TILE_SIZE / 2;
        const cy = tileY * TILE_SIZE + TILE_SIZE / 2;
        for (let i = 0; i < 12; i++) {
            const angle = (Math.PI * 2 * i) / 12;
            this.particles.push({
                x: cx,
                y: cy,
                vx: Math.cos(angle) * (60 + Math.random() * 40),
                vy: Math.sin(angle) * (60 + Math.random() * 40),
                gravity: 0,
                size: 2 + Math.random() * 2,
                color: color,
                life: 0.5 + Math.random() * 0.3,
                maxLife: 0.8,
                alpha: 1
            });
        }
    }

    // Coin cascade for selling
    emitCoins(screenX, screenY, count) {
        for (let i = 0; i < count; i++) {
            this.particles.push({
                x: screenX + (Math.random() - 0.5) * 40,
                y: screenY,
                vx: (Math.random() - 0.5) * 80,
                vy: -Math.random() * 150 - 50,
                gravity: 200,
                size: 3 + Math.random() * 2,
                color: Math.random() > 0.3 ? '#FFD700' : '#FFA500',
                life: 0.8 + Math.random() * 0.5,
                maxLife: 1.3,
                alpha: 1
            });
        }
    }
}


class Camera {
    constructor() {
        this.x = 0;
        this.y = 0;
        this.targetX = 0;
        this.targetY = 0;
        this.smoothing = 8;
        this.shake = 0;
        this.shakeDecay = 5;
    }

    follow(player, dt) {
        // Target: center on player
        this.targetX = player.x * TILE_SIZE - CANVAS_WIDTH / 2 + TILE_SIZE / 2;
        this.targetY = player.y * TILE_SIZE - CANVAS_HEIGHT / 2 + TILE_SIZE / 2;

        // Smooth follow
        this.x += (this.targetX - this.x) * this.smoothing * dt;
        this.y += (this.targetY - this.y) * this.smoothing * dt;

        // Clamp to world bounds
        this.x = Math.max(0, Math.min(this.x, WORLD_WIDTH * TILE_SIZE - CANVAS_WIDTH));
        this.y = Math.max(-CANVAS_HEIGHT / 3, Math.min(this.y, WORLD_HEIGHT * TILE_SIZE - CANVAS_HEIGHT));

        // Screen shake
        if (this.shake > 0) {
            this.shake -= this.shakeDecay * dt;
            if (this.shake < 0) this.shake = 0;
        }
    }

    addShake(amount) {
        this.shake = Math.min(this.shake + amount, 10);
    }

    getOffset() {
        let ox = -Math.round(this.x);
        let oy = -Math.round(this.y);
        if (this.shake > 0) {
            ox += (Math.random() - 0.5) * this.shake * 2;
            oy += (Math.random() - 0.5) * this.shake * 2;
        }
        return { x: ox, y: oy };
    }

    // Get visible tile range for culling
    getVisibleRange() {
        const startX = Math.max(0, Math.floor(this.x / TILE_SIZE) - 1);
        const startY = Math.max(0, Math.floor(this.y / TILE_SIZE) - 1);
        const endX = Math.min(WORLD_WIDTH, Math.ceil((this.x + CANVAS_WIDTH) / TILE_SIZE) + 1);
        const endY = Math.min(WORLD_HEIGHT, Math.ceil((this.y + CANVAS_HEIGHT) / TILE_SIZE) + 1);
        return { startX, startY, endX, endY };
    }
}


class World {
    constructor() {
        // 2D array: tiles[y][x]
        this.tiles = [];
        this.items = []; // items embedded in tiles: { x, y, itemDef }
        this.revealedItems = new Set(); // keys like "x,y" for items the player has uncovered
        this.generate();
    }

    generate() {
        this.tiles = [];
        this.items = [];
        this.revealedItems = new Set();

        for (let y = 0; y < WORLD_HEIGHT; y++) {
            this.tiles[y] = [];
            for (let x = 0; x < WORLD_WIDTH; x++) {
                this.tiles[y][x] = this.generateTile(x, y);
            }
        }

        // Place items throughout the world
        this.placeItems();
    }

    generateTile(x, y) {
        // Sky / surface
        if (y < SURFACE_HEIGHT - 1) {
            return { type: TILES.AIR, stratum: 0 };
        }

        // Surface grass
        if (y === SURFACE_HEIGHT - 1) {
            return { type: TILES.SURFACE_GRASS, stratum: 0 };
        }

        // Buildings on surface (market area)
        if (y === SURFACE_HEIGHT - 2) {
            const mid = Math.floor(WORLD_WIDTH / 2);
            if (x >= mid - 3 && x <= mid + 3) {
                return { type: TILES.BUILDING, stratum: 0 };
            }
        }

        // Underground - determine stratum
        const stratum = this.getStratumAt(y);
        if (!stratum) {
            return { type: TILES.BEDROCK, stratum: 0 };
        }

        // Generate terrain based on stratum with noise-like variation
        const noise = this.pseudoNoise(x, y);

        // Transition zones between strata
        const depthInStratum = (y - stratum.startDepth) / (stratum.endDepth - stratum.startDepth);

        if (noise > 0.92) {
            // Air pockets / caves
            return { type: TILES.AIR, stratum: stratum.id };
        }

        if (stratum.id >= 2 && noise > 0.88) {
            return { type: TILES.HARD_ROCK, stratum: stratum.id, hardness: stratum.hardness + 1 };
        }

        if (noise > 0.75) {
            return { type: TILES.ROCK, stratum: stratum.id, hardness: stratum.hardness };
        }

        // Hazards - sparse
        if (stratum.hazards && noise > 0.70 && noise < 0.73) {
            if (stratum.hazards.includes('gas') || stratum.hazards.includes('flood')) {
                return {
                    type: stratum.hazards.includes('gas') ? TILES.HAZARD_GAS : TILES.HAZARD_WATER,
                    stratum: stratum.id,
                    hardness: 1
                };
            }
        }

        return { type: TILES.DIRT, stratum: stratum.id, hardness: stratum.hardness };
    }

    placeItems() {
        // Place items based on stratum
        for (const stratum of STRATA) {
            if (stratum.id === 0) continue; // no items on surface

            const startY = stratum.startDepth;
            const endY = stratum.endDepth;

            // Item density increases with depth
            const density = 0.03 + stratum.id * 0.01;

            for (let y = startY; y < endY && y < WORLD_HEIGHT; y++) {
                for (let x = 0; x < WORLD_WIDTH; x++) {
                    const tile = this.tiles[y][x];
                    if (tile.type === TILES.DIRT || tile.type === TILES.ROCK) {
                        if (Math.random() < density) {
                            const itemDef = rollItem(stratum.id);
                            if (itemDef) {
                                this.items.push({ x, y, itemDef });
                                // Mark tile as containing an item
                                tile.type = TILES.ITEM;
                                tile.itemDef = itemDef;
                            }
                        }
                    }
                }
            }
        }
    }

    pseudoNoise(x, y) {
        // Simple deterministic noise using sin
        const n = Math.sin(x * 12.9898 + y * 78.233) * 43758.5453;
        return n - Math.floor(n);
    }

    getStratumAt(y) {
        for (const s of STRATA) {
            if (y >= s.startDepth && y < s.endDepth) return s;
        }
        return null;
    }

    getTile(x, y) {
        if (x < 0 || x >= WORLD_WIDTH || y < 0 || y >= WORLD_HEIGHT) {
            return { type: TILES.BEDROCK, stratum: 0 };
        }
        return this.tiles[y][x];
    }

    setTile(x, y, tile) {
        if (x >= 0 && x < WORLD_WIDTH && y >= 0 && y < WORLD_HEIGHT) {
            this.tiles[y][x] = tile;
        }
    }

    // Dig a tile, returns item if one was embedded
    digTile(x, y) {
        const tile = this.getTile(x, y);
        if (tile.type === TILES.AIR || tile.type === TILES.BEDROCK || tile.type === TILES.BUILDING) {
            return null;
        }

        let foundItem = null;
        if (tile.type === TILES.ITEM && tile.itemDef) {
            foundItem = { ...tile.itemDef };
        }

        // Replace with air
        this.tiles[y][x] = { type: TILES.AIR, stratum: tile.stratum };

        return foundItem;
    }

    isSolid(x, y) {
        const tile = this.getTile(x, y);
        return tile.type !== TILES.AIR;
    }

    isDiggable(x, y) {
        const tile = this.getTile(x, y);
        return tile.type === TILES.DIRT || tile.type === TILES.ROCK ||
               tile.type === TILES.HARD_ROCK || tile.type === TILES.ITEM ||
               tile.type === TILES.HAZARD_GAS || tile.type === TILES.HAZARD_WATER ||
               tile.type === TILES.SURFACE_GRASS;
    }

    // Get the required dig time for a tile (in seconds)
    getDigTime(x, y, digPower) {
        const tile = this.getTile(x, y);
        if (tile.type === TILES.SURFACE_GRASS) return 0.2 / digPower;
        const hardness = tile.hardness || 1;
        // Base: 0.3s for hardness 1, scales up
        return (0.2 + hardness * 0.15) / digPower;
    }
}


class Player {
    constructor(world) {
        this.world = world;
        this.x = PLAYER.startX;
        this.y = PLAYER.startY;
        this.vx = 0;
        this.vy = 0;
        this.onGround = false;
        this.facingRight = true;
        this.stamina = PLAYER.maxStamina;
        this.maxStamina = PLAYER.maxStamina;
        this.digPower = PLAYER.digSpeed;
        this.moveSpeed = PLAYER.moveSpeed;
        this.headlampRadius = PLAYER.headlampRadius;

        // Digging state
        this.digging = false;
        this.digTarget = null; // { x, y }
        this.digProgress = 0;
        this.digTime = 0;

        // Inventory
        this.inventorySlots = PLAYER.inventorySlots;
        this.inventory = [];
        this.gold = 0;

        // Visual
        this.animFrame = 0;
        this.animTimer = 0;

        // Underground flag
        this.isUnderground = false;
    }

    update(dt) {
        this.isUnderground = this.y >= SURFACE_HEIGHT;

        if (this.digging) {
            return this.updateDigging(dt);
        }

        this.handleMovement(dt);
        this.handleDigInput();
        this.applyGravity(dt);
        this.applyMovement(dt);

        // Stamina drain while underground
        if (this.isUnderground) {
            this.stamina -= PLAYER.staminaDrainMove * dt;
            if (this.stamina < 0) this.stamina = 0;
        }

        // Animation
        this.animTimer += dt;
        if (this.animTimer > 0.15) {
            this.animTimer = 0;
            this.animFrame = (this.animFrame + 1) % 4;
        }
    }

    handleMovement(dt) {
        this.vx = 0;

        if (isKeyDown('ArrowLeft') || isKeyDown('KeyA')) {
            this.vx = -this.moveSpeed;
            this.facingRight = false;
        }
        if (isKeyDown('ArrowRight') || isKeyDown('KeyD')) {
            this.vx = this.moveSpeed;
            this.facingRight = true;
        }
        if ((isKeyDown('ArrowUp') || isKeyDown('KeyW') || isKeyDown('Space')) && this.onGround) {
            this.vy = -8;
            this.onGround = false;
        }
    }

    handleDigInput() {
        // Dig down
        if (isKeyDown('ArrowDown') || isKeyDown('KeyS')) {
            const tx = Math.round(this.x);
            const ty = Math.round(this.y) + 1;
            if (this.world.isDiggable(tx, ty) && this.stamina > 0) {
                this.startDig(tx, ty);
                return;
            }
        }

        // Dig left/right (hold shift + direction, or just direction into a wall)
        if (isKeyDown('ShiftLeft') || isKeyDown('ShiftRight')) {
            if (isKeyDown('ArrowLeft') || isKeyDown('KeyA')) {
                const tx = Math.round(this.x) - 1;
                const ty = Math.round(this.y);
                if (this.world.isDiggable(tx, ty) && this.stamina > 0) {
                    this.startDig(tx, ty);
                    return;
                }
            }
            if (isKeyDown('ArrowRight') || isKeyDown('KeyD')) {
                const tx = Math.round(this.x) + 1;
                const ty = Math.round(this.y);
                if (this.world.isDiggable(tx, ty) && this.stamina > 0) {
                    this.startDig(tx, ty);
                    return;
                }
            }
        }
    }

    startDig(tx, ty) {
        this.digging = true;
        this.digTarget = { x: tx, y: ty };
        this.digProgress = 0;
        this.digTime = this.world.getDigTime(tx, ty, this.digPower);
    }

    updateDigging(dt) {
        this.digProgress += dt;
        this.stamina -= PLAYER.staminaDrainDig * dt;

        if (this.stamina <= 0) {
            this.stamina = 0;
            this.digging = false;
            this.digTarget = null;
            return null;
        }

        if (this.digProgress >= this.digTime) {
            // Complete the dig
            const item = this.world.digTile(this.digTarget.x, this.digTarget.y);
            this.digging = false;
            // Keep digTarget so game.js can read it for particle position
            this.lastDigTarget = { ...this.digTarget };
            this.digTarget = null;

            if (item && this.inventory.length < this.inventorySlots) {
                this.inventory.push(item);
                return item; // Signal that an item was found
            }

            return item ? 'full' : 'dug';
        }

        return null;
    }

    applyGravity(dt) {
        if (!this.onGround) {
            this.vy += 20 * dt; // gravity
            if (this.vy > 12) this.vy = 12; // terminal velocity
        }
    }

    applyMovement(dt) {
        // Horizontal movement
        const newX = this.x + this.vx * dt;
        const tileX = Math.round(newX);
        const tileY = Math.round(this.y);

        if (!this.world.isSolid(tileX, tileY) && newX >= 0 && newX < WORLD_WIDTH) {
            this.x = newX;
        }

        // Vertical movement
        const newY = this.y + this.vy * dt;
        const checkY = Math.round(newY);

        if (this.vy >= 0) {
            // Falling - check below
            if (this.world.isSolid(Math.round(this.x), checkY)) {
                this.y = checkY - 1;
                this.vy = 0;
                this.onGround = true;
            } else {
                this.y = newY;
                this.onGround = false;
            }
        } else {
            // Jumping - check above
            if (this.world.isSolid(Math.round(this.x), checkY)) {
                this.vy = 0;
            } else {
                this.y = newY;
            }
        }
    }

    isAtSurface() {
        return this.y < SURFACE_HEIGHT;
    }

    getDepthMeters() {
        if (this.y <= SURFACE_HEIGHT) return 0;
        return Math.floor((this.y - SURFACE_HEIGHT) * 3); // 3 meters per tile
    }

    recoverStamina(amount) {
        this.stamina = Math.min(this.maxStamina, this.stamina + amount);
    }
}


// Buyer factions
const FACTIONS = {
    curator: {
        id: 'curator',
        name: 'The Curator',
        description: 'Museum collector — pays top price for artifacts, fossils, and complete sets',
        color: '#E8B04A',
        icon: '🏛',
        baseMultiplier: 1.0,
        specialty: 'Historical artifacts and complete collections'
    },
    foundry: {
        id: 'foundry',
        name: 'The Foundry',
        description: 'Blacksmiths and engineers — wants metals, alloys, and machine parts',
        color: '#FF6B35',
        icon: '⚒',
        baseMultiplier: 1.0,
        specialty: 'Metals, ores, and industrial materials'
    },
    fences: {
        id: 'fences',
        name: "Finn's Fences",
        description: 'Black market — buys anything, no questions asked, but pays less',
        color: '#7B68EE',
        icon: '🎭',
        baseMultiplier: 0.7,
        specialty: 'Anything goes — convenience over price'
    }
};

class Economy {
    constructor() {
        // Price modifiers per item type (fluctuate over time)
        this.priceModifiers = {};
        // Supply tracking: how much of each item has been sold
        this.supplyHistory = {};
        // Demand shifts
        this.demandBoosts = {};
        // Faction reputation: 0-100
        this.reputation = {
            curator: 0,
            foundry: 0,
            fences: 0
        };
        // Market events
        this.activeEvent = null;
        this.eventTimer = 0;
        // Price history for the market board
        this.priceHistory = []; // { item, faction, price, expedition }
        this.expeditionCount = 0;

        this.initPrices();
        this.rollEvent();
    }

    initPrices() {
        for (const id in ITEMS_BY_ID) {
            this.priceModifiers[id] = 0.9 + Math.random() * 0.2; // 0.9-1.1
            this.supplyHistory[id] = 0;
            this.demandBoosts[id] = 0;
        }
    }

    // Get the current price a faction will pay for an item
    getPrice(itemDef, factionId) {
        const faction = FACTIONS[factionId];
        if (!faction) return 0;

        const buyerInterest = itemDef.buyers[factionId] || 0;
        if (buyerInterest === 0) return 0; // Faction doesn't want this item

        const basePrice = itemDef.baseValue;
        const rarityMult = getItemRarity(itemDef)?.multiplier || 1;
        const priceMod = this.priceModifiers[itemDef.id] || 1;
        const factionMult = faction.baseMultiplier;
        const repBonus = this.getRepBonus(factionId);
        const demandBoost = this.demandBoosts[itemDef.id] || 0;
        const eventMult = this.getEventMultiplier(itemDef, factionId);

        // Supply depression: more sold = lower price
        const supplySold = this.supplyHistory[itemDef.id] || 0;
        const supplyPenalty = 1 / (1 + supplySold * ECONOMY.supplyImpactFactor);

        const price = basePrice * rarityMult * priceMod * buyerInterest * factionMult *
                      (1 + repBonus) * (1 + demandBoost) * supplyPenalty * eventMult;

        return Math.max(1, Math.round(price));
    }

    // Get best price across all factions
    getBestPrice(itemDef) {
        let best = { price: 0, factionId: null };
        for (const fid in FACTIONS) {
            const price = this.getPrice(itemDef, fid);
            if (price > best.price) {
                best = { price, factionId: fid };
            }
        }
        return best;
    }

    // Sell an item to a faction
    sell(itemDef, factionId) {
        const price = this.getPrice(itemDef, factionId);
        if (price <= 0) return 0;

        // Track supply
        this.supplyHistory[itemDef.id] = (this.supplyHistory[itemDef.id] || 0) + 1;

        // Gain reputation
        const repGain = Math.ceil(price / 50);
        this.reputation[factionId] = Math.min(100, this.reputation[factionId] + repGain);

        // Record price history
        this.priceHistory.push({
            item: itemDef.id,
            faction: factionId,
            price,
            expedition: this.expeditionCount
        });
        if (this.priceHistory.length > 50) this.priceHistory.shift();

        return price;
    }

    getRepBonus(factionId) {
        const rep = this.reputation[factionId] || 0;
        if (rep >= 80) return 0.40; // Patron
        if (rep >= 60) return 0.30; // Partner
        if (rep >= 40) return 0.20; // Preferred
        if (rep >= 20) return 0.10; // Trusted
        return 0;
    }

    getRepTier(factionId) {
        const rep = this.reputation[factionId] || 0;
        if (rep >= 80) return { name: 'Patron', color: '#FFD700' };
        if (rep >= 60) return { name: 'Partner', color: '#AB47BC' };
        if (rep >= 40) return { name: 'Preferred', color: '#4FC3F7' };
        if (rep >= 20) return { name: 'Trusted', color: '#81C784' };
        return { name: 'Neutral', color: '#AAA' };
    }

    // Market events
    rollEvent() {
        const events = [
            { name: "Founder's Festival", description: 'Historical artifacts in high demand!', boosts: { curator: 1.5 }, items: ['old_radio', 'gold_pocket_watch', 'jeweled_cross', 'crown_fragment', 'ritual_mask'] },
            { name: 'Industrial Boom', description: 'The Foundry needs materials!', boosts: { foundry: 1.4 }, items: ['scrap_metal', 'steel_beam', 'machine_gear', 'coal_chunk', 'steam_engine_part'] },
            { name: 'Black Market Bonanza', description: "Finn's paying extra for everything", boosts: { fences: 1.6 }, items: ['lost_wallet', 'safe_box', 'old_coin'] },
            { name: 'Ancient Discovery', description: 'Scholars crave artifacts from the deep', boosts: { curator: 1.8 }, items: ['gold_figurine', 'jade_amulet', 'temple_glyph', 'void_crystal'] },
            null // No event
        ];

        this.activeEvent = events[Math.floor(Math.random() * events.length)];
        this.eventTimer = ECONOMY.eventDuration;
    }

    getEventMultiplier(itemDef, factionId) {
        if (!this.activeEvent) return 1;
        let mult = 1;
        if (this.activeEvent.boosts[factionId]) {
            mult *= this.activeEvent.boosts[factionId];
        }
        if (this.activeEvent.items && this.activeEvent.items.includes(itemDef.id)) {
            mult *= 1.3;
        }
        return mult;
    }

    // Called between expeditions
    advanceMarket() {
        this.expeditionCount++;

        // Fluctuate prices
        for (const id in this.priceModifiers) {
            this.priceModifiers[id] += (Math.random() - 0.5) * ECONOMY.priceFluctuationRate * 2;
            this.priceModifiers[id] = Math.max(0.6, Math.min(1.5, this.priceModifiers[id]));
        }

        // Decay supply impact
        for (const id in this.supplyHistory) {
            this.supplyHistory[id] = Math.max(0, this.supplyHistory[id] - 1);
        }

        // Demand shifts
        for (const id in this.demandBoosts) {
            this.demandBoosts[id] *= (1 - ECONOMY.demandDecayRate);
            // Random demand spikes
            if (Math.random() < 0.05) {
                this.demandBoosts[id] += 0.2 + Math.random() * 0.3;
            }
        }

        // Event timer
        if (this.eventTimer > 0) {
            this.eventTimer--;
            if (this.eventTimer <= 0) {
                this.rollEvent();
            }
        }
    }

    // Get price trend for an item (up, down, or stable)
    getPriceTrend(itemId) {
        const recent = this.priceHistory.filter(h => h.item === itemId);
        if (recent.length < 2) return 'stable';
        const last = recent[recent.length - 1].price;
        const prev = recent[recent.length - 2].price;
        if (last > prev * 1.1) return 'up';
        if (last < prev * 0.9) return 'down';
        return 'stable';
    }
}

// Upgrade tree definitions and state management
// Implementing 3 trees: Excavation, Logistics, Commerce (as specified in plan)

const UPGRADE_TREES = {
    excavation: {
        name: 'Excavation',
        description: 'How you dig',
        color: '#FF6B35',
        tiers: [
            {
                tier: 1,
                name: 'Basic Pickaxe',
                cost: 0,
                effect: { digSpeed: 1.0 },
                description: 'Standard mining pickaxe',
                auto: true
            },
            {
                tier: 2,
                choices: [
                    {
                        id: 'pneumatic_hammer',
                        name: 'Pneumatic Hammer',
                        branch: 'Power',
                        cost: 150,
                        effect: { digSpeed: 2.0, itemDamageChance: 0.2 },
                        description: '2x dig speed, but 20% chance to damage items'
                    },
                    {
                        id: 'archaeologist_kit',
                        name: "Archaeologist's Kit",
                        branch: 'Precision',
                        cost: 200,
                        effect: { digSpeed: 0.8, itemQualityBonus: 1.5 },
                        description: 'Slower dig, but items worth 50% more'
                    },
                    {
                        id: 'blast_charges',
                        name: 'Blast Charges',
                        branch: 'Range',
                        cost: 175,
                        effect: { digSpeed: 1.5, aoeRadius: 1 },
                        description: 'Clear 3x3 areas, destroys fragile items'
                    }
                ]
            },
            {
                tier: 3,
                choices: [
                    {
                        id: 'thermal_lance',
                        name: 'Thermal Lance',
                        branch: 'Power',
                        requires: 'pneumatic_hammer',
                        cost: 500,
                        effect: { digSpeed: 3.0, canDigHardRock: true },
                        description: 'Cuts through any material instantly'
                    },
                    {
                        id: 'laser_scalpel',
                        name: 'Laser Scalpel',
                        branch: 'Precision',
                        requires: 'archaeologist_kit',
                        cost: 600,
                        effect: { digSpeed: 1.0, itemQualityBonus: 2.0, neverDamage: true },
                        description: 'Surgical extraction, never damages items'
                    },
                    {
                        id: 'tunnel_bore',
                        name: 'Tunnel Bore',
                        branch: 'Range',
                        requires: 'blast_charges',
                        cost: 550,
                        effect: { digSpeed: 2.5, aoeRadius: 2 },
                        description: 'Drills wide tunnels automatically'
                    }
                ]
            },
            {
                tier: 4,
                choices: [
                    {
                        id: 'worldbreaker',
                        name: 'The Worldbreaker',
                        branch: 'Power',
                        requires: 'thermal_lance',
                        cost: 2000,
                        effect: { digSpeed: 5.0, aoeRadius: 3 },
                        description: 'Destroys entire layers — massive yield, no quality control'
                    },
                    {
                        id: 'quantum_extractor',
                        name: 'Quantum Extractor',
                        branch: 'Precision',
                        requires: 'laser_scalpel',
                        cost: 2500,
                        effect: { digSpeed: 2.0, itemQualityBonus: 3.0, neverDamage: true },
                        description: 'Phases items out of rock perfectly'
                    },
                    {
                        id: 'the_colony',
                        name: 'The Colony',
                        branch: 'Range',
                        requires: 'tunnel_bore',
                        cost: 2200,
                        effect: { digSpeed: 3.0, autoDig: true },
                        description: 'Autonomous mini-drills dig while you explore'
                    }
                ]
            }
        ]
    },

    logistics: {
        name: 'Logistics',
        description: 'How you carry and move',
        color: '#4FC3F7',
        tiers: [
            {
                tier: 1,
                name: 'Canvas Backpack',
                cost: 0,
                effect: { inventorySlots: 10 },
                description: '10 inventory slots',
                auto: true
            },
            {
                tier: 2,
                choices: [
                    {
                        id: 'reinforced_pack',
                        name: 'Reinforced Pack',
                        branch: 'Capacity',
                        cost: 120,
                        effect: { inventorySlots: 18 },
                        description: '18 slots, can carry heavy items'
                    },
                    {
                        id: 'grapple_line',
                        name: 'Grapple Line',
                        branch: 'Speed',
                        cost: 150,
                        effect: { inventorySlots: 12, moveSpeed: 6 },
                        description: 'Faster return to surface, 12 slots'
                    },
                    {
                        id: 'supply_drone',
                        name: 'Supply Drone',
                        branch: 'Automation',
                        cost: 200,
                        effect: { inventorySlots: 14, autoSend: true },
                        description: 'Auto-sends items to surface every 5 min'
                    }
                ]
            },
            {
                tier: 3,
                choices: [
                    {
                        id: 'cargo_mech',
                        name: 'Cargo Mech',
                        branch: 'Capacity',
                        requires: 'reinforced_pack',
                        cost: 450,
                        effect: { inventorySlots: 30, moveSpeed: 3 },
                        description: '30 slots but slower movement'
                    },
                    {
                        id: 'pneumatic_tubes',
                        name: 'Pneumatic Tubes',
                        branch: 'Speed',
                        requires: 'grapple_line',
                        cost: 500,
                        effect: { inventorySlots: 14, instantReturn: true },
                        description: 'Items shoot to surface instantly'
                    },
                    {
                        id: 'conveyor_system',
                        name: 'Conveyor System',
                        branch: 'Automation',
                        requires: 'supply_drone',
                        cost: 550,
                        effect: { inventorySlots: 20, autoSend: true, autoInterval: 120 },
                        description: 'Permanent conveyor belts in tunnels'
                    }
                ]
            },
            {
                tier: 4,
                choices: [
                    {
                        id: 'pocket_dimension',
                        name: 'Pocket Dimension',
                        branch: 'Capacity',
                        requires: 'cargo_mech',
                        cost: 1800,
                        effect: { inventorySlots: 50 },
                        description: '50 slots — carry a museum\'s worth of loot'
                    },
                    {
                        id: 'teleport_pad',
                        name: 'Teleport Pad',
                        branch: 'Speed',
                        requires: 'pneumatic_tubes',
                        cost: 2000,
                        effect: { inventorySlots: 20, teleport: true },
                        description: 'Teleport between surface and depth at will'
                    },
                    {
                        id: 'logistics_ai',
                        name: 'The Logistics AI',
                        branch: 'Automation',
                        requires: 'conveyor_system',
                        cost: 2200,
                        effect: { inventorySlots: 30, autoSend: true, autoSort: true },
                        description: 'Fully automated sorting and delivery'
                    }
                ]
            }
        ]
    },

    commerce: {
        name: 'Commerce',
        description: 'How you sell',
        color: '#FFD700',
        tiers: [
            {
                tier: 1,
                name: 'Market Stall',
                cost: 0,
                effect: { sellBonus: 0 },
                description: 'Basic market access',
                auto: true
            },
            {
                tier: 2,
                choices: [
                    {
                        id: 'jewelers_loupe',
                        name: "Jeweler's Loupe",
                        branch: 'Appraisal',
                        cost: 130,
                        effect: { sellBonus: 0.1, revealItems: true },
                        description: '10% better prices, see item hints in terrain'
                    },
                    {
                        id: 'silver_tongue',
                        name: 'Silver Tongue',
                        branch: 'Negotiation',
                        cost: 160,
                        effect: { sellBonus: 0.15 },
                        description: '15% price bonus on all sales'
                    },
                    {
                        id: 'trade_contacts',
                        name: 'Trade Contacts',
                        branch: 'Network',
                        cost: 180,
                        effect: { sellBonus: 0.05, extraBuyers: true },
                        description: 'Unlock traveling merchants with special offers'
                    }
                ]
            },
            {
                tier: 3,
                choices: [
                    {
                        id: 'master_appraiser',
                        name: 'Master Appraiser',
                        branch: 'Appraisal',
                        requires: 'jewelers_loupe',
                        cost: 500,
                        effect: { sellBonus: 0.25, instantAppraisal: true },
                        description: 'Items auto-appraised, +25% value'
                    },
                    {
                        id: 'market_manipulation',
                        name: 'Market Manipulation',
                        branch: 'Negotiation',
                        requires: 'silver_tongue',
                        cost: 600,
                        effect: { sellBonus: 0.3, canManipulate: true },
                        description: '30% bonus + artificially inflate prices'
                    },
                    {
                        id: 'auction_house',
                        name: 'Auction House',
                        branch: 'Network',
                        requires: 'trade_contacts',
                        cost: 650,
                        effect: { sellBonus: 0.15, auction: true },
                        description: 'Auction items for chance of bidding wars (up to 3x)'
                    }
                ]
            },
            {
                tier: 4,
                choices: [
                    {
                        id: 'the_connoisseur',
                        name: 'The Connoisseur',
                        branch: 'Appraisal',
                        requires: 'master_appraiser',
                        cost: 2000,
                        effect: { sellBonus: 0.4, showBestItem: true },
                        description: 'Auto-identifies the most valuable item per stratum'
                    },
                    {
                        id: 'monopoly',
                        name: 'Monopoly',
                        branch: 'Negotiation',
                        requires: 'market_manipulation',
                        cost: 2500,
                        effect: { sellBonus: 0.5 },
                        description: 'Exclusive contracts — factions pay 2x'
                    },
                    {
                        id: 'trade_empire',
                        name: 'The Trade Empire',
                        branch: 'Network',
                        requires: 'auction_house',
                        cost: 2200,
                        effect: { sellBonus: 0.3, passiveIncome: true },
                        description: 'Passive income from merchant network'
                    }
                ]
            }
        ]
    }
};

class UpgradeManager {
    constructor() {
        // Track purchased upgrades
        this.purchased = new Set();
        // Active effects (aggregated from purchased upgrades)
        this.effects = {
            digSpeed: 1.0,
            inventorySlots: 10,
            moveSpeed: 4,
            sellBonus: 0,
            itemQualityBonus: 1.0,
            neverDamage: false,
            aoeRadius: 0,
            headlampRadius: 6
        };
    }

    canPurchase(upgradeId, gold) {
        if (this.purchased.has(upgradeId)) return false;

        // Find the upgrade definition
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const choice of tier.choices) {
                        if (choice.id === upgradeId) {
                            // Check requirements
                            if (choice.requires && !this.purchased.has(choice.requires)) return false;
                            // Check we haven't bought another choice from same tier
                            const sametierchoices = tier.choices.filter(c => c.id !== upgradeId);
                            for (const other of sametierchoices) {
                                if (this.purchased.has(other.id)) return false;
                            }
                            // Check gold
                            return gold >= choice.cost;
                        }
                    }
                }
            }
        }
        return false;
    }

    purchase(upgradeId) {
        this.purchased.add(upgradeId);
        this.recalcEffects();
        return this.getCost(upgradeId);
    }

    getCost(upgradeId) {
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const choice of tier.choices) {
                        if (choice.id === upgradeId) return choice.cost;
                    }
                }
            }
        }
        return 0;
    }

    recalcEffects() {
        // Reset to defaults
        this.effects = {
            digSpeed: 1.0,
            inventorySlots: 10,
            moveSpeed: 4,
            sellBonus: 0,
            itemQualityBonus: 1.0,
            neverDamage: false,
            aoeRadius: 0,
            headlampRadius: 6
        };

        // Apply all purchased upgrade effects
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const choice of tier.choices) {
                        if (this.purchased.has(choice.id)) {
                            this.applyEffect(choice.effect);
                        }
                    }
                }
            }
        }
    }

    applyEffect(effect) {
        if (effect.digSpeed !== undefined) this.effects.digSpeed = effect.digSpeed;
        if (effect.inventorySlots !== undefined) this.effects.inventorySlots = effect.inventorySlots;
        if (effect.moveSpeed !== undefined) this.effects.moveSpeed = effect.moveSpeed;
        if (effect.sellBonus !== undefined) this.effects.sellBonus = Math.max(this.effects.sellBonus, effect.sellBonus);
        if (effect.itemQualityBonus !== undefined) this.effects.itemQualityBonus = effect.itemQualityBonus;
        if (effect.neverDamage) this.effects.neverDamage = true;
        if (effect.aoeRadius !== undefined) this.effects.aoeRadius = Math.max(this.effects.aoeRadius, effect.aoeRadius);
    }

    // Apply effects to player
    applyToPlayer(player) {
        player.digPower = this.effects.digSpeed;
        player.inventorySlots = this.effects.inventorySlots;
        player.moveSpeed = this.effects.moveSpeed;
    }

    // Get current tier for a tree
    getCurrentTier(treeId) {
        const tree = UPGRADE_TREES[treeId];
        if (!tree) return 1;
        let maxTier = 1;
        for (const tier of tree.tiers) {
            if (tier.choices) {
                for (const choice of tier.choices) {
                    if (this.purchased.has(choice.id)) {
                        maxTier = Math.max(maxTier, tier.tier);
                    }
                }
            }
        }
        return maxTier;
    }

    // Get available upgrades for a tree
    getAvailableUpgrades(treeId, gold) {
        const tree = UPGRADE_TREES[treeId];
        if (!tree) return [];

        const available = [];
        for (const tier of tree.tiers) {
            if (tier.choices) {
                for (const choice of tier.choices) {
                    if (!this.purchased.has(choice.id)) {
                        const canBuy = this.canPurchase(choice.id, gold);
                        const meetsReqs = !choice.requires || this.purchased.has(choice.requires);
                        // Check no other choice in same tier is bought
                        const tierBlocked = tier.choices.some(c => c.id !== choice.id && this.purchased.has(c.id));
                        if (meetsReqs && !tierBlocked) {
                            available.push({ ...choice, canAfford: gold >= choice.cost, treeId });
                        }
                    }
                }
            }
        }
        return available;
    }
}


class ContractManager {
    constructor() {
        this.contracts = [];
        this.completedContracts = 0;
        this.generateContracts();
    }

    generateContracts() {
        this.contracts = [];
        // Generate 3-5 contracts
        const count = 3 + Math.floor(Math.random() * 3);
        for (let i = 0; i < count; i++) {
            this.contracts.push(this.createContract());
        }
    }

    createContract() {
        const factionIds = Object.keys(FACTIONS);
        const factionId = factionIds[Math.floor(Math.random() * factionIds.length)];
        const faction = FACTIONS[factionId];

        // Pick a random item from strata 1-4
        const stratum = 1 + Math.floor(Math.random() * 4);
        const stratumItems = getItemsForStratum(stratum);
        const targetItem = stratumItems[Math.floor(Math.random() * stratumItems.length)];

        // Quantity scales with item rarity
        const rarityQuantities = { COMMON: [3, 6], UNCOMMON: [2, 4], RARE: [1, 2], LEGENDARY: [1, 1] };
        const range = rarityQuantities[targetItem.rarity] || [1, 3];
        const quantity = range[0] + Math.floor(Math.random() * (range[1] - range[0] + 1));

        // Reward: base value * quantity * bonus multiplier
        const reward = Math.round(targetItem.baseValue * quantity * (1.5 + Math.random()));
        const repReward = 5 + Math.floor(Math.random() * 10);

        return {
            id: `contract_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`,
            factionId,
            factionName: faction.name,
            factionColor: faction.color,
            targetItemId: targetItem.id,
            targetItemName: targetItem.name,
            quantity,
            collected: 0,
            reward,
            repReward,
            description: `${faction.name} wants ${quantity}x ${targetItem.name}`,
            active: false,
            completed: false,
            expires: 5 // Expires in 5 expeditions
        };
    }

    acceptContract(contractId) {
        const contract = this.contracts.find(c => c.id === contractId);
        if (contract && !contract.active && !contract.completed) {
            contract.active = true;
            return true;
        }
        return false;
    }

    // Check if selling an item contributes to any active contract
    checkSale(itemDef, factionId) {
        const contributions = [];
        for (const contract of this.contracts) {
            if (contract.active && !contract.completed &&
                contract.targetItemId === itemDef.id &&
                contract.factionId === factionId) {
                contract.collected++;
                if (contract.collected >= contract.quantity) {
                    contract.completed = true;
                    this.completedContracts++;
                    contributions.push({ contract, completed: true });
                } else {
                    contributions.push({ contract, completed: false });
                }
            }
        }
        return contributions;
    }

    // Get reward for completed contracts
    claimRewards() {
        let totalGold = 0;
        let repGains = {};

        for (const contract of this.contracts) {
            if (contract.completed && contract.active) {
                totalGold += contract.reward;
                repGains[contract.factionId] = (repGains[contract.factionId] || 0) + contract.repReward;
                contract.claimed = true;
            }
        }

        // Remove claimed contracts
        this.contracts = this.contracts.filter(c => !c.claimed);

        return { totalGold, repGains };
    }

    // Called between expeditions
    advanceContracts() {
        // Decrement expiry on active contracts
        for (const contract of this.contracts) {
            if (contract.active && !contract.completed) {
                contract.expires--;
                if (contract.expires <= 0) {
                    contract.expired = true;
                }
            }
        }

        // Remove expired contracts
        this.contracts = this.contracts.filter(c => !c.expired);

        // Generate new contracts to fill gaps
        while (this.contracts.length < 3) {
            this.contracts.push(this.createContract());
        }
    }

    getActiveContracts() {
        return this.contracts.filter(c => c.active && !c.completed);
    }

    getAvailableContracts() {
        return this.contracts.filter(c => !c.active && !c.completed);
    }
}

// Procedural audio using Web Audio API
class AudioManager {
    constructor() {
        this.ctx = null;
        this.enabled = true;
        this.masterGain = null;
        this.initialized = false;
    }

    init() {
        try {
            this.ctx = new (window.AudioContext || window.webkitAudioContext)();
            this.masterGain = this.ctx.createGain();
            this.masterGain.gain.value = 0.3;
            this.masterGain.connect(this.ctx.destination);
            this.initialized = true;
        } catch (e) {
            this.enabled = false;
        }
    }

    ensureResumed() {
        if (this.ctx && this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
    }

    playDig(hardness) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();
        const now = this.ctx.currentTime;

        // Lower pitch for harder materials
        const baseFreq = 200 - hardness * 30;
        osc.frequency.setValueAtTime(baseFreq + Math.random() * 50, now);
        osc.frequency.exponentialRampToValueAtTime(baseFreq * 0.5, now + 0.1);
        osc.type = 'square';

        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.12);
    }

    playItemFound(rarity) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;

        // Pleasant ascending arpeggio
        const notes = [523, 659, 784]; // C5, E5, G5
        if (rarity === 'RARE') notes.push(1047); // C6
        if (rarity === 'LEGENDARY') { notes.push(1047, 1319); } // C6, E6

        notes.forEach((freq, i) => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            const t = now + i * 0.08;

            osc.frequency.value = freq;
            osc.type = 'sine';

            gain.gain.setValueAtTime(0, t);
            gain.gain.linearRampToValueAtTime(0.12, t + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.25);

            osc.connect(gain);
            gain.connect(this.masterGain);
            osc.start(t);
            osc.stop(t + 0.25);
        });
    }

    playSell(amount) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        const count = Math.min(5, Math.ceil(amount / 50));

        for (let i = 0; i < count; i++) {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            const t = now + i * 0.06;

            osc.frequency.value = 800 + i * 100 + Math.random() * 50;
            osc.type = 'sine';

            gain.gain.setValueAtTime(0.1, t);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.15);

            osc.connect(gain);
            gain.connect(this.masterGain);
            osc.start(t);
            osc.stop(t + 0.15);
        }
    }

    playUpgrade() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        // Fanfare: C-E-G-C ascending
        const notes = [262, 330, 392, 523];

        notes.forEach((freq, i) => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            const t = now + i * 0.12;

            osc.frequency.value = freq;
            osc.type = 'triangle';

            gain.gain.setValueAtTime(0.15, t);
            gain.gain.linearRampToValueAtTime(0.1, t + 0.1);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.4);

            osc.connect(gain);
            gain.connect(this.masterGain);
            osc.start(t);
            osc.stop(t + 0.4);
        });
    }

    playAmbient(stratum) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        // Create a subtle ambient drone based on depth
        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        const baseFreq = 80 - stratum * 10;
        osc.frequency.value = Math.max(30, baseFreq);
        osc.type = 'sine';

        gain.gain.setValueAtTime(0, now);
        gain.gain.linearRampToValueAtTime(0.03, now + 1);
        gain.gain.linearRampToValueAtTime(0, now + 3);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 3);
    }

    playClick() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.frequency.value = 600;
        osc.type = 'square';

        gain.gain.setValueAtTime(0.08, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.05);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.05);
    }

    playHazard() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.frequency.setValueAtTime(300, now);
        osc.frequency.linearRampToValueAtTime(100, now + 0.3);
        osc.type = 'sawtooth';

        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.3);
    }

    playReturn() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        // Rising sweep
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.frequency.setValueAtTime(200, now);
        osc.frequency.exponentialRampToValueAtTime(800, now + 0.5);
        osc.type = 'sine';

        gain.gain.setValueAtTime(0.1, now);
        gain.gain.linearRampToValueAtTime(0.15, now + 0.3);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.6);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.6);
    }

    toggle() {
        this.enabled = !this.enabled;
        if (this.masterGain) {
            this.masterGain.gain.value = this.enabled ? 0.3 : 0;
        }
        return this.enabled;
    }
}


class Renderer {
    constructor(ctx) {
        this.ctx = ctx;
        // Pre-render tile textures
        this.tileCache = new Map();
        this.lightCanvas = document.createElement('canvas');
        this.lightCanvas.width = CANVAS_WIDTH;
        this.lightCanvas.height = CANVAS_HEIGHT;
        this.lightCtx = this.lightCanvas.getContext('2d');
    }

    clear() {
        this.ctx.fillStyle = '#1a1a2e';
        this.ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
    }

    drawWorld(world, camera, player) {
        const offset = camera.getOffset();
        const range = camera.getVisibleRange();

        // Draw sky background
        this.drawSky(offset);

        // Draw tiles
        for (let y = range.startY; y < range.endY; y++) {
            for (let x = range.startX; x < range.endX; x++) {
                const tile = world.getTile(x, y);
                const sx = x * TILE_SIZE + offset.x;
                const sy = y * TILE_SIZE + offset.y;

                this.drawTile(tile, sx, sy, x, y, world);
            }
        }

        // Draw surface labels
        if (!player.isUnderground) {
            this.drawSurfaceLabels(offset);
        }

        // Draw player
        this.drawPlayer(player, offset);

        // Draw lighting overlay if underground
        if (player.isUnderground) {
            this.drawLighting(player, offset);
        }

        // Draw stratum transition labels
        if (player.isUnderground) {
            this.drawStratumLabels(offset, camera);
        }
    }

    drawSurfaceLabels(offset) {
        const ctx = this.ctx;
        const mid = Math.floor(WORLD_WIDTH / 2);
        const labelX = mid * TILE_SIZE + offset.x - 40;
        const labelY = (SURFACE_HEIGHT - 3) * TILE_SIZE + offset.y;

        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 10px monospace';
        ctx.textAlign = 'center';
        ctx.fillText('HOLLOWMARKET', mid * TILE_SIZE + offset.x + TILE_SIZE / 2, labelY);
        ctx.textAlign = 'left';
    }

    drawStratumLabels(offset, camera) {
        const ctx = this.ctx;
        for (const s of STRATA) {
            if (s.id === 0) continue;
            const labelY = s.startDepth * TILE_SIZE + offset.y + 20;
            if (labelY > -20 && labelY < CANVAS_HEIGHT + 20) {
                ctx.fillStyle = s.colors?.accent2 || '#FFF';
                ctx.globalAlpha = 0.4;
                ctx.font = 'bold 16px monospace';
                ctx.fillText(`--- ${s.name} ---`, 20, labelY);
                ctx.globalAlpha = 1;
            }
        }
    }

    drawSky(offset) {
        const skyBottom = SURFACE_HEIGHT * TILE_SIZE + offset.y;
        if (skyBottom > 0) {
            // Sky gradient
            const grad = this.ctx.createLinearGradient(0, Math.min(0, offset.y), 0, skyBottom);
            grad.addColorStop(0, '#4A90D9');
            grad.addColorStop(0.7, '#87CEEB');
            grad.addColorStop(1, '#B0E0E6');
            this.ctx.fillStyle = grad;
            this.ctx.fillRect(0, 0, CANVAS_WIDTH, Math.max(0, skyBottom));
        }
    }

    drawTile(tile, sx, sy, wx, wy, world) {
        if (tile.type === TILES.AIR) return;

        const ctx = this.ctx;

        switch (tile.type) {
            case TILES.SURFACE_GRASS: {
                // Grass surface
                ctx.fillStyle = '#4a8c3f';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                // Grass blades on top
                ctx.fillStyle = '#5ca84f';
                for (let i = 0; i < 5; i++) {
                    const gx = sx + 3 + i * 6;
                    ctx.fillRect(gx, sy, 2, 4);
                }
                // Dirt below grass
                ctx.fillStyle = '#8B7355';
                ctx.fillRect(sx, sy + TILE_SIZE * 0.6, TILE_SIZE, TILE_SIZE * 0.4);
                break;
            }

            case TILES.BUILDING: {
                // Wooden market building
                ctx.fillStyle = '#8B6914';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                ctx.fillStyle = '#A0522D';
                ctx.fillRect(sx + 2, sy + 2, TILE_SIZE - 4, TILE_SIZE - 4);
                // Window
                ctx.fillStyle = '#FFE4B5';
                ctx.fillRect(sx + 10, sy + 8, 12, 10);
                ctx.fillStyle = '#333';
                ctx.fillRect(sx + 15, sy + 8, 2, 10);
                ctx.fillRect(sx + 10, sy + 12, 12, 2);
                break;
            }

            case TILES.DIRT: {
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                if (!colors) { ctx.fillStyle = '#5C4033'; ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE); break; }

                ctx.fillStyle = colors.dirt || colors.base;
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Pixel texture
                const hash = this.tileHash(wx, wy);
                ctx.fillStyle = colors.base;
                if (hash % 3 === 0) ctx.fillRect(sx + 4, sy + 4, 6, 4);
                if (hash % 5 === 0) ctx.fillRect(sx + 16, sy + 12, 8, 5);
                if (hash % 7 === 0) ctx.fillRect(sx + 8, sy + 20, 5, 6);
                break;
            }

            case TILES.ROCK: {
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                ctx.fillStyle = colors?.base || '#666';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Rock texture - cracks
                ctx.strokeStyle = colors?.accent1 || '#888';
                ctx.lineWidth = 1;
                ctx.beginPath();
                const h = this.tileHash(wx, wy);
                ctx.moveTo(sx + (h % 12) + 4, sy + 2);
                ctx.lineTo(sx + TILE_SIZE / 2, sy + TILE_SIZE / 2);
                ctx.lineTo(sx + TILE_SIZE - (h % 8) - 4, sy + TILE_SIZE - 4);
                ctx.stroke();
                break;
            }

            case TILES.HARD_ROCK: {
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                ctx.fillStyle = colors?.accent1 || '#555';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Harder look - crystal/metallic shards
                ctx.fillStyle = colors?.accent2 || '#777';
                const h = this.tileHash(wx, wy);
                ctx.fillRect(sx + (h % 10) + 4, sy + 6, 8, 4);
                ctx.fillRect(sx + 12, sy + (h % 10) + 8, 6, 8);
                break;
            }

            case TILES.ITEM: {
                // Draw as dirt/rock with a sparkle hint
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                ctx.fillStyle = colors?.dirt || '#5C4033';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Subtle sparkle to hint at embedded item
                const rarityColor = tile.itemDef ? (RARITY[tile.itemDef.rarity]?.color || '#FFF') : '#FFF';
                const sparklePhase = (Date.now() / 500 + wx * 7 + wy * 13) % (Math.PI * 2);
                const sparkleAlpha = 0.2 + Math.sin(sparklePhase) * 0.15;
                ctx.fillStyle = rarityColor;
                ctx.globalAlpha = sparkleAlpha;
                ctx.fillRect(sx + 10, sy + 10, 12, 12);
                ctx.globalAlpha = 1;
                break;
            }

            case TILES.HAZARD_GAS: {
                ctx.fillStyle = '#2D4A2D';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                // Gas wisps
                const t = Date.now() / 1000;
                ctx.fillStyle = '#4AFF4A';
                ctx.globalAlpha = 0.3 + Math.sin(t + wx) * 0.15;
                ctx.fillRect(sx + 6, sy + 4 + Math.sin(t * 2 + wx) * 3, 10, 6);
                ctx.fillRect(sx + 14, sy + 16 + Math.sin(t * 2.5 + wy) * 3, 12, 5);
                ctx.globalAlpha = 1;
                break;
            }

            case TILES.HAZARD_WATER: {
                ctx.fillStyle = '#1a3a5c';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                // Water animation
                const t = Date.now() / 800;
                ctx.fillStyle = '#4A90D9';
                ctx.globalAlpha = 0.5;
                for (let i = 0; i < 3; i++) {
                    const waveY = sy + 8 + i * 8 + Math.sin(t + wx + i) * 3;
                    ctx.fillRect(sx, waveY, TILE_SIZE, 3);
                }
                ctx.globalAlpha = 1;
                break;
            }

            case TILES.BEDROCK: {
                ctx.fillStyle = '#1a1a1a';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                ctx.fillStyle = '#2a2a2a';
                ctx.fillRect(sx + 4, sy + 4, 8, 8);
                ctx.fillRect(sx + 18, sy + 16, 10, 6);
                break;
            }
        }

        // Tile borders (subtle grid)
        if (tile.type !== TILES.AIR) {
            // Only draw edges where adjacent tiles are air
            ctx.strokeStyle = 'rgba(0,0,0,0.15)';
            ctx.lineWidth = 1;
            if (!world.isSolid(wx, wy - 1)) {
                ctx.beginPath(); ctx.moveTo(sx, sy + 0.5); ctx.lineTo(sx + TILE_SIZE, sy + 0.5); ctx.stroke();
            }
            if (!world.isSolid(wx, wy + 1)) {
                ctx.beginPath(); ctx.moveTo(sx, sy + TILE_SIZE - 0.5); ctx.lineTo(sx + TILE_SIZE, sy + TILE_SIZE - 0.5); ctx.stroke();
            }
        }
    }

    drawPlayer(player, offset) {
        const ctx = this.ctx;
        const px = player.x * TILE_SIZE + offset.x;
        const py = player.y * TILE_SIZE + offset.y;

        // Shadow
        ctx.fillStyle = 'rgba(0,0,0,0.3)';
        ctx.fillRect(px + 4, py + TILE_SIZE - 2, TILE_SIZE - 8, 4);

        // Body
        ctx.fillStyle = '#E8B04A';
        ctx.fillRect(px + 6, py + 4, TILE_SIZE - 12, TILE_SIZE - 8);

        // Hard hat
        ctx.fillStyle = '#FFD700';
        ctx.fillRect(px + 4, py, TILE_SIZE - 8, 8);
        ctx.fillRect(px + 2, py + 4, TILE_SIZE - 4, 4);

        // Eyes
        ctx.fillStyle = '#1a1a2e';
        if (player.facingRight) {
            ctx.fillRect(px + 16, py + 8, 3, 3);
        } else {
            ctx.fillRect(px + 13, py + 8, 3, 3);
        }

        // Headlamp glow
        if (player.isUnderground) {
            ctx.fillStyle = '#FFE';
            ctx.globalAlpha = 0.8;
            if (player.facingRight) {
                ctx.fillRect(px + TILE_SIZE - 6, py + 4, 4, 3);
            } else {
                ctx.fillRect(px + 2, py + 4, 4, 3);
            }
            ctx.globalAlpha = 1;
        }

        // Tool (pickaxe) while digging
        if (player.digging) {
            ctx.fillStyle = '#888';
            const swingAngle = (player.digProgress / player.digTime) * Math.PI;
            const toolX = player.facingRight ? px + TILE_SIZE : px;
            const toolLen = 12;
            ctx.save();
            ctx.translate(toolX, py + 12);
            ctx.rotate(player.facingRight ? swingAngle - 0.5 : -(swingAngle - 0.5));
            ctx.fillRect(0, -1, toolLen, 3);
            ctx.fillStyle = '#AAA';
            ctx.fillRect(toolLen - 4, -3, 6, 7);
            ctx.restore();
        }

        // Dig progress bar
        if (player.digging && player.digTarget) {
            const dtx = player.digTarget.x * TILE_SIZE + offset.x;
            const dty = player.digTarget.y * TILE_SIZE + offset.y;
            const pct = player.digProgress / player.digTime;
            ctx.fillStyle = 'rgba(0,0,0,0.5)';
            ctx.fillRect(dtx, dty - 6, TILE_SIZE, 4);
            ctx.fillStyle = '#4FC3F7';
            ctx.fillRect(dtx, dty - 6, TILE_SIZE * pct, 4);
        }
    }

    drawLighting(player, offset) {
        const lctx = this.lightCtx;
        lctx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Fill with darkness
        lctx.fillStyle = 'rgba(0, 0, 0, 0.85)';
        lctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Cut out light around player (headlamp)
        const px = player.x * TILE_SIZE + offset.x + TILE_SIZE / 2;
        const py = player.y * TILE_SIZE + offset.y + TILE_SIZE / 2;
        const radius = player.headlampRadius * TILE_SIZE;

        lctx.globalCompositeOperation = 'destination-out';

        // Main ambient glow around player
        const ambientGrad = lctx.createRadialGradient(px, py, 0, px, py, radius * 0.5);
        ambientGrad.addColorStop(0, 'rgba(0, 0, 0, 1)');
        ambientGrad.addColorStop(0.7, 'rgba(0, 0, 0, 0.5)');
        ambientGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');
        lctx.fillStyle = ambientGrad;
        lctx.fillRect(px - radius, py - radius, radius * 2, radius * 2);

        // Headlamp cone
        const lampAngle = player.facingRight ? 0 : Math.PI;
        const coneAngle = Math.PI / 4;
        const coneRadius = radius * 1.2;

        lctx.beginPath();
        lctx.moveTo(px, py);
        lctx.arc(px, py, coneRadius, lampAngle - coneAngle, lampAngle + coneAngle);
        lctx.closePath();

        const coneGrad = lctx.createRadialGradient(px, py, 0, px, py, coneRadius);
        coneGrad.addColorStop(0, 'rgba(0, 0, 0, 1)');
        coneGrad.addColorStop(0.6, 'rgba(0, 0, 0, 0.8)');
        coneGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');
        lctx.fillStyle = coneGrad;
        lctx.fill();

        lctx.globalCompositeOperation = 'source-over';

        // Apply light overlay with stratum tint
        const stratum = this.getStratumAtY(player.y);
        if (stratum && stratum.id > 0) {
            lctx.fillStyle = stratum.bgColor;
            lctx.globalAlpha = 0.15;
            lctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
            lctx.globalAlpha = 1;
        }

        // Composite onto main canvas
        this.ctx.drawImage(this.lightCanvas, 0, 0);
    }

    drawHUD(player, gameState, expedition) {
        const ctx = this.ctx;

        // Stamina bar
        const barX = 10, barY = 10, barW = 200, barH = 16;
        const staminaPct = player.stamina / player.maxStamina;

        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(barX - 2, barY - 2, barW + 4, barH + 4);
        ctx.fillStyle = staminaPct > 0.3 ? '#4CAF50' : (staminaPct > 0.15 ? '#FF9800' : '#F44336');
        ctx.fillRect(barX, barY, barW * staminaPct, barH);
        ctx.strokeStyle = '#FFF';
        ctx.lineWidth = 1;
        ctx.strokeRect(barX, barY, barW, barH);

        // Stamina text
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 11px monospace';
        ctx.fillText(`STAMINA ${Math.ceil(player.stamina)}/${player.maxStamina}`, barX + 4, barY + 12);

        // Depth indicator
        const depth = player.getDepthMeters();
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(barX, barY + barH + 8, 120, 20);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 12px monospace';
        ctx.fillText(`DEPTH: ${depth}m`, barX + 6, barY + barH + 22);

        // Current stratum
        const stratum = this.getStratumAtY(player.y);
        if (stratum && stratum.name !== 'Surface') {
            ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
            ctx.fillRect(barX, barY + barH + 32, 220, 20);
            ctx.fillStyle = stratum.colors?.accent2 || '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText(`${stratum.name} — ${stratum.description || ''}`, barX + 6, barY + barH + 46);
        }

        // Gold
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(CANVAS_WIDTH - 140, 10, 130, 24);
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 14px monospace';
        ctx.fillText(`${player.gold} G`, CANVAS_WIDTH - 132, 28);

        // Inventory count
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(CANVAS_WIDTH - 140, 40, 130, 24);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 12px monospace';
        ctx.fillText(`BAG: ${player.inventory.length}/${player.inventorySlots}`, CANVAS_WIDTH - 132, 56);

        // Items found this expedition
        if (!player.isAtSurface() && expedition) {
            ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
            ctx.fillRect(CANVAS_WIDTH - 140, 70, 130, 24);
            ctx.fillStyle = '#AAA';
            ctx.font = '11px monospace';
            ctx.fillText(`FOUND: ${expedition.itemsFound}`, CANVAS_WIDTH - 132, 86);
        }

        // Depth sidebar (visual depth meter on right edge)
        if (!player.isAtSurface()) {
            this.drawDepthMeter(player);
        }

        // Controls hint (context-sensitive)
        if (player.isAtSurface()) {
            this.drawControlHint('ARROWS: Move  |  DOWN: Enter Mine  |  M: Market  |  U: Upgrades  |  C: Contracts');
        } else {
            this.drawControlHint('ARROWS: Move  |  DOWN/SHIFT+DIR: Dig  |  SPACE: Jump  |  E: Return  |  I: Inventory');
        }
    }

    drawDepthMeter(player) {
        const ctx = this.ctx;
        const x = CANVAS_WIDTH - 16;
        const h = CANVAS_HEIGHT - 80;
        const y = 40;

        // Background
        ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
        ctx.fillRect(x, y, 12, h);

        // Stratum colors
        const totalDepth = 200; // WORLD_HEIGHT
        for (const s of STRATA) {
            if (s.id === 0) continue;
            const sy = y + (s.startDepth / totalDepth) * h;
            const sh = ((s.endDepth - s.startDepth) / totalDepth) * h;
            ctx.fillStyle = s.colors?.accent2 || s.bgColor || '#333';
            ctx.globalAlpha = 0.6;
            ctx.fillRect(x + 1, sy, 10, sh);
        }
        ctx.globalAlpha = 1;

        // Player position marker
        const playerPct = Math.max(0, Math.min(1, player.y / totalDepth));
        const markerY = y + playerPct * h;
        ctx.fillStyle = '#FFD700';
        ctx.fillRect(x - 3, markerY - 2, 18, 4);
    }

    drawControlHint(text) {
        const ctx = this.ctx;
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        const w = ctx.measureText(text).width + 20;
        ctx.font = '10px monospace';
        const measured = ctx.measureText(text).width + 20;
        ctx.fillRect((CANVAS_WIDTH - measured) / 2, CANVAS_HEIGHT - 28, measured, 22);
        ctx.fillStyle = '#AAA';
        ctx.fillText(text, (CANVAS_WIDTH - measured) / 2 + 10, CANVAS_HEIGHT - 12);
    }

    drawNotification(text, color, progress, index) {
        const ctx = this.ctx;
        const alpha = progress < 0.1 ? progress / 0.1 : (progress > 0.7 ? (1 - progress) / 0.3 : 1);
        ctx.globalAlpha = alpha;
        ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
        ctx.font = 'bold 14px monospace';
        const w = ctx.measureText(text).width + 24;
        const x = (CANVAS_WIDTH - w) / 2;
        const baseY = CANVAS_HEIGHT / 3 + (index || 0) * 35;
        const y = baseY - progress * 20;
        ctx.fillRect(x, y, w, 28);
        ctx.fillStyle = color || '#FFF';
        ctx.fillText(text, x + 12, y + 19);
        ctx.globalAlpha = 1;
    }

    getStratum(id) {
        return STRATA[id] || null;
    }

    getStratumAtY(y) {
        for (const s of STRATA) {
            if (y >= s.startDepth && y < s.endDepth) return s;
        }
        return STRATA[0];
    }

    tileHash(x, y) {
        return Math.abs(((x * 73856093) ^ (y * 19349663)) % 1000);
    }
}


class UI {
    constructor(ctx) {
        this.ctx = ctx;
        this.scrollOffset = 0;
        this.maxScroll = 0;
        this.selectedFaction = 'curator';
        this.selectedTree = 'excavation';
        this.hoveredItem = null;
        this.hoveredUpgrade = null;
        this.sellAnimations = [];
        this.tooltipText = null;
    }

    // === MARKET SCREEN ===
    drawMarket(economy, player, contracts, particles) {
        const ctx = this.ctx;
        const mouse = getMouse();

        // Background
        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Title
        this.drawPanelHeader('HOLLOWMARKET', 0, 0, CANVAS_WIDTH);

        // Market event banner
        if (economy.activeEvent) {
            ctx.fillStyle = '#2a1a0a';
            ctx.fillRect(10, 40, CANVAS_WIDTH - 20, 30);
            ctx.strokeStyle = '#FFD700';
            ctx.lineWidth = 1;
            ctx.strokeRect(10, 40, CANVAS_WIDTH - 20, 30);
            ctx.fillStyle = '#FFD700';
            ctx.font = 'bold 12px monospace';
            ctx.fillText(`EVENT: ${economy.activeEvent.name} — ${economy.activeEvent.description}`, 20, 60);
        }

        // Faction tabs
        const tabY = economy.activeEvent ? 80 : 45;
        const factionIds = Object.keys(FACTIONS);
        const tabW = (CANVAS_WIDTH - 20) / factionIds.length;

        factionIds.forEach((fid, i) => {
            const faction = FACTIONS[fid];
            const tx = 10 + i * tabW;
            const selected = this.selectedFaction === fid;

            ctx.fillStyle = selected ? faction.color : '#333';
            ctx.fillRect(tx, tabY, tabW - 4, 30);
            ctx.fillStyle = selected ? '#000' : '#AAA';
            ctx.font = 'bold 11px monospace';
            ctx.fillText(faction.name, tx + 8, tabY + 19);

            // Reputation
            const rep = economy.getRepTier(fid);
            ctx.fillStyle = rep.color;
            ctx.font = '9px monospace';
            ctx.fillText(rep.name, tx + tabW - 60, tabY + 19);

            // Click detection
            if (mouse.justClicked && mouse.x >= tx && mouse.x < tx + tabW - 4 &&
                mouse.y >= tabY && mouse.y < tabY + 30) {
                this.selectedFaction = fid;
            }
        });

        // Inventory items for sale
        const listY = tabY + 40;
        ctx.fillStyle = '#222';
        ctx.fillRect(10, listY, CANVAS_WIDTH - 20, CANVAS_HEIGHT - listY - 50);

        if (player.inventory.length === 0) {
            ctx.fillStyle = '#666';
            ctx.font = '14px monospace';
            ctx.fillText('Your bag is empty. Go dig something up!', 30, listY + 40);
        } else {
            ctx.fillStyle = '#AAA';
            ctx.font = 'bold 11px monospace';
            ctx.fillText('ITEM', 25, listY + 16);
            ctx.fillText('RARITY', 250, listY + 16);
            ctx.fillText('PRICE', 380, listY + 16);
            ctx.fillText('', 500, listY + 16);

            let clickedSell = -1;

            for (let i = 0; i < player.inventory.length; i++) {
                const item = player.inventory[i];
                const iy = listY + 25 + i * 32;
                if (iy > CANVAS_HEIGHT - 60) break;

                const price = economy.getPrice(item, this.selectedFaction);
                const rarity = RARITY[item.rarity];
                const hovered = mouse.x >= 15 && mouse.x < CANVAS_WIDTH - 15 &&
                               mouse.y >= iy && mouse.y < iy + 30;

                // Row bg
                ctx.fillStyle = hovered ? '#333' : (i % 2 === 0 ? '#252525' : '#222');
                ctx.fillRect(15, iy, CANVAS_WIDTH - 30, 30);

                // Item name
                ctx.fillStyle = rarity.color;
                ctx.font = '12px monospace';
                ctx.fillText(item.name, 25, iy + 19);

                // Rarity
                ctx.fillStyle = rarity.color;
                ctx.fillText(rarity.name, 250, iy + 19);

                // Price
                if (price > 0) {
                    ctx.fillStyle = '#FFD700';
                    ctx.fillText(`${price} G`, 380, iy + 19);

                    // Sell button
                    const btnX = 500, btnW = 70, btnH = 22;
                    const btnHovered = mouse.x >= btnX && mouse.x < btnX + btnW &&
                                      mouse.y >= iy + 4 && mouse.y < iy + 4 + btnH;
                    ctx.fillStyle = btnHovered ? '#4CAF50' : '#2E7D32';
                    ctx.fillRect(btnX, iy + 4, btnW, btnH);
                    ctx.fillStyle = '#FFF';
                    ctx.font = 'bold 11px monospace';
                    ctx.fillText('SELL', btnX + 18, iy + 19);

                    if (btnHovered && mouse.justClicked) {
                        clickedSell = i;
                    }
                } else {
                    ctx.fillStyle = '#666';
                    ctx.font = '12px monospace';
                    ctx.fillText('Not interested', 380, iy + 19);
                }
            }

            // Process sell
            if (clickedSell >= 0) {
                return { action: 'sell', index: clickedSell, factionId: this.selectedFaction };
            }
        }

        // Sell All button
        if (player.inventory.length > 0) {
            const saX = CANVAS_WIDTH - 160, saY = CANVAS_HEIGHT - 45, saW = 140, saH = 30;
            const saHover = mouse.x >= saX && mouse.x < saX + saW && mouse.y >= saY && mouse.y < saY + saH;
            ctx.fillStyle = saHover ? '#D32F2F' : '#B71C1C';
            ctx.fillRect(saX, saY, saW, saH);
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('SELL ALL', saX + 30, saY + 20);

            if (saHover && mouse.justClicked) {
                return { action: 'sell_all', factionId: this.selectedFaction };
            }
        }

        // Gold display
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 16px monospace';
        ctx.fillText(`Gold: ${player.gold}`, 20, CANVAS_HEIGHT - 20);

        // Back button
        return this.drawBackButton(mouse);
    }

    // === UPGRADE SCREEN ===
    drawUpgrades(upgradeManager, player) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('UPGRADES', 0, 0, CANVAS_WIDTH);

        // Tree tabs
        const treeIds = Object.keys(UPGRADE_TREES);
        const tabW = (CANVAS_WIDTH - 20) / treeIds.length;

        treeIds.forEach((tid, i) => {
            const tree = UPGRADE_TREES[tid];
            const tx = 10 + i * tabW;
            const selected = this.selectedTree === tid;

            ctx.fillStyle = selected ? tree.color : '#333';
            ctx.fillRect(tx, 45, tabW - 4, 30);
            ctx.fillStyle = selected ? '#000' : '#AAA';
            ctx.font = 'bold 11px monospace';
            ctx.fillText(`${tree.name} (T${upgradeManager.getCurrentTier(tid)})`, tx + 8, 65);

            if (mouse.justClicked && mouse.x >= tx && mouse.x < tx + tabW - 4 &&
                mouse.y >= 45 && mouse.y < 75) {
                this.selectedTree = tid;
            }
        });

        // Draw selected tree
        const tree = UPGRADE_TREES[this.selectedTree];
        const startY = 90;
        let y = startY;

        for (const tier of tree.tiers) {
            // Tier header
            ctx.fillStyle = '#444';
            ctx.fillRect(10, y, CANVAS_WIDTH - 20, 24);
            ctx.fillStyle = tree.color;
            ctx.font = 'bold 12px monospace';

            if (tier.auto) {
                ctx.fillText(`Tier ${tier.tier}: ${tier.name} (Base)`, 20, y + 16);
                y += 30;
                continue;
            }

            ctx.fillText(`Tier ${tier.tier}`, 20, y + 16);
            y += 30;

            if (tier.choices) {
                for (const choice of tier.choices) {
                    const owned = upgradeManager.purchased.has(choice.id);
                    const canBuy = upgradeManager.canPurchase(choice.id, player.gold);
                    const meetsReqs = !choice.requires || upgradeManager.purchased.has(choice.requires);
                    const tierBlocked = tier.choices.some(c => c.id !== choice.id && upgradeManager.purchased.has(c.id));

                    const cardH = 60;
                    const hovered = mouse.x >= 20 && mouse.x < CANVAS_WIDTH - 20 &&
                                   mouse.y >= y && mouse.y < y + cardH;

                    // Card background
                    if (owned) {
                        ctx.fillStyle = '#1a3a1a';
                    } else if (tierBlocked || !meetsReqs) {
                        ctx.fillStyle = '#1a1a1a';
                    } else if (hovered) {
                        ctx.fillStyle = '#2a2a3a';
                    } else {
                        ctx.fillStyle = '#222';
                    }
                    ctx.fillRect(20, y, CANVAS_WIDTH - 40, cardH);

                    // Border
                    ctx.strokeStyle = owned ? '#4CAF50' : (canBuy ? tree.color : '#444');
                    ctx.lineWidth = owned ? 2 : 1;
                    ctx.strokeRect(20, y, CANVAS_WIDTH - 40, cardH);

                    // Name + branch
                    ctx.fillStyle = owned ? '#4CAF50' : (meetsReqs && !tierBlocked ? '#FFF' : '#666');
                    ctx.font = 'bold 12px monospace';
                    ctx.fillText(`[${choice.branch}] ${choice.name}`, 30, y + 18);

                    // Description
                    ctx.fillStyle = owned ? '#6a6' : '#AAA';
                    ctx.font = '10px monospace';
                    ctx.fillText(choice.description, 30, y + 34);

                    // Cost / Status
                    if (owned) {
                        ctx.fillStyle = '#4CAF50';
                        ctx.font = 'bold 11px monospace';
                        ctx.fillText('OWNED', CANVAS_WIDTH - 100, y + 18);
                    } else if (meetsReqs && !tierBlocked) {
                        ctx.fillStyle = canBuy ? '#FFD700' : '#888';
                        ctx.font = 'bold 11px monospace';
                        ctx.fillText(`${choice.cost} G`, CANVAS_WIDTH - 110, y + 18);

                        if (canBuy) {
                            // Buy button
                            const bx = CANVAS_WIDTH - 110, by = y + 30, bw = 65, bh = 22;
                            const bHover = mouse.x >= bx && mouse.x < bx + bw &&
                                          mouse.y >= by && mouse.y < by + bh;
                            ctx.fillStyle = bHover ? '#FF9800' : '#E65100';
                            ctx.fillRect(bx, by, bw, bh);
                            ctx.fillStyle = '#FFF';
                            ctx.font = 'bold 10px monospace';
                            ctx.fillText('BUY', bx + 20, by + 15);

                            if (bHover && mouse.justClicked) {
                                return { action: 'buy', upgradeId: choice.id };
                            }
                        }
                    } else {
                        ctx.fillStyle = '#555';
                        ctx.font = '10px monospace';
                        if (tierBlocked) {
                            ctx.fillText('(Branch chosen)', CANVAS_WIDTH - 140, y + 18);
                        } else if (choice.requires) {
                            const reqName = this.findUpgradeName(choice.requires);
                            ctx.fillText(`Requires: ${reqName}`, CANVAS_WIDTH - 200, y + 18);
                        }
                    }

                    y += cardH + 5;
                }
            }
            y += 10;
        }

        // Gold display
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 16px monospace';
        ctx.fillText(`Gold: ${player.gold}`, 20, CANVAS_HEIGHT - 20);

        return this.drawBackButton(mouse);
    }

    findUpgradeName(upgradeId) {
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const c of tier.choices) {
                        if (c.id === upgradeId) return c.name;
                    }
                }
            }
        }
        return upgradeId;
    }

    // === CONTRACT SCREEN ===
    drawContracts(contractManager, economy) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('BOUNTY BOARD', 0, 0, CANVAS_WIDTH);

        // Active contracts
        let y = 50;
        const active = contractManager.getActiveContracts();
        if (active.length > 0) {
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('ACTIVE CONTRACTS', 20, y);
            y += 20;

            for (const contract of active) {
                this.drawContractCard(contract, 20, y, true);
                y += 70;
            }
        }

        y += 10;
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 12px monospace';
        ctx.fillText('AVAILABLE CONTRACTS', 20, y);
        y += 20;

        const available = contractManager.getAvailableContracts();
        for (const contract of available) {
            const result = this.drawContractCard(contract, 20, y, false, mouse);
            if (result) return result;
            y += 70;
        }

        // Claim rewards button
        const completed = contractManager.contracts.filter(c => c.completed && c.active && !c.claimed);
        if (completed.length > 0) {
            const totalReward = completed.reduce((sum, c) => sum + c.reward, 0);
            const bx = CANVAS_WIDTH / 2 - 100, by = CANVAS_HEIGHT - 60, bw = 200, bh = 35;
            const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;

            ctx.fillStyle = bHover ? '#FFD700' : '#E8B04A';
            ctx.fillRect(bx, by, bw, bh);
            ctx.fillStyle = '#000';
            ctx.font = 'bold 14px monospace';
            ctx.fillText(`CLAIM ${totalReward} G`, bx + 40, by + 23);

            if (bHover && mouse.justClicked) {
                return { action: 'claim' };
            }
        }

        return this.drawBackButton(mouse);
    }

    drawContractCard(contract, x, y, isActive, mouse) {
        const ctx = this.ctx;
        const w = CANVAS_WIDTH - 40;

        ctx.fillStyle = contract.completed ? '#1a3a1a' : '#222';
        ctx.fillRect(x, y, w, 62);
        ctx.strokeStyle = contract.factionColor || '#555';
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, w, 62);

        // Faction + description
        ctx.fillStyle = contract.factionColor || '#FFF';
        ctx.font = 'bold 11px monospace';
        ctx.fillText(contract.factionName, x + 10, y + 16);

        ctx.fillStyle = '#CCC';
        ctx.font = '11px monospace';
        ctx.fillText(contract.description, x + 10, y + 34);

        // Progress
        ctx.fillStyle = '#AAA';
        ctx.fillText(`Progress: ${contract.collected}/${contract.quantity}`, x + 10, y + 50);

        // Reward
        ctx.fillStyle = '#FFD700';
        ctx.fillText(`Reward: ${contract.reward} G  +${contract.repReward} Rep`, x + 300, y + 50);

        if (contract.completed) {
            ctx.fillStyle = '#4CAF50';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('COMPLETE!', x + w - 100, y + 16);
        } else if (isActive) {
            ctx.fillStyle = '#888';
            ctx.font = '10px monospace';
            ctx.fillText(`Expires in ${contract.expires} expeditions`, x + w - 200, y + 16);
        }

        // Accept button for available contracts
        if (!isActive && mouse) {
            const bx = x + w - 90, by2 = y + 10, bw = 80, bh = 25;
            const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by2 && mouse.y < by2 + bh;
            ctx.fillStyle = bHover ? '#4FC3F7' : '#1565C0';
            ctx.fillRect(bx, by2, bw, bh);
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 10px monospace';
            ctx.fillText('ACCEPT', bx + 14, by2 + 17);

            if (bHover && mouse.justClicked) {
                return { action: 'accept', contractId: contract.id };
            }
        }

        return null;
    }

    // === INVENTORY SCREEN ===
    drawInventory(player) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = 'rgba(0, 0, 0, 0.85)';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader(`INVENTORY (${player.inventory.length}/${player.inventorySlots})`, 0, 0, CANVAS_WIDTH);

        if (player.inventory.length === 0) {
            ctx.fillStyle = '#666';
            ctx.font = '14px monospace';
            ctx.fillText('Empty bag. Time to dig!', 30, 80);
        } else {
            // Grid layout
            const cols = 5;
            const cellSize = 120;
            const startX = (CANVAS_WIDTH - cols * cellSize) / 2;
            const startY = 50;

            for (let i = 0; i < player.inventory.length; i++) {
                const item = player.inventory[i];
                const col = i % cols;
                const row = Math.floor(i / cols);
                const cx = startX + col * cellSize;
                const cy = startY + row * (cellSize + 10);

                if (cy > CANVAS_HEIGHT - 50) break;

                const rarity = RARITY[item.rarity];
                const hovered = mouse.x >= cx && mouse.x < cx + cellSize - 8 &&
                               mouse.y >= cy && mouse.y < cy + cellSize;

                // Cell
                ctx.fillStyle = hovered ? '#333' : '#222';
                ctx.fillRect(cx, cy, cellSize - 8, cellSize);
                ctx.strokeStyle = rarity.color;
                ctx.lineWidth = hovered ? 2 : 1;
                ctx.strokeRect(cx, cy, cellSize - 8, cellSize);

                // Item icon (colored square with letter)
                ctx.fillStyle = rarity.color;
                ctx.fillRect(cx + 30, cy + 15, 52, 40);
                ctx.fillStyle = '#000';
                ctx.font = 'bold 20px monospace';
                ctx.fillText(item.name[0], cx + 46, cy + 42);

                // Name
                ctx.fillStyle = '#FFF';
                ctx.font = '9px monospace';
                const displayName = item.name.length > 14 ? item.name.substring(0, 13) + '..' : item.name;
                ctx.fillText(displayName, cx + 5, cy + 72);

                // Rarity
                ctx.fillStyle = rarity.color;
                ctx.font = '8px monospace';
                ctx.fillText(rarity.name, cx + 5, cy + 84);

                // Value
                ctx.fillStyle = '#FFD700';
                ctx.fillText(`~${item.baseValue} G`, cx + 5, cy + 96);

                // Tooltip on hover
                if (hovered) {
                    this.drawTooltip(mouse.x + 10, mouse.y - 10, item);
                }
            }
        }

        // Drop item hint
        ctx.fillStyle = '#666';
        ctx.font = '10px monospace';
        ctx.fillText('Press I to close', CANVAS_WIDTH / 2 - 50, CANVAS_HEIGHT - 15);

        return null;
    }

    drawTooltip(x, y, item) {
        const ctx = this.ctx;
        const rarity = RARITY[item.rarity];
        const w = 200, h = 80;

        // Clamp to screen
        if (x + w > CANVAS_WIDTH) x = CANVAS_WIDTH - w - 5;
        if (y + h > CANVAS_HEIGHT) y = CANVAS_HEIGHT - h - 5;

        ctx.fillStyle = 'rgba(20, 20, 40, 0.95)';
        ctx.fillRect(x, y, w, h);
        ctx.strokeStyle = rarity.color;
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, w, h);

        ctx.fillStyle = rarity.color;
        ctx.font = 'bold 11px monospace';
        ctx.fillText(item.name, x + 8, y + 16);

        ctx.fillStyle = '#CCC';
        ctx.font = '10px monospace';
        ctx.fillText(item.description, x + 8, y + 32);

        ctx.fillStyle = '#AAA';
        ctx.fillText(`Stratum ${item.stratum} | ${rarity.name}`, x + 8, y + 48);
        ctx.fillStyle = '#FFD700';
        ctx.fillText(`Base value: ${item.baseValue} G`, x + 8, y + 64);
    }

    // === EXPEDITION SUMMARY SCREEN ===
    drawExpeditionSummary(summary) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('EXPEDITION COMPLETE', 0, 0, CANVAS_WIDTH);

        let y = 60;

        // Stats
        ctx.fillStyle = '#FFF';
        ctx.font = '14px monospace';
        ctx.fillText(`Max Depth: ${summary.maxDepth}m`, 40, y); y += 25;
        ctx.fillText(`Items Found: ${summary.itemsFound}`, 40, y); y += 25;
        ctx.fillText(`Tiles Dug: ${summary.tilesDug}`, 40, y); y += 25;

        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 16px monospace';
        ctx.fillText(`Estimated Haul Value: ${summary.estimatedValue} G`, 40, y); y += 40;

        // Items list
        if (summary.items && summary.items.length > 0) {
            ctx.fillStyle = '#AAA';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('YOUR HAUL:', 40, y); y += 20;

            for (const item of summary.items) {
                const rarity = RARITY[item.rarity];
                ctx.fillStyle = rarity.color;
                ctx.font = '11px monospace';
                ctx.fillText(`- ${item.name} (${rarity.name})`, 50, y);
                ctx.fillStyle = '#FFD700';
                ctx.fillText(`~${item.baseValue} G`, 400, y);
                y += 18;
                if (y > CANVAS_HEIGHT - 80) break;
            }
        }

        // Continue button
        const bx = CANVAS_WIDTH / 2 - 80, by = CANVAS_HEIGHT - 55, bw = 160, bh = 35;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;
        ctx.fillStyle = bHover ? '#4CAF50' : '#2E7D32';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 14px monospace';
        ctx.fillText('CONTINUE', bx + 30, by + 23);

        if (bHover && mouse.justClicked) {
            return { action: 'continue' };
        }
        return null;
    }

    // === PREP SCREEN ===
    drawPrep(player, economy) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('EXPEDITION PREP', 0, 0, CANVAS_WIDTH);

        let y = 60;
        ctx.fillStyle = '#FFF';
        ctx.font = '14px monospace';
        ctx.fillText('Prepare for your descent into The Column.', 40, y); y += 30;

        ctx.fillStyle = '#AAA';
        ctx.font = '12px monospace';
        ctx.fillText(`Stamina: ${Math.ceil(player.stamina)}/${player.maxStamina}`, 40, y); y += 20;
        ctx.fillText(`Bag Space: ${player.inventory.length}/${player.inventorySlots}`, 40, y); y += 20;
        ctx.fillText(`Dig Power: ${player.digPower.toFixed(1)}x`, 40, y); y += 30;

        // Rest option
        if (player.stamina < player.maxStamina) {
            const rx = 40, ry = y, rw = 200, rh = 30;
            const rHover = mouse.x >= rx && mouse.x < rx + rw && mouse.y >= ry && mouse.y < ry + rh;
            ctx.fillStyle = rHover ? '#4FC3F7' : '#1565C0';
            ctx.fillRect(rx, ry, rw, rh);
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('REST (Full Recovery)', rx + 20, ry + 20);

            if (rHover && mouse.justClicked) {
                return { action: 'rest' };
            }
            y += 40;
        }

        // Market event info
        if (economy.activeEvent) {
            ctx.fillStyle = '#FFD700';
            ctx.font = 'bold 12px monospace';
            ctx.fillText(`Active Event: ${economy.activeEvent.name}`, 40, y);
            ctx.fillStyle = '#AAA';
            ctx.font = '11px monospace';
            ctx.fillText(economy.activeEvent.description, 40, y + 18);
            y += 50;
        }

        // Begin expedition button
        const bx = CANVAS_WIDTH / 2 - 100, by = CANVAS_HEIGHT - 80, bw = 200, bh = 40;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;
        ctx.fillStyle = bHover ? '#FF6B35' : '#E65100';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 16px monospace';
        ctx.fillText('DESCEND', bx + 50, by + 27);

        if (bHover && mouse.justClicked) {
            return { action: 'descend' };
        }

        return this.drawBackButton(mouse);
    }

    // === MAIN MENU ===
    drawMenu() {
        const ctx = this.ctx;
        const mouse = getMouse();

        // Background
        ctx.fillStyle = '#0a0a1e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Parallax dirt layers
        const t = Date.now() / 3000;
        const layerColors = ['#5C4033', '#4A4A5A', '#2D1B2E', '#8B6914'];
        for (let i = 0; i < 4; i++) {
            const ly = 350 + i * 40 + Math.sin(t + i) * 5;
            ctx.fillStyle = layerColors[i];
            ctx.globalAlpha = 0.3;
            ctx.fillRect(0, ly, CANVAS_WIDTH, 40);
        }
        ctx.globalAlpha = 1;

        // Title
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 64px monospace';
        ctx.textAlign = 'center';
        ctx.fillText('STRATA', CANVAS_WIDTH / 2, 160);

        ctx.fillStyle = '#AAA';
        ctx.font = '16px monospace';
        ctx.fillText('Dig. Appraise. Sell. Go Deeper.', CANVAS_WIDTH / 2, 200);

        // Start button
        const bx = CANVAS_WIDTH / 2 - 100, by = 280, bw = 200, bh = 50;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;

        ctx.fillStyle = bHover ? '#FF6B35' : '#E65100';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 20px monospace';
        ctx.fillText('NEW GAME', CANVAS_WIDTH / 2, by + 33);

        ctx.textAlign = 'left';

        // Controls info
        ctx.fillStyle = '#666';
        ctx.font = '11px monospace';
        ctx.textAlign = 'center';
        ctx.fillText('Arrow Keys / WASD: Move & Dig  |  Space: Jump  |  Shift+Dir: Dig sideways', CANVAS_WIDTH / 2, CANVAS_HEIGHT - 60);
        ctx.fillText('M: Market  |  U: Upgrades  |  C: Contracts  |  I: Inventory  |  E: Return to Surface', CANVAS_WIDTH / 2, CANVAS_HEIGHT - 40);
        ctx.textAlign = 'left';

        if (bHover && mouse.justClicked) {
            return { action: 'start' };
        }
        return null;
    }

    // === HELPERS ===
    drawPanelHeader(text, x, y, width) {
        const ctx = this.ctx;
        ctx.fillStyle = '#2a2a3a';
        ctx.fillRect(x, y, width, 35);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 18px monospace';
        ctx.fillText(text, x + 15, y + 24);
    }

    drawBackButton(mouse) {
        const ctx = this.ctx;
        const bx = 10, by = CANVAS_HEIGHT - 42, bw = 80, bh = 28;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;
        ctx.fillStyle = bHover ? '#555' : '#333';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 11px monospace';
        ctx.fillText('< BACK', bx + 12, by + 18);

        if (bHover && mouse.justClicked) {
            return { action: 'back' };
        }
        if (isKeyJustPressed('Escape')) {
            return { action: 'back' };
        }
        return null;
    }
}


class Game {
    constructor(canvas) {
        this.canvas = canvas;
        this.ctx = canvas.getContext('2d');
        canvas.width = CANVAS_WIDTH;
        canvas.height = CANVAS_HEIGHT;

        // Disable image smoothing for pixel art look
        this.ctx.imageSmoothingEnabled = false;

        // Initialize input
        initInput();
        initMouse(canvas);

        // Systems
        this.renderer = new Renderer(this.ctx);
        this.particles = new ParticleSystem();
        this.ui = new UI(this.ctx);
        this.audio = new AudioManager();

        // Game state
        this.state = GAME_STATES.MENU;
        this.world = null;
        this.player = null;
        this.camera = null;
        this.economy = new Economy();
        this.upgrades = new UpgradeManager();
        this.contracts = new ContractManager();

        // Expedition tracking
        this.expedition = {
            maxDepth: 0,
            itemsFound: 0,
            tilesDug: 0,
            startTime: 0,
            items: []
        };

        // Notifications
        this.notifications = [];

        // Ambient timer
        this.ambientTimer = 0;

        // Frame timing
        this.lastTime = 0;
        this.running = false;
    }

    start() {
        this.running = true;
        this.lastTime = performance.now();
        requestAnimationFrame((t) => this.loop(t));
    }

    newGame() {
        this.world = new World();
        this.player = new Player(this.world);
        this.player.gold = ECONOMY.startingGold;
        this.camera = new Camera();
        this.economy = new Economy();
        this.upgrades = new UpgradeManager();
        this.contracts = new ContractManager();
        this.state = GAME_STATES.SURFACE;

        // Initialize audio on first user interaction
        if (!this.audio.initialized) {
            this.audio.init();
        }

        this.addNotification('Welcome to Hollowmarket! Press DOWN to start digging.', '#FFD700');
    }

    loop(time) {
        if (!this.running) return;

        const dt = Math.min((time - this.lastTime) / 1000, 0.05); // Cap at 50ms
        this.lastTime = time;

        this.update(dt);
        this.draw();

        clearJustPressed();
        clearMouseJustClicked();

        requestAnimationFrame((t) => this.loop(t));
    }

    update(dt) {
        switch (this.state) {
            case GAME_STATES.MENU:
                break;

            case GAME_STATES.SURFACE:
            case GAME_STATES.DIGGING:
                this.updateGameplay(dt);
                break;

            case GAME_STATES.MARKET:
            case GAME_STATES.UPGRADES:
            case GAME_STATES.CONTRACTS:
            case GAME_STATES.INVENTORY:
            case GAME_STATES.PREP:
            case GAME_STATES.EXPEDITION_SUMMARY:
                // UI screens - no gameplay update
                break;
        }

        // Update particles always
        this.particles.update(dt);

        // Update notifications
        this.updateNotifications(dt);
    }

    updateGameplay(dt) {
        // Track if we just started digging (for particles)
        const wasDig = this.player.digging;
        const prevProgress = this.player.digProgress;

        // Player update
        const digResult = this.player.update(dt);

        // Check if a dig just completed
        if (digResult !== null && digResult !== undefined) {
            const target = this.player.lastDigTarget || { x: Math.round(this.player.x), y: Math.round(this.player.y) + 1 };

            if (typeof digResult === 'object') {
                // Item found!
                const rarity = getItemRarity(digResult);
                this.particles.emitSparkle(target.x, target.y, rarity.color);
                this.audio.playItemFound(digResult.rarity);
                this.addNotification(`Found: ${digResult.name} (${rarity.name})`, rarity.color);
                this.expedition.itemsFound++;
                this.expedition.items.push(digResult);
            } else if (digResult === 'dug') {
                this.expedition.tilesDug++;
            } else if (digResult === 'full') {
                this.addNotification('Bag is full! Return to surface to sell.', '#F44336');
            }
        }

        // Dig start particles (only once when dig begins)
        if (this.player.digging && this.player.digTarget && this.player.digProgress < dt * 2 && prevProgress === 0) {
            const tile = this.world.getTile(this.player.digTarget.x, this.player.digTarget.y);
            const stratum = this.renderer.getStratum(tile.stratum);
            const color = stratum?.colors?.base || '#888';
            this.particles.emitDig(this.player.digTarget.x, this.player.digTarget.y, color);
            this.audio.playDig(tile.hardness || 1);
            this.camera.addShake(1);
        }

        // Track max depth
        const depth = this.player.getDepthMeters();
        if (depth > this.expedition.maxDepth) {
            this.expedition.maxDepth = depth;
        }

        // Camera
        this.camera.follow(this.player, dt);

        // Ambient sounds
        if (this.player.isUnderground) {
            this.ambientTimer -= dt;
            if (this.ambientTimer <= 0) {
                const stratum = this.renderer.getStratumAtY(this.player.y);
                if (stratum) {
                    this.audio.playAmbient(stratum.id);
                }
                this.ambientTimer = 5 + Math.random() * 5;
            }
        }

        // Hazard detection
        this.checkHazards();

        // State transitions
        this.handleStateInput();

        // Stamina warning
        if (this.player.isUnderground && this.player.stamina < 15 && this.player.stamina > 14.5) {
            this.addNotification('Low stamina! Return to surface soon!', '#F44336');
        }

        // Auto-return at zero stamina
        if (this.player.isUnderground && this.player.stamina <= 0) {
            this.addNotification('Exhausted! Forced return to surface.', '#F44336');
            this.returnToSurface();
            return; // Don't overwrite state
        }

        // Track state (only if not in a special UI state)
        if (this.state === GAME_STATES.SURFACE || this.state === GAME_STATES.DIGGING) {
            this.state = this.player.isAtSurface() ? GAME_STATES.SURFACE : GAME_STATES.DIGGING;
        }
    }

    handleStateInput() {
        // Surface-only actions
        if (this.player.isAtSurface()) {
            if (isKeyJustPressed('KeyM')) {
                this.state = GAME_STATES.MARKET;
                this.audio.playClick();
            }
            if (isKeyJustPressed('KeyU')) {
                this.state = GAME_STATES.UPGRADES;
                this.audio.playClick();
            }
            if (isKeyJustPressed('KeyC')) {
                this.state = GAME_STATES.CONTRACTS;
                this.audio.playClick();
            }
        }

        // Universal
        if (isKeyJustPressed('KeyI')) {
            if (this.state === GAME_STATES.INVENTORY) {
                this.state = this.player.isAtSurface() ? GAME_STATES.SURFACE : GAME_STATES.DIGGING;
            } else {
                this.state = GAME_STATES.INVENTORY;
            }
            this.audio.playClick();
        }

        // Return to surface
        if (isKeyJustPressed('KeyE') && this.player.isUnderground) {
            this.returnToSurface();
        }

        // Toggle audio
        if (isKeyJustPressed('KeyF')) {
            const enabled = this.audio.toggle();
            this.addNotification(enabled ? 'Audio ON' : 'Audio OFF', '#AAA');
        }
    }

    checkHazards() {
        const tx = Math.round(this.player.x);
        const ty = Math.round(this.player.y);

        // Check adjacent tiles for hazards
        for (let dy = -1; dy <= 1; dy++) {
            for (let dx = -1; dx <= 1; dx++) {
                const tile = this.world.getTile(tx + dx, ty + dy);
                if (tile.type === 5) { // Gas
                    this.player.stamina -= 0.5;
                    if (Math.random() < 0.01) {
                        this.addNotification('Gas pocket! Stamina draining fast!', '#4AFF4A');
                        this.audio.playHazard();
                    }
                }
                if (tile.type === 6) { // Water
                    this.player.stamina -= 0.3;
                    if (Math.random() < 0.01) {
                        this.addNotification('Flooded area! Watch your stamina!', '#4A90D9');
                        this.audio.playHazard();
                    }
                }
            }
        }
    }

    returnToSurface() {
        this.audio.playReturn();

        // Calculate estimated haul value
        let estimatedValue = 0;
        for (const item of this.player.inventory) {
            const best = this.economy.getBestPrice(item);
            estimatedValue += best.price;
        }

        // Show expedition summary
        this.expedition.estimatedValue = estimatedValue;
        this.state = GAME_STATES.EXPEDITION_SUMMARY;

        // Move player to surface
        this.player.x = PLAYER.startX;
        this.player.y = PLAYER.startY;
        this.player.vy = 0;
        this.player.vx = 0;
        this.player.digging = false;

        // Advance market between expeditions
        this.economy.advanceMarket();
        this.contracts.advanceContracts();

        // Recover some stamina
        this.player.recoverStamina(this.player.maxStamina * 0.5);
    }

    startExpedition() {
        this.expedition = {
            maxDepth: 0,
            itemsFound: 0,
            tilesDug: 0,
            startTime: Date.now(),
            items: [],
            estimatedValue: 0
        };

        // Apply upgrades to player
        this.upgrades.applyToPlayer(this.player);

        this.state = GAME_STATES.DIGGING;
        this.addNotification('Descending into The Column...', '#4FC3F7');
    }

    draw() {
        this.renderer.clear();

        switch (this.state) {
            case GAME_STATES.MENU:
                this.handleMenuUI();
                break;

            case GAME_STATES.SURFACE:
            case GAME_STATES.DIGGING:
                this.renderer.drawWorld(this.world, this.camera, this.player);
                this.particles.draw(this.ctx, this.camera);
                this.renderer.drawHUD(this.player, this.state, this.expedition);
                break;

            case GAME_STATES.MARKET:
                this.handleMarketUI();
                break;

            case GAME_STATES.UPGRADES:
                this.handleUpgradesUI();
                break;

            case GAME_STATES.CONTRACTS:
                this.handleContractsUI();
                break;

            case GAME_STATES.INVENTORY:
                this.ui.drawInventory(this.player);
                break;

            case GAME_STATES.EXPEDITION_SUMMARY:
                this.handleSummaryUI();
                break;

            case GAME_STATES.PREP:
                this.handlePrepUI();
                break;
        }

        // Draw notifications on top
        this.drawNotifications();
    }

    handleMenuUI() {
        const result = this.ui.drawMenu();
        if (result?.action === 'start') {
            this.newGame();
        }
    }

    handleMarketUI() {
        const result = this.ui.drawMarket(this.economy, this.player, this.contracts, this.particles);
        if (!result) return;

        if (result.action === 'sell' && result.index >= 0) {
            const item = this.player.inventory[result.index];
            const price = this.economy.sell(item, result.factionId);

            // Apply sell bonus from upgrades
            const bonus = Math.round(price * this.upgrades.effects.sellBonus);
            const total = price + bonus;

            this.player.gold += total;
            this.player.inventory.splice(result.index, 1);
            this.audio.playSell(total);
            this.particles.emitCoins(CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2, Math.min(20, Math.ceil(total / 10)));

            // Check contracts
            const contributions = this.contracts.checkSale(item, result.factionId);
            for (const c of contributions) {
                if (c.completed) {
                    this.addNotification(`Contract complete: ${c.contract.description}!`, '#4CAF50');
                }
            }

            const bonusText = bonus > 0 ? ` (+${bonus} bonus)` : '';
            this.addNotification(`Sold ${item.name} for ${total} G${bonusText}`, '#FFD700');
        }

        if (result.action === 'sell_all') {
            let totalGold = 0;
            const items = [...this.player.inventory];
            for (const item of items) {
                const price = this.economy.sell(item, result.factionId);
                if (price > 0) {
                    const bonus = Math.round(price * this.upgrades.effects.sellBonus);
                    totalGold += price + bonus;
                    this.contracts.checkSale(item, result.factionId);
                }
            }
            if (totalGold > 0) {
                this.player.gold += totalGold;
                this.player.inventory = [];
                this.audio.playSell(totalGold);
                this.particles.emitCoins(CANVAS_WIDTH / 2, CANVAS_HEIGHT / 2, Math.min(30, Math.ceil(totalGold / 10)));
                this.addNotification(`Sold all for ${totalGold} G!`, '#FFD700');
            }
        }

        if (result.action === 'back') {
            this.state = GAME_STATES.SURFACE;
        }
    }

    handleUpgradesUI() {
        const result = this.ui.drawUpgrades(this.upgrades, this.player);
        if (!result) return;

        if (result.action === 'buy') {
            const cost = this.upgrades.getCost(result.upgradeId);
            if (this.upgrades.canPurchase(result.upgradeId, this.player.gold)) {
                this.upgrades.purchase(result.upgradeId);
                this.player.gold -= cost;
                this.upgrades.applyToPlayer(this.player);
                this.audio.playUpgrade();
                this.addNotification('Upgrade purchased!', '#4CAF50');
            }
        }

        if (result.action === 'back') {
            this.state = GAME_STATES.SURFACE;
        }
    }

    handleContractsUI() {
        const result = this.ui.drawContracts(this.contracts, this.economy);
        if (!result) return;

        if (result.action === 'accept') {
            this.contracts.acceptContract(result.contractId);
            this.audio.playClick();
            this.addNotification('Contract accepted!', '#4FC3F7');
        }

        if (result.action === 'claim') {
            const rewards = this.contracts.claimRewards();
            this.player.gold += rewards.totalGold;
            for (const [fid, rep] of Object.entries(rewards.repGains)) {
                this.economy.reputation[fid] = Math.min(100, this.economy.reputation[fid] + rep);
            }
            if (rewards.totalGold > 0) {
                this.audio.playSell(rewards.totalGold);
                this.addNotification(`Claimed ${rewards.totalGold} G in contract rewards!`, '#FFD700');
            }
        }

        if (result.action === 'back') {
            this.state = GAME_STATES.SURFACE;
        }
    }

    handleSummaryUI() {
        const result = this.ui.drawExpeditionSummary(this.expedition);
        if (result?.action === 'continue') {
            this.state = GAME_STATES.SURFACE;
        }
    }

    handlePrepUI() {
        const result = this.ui.drawPrep(this.player, this.economy);
        if (!result) return;

        if (result.action === 'rest') {
            this.player.recoverStamina(this.player.maxStamina);
            this.economy.advanceMarket();
            this.addNotification('Fully rested. Market has shifted.', '#81C784');
        }

        if (result.action === 'descend') {
            this.startExpedition();
        }

        if (result.action === 'back') {
            this.state = GAME_STATES.SURFACE;
        }
    }

    // Notifications
    addNotification(text, color) {
        this.notifications.push({ text, color, timer: 0, duration: 3 });
        if (this.notifications.length > 5) this.notifications.shift();
    }

    updateNotifications(dt) {
        for (let i = this.notifications.length - 1; i >= 0; i--) {
            this.notifications[i].timer += dt;
            if (this.notifications[i].timer >= this.notifications[i].duration) {
                this.notifications.splice(i, 1);
            }
        }
    }

    drawNotifications() {
        for (let i = 0; i < this.notifications.length; i++) {
            const n = this.notifications[i];
            const progress = n.timer / n.duration;
            this.renderer.drawNotification(n.text, n.color, progress, i);
        }
    }
}

const canvas = document.getElementById('gameCanvas');
const game = new Game(canvas);
game.start();

})();
