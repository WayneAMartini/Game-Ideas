// STRATA - Surface Scenes (Market, Upgrades, Contracts, Prep, Summary, Inventory)
// ================================================================================
(function(S) {
    'use strict';

    // Helper: draw a panel background with subtle inner glow
    function drawPanel(g, x, y, w, h, color) {
        color = color || 0x1a1a2e;
        // Drop shadow
        g.fillStyle(0x000000, 0.5);
        g.fillRoundedRect(x + 4, y + 4, w, h, 10);
        // Main panel
        g.fillStyle(color, 0.95);
        g.fillRoundedRect(x, y, w, h, 10);
        // Inner border highlight (top)
        g.fillStyle(0xFFFFFF, 0.04);
        g.fillRoundedRect(x + 2, y + 2, w - 4, h / 3, { tl: 8, tr: 8, bl: 0, br: 0 });
        // Outer border
        g.lineStyle(2, 0x444477, 0.8);
        g.strokeRoundedRect(x, y, w, h, 10);
        // Inner border
        g.lineStyle(1, 0x222244, 0.4);
        g.strokeRoundedRect(x + 3, y + 3, w - 6, h - 6, 8);
    }

    // Helper: draw a button with depth
    function drawBtn(g, x, y, w, h, color, hover) {
        g.clear();
        // Shadow
        g.fillStyle(0x000000, 0.35);
        g.fillRoundedRect(x + 2, y + 2, w, h, 5);
        // Main fill
        g.fillStyle(color, hover ? 1 : 0.8);
        g.fillRoundedRect(x, y, w, h, 5);
        // Top highlight
        g.fillStyle(0xFFFFFF, hover ? 0.15 : 0.06);
        g.fillRoundedRect(x + 2, y + 1, w - 4, h / 2 - 2, { tl: 4, tr: 4, bl: 0, br: 0 });
        // Border
        g.lineStyle(1, hover ? 0xFFFFFF : S.brighten(color, 50), hover ? 0.9 : 0.6);
        g.strokeRoundedRect(x, y, w, h, 5);
        // Bottom edge darkening
        if (!hover) {
            g.fillStyle(0x000000, 0.1);
            g.fillRoundedRect(x + 2, y + h - 4, w - 4, 3, { tl: 0, tr: 0, bl: 4, br: 4 });
        }
    }

    // Helper: make interactive button with hover animation
    function makeButton(scene, x, y, w, h, label, color, callback) {
        var bg = scene.add.graphics();
        drawBtn(bg, x, y, w, h, color, false);
        var txt = scene.add.text(x + w / 2, y + h / 2, label, {
            fontSize: '13px', fontFamily: 'monospace', color: '#FFFFFF', fontStyle: 'bold'
        }).setOrigin(0.5);
        var zone = scene.add.zone(x + w / 2, y + h / 2, w, h).setInteractive({ useHandCursor: true });
        zone.on('pointerover', function() {
            drawBtn(bg, x, y, w, h, color, true);
            scene.tweens.add({ targets: txt, scaleX: 1.05, scaleY: 1.05, duration: 80, ease: 'Power1' });
        });
        zone.on('pointerout', function() {
            drawBtn(bg, x, y, w, h, color, false);
            scene.tweens.add({ targets: txt, scaleX: 1, scaleY: 1, duration: 80, ease: 'Power1' });
        });
        zone.on('pointerdown', function() {
            if (S.Audio && S.Audio.playClick) S.Audio.playClick();
            // Press feedback: brief scale down then call
            scene.tweens.add({
                targets: txt, scaleX: 0.95, scaleY: 0.95, duration: 50, yoyo: true,
                onComplete: function() { callback(); }
            });
        });
        return { bg: bg, txt: txt, zone: zone };
    }

    // ==================================================================
    // PREP SCENE - Before each expedition
    // ==================================================================
    S.PrepScene = new Phaser.Class({
        Extends: Phaser.Scene,
        initialize: function PrepScene() {
            Phaser.Scene.call(this, { key: 'PrepScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var W = S.WIDTH, H = S.HEIGHT;

            // Background
            this.cameras.main.setBackgroundColor('#0a0a0a');
            var bg = this.add.graphics();
            drawPanel(bg, 60, 40, W - 120, H - 80);

            // Title
            this.add.text(cx, 70, 'EXPEDITION PREP', {
                fontSize: '28px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5);

            // Expedition number
            this.add.text(cx, 100, 'Expedition #' + (S.state.expedition.number + 1), {
                fontSize: '14px', fontFamily: 'monospace', color: '#AAAAAA'
            }).setOrigin(0.5);

            // Stats
            var statsY = 140;
            var stats = [
                { label: 'Stamina', value: Math.ceil(S.state.stamina) + '/' + S.state.maxStamina, color: '#44FF44' },
                { label: 'Gold', value: S.state.gold, color: '#FFD700' },
                { label: 'Inventory', value: S.state.inventory.length + '/' + S.state.effects.inventorySlots, color: '#C8A060' },
                { label: 'Dig Speed', value: S.state.effects.digSpeed.toFixed(1) + 'x', color: '#E07040' },
                { label: 'Sell Bonus', value: '+' + (S.state.effects.sellBonus * 100).toFixed(0) + '%', color: '#FFD700' }
            ];

            for (var i = 0; i < stats.length; i++) {
                this.add.text(200, statsY + i * 28, stats[i].label + ':', {
                    fontSize: '14px', fontFamily: 'monospace', color: '#888888'
                });
                this.add.text(400, statsY + i * 28, '' + stats[i].value, {
                    fontSize: '14px', fontFamily: 'monospace', color: stats[i].color, fontStyle: 'bold'
                });
            }

            // Market event
            var eventY = statsY + stats.length * 28 + 20;
            var econ = S.Systems.Economy;
            if (econ.currentEvent) {
                this.add.text(cx, eventY, 'Market Event: ' + econ.currentEvent.name, {
                    fontSize: '14px', fontFamily: 'monospace',
                    color: S.colorToHex(econ.currentEvent.color), fontStyle: 'bold'
                }).setOrigin(0.5);
                this.add.text(cx, eventY + 20, '(' + econ.eventTimer + ' expeditions remaining)', {
                    fontSize: '11px', fontFamily: 'monospace', color: '#888888'
                }).setOrigin(0.5);
            } else {
                this.add.text(cx, eventY, 'No active market events', {
                    fontSize: '13px', fontFamily: 'monospace', color: '#666666'
                }).setOrigin(0.5);
            }

            // Active contracts
            var contractY = eventY + 50;
            var contracts = S.Systems.Contracts;
            if (contracts.active.length > 0) {
                this.add.text(cx, contractY, 'Active Contracts:', {
                    fontSize: '14px', fontFamily: 'monospace', color: '#AAAAAA', fontStyle: 'bold'
                }).setOrigin(0.5);
                for (i = 0; i < Math.min(3, contracts.active.length); i++) {
                    var c = contracts.active[i];
                    var status = c.completed ? '[COMPLETE]' : c.collected + '/' + c.quantity;
                    this.add.text(cx, contractY + 22 + i * 18,
                        c.targetItemName + ' x' + c.quantity + ' -> ' + c.factionName + '  ' + status, {
                        fontSize: '11px', fontFamily: 'monospace',
                        color: c.completed ? '#44FF44' : S.colorToHex(c.factionColor)
                    }).setOrigin(0.5);
                }
            }

            // Auto-Digger Section — compact side-by-side layout
            var diggerY = contractY + (contracts.active.length > 0 ? 22 + Math.min(3, contracts.active.length) * 18 + 15 : 25);
            this.add.text(cx, diggerY, 'DIGGER CREW', {
                fontSize: '12px', fontFamily: 'monospace', color: '#E07040', fontStyle: 'bold'
            }).setOrigin(0.5);
            diggerY += 20;

            // Left side: auto-dig | Right side: hire diggers
            var leftX = cx - 170;
            var rightX = cx + 10;
            var colW = 160;

            // Auto-dig (unlocks Q toggle for player character)
            if (!S.state.autoDigEnabled) {
                var enableCost = S.AUTO_DIGGER.enableCost;
                var canAffordAuto = S.state.gold >= enableCost;
                makeButton(self, leftX, diggerY, colW, 28,
                    'Auto [Q] ' + enableCost + 'g', canAffordAuto ? 0x5A4A2E : 0x333333,
                    function() {
                        if (S.state.gold >= S.AUTO_DIGGER.enableCost) {
                            S.state.gold -= S.AUTO_DIGGER.enableCost;
                            S.state.autoDigEnabled = true;
                            if (S.Audio && S.Audio.playUpgrade) S.Audio.playUpgrade();
                            self.scene.restart();
                        }
                    });
            } else {
                this.add.text(leftX + colW / 2, diggerY + 14, 'Auto [Q]: Unlocked', {
                    fontSize: '10px', fontFamily: 'monospace', color: '#44FF44', fontStyle: 'bold'
                }).setOrigin(0.5);
            }

            // Hire diggers
            var currentDiggers = S.state.hiredDiggers;
            var maxDiggers = S.AUTO_DIGGER.maxDiggers;
            if (currentDiggers < maxDiggers) {
                var hireCost = S.AUTO_DIGGER.hireCosts[currentDiggers] || 9999;
                var canAffordHire = S.state.gold >= hireCost;
                makeButton(self, rightX, diggerY, colW, 28,
                    '+Digger ' + hireCost + 'g (' + currentDiggers + '/' + maxDiggers + ')',
                    canAffordHire ? 0x2E5A4A : 0x333333,
                    function() {
                        var cost = S.AUTO_DIGGER.hireCosts[S.state.hiredDiggers];
                        if (S.state.gold >= cost && S.state.hiredDiggers < S.AUTO_DIGGER.maxDiggers) {
                            S.state.gold -= cost;
                            S.state.hiredDiggers++;
                            if (S.Audio && S.Audio.playUpgrade) S.Audio.playUpgrade();
                            self.scene.restart();
                        }
                    });
            } else {
                this.add.text(rightX + colW / 2, diggerY + 14, 'Crew: ' + currentDiggers + '/' + maxDiggers, {
                    fontSize: '11px', fontFamily: 'monospace', color: '#44FF44', fontStyle: 'bold'
                }).setOrigin(0.5);
            }

            // Buttons — positioned relative to bottom
            var btnY = H - 200;
            makeButton(this, cx - 80, btnY, 160, 44, 'DESCEND', 0x4A7A2E, function() {
                // Descent animation: screen shakes, text slides down, fade to black
                self.cameras.main.shake(300, 0.008);
                var midY = H / 2;
                var descentText = self.add.text(cx, midY - 20, 'DESCENDING...', {
                    fontSize: '28px', fontFamily: 'monospace', color: '#FFD700',
                    fontStyle: 'bold', stroke: '#000000', strokeThickness: 4
                }).setOrigin(0.5).setAlpha(0);
                self.tweens.add({
                    targets: descentText,
                    alpha: 1, y: midY + 30, duration: 400, ease: 'Power2'
                });
                self.time.delayedCall(300, function() {
                    self.cameras.main.fadeOut(400, 0, 0, 0, function(cam, progress) {
                        if (progress === 1) {
                            self.scene.start('GameScene');
                        }
                    });
                });
            });

            // Save & Quit button
            makeButton(this, cx - 180, btnY + 56, 160, 44, 'SAVE & QUIT', 0x664444, function() {
                if (S.Save) S.Save.save();
                self.scene.start('MenuScene');
            });

            // Manual save button
            makeButton(this, cx + 20, btnY + 56, 160, 44, 'SAVE', 0x446644, function() {
                if (S.Save && S.Save.save()) {
                    self.showSaveIndicator();
                }
            });

            // Navigation buttons
            var navY = H - 80;
            makeButton(this, 100, navY, 120, 36, 'Market [M]', 0x444466, function() {
                self.scene.start('MarketScene');
            });
            makeButton(this, 240, navY, 140, 36, 'Upgrades [U]', 0x444466, function() {
                self.scene.start('UpgradeScene');
            });
            makeButton(this, 400, navY, 140, 36, 'Contracts [C]', 0x444466, function() {
                self.scene.start('ContractScene');
            });
            makeButton(this, 560, navY, 140, 36, 'Inventory [I]', 0x444466, function() {
                self.scene.launch('InventoryScene');
            });

            // Save indicator
            var saveInfo = S.Save ? S.Save.getSaveInfo() : null;
            if (saveInfo && saveInfo.timestamp) {
                this.add.text(W - 80, 55, 'Saved ' + S.Save.formatTime(saveInfo.timestamp), {
                    fontSize: '10px', fontFamily: 'monospace', color: '#556655'
                }).setOrigin(1, 0.5);
            }

            // Keyboard shortcuts
            this.input.keyboard.on('keydown-M', function() { self.scene.start('MarketScene'); });
            this.input.keyboard.on('keydown-U', function() { self.scene.start('UpgradeScene'); });
            this.input.keyboard.on('keydown-C', function() { self.scene.start('ContractScene'); });
            this.input.keyboard.on('keydown-I', function() { self.scene.launch('InventoryScene'); });

            this.cameras.main.fadeIn(300);
        },

        showSaveIndicator: function() {
            var cx = S.WIDTH / 2;
            var indicator = this.add.text(cx, 55, 'Game Saved!', {
                fontSize: '14px', fontFamily: 'monospace', color: '#44FF44', fontStyle: 'bold'
            }).setOrigin(0.5).setAlpha(0);

            this.tweens.add({
                targets: indicator,
                alpha: 1, duration: 200,
                yoyo: true, hold: 1000,
                onComplete: function() { indicator.destroy(); }
            });
        }
    });

    // ==================================================================
    // SUMMARY SCENE - After each expedition
    // ==================================================================
    S.SummaryScene = new Phaser.Class({
        Extends: Phaser.Scene,
        initialize: function SummaryScene() {
            Phaser.Scene.call(this, { key: 'SummaryScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var W = S.WIDTH, H = S.HEIGHT;
            var exp = S.state.expedition;

            this.cameras.main.setBackgroundColor('#0a0a0a');
            var bg = this.add.graphics();
            drawPanel(bg, 80, 30, W - 160, H - 60);

            // Title with entrance animation
            var title = this.add.text(cx, 60, 'EXPEDITION COMPLETE', {
                fontSize: '26px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5).setAlpha(0).setScale(0.8);
            this.tweens.add({ targets: title, alpha: 1, scaleX: 1, scaleY: 1, duration: 400, ease: 'Back.easeOut' });

            // Stats with staggered reveal
            var statsY = 110;
            var statLines = [
                { label: 'Max Depth', value: exp.maxDepth + 'm', color: '#4FC3F7' },
                { label: 'Tiles Dug', value: '' + exp.tilesDug, color: '#E07040' },
                { label: 'Items Found', value: '' + exp.itemsFound, color: '#FFD700' },
                { label: 'Duration', value: Math.floor((this.time.now - exp.startTime) / 1000) + 's', color: '#AAAAAA' }
            ];

            for (var i = 0; i < statLines.length; i++) {
                var labelTxt = this.add.text(220, statsY + i * 28, statLines[i].label + ':', {
                    fontSize: '14px', fontFamily: 'monospace', color: '#888888'
                }).setAlpha(0);
                var valTxt = this.add.text(440, statsY + i * 28, statLines[i].value, {
                    fontSize: '14px', fontFamily: 'monospace', color: statLines[i].color, fontStyle: 'bold'
                }).setAlpha(0);
                this.tweens.add({ targets: [labelTxt, valTxt], alpha: 1, x: '+=0', duration: 300, delay: 300 + i * 120 });
            }

            // Items found list
            var itemsY = statsY + statLines.length * 28 + 20;
            var itemsHeader = this.add.text(cx, itemsY, 'Items in Inventory:', {
                fontSize: '14px', fontFamily: 'monospace', color: '#AAAAAA', fontStyle: 'bold'
            }).setOrigin(0.5).setAlpha(0);
            this.tweens.add({ targets: itemsHeader, alpha: 1, duration: 300, delay: 800 });

            itemsY += 24;
            var maxShow = Math.min(8, S.state.inventory.length);
            for (i = 0; i < maxShow; i++) {
                var item = S.state.inventory[i];
                var rColor = S.RARITY[item.def.rarity].hex;
                var dmg = item.damaged ? ' [DAMAGED]' : '';
                var itemNameTxt = this.add.text(200, itemsY + i * 20,
                    item.def.name + dmg, {
                    fontSize: '12px', fontFamily: 'monospace', color: rColor
                }).setAlpha(0);
                // Estimated best price
                var bestPrice = this.getBestPrice(item);
                var priceTxt = this.add.text(550, itemsY + i * 20, '~' + bestPrice + 'g', {
                    fontSize: '12px', fontFamily: 'monospace', color: '#FFD700'
                }).setAlpha(0);
                this.tweens.add({ targets: [itemNameTxt, priceTxt], alpha: 1, duration: 200, delay: 900 + i * 80 });
            }
            if (S.state.inventory.length > maxShow) {
                var moreText = this.add.text(cx, itemsY + maxShow * 20, '...and ' + (S.state.inventory.length - maxShow) + ' more', {
                    fontSize: '11px', fontFamily: 'monospace', color: '#666666'
                }).setOrigin(0.5).setAlpha(0);
                this.tweens.add({ targets: moreText, alpha: 1, duration: 300, delay: 900 + maxShow * 80 });
            }
            if (S.state.inventory.length === 0) {
                this.add.text(cx, itemsY, 'No items collected', {
                    fontSize: '12px', fontFamily: 'monospace', color: '#666666'
                }).setOrigin(0.5);
            }

            // Estimated total haul
            var totalHaul = 0;
            for (i = 0; i < S.state.inventory.length; i++) {
                totalHaul += this.getBestPrice(S.state.inventory[i]);
            }
            this.add.text(cx, H - 130, 'Estimated Haul Value: ' + totalHaul + ' gold', {
                fontSize: '16px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5);

            // Continue button
            makeButton(this, cx - 80, H - 90, 160, 44, 'CONTINUE', 0x4A7A2E, function() {
                self.scene.start('PrepScene');
            });

            this.cameras.main.fadeIn(300);
        },

        getBestPrice: function(invItem) {
            var best = 0;
            var factions = Object.keys(S.FACTIONS);
            for (var i = 0; i < factions.length; i++) {
                var p = S.Systems.Economy.getPrice(invItem.def, factions[i], invItem.damaged);
                if (p > best) best = p;
            }
            return best;
        }
    });

    // ==================================================================
    // MARKET SCENE - Selling items to factions
    // ==================================================================
    S.MarketScene = new Phaser.Class({
        Extends: Phaser.Scene,
        initialize: function MarketScene() {
            Phaser.Scene.call(this, { key: 'MarketScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var W = S.WIDTH, H = S.HEIGHT;

            this.cameras.main.setBackgroundColor('#0a0a0a');
            this.selectedFaction = 'curator';
            this.scrollOffset = 0;

            // Background panel
            var bg = this.add.graphics();
            drawPanel(bg, 20, 20, W - 40, H - 40);

            // Title
            this.add.text(cx, 45, 'HOLLOWMARKET', {
                fontSize: '24px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5);

            // Market event banner
            var econ = S.Systems.Economy;
            if (econ.currentEvent) {
                this.add.text(cx, 70, econ.currentEvent.name + ' (' + econ.eventTimer + ' exp. left)', {
                    fontSize: '12px', fontFamily: 'monospace',
                    color: S.colorToHex(econ.currentEvent.color), fontStyle: 'bold'
                }).setOrigin(0.5);
            }

            // Faction tabs
            var factions = Object.keys(S.FACTIONS);
            var tabX = 60;
            this.factionTabs = [];
            for (var fi = 0; fi < factions.length; fi++) {
                (function(fid, idx) {
                    var f = S.FACTIONS[fid];
                    var tx = tabX + idx * 200;
                    var tabBg = self.add.graphics();
                    var tabTxt = self.add.text(tx + 90, 100, f.name, {
                        fontSize: '13px', fontFamily: 'monospace', color: '#FFFFFF', fontStyle: 'bold'
                    }).setOrigin(0.5);

                    var tier = econ.getRepTier(fid);
                    var repTxt = self.add.text(tx + 90, 118, tier.name + ' (+' + (tier.bonus * 100) + '%)', {
                        fontSize: '10px', fontFamily: 'monospace', color: S.colorToHex(f.color)
                    }).setOrigin(0.5);

                    var zone = self.add.zone(tx + 90, 108, 180, 36).setInteractive({ useHandCursor: true });
                    zone.on('pointerdown', function() {
                        self.selectedFaction = fid;
                        self.refreshItems();
                    });

                    self.factionTabs.push({ bg: tabBg, txt: tabTxt, repTxt: repTxt, zone: zone, factionId: fid });
                })(factions[fi], fi);
            }

            // Gold display
            this.goldText = this.add.text(W - 60, 45, 'Gold: ' + S.state.gold, {
                fontSize: '14px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(1, 0.5);

            // Item list area
            this.itemContainer = this.add.container(0, 0);

            // Sell All button
            makeButton(this, cx - 80, H - 80, 160, 36, 'SELL ALL', 0x8B6914, function() {
                self.sellAll();
            });

            // Back button
            makeButton(this, 40, H - 80, 100, 36, 'BACK [ESC]', 0x444466, function() {
                self.goBack();
            });

            this.input.keyboard.on('keydown-ESC', function() { self.goBack(); });

            // Scroll with mouse wheel
            this.input.on('wheel', function(pointer, dx, dy, dz) {
                self.scrollOffset = Math.max(0, self.scrollOffset + (dz > 0 ? 1 : -1) * 30);
                self.refreshItems();
            });

            this.refreshItems();
            this.refreshTabs();
            this.cameras.main.fadeIn(200);
        },

        refreshTabs: function() {
            for (var i = 0; i < this.factionTabs.length; i++) {
                var tab = this.factionTabs[i];
                var f = S.FACTIONS[tab.factionId];
                var selected = (tab.factionId === this.selectedFaction);
                tab.bg.clear();
                var tx = 60 + i * 200;
                tab.bg.fillStyle(f.color, selected ? 0.4 : 0.15);
                tab.bg.fillRoundedRect(tx, 88, 180, 36, 4);
                if (selected) {
                    tab.bg.lineStyle(2, f.color, 1);
                    tab.bg.strokeRoundedRect(tx, 88, 180, 36, 4);
                }
            }
        },

        refreshItems: function() {
            var self = this;
            this.refreshTabs();
            this.itemContainer.removeAll(true);

            var startY = 145 - this.scrollOffset;
            var inv = S.state.inventory;
            var faction = this.selectedFaction;

            if (inv.length === 0) {
                var emptyTxt = this.add.text(S.WIDTH / 2, 250, 'No items to sell', {
                    fontSize: '14px', fontFamily: 'monospace', color: '#666666'
                }).setOrigin(0.5);
                this.itemContainer.add(emptyTxt);
                return;
            }

            for (var i = 0; i < inv.length; i++) {
                var item = inv[i];
                var yPos = startY + i * 36;
                if (yPos < 130 || yPos > S.HEIGHT - 110) continue;

                var price = S.Systems.Economy.getPrice(item.def, faction, item.damaged);
                var rColor = S.RARITY[item.def.rarity].hex;
                var dmg = item.damaged ? ' [DMG]' : '';

                // Item row background
                var rowBg = this.add.graphics();
                rowBg.fillStyle(0xFFFFFF, 0.03);
                rowBg.fillRect(60, yPos - 2, S.WIDTH - 120, 32);
                this.itemContainer.add(rowBg);

                // Rarity dot
                var dot = this.add.circle(80, yPos + 12, 5, S.RARITY[item.def.rarity].color);
                this.itemContainer.add(dot);

                // Name
                var nameTxt = this.add.text(95, yPos + 4, item.def.name + dmg, {
                    fontSize: '12px', fontFamily: 'monospace', color: rColor
                });
                this.itemContainer.add(nameTxt);

                // Price
                var priceTxt = this.add.text(450, yPos + 4, price > 0 ? price + 'g' : 'No interest', {
                    fontSize: '12px', fontFamily: 'monospace',
                    color: price > 0 ? '#FFD700' : '#666666', fontStyle: 'bold'
                });
                this.itemContainer.add(priceTxt);

                // Sell button
                if (price > 0) {
                    (function(index, p) {
                        var sbg = self.add.graphics();
                        drawBtn(sbg, 560, yPos, 70, 28, 0x4A7A2E, false);
                        self.itemContainer.add(sbg);
                        var stxt = self.add.text(595, yPos + 14, 'SELL', {
                            fontSize: '11px', fontFamily: 'monospace', color: '#FFFFFF', fontStyle: 'bold'
                        }).setOrigin(0.5);
                        self.itemContainer.add(stxt);
                        var szone = self.add.zone(595, yPos + 14, 70, 28).setInteractive({ useHandCursor: true });
                        szone.on('pointerover', function() { drawBtn(sbg, 560, yPos, 70, 28, 0x4A7A2E, true); });
                        szone.on('pointerout', function() { drawBtn(sbg, 560, yPos, 70, 28, 0x4A7A2E, false); });
                        szone.on('pointerdown', function() {
                            self.sellItem(index);
                        });
                        self.itemContainer.add(szone);
                    })(i, price);
                }

                // Trend indicator
                var trend = S.Systems.Economy.getPriceTrend(item.def.id);
                var trendSymbol = trend === 'up' ? '^' : trend === 'down' ? 'v' : '-';
                var trendColor = trend === 'up' ? '#44FF44' : trend === 'down' ? '#FF4444' : '#888888';
                var trendTxt = this.add.text(640, yPos + 4, trendSymbol, {
                    fontSize: '14px', fontFamily: 'monospace', color: trendColor, fontStyle: 'bold'
                });
                this.itemContainer.add(trendTxt);
            }
        },

        sellItem: function(index) {
            if (index < 0 || index >= S.state.inventory.length) return;

            var item = S.state.inventory[index];
            var price = S.Systems.Economy.sell(item, this.selectedFaction);

            if (price > 0) {
                S.state.gold += price;
                S.Systems.Contracts.checkSale(item.def, this.selectedFaction);
                S.state.inventory.splice(index, 1);

                if (S.Audio && S.Audio.playSell) S.Audio.playSell(price);

                // Floating gold value animation
                var goldFloat = this.add.text(S.WIDTH - 60, 60, '+' + price + 'g', {
                    fontSize: '16px', fontFamily: 'monospace', color: '#FFD700',
                    fontStyle: 'bold', stroke: '#000000', strokeThickness: 2
                }).setOrigin(0.5).setAlpha(0);
                this.tweens.add({
                    targets: goldFloat,
                    alpha: 1, y: 40, duration: 300, ease: 'Power2'
                });
                this.tweens.add({
                    targets: goldFloat,
                    alpha: 0, y: 20, duration: 400, delay: 800,
                    onComplete: function() { goldFloat.destroy(); }
                });

                // Gold text pulse
                this.goldText.setText('Gold: ' + S.state.gold);
                this.tweens.add({
                    targets: this.goldText,
                    scaleX: 1.2, scaleY: 1.2, duration: 100,
                    yoyo: true, ease: 'Power2'
                });

                this.scrollOffset = 0;
                this.refreshItems();

                if (S.Save) S.Save.save();
            }
        },

        sellAll: function() {
            var totalGold = 0;
            var faction = this.selectedFaction;

            for (var i = S.state.inventory.length - 1; i >= 0; i--) {
                var item = S.state.inventory[i];
                var price = S.Systems.Economy.getPrice(item.def, faction, item.damaged);
                if (price > 0) {
                    var actual = S.Systems.Economy.sell(item, faction);
                    S.state.gold += actual;
                    totalGold += actual;
                    S.Systems.Contracts.checkSale(item.def, faction);
                    S.state.inventory.splice(i, 1);
                }
            }

            if (totalGold > 0) {
                if (S.Audio && S.Audio.playSell) S.Audio.playSell(totalGold);

                // Big floating total
                var goldFloat = this.add.text(S.WIDTH / 2, S.HEIGHT / 2 - 20, '+' + totalGold + ' GOLD', {
                    fontSize: '24px', fontFamily: 'monospace', color: '#FFD700',
                    fontStyle: 'bold', stroke: '#000000', strokeThickness: 3
                }).setOrigin(0.5).setAlpha(0);
                this.tweens.add({
                    targets: goldFloat,
                    alpha: 1, scaleX: 1.1, scaleY: 1.1, duration: 300, ease: 'Back.easeOut'
                });
                this.tweens.add({
                    targets: goldFloat,
                    alpha: 0, y: S.HEIGHT / 2 - 60, duration: 500, delay: 1000,
                    onComplete: function() { goldFloat.destroy(); }
                });

                this.cameras.main.flash(200, 255, 215, 0);
            }

            this.goldText.setText('Gold: ' + S.state.gold);
            this.tweens.add({
                targets: this.goldText,
                scaleX: 1.3, scaleY: 1.3, duration: 150,
                yoyo: true, ease: 'Power2'
            });
            this.scrollOffset = 0;
            this.refreshItems();

            if (S.Save) S.Save.save();
        },

        goBack: function() {
            this.scene.start('PrepScene');
        }
    });

    // ==================================================================
    // UPGRADE SCENE - 3 branching trees
    // ==================================================================
    S.UpgradeScene = new Phaser.Class({
        Extends: Phaser.Scene,
        initialize: function UpgradeScene() {
            Phaser.Scene.call(this, { key: 'UpgradeScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var W = S.WIDTH, H = S.HEIGHT;

            this.cameras.main.setBackgroundColor('#0a0a0a');
            this.selectedTree = 'excavation';

            // Background
            var bg = this.add.graphics();
            drawPanel(bg, 20, 20, W - 40, H - 40);

            // Title
            this.add.text(cx, 45, 'UPGRADES', {
                fontSize: '24px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5);

            // Gold
            this.goldText = this.add.text(W - 60, 45, 'Gold: ' + S.state.gold, {
                fontSize: '14px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(1, 0.5);

            // Tree tabs
            var trees = Object.keys(S.UPGRADE_TREES);
            this.treeTabs = [];
            for (var ti = 0; ti < trees.length; ti++) {
                (function(treeId, idx) {
                    var tree = S.UPGRADE_TREES[treeId];
                    var tx = 60 + idx * 200;
                    var tabBg = self.add.graphics();
                    var tabTxt = self.add.text(tx + 90, 80, tree.name, {
                        fontSize: '14px', fontFamily: 'monospace', color: '#FFFFFF', fontStyle: 'bold'
                    }).setOrigin(0.5);
                    var subtxt = self.add.text(tx + 90, 96, tree.desc, {
                        fontSize: '10px', fontFamily: 'monospace', color: '#888888'
                    }).setOrigin(0.5);

                    var zone = self.add.zone(tx + 90, 88, 180, 30).setInteractive({ useHandCursor: true });
                    zone.on('pointerdown', function() {
                        self.selectedTree = treeId;
                        self.refreshTree();
                    });

                    self.treeTabs.push({ bg: tabBg, factionId: treeId, color: tree.color });
                })(trees[ti], ti);
            }

            // Tree container
            this.treeContainer = this.add.container(0, 0);

            // Tooltip
            this.tooltipBg = this.add.graphics().setDepth(50).setVisible(false);
            this.tooltipText = this.add.text(0, 0, '', {
                fontSize: '11px', fontFamily: 'monospace', color: '#FFFFFF',
                wordWrap: { width: 200 }
            }).setDepth(51).setVisible(false);

            // Back button
            makeButton(this, 40, H - 80, 100, 36, 'BACK [ESC]', 0x444466, function() {
                self.scene.start('PrepScene');
            });
            this.input.keyboard.on('keydown-ESC', function() { self.scene.start('PrepScene'); });

            this.refreshTree();
            this.cameras.main.fadeIn(200);
        },

        refreshTree: function() {
            var self = this;
            this.treeContainer.removeAll(true);

            // Update tab highlights
            for (var i = 0; i < this.treeTabs.length; i++) {
                var tab = this.treeTabs[i];
                var selected = (tab.factionId === this.selectedTree);
                tab.bg.clear();
                var tx = 60 + i * 200;
                tab.bg.fillStyle(tab.color, selected ? 0.4 : 0.15);
                tab.bg.fillRoundedRect(tx, 70, 180, 36, 4);
                if (selected) {
                    tab.bg.lineStyle(2, tab.color, 1);
                    tab.bg.strokeRoundedRect(tx, 70, 180, 36, 4);
                }
            }

            var tree = S.UPGRADE_TREES[this.selectedTree];
            if (!tree) return;

            var startY = 130;
            var tierSpacing = 110;
            var cx = S.WIDTH / 2;

            // Draw connection lines
            var lineG = this.add.graphics();
            lineG.lineStyle(2, 0x333366, 0.5);
            this.treeContainer.add(lineG);

            var visibleTierCount = 0;
            for (var t = 0; t < tree.tiers.length; t++) {
                var tier = tree.tiers[t];
                var visible = this.isTierVisible(this.selectedTree, t);

                if (!visible) {
                    // Show a locked placeholder row
                    var lockY = startY + visibleTierCount * tierSpacing;
                    var lockLabel = this.add.text(cx, lockY + 16, '? ? ?  Tier ' + (t + 1) + ' — Locked  ? ? ?', {
                        fontSize: '13px', fontFamily: 'monospace', color: '#333344', fontStyle: 'bold'
                    }).setOrigin(0.5);
                    this.treeContainer.add(lockLabel);

                    var lockHint = this.add.text(cx, lockY + 34, 'Purchase a Tier ' + t + ' upgrade to reveal', {
                        fontSize: '10px', fontFamily: 'monospace', color: '#222233'
                    }).setOrigin(0.5);
                    this.treeContainer.add(lockHint);
                    visibleTierCount++;
                    continue;
                }

                var tierY = startY + visibleTierCount * tierSpacing;
                var nodeSpacing = 220;
                var startX = cx - ((tier.length - 1) * nodeSpacing) / 2;

                // Tier label
                var tierLabel = this.add.text(50, tierY + 10, 'Tier ' + (t + 1), {
                    fontSize: '11px', fontFamily: 'monospace', color: '#555555'
                });
                this.treeContainer.add(tierLabel);

                for (var n = 0; n < tier.length; n++) {
                    var upgrade = tier[n];
                    var nx = startX + n * nodeSpacing;
                    var ny = tierY;

                    var purchased = S.state.purchasedUpgrades.indexOf(upgrade.id) !== -1;
                    var canBuy = this.canPurchase(upgrade);
                    var requiresMet = !upgrade.requires || S.state.purchasedUpgrades.indexOf(upgrade.requires) !== -1;

                    // Draw connection to parent with styled branch lines
                    if (upgrade.requires) {
                        var parent = S.UPGRADE_MAP[upgrade.requires];
                        if (parent && this.isTierVisible(this.selectedTree, parent.tier)) {
                            var parentPurchased = S.state.purchasedUpgrades.indexOf(parent.id) !== -1;
                            var lineColor = parentPurchased ? tree.color : 0x333366;
                            var lineAlpha = parentPurchased ? 0.8 : 0.3;
                            var parentTier = tree.tiers[parent.tier];
                            var parentIdx = parentTier.indexOf(parent);
                            var parentNodeSpacing = 220;
                            var parentStartX = cx - ((parentTier.length - 1) * parentNodeSpacing) / 2;
                            var parentVisIdx = 0;
                            for (var pt = 0; pt < parent.tier; pt++) {
                                if (this.isTierVisible(this.selectedTree, pt)) parentVisIdx++;
                            }
                            var px = parentStartX + parentIdx * parentNodeSpacing;
                            var py = startY + parentVisIdx * tierSpacing;
                            // Draw L-shaped connection (down from parent, then across to child)
                            var midY = py + 44 + (ny - py - 44) / 2;
                            lineG.lineStyle(2, lineColor, lineAlpha);
                            lineG.lineBetween(px, py + 44, px, midY);
                            lineG.lineBetween(px, midY, nx, midY);
                            lineG.lineBetween(nx, midY, nx, ny);
                            // Arrow dot at child end
                            lineG.fillStyle(lineColor, lineAlpha);
                            lineG.fillCircle(nx, ny, 3);
                            // Glow dot at parent if purchased
                            if (parentPurchased) {
                                lineG.fillStyle(tree.color, 0.4);
                                lineG.fillCircle(px, py + 44, 4);
                            }
                        }
                    }

                    // Node background with depth
                    var nodeColor = purchased ? tree.color : canBuy ? 0x444466 : requiresMet ? 0x333344 : 0x222233;
                    var nodeBg = this.add.graphics();
                    // Shadow
                    nodeBg.fillStyle(0x000000, 0.25);
                    nodeBg.fillRoundedRect(nx - 88, ny + 3, 180, 44, 6);
                    // Main
                    nodeBg.fillStyle(nodeColor, purchased ? 0.6 : 0.3);
                    nodeBg.fillRoundedRect(nx - 90, ny, 180, 44, 6);
                    // Top highlight for purchased
                    if (purchased) {
                        nodeBg.fillStyle(0xFFFFFF, 0.08);
                        nodeBg.fillRoundedRect(nx - 88, ny + 2, 176, 18, { tl: 4, tr: 4, bl: 0, br: 0 });
                    }
                    // Border
                    if (purchased) {
                        nodeBg.lineStyle(2, tree.color, 1);
                    } else if (canBuy) {
                        nodeBg.lineStyle(2, S.brighten(tree.color, -40), 0.6);
                    } else {
                        nodeBg.lineStyle(1, 0x333333, 0.4);
                    }
                    nodeBg.strokeRoundedRect(nx - 90, ny, 180, 44, 6);
                    // Checkmark for purchased
                    if (purchased) {
                        nodeBg.fillStyle(0x44FF44, 0.8);
                        nodeBg.fillCircle(nx + 78, ny + 10, 6);
                        nodeBg.fillStyle(0x000000, 0.9);
                        nodeBg.fillRect(nx + 75, ny + 10, 3, 1);
                        nodeBg.fillRect(nx + 77, ny + 9, 1, 3);
                    }
                    this.treeContainer.add(nodeBg);

                    // Node text
                    var nameColor = purchased ? '#FFFFFF' : canBuy ? '#CCCCCC' : requiresMet ? '#888888' : '#555555';
                    var nameTxt = this.add.text(nx, ny + 12, upgrade.name, {
                        fontSize: '12px', fontFamily: 'monospace', color: nameColor, fontStyle: 'bold'
                    }).setOrigin(0.5);
                    this.treeContainer.add(nameTxt);

                    var costText = purchased ? 'OWNED' : upgrade.cost + 'g';
                    var costColor = purchased ? '#44FF44' : canBuy ? '#FFD700' : '#666666';
                    var costTxt = this.add.text(nx, ny + 30, costText, {
                        fontSize: '11px', fontFamily: 'monospace', color: costColor
                    }).setOrigin(0.5);
                    this.treeContainer.add(costTxt);

                    // Interactive zone
                    (function(upg, bx, by) {
                        var zone = self.add.zone(bx, by + 22, 180, 44).setInteractive({ useHandCursor: true });
                        zone.on('pointerover', function(pointer) {
                            self.showTooltip(pointer.x, pointer.y, upg);
                        });
                        zone.on('pointerout', function() {
                            self.hideTooltip();
                        });
                        zone.on('pointerdown', function() {
                            if (self.canPurchase(upg) && !self.isPurchased(upg.id)) {
                                self.purchaseUpgrade(upg);
                            }
                        });
                        self.treeContainer.add(zone);
                    })(upgrade, nx, ny);
                }
                visibleTierCount++;
            }
        },

        canPurchase: function(upgrade) {
            if (S.state.purchasedUpgrades.indexOf(upgrade.id) !== -1) return false;
            if (S.state.gold < upgrade.cost) return false;
            if (upgrade.requires && S.state.purchasedUpgrades.indexOf(upgrade.requires) === -1) return false;
            return true;
        },

        // Determine if a tier is visible based on progressive reveal:
        // Tiers 0-1 always visible, tier N+1 visible when any tier N-1 upgrade is purchased
        isTierVisible: function(treeId, tierIdx) {
            if (tierIdx <= 1) return true; // Base + first branch always visible
            // Tier N is visible if player owns any upgrade from tier N-2
            var tree = S.UPGRADE_TREES[treeId];
            var checkTier = tree.tiers[tierIdx - 1];
            if (!checkTier) return false;
            for (var i = 0; i < checkTier.length; i++) {
                if (S.state.purchasedUpgrades.indexOf(checkTier[i].id) !== -1) return true;
            }
            return false;
        },

        isPurchased: function(id) {
            return S.state.purchasedUpgrades.indexOf(id) !== -1;
        },

        purchaseUpgrade: function(upgrade) {
            S.state.gold -= upgrade.cost;
            S.state.purchasedUpgrades.push(upgrade.id);
            S.recalcEffects();

            if (S.Audio && S.Audio.playUpgrade) S.Audio.playUpgrade();

            this.goldText.setText('Gold: ' + S.state.gold);

            // Upgrade name announcement
            var tree = S.UPGRADE_TREES[upgrade.tree];
            var treeColor = tree ? S.colorToHex(tree.color) : '#FFD700';
            var unlockText = this.add.text(S.WIDTH / 2, S.HEIGHT / 2 - 10, upgrade.name + ' UNLOCKED', {
                fontSize: '20px', fontFamily: 'monospace', color: treeColor,
                fontStyle: 'bold', stroke: '#000000', strokeThickness: 4
            }).setOrigin(0.5).setAlpha(0).setDepth(50);

            this.tweens.add({
                targets: unlockText,
                alpha: 1, scaleX: 1.1, scaleY: 1.1, duration: 300, ease: 'Back.easeOut'
            });
            this.tweens.add({
                targets: unlockText,
                alpha: 0, y: S.HEIGHT / 2 - 50, duration: 500, delay: 1200,
                onComplete: function() { unlockText.destroy(); }
            });

            // Flash with tree color
            var color = tree ? tree.color : 0xFFD700;
            var r = (color >> 16) & 0xFF;
            var g = (color >> 8) & 0xFF;
            var b = color & 0xFF;
            this.cameras.main.flash(300, r, g, b);

            // Brief shake
            this.cameras.main.shake(200, 0.005);

            this.refreshTree();

            if (S.Save) S.Save.save();
        },

        showTooltip: function(x, y, upgrade) {
            var text = upgrade.name + '\n' + upgrade.desc + '\nCost: ' + upgrade.cost + 'g';
            var effects = [];
            var fx = upgrade.effects;
            if (fx.digSpeed !== undefined) effects.push('Dig Speed: ' + fx.digSpeed + 'x');
            if (fx.inventorySlots !== undefined) effects.push('Inventory: ' + fx.inventorySlots + ' slots');
            if (fx.sellBonus !== undefined && fx.sellBonus > 0) effects.push('Sell Bonus: +' + (fx.sellBonus * 100) + '%');
            if (fx.moveSpeed !== undefined) effects.push('Move Speed: ' + fx.moveSpeed);
            if (fx.neverDamage) effects.push('Never damages items');
            if (fx.aoeRadius) effects.push('AoE Radius: ' + fx.aoeRadius);
            if (effects.length > 0) text += '\n\n' + effects.join('\n');

            this.tooltipText.setText(text);
            var tw = Math.min(220, this.tooltipText.width + 20);
            var th = this.tooltipText.height + 16;

            var tx = Math.min(x + 10, S.WIDTH - tw - 10);
            var ty = Math.min(y + 10, S.HEIGHT - th - 10);

            this.tooltipBg.clear();
            this.tooltipBg.fillStyle(0x000000, 0.9);
            this.tooltipBg.fillRoundedRect(tx - 4, ty - 4, tw, th, 4);
            this.tooltipBg.lineStyle(1, 0x666666, 1);
            this.tooltipBg.strokeRoundedRect(tx - 4, ty - 4, tw, th, 4);
            this.tooltipBg.setVisible(true);

            this.tooltipText.setPosition(tx + 4, ty + 4);
            this.tooltipText.setVisible(true);
        },

        hideTooltip: function() {
            this.tooltipBg.setVisible(false);
            this.tooltipText.setVisible(false);
        }
    });

    // ==================================================================
    // CONTRACT SCENE - Bounty board
    // ==================================================================
    S.ContractScene = new Phaser.Class({
        Extends: Phaser.Scene,
        initialize: function ContractScene() {
            Phaser.Scene.call(this, { key: 'ContractScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var W = S.WIDTH, H = S.HEIGHT;
            var contracts = S.Systems.Contracts;

            this.cameras.main.setBackgroundColor('#0a0a0a');

            var bg = this.add.graphics();
            drawPanel(bg, 40, 20, W - 80, H - 40);

            // Title
            this.add.text(cx, 50, 'BOUNTY BOARD', {
                fontSize: '24px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(0.5);

            this.goldText = this.add.text(W - 80, 50, 'Gold: ' + S.state.gold, {
                fontSize: '14px', fontFamily: 'monospace', color: '#FFD700', fontStyle: 'bold'
            }).setOrigin(1, 0.5);

            // Available contracts
            var y = 90;
            this.add.text(cx, y, 'Available Contracts', {
                fontSize: '16px', fontFamily: 'monospace', color: '#AAAAAA', fontStyle: 'bold'
            }).setOrigin(0.5);
            y += 30;

            if (contracts.available.length === 0) {
                this.add.text(cx, y, 'No contracts available right now', {
                    fontSize: '12px', fontFamily: 'monospace', color: '#666666'
                }).setOrigin(0.5);
                y += 30;
            }

            for (var i = 0; i < contracts.available.length; i++) {
                var c = contracts.available[i];
                this.drawContract(c, 80, y, false, i);
                y += 70;
            }

            // Active contracts
            y += 20;
            this.add.text(cx, y, 'Active Contracts', {
                fontSize: '16px', fontFamily: 'monospace', color: '#AAAAAA', fontStyle: 'bold'
            }).setOrigin(0.5);
            y += 30;

            if (contracts.active.length === 0) {
                this.add.text(cx, y, 'No active contracts', {
                    fontSize: '12px', fontFamily: 'monospace', color: '#666666'
                }).setOrigin(0.5);
            }

            for (i = 0; i < contracts.active.length; i++) {
                c = contracts.active[i];
                this.drawContract(c, 80, y, true, i);
                y += 70;
            }

            // Back button
            makeButton(this, 60, H - 80, 100, 36, 'BACK [ESC]', 0x444466, function() {
                self.scene.start('PrepScene');
            });
            this.input.keyboard.on('keydown-ESC', function() { self.scene.start('PrepScene'); });

            this.cameras.main.fadeIn(200);
        },

        drawContract: function(c, x, y, isActive, index) {
            var self = this;
            var W = S.WIDTH - 160;
            var g = this.add.graphics();

            g.fillStyle(0xFFFFFF, 0.03);
            g.fillRoundedRect(x, y, W, 60, 4);
            g.lineStyle(1, c.factionColor, 0.5);
            g.strokeRoundedRect(x, y, W, 60, 4);

            // Faction color bar
            g.fillStyle(c.factionColor, 0.3);
            g.fillRect(x, y, 5, 60);

            // Contract details
            this.add.text(x + 20, y + 8, c.factionName + ' wants:', {
                fontSize: '11px', fontFamily: 'monospace', color: S.colorToHex(c.factionColor)
            });

            var rColor = S.RARITY[c.targetRarity].hex;
            this.add.text(x + 20, y + 24, c.targetItemName + ' x' + c.quantity, {
                fontSize: '13px', fontFamily: 'monospace', color: rColor, fontStyle: 'bold'
            });

            this.add.text(x + 20, y + 42, 'Reward: ' + c.reward + 'g + ' + c.repReward + ' rep', {
                fontSize: '11px', fontFamily: 'monospace', color: '#FFD700'
            });

            // Expires
            this.add.text(x + W - 120, y + 8, 'Expires: ' + c.expires + ' exp.', {
                fontSize: '10px', fontFamily: 'monospace', color: '#888888'
            });

            if (isActive) {
                // Progress
                var pct = c.collected / c.quantity;
                g.fillStyle(0x333333, 0.8);
                g.fillRect(x + W - 200, y + 30, 100, 12);
                g.fillStyle(c.completed ? 0x44FF44 : 0x4FC3F7, 1);
                g.fillRect(x + W - 200, y + 30, 100 * pct, 12);

                this.add.text(x + W - 150, y + 32, c.collected + '/' + c.quantity, {
                    fontSize: '10px', fontFamily: 'monospace', color: '#FFFFFF'
                }).setOrigin(0.5, 0);

                // Claim button if completed
                if (c.completed) {
                    makeButton(this, x + W - 90, y + 26, 80, 28, 'CLAIM', 0x4A7A2E, function() {
                        var claimed = S.Systems.Contracts.claim(c.id);
                        if (claimed) {
                            if (S.Audio && S.Audio.playSell) S.Audio.playSell(claimed.reward);
                            if (S.Save) S.Save.save();
                            self.scene.restart();
                        }
                    });
                }
            } else {
                // Accept button
                makeButton(this, x + W - 100, y + 16, 90, 28, 'ACCEPT', 0x8B6914, function() {
                    S.Systems.Contracts.accept(c.id);
                    self.scene.restart();
                });
            }
        }
    });

    // ==================================================================
    // INVENTORY SCENE - Overlay showing items
    // ==================================================================
    S.InventoryScene = new Phaser.Class({
        Extends: Phaser.Scene,
        initialize: function InventoryScene() {
            Phaser.Scene.call(this, { key: 'InventoryScene' });
        },

        create: function() {
            var self = this;
            var cx = S.WIDTH / 2;
            var W = 400, H = 450;
            var ox = cx - W / 2;
            var oy = (S.HEIGHT - H) / 2;

            // Semi-transparent backdrop
            this.add.rectangle(S.WIDTH / 2, S.HEIGHT / 2, S.WIDTH, S.HEIGHT, 0x000000, 0.5)
                .setInteractive(); // block clicks through

            // Panel
            var bg = this.add.graphics();
            drawPanel(bg, ox, oy, W, H);

            // Title
            this.add.text(cx, oy + 20, 'INVENTORY', {
                fontSize: '18px', fontFamily: 'monospace', color: '#C8A060', fontStyle: 'bold'
            }).setOrigin(0.5);

            var capacity = S.state.effects.inventorySlots;
            this.add.text(cx, oy + 42, S.state.inventory.length + '/' + capacity + ' slots', {
                fontSize: '12px', fontFamily: 'monospace', color: '#888888'
            }).setOrigin(0.5);

            // Grid
            var gridCols = 5;
            var cellSize = 64;
            var gridX = ox + (W - gridCols * cellSize) / 2;
            var gridY = oy + 65;

            for (var i = 0; i < capacity; i++) {
                var col = i % gridCols;
                var row = Math.floor(i / gridCols);
                var cellX = gridX + col * cellSize;
                var cellY = gridY + row * cellSize;

                // Cell background
                var cellBg = this.add.graphics();
                cellBg.fillStyle(0x222244, 0.5);
                cellBg.fillRoundedRect(cellX, cellY, cellSize - 4, cellSize - 4, 3);
                cellBg.lineStyle(1, 0x444466, 0.5);
                cellBg.strokeRoundedRect(cellX, cellY, cellSize - 4, cellSize - 4, 3);

                if (i < S.state.inventory.length) {
                    var item = S.state.inventory[i];
                    var rColor = S.RARITY[item.def.rarity].color;

                    // Rarity border
                    cellBg.lineStyle(2, rColor, 0.8);
                    cellBg.strokeRoundedRect(cellX, cellY, cellSize - 4, cellSize - 4, 3);

                    // Item icon (colored square with first letter)
                    var icon = this.add.rectangle(cellX + 30, cellY + 22, 24, 24, rColor, 0.3);
                    var letter = this.add.text(cellX + 30, cellY + 22, item.def.name.charAt(0), {
                        fontSize: '16px', fontFamily: 'monospace', color: S.RARITY[item.def.rarity].hex,
                        fontStyle: 'bold'
                    }).setOrigin(0.5);

                    // Name (truncated)
                    var shortName = item.def.name.length > 8 ? item.def.name.substring(0, 8) + '..' : item.def.name;
                    this.add.text(cellX + 30, cellY + 42, shortName, {
                        fontSize: '8px', fontFamily: 'monospace', color: '#CCCCCC'
                    }).setOrigin(0.5);

                    if (item.damaged) {
                        this.add.text(cellX + 30, cellY + 52, 'DMG', {
                            fontSize: '8px', fontFamily: 'monospace', color: '#FF4444'
                        }).setOrigin(0.5);
                    }

                    // Tooltip on hover
                    (function(itm, ix, iy) {
                        var zone = self.add.zone(ix + 30, iy + 30, cellSize - 4, cellSize - 4)
                            .setInteractive({ useHandCursor: true });
                        zone.on('pointerover', function(ptr) {
                            self.showTooltip(ptr.x, ptr.y, itm);
                        });
                        zone.on('pointerout', function() {
                            self.hideTooltip();
                        });
                    })(item, cellX, cellY);
                }
            }

            // Close button
            makeButton(this, cx - 50, oy + H - 45, 100, 32, 'CLOSE [I]', 0x444466, function() {
                self.scene.stop();
            });

            this.input.keyboard.on('keydown-I', function() { self.scene.stop(); });
            this.input.keyboard.on('keydown-ESC', function() { self.scene.stop(); });

            // Tooltip elements
            this.tooltipBg = this.add.graphics().setDepth(50).setVisible(false);
            this.tooltipText = this.add.text(0, 0, '', {
                fontSize: '11px', fontFamily: 'monospace', color: '#FFFFFF',
                wordWrap: { width: 200 }
            }).setDepth(51).setVisible(false);
        },

        showTooltip: function(x, y, item) {
            var text = item.def.name + (item.damaged ? ' [DAMAGED]' : '') +
                       '\n' + S.RARITY[item.def.rarity].name +
                       '\n' + item.def.desc +
                       '\nWeight: ' + item.def.weight +
                       '\nBase Value: ' + item.def.baseValue + 'g';

            this.tooltipText.setText(text);
            var tw = this.tooltipText.width + 16;
            var th = this.tooltipText.height + 12;
            var tx = Math.min(x + 10, S.WIDTH - tw - 10);
            var ty = Math.min(y - th - 10, S.HEIGHT - th - 10);
            if (ty < 10) ty = y + 20;

            this.tooltipBg.clear();
            this.tooltipBg.fillStyle(0x000000, 0.95);
            this.tooltipBg.fillRoundedRect(tx - 4, ty - 4, tw, th, 4);
            this.tooltipBg.lineStyle(1, 0x666666, 1);
            this.tooltipBg.strokeRoundedRect(tx - 4, ty - 4, tw, th, 4);
            this.tooltipBg.setVisible(true);
            this.tooltipText.setPosition(tx + 4, ty + 2);
            this.tooltipText.setVisible(true);
        },

        hideTooltip: function() {
            this.tooltipBg.setVisible(false);
            this.tooltipText.setVisible(false);
        }
    });

})(window.STRATA);
