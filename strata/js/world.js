import { TILE_SIZE, WORLD_WIDTH, WORLD_HEIGHT, SURFACE_HEIGHT, STRATA, TILES } from './config.js';
import { rollItem } from './items.js';

export class World {
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
