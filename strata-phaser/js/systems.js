// STRATA - Game Systems (Economy, Upgrades, Contracts)
// =====================================================
(function(S) {
    'use strict';

    S.Systems = S.Systems || {};

    // ==================================================================
    // ECONOMY SYSTEM
    // ==================================================================
    S.Systems.Economy = {
        priceModifiers: {},
        supply: {},
        demand: {},
        currentEvent: null,
        eventTimer: 0,
        priceHistory: {},

        init: function() {
            var self = this;
            this.priceModifiers = {};
            this.supply = {};
            this.demand = {};
            this.priceHistory = {};

            S.ITEMS.forEach(function(item) {
                self.priceModifiers[item.id] = 0.9 + Math.random() * 0.2;
                self.supply[item.id] = 0;
                self.demand[item.id] = 0;
                self.priceHistory[item.id] = [];
            });

            this.rollEvent();
        },

        getPrice: function(itemDef, factionId, damaged) {
            var faction = S.FACTIONS[factionId];
            if (!faction) return 0;

            var buyerInterest = (itemDef.buyers && itemDef.buyers[factionId]) || 0;
            if (buyerInterest === 0) return 0;

            var rarityMult = S.RARITY[itemDef.rarity].mult;
            var priceMod = this.priceModifiers[itemDef.id] || 1;
            var factionMult = faction.baseMult;

            // Reputation bonus
            var rep = S.state.reputation[factionId] || 0;
            var repBonus = 0;
            for (var i = S.REP_TIERS.length - 1; i >= 0; i--) {
                if (rep >= S.REP_TIERS[i].min) {
                    repBonus = S.REP_TIERS[i].bonus;
                    break;
                }
            }

            // Demand boost
            var demandBoost = this.demand[itemDef.id] || 0;

            // Supply penalty
            var supplySold = this.supply[itemDef.id] || 0;
            var supplyPenalty = 1 / (1 + supplySold * S.ECONOMY.supplyImpactFactor);

            // Event multiplier
            var eventMult = 1;
            if (this.currentEvent) {
                if (this.currentEvent.factionBoost && this.currentEvent.factionBoost[factionId]) {
                    eventMult *= this.currentEvent.factionBoost[factionId];
                }
                if (this.currentEvent.itemBoost && this.currentEvent.itemBoost.indexOf(itemDef.id) !== -1) {
                    eventMult *= this.currentEvent.itemMult;
                }
            }

            var price = itemDef.baseValue * rarityMult * priceMod * buyerInterest *
                        factionMult * (1 + repBonus) * (1 + demandBoost) *
                        supplyPenalty * eventMult;

            if (damaged) price *= 0.3;

            // Sell bonus from upgrades
            var bonus = Math.round(price * S.state.effects.sellBonus);
            price = Math.max(1, Math.round(price)) + bonus;

            return price;
        },

        sell: function(inventoryItem, factionId) {
            var itemDef = inventoryItem.def;
            var price = this.getPrice(itemDef, factionId, inventoryItem.damaged);
            if (price <= 0) return 0;

            // Record supply
            this.supply[itemDef.id] = (this.supply[itemDef.id] || 0) + 1;

            // Gain reputation
            var repGain = Math.ceil(price / 50);
            S.state.reputation[factionId] = Math.min(100,
                (S.state.reputation[factionId] || 0) + repGain);

            // Record price history
            if (!this.priceHistory[itemDef.id]) this.priceHistory[itemDef.id] = [];
            this.priceHistory[itemDef.id].push(price);
            if (this.priceHistory[itemDef.id].length > 10) {
                this.priceHistory[itemDef.id].shift();
            }

            return price;
        },

        advanceMarket: function() {
            var self = this;

            // Price fluctuation
            S.ITEMS.forEach(function(item) {
                var change = (Math.random() * 2 - 1) * S.ECONOMY.priceFluctuationRate;
                self.priceModifiers[item.id] = Phaser.Math.Clamp(
                    (self.priceModifiers[item.id] || 1) + change, 0.6, 1.5
                );
            });

            // Supply decay
            Object.keys(this.supply).forEach(function(id) {
                if (self.supply[id] > 0) self.supply[id]--;
            });

            // Demand decay and spikes
            Object.keys(this.demand).forEach(function(id) {
                self.demand[id] *= (1 - S.ECONOMY.demandDecayRate);
                if (Math.random() < 0.05) {
                    self.demand[id] += 0.2 + Math.random() * 0.3;
                }
            });

            // Event timer
            this.eventTimer--;
            if (this.eventTimer <= 0) {
                this.rollEvent();
            }
        },

        rollEvent: function() {
            var idx = Math.floor(Math.random() * S.MARKET_EVENTS.length);
            this.currentEvent = S.MARKET_EVENTS[idx];
            this.eventTimer = S.ECONOMY.eventDuration;
        },

        getPriceTrend: function(itemId) {
            var history = this.priceHistory[itemId];
            if (!history || history.length < 2) return 'stable';
            var last = history[history.length - 1];
            var prev = history[history.length - 2];
            var change = (last - prev) / prev;
            if (change > 0.1) return 'up';
            if (change < -0.1) return 'down';
            return 'stable';
        },

        getRepTier: function(factionId) {
            var rep = S.state.reputation[factionId] || 0;
            var tier = S.REP_TIERS[0];
            for (var i = S.REP_TIERS.length - 1; i >= 0; i--) {
                if (rep >= S.REP_TIERS[i].min) {
                    tier = S.REP_TIERS[i];
                    break;
                }
            }
            return tier;
        }
    };

    // ==================================================================
    // CONTRACT SYSTEM
    // ==================================================================
    S.Systems.Contracts = {
        available: [],
        active: [],
        nextId: 1,

        init: function() {
            this.available = [];
            this.active = [];
            this.nextId = 1;
            this.generate(Phaser.Math.Between(3, 5));
        },

        generate: function(count) {
            var factionIds = Object.keys(S.FACTIONS);
            for (var i = 0; i < count; i++) {
                var factionId = factionIds[Math.floor(Math.random() * factionIds.length)];
                var faction = S.FACTIONS[factionId];
                var stratumId = Phaser.Math.Between(1, 4);
                var items = S.ITEMS_BY_STRATUM[stratumId];
                if (!items || items.length === 0) continue;

                var itemDef = items[Math.floor(Math.random() * items.length)];

                // Quantity based on rarity
                var qty;
                switch (itemDef.rarity) {
                    case 'COMMON': qty = Phaser.Math.Between(3, 6); break;
                    case 'UNCOMMON': qty = Phaser.Math.Between(2, 4); break;
                    case 'RARE': qty = Phaser.Math.Between(1, 2); break;
                    case 'LEGENDARY': qty = 1; break;
                    default: qty = 2;
                }

                var reward = Math.round(itemDef.baseValue * qty * (1.5 + Math.random()));
                var repReward = 5 + Math.floor(Math.random() * 10);

                this.available.push({
                    id: 'contract_' + this.nextId++,
                    factionId: factionId,
                    factionName: faction.name,
                    factionColor: faction.color,
                    targetItemId: itemDef.id,
                    targetItemName: itemDef.name,
                    targetRarity: itemDef.rarity,
                    quantity: qty,
                    collected: 0,
                    reward: reward,
                    repReward: repReward,
                    expires: 5,
                    completed: false
                });
            }
        },

        accept: function(contractId) {
            var idx = -1;
            for (var i = 0; i < this.available.length; i++) {
                if (this.available[i].id === contractId) { idx = i; break; }
            }
            if (idx === -1) return false;

            var contract = this.available.splice(idx, 1)[0];
            this.active.push(contract);
            return true;
        },

        checkSale: function(itemDef, factionId) {
            for (var i = 0; i < this.active.length; i++) {
                var c = this.active[i];
                if (!c.completed && c.targetItemId === itemDef.id && c.factionId === factionId) {
                    c.collected++;
                    if (c.collected >= c.quantity) {
                        c.completed = true;
                    }
                }
            }
        },

        claim: function(contractId) {
            for (var i = 0; i < this.active.length; i++) {
                var c = this.active[i];
                if (c.id === contractId && c.completed) {
                    S.state.gold += c.reward;
                    S.state.reputation[c.factionId] = Math.min(100,
                        (S.state.reputation[c.factionId] || 0) + c.repReward);
                    this.active.splice(i, 1);
                    return c;
                }
            }
            return null;
        },

        advance: function() {
            // Decrement active contract timers
            for (var i = this.active.length - 1; i >= 0; i--) {
                if (!this.active[i].completed) {
                    this.active[i].expires--;
                    if (this.active[i].expires <= 0) {
                        this.active.splice(i, 1);
                    }
                }
            }

            // Remove expired available contracts and generate new ones
            for (i = this.available.length - 1; i >= 0; i--) {
                this.available[i].expires--;
                if (this.available[i].expires <= 0) {
                    this.available.splice(i, 1);
                }
            }

            // Ensure minimum contracts
            var total = this.available.length + this.active.length;
            if (total < 3) {
                this.generate(3 - total);
            }
        }
    };

})(window.STRATA);
