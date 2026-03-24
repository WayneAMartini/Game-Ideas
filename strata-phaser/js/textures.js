// STRATA - Procedural Texture Generation
// ========================================
// Generates all tile and sprite textures using Phaser Graphics + generateTexture
(function(S) {
    'use strict';

    S.Textures = {
        generateAll: function(scene) {
            this.generateTileTextures(scene);
            this.generatePlayerTextures(scene);
            this.generateUITextures(scene);
            this.generateParticleTextures(scene);
        },

        // ---- Tile Textures ----
        generateTileTextures: function(scene) {
            var T = S.TILE;
            var g = scene.make.graphics({ x: 0, y: 0, add: false });

            // --- Generate tileset as a texture atlas (8 columns x N rows) ---
            // Layout: each tile type gets a unique index
            // Row 0: dirt per stratum (0-4)
            // Row 1: rock per stratum (0-4)
            // Row 2: hard rock per stratum (0-4)
            // Row 3: special tiles (grass, bedrock, hazard_gas, hazard_water, building, item_sparkle)
            // Row 4: additional dirt variants per stratum (for visual variety)
            // Row 5: additional rock variants

            var cols = 8;
            var rows = 8;
            var atlasW = cols * T;
            var atlasH = rows * T;

            g.clear();

            // --- Row 0: Dirt tiles per stratum ---
            for (var s = 1; s <= 4; s++) {
                var sc = S.STRATA[s].colors;
                var x = (s - 1) * T;
                this._drawDirtTile(g, x, 0, T, sc.dirt || sc.base, sc.base, s);
            }

            // --- Row 1: Rock tiles per stratum ---
            for (s = 1; s <= 4; s++) {
                sc = S.STRATA[s].colors;
                x = (s - 1) * T;
                this._drawRockTile(g, x, T, T, sc.base, sc.accent1, s);
            }

            // --- Row 2: Hard rock tiles per stratum ---
            for (s = 1; s <= 4; s++) {
                sc = S.STRATA[s].colors;
                x = (s - 1) * T;
                this._drawHardRockTile(g, x, T * 2, T, sc.accent2 || sc.base, sc.accent1, s);
            }

            // --- Row 3: Special tiles ---
            // Grass tile (col 0)
            this._drawGrassTile(g, 0, T * 3, T);
            // Bedrock tile (col 1)
            this._drawBedrockTile(g, T, T * 3, T);
            // Gas hazard tile (col 2)
            this._drawHazardGasTile(g, T * 2, T * 3, T);
            // Water hazard tile (col 3)
            this._drawHazardWaterTile(g, T * 3, T * 3, T);
            // Building tile (col 4)
            this._drawBuildingTile(g, T * 4, T * 3, T);
            // Item sparkle overlay (col 5)
            this._drawItemSparkle(g, T * 5, T * 3, T);
            // Air/sky (col 6) - just transparent
            // Empty slot (col 7)

            // --- Row 4-5: Dirt and rock variants ---
            for (s = 1; s <= 4; s++) {
                sc = S.STRATA[s].colors;
                x = (s - 1) * T;
                this._drawDirtTile(g, x, T * 4, T, sc.dirt || sc.base, sc.accent2 || sc.base, s + 10);
                this._drawRockTile(g, x, T * 5, T, sc.accent1 || sc.base, sc.accent2 || sc.base, s + 10);
            }

            // --- Row 6: Stratum transition tiles ---
            for (s = 1; s <= 4; s++) {
                sc = S.STRATA[s].colors;
                x = (s - 1) * T;
                this._drawTransitionTile(g, x, T * 6, T, sc);
            }

            g.generateTexture('tileset', atlasW, atlasH);
            g.destroy();

            // Create individual tile textures for the tilemap
            this._createIndividualTiles(scene);
        },

        _drawDirtTile: function(g, x, y, T, baseColor, accentColor, seed) {
            // Base fill
            g.fillStyle(baseColor, 1);
            g.fillRect(x, y, T, T);

            // Layered noise texture - varying sizes for organic feel
            for (var i = 0; i < 25; i++) {
                var nx = x + ((seed * 73 + i * 37) % T);
                var ny = y + ((seed * 51 + i * 19) % T);
                var bright = (i % 3 === 0) ? S.brighten(baseColor, 15) : S.darken(baseColor, 10);
                g.fillStyle(bright, 0.4 + (i % 5) * 0.1);
                var sz = (i % 4 === 0) ? 3 : 2;
                g.fillRect(nx, ny, sz, sz);
            }

            // Embedded pebbles / small rocks
            var pebbleCount = ((seed * 7) % 3) + 1;
            for (i = 0; i < pebbleCount; i++) {
                var px = x + ((seed * 89 + i * 67) % (T - 6)) + 2;
                var py = y + ((seed * 43 + i * 91) % (T - 6)) + 2;
                var pColor = S.darken(baseColor, 20 + (i * 7) % 15);
                g.fillStyle(pColor, 0.6);
                g.fillRect(px, py, 4 + (i % 2), 3);
                g.fillStyle(S.brighten(pColor, 15), 0.3);
                g.fillRect(px, py, 2, 1);
            }

            // Accent speckles
            for (i = 0; i < 8; i++) {
                nx = x + ((seed * 31 + i * 47) % (T - 2));
                ny = y + ((seed * 67 + i * 23) % (T - 2));
                g.fillStyle(accentColor, 0.3);
                g.fillRect(nx, ny, 1, 1);
            }

            // Stratum-specific detail: worm trails (s1), rust lines (s2), bone bits (s3), glyphs (s4)
            var strataId = seed % 10;
            if (strataId >= 1 && strataId <= 4 && (seed * 13) % 5 === 0) {
                if (strataId === 1) {
                    // Trash/debris detail
                    g.fillStyle(0x888888, 0.25);
                    var dx = x + ((seed * 29) % (T - 8)) + 2;
                    var dy = y + ((seed * 61) % (T - 6)) + 2;
                    g.fillRect(dx, dy, 5, 2);
                    g.fillRect(dx + 1, dy + 2, 3, 1);
                } else if (strataId === 2) {
                    // Rust / pipe fragment
                    g.fillStyle(0x8B4513, 0.3);
                    dx = x + ((seed * 41) % (T - 10)) + 2;
                    dy = y + ((seed * 59) % (T - 4)) + 2;
                    g.fillRect(dx, dy, 7, 2);
                    g.fillStyle(0xA0522D, 0.2);
                    g.fillRect(dx + 1, dy - 1, 5, 1);
                } else if (strataId === 3) {
                    // Bone fragment
                    g.fillStyle(0xE8E8D0, 0.25);
                    dx = x + ((seed * 53) % (T - 8)) + 2;
                    dy = y + ((seed * 37) % (T - 6)) + 2;
                    g.fillRect(dx, dy, 2, 5);
                    g.fillRect(dx + 2, dy + 1, 2, 3);
                } else if (strataId === 4) {
                    // Ancient glyph mark
                    g.fillStyle(0xDAA520, 0.2);
                    dx = x + ((seed * 47) % (T - 8)) + 2;
                    dy = y + ((seed * 71) % (T - 8)) + 2;
                    g.fillRect(dx, dy, 1, 5);
                    g.fillRect(dx, dy + 2, 4, 1);
                }
            }

            // Subtle edge darkening (top and left)
            g.fillStyle(0x000000, 0.12);
            g.fillRect(x, y, T, 1);
            g.fillRect(x, y, 1, T);
            // Bottom and right highlight
            g.fillStyle(0xFFFFFF, 0.05);
            g.fillRect(x, y + T - 1, T, 1);
        },

        _drawRockTile: function(g, x, y, T, baseColor, crackColor, seed) {
            // Base
            g.fillStyle(baseColor, 1);
            g.fillRect(x, y, T, T);

            // Rock face texture - larger blocks with varied brightness
            for (var i = 0; i < 6; i++) {
                var bx = x + ((seed * 41 + i * 53) % (T - 8));
                var by = y + ((seed * 83 + i * 29) % (T - 6));
                var bw = 4 + (i % 3) * 3;
                var bh = 3 + (i % 4) * 2;
                g.fillStyle(S.brighten(baseColor, 8 + i * 4), 0.6);
                g.fillRect(bx, by, bw, bh);
                // Subtle highlight on top edge of each block
                g.fillStyle(0xFFFFFF, 0.06);
                g.fillRect(bx, by, bw, 1);
            }

            // Mineral vein - colored streak through rock
            if ((seed * 17) % 4 === 0) {
                var veinColor = (seed % 3 === 0) ? 0xC0C0C0 : (seed % 3 === 1) ? 0x8B6914 : 0x4A7A6A;
                g.fillStyle(veinColor, 0.35);
                var vx = x + ((seed * 23) % (T - 12)) + 2;
                var vy = y + ((seed * 37) % (T - 4)) + 2;
                g.fillRect(vx, vy, 8, 2);
                g.fillRect(vx + 2, vy - 1, 4, 1);
                g.fillRect(vx + 3, vy + 2, 5, 1);
                // Sparkle point on vein
                g.fillStyle(0xFFFFFF, 0.35);
                g.fillRect(vx + 4, vy, 1, 1);
            }

            // Crack lines
            g.lineStyle(1, crackColor, 0.3);
            var cx1 = x + ((seed * 17) % (T - 4));
            var cy1 = y + 2;
            g.lineBetween(cx1, cy1, cx1 + 6, cy1 + T - 4);
            var cx2 = x + ((seed * 59) % (T - 4));
            g.lineBetween(cx2, y + T - 2, cx2 + 8, y + 4);

            // Edge definition
            g.fillStyle(0x000000, 0.15);
            g.fillRect(x, y, T, 1);
            g.fillRect(x, y, 1, T);
            g.fillStyle(0xFFFFFF, 0.08);
            g.fillRect(x + 1, y + T - 1, T - 1, 1);
        },

        _drawHardRockTile: function(g, x, y, T, baseColor, streakColor, seed) {
            // Dark base
            g.fillStyle(S.darken(baseColor, 20), 1);
            g.fillRect(x, y, T, T);

            // Subtle stone grain
            for (var i = 0; i < 8; i++) {
                var gx = x + ((seed * 47 + i * 29) % T);
                var gy = y + ((seed * 61 + i * 41) % T);
                g.fillStyle(S.brighten(baseColor, 5), 0.2);
                g.fillRect(gx, gy, 3 + (i % 2), 2);
            }

            // Chunky mineral veins
            for (i = 0; i < 4; i++) {
                var vx = x + ((seed * 29 + i * 61) % (T - 6));
                var vy = y + ((seed * 43 + i * 17) % (T - 4));
                g.fillStyle(streakColor, 0.4 + i * 0.1);
                g.fillRect(vx, vy, 5 + i * 2, 2);
            }

            // Crystal formation (on some tiles)
            if ((seed * 11) % 5 === 0) {
                var crx = x + ((seed * 67) % (T - 10)) + 3;
                var cry = y + ((seed * 83) % (T - 10)) + 3;
                g.fillStyle(streakColor, 0.6);
                // Small crystal shape
                g.fillRect(crx + 1, cry, 2, 5);
                g.fillRect(crx, cry + 1, 4, 3);
                g.fillStyle(0xFFFFFF, 0.4);
                g.fillRect(crx + 1, cry + 1, 1, 1);
            }

            // Sparkle points - more prominent
            for (i = 0; i < 4; i++) {
                var sx = x + ((seed * 97 + i * 71) % (T - 2));
                var sy = y + ((seed * 13 + i * 83) % (T - 2));
                g.fillStyle(0xFFFFFF, 0.5);
                g.fillRect(sx, sy, 1, 1);
                // Cross sparkle on every other
                if (i % 2 === 0) {
                    g.fillStyle(0xFFFFFF, 0.2);
                    g.fillRect(sx - 1, sy, 3, 1);
                    g.fillRect(sx, sy - 1, 1, 3);
                }
            }

            // Strong edges
            g.fillStyle(0x000000, 0.25);
            g.fillRect(x, y, T, 1);
            g.fillRect(x, y, 1, T);
            g.fillStyle(0xFFFFFF, 0.06);
            g.fillRect(x + 1, y + T - 1, T - 1, 1);
        },

        _drawGrassTile: function(g, x, y, T) {
            // Rich dirt base with depth
            g.fillStyle(0x7B6345, 1);
            g.fillRect(x, y, T, T);
            g.fillStyle(0x8B7355, 1);
            g.fillRect(x, y, T, T * 0.6);

            // Green top layer with gradient
            g.fillStyle(0x3A6A1E, 1);
            g.fillRect(x, y, T, T * 0.3);
            g.fillStyle(0x4A7A2E, 1);
            g.fillRect(x, y, T, T * 0.2);

            // Grass blades - varied heights and shades
            for (var i = 0; i < 14; i++) {
                var gx = x + (i * 2.2) + 1;
                var gh = 2 + (i % 4) * 2;
                var shade = S.brighten(0x4A7A2E, 5 + (i * 7) % 20);
                g.fillStyle(shade, 0.9);
                g.fillRect(gx, y, 1, gh);
            }

            // Transition zone - dirt/grass mix
            for (i = 0; i < 8; i++) {
                var mx = x + (i * 4) % T;
                g.fillStyle(0x5A7A3E, 0.4);
                g.fillRect(mx, y + T * 0.3, 2, 2);
            }

            // Dirt texture with pebbles
            for (i = 0; i < 6; i++) {
                var dx = x + (i * 5) + 1;
                g.fillStyle(0xA08060, 0.4);
                g.fillRect(dx, y + T * 0.55, 3, 2);
            }

            // Root hints
            g.fillStyle(0x6B5335, 0.3);
            g.fillRect(x + 4, y + T * 0.4, 6, 1);
            g.fillRect(x + 18, y + T * 0.45, 8, 1);
        },

        _drawBedrockTile: function(g, x, y, T) {
            g.fillStyle(0x0E0E0E, 1);
            g.fillRect(x, y, T, T);
            // Massive stone blocks
            for (var i = 0; i < 6; i++) {
                g.fillStyle(0x1A1A1A, 0.6);
                g.fillRect(x + (i * 5) % T, y + (i * 7) % T, 5 + (i % 3), 4 + (i % 2));
            }
            // Faint shimmer - bedrock is ancient
            g.fillStyle(0x2A2A3A, 0.3);
            g.fillRect(x + 8, y + 12, 6, 1);
            g.fillRect(x + 18, y + 6, 4, 1);
            // Pressure cracks
            g.lineStyle(1, 0x222222, 0.4);
            g.lineBetween(x + 4, y, x + 12, y + T);
            // Oppressive dark edge
            g.fillStyle(0x000000, 0.3);
            g.fillRect(x, y, T, 2);
            g.fillRect(x, y, 2, T);
        },

        _drawHazardGasTile: function(g, x, y, T) {
            // Dark greenish dirt base
            g.fillStyle(0x2D3020, 1);
            g.fillRect(x, y, T, T);
            // Cracked earth texture
            g.fillStyle(0x3D4030, 0.6);
            g.fillRect(x + 2, y + 2, T - 4, T - 4);
            // Gas vent cracks
            g.lineStyle(1, 0x556644, 0.4);
            g.lineBetween(x + T/2 - 2, y + T, x + T/2, y + T/2);
            g.lineBetween(x + T/2, y + T/2, x + T/2 + 3, y);
            // Gas wisps - layered bubbles
            for (var i = 0; i < 6; i++) {
                var wx = x + (i * 5) + 2;
                var wy = y + T - 6 - i * 4;
                g.fillStyle(0x88CC44, 0.15 + i * 0.04);
                g.fillCircle(wx + 3, wy + 3, 3 + (i % 2));
                g.fillStyle(0xAAEE66, 0.1);
                g.fillCircle(wx + 2, wy + 2, 2);
            }
            // Warning tint border
            g.lineStyle(1, 0x88CC44, 0.25);
            g.strokeRect(x, y, T, T);
        },

        _drawHazardWaterTile: function(g, x, y, T) {
            // Wet dark base
            g.fillStyle(0x1A2A3A, 1);
            g.fillRect(x, y, T, T);
            // Damp texture patches
            for (var d = 0; d < 5; d++) {
                g.fillStyle(0x2A3A4A, 0.5);
                g.fillRect(x + (d * 6) % (T - 4), y + (d * 8) % (T - 3), 5, 3);
            }
            // Water streaks with reflection highlights
            for (var i = 0; i < 4; i++) {
                var wy = y + 5 + i * 7;
                g.fillStyle(0x3366AA, 0.3 + i * 0.05);
                g.fillRect(x + 2, wy, T - 4, 2);
                // Reflection highlight
                g.fillStyle(0x88BBEE, 0.15);
                g.fillRect(x + 4 + (i * 5) % (T - 10), wy, 4, 1);
            }
            // Drip marks
            g.fillStyle(0x4488CC, 0.2);
            g.fillRect(x + T/3, y, 1, T/2);
            g.fillRect(x + T*2/3, y + T/4, 1, T/2);
            // Blue tint border
            g.lineStyle(1, 0x4488CC, 0.2);
            g.strokeRect(x, y, T, T);
        },

        _drawBuildingTile: function(g, x, y, T) {
            // Wooden planks with wood grain
            g.fillStyle(0x7B5C2C, 1);
            g.fillRect(x, y, T, T);
            g.fillStyle(0x8B6C3C, 1);
            g.fillRect(x + 1, y + 1, T - 2, T - 2);

            // Plank lines with highlight
            g.lineStyle(1, 0x5B3C1C, 0.6);
            g.lineBetween(x, y + T / 3, x + T, y + T / 3);
            g.lineBetween(x, y + 2 * T / 3, x + T, y + 2 * T / 3);
            // Highlight below lines
            g.fillStyle(0x9B7C4C, 0.3);
            g.fillRect(x, y + T / 3 + 1, T, 1);
            g.fillRect(x, y + 2 * T / 3 + 1, T, 1);

            // Wood grain texture
            for (var i = 0; i < 6; i++) {
                g.fillStyle(0x7B5C2C, 0.25);
                g.fillRect(x + 2 + (i * 5) % (T - 4), y + (i * 4) % T, 4, 1);
            }

            // Nail dots
            g.fillStyle(0x777777, 0.7);
            g.fillCircle(x + 4, y + T / 3, 1);
            g.fillCircle(x + T - 4, y + 2 * T / 3, 1);
            // Nail highlights
            g.fillStyle(0xAAAAAA, 0.3);
            g.fillRect(x + 3, y + T / 3 - 1, 1, 1);
            g.fillRect(x + T - 5, y + 2 * T / 3 - 1, 1, 1);
        },

        _drawItemSparkle: function(g, x, y, T) {
            // Outer glow
            g.fillStyle(0xFFD700, 0.15);
            g.fillCircle(x + T / 2, y + T / 2, 6);
            // Inner glow
            g.fillStyle(0xFFD700, 0.4);
            g.fillCircle(x + T / 2, y + T / 2, 3);
            // Bright center
            g.fillStyle(0xFFFFFF, 0.9);
            g.fillCircle(x + T / 2, y + T / 2, 1);
            // Cross sparkle rays
            g.fillStyle(0xFFD700, 0.3);
            g.fillRect(x + T / 2 - 5, y + T / 2, 11, 1);
            g.fillRect(x + T / 2, y + T / 2 - 5, 1, 11);
            // Diagonal rays
            g.fillStyle(0xFFD700, 0.15);
            for (var d = 1; d <= 3; d++) {
                g.fillRect(x + T / 2 - d, y + T / 2 - d, 1, 1);
                g.fillRect(x + T / 2 + d, y + T / 2 - d, 1, 1);
                g.fillRect(x + T / 2 - d, y + T / 2 + d, 1, 1);
                g.fillRect(x + T / 2 + d, y + T / 2 + d, 1, 1);
            }
        },

        _drawTransitionTile: function(g, x, y, T, colors) {
            // Gradient blend tile for stratum boundaries
            g.fillStyle(colors.base, 1);
            g.fillRect(x, y, T, T);
            g.fillStyle(colors.dirt || colors.base, 0.5);
            g.fillRect(x, y + T / 2, T, T / 2);
        },

        _createIndividualTiles: function(scene) {
            var T = S.TILE;

            // Create per-stratum tile textures for the tilemap
            for (var s = 1; s <= 4; s++) {
                var sc = S.STRATA[s].colors;

                // Dirt tiles (2 variants per stratum)
                for (var v = 0; v < 3; v++) {
                    var gd = scene.make.graphics({ x: 0, y: 0, add: false });
                    this._drawDirtTile(gd, 0, 0, T, sc.dirt || sc.base, sc.accent1 || sc.base, s * 100 + v * 37);
                    gd.generateTexture('dirt_' + s + '_' + v, T, T);
                    gd.destroy();
                }

                // Rock tiles
                for (v = 0; v < 2; v++) {
                    var gr = scene.make.graphics({ x: 0, y: 0, add: false });
                    this._drawRockTile(gr, 0, 0, T, sc.base, sc.accent1, s * 100 + v * 53);
                    gr.generateTexture('rock_' + s + '_' + v, T, T);
                    gr.destroy();
                }

                // Hard rock
                var gh = scene.make.graphics({ x: 0, y: 0, add: false });
                this._drawHardRockTile(gh, 0, 0, T, sc.accent2 || sc.base, sc.accent1, s * 100);
                gh.generateTexture('hardrock_' + s, T, T);
                gh.destroy();
            }

            // Special tiles
            var specials = [
                { name: 'grass', fn: '_drawGrassTile' },
                { name: 'bedrock', fn: '_drawBedrockTile' },
                { name: 'hazard_gas', fn: '_drawHazardGasTile' },
                { name: 'hazard_water', fn: '_drawHazardWaterTile' },
                { name: 'building', fn: '_drawBuildingTile' },
                { name: 'item_sparkle', fn: '_drawItemSparkle' }
            ];

            for (var si = 0; si < specials.length; si++) {
                var gs = scene.make.graphics({ x: 0, y: 0, add: false });
                this[specials[si].fn](gs, 0, 0, T);
                gs.generateTexture(specials[si].name, T, T);
                gs.destroy();
            }

            // Background textures per stratum (for parallax)
            for (s = 1; s <= 4; s++) {
                sc = S.STRATA[s].colors;
                var gb = scene.make.graphics({ x: 0, y: 0, add: false });
                gb.fillStyle(sc.bg, 1);
                gb.fillRect(0, 0, T * 4, T * 4);
                // Add subtle texture
                for (var bi = 0; bi < 30; bi++) {
                    gb.fillStyle(S.brighten(sc.bg, 5 + bi % 10), 0.2);
                    gb.fillRect(
                        (bi * 17) % (T * 4),
                        (bi * 31) % (T * 4),
                        2 + bi % 3, 2 + bi % 3
                    );
                }
                gb.generateTexture('bg_' + s, T * 4, T * 4);
                gb.destroy();
            }

            // Sky texture
            var gsky = scene.make.graphics({ x: 0, y: 0, add: false });
            // Gradient sky
            for (var sy = 0; sy < S.HEIGHT; sy++) {
                var t = sy / S.HEIGHT;
                var skyCol = S.lerpColor(0x4A90D9, 0x87CEEB, t);
                gsky.fillStyle(skyCol, 1);
                gsky.fillRect(0, sy, S.WIDTH, 1);
            }
            gsky.generateTexture('sky', S.WIDTH, S.HEIGHT);
            gsky.destroy();
        },

        // ---- Player Textures ----
        generatePlayerTextures: function(scene) {
            var T = S.TILE;
            var pH = T;  // player height = 1 tile
            var pW = Math.floor(T * 0.75);  // slightly narrower than tile

            // Player idle frame
            this._generatePlayerFrame(scene, 'player_idle', pW, pH, 0);
            // Player walk frames
            this._generatePlayerFrame(scene, 'player_walk1', pW, pH, 1);
            this._generatePlayerFrame(scene, 'player_walk2', pW, pH, 2);
            // Player dig frame
            this._generatePlayerFrame(scene, 'player_dig', pW, pH, 3);

            // Combined spritesheet
            var sheetW = pW * 6;
            var sheetH = pH;
            var gs = scene.make.graphics({ x: 0, y: 0, add: false });

            for (var f = 0; f < 6; f++) {
                var xOff = f * pW;
                this._drawPlayerFrame(gs, xOff, 0, pW, pH, f);
            }

            gs.generateTexture('player_sheet', sheetW, sheetH);
            gs.destroy();

            // Create spritesheet config
            if (!scene.textures.exists('player_atlas')) {
                var frameW = pW;
                var config = {
                    frameWidth: frameW,
                    frameHeight: pH
                };
                scene.textures.get('player_sheet').setFilter(Phaser.Textures.FilterMode.NEAREST);
            }
        },

        _generatePlayerFrame: function(scene, key, w, h, frame) {
            var g = scene.make.graphics({ x: 0, y: 0, add: false });
            this._drawPlayerFrame(g, 0, 0, w, h, frame);
            g.generateTexture(key, w, h);
            g.destroy();
        },

        _drawPlayerFrame: function(g, x, y, w, h, frame) {
            var cx = x + w / 2;

            // Body color (mining suit - tan/brown)
            var bodyColor = 0xC8A060;
            var helmColor = 0xE8C840;
            var bootColor = 0x5C3C1C;
            var lampColor = 0xFFFF88;

            // Legs position based on frame
            var legOffset = 0;
            if (frame === 1) legOffset = 2;
            if (frame === 2) legOffset = -2;

            // Boots
            g.fillStyle(bootColor, 1);
            g.fillRect(cx - 5, y + h - 5, 4, 5);
            g.fillRect(cx + 1 - legOffset, y + h - 5, 4, 5);

            // Legs
            g.fillStyle(0x6B4C2C, 1);
            g.fillRect(cx - 4, y + h - 10, 3, 6);
            g.fillRect(cx + 1 - legOffset, y + h - 10, 3, 6);

            // Body
            g.fillStyle(bodyColor, 1);
            g.fillRect(cx - 5, y + 8, 10, 14);

            // Belt
            g.fillStyle(0x8B6914, 1);
            g.fillRect(cx - 5, y + 18, 10, 2);

            // Arms
            g.fillStyle(bodyColor, 1);
            if (frame === 3) {
                // Dig pose - arm extended down
                g.fillRect(cx + 5, y + 10, 3, 10);
                // Pickaxe
                g.fillStyle(0x8B6914, 1);
                g.fillRect(cx + 7, y + 8, 2, 8);
                g.fillStyle(0xAAAAAA, 1);
                g.fillRect(cx + 5, y + 6, 6, 2);
            } else {
                // Normal arms
                g.fillRect(cx - 7, y + 10, 2, 8);
                g.fillRect(cx + 5, y + 10, 2, 8);
            }

            // Head
            g.fillStyle(0xE8B88C, 1);
            g.fillRect(cx - 4, y + 2, 8, 7);

            // Hard hat
            g.fillStyle(helmColor, 1);
            g.fillRect(cx - 5, y, 10, 4);
            g.fillRect(cx - 6, y + 3, 12, 2);

            // Headlamp
            g.fillStyle(lampColor, 1);
            g.fillRect(cx - 2, y + 1, 3, 2);

            // Eye
            g.fillStyle(0x000000, 1);
            g.fillRect(cx + 1, y + 5, 2, 2);

            // Lamp glow
            g.fillStyle(lampColor, 0.3);
            g.fillCircle(cx - 1, y + 2, 4);
        },

        // ---- UI Textures ----
        generateUITextures: function(scene) {
            var g;

            // Coin icon
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0xFFD700, 1);
            g.fillCircle(8, 8, 7);
            g.fillStyle(0xDAA520, 1);
            g.fillCircle(8, 8, 5);
            g.fillStyle(0xFFD700, 1);
            g.fillCircle(7, 7, 4);
            g.fillStyle(0xDAA520, 1);
            // Dollar sign
            g.fillRect(7, 4, 2, 8);
            g.fillRect(5, 5, 6, 2);
            g.fillRect(5, 9, 6, 2);
            g.generateTexture('coin', 16, 16);
            g.destroy();

            // Stamina icon (lightning bolt)
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0x44FF44, 1);
            g.beginPath();
            g.moveTo(8, 0);
            g.lineTo(3, 8);
            g.lineTo(7, 8);
            g.lineTo(5, 16);
            g.lineTo(12, 6);
            g.lineTo(8, 6);
            g.closePath();
            g.fillPath();
            g.generateTexture('stamina_icon', 16, 16);
            g.destroy();

            // Bag icon
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0xC8A060, 1);
            g.fillRoundedRect(2, 4, 12, 11, 2);
            g.fillStyle(0xA08040, 1);
            g.fillRect(4, 0, 8, 5);
            g.lineStyle(2, 0x8B6914, 1);
            g.strokeRoundedRect(2, 4, 12, 11, 2);
            g.generateTexture('bag_icon', 16, 16);
            g.destroy();

            // Depth icon (arrow down)
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0x4FC3F7, 1);
            g.fillRect(6, 0, 4, 10);
            g.beginPath();
            g.moveTo(8, 16);
            g.lineTo(2, 8);
            g.lineTo(14, 8);
            g.closePath();
            g.fillPath();
            g.generateTexture('depth_icon', 16, 16);
            g.destroy();

            // Button backgrounds
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0x333333, 0.9);
            g.fillRoundedRect(0, 0, 200, 40, 4);
            g.lineStyle(1, 0x666666, 1);
            g.strokeRoundedRect(0, 0, 200, 40, 4);
            g.generateTexture('btn_bg', 200, 40);
            g.destroy();

            // Panel background
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0x1a1a2e, 0.95);
            g.fillRoundedRect(0, 0, 100, 100, 8);
            g.lineStyle(2, 0x333355, 1);
            g.strokeRoundedRect(0, 0, 100, 100, 8);
            g.generateTexture('panel_bg', 100, 100);
            g.destroy();
        },

        // ---- Particle Textures ----
        generateParticleTextures: function(scene) {
            var g;

            // Dirt particle
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0x8B7355, 1);
            g.fillRect(0, 0, 4, 4);
            g.generateTexture('particle_dirt', 4, 4);
            g.destroy();

            // Rock particle
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0x888888, 1);
            g.fillRect(0, 0, 4, 4);
            g.fillStyle(0xAAAAAA, 1);
            g.fillRect(0, 0, 2, 2);
            g.generateTexture('particle_rock', 4, 4);
            g.destroy();

            // Sparkle particle
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0xFFFFFF, 1);
            g.fillRect(1, 0, 2, 4);
            g.fillRect(0, 1, 4, 2);
            g.generateTexture('particle_sparkle', 4, 4);
            g.destroy();

            // Coin particle
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            g.fillStyle(0xFFD700, 1);
            g.fillCircle(3, 3, 3);
            g.fillStyle(0xDAA520, 1);
            g.fillCircle(3, 3, 2);
            g.generateTexture('particle_coin', 6, 6);
            g.destroy();

            // Generic colored particles (for per-stratum dig effects)
            for (var s = 1; s <= 4; s++) {
                var sc = S.STRATA[s].colors;
                g = scene.make.graphics({ x: 0, y: 0, add: false });
                g.fillStyle(sc.dirt || sc.base, 1);
                g.fillRect(0, 0, 4, 4);
                g.fillStyle(S.brighten(sc.dirt || sc.base, 30), 1);
                g.fillRect(0, 0, 2, 2);
                g.generateTexture('particle_dig_' + s, 4, 4);
                g.destroy();
            }

            // Rarity sparkle particles
            var rarities = ['COMMON', 'UNCOMMON', 'RARE', 'LEGENDARY'];
            for (var ri = 0; ri < rarities.length; ri++) {
                var rc = S.RARITY[rarities[ri]].color;
                g = scene.make.graphics({ x: 0, y: 0, add: false });
                g.fillStyle(rc, 1);
                g.fillRect(1, 0, 2, 4);
                g.fillRect(0, 1, 4, 2);
                g.fillStyle(0xFFFFFF, 0.8);
                g.fillRect(1, 1, 2, 2);
                g.generateTexture('particle_rarity_' + rarities[ri].toLowerCase(), 4, 4);
                g.destroy();
            }

            // Light mask for headlamp
            var lmSize = 512;
            g = scene.make.graphics({ x: 0, y: 0, add: false });
            // Radial gradient approximation
            var steps = 32;
            for (var ls = steps; ls >= 0; ls--) {
                var lr = (ls / steps) * (lmSize / 2);
                var la = 1 - (ls / steps);
                g.fillStyle(0xFFFFFF, la);
                g.fillCircle(lmSize / 2, lmSize / 2, lr);
            }
            g.generateTexture('light_mask', lmSize, lmSize);
            g.destroy();
        }
    };

})(window.STRATA);
