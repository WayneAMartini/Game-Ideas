// STRATA - Save/Load System
// ==========================
// localStorage-based persistence with versioning and auto-save
(function(S) {
    'use strict';

    var SAVE_KEY = 'strata_save';
    var SAVE_VERSION = 1;

    S.Save = {
        // ---- Serialize game state to a plain object ----
        serialize: function() {
            var econ = S.Systems.Economy;
            var contracts = S.Systems.Contracts;

            // Find currentEvent index in MARKET_EVENTS
            var eventIdx = 0;
            if (econ.currentEvent) {
                for (var i = 0; i < S.MARKET_EVENTS.length; i++) {
                    if (S.MARKET_EVENTS[i] === econ.currentEvent) {
                        eventIdx = i;
                        break;
                    }
                }
            }

            // Serialize inventory: store item IDs + damaged flag
            var inv = [];
            for (var j = 0; j < S.state.inventory.length; j++) {
                var item = S.state.inventory[j];
                inv.push({ id: item.def.id, damaged: item.damaged });
            }

            return {
                version: SAVE_VERSION,
                timestamp: Date.now(),
                state: {
                    gold: S.state.gold,
                    stamina: S.state.stamina,
                    maxStamina: S.state.maxStamina,
                    inventory: inv,
                    reputation: S.state.reputation,
                    purchasedUpgrades: S.state.purchasedUpgrades,
                    expeditionNumber: S.state.expedition.number
                },
                economy: {
                    priceModifiers: econ.priceModifiers,
                    supply: econ.supply,
                    demand: econ.demand,
                    currentEventIdx: eventIdx,
                    eventTimer: econ.eventTimer,
                    priceHistory: econ.priceHistory
                },
                contracts: {
                    available: contracts.available,
                    active: contracts.active,
                    nextId: contracts.nextId
                }
            };
        },

        // ---- Restore game state from a save object ----
        deserialize: function(data) {
            if (!data || !data.state) return false;

            // Migrate if needed
            data = this.migrate(data);

            var st = data.state;

            // Core state
            S.state.gold = st.gold;
            S.state.stamina = st.stamina;
            S.state.maxStamina = st.maxStamina;
            S.state.reputation = st.reputation;
            S.state.purchasedUpgrades = st.purchasedUpgrades;
            S.state.expedition.number = st.expeditionNumber;
            S.state.expedition.maxDepth = 0;
            S.state.expedition.itemsFound = 0;
            S.state.expedition.tilesDug = 0;
            S.state.expedition.startTime = 0;
            S.state.expedition.items = [];

            // Rebuild inventory from IDs
            S.state.inventory = [];
            if (st.inventory) {
                for (var i = 0; i < st.inventory.length; i++) {
                    var saved = st.inventory[i];
                    var def = S.ITEM_MAP[saved.id];
                    if (def) {
                        S.state.inventory.push({ def: def, damaged: !!saved.damaged });
                    }
                }
            }

            // Recalculate derived effects from upgrades
            S.recalcEffects();

            // Economy state
            var econ = S.Systems.Economy;
            if (data.economy) {
                var ec = data.economy;
                econ.priceModifiers = ec.priceModifiers || {};
                econ.supply = ec.supply || {};
                econ.demand = ec.demand || {};
                econ.eventTimer = ec.eventTimer || 0;
                econ.priceHistory = ec.priceHistory || {};

                // Restore current event from index
                var idx = ec.currentEventIdx || 0;
                if (idx >= 0 && idx < S.MARKET_EVENTS.length) {
                    econ.currentEvent = S.MARKET_EVENTS[idx];
                } else {
                    econ.currentEvent = null;
                }

                // Ensure all items have entries (handles new items added in updates)
                S.ITEMS.forEach(function(item) {
                    if (econ.priceModifiers[item.id] === undefined) {
                        econ.priceModifiers[item.id] = 0.9 + Math.random() * 0.2;
                    }
                    if (econ.supply[item.id] === undefined) econ.supply[item.id] = 0;
                    if (econ.demand[item.id] === undefined) econ.demand[item.id] = 0;
                    if (!econ.priceHistory[item.id]) econ.priceHistory[item.id] = [];
                });
            } else {
                econ.init();
            }

            // Contract state
            var contracts = S.Systems.Contracts;
            if (data.contracts) {
                contracts.available = data.contracts.available || [];
                contracts.active = data.contracts.active || [];
                contracts.nextId = data.contracts.nextId || 1;
            } else {
                contracts.init();
            }

            return true;
        },

        // ---- Save to localStorage ----
        save: function() {
            try {
                var data = this.serialize();
                localStorage.setItem(SAVE_KEY, JSON.stringify(data));
                return true;
            } catch (e) {
                console.warn('STRATA Save: failed to write', e);
                return false;
            }
        },

        // ---- Load from localStorage ----
        load: function() {
            try {
                var raw = localStorage.getItem(SAVE_KEY);
                if (!raw) return false;
                var data = JSON.parse(raw);
                if (!data || typeof data !== 'object') return false;
                return this.deserialize(data);
            } catch (e) {
                console.warn('STRATA Save: failed to read/parse', e);
                return false;
            }
        },

        // ---- Check if a save exists ----
        hasSave: function() {
            try {
                var raw = localStorage.getItem(SAVE_KEY);
                if (!raw) return false;
                var data = JSON.parse(raw);
                return !!(data && data.state);
            } catch (e) {
                return false;
            }
        },

        // ---- Get save metadata without full load ----
        getSaveInfo: function() {
            try {
                var raw = localStorage.getItem(SAVE_KEY);
                if (!raw) return null;
                var data = JSON.parse(raw);
                if (!data || !data.state) return null;
                return {
                    timestamp: data.timestamp,
                    gold: data.state.gold,
                    expeditionNumber: data.state.expeditionNumber,
                    upgradeCount: data.state.purchasedUpgrades ? data.state.purchasedUpgrades.length : 0,
                    version: data.version
                };
            } catch (e) {
                return null;
            }
        },

        // ---- Delete save ----
        deleteSave: function() {
            try {
                localStorage.removeItem(SAVE_KEY);
            } catch (e) {
                console.warn('STRATA Save: failed to delete', e);
            }
        },

        // ---- Version migration ----
        migrate: function(data) {
            // Currently version 1 — no migrations needed yet.
            // Future versions add cases here:
            // if (data.version < 2) { ... transform to v2 ... data.version = 2; }
            return data;
        },

        // ---- Format timestamp for display ----
        formatTime: function(ts) {
            if (!ts) return '';
            var d = new Date(ts);
            var h = d.getHours();
            var m = d.getMinutes();
            var ampm = h >= 12 ? 'PM' : 'AM';
            h = h % 12 || 12;
            return h + ':' + (m < 10 ? '0' : '') + m + ' ' + ampm;
        }
    };

})(window.STRATA);
