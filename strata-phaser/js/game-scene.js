// STRATA - Game Scene (Underground Gameplay)
// ============================================
// Contains: world generation, player, digging, HUD, lighting
(function(S) {
    'use strict';

    S.GameScene = new Phaser.Class({
        Extends: Phaser.Scene,

        initialize: function GameScene() {
            Phaser.Scene.call(this, { key: 'GameScene' });
            this.worldData = null;
            this.itemData = null;
            this.player = null;
            this.cursors = null;
            this.isDigging = false;
            this.digTarget = null;
            this.digProgress = 0;
            this.digTime = 0;
            this.facingRight = true;
            this.isUnderground = false;
            this.notifications = [];
        },

        create: function() {
            var self = this;

            // Reset expedition state
            S.state.expedition.number++;
            S.state.expedition.maxDepth = 0;
            S.state.expedition.itemsFound = 0;
            S.state.expedition.tilesDug = 0;
            S.state.expedition.startTime = this.time.now;
            S.state.expedition.items = [];
            S.recalcEffects();

            // ---- Generate world ----
            this.generateWorld();

            // ---- Create tilemap ----
            this.createTilemap();

            // ---- Create player ----
            this.createPlayer();

            // ---- Camera ----
            this.cameras.main.startFollow(this.player, true, 0.1, 0.1);
            this.cameras.main.setBounds(0, -S.HEIGHT / 3, S.WORLD_W * S.TILE, S.WORLD_H * S.TILE + S.HEIGHT / 3);
            this.cameras.main.setBackgroundColor('#000000');

            // ---- Input ----
            this.cursors = this.input.keyboard.createCursorKeys();
            this.keys = {
                W: this.input.keyboard.addKey('W'),
                A: this.input.keyboard.addKey('A'),
                S: this.input.keyboard.addKey('S'),
                D: this.input.keyboard.addKey('D'),
                E: this.input.keyboard.addKey('E'),
                I: this.input.keyboard.addKey('I'),
                M: this.input.keyboard.addKey('M'),
                U: this.input.keyboard.addKey('U'),
                C: this.input.keyboard.addKey('C'),
                Q: this.input.keyboard.addKey('Q'),
                SHIFT: this.input.keyboard.addKey('SHIFT'),
                SPACE: this.input.keyboard.addKey('SPACE'),
                ESC: this.input.keyboard.addKey('ESC')
            };
            this.playerAutoDig = false; // toggled by Q

            // ---- Dig progress bar ----
            this.digBar = this.add.graphics();
            this.digBar.setDepth(10);

            // ---- Lighting overlay ----
            this.createLighting();

            // ---- HUD ----
            this.createHUD();

            // ---- Sky background ----
            if (this.textures.exists('sky')) {
                this.skyBg = this.add.image(S.WORLD_W * S.TILE / 2, S.HEIGHT / 2 - S.HEIGHT / 3, 'sky');
                this.skyBg.setScrollFactor(0.1, 0.3);
                this.skyBg.setDepth(-10);
            }

            // ---- Parallax backgrounds per stratum ----
            this.bgTiles = [];
            for (var s = 1; s <= 4; s++) {
                if (this.textures.exists('bg_' + s)) {
                    var stratum = S.STRATA[s];
                    var bgY = (stratum.startY + stratum.endY) / 2 * S.TILE;
                    var bg = this.add.tileSprite(
                        S.WORLD_W * S.TILE / 2, bgY,
                        S.WORLD_W * S.TILE + S.WIDTH, (stratum.endY - stratum.startY) * S.TILE,
                        'bg_' + s
                    );
                    bg.setScrollFactor(0.3, 0.8);
                    bg.setDepth(-5);
                    bg.setAlpha(0.4);
                    this.bgTiles.push(bg);
                }
            }

            // ---- Particle emitter setup ----
            this.setupParticles();

            // ---- Surface decorations ----
            this.createSurfaceDecorations();

            // ---- NPC Diggers ----
            this.createNPCDiggers();

            // ---- Entrance transition ----
            this.cameras.main.fadeIn(500, 0, 0, 0);

            // ---- Initial notification ----
            this.showNotification('Expedition #' + S.state.expedition.number + ' - Go deeper!', 0xFFD700);
        },

        // ==================================================================
        // WORLD GENERATION
        // ==================================================================
        generateWorld: function() {
            this.worldData = [];
            this.itemData = [];

            for (var y = 0; y < S.WORLD_H; y++) {
                this.worldData[y] = [];
                this.itemData[y] = [];
                for (var x = 0; x < S.WORLD_W; x++) {
                    this.worldData[y][x] = this.getTileAt(x, y);
                    this.itemData[y][x] = null;
                }
            }

            // Place items
            for (y = S.SURFACE_Y; y < S.WORLD_H; y++) {
                for (x = 0; x < S.WORLD_W; x++) {
                    var tile = this.worldData[y][x];
                    if (tile === S.T.DIRT || tile === S.T.ROCK) {
                        var stratum = S.getStratumAt(y);
                        if (stratum.id > 0 && stratum.itemDensity) {
                            if (Math.random() < stratum.itemDensity) {
                                var item = S.rollItem(stratum.id);
                                if (item) {
                                    this.worldData[y][x] = S.T.ITEM;
                                    this.itemData[y][x] = item;
                                }
                            }
                        }
                    }
                }
            }
        },

        getTileAt: function(x, y) {
            // Sky
            if (y < S.SURFACE_Y - 1) return S.T.AIR;

            // Surface grass
            if (y === S.SURFACE_Y - 1) return S.T.SURFACE_GRASS;

            // Buildings - market area center, plus flanking structures
            var midX = Math.floor(S.WORLD_W / 2);
            // Main market building (wider)
            if (y === S.SURFACE_Y - 2 && x >= midX - 4 && x <= midX + 4) return S.T.BUILDING;
            if (y === S.SURFACE_Y - 3 && x >= midX - 3 && x <= midX + 3) return S.T.BUILDING;
            // Left workshop (The Foundry area)
            if (y === S.SURFACE_Y - 2 && x >= midX - 12 && x <= midX - 8) return S.T.BUILDING;
            if (y === S.SURFACE_Y - 3 && x >= midX - 11 && x <= midX - 9) return S.T.BUILDING;
            // Right shop (Finn's Fences area)
            if (y === S.SURFACE_Y - 2 && x >= midX + 8 && x <= midX + 12) return S.T.BUILDING;
            if (y === S.SURFACE_Y - 3 && x >= midX + 9 && x <= midX + 11) return S.T.BUILDING;

            // Below all strata = bedrock
            if (y >= S.STRATA[S.STRATA.length - 1].endY) return S.T.BEDROCK;

            // Left and right edges = bedrock
            if (x <= 0 || x >= S.WORLD_W - 1) return S.T.BEDROCK;

            // Underground noise-based generation
            var noise = S.noise(x, y);
            var stratum = S.getStratumAt(y);

            // Caves
            if (noise > 0.92) return S.T.AIR;

            // Hard rock (deeper strata)
            if (noise > 0.88 && stratum.id >= 2) return S.T.HARD_ROCK;

            // Regular rock
            if (noise > 0.75) return S.T.ROCK;

            // Hazards
            if (noise > 0.70 && noise < 0.73) {
                var hazards = stratum.hazards;
                if (hazards.indexOf('gas') !== -1 || hazards.indexOf('unstable') !== -1) return S.T.HAZARD_GAS;
                if (hazards.indexOf('flood') !== -1 || hazards.indexOf('rust') !== -1) return S.T.HAZARD_WATER;
            }

            return S.T.DIRT;
        },

        // ==================================================================
        // TILEMAP CREATION
        // ==================================================================
        createTilemap: function() {
            // Use Phaser's built-in tilemap with tile sprites rendered as image objects
            // We'll use a group of sprites for visible tiles (chunked rendering)

            this.tileGroup = this.add.group();
            this.tileSprites = [];

            // We render only visible tiles and update as camera moves
            this.lastCamX = -999;
            this.lastCamY = -999;
            this.visibleTiles = {};

            // Pre-create a pool of sprites for tile rendering
            this.tilePool = [];
            var poolSize = 40 * 25; // enough for screen plus margins
            for (var i = 0; i < poolSize; i++) {
                var ts = this.add.rectangle(0, 0, S.TILE, S.TILE, 0x000000);
                ts.setOrigin(0, 0);
                ts.setVisible(false);
                ts.setDepth(0);
                this.tilePool.push(ts);
            }
            this.tilePoolIndex = 0;

            // Item sparkle overlays
            this.itemSparkles = {};

            // For collision, we'll check worldData directly (custom collision)
            this.updateVisibleTiles();
        },

        updateVisibleTiles: function() {
            var cam = this.cameras.main;
            var startX = Math.max(0, Math.floor((cam.scrollX - S.TILE) / S.TILE));
            var startY = Math.max(0, Math.floor((cam.scrollY - S.TILE) / S.TILE));
            var endX = Math.min(S.WORLD_W, Math.ceil((cam.scrollX + cam.width + S.TILE) / S.TILE));
            var endY = Math.min(S.WORLD_H, Math.ceil((cam.scrollY + cam.height + S.TILE) / S.TILE));

            // Reset pool
            for (var pi = 0; pi < this.tilePool.length; pi++) {
                this.tilePool[pi].setVisible(false);
            }
            this.tilePoolIndex = 0;

            // Remove old sparkles
            var oldSparkles = Object.keys(this.itemSparkles);
            for (var si = 0; si < oldSparkles.length; si++) {
                if (this.itemSparkles[oldSparkles[si]]) {
                    this.itemSparkles[oldSparkles[si]].setVisible(false);
                }
            }

            for (var y = startY; y < endY; y++) {
                for (var x = startX; x < endX; x++) {
                    if (!this.worldData[y]) continue;
                    var tile = this.worldData[y][x];
                    if (tile === S.T.AIR) continue;

                    var color = this.getTileColor(tile, x, y);
                    if (color === null) continue;

                    if (this.tilePoolIndex >= this.tilePool.length) continue;

                    var sprite = this.tilePool[this.tilePoolIndex++];
                    sprite.setPosition(x * S.TILE, y * S.TILE);
                    sprite.setFillStyle(color.fill, 1);
                    sprite.setVisible(true);

                    // Item tiles get a pulsing sparkle effect
                    if (tile === S.T.ITEM) {
                        var key = x + '_' + y;
                        if (!this.itemSparkles[key]) {
                            this.itemSparkles[key] = this.add.circle(
                                x * S.TILE + S.TILE / 2,
                                y * S.TILE + S.TILE / 2,
                                4, 0xFFD700, 0.5
                            ).setDepth(1);
                        }
                        var sparkle = this.itemSparkles[key];
                        sparkle.setVisible(true);
                        var itemDef = this.itemData[y][x];
                        if (itemDef) {
                            sparkle.setFillStyle(S.RARITY[itemDef.rarity].color, 0.5);
                        }
                    }
                }
            }
        },

        getTileColor: function(tile, x, y) {
            var stratum = S.getStratumAt(y);
            var h = S.hash(x, y) % 1000;
            var sc = stratum.colors;

            // Stratum boundary blending - fade between strata colors near boundaries
            var boundaryBlend = 0;
            if (stratum.id > 0 && stratum.id < 4) {
                var distFromEnd = stratum.endY - y;
                if (distFromEnd <= 3 && distFromEnd > 0) {
                    boundaryBlend = (3 - distFromEnd) / 3;
                }
            }

            switch (tile) {
                case S.T.DIRT:
                    var baseC = sc.dirt || sc.base;
                    if (h < 200) baseC = S.brighten(baseC, 10);
                    else if (h < 400) baseC = S.darken(baseC, 6);
                    else if (h < 500) baseC = S.brighten(baseC, 4);
                    // Blend at stratum boundaries
                    if (boundaryBlend > 0 && stratum.id < 4) {
                        var nextSc = S.STRATA[stratum.id + 1].colors;
                        baseC = S.lerpColor(baseC, nextSc.dirt || nextSc.base, boundaryBlend * 0.5);
                    }
                    return { fill: baseC };

                case S.T.ROCK:
                    var rockC = sc.base;
                    if (h < 250) rockC = S.brighten(rockC, 14);
                    else if (h < 500) rockC = S.darken(rockC, 6);
                    else if (h < 700) rockC = S.brighten(rockC, 6);
                    return { fill: rockC };

                case S.T.HARD_ROCK:
                    var hrC = sc.accent2 || S.brighten(sc.base, 20);
                    if (h < 300) hrC = S.brighten(hrC, 12);
                    else if (h < 500) hrC = S.darken(hrC, 5);
                    return { fill: hrC };

                case S.T.ITEM:
                    // Show as dirt with warmth hint
                    return { fill: S.brighten(sc.dirt || sc.base, 12) };

                case S.T.HAZARD_GAS:
                    // Animated green tint
                    var gasPulse = (h % 3) * 4;
                    return { fill: S.brighten(0x2D3020, gasPulse) };

                case S.T.HAZARD_WATER:
                    // Wet blue-dark
                    var waterPulse = (h % 3) * 3;
                    return { fill: S.brighten(0x1A2A3A, waterPulse) };

                case S.T.BEDROCK:
                    var brc = 0x0E0E0E;
                    if (h < 300) brc = 0x141414;
                    return { fill: brc };

                case S.T.SURFACE_GRASS:
                    return { fill: h < 400 ? 0x4A7A2E : 0x3A6A1E };

                case S.T.BUILDING:
                    return { fill: h < 500 ? 0x8B6C3C : 0x7B5C2C };

                default:
                    return null;
            }
        },

        isSolid: function(tileX, tileY) {
            if (tileX < 0 || tileX >= S.WORLD_W || tileY < 0 || tileY >= S.WORLD_H) return true;
            var t = this.worldData[tileY][tileX];
            return t !== S.T.AIR;
        },

        isDiggable: function(tileX, tileY) {
            if (tileX < 0 || tileX >= S.WORLD_W || tileY < 0 || tileY >= S.WORLD_H) return false;
            var t = this.worldData[tileY][tileX];
            return t === S.T.DIRT || t === S.T.ROCK || t === S.T.HARD_ROCK ||
                   t === S.T.ITEM || t === S.T.HAZARD_GAS || t === S.T.HAZARD_WATER ||
                   t === S.T.SURFACE_GRASS;
        },

        getHardness: function(tileX, tileY) {
            if (!this.worldData[tileY]) return 99;
            var t = this.worldData[tileY][tileX];
            if (t === S.T.SURFACE_GRASS) return 0;
            var stratum = S.getStratumAt(tileY);
            if (t === S.T.HARD_ROCK) return stratum.hardness + 1;
            return stratum.hardness;
        },

        // ==================================================================
        // PLAYER CREATION
        // ==================================================================
        createPlayer: function() {
            var startX = Math.floor(S.WORLD_W / 2) * S.TILE + S.TILE / 2;
            var startY = (S.SURFACE_Y - 2) * S.TILE;

            this.player = this.add.rectangle(startX, startY, 20, 28, 0xE8C840, 0);
            this.player.setOrigin(0.5, 1);
            this.player.setDepth(4); // invisible anchor, body drawn by playerBody graphics

            // Player physics properties (manual implementation)
            this.playerVX = 0;
            this.playerVY = 0;
            this.playerOnGround = false;
            this.playerTileX = Math.floor(startX / S.TILE);
            this.playerTileY = Math.floor(startY / S.TILE);

            // Player visual components
            this.playerBody = this.add.graphics();
            this.playerBody.setDepth(5);

            // Headlamp beam graphics (drawn under lighting)
            this.headlampBeam = this.add.graphics();
            this.headlampBeam.setDepth(3);

            // Track previous ground state for landing puff
            this.wasOnGround = true;

            this.drawPlayer();
        },

        drawPlayer: function() {
            var g = this.playerBody;
            g.clear();

            var px = this.player.x;
            var py = this.player.y;
            var flip = this.facingRight ? 1 : -1;
            var t = this.time.now;

            // Idle breathing: subtle vertical bob
            var breathOffset = 0;
            var armSwing = 0;
            if (this.playerVX === 0 && !this.isDigging && this.playerOnGround) {
                breathOffset = Math.sin(t * 0.003) * 0.8;
                armSwing = Math.sin(t * 0.003) * 0.5;
            }

            // Walk cycle
            var walkPhase = 0;
            var legL = 0, legR = 0;
            if (this.playerVX !== 0) {
                walkPhase = t * 0.012;
                legL = Math.sin(walkPhase) * 3;
                legR = Math.sin(walkPhase + Math.PI) * 3;
                armSwing = Math.sin(walkPhase) * 2;
            }

            // Dig animation phase
            var digSwing = 0;
            if (this.isDigging && this.digTime > 0) {
                digSwing = Math.sin(t * 0.015) * 8;
            }

            var bodyY = py + breathOffset;

            // Shadow (subtle)
            g.fillStyle(0x000000, 0.15);
            g.fillEllipse(px, py + 1, 16, 4);

            // Boots
            g.fillStyle(0x4C2C0C, 1);
            g.fillRect(px - 5, bodyY - 1, 4, 2);
            g.fillRect(px + 1 + legR, bodyY - 1, 4, 2);
            // Boot soles
            g.fillStyle(0x3C1C00, 1);
            g.fillRect(px - 6, bodyY, 5, 1);
            g.fillRect(px + legR, bodyY, 5, 1);

            // Legs with walk animation
            g.fillStyle(0x6B4C2C, 1);
            g.fillRect(px - 4, bodyY - 4 + legL * 0.3, 3, 4);
            g.fillRect(px + 1 + legR * 0.3, bodyY - 4 + legR * 0.3, 3, 4);

            // Body
            g.fillStyle(0xC8A060, 1);
            g.fillRect(px - 6, bodyY - 14, 12, 11);
            // Jacket detail
            g.fillStyle(0xB89050, 0.5);
            g.fillRect(px - 1, bodyY - 14, 1, 10);
            // Pocket
            g.fillStyle(0xA88040, 0.4);
            g.fillRect(px + 2 * flip, bodyY - 10, 3, 3);

            // Belt with buckle
            g.fillStyle(0x8B6914, 1);
            g.fillRect(px - 6, bodyY - 4, 12, 2);
            g.fillStyle(0xDAA520, 0.7);
            g.fillRect(px - 1, bodyY - 4, 2, 2);

            // Arms
            g.fillStyle(0xC8A060, 1);
            if (this.isDigging) {
                // Dig pose - arm swings with pickaxe
                var armX = px + flip * 6;
                var armEndY = bodyY - 14 + Math.abs(digSwing) * 0.3;
                g.fillRect(armX, armEndY, 3, 9);
                // Other arm bracing
                g.fillRect(px - flip * 7, bodyY - 12, 3, 7);
                // Pickaxe with swing
                g.fillStyle(0x6B4912, 1);
                var pickBaseX = armX + flip * 2;
                var pickAngle = digSwing;
                g.fillRect(pickBaseX, armEndY - 6 + pickAngle * 0.3, 2, 12);
                // Pickaxe head
                g.fillStyle(0x999999, 1);
                g.fillRect(pickBaseX - 2, armEndY - 8 + pickAngle * 0.3, 6, 3);
                g.fillStyle(0xBBBBBB, 0.6);
                g.fillRect(pickBaseX - 1, armEndY - 8 + pickAngle * 0.3, 4, 1);
            } else {
                // Normal arms with swing
                g.fillRect(px - 8, bodyY - 13 + armSwing, 3, 8);
                g.fillRect(px + 5, bodyY - 13 - armSwing, 3, 8);
                // Glove cuffs
                g.fillStyle(0x8B6914, 0.5);
                g.fillRect(px - 8, bodyY - 13 + armSwing, 3, 1);
                g.fillRect(px + 5, bodyY - 13 - armSwing, 3, 1);
            }

            // Face
            g.fillStyle(0xE8B88C, 1);
            g.fillRect(px - 5, bodyY - 22, 10, 8);
            // Chin shadow
            g.fillStyle(0xD0A070, 0.3);
            g.fillRect(px - 4, bodyY - 15, 8, 1);

            // Eye - blinks occasionally
            var blink = (t % 4000) < 100;
            g.fillStyle(0x000000, 1);
            if (blink) {
                g.fillRect(px + flip * 2, bodyY - 18, 2, 1);
            } else {
                g.fillRect(px + flip * 2, bodyY - 19, 2, 2);
                // Eye highlight
                g.fillStyle(0xFFFFFF, 0.5);
                g.fillRect(px + flip * 2, bodyY - 19, 1, 1);
            }

            // Hard hat with shine
            g.fillStyle(0xE8C840, 1);
            g.fillRect(px - 6, bodyY - 28, 12, 5);
            g.fillRect(px - 7, bodyY - 24, 14, 3);
            // Hat shine
            g.fillStyle(0xFFE870, 0.3);
            g.fillRect(px - 4, bodyY - 28, 6, 1);

            // Headlamp (brighter, more visible)
            g.fillStyle(0xFFFF88, 1);
            g.fillRect(px + flip * 2 - 1, bodyY - 27, 3, 2);
            // Lamp lens highlight
            g.fillStyle(0xFFFFCC, 0.8);
            g.fillRect(px + flip * 2, bodyY - 27, 1, 1);

            // Draw headlamp beam when underground
            this.headlampBeam.clear();
            if (this.isUnderground) {
                var beamLen = S.state.effects.headlampRadius * S.TILE * 0.6;
                var beamX = px + flip * 5;
                var beamY = bodyY - 26;
                this.headlampBeam.fillStyle(0xFFFF88, 0.04);
                // Triangular beam shape
                this.headlampBeam.beginPath();
                this.headlampBeam.moveTo(beamX, beamY);
                this.headlampBeam.lineTo(beamX + flip * beamLen, beamY - beamLen * 0.25);
                this.headlampBeam.lineTo(beamX + flip * beamLen, beamY + beamLen * 0.35);
                this.headlampBeam.closePath();
                this.headlampBeam.fillPath();
                // Inner bright beam
                this.headlampBeam.fillStyle(0xFFFF88, 0.03);
                this.headlampBeam.beginPath();
                this.headlampBeam.moveTo(beamX, beamY);
                this.headlampBeam.lineTo(beamX + flip * beamLen * 0.6, beamY - beamLen * 0.1);
                this.headlampBeam.lineTo(beamX + flip * beamLen * 0.6, beamY + beamLen * 0.15);
                this.headlampBeam.closePath();
                this.headlampBeam.fillPath();
            }
        },

        // ==================================================================
        // LIGHTING
        // ==================================================================
        createLighting: function() {
            // RenderTexture for darkness overlay with headlamp cutout
            this.lightRT = this.add.renderTexture(0, 0, S.WIDTH, S.HEIGHT);
            this.lightRT.setDepth(15);
            this.lightRT.setScrollFactor(0);

            // Light gradient sprite (used to erase darkness for headlamp)
            if (this.textures.exists('light_mask')) {
                this.lightSprite = this.make.image({ key: 'light_mask', add: false });
            }

            // Ambient timer for underground sounds
            this.ambientTimer = 0;
        },

        updateLighting: function() {
            // Gradual darkness fade-in near surface
            var nearSurface = this.playerTileY >= S.SURFACE_Y - 2 && this.playerTileY < S.SURFACE_Y + 4;
            if (!this.isUnderground && !nearSurface) {
                this.lightRT.setAlpha(0);
                return;
            }

            // Darkness intensity increases with depth
            var depthFactor = Math.min(1, Math.max(0, (this.playerTileY - S.SURFACE_Y + 2) / 6));
            var darkness = nearSurface && !this.isUnderground ? depthFactor * 0.3 : 0.7 + depthFactor * 0.2;

            this.lightRT.setAlpha(1);
            this.lightRT.clear();
            this.lightRT.fill(0x000000, darkness);

            var cam = this.cameras.main;
            var screenX = this.player.x - cam.scrollX;
            var screenY = this.player.y - 14 - cam.scrollY;
            var radius = S.state.effects.headlampRadius * S.TILE;

            if (this.lightSprite) {
                var scale = (radius * 2) / 512;

                // Main ambient glow around player (warm close light)
                this.lightSprite.setScale(scale * 0.6);
                this.lightRT.erase(this.lightSprite, screenX, screenY);

                // Directional headlamp cone - wider and more pronounced
                var dirX = this.facingRight ? 1 : -1;
                var coneX = screenX + dirX * radius * 0.4;
                var coneY = screenY - radius * 0.08;
                this.lightSprite.setScale(scale * 1.0, scale * 0.55);
                this.lightRT.erase(this.lightSprite, coneX, coneY);

                // Secondary forward glow for beam visibility
                var farX = screenX + dirX * radius * 0.6;
                this.lightSprite.setScale(scale * 0.5, scale * 0.3);
                this.lightRT.erase(this.lightSprite, farX, coneY);

                // Soft ambient fill close to player
                this.lightSprite.setScale(scale * 0.35);
                this.lightRT.erase(this.lightSprite, screenX, screenY + 4);
            }

            // Stratum-colored ambient tint - stronger for atmosphere
            var stratum = S.getStratumAt(this.playerTileY);
            if (stratum.id > 0 && stratum.colors.accent1) {
                var tintStrength = 0.03 + stratum.id * 0.01;
                this.lightRT.fill(stratum.colors.accent1, tintStrength);
            }
        },

        // ==================================================================
        // HUD
        // ==================================================================
        createHUD: function() {
            var self = this;
            this.hudGroup = this.add.group();

            // Stamina bar background
            this.hudStaminaBg = this.add.rectangle(120, 20, 152, 18, 0x333333, 0.8)
                .setScrollFactor(0).setDepth(20).setOrigin(0, 0);
            this.hudStaminaBar = this.add.rectangle(121, 21, 150, 16, 0x44FF44, 1)
                .setScrollFactor(0).setDepth(21).setOrigin(0, 0);
            this.hudStaminaText = this.add.text(196, 22, '100', {
                fontSize: '11px', fontFamily: 'monospace', color: '#FFFFFF'
            }).setScrollFactor(0).setDepth(22).setOrigin(0.5, 0);

            // Stamina label
            this.add.text(16, 22, 'STAMINA', {
                fontSize: '11px', fontFamily: 'monospace', color: '#44FF44', fontStyle: 'bold'
            }).setScrollFactor(0).setDepth(20);

            // Gold display
            this.hudGold = this.add.text(16, 46, 'GOLD: ' + S.state.gold, {
                fontSize: '13px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setScrollFactor(0).setDepth(20);

            // Depth display
            this.hudDepth = this.add.text(16, 66, 'Depth: 0m', {
                fontSize: '12px', fontFamily: 'monospace', color: '#4FC3F7'
            }).setScrollFactor(0).setDepth(20);

            // Stratum name
            this.hudStratum = this.add.text(16, 84, 'Surface', {
                fontSize: '12px', fontFamily: 'monospace', color: '#AAAAAA'
            }).setScrollFactor(0).setDepth(20);

            // Inventory count
            this.hudInventory = this.add.text(16, 104, 'Bag: 0/' + S.state.effects.inventorySlots, {
                fontSize: '12px', fontFamily: 'monospace', color: '#C8A060'
            }).setScrollFactor(0).setDepth(20);

            // Items found this expedition
            this.hudItemsFound = this.add.text(16, 122, 'Found: 0', {
                fontSize: '11px', fontFamily: 'monospace', color: '#AAAAAA'
            }).setScrollFactor(0).setDepth(20);

            // NPC Digger status panel
            this.hudDiggerLabel = this.add.text(16, 142, '', {
                fontSize: '10px', fontFamily: 'monospace', color: '#E07040', fontStyle: 'bold'
            }).setScrollFactor(0).setDepth(20);
            this.hudDiggerLines = [];
            for (var di = 0; di < 5; di++) {
                var dline = this.add.text(16, 156 + di * 14, '', {
                    fontSize: '10px', fontFamily: 'monospace', color: '#888888'
                }).setScrollFactor(0).setDepth(20);
                this.hudDiggerLines.push(dline);
            }
            this.hudDiggerIcons = this.add.graphics().setScrollFactor(0).setDepth(20);

            // Depth sidebar (right side)
            this.hudDepthSidebar = this.add.graphics();
            this.hudDepthSidebar.setScrollFactor(0).setDepth(20);

            // Control hints (bottom)
            this.hudControls = this.add.text(S.WIDTH / 2, S.HEIGHT - 16, '', {
                fontSize: '11px', fontFamily: 'monospace', color: '#666666'
            }).setScrollFactor(0).setDepth(20).setOrigin(0.5);

            // Notification container
            this.notifY = S.HEIGHT - 60;

            // Minimap
            this.minimapW = 100;
            this.minimapH = 140;
            this.minimapX = S.WIDTH - this.minimapW - 36;
            this.minimapY = 50;
            this.minimapBg = this.add.graphics();
            this.minimapBg.setScrollFactor(0).setDepth(19);
            this.minimapG = this.add.graphics();
            this.minimapG.setScrollFactor(0).setDepth(20);
            this.exploredTiles = {};
            this.minimapUpdateTimer = 0;
        },

        updateHUD: function() {
            // Stamina bar with smooth width transition
            var stPct = S.state.stamina / S.state.maxStamina;
            var targetW = Math.max(0, 150 * stPct);
            // Smooth the bar width change
            var currW = this.hudStaminaBar.width;
            this.hudStaminaBar.width = currW + (targetW - currW) * 0.15;
            this.hudStaminaText.setText(Math.ceil(S.state.stamina));
            if (stPct > 0.5) this.hudStaminaBar.setFillStyle(0x44FF44);
            else if (stPct > 0.25) this.hudStaminaBar.setFillStyle(0xFFAA00);
            else {
                this.hudStaminaBar.setFillStyle(0xFF4444);
                // Pulse the bar when critically low
                if (stPct > 0 && stPct < 0.15) {
                    this.hudStaminaBg.setAlpha(0.5 + 0.3 * Math.sin(this.time.now * 0.01));
                }
            }

            // Gold
            this.hudGold.setText('GOLD: ' + S.state.gold);

            // Depth
            var depth = S.depthToMeters(this.playerTileY);
            this.hudDepth.setText('Depth: ' + depth + 'm');

            // Stratum
            var stratum = S.getStratumAt(this.playerTileY);
            this.hudStratum.setText(stratum.name);
            this.hudStratum.setColor(S.colorToHex(stratum.colors.accent1 || stratum.colors.base));

            // Inventory
            this.hudInventory.setText('Bag: ' + S.state.inventory.length + '/' + S.state.effects.inventorySlots);

            // Items found
            this.hudItemsFound.setText('Found: ' + S.state.expedition.itemsFound);

            // Control hints
            if (this.isUnderground) {
                var autoLabel = S.state.autoDigEnabled ? ('  [Q] Auto-Dig ' + (this.playerAutoDig ? 'ON' : 'OFF')) : '';
                this.hudControls.setText('[WASD] Move  [DOWN] Dig  [SHIFT+Dir] Dig Sideways  [E] Return' + autoLabel);
            } else {
                this.hudControls.setText('[WASD] Move  [DOWN] Dig Down  [M] Market  [U] Upgrades  [C] Contracts  [I] Inventory');
            }

            // NPC Digger status
            this.hudDiggerIcons.clear();
            if (this.npcDiggers && this.npcDiggers.length > 0) {
                this.hudDiggerLabel.setText('CREW (' + this.npcDiggers.length + ')');
                for (var di = 0; di < this.hudDiggerLines.length; di++) {
                    if (di < this.npcDiggers.length) {
                        var dnpc = this.npcDiggers[di];
                        var dDepth = S.depthToMeters(dnpc.tileY);
                        var dStratum = S.getStratumAt(dnpc.tileY);
                        var dStatus = dnpc.isDigging ? 'DIG' : (dnpc.vx !== 0 ? 'MOV' : 'IDL');
                        var dText = '  #' + (di + 1) + ' ' + dDepth + 'm ' + dnpc.inventory.length + '/' + dnpc.maxInventory + ' [' + dStatus + ']';
                        this.hudDiggerLines[di].setText(dText);
                        this.hudDiggerLines[di].setColor(dnpc.isDigging ? '#CCCCCC' : '#777777');
                        this.hudDiggerLines[di].setVisible(true);
                        // Hat color icon
                        this.hudDiggerIcons.fillStyle(dnpc.hatColor, 1);
                        this.hudDiggerIcons.fillRect(16, 158 + di * 14, 6, 6);
                    } else {
                        this.hudDiggerLines[di].setVisible(false);
                    }
                }
            } else {
                this.hudDiggerLabel.setText('');
                for (var dj = 0; dj < this.hudDiggerLines.length; dj++) {
                    this.hudDiggerLines[dj].setVisible(false);
                }
            }

            // Depth sidebar
            this.drawDepthSidebar();

            // Track max depth
            if (depth > S.state.expedition.maxDepth) {
                S.state.expedition.maxDepth = depth;
            }

            // Update minimap (throttled to reduce perf cost)
            this.minimapUpdateTimer = (this.minimapUpdateTimer || 0) + 1;
            if (this.minimapUpdateTimer >= 10) {
                this.minimapUpdateTimer = 0;
                this.updateMinimap();
            }
        },

        drawDepthSidebar: function() {
            var g = this.hudDepthSidebar;
            g.clear();

            var sideX = S.WIDTH - 24;
            var sideY = 40;
            var sideH = S.HEIGHT - 100;
            var totalDepth = S.STRATA[S.STRATA.length - 1].endY;

            // Background
            g.fillStyle(0x000000, 0.6);
            g.fillRect(sideX - 4, sideY - 4, 20, sideH + 8);
            g.lineStyle(1, 0x333333, 1);
            g.strokeRect(sideX - 4, sideY - 4, 20, sideH + 8);

            // Strata colors
            for (var i = 0; i <= 4; i++) {
                var s = S.STRATA[i];
                var y1 = sideY + (s.startY / totalDepth) * sideH;
                var y2 = sideY + (s.endY / totalDepth) * sideH;
                g.fillStyle(s.colors.base, 0.8);
                g.fillRect(sideX, y1, 12, y2 - y1);
            }

            // Player position marker
            var playerDepthPct = Math.max(0, this.playerTileY) / totalDepth;
            var markerY = sideY + playerDepthPct * sideH;
            g.fillStyle(0xFFFFFF, 1);
            g.fillTriangle(sideX - 6, markerY, sideX, markerY - 3, sideX, markerY + 3);
        },

        // ==================================================================
        // MINIMAP
        // ==================================================================
        updateMinimap: function() {
            var mx = this.minimapX;
            var my = this.minimapY;
            var mw = this.minimapW;
            var mh = this.minimapH;

            // Mark explored tiles around player
            var viewR = 8;
            for (var dy = -viewR; dy <= viewR; dy++) {
                for (var dx = -viewR; dx <= viewR; dx++) {
                    var tx = this.playerTileX + dx;
                    var ty = this.playerTileY + dy;
                    if (tx >= 0 && tx < S.WORLD_W && ty >= 0 && ty < S.WORLD_H) {
                        this.exploredTiles[tx + ',' + ty] = true;
                    }
                }
            }

            // Draw minimap background
            var bg = this.minimapBg;
            bg.clear();
            bg.fillStyle(0x000000, 0.6);
            bg.fillRoundedRect(mx - 4, my - 4, mw + 8, mh + 8, 4);
            bg.lineStyle(1, 0x444466, 0.6);
            bg.strokeRoundedRect(mx - 4, my - 4, mw + 8, mh + 8, 4);

            var g = this.minimapG;
            g.clear();

            // Scale: show area centered on player
            var viewTilesX = 50;
            var viewTilesY = 70;
            var scaleX = mw / viewTilesX;
            var scaleY = mh / viewTilesY;
            var offsetX = this.playerTileX - viewTilesX / 2;
            var offsetY = this.playerTileY - viewTilesY / 2;

            // Draw explored tiles
            for (var key in this.exploredTiles) {
                var parts = key.split(',');
                var ttx = parseInt(parts[0]);
                var tty = parseInt(parts[1]);
                var sx = mx + (ttx - offsetX) * scaleX;
                var sy = my + (tty - offsetY) * scaleY;

                if (sx < mx || sx > mx + mw || sy < my || sy > my + mh) continue;

                if (tty >= 0 && tty < S.WORLD_H && ttx >= 0 && ttx < S.WORLD_W) {
                    var tile = this.worldData[tty][ttx];
                    if (tile === S.T.AIR) {
                        g.fillStyle(0x111122, 0.5);
                    } else if (tile === S.T.ITEM) {
                        var itemDef = this.itemData[tty] && this.itemData[tty][ttx];
                        g.fillStyle(itemDef ? S.RARITY[itemDef.rarity].color : 0xFFD700, 0.8);
                    } else if (tile === S.T.BEDROCK) {
                        g.fillStyle(0x0A0A0A, 0.8);
                    } else {
                        var stratum = S.getStratumAt(tty);
                        g.fillStyle(stratum.colors.base, 0.5);
                    }
                    g.fillRect(sx, sy, Math.max(1, scaleX), Math.max(1, scaleY));
                }
            }

            // Player position (bright dot)
            var playerSX = mx + (this.playerTileX - offsetX) * scaleX;
            var playerSY = my + (this.playerTileY - offsetY) * scaleY;
            g.fillStyle(0xFFFFFF, 1);
            g.fillRect(playerSX - 1, playerSY - 1, 3, 3);
            g.fillStyle(0xFFFF88, 0.5);
            g.fillRect(playerSX - 2, playerSY - 2, 5, 5);
        },

        // ==================================================================
        // PARTICLES
        // ==================================================================
        setupParticles: function() {
            // Dig particles
            if (this.textures.exists('particle_dirt')) {
                this.digEmitter = this.add.particles(0, 0, 'particle_dirt', {
                    speed: { min: 50, max: 150 },
                    angle: { min: 200, max: 340 },
                    scale: { start: 1.5, end: 0 },
                    lifespan: { min: 300, max: 700 },
                    gravityY: 200,
                    alpha: { start: 1, end: 0 },
                    emitting: false
                });
                this.digEmitter.setDepth(6);
            }

            // Item found particles
            if (this.textures.exists('particle_sparkle')) {
                this.sparkleEmitter = this.add.particles(0, 0, 'particle_sparkle', {
                    speed: { min: 30, max: 100 },
                    angle: { min: 0, max: 360 },
                    scale: { start: 1.5, end: 0 },
                    lifespan: { min: 500, max: 800 },
                    gravityY: 0,
                    alpha: { start: 1, end: 0 },
                    emitting: false,
                    tint: 0xFFD700
                });
                this.sparkleEmitter.setDepth(6);
            }
        },

        // ==================================================================
        // SURFACE DECORATIONS
        // ==================================================================
        createSurfaceDecorations: function() {
            var midX = Math.floor(S.WORLD_W / 2);
            var surfY = (S.SURFACE_Y - 2) * S.TILE;
            var T = S.TILE;
            var decoG = this.add.graphics();
            decoG.setDepth(3);

            // Market sign above center building
            var signX = midX * T;
            var signY = surfY - T * 1.5;
            decoG.fillStyle(0x5C3C1C, 1);
            decoG.fillRect(signX - 2, signY + 12, 4, 20);
            decoG.fillStyle(0x3A2510, 0.9);
            decoG.fillRoundedRect(signX - 40, signY, 80, 16, 2);
            decoG.lineStyle(1, 0x8B6914, 0.7);
            decoG.strokeRoundedRect(signX - 40, signY, 80, 16, 2);
            this.add.text(signX, signY + 8, 'HOLLOWMARKET', {
                fontSize: '8px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5).setDepth(3);

            // Foundry sign (left)
            var foundryX = (midX - 10) * T;
            decoG.fillStyle(0x5C3C1C, 1);
            decoG.fillRect(foundryX - 2, signY + 12, 4, 20);
            decoG.fillStyle(0x4A2A10, 0.9);
            decoG.fillRoundedRect(foundryX - 36, signY, 72, 14, 2);
            decoG.lineStyle(1, 0xE07040, 0.6);
            decoG.strokeRoundedRect(foundryX - 36, signY, 72, 14, 2);
            this.add.text(foundryX, signY + 7, 'THE FOUNDRY', {
                fontSize: '7px', fontFamily: 'monospace', color: '#E07040'
            }).setOrigin(0.5).setDepth(3);

            // Fences sign (right)
            var fencesX = (midX + 10) * T;
            decoG.fillStyle(0x5C3C1C, 1);
            decoG.fillRect(fencesX - 2, signY + 12, 4, 20);
            decoG.fillStyle(0x2A3A2A, 0.9);
            decoG.fillRoundedRect(fencesX - 36, signY, 72, 14, 2);
            decoG.lineStyle(1, 0x70B070, 0.6);
            decoG.strokeRoundedRect(fencesX - 36, signY, 72, 14, 2);
            this.add.text(fencesX, signY + 7, "FINN'S FENCES", {
                fontSize: '7px', fontFamily: 'monospace', color: '#70B070'
            }).setOrigin(0.5).setDepth(3);

            // Lanterns on buildings (glow circles)
            this.surfaceLanterns = [];
            var lanternPositions = [
                { x: (midX - 4) * T, y: surfY - 4 },
                { x: (midX + 4) * T + T, y: surfY - 4 },
                { x: (midX - 8) * T, y: surfY - 4 },
                { x: (midX + 12) * T + T, y: surfY - 4 }
            ];
            for (var li = 0; li < lanternPositions.length; li++) {
                var lp = lanternPositions[li];
                // Lantern body
                decoG.fillStyle(0xFFAA44, 0.8);
                decoG.fillRect(lp.x - 2, lp.y, 4, 6);
                decoG.fillStyle(0x5C3C1C, 1);
                decoG.fillRect(lp.x - 3, lp.y - 2, 6, 2);
                // Glow
                var glow = this.add.circle(lp.x, lp.y + 3, 12, 0xFFAA44, 0.12).setDepth(2);
                this.surfaceLanterns.push(glow);
            }

            // Ambient surface dust particles
            if (this.textures.exists('particle_dirt')) {
                this.surfaceDust = this.add.particles(midX * T, surfY, 'particle_dirt', {
                    x: { min: -600, max: 600 },
                    y: { min: -40, max: 20 },
                    speed: { min: 5, max: 20 },
                    angle: { min: 250, max: 290 },
                    scale: { start: 0.5, end: 0 },
                    lifespan: { min: 2000, max: 4000 },
                    alpha: { start: 0.2, end: 0 },
                    frequency: 800,
                    tint: 0xC8A060,
                    gravityY: -5
                });
                this.surfaceDust.setDepth(2);
            }
        },

        emitDigParticles: function(wx, wy, stratum) {
            if (this.digEmitter) {
                var texKey = 'particle_dig_' + (stratum ? stratum.id : 1);
                if (this.textures.exists(texKey)) {
                    this.digEmitter.setTexture(texKey);
                }
                this.digEmitter.emitParticleAt(wx, wy, 8);
            }
        },

        emitItemParticles: function(wx, wy, rarity) {
            if (this.sparkleEmitter) {
                var texKey = 'particle_rarity_' + rarity.toLowerCase();
                if (this.textures.exists(texKey)) {
                    this.sparkleEmitter.setTexture(texKey);
                }
                this.sparkleEmitter.setParticleTint(S.RARITY[rarity].color);
                this.sparkleEmitter.emitParticleAt(wx, wy, 12);
            }
        },

        // ==================================================================
        // NOTIFICATIONS
        // ==================================================================
        showNotification: function(text, color) {
            color = color || 0xFFFFFF;

            // Limit active notifications to 5, pushing old ones up
            if (!this.activeNotifs) this.activeNotifs = [];

            // Push existing notifications up
            for (var i = 0; i < this.activeNotifs.length; i++) {
                var existing = this.activeNotifs[i];
                if (existing && existing.active) {
                    this.tweens.add({
                        targets: existing,
                        y: existing.y - 25,
                        duration: 200, ease: 'Power1'
                    });
                }
            }

            // Remove expired
            this.activeNotifs = this.activeNotifs.filter(function(n) { return n && n.active; });

            // Cap at 5 visible
            if (this.activeNotifs.length >= 5) {
                var oldest = this.activeNotifs.shift();
                if (oldest && oldest.active) oldest.destroy();
            }

            var startY = S.HEIGHT - 60;
            var notif = this.add.text(S.WIDTH / 2, startY, text, {
                fontSize: '14px', fontFamily: 'monospace', color: S.colorToHex(color),
                fontStyle: 'bold', stroke: '#000000', strokeThickness: 3
            }).setOrigin(0.5).setScrollFactor(0).setDepth(25).setAlpha(0);

            this.activeNotifs.push(notif);

            this.tweens.add({
                targets: notif,
                alpha: 1, y: startY - 20,
                duration: 300, ease: 'Power2'
            });

            this.tweens.add({
                targets: notif,
                alpha: 0, y: startY - 60,
                duration: 400, delay: 2500, ease: 'Power2',
                onComplete: function() { notif.destroy(); }
            });
        },

        // ==================================================================
        // DIGGING
        // ==================================================================
        startDig: function(tx, ty) {
            if (!this.isDiggable(tx, ty)) return;
            if (this.isDigging) return;

            var tile = this.worldData[ty][tx];
            if (tile === S.T.HARD_ROCK && !S.state.effects.canDigHardRock) {
                // Allow but slower
            }

            this.isDigging = true;
            this.digTarget = { x: tx, y: ty };
            this.digProgress = 0;

            var hardness = this.getHardness(tx, ty);
            if (tile === S.T.SURFACE_GRASS) {
                this.digTime = 0.2 / S.state.effects.digSpeed;
            } else {
                this.digTime = (0.2 + hardness * 0.15) / S.state.effects.digSpeed;
            }
        },

        updateDig: function(dt) {
            if (!this.isDigging || !this.digTarget) return;

            // Cancel dig if player moves too far from target
            var dx = Math.abs(this.playerTileX - this.digTarget.x);
            var dy = Math.abs(this.playerTileY - this.digTarget.y);
            if (dx > 1 || dy > 1) {
                this.cancelDig();
                return;
            }

            // Drain stamina while digging
            S.state.stamina -= S.PLAYER.staminaDrainDig * dt;
            if (S.state.stamina <= 0) {
                S.state.stamina = 0;
                this.cancelDig();
                this.returnToSurface();
                return;
            }

            this.digProgress += dt;

            // Draw dig progress bar
            this.digBar.clear();
            var tx = this.digTarget.x * S.TILE;
            var ty = this.digTarget.y * S.TILE;
            var pct = Math.min(1, this.digProgress / this.digTime);

            // Background
            this.digBar.fillStyle(0x000000, 0.7);
            this.digBar.fillRect(tx, ty - 6, S.TILE, 4);
            // Progress
            this.digBar.fillStyle(0x4FC3F7, 1);
            this.digBar.fillRect(tx, ty - 6, S.TILE * pct, 4);

            if (this.digProgress >= this.digTime) {
                this.completeDig();
            }
        },

        completeDig: function() {
            var tx = this.digTarget.x;
            var ty = this.digTarget.y;
            var aoe = S.state.effects.aoeRadius || 0;

            if (aoe > 0) {
                // AOE dig: clear a (2*aoe+1) x (2*aoe+1) area centered on target
                this.cameras.main.shake(150 + aoe * 50, 0.005 + aoe * 0.003);
                if (S.Audio && S.Audio.playDig) S.Audio.playDig(2);

                for (var dy = -aoe; dy <= aoe; dy++) {
                    for (var dx = -aoe; dx <= aoe; dx++) {
                        var ax = tx + dx;
                        var ay = ty + dy;
                        if (!this.isDiggable(ax, ay)) continue;

                        var tile = this.worldData[ay][ax];
                        var stratum = S.getStratumAt(ay);

                        // Particles for each cleared tile
                        this.emitDigParticles(ax * S.TILE + S.TILE / 2, ay * S.TILE + S.TILE / 2, stratum);

                        // Collect item if present (AOE may damage fragile items)
                        if (tile === S.T.ITEM && this.itemData[ay] && this.itemData[ay][ax]) {
                            var itemDef = this.itemData[ay][ax];
                            // AOE blasts have a chance to destroy fragile items (common = safe, rare+ = risky)
                            var destroyChance = (dx === 0 && dy === 0) ? 0 : 0.3;
                            if (Math.random() >= destroyChance) {
                                this.collectItem(itemDef, ax, ay);
                            } else {
                                this.showNotification('Blast destroyed: ' + itemDef.name + '!', 0xFF6644);
                            }
                        }

                        // Clear tile
                        this.worldData[ay][ax] = S.T.AIR;
                        if (this.itemData[ay]) this.itemData[ay][ax] = null;

                        // Remove sparkle
                        var key = ax + '_' + ay;
                        if (this.itemSparkles[key]) {
                            this.itemSparkles[key].destroy();
                            delete this.itemSparkles[key];
                        }

                        S.state.expedition.tilesDug++;
                    }
                }
            } else {
                // Single tile dig
                var tile = this.worldData[ty][tx];
                var stratum = S.getStratumAt(ty);

                this.cameras.main.shake(100, 0.005);
                this.emitDigParticles(tx * S.TILE + S.TILE / 2, ty * S.TILE + S.TILE / 2, stratum);

                if (S.Audio && S.Audio.playDig) {
                    S.Audio.playDig(this.getHardness(tx, ty));
                }

                // Check for item
                if (tile === S.T.ITEM && this.itemData[ty] && this.itemData[ty][tx]) {
                    var itemDef = this.itemData[ty][tx];
                    this.collectItem(itemDef, tx, ty);
                }

                // Remove tile
                this.worldData[ty][tx] = S.T.AIR;
                if (this.itemData[ty]) this.itemData[ty][tx] = null;

                // Remove sparkle
                var key = tx + '_' + ty;
                if (this.itemSparkles[key]) {
                    this.itemSparkles[key].destroy();
                    delete this.itemSparkles[key];
                }

                S.state.expedition.tilesDug++;
            }

            this.isDigging = false;
            this.digTarget = null;
            this.digProgress = 0;
            this.digBar.clear();
        },

        cancelDig: function() {
            this.isDigging = false;
            this.digTarget = null;
            this.digProgress = 0;
            this.digBar.clear();
        },

        collectItem: function(itemDef, tx, ty) {
            if (S.state.inventory.length >= S.state.effects.inventorySlots) {
                this.showNotification('Inventory full! Cannot pick up ' + itemDef.name, 0xFF4444);
                return;
            }

            // Check for item damage
            if (S.state.effects.itemDamageChance > 0 && !S.state.effects.neverDamage) {
                if (Math.random() < S.state.effects.itemDamageChance) {
                    this.showNotification('Damaged: ' + itemDef.name + '!', 0xFF8844);
                    // Damaged items sell for less - store a flag
                    S.state.inventory.push({ def: itemDef, damaged: true });
                    S.state.expedition.itemsFound++;
                    S.state.expedition.items.push(itemDef);
                    return;
                }
            }

            S.state.inventory.push({ def: itemDef, damaged: false });
            S.state.expedition.itemsFound++;
            S.state.expedition.items.push(itemDef);

            var wx = tx * S.TILE + S.TILE / 2;
            var wy = ty * S.TILE + S.TILE / 2;
            var rarityColor = S.RARITY[itemDef.rarity].color;
            var isRare = itemDef.rarity === 'RARE' || itemDef.rarity === 'LEGENDARY';

            // Particle burst - more particles for rarer items
            var particleCount = isRare ? 20 : 12;
            this.emitItemParticles(wx, wy, itemDef.rarity);
            if (isRare && this.sparkleEmitter) {
                this.sparkleEmitter.emitParticleAt(wx, wy, particleCount);
            }

            // Audio
            if (S.Audio && S.Audio.playItemFound) {
                S.Audio.playItemFound(itemDef.rarity);
            }

            // Camera effects - bigger shake for rare items
            var shakeIntensity = isRare ? 0.015 : 0.008;
            var shakeDuration = isRare ? 250 : 150;
            this.cameras.main.shake(shakeDuration, shakeIntensity);

            // Brief slow-motion effect for rare/legendary items
            if (isRare) {
                this.time.timeScale = 0.3;
                var self = this;
                this.time.delayedCall(200, function() {
                    self.time.timeScale = 1;
                });
                // Flash screen with rarity color
                var r = (rarityColor >> 16) & 0xFF;
                var g = (rarityColor >> 8) & 0xFF;
                var b = rarityColor & 0xFF;
                this.cameras.main.flash(300, r, g, b);
            }

            // Floating item name rises from dig spot
            var floatName = this.add.text(wx, wy, itemDef.name, {
                fontSize: isRare ? '14px' : '11px', fontFamily: 'monospace',
                color: S.RARITY[itemDef.rarity].hex, fontStyle: 'bold',
                stroke: '#000000', strokeThickness: 3
            }).setOrigin(0.5).setDepth(25).setAlpha(0);

            this.tweens.add({
                targets: floatName,
                alpha: 1, y: wy - 20,
                duration: 300, ease: 'Power2'
            });
            this.tweens.add({
                targets: floatName,
                alpha: 0, y: wy - 50,
                duration: 500, delay: 1200, ease: 'Power2',
                onComplete: function() { floatName.destroy(); }
            });

            // Rarity label beneath
            var floatRarity = this.add.text(wx, wy + 8, S.RARITY[itemDef.rarity].name, {
                fontSize: '9px', fontFamily: 'monospace',
                color: S.RARITY[itemDef.rarity].hex,
                stroke: '#000000', strokeThickness: 2
            }).setOrigin(0.5).setDepth(25).setAlpha(0);
            this.tweens.add({
                targets: floatRarity,
                alpha: 0.8, y: wy - 8,
                duration: 300, delay: 150, ease: 'Power2'
            });
            this.tweens.add({
                targets: floatRarity,
                alpha: 0, y: wy - 38,
                duration: 400, delay: 1400, ease: 'Power2',
                onComplete: function() { floatRarity.destroy(); }
            });

            this.showNotification('Found: ' + itemDef.name + ' (' + S.RARITY[itemDef.rarity].name + ')', rarityColor);
        },

        // ==================================================================
        // PLAYER PHYSICS & MOVEMENT
        // ==================================================================
        updatePlayer: function(dt) {
            var speed = S.state.effects.moveSpeed;
            var onGround = this.playerOnGround;

            // Horizontal movement (disabled when shift-digging or actively digging)
            this.playerVX = 0;
            if (!this.isDigging && !this.shiftDigging) {
                if (this.cursors.left.isDown || this.keys.A.isDown) {
                    this.playerVX = -speed;
                    this.facingRight = false;
                }
                if (this.cursors.right.isDown || this.keys.D.isDown) {
                    this.playerVX = speed;
                    this.facingRight = true;
                }
            }

            // Jump (not while digging)
            if (!this.isDigging && onGround && (this.cursors.up.isDown || this.keys.W.isDown || this.keys.SPACE.isDown)) {
                this.playerVY = S.PLAYER.jumpVelocity;
                this.playerOnGround = false;
            }

            // Gravity
            this.playerVY += S.PLAYER.gravity * dt;
            if (this.playerVY > 600) this.playerVY = 600; // terminal velocity

            // Apply movement with collision
            var newX = this.player.x + this.playerVX * dt;
            var newY = this.player.y + this.playerVY * dt;

            // Player dimensions (hitbox)
            var pw = 16; // half width
            var ph = 26; // height from origin (bottom)

            // Horizontal collision
            var testTileX, testTileY;
            if (this.playerVX !== 0) {
                var edgeX = newX + (this.playerVX > 0 ? pw / 2 : -pw / 2);
                testTileX = Math.floor(edgeX / S.TILE);
                var topTile = Math.floor((this.player.y - ph) / S.TILE);
                var botTile = Math.floor((this.player.y - 2) / S.TILE);

                var blocked = false;
                for (var checkY = topTile; checkY <= botTile; checkY++) {
                    if (this.isSolid(testTileX, checkY)) {
                        blocked = true;
                        break;
                    }
                }
                if (blocked) {
                    newX = this.player.x;
                    this.playerVX = 0;
                }
            }

            // Vertical collision
            if (this.playerVY > 0) {
                // Falling - check below
                testTileY = Math.floor(newY / S.TILE);
                var leftTile = Math.floor((newX - pw / 2) / S.TILE);
                var rightTile = Math.floor((newX + pw / 2 - 1) / S.TILE);

                var landed = false;
                for (var checkX = leftTile; checkX <= rightTile; checkX++) {
                    if (this.isSolid(checkX, testTileY)) {
                        landed = true;
                        break;
                    }
                }
                if (landed) {
                    // Landing dust puff when falling fast enough
                    if (!this.wasOnGround && this.playerVY > 150) {
                        this.emitLandingPuff(newX, testTileY * S.TILE);
                    }
                    newY = testTileY * S.TILE;
                    this.playerVY = 0;
                    this.playerOnGround = true;
                } else {
                    this.playerOnGround = false;
                }
            } else if (this.playerVY < 0) {
                // Rising - check above
                testTileY = Math.floor((newY - ph) / S.TILE);
                leftTile = Math.floor((newX - pw / 2) / S.TILE);
                rightTile = Math.floor((newX + pw / 2 - 1) / S.TILE);

                var hitCeiling = false;
                for (checkX = leftTile; checkX <= rightTile; checkX++) {
                    if (this.isSolid(checkX, testTileY)) {
                        hitCeiling = true;
                        break;
                    }
                }
                if (hitCeiling) {
                    newY = (testTileY + 1) * S.TILE + ph;
                    this.playerVY = 0;
                }
            }

            // Clamp to world bounds
            newX = Phaser.Math.Clamp(newX, pw, S.WORLD_W * S.TILE - pw);
            newY = Phaser.Math.Clamp(newY, ph, S.WORLD_H * S.TILE);

            this.player.setPosition(newX, newY);
            this.playerTileX = Math.floor(newX / S.TILE);
            this.playerTileY = Math.floor(newY / S.TILE);

            // Update underground state
            this.isUnderground = this.playerTileY >= S.SURFACE_Y;

            // Stamina drain
            if (this.isUnderground && !this.isDigging) {
                S.state.stamina -= S.PLAYER.staminaDrainMove * dt;
                if (S.state.stamina <= 0) {
                    S.state.stamina = 0;
                    this.returnToSurface();
                }
                // Low stamina warning
                if (S.state.stamina < 15 && S.state.stamina > 14.5) {
                    this.showNotification('LOW STAMINA! Return to surface!', 0xFF4444);
                    if (S.Audio && S.Audio.playHazard) S.Audio.playHazard();
                }
            }

            // Hazard check
            this.checkHazards(dt);

            // Track ground state for next frame
            this.wasOnGround = this.playerOnGround;

            this.drawPlayer();
        },

        emitLandingPuff: function(wx, wy) {
            if (this.digEmitter) {
                var stratum = S.getStratumAt(Math.floor(wy / S.TILE));
                var texKey = 'particle_dig_' + (stratum ? stratum.id : 1);
                if (this.textures.exists(texKey)) {
                    this.digEmitter.setTexture(texKey);
                }
                this.digEmitter.emitParticleAt(wx - 6, wy, 4);
                this.digEmitter.emitParticleAt(wx + 6, wy, 4);
            }
        },

        checkHazards: function(dt) {
            var ptx = this.playerTileX;
            var pty = this.playerTileY;

            // Check 3x3 around player
            for (var dy = -1; dy <= 1; dy++) {
                for (var dx = -1; dx <= 1; dx++) {
                    var tx = ptx + dx;
                    var ty = pty + dy;
                    if (tx < 0 || tx >= S.WORLD_W || ty < 0 || ty >= S.WORLD_H) continue;
                    var tile = this.worldData[ty][tx];

                    if (tile === S.T.HAZARD_GAS) {
                        S.state.stamina -= 0.5 * dt;
                        if (Math.random() < 0.002) {
                            this.showNotification('Toxic gas!', 0x88CC44);
                            if (S.Audio && S.Audio.playHazard) S.Audio.playHazard();
                        }
                    }
                    if (tile === S.T.HAZARD_WATER) {
                        S.state.stamina -= 0.3 * dt;
                        if (Math.random() < 0.002) {
                            this.showNotification('Flooded area!', 0x4488CC);
                        }
                    }
                }
            }
        },

        // ==================================================================
        // INPUT HANDLING
        // ==================================================================
        handleInput: function() {
            // Digging
            this.shiftDigging = false;
            if (!this.isDigging) {
                var ptx = this.playerTileX;
                var pty = this.playerTileY; // tile the player's feet are on

                // Dig sideways (shift + direction) - check first so shift blocks movement
                if (this.keys.SHIFT.isDown) {
                    this.shiftDigging = true;
                    if (this.cursors.left.isDown || this.keys.A.isDown) {
                        this.startDig(ptx - 1, pty - 1); // dig left at body level
                    } else if (this.cursors.right.isDown || this.keys.D.isDown) {
                        this.startDig(ptx + 1, pty - 1); // dig right at body level
                    } else if (this.cursors.down.isDown || this.keys.S.isDown) {
                        this.startDig(ptx, pty); // dig down
                    }
                }
                // Dig down (no shift)
                else if (this.cursors.down.isDown || this.keys.S.isDown) {
                    this.startDig(ptx, pty);
                }
            } else {
                this.shiftDigging = this.keys.SHIFT.isDown;
            }

            // Toggle auto-dig with Q (only if purchased)
            if (Phaser.Input.Keyboard.JustDown(this.keys.Q) && S.state.autoDigEnabled) {
                this.playerAutoDig = !this.playerAutoDig;
                this.showNotification('Auto-Dig: ' + (this.playerAutoDig ? 'ON' : 'OFF'), this.playerAutoDig ? 0x44FF44 : 0xFF4444);
            }

            // Player auto-dig: when enabled and not manually digging, auto-dig adjacent tiles
            if (this.playerAutoDig && !this.isDigging && this.isUnderground) {
                var ptx2 = this.playerTileX;
                var pty2 = this.playerTileY;
                // Same priority as NPC: below > ahead > behind > above
                var autoDirs = [
                    { dx: 0, dy: 1 },
                    { dx: this.facingRight ? 1 : -1, dy: 0 },
                    { dx: this.facingRight ? -1 : 1, dy: 0 },
                    { dx: 0, dy: -1 }
                ];
                for (var ad = 0; ad < autoDirs.length; ad++) {
                    var adx = ptx2 + autoDirs[ad].dx;
                    var ady = pty2 + autoDirs[ad].dy;
                    // Adjust Y for player body offset (feet vs body)
                    if (autoDirs[ad].dy === 0) ady = pty2 - 1;
                    if (autoDirs[ad].dy === 1) ady = pty2;
                    if (autoDirs[ad].dy === -1) ady = pty2 - 2;
                    if (this.isDiggable(adx, ady)) {
                        this.startDig(adx, ady);
                        break;
                    }
                }
            }

            // Return to surface
            if (Phaser.Input.Keyboard.JustDown(this.keys.E) && this.isUnderground) {
                this.returnToSurface();
            }

            // Toggle inventory
            if (Phaser.Input.Keyboard.JustDown(this.keys.I)) {
                if (this.scene.isActive('InventoryScene')) {
                    this.scene.stop('InventoryScene');
                } else if (this.scene.get('InventoryScene')) {
                    this.scene.launch('InventoryScene');
                }
            }

            // Surface-only actions
            if (!this.isUnderground) {
                if (Phaser.Input.Keyboard.JustDown(this.keys.M) && this.scene.get('MarketScene')) {
                    this.scene.launch('MarketScene');
                    this.scene.pause();
                }
                if (Phaser.Input.Keyboard.JustDown(this.keys.U) && this.scene.get('UpgradeScene')) {
                    this.scene.launch('UpgradeScene');
                    this.scene.pause();
                }
                if (Phaser.Input.Keyboard.JustDown(this.keys.C) && this.scene.get('ContractScene')) {
                    this.scene.launch('ContractScene');
                    this.scene.pause();
                }
            }
        },

        // ==================================================================
        // RETURN TO SURFACE
        // ==================================================================
        returnToSurface: function() {
            if (S.Audio && S.Audio.playReturn) S.Audio.playReturn();

            var self = this;

            // Rising animation: camera zooms up, screen brightens
            var ascendText = this.add.text(S.WIDTH / 2, S.HEIGHT / 2, 'ASCENDING...', {
                fontSize: '22px', fontFamily: 'monospace', color: '#FFFFFF',
                fontStyle: 'bold', stroke: '#000000', strokeThickness: 4
            }).setOrigin(0.5).setScrollFactor(0).setDepth(30).setAlpha(0);

            this.tweens.add({
                targets: ascendText,
                alpha: 1, y: S.HEIGHT / 2 - 30,
                duration: 300, ease: 'Power2'
            });

            this.cameras.main.flash(400, 255, 255, 255);
            this.cameras.main.zoomTo(1.5, 500, 'Power2');

            this.time.delayedCall(500, function() {
                // Merge NPC digger inventories into player inventory
                self.mergeNPCInventories();

                // Recover stamina
                S.state.stamina = Math.min(S.state.maxStamina, S.state.stamina + S.state.maxStamina * 0.5);

                // Advance market
                if (S.Systems && S.Systems.Economy && S.Systems.Economy.advanceMarket) {
                    S.Systems.Economy.advanceMarket();
                }
                if (S.Systems && S.Systems.Contracts && S.Systems.Contracts.advance) {
                    S.Systems.Contracts.advance();
                }

                // Auto-save on expedition return
                if (S.Save) S.Save.save();

                // Stop ambient music
                if (S.Audio && S.Audio.stopAmbientMusic) S.Audio.stopAmbientMusic();

                // Go to summary
                if (self.scene.get('SummaryScene')) {
                    self.scene.start('SummaryScene');
                } else {
                    // Teleport player to surface
                    self.player.setPosition(
                        Math.floor(S.WORLD_W / 2) * S.TILE + S.TILE / 2,
                        (S.SURFACE_Y - 2) * S.TILE
                    );
                    self.playerVY = 0;
                    self.showNotification('Returned to surface!', 0x44FF44);
                }
            });
        },

        // ==================================================================
        // ITEM SPARKLE ANIMATION
        // ==================================================================
        updateItemSparkles: function(time) {
            var keys = Object.keys(this.itemSparkles);
            for (var i = 0; i < keys.length; i++) {
                var sparkle = this.itemSparkles[keys[i]];
                if (sparkle && sparkle.visible) {
                    sparkle.setAlpha(0.2 + 0.3 * Math.sin(time * 0.003 + i));
                    sparkle.setScale(0.8 + 0.3 * Math.sin(time * 0.004 + i * 0.5));
                }
            }
        },

        // ==================================================================
        // MAIN UPDATE LOOP
        // ==================================================================
        update: function(time, delta) {
            var dt = Math.min(delta / 1000, 0.05); // cap dt

            // Handle input
            this.handleInput();

            // Update player physics
            this.updatePlayer(dt);

            // Update digging
            this.updateDig(dt);

            // Update visible tiles (only when camera moves significantly)
            var cam = this.cameras.main;
            if (Math.abs(cam.scrollX - this.lastCamX) > S.TILE / 2 ||
                Math.abs(cam.scrollY - this.lastCamY) > S.TILE / 2) {
                this.updateVisibleTiles();
                this.lastCamX = cam.scrollX;
                this.lastCamY = cam.scrollY;
            }

            // Update item sparkles
            this.updateItemSparkles(time);

            // Animate parallax backgrounds
            if (this.bgTiles) {
                for (var bi = 0; bi < this.bgTiles.length; bi++) {
                    this.bgTiles[bi].tilePositionX = cam.scrollX * 0.05 * (bi + 1);
                    this.bgTiles[bi].tilePositionY = cam.scrollY * 0.02 * (bi + 1);
                }
            }

            // Animate surface lanterns (gentle flicker)
            if (this.surfaceLanterns) {
                for (var li = 0; li < this.surfaceLanterns.length; li++) {
                    var lantern = this.surfaceLanterns[li];
                    lantern.setAlpha(0.08 + 0.06 * Math.sin(time * 0.003 + li * 1.7));
                    lantern.setScale(0.9 + 0.15 * Math.sin(time * 0.002 + li * 2.3));
                }
            }

            // Update lighting
            this.updateLighting();

            // Update HUD
            this.updateHUD();

            // Ambient music - changes with stratum
            var currentStratum = S.getStratumAt(this.playerTileY);
            if (S.Audio && S.Audio.startAmbientMusic) {
                S.Audio.startAmbientMusic(currentStratum.id);
            }

            // Ambient sound effects (occasional drips, creaks)
            if (this.isUnderground) {
                this.ambientTimer = (this.ambientTimer || 0) + dt;
                if (this.ambientTimer > 5 + Math.random() * 5) {
                    this.ambientTimer = 0;
                    if (S.Audio && S.Audio.playAmbient) S.Audio.playAmbient(currentStratum.id);
                }
            }

            // NPC diggers
            this.updateNPCDiggers(dt);
        },

        // ==================================================================
        // NPC DIGGER SYSTEM — visible characters that dig like the player
        // ==================================================================
        createNPCDiggers: function() {
            this.npcDiggers = [];
            var total = S.state.hiredDiggers;
            if (total <= 0) return;

            var startX = Math.floor(S.WORLD_W / 2);
            var startY = S.SURFACE_Y - 2;

            // Assign each digger a column offset from the player (1-5 tiles away, alternating sides)
            var offsets = [-2, 3, -4, 5, -1];

            for (var i = 0; i < total; i++) {
                var colOffset = offsets[i % offsets.length];
                var npc = {
                    x: (startX + colOffset) * S.TILE + S.TILE / 2,
                    y: startY * S.TILE,
                    vx: 0, vy: 0,
                    tileX: startX + colOffset,
                    tileY: startY,
                    onGround: false,
                    facingRight: colOffset > 0,
                    isDigging: false,
                    digTarget: null,
                    digProgress: 0,
                    digTime: 0,
                    aiTimer: Math.random() * 0.5,
                    body: this.add.graphics().setDepth(5),
                    digBar: this.add.graphics().setDepth(10),
                    hatColor: [0xE84040, 0x40A0E8, 0x40E870, 0xE8A040, 0xA040E8][i % 5],
                    id: i,
                    columnOffset: colOffset,  // their preferred column relative to player
                    inventory: [],             // their own inventory
                    maxInventory: 8            // smaller pack than player
                };
                this.npcDiggers.push(npc);
            }
        },

        updateNPCDiggers: function(dt) {
            if (!this.npcDiggers) return;
            for (var i = 0; i < this.npcDiggers.length; i++) {
                var npc = this.npcDiggers[i];
                this.updateNPCPhysics(npc, dt);
                this.updateNPCAI(npc, dt);
                this.updateNPCDig(npc, dt);
                this.drawNPCDigger(npc);
            }
        },

        updateNPCPhysics: function(npc, dt) {
            npc.vy += 600 * dt;
            if (npc.vy > 400) npc.vy = 400;

            var newX = npc.x + npc.vx * dt;
            var ntx = Math.floor(newX / S.TILE);
            var nty = Math.floor(npc.y / S.TILE);
            if (!this.isSolid(ntx, nty) && newX > S.TILE && newX < (S.WORLD_W - 1) * S.TILE) {
                npc.x = newX;
            } else {
                npc.vx = 0;
            }

            var newY = npc.y + npc.vy * dt;
            var cyBelow = Math.floor((newY + 1) / S.TILE);
            var cxAt = Math.floor(npc.x / S.TILE);

            if (npc.vy >= 0 && this.isSolid(cxAt, cyBelow)) {
                npc.y = cyBelow * S.TILE - 1;
                npc.vy = 0;
                npc.onGround = true;
            } else {
                npc.y = newY;
                if (npc.vy > 0) npc.onGround = false;
            }

            npc.tileX = Math.floor(npc.x / S.TILE);
            npc.tileY = Math.floor(npc.y / S.TILE);
        },

        // Immediately find and start the next dig after completing one
        queueNextDig: function(npc) {
            npc.aiTimer = 999; // force immediate AI tick
        },

        updateNPCAI: function(npc, dt) {
            if (npc.isDigging) return;

            npc.aiTimer += dt;
            if (npc.aiTimer < 0.08) return; // fast tick — 80ms
            npc.aiTimer = 0;

            var px = this.playerTileX;
            var py = this.playerTileY;

            // Their target column is player X + their offset
            var targetCol = px + npc.columnOffset;
            targetCol = Math.max(2, Math.min(S.WORLD_W - 3, targetCol));

            // If too far from player vertically (>12 tiles), teleport near them
            var vertDist = Math.abs(npc.tileY - py);
            var horizDist = Math.abs(npc.tileX - targetCol);
            if (vertDist > 12 || (vertDist + horizDist) > 18) {
                // Find air tile near their target column at player's depth
                for (var r = 0; r <= 4; r++) {
                    for (var attempt = 0; attempt < 12; attempt++) {
                        var ox = targetCol + Math.floor(Math.random() * (r * 2 + 1)) - r;
                        var oy = py + Math.floor(Math.random() * 5) - 2;
                        if (ox > 0 && ox < S.WORLD_W && oy > 0 && oy < S.WORLD_H) {
                            if (this.worldData[oy] && this.worldData[oy][ox] === S.T.AIR) {
                                npc.x = ox * S.TILE + S.TILE / 2;
                                npc.y = oy * S.TILE;
                                npc.tileX = ox;
                                npc.tileY = oy;
                                npc.vx = 0; npc.vy = 0;
                                return;
                            }
                        }
                    }
                }
            }

            // PRIORITY 1: If not in their column, move toward it (and dig through obstacles)
            if (Math.abs(npc.tileX - targetCol) > 1) {
                var colDir = targetCol > npc.tileX ? 1 : -1;
                npc.vx = colDir * S.state.effects.moveSpeed * 0.8;
                npc.facingRight = npc.vx > 0;

                if (npc.onGround) {
                    var ahead = npc.tileX + colDir;
                    if (this.isSolid(ahead, npc.tileY)) {
                        if (this.isDiggable(ahead, npc.tileY)) {
                            npc.vx = 0;
                            this.startNPCDig(npc, ahead, npc.tileY);
                        } else {
                            npc.vy = -280; npc.onGround = false;
                        }
                    }
                }
                return;
            }

            // PRIORITY 2: Dig continuously
            // Step A: Check all 4 directly adjacent tiles — if any is diggable, dig it immediately
            // Prefer: below > ahead > behind > above
            var immediateDirs = [
                { dx: 0, dy: 1 },  // below (always preferred)
                { dx: npc.facingRight ? 1 : -1, dy: 0 }, // ahead
                { dx: npc.facingRight ? -1 : 1, dy: 0 }, // behind
                { dx: 0, dy: -1 }  // above (last resort)
            ];

            for (var d = 0; d < immediateDirs.length; d++) {
                var ix = npc.tileX + immediateDirs[d].dx;
                var iy = npc.tileY + immediateDirs[d].dy;
                if (this.isDiggable(ix, iy)) {
                    npc.vx = 0;
                    npc.facingRight = immediateDirs[d].dx > 0 ? true : (immediateDirs[d].dx < 0 ? false : npc.facingRight);
                    this.startNPCDig(npc, ix, iy);
                    return;
                }
            }

            // Step B: Nothing adjacent — scan wider for nearest diggable tile and walk to it
            var best = null;
            var bestDist = 999;

            for (var dy = -2; dy <= 8; dy++) {
                for (var dx = -8; dx <= 8; dx++) {
                    var tx = npc.tileX + dx;
                    var ty = npc.tileY + dy;
                    if (dx === 0 && dy === 0) continue;
                    if (!this.isDiggable(tx, ty)) continue;

                    // Prefer downward, closer, and near their column
                    var dist = Math.abs(dx) + Math.abs(dy) - (dy > 0 ? dy * 2 : 0);
                    if (dist < bestDist) { bestDist = dist; best = { x: tx, y: ty }; }
                }
            }

            if (!best) {
                // Nothing at all — follow player
                if (Math.abs(npc.tileX - px) > 2) {
                    npc.vx = (px > npc.tileX ? 1 : -1) * S.state.effects.moveSpeed * 0.8;
                    npc.facingRight = npc.vx > 0;
                } else { npc.vx = 0; }
                return;
            }

            // Walk toward the target — dig through anything in the way
            var moveDir = 0;
            if (best.x > npc.tileX) moveDir = 1;
            else if (best.x < npc.tileX) moveDir = -1;

            if (moveDir !== 0) {
                // Check if the tile ahead is diggable — if so, just dig it (path-making)
                var aheadX = npc.tileX + moveDir;
                if (this.isDiggable(aheadX, npc.tileY)) {
                    npc.vx = 0;
                    npc.facingRight = moveDir > 0;
                    this.startNPCDig(npc, aheadX, npc.tileY);
                    return;
                }

                npc.vx = moveDir * S.state.effects.moveSpeed * 0.8;
                npc.facingRight = moveDir > 0;

                // Jump if blocked by non-diggable
                if (npc.onGround && this.isSolid(aheadX, npc.tileY)) {
                    npc.vy = -280; npc.onGround = false;
                }
            } else {
                // Same column — need to go down. Dig below if possible.
                if (this.isDiggable(npc.tileX, npc.tileY + 1)) {
                    npc.vx = 0;
                    this.startNPCDig(npc, npc.tileX, npc.tileY + 1);
                    return;
                }
                npc.vx = 0;
            }

            if (npc.onGround && best.y < npc.tileY) {
                npc.vy = -280; npc.onGround = false;
            }
        },

        startNPCDig: function(npc, tx, ty) {
            if (!this.isDiggable(tx, ty)) return;
            npc.isDigging = true;
            npc.digTarget = { x: tx, y: ty };
            npc.digProgress = 0;
            var hardness = this.getHardness(tx, ty);
            // NPCs dig at 50% of player speed
            npc.digTime = (0.2 + hardness * 0.15) / (S.state.effects.digSpeed * 0.5);
        },

        updateNPCDig: function(npc, dt) {
            if (!npc.isDigging || !npc.digTarget) return;
            npc.digProgress += dt;

            npc.digBar.clear();
            var bx = npc.digTarget.x * S.TILE;
            var by = npc.digTarget.y * S.TILE;
            var pct = Math.min(1, npc.digProgress / npc.digTime);
            npc.digBar.fillStyle(0x000000, 0.7);
            npc.digBar.fillRect(bx, by - 6, S.TILE, 4);
            npc.digBar.fillStyle(npc.hatColor, 1);
            npc.digBar.fillRect(bx, by - 6, S.TILE * pct, 4);

            if (npc.digProgress >= npc.digTime) this.completeNPCDig(npc);
        },

        completeNPCDig: function(npc) {
            var tx = npc.digTarget.x, ty = npc.digTarget.y;
            var tile = this.worldData[ty][tx];
            var aoe = S.state.effects.aoeRadius || 0;

            this.cameras.main.shake(60, 0.002);

            if (aoe > 0) {
                for (var dy2 = -aoe; dy2 <= aoe; dy2++) {
                    for (var dx2 = -aoe; dx2 <= aoe; dx2++) {
                        var ax = tx + dx2, ay = ty + dy2;
                        if (!this.isDiggable(ax, ay)) continue;
                        this.emitDigParticles(ax * S.TILE + S.TILE / 2, ay * S.TILE + S.TILE / 2, S.getStratumAt(ay));
                        var at = this.worldData[ay][ax];
                        if (at === S.T.ITEM && this.itemData[ay] && this.itemData[ay][ax]) {
                            if (dx2 === 0 && dy2 === 0 || Math.random() >= 0.3)
                                this.npcCollectItem(npc, this.itemData[ay][ax], ax, ay);
                        }
                        this.worldData[ay][ax] = S.T.AIR;
                        if (this.itemData[ay]) this.itemData[ay][ax] = null;
                        var k = ax + '_' + ay;
                        if (this.itemSparkles[k]) { this.itemSparkles[k].destroy(); delete this.itemSparkles[k]; }
                        S.state.expedition.tilesDug++;
                    }
                }
            } else {
                this.emitDigParticles(tx * S.TILE + S.TILE / 2, ty * S.TILE + S.TILE / 2, S.getStratumAt(ty));
                if (tile === S.T.ITEM && this.itemData[ty] && this.itemData[ty][tx])
                    this.npcCollectItem(npc, this.itemData[ty][tx], tx, ty);
                this.worldData[ty][tx] = S.T.AIR;
                if (this.itemData[ty]) this.itemData[ty][tx] = null;
                var key = tx + '_' + ty;
                if (this.itemSparkles[key]) { this.itemSparkles[key].destroy(); delete this.itemSparkles[key]; }
                S.state.expedition.tilesDug++;
            }

            if (S.Audio && S.Audio.playDig) S.Audio.playDig(this.getHardness(tx, ty));
            var depth = S.depthToMeters(ty);
            if (depth > S.state.expedition.maxDepth) S.state.expedition.maxDepth = depth;

            npc.isDigging = false;
            npc.digTarget = null;
            npc.digProgress = 0;
            npc.digBar.clear();

            // Immediately look for next tile to dig — no idle gap
            this.queueNextDig(npc);
        },

        npcCollectItem: function(npc, itemDef, tx, ty) {
            // NPCs have their own inventory with limited capacity
            if (npc.inventory.length >= npc.maxInventory) return;

            if (S.state.effects.itemDamageChance > 0 && !S.state.effects.neverDamage) {
                if (Math.random() < S.state.effects.itemDamageChance) {
                    npc.inventory.push({ def: itemDef, damaged: true });
                    S.state.expedition.itemsFound++;
                    S.state.expedition.items.push(itemDef);
                    this.showNotification('Digger damaged: ' + itemDef.name + '!', 0xFF8844);
                    return;
                }
            }
            npc.inventory.push({ def: itemDef, damaged: false });
            S.state.expedition.itemsFound++;
            S.state.expedition.items.push(itemDef);
            this.emitItemParticles(tx * S.TILE + S.TILE / 2, ty * S.TILE + S.TILE / 2, itemDef.rarity);
            if (S.Audio && S.Audio.playItemFound) S.Audio.playItemFound(itemDef.rarity);
            var rd = S.RARITY[itemDef.rarity];
            this.showNotification('Digger found: ' + itemDef.name, rd ? rd.color : 0xFFFFFF);
        },

        // Merge all NPC inventories into the player's inventory on return
        mergeNPCInventories: function() {
            if (!this.npcDiggers) return;
            var merged = 0;
            for (var i = 0; i < this.npcDiggers.length; i++) {
                var npc = this.npcDiggers[i];
                for (var j = 0; j < npc.inventory.length; j++) {
                    S.state.inventory.push(npc.inventory[j]);
                    merged++;
                }
                npc.inventory = [];
            }
            if (merged > 0) {
                this.showNotification('Diggers brought back ' + merged + ' items!', 0x40A0E8);
            }
        },

        drawNPCDigger: function(npc) {
            var g = npc.body;
            g.clear();
            var px = npc.x, py = npc.y;
            var flip = npc.facingRight ? 1 : -1;
            var t = this.time.now + npc.id * 1000;

            var breathOffset = 0, armSwing = 0;
            if (npc.vx === 0 && !npc.isDigging && npc.onGround) {
                breathOffset = Math.sin(t * 0.003) * 0.8;
                armSwing = Math.sin(t * 0.003) * 0.5;
            }
            var legL = 0, legR = 0;
            if (npc.vx !== 0) {
                var wp = t * 0.012;
                legL = Math.sin(wp) * 3; legR = Math.sin(wp + Math.PI) * 3;
                armSwing = Math.sin(wp) * 2;
            }
            var digSwing = 0;
            if (npc.isDigging && npc.digTime > 0) digSwing = Math.sin(t * 0.015) * 8;

            var by = py + breathOffset;

            // Shadow
            g.fillStyle(0x000000, 0.15); g.fillEllipse(px, py + 1, 16, 4);
            // Boots
            g.fillStyle(0x4C2C0C, 1);
            g.fillRect(px - 5, by - 1, 4, 2); g.fillRect(px + 1 + legR, by - 1, 4, 2);
            g.fillStyle(0x3C1C00, 1);
            g.fillRect(px - 6, by, 5, 1); g.fillRect(px + legR, by, 5, 1);
            // Legs
            g.fillStyle(0x6B4C2C, 1);
            g.fillRect(px - 4, by - 4 + legL * 0.3, 3, 4);
            g.fillRect(px + 1 + legR * 0.3, by - 4 + legR * 0.3, 3, 4);
            // Body
            g.fillStyle(0xC8A060, 1); g.fillRect(px - 6, by - 14, 12, 11);
            g.fillStyle(0xB89050, 0.5); g.fillRect(px - 1, by - 14, 1, 10);
            // Belt
            g.fillStyle(0x8B6914, 1); g.fillRect(px - 6, by - 4, 12, 2);
            // Arms + pickaxe
            g.fillStyle(0xC8A060, 1);
            if (npc.isDigging) {
                var armX = px + flip * 6;
                var armEndY = by - 14 + Math.abs(digSwing) * 0.3;
                g.fillRect(armX, armEndY, 3, 9);
                g.fillRect(px - flip * 7, by - 12, 3, 7);
                g.fillStyle(0x6B4912, 1);
                var pbx = armX + flip * 2;
                g.fillRect(pbx, armEndY - 6 + digSwing * 0.3, 2, 12);
                g.fillStyle(0x999999, 1);
                g.fillRect(pbx - 2, armEndY - 8 + digSwing * 0.3, 6, 3);
            } else {
                g.fillRect(px - 8, by - 13 + armSwing, 3, 8);
                g.fillRect(px + 5, by - 13 - armSwing, 3, 8);
            }
            // Face
            g.fillStyle(0xE8B88C, 1); g.fillRect(px - 5, by - 22, 10, 8);
            g.fillStyle(0x000000, 1); g.fillRect(px + flip * 2, by - 19, 2, 2);
            // Hard hat — unique color per digger
            g.fillStyle(npc.hatColor, 1);
            g.fillRect(px - 6, by - 28, 12, 5); g.fillRect(px - 7, by - 24, 14, 3);
            g.fillStyle(0xFFFFFF, 0.2); g.fillRect(px - 4, by - 28, 6, 1);
            // Headlamp
            g.fillStyle(0xFFFF88, 1); g.fillRect(px + flip * 2 - 1, by - 27, 3, 2);
        }
    });

})(window.STRATA);
