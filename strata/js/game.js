import { CANVAS_WIDTH, CANVAS_HEIGHT, GAME_STATES, SURFACE_HEIGHT, PLAYER, ECONOMY, RARITY } from './config.js';
import { initInput, initMouse, clearJustPressed, clearMouseJustClicked, isKeyJustPressed, getMouse } from './input.js';
import { World } from './world.js';
import { Player } from './player.js';
import { Camera } from './camera.js';
import { Renderer } from './renderer.js';
import { ParticleSystem } from './particles.js';
import { Economy, FACTIONS } from './economy.js';
import { UpgradeManager } from './upgrades.js';
import { ContractManager } from './contracts.js';
import { AudioManager } from './audio.js';
import { UI } from './ui.js';
import { getItemRarity } from './items.js';

export class Game {
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
