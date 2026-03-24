import { ECONOMY, RARITY } from './config.js';
import { ITEMS_BY_ID, getItemRarity } from './items.js';

// Buyer factions
export const FACTIONS = {
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

export class Economy {
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
