// STRATA - Boot Scene & Menu Scene
// ==================================
(function(S) {
    'use strict';

    // ============================================================
    // BootScene: generates all procedural textures, shows loading
    // ============================================================
    S.BootScene = new Phaser.Class({
        Extends: Phaser.Scene,

        initialize: function BootScene() {
            Phaser.Scene.call(this, { key: 'BootScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var cy = S.HEIGHT / 2;

            // Dark background
            this.cameras.main.setBackgroundColor('#0a0a0a');

            // Loading text
            var loadText = this.add.text(cx, cy, 'LOADING...', {
                fontSize: '24px', fontFamily: 'monospace', color: '#FFD700'
            }).setOrigin(0.5).setAlpha(0);

            this.tweens.add({
                targets: loadText, alpha: 1, duration: 300,
                onComplete: function() {
                    // Generate textures (will be populated when textures.js is built)
                    if (S.Textures && S.Textures.generateAll) {
                        S.Textures.generateAll(self);
                    }
                    // Small delay for visual feedback then transition
                    self.time.delayedCall(200, function() {
                        self.scene.start('MenuScene');
                    });
                }
            });
        }
    });

    // ============================================================
    // MenuScene: animated title screen with parallax background
    // ============================================================
    S.MenuScene = new Phaser.Class({
        Extends: Phaser.Scene,

        initialize: function MenuScene() {
            Phaser.Scene.call(this, { key: 'MenuScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var cy = S.HEIGHT / 2;
            var W = S.WIDTH;
            var H = S.HEIGHT;

            // ---- Parallax dirt layer background ----
            this.bgLayers = [];
            var layerColors = [
                { top: 0x1a1a2e, bot: 0x16213e },  // deep blue
                { top: 0x2D1B2E, bot: 0x1A0A1A },  // catacombs purple
                { top: 0x36454F, bot: 0x2A2A3A },  // foundation gray
                { top: 0x5C4033, bot: 0x3D3D3D },  // dump brown
                { top: 0x6B8E23, bot: 0x556B2F }   // surface green
            ];

            for (var i = 0; i < layerColors.length; i++) {
                var g = this.add.graphics();
                var yStart = H * (i / layerColors.length);
                var yEnd = H * ((i + 1) / layerColors.length);
                var layerH = yEnd - yStart;

                // Draw jagged layer with slight texture
                g.fillStyle(layerColors[i].top, 1);
                g.fillRect(0, yStart, W, layerH * 0.5);
                g.fillStyle(layerColors[i].bot, 1);
                g.fillRect(0, yStart + layerH * 0.5, W, layerH * 0.5);

                // Add some noise dots for texture
                for (var d = 0; d < 60; d++) {
                    var dx = Math.random() * W;
                    var dy = yStart + Math.random() * layerH;
                    var dc = S.brighten(layerColors[i].top, Math.floor(Math.random() * 30));
                    g.fillStyle(dc, 0.3 + Math.random() * 0.3);
                    g.fillRect(dx, dy, 2 + Math.random() * 3, 2 + Math.random() * 3);
                }

                // Jagged transition line at top of layer (except first)
                if (i > 0) {
                    g.lineStyle(2, S.brighten(layerColors[i].top, 40), 0.6);
                    g.beginPath();
                    g.moveTo(0, yStart);
                    for (var jx = 0; jx <= W; jx += 8) {
                        g.lineTo(jx, yStart + Math.sin(jx * 0.05 + i) * 4 + Math.random() * 2);
                    }
                    g.strokePath();
                }

                this.bgLayers.push({ graphics: g, baseY: yStart, speed: 0.2 + i * 0.1 });
            }

            // ---- Floating particles (dust/debris) ----
            this.dustParticles = [];
            for (var p = 0; p < 30; p++) {
                var particle = this.add.circle(
                    Math.random() * W, Math.random() * H,
                    1 + Math.random() * 2,
                    0xFFD700, 0.1 + Math.random() * 0.3
                );
                this.dustParticles.push({
                    obj: particle,
                    vx: -0.3 + Math.random() * 0.6,
                    vy: -0.5 - Math.random() * 0.5,
                    baseAlpha: particle.alpha
                });
            }

            // ---- Dark overlay for contrast ----
            this.add.rectangle(cx, cy, W, H, 0x000000, 0.5);

            // ---- Title: "STRATA" with letter-by-letter animation ----
            var titleLetters = 'STRATA';
            this.titleTexts = [];
            var letterSpacing = 52;
            var titleStartX = cx - ((titleLetters.length - 1) * letterSpacing) / 2;

            for (var li = 0; li < titleLetters.length; li++) {
                var letter = this.add.text(
                    titleStartX + li * letterSpacing, cy - 120,
                    titleLetters[li],
                    {
                        fontSize: '72px', fontFamily: 'monospace',
                        color: '#FFD700', fontStyle: 'bold',
                        stroke: '#8B6914', strokeThickness: 4
                    }
                ).setOrigin(0.5).setAlpha(0).setScale(0.5);

                this.titleTexts.push(letter);

                this.tweens.add({
                    targets: letter,
                    alpha: 1, scaleX: 1, scaleY: 1,
                    duration: 400,
                    delay: 150 + li * 100,
                    ease: 'Back.easeOut'
                });
            }

            // ---- Subtitle ----
            var subtitle = this.add.text(cx, cy - 55, 'Dig. Appraise. Sell. Go Deeper.', {
                fontSize: '16px', fontFamily: 'monospace', color: '#CCCCCC'
            }).setOrigin(0.5).setAlpha(0);

            this.tweens.add({
                targets: subtitle, alpha: 1, y: cy - 50,
                duration: 600, delay: 900, ease: 'Power2'
            });

            // ---- Decorative line ----
            var line = this.add.graphics();
            line.lineStyle(1, 0xFFD700, 0.6);
            line.lineBetween(cx - 120, cy - 30, cx + 120, cy - 30);
            line.setAlpha(0);
            this.tweens.add({ targets: line, alpha: 1, duration: 400, delay: 1100 });

            // ---- Check for existing save ----
            var hasSave = S.Save && S.Save.hasSave();
            var saveInfo = hasSave ? S.Save.getSaveInfo() : null;
            var btnW = 220;
            var btnH = 48;
            var contBtnH = 64; // taller to fit info text

            // ---- CONTINUE Button (if save exists) ----
            if (hasSave) {
                var contY = cy + 10;
                var contBg = this.add.graphics();
                this.drawButton(contBg, cx - btnW / 2, contY - contBtnH / 2, btnW, contBtnH, 0x4A7A2E, false);

                var contText = this.add.text(cx, contY - 8, 'CONTINUE', {
                    fontSize: '22px', fontFamily: 'monospace', color: '#44FF44', fontStyle: 'bold'
                }).setOrigin(0.5);

                // Save info text
                var infoStr = '';
                if (saveInfo) {
                    infoStr = 'Exp #' + saveInfo.expeditionNumber + '  |  ' + saveInfo.gold + 'g';
                }
                var contInfo = this.add.text(cx, contY + 14, infoStr, {
                    fontSize: '10px', fontFamily: 'monospace', color: '#88CC88'
                }).setOrigin(0.5);

                var contZone = this.add.zone(cx, contY, btnW, contBtnH).setInteractive({ useHandCursor: true });
                contZone.on('pointerover', function() {
                    self.drawButton(contBg, cx - btnW / 2, contY - contBtnH / 2, btnW, contBtnH, 0x66AA44, true);
                    contText.setColor('#FFFFFF');
                    self.tweens.add({ targets: contText, scaleX: 1.05, scaleY: 1.05, duration: 100 });
                });
                contZone.on('pointerout', function() {
                    self.drawButton(contBg, cx - btnW / 2, contY - contBtnH / 2, btnW, contBtnH, 0x4A7A2E, false);
                    contText.setColor('#44FF44');
                    self.tweens.add({ targets: contText, scaleX: 1, scaleY: 1, duration: 100 });
                });
                contZone.on('pointerdown', function() {
                    if (S.Audio && S.Audio.playClick) S.Audio.playClick();
                    self.continueGame();
                });

                contBg.setAlpha(0); contText.setAlpha(0); contInfo.setAlpha(0);
                this.tweens.add({ targets: [contBg, contText, contInfo, contZone], alpha: 1, duration: 400, delay: 1300 });
            }

            // ---- NEW GAME Button ----
            var btnY = hasSave ? cy + 85 : cy + 30;

            var btnBg = this.add.graphics();
            this.drawButton(btnBg, cx - btnW / 2, btnY - btnH / 2, btnW, btnH, 0x8B6914, false);

            var btnText = this.add.text(cx, btnY, 'NEW GAME', {
                fontSize: '22px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5);

            var btnZone = this.add.zone(cx, btnY, btnW, btnH).setInteractive({ useHandCursor: true });

            // Button hover effects
            btnZone.on('pointerover', function() {
                self.drawButton(btnBg, cx - btnW / 2, btnY - btnH / 2, btnW, btnH, 0xDAA520, true);
                btnText.setColor('#FFFFFF');
                self.tweens.add({ targets: btnText, scaleX: 1.05, scaleY: 1.05, duration: 100 });
            });
            btnZone.on('pointerout', function() {
                self.drawButton(btnBg, cx - btnW / 2, btnY - btnH / 2, btnW, btnH, 0x8B6914, false);
                btnText.setColor('#FFD700');
                self.tweens.add({ targets: btnText, scaleX: 1, scaleY: 1, duration: 100 });
            });
            btnZone.on('pointerdown', function() {
                if (S.Audio && S.Audio.playClick) S.Audio.playClick();
                self.startGame();
            });

            // Fade in button group
            btnBg.setAlpha(0);
            btnText.setAlpha(0);
            this.tweens.add({ targets: [btnBg, btnText, btnZone], alpha: 1, duration: 400, delay: 1300 });

            // ---- Controls Overview ----
            var controlsY = btnY + btnH / 2 + 16;
            var controls = [
                'WASD / Arrows : Move & Jump',
                'DOWN / S : Dig below',
                'SHIFT + Direction : Dig sideways',
                'E : Return to surface',
                'M : Market  |  U : Upgrades  |  C : Contracts',
                'I : Inventory  |  ESC : Back'
            ];

            var controlsGroup = [];
            for (var ci = 0; ci < controls.length; ci++) {
                var ct = this.add.text(cx, controlsY + ci * 20, controls[ci], {
                    fontSize: '12px', fontFamily: 'monospace', color: '#888888'
                }).setOrigin(0.5).setAlpha(0);
                controlsGroup.push(ct);

                this.tweens.add({
                    targets: ct, alpha: 0.8,
                    duration: 300, delay: 1500 + ci * 60
                });
            }

            // ---- Version info ----
            this.add.text(W - 10, H - 10, 'v0.1 | Phaser ' + Phaser.VERSION, {
                fontSize: '10px', fontFamily: 'monospace', color: '#444444'
            }).setOrigin(1, 1);

            // ---- Pulsing prompt ----
            this.time.delayedCall(2000, function() {
                self.tweens.add({
                    targets: btnText,
                    alpha: { from: 1, to: 0.6 },
                    yoyo: true, repeat: -1, duration: 800
                });
            });
        },

        drawButton: function(g, x, y, w, h, color, hover) {
            g.clear();
            // Shadow
            g.fillStyle(0x000000, 0.4);
            g.fillRoundedRect(x + 2, y + 2, w, h, 6);
            // Main
            g.fillStyle(color, hover ? 1 : 0.8);
            g.fillRoundedRect(x, y, w, h, 6);
            // Border
            g.lineStyle(2, hover ? 0xFFD700 : 0xDAA520, 1);
            g.strokeRoundedRect(x, y, w, h, 6);
            // Highlight
            g.fillStyle(0xFFFFFF, hover ? 0.15 : 0.08);
            g.fillRoundedRect(x + 2, y + 2, w - 4, h / 2 - 2, { tl: 4, tr: 4, bl: 0, br: 0 });
        },

        startGame: function() {
            var self = this;

            // Flash and transition
            this.cameras.main.flash(300, 255, 215, 0);

            // Tween all elements down
            this.tweens.add({
                targets: this.titleTexts.concat(this.dustParticles.map(function(p) { return p.obj; })),
                alpha: 0, y: '+=50',
                duration: 400, ease: 'Power2'
            });

            this.time.delayedCall(500, function() {
                S.resetState();
                if (S.Systems && S.Systems.Economy) {
                    S.Systems.Economy.init();
                }
                if (S.Systems && S.Systems.Contracts) {
                    S.Systems.Contracts.init();
                }
                S.recalcEffects();

                // Delete any old save when starting fresh
                if (S.Save) S.Save.deleteSave();

                // Start the game - go to PrepScene or GameScene depending on what's available
                if (self.scene.get('PrepScene')) {
                    self.scene.start('PrepScene');
                } else if (self.scene.get('GameScene')) {
                    self.scene.start('GameScene');
                } else {
                    console.log('STRATA: No game scene available yet');
                }
            });
        },

        continueGame: function() {
            var self = this;

            this.cameras.main.flash(300, 68, 255, 68);

            this.tweens.add({
                targets: this.titleTexts.concat(this.dustParticles.map(function(p) { return p.obj; })),
                alpha: 0, y: '+=50',
                duration: 400, ease: 'Power2'
            });

            this.time.delayedCall(500, function() {
                // Initialize systems before loading (load overwrites their state)
                if (S.Systems && S.Systems.Economy) S.Systems.Economy.init();
                if (S.Systems && S.Systems.Contracts) S.Systems.Contracts.init();

                if (S.Save && S.Save.load()) {
                    if (self.scene.get('PrepScene')) {
                        self.scene.start('PrepScene');
                    }
                } else {
                    // Save corrupted — fall back to new game
                    S.resetState();
                    S.recalcEffects();
                    if (self.scene.get('PrepScene')) {
                        self.scene.start('PrepScene');
                    }
                }
            });
        },

        update: function(time) {
            // Animate dust particles
            if (this.dustParticles) {
                for (var i = 0; i < this.dustParticles.length; i++) {
                    var p = this.dustParticles[i];
                    p.obj.x += p.vx;
                    p.obj.y += p.vy;
                    p.obj.alpha = p.baseAlpha * (0.5 + 0.5 * Math.sin(time * 0.002 + i));

                    // Wrap
                    if (p.obj.y < -10) { p.obj.y = S.HEIGHT + 10; p.obj.x = Math.random() * S.WIDTH; }
                    if (p.obj.x < -10) p.obj.x = S.WIDTH + 10;
                    if (p.obj.x > S.WIDTH + 10) p.obj.x = -10;
                }
            }

            // Gentle bob on title letters
            if (this.titleTexts) {
                for (var t = 0; t < this.titleTexts.length; t++) {
                    this.titleTexts[t].y = (S.HEIGHT / 2 - 120) + Math.sin(time * 0.002 + t * 0.5) * 3;
                }
            }
        }
    });

})(window.STRATA);
