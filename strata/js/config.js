// Game configuration constants
export const TILE_SIZE = 32;
export const WORLD_WIDTH = 60;  // tiles wide
export const WORLD_HEIGHT = 200; // tiles tall (covers all 4 strata)
export const SURFACE_HEIGHT = 5; // rows of sky/surface before underground

export const CANVAS_WIDTH = 800;
export const CANVAS_HEIGHT = 600;

// Strata definitions
export const STRATA = [
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
export const PLAYER = {
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
export const ECONOMY = {
    priceFluctuationRate: 0.02,
    demandDecayRate: 0.01,
    supplyImpactFactor: 0.05,
    eventDuration: 5, // expeditions
    startingGold: 50
};

// Tile types
export const TILES = {
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
export const GAME_STATES = {
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
export const RARITY = {
    COMMON: { name: 'Common', color: '#AAAAAA', multiplier: 1.0 },
    UNCOMMON: { name: 'Uncommon', color: '#4FC3F7', multiplier: 1.5 },
    RARE: { name: 'Rare', color: '#AB47BC', multiplier: 2.5 },
    LEGENDARY: { name: 'Legendary', color: '#FFD700', multiplier: 5.0 }
};
