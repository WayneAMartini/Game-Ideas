// Upgrade tree definitions and state management
// Implementing 3 trees: Excavation, Logistics, Commerce (as specified in plan)

export const UPGRADE_TREES = {
    excavation: {
        name: 'Excavation',
        description: 'How you dig',
        color: '#FF6B35',
        tiers: [
            {
                tier: 1,
                name: 'Basic Pickaxe',
                cost: 0,
                effect: { digSpeed: 1.0 },
                description: 'Standard mining pickaxe',
                auto: true
            },
            {
                tier: 2,
                choices: [
                    {
                        id: 'pneumatic_hammer',
                        name: 'Pneumatic Hammer',
                        branch: 'Power',
                        cost: 150,
                        effect: { digSpeed: 2.0, itemDamageChance: 0.2 },
                        description: '2x dig speed, but 20% chance to damage items'
                    },
                    {
                        id: 'archaeologist_kit',
                        name: "Archaeologist's Kit",
                        branch: 'Precision',
                        cost: 200,
                        effect: { digSpeed: 0.8, itemQualityBonus: 1.5 },
                        description: 'Slower dig, but items worth 50% more'
                    },
                    {
                        id: 'blast_charges',
                        name: 'Blast Charges',
                        branch: 'Range',
                        cost: 175,
                        effect: { digSpeed: 1.5, aoeRadius: 1 },
                        description: 'Clear 3x3 areas, destroys fragile items'
                    }
                ]
            },
            {
                tier: 3,
                choices: [
                    {
                        id: 'thermal_lance',
                        name: 'Thermal Lance',
                        branch: 'Power',
                        requires: 'pneumatic_hammer',
                        cost: 500,
                        effect: { digSpeed: 3.0, canDigHardRock: true },
                        description: 'Cuts through any material instantly'
                    },
                    {
                        id: 'laser_scalpel',
                        name: 'Laser Scalpel',
                        branch: 'Precision',
                        requires: 'archaeologist_kit',
                        cost: 600,
                        effect: { digSpeed: 1.0, itemQualityBonus: 2.0, neverDamage: true },
                        description: 'Surgical extraction, never damages items'
                    },
                    {
                        id: 'tunnel_bore',
                        name: 'Tunnel Bore',
                        branch: 'Range',
                        requires: 'blast_charges',
                        cost: 550,
                        effect: { digSpeed: 2.5, aoeRadius: 2 },
                        description: 'Drills wide tunnels automatically'
                    }
                ]
            },
            {
                tier: 4,
                choices: [
                    {
                        id: 'worldbreaker',
                        name: 'The Worldbreaker',
                        branch: 'Power',
                        requires: 'thermal_lance',
                        cost: 2000,
                        effect: { digSpeed: 5.0, aoeRadius: 3 },
                        description: 'Destroys entire layers — massive yield, no quality control'
                    },
                    {
                        id: 'quantum_extractor',
                        name: 'Quantum Extractor',
                        branch: 'Precision',
                        requires: 'laser_scalpel',
                        cost: 2500,
                        effect: { digSpeed: 2.0, itemQualityBonus: 3.0, neverDamage: true },
                        description: 'Phases items out of rock perfectly'
                    },
                    {
                        id: 'the_colony',
                        name: 'The Colony',
                        branch: 'Range',
                        requires: 'tunnel_bore',
                        cost: 2200,
                        effect: { digSpeed: 3.0, autoDig: true },
                        description: 'Autonomous mini-drills dig while you explore'
                    }
                ]
            }
        ]
    },

    logistics: {
        name: 'Logistics',
        description: 'How you carry and move',
        color: '#4FC3F7',
        tiers: [
            {
                tier: 1,
                name: 'Canvas Backpack',
                cost: 0,
                effect: { inventorySlots: 10 },
                description: '10 inventory slots',
                auto: true
            },
            {
                tier: 2,
                choices: [
                    {
                        id: 'reinforced_pack',
                        name: 'Reinforced Pack',
                        branch: 'Capacity',
                        cost: 120,
                        effect: { inventorySlots: 18 },
                        description: '18 slots, can carry heavy items'
                    },
                    {
                        id: 'grapple_line',
                        name: 'Grapple Line',
                        branch: 'Speed',
                        cost: 150,
                        effect: { inventorySlots: 12, moveSpeed: 6 },
                        description: 'Faster return to surface, 12 slots'
                    },
                    {
                        id: 'supply_drone',
                        name: 'Supply Drone',
                        branch: 'Automation',
                        cost: 200,
                        effect: { inventorySlots: 14, autoSend: true },
                        description: 'Auto-sends items to surface every 5 min'
                    }
                ]
            },
            {
                tier: 3,
                choices: [
                    {
                        id: 'cargo_mech',
                        name: 'Cargo Mech',
                        branch: 'Capacity',
                        requires: 'reinforced_pack',
                        cost: 450,
                        effect: { inventorySlots: 30, moveSpeed: 3 },
                        description: '30 slots but slower movement'
                    },
                    {
                        id: 'pneumatic_tubes',
                        name: 'Pneumatic Tubes',
                        branch: 'Speed',
                        requires: 'grapple_line',
                        cost: 500,
                        effect: { inventorySlots: 14, instantReturn: true },
                        description: 'Items shoot to surface instantly'
                    },
                    {
                        id: 'conveyor_system',
                        name: 'Conveyor System',
                        branch: 'Automation',
                        requires: 'supply_drone',
                        cost: 550,
                        effect: { inventorySlots: 20, autoSend: true, autoInterval: 120 },
                        description: 'Permanent conveyor belts in tunnels'
                    }
                ]
            },
            {
                tier: 4,
                choices: [
                    {
                        id: 'pocket_dimension',
                        name: 'Pocket Dimension',
                        branch: 'Capacity',
                        requires: 'cargo_mech',
                        cost: 1800,
                        effect: { inventorySlots: 50 },
                        description: '50 slots — carry a museum\'s worth of loot'
                    },
                    {
                        id: 'teleport_pad',
                        name: 'Teleport Pad',
                        branch: 'Speed',
                        requires: 'pneumatic_tubes',
                        cost: 2000,
                        effect: { inventorySlots: 20, teleport: true },
                        description: 'Teleport between surface and depth at will'
                    },
                    {
                        id: 'logistics_ai',
                        name: 'The Logistics AI',
                        branch: 'Automation',
                        requires: 'conveyor_system',
                        cost: 2200,
                        effect: { inventorySlots: 30, autoSend: true, autoSort: true },
                        description: 'Fully automated sorting and delivery'
                    }
                ]
            }
        ]
    },

    commerce: {
        name: 'Commerce',
        description: 'How you sell',
        color: '#FFD700',
        tiers: [
            {
                tier: 1,
                name: 'Market Stall',
                cost: 0,
                effect: { sellBonus: 0 },
                description: 'Basic market access',
                auto: true
            },
            {
                tier: 2,
                choices: [
                    {
                        id: 'jewelers_loupe',
                        name: "Jeweler's Loupe",
                        branch: 'Appraisal',
                        cost: 130,
                        effect: { sellBonus: 0.1, revealItems: true },
                        description: '10% better prices, see item hints in terrain'
                    },
                    {
                        id: 'silver_tongue',
                        name: 'Silver Tongue',
                        branch: 'Negotiation',
                        cost: 160,
                        effect: { sellBonus: 0.15 },
                        description: '15% price bonus on all sales'
                    },
                    {
                        id: 'trade_contacts',
                        name: 'Trade Contacts',
                        branch: 'Network',
                        cost: 180,
                        effect: { sellBonus: 0.05, extraBuyers: true },
                        description: 'Unlock traveling merchants with special offers'
                    }
                ]
            },
            {
                tier: 3,
                choices: [
                    {
                        id: 'master_appraiser',
                        name: 'Master Appraiser',
                        branch: 'Appraisal',
                        requires: 'jewelers_loupe',
                        cost: 500,
                        effect: { sellBonus: 0.25, instantAppraisal: true },
                        description: 'Items auto-appraised, +25% value'
                    },
                    {
                        id: 'market_manipulation',
                        name: 'Market Manipulation',
                        branch: 'Negotiation',
                        requires: 'silver_tongue',
                        cost: 600,
                        effect: { sellBonus: 0.3, canManipulate: true },
                        description: '30% bonus + artificially inflate prices'
                    },
                    {
                        id: 'auction_house',
                        name: 'Auction House',
                        branch: 'Network',
                        requires: 'trade_contacts',
                        cost: 650,
                        effect: { sellBonus: 0.15, auction: true },
                        description: 'Auction items for chance of bidding wars (up to 3x)'
                    }
                ]
            },
            {
                tier: 4,
                choices: [
                    {
                        id: 'the_connoisseur',
                        name: 'The Connoisseur',
                        branch: 'Appraisal',
                        requires: 'master_appraiser',
                        cost: 2000,
                        effect: { sellBonus: 0.4, showBestItem: true },
                        description: 'Auto-identifies the most valuable item per stratum'
                    },
                    {
                        id: 'monopoly',
                        name: 'Monopoly',
                        branch: 'Negotiation',
                        requires: 'market_manipulation',
                        cost: 2500,
                        effect: { sellBonus: 0.5 },
                        description: 'Exclusive contracts — factions pay 2x'
                    },
                    {
                        id: 'trade_empire',
                        name: 'The Trade Empire',
                        branch: 'Network',
                        requires: 'auction_house',
                        cost: 2200,
                        effect: { sellBonus: 0.3, passiveIncome: true },
                        description: 'Passive income from merchant network'
                    }
                ]
            }
        ]
    }
};

export class UpgradeManager {
    constructor() {
        // Track purchased upgrades
        this.purchased = new Set();
        // Active effects (aggregated from purchased upgrades)
        this.effects = {
            digSpeed: 1.0,
            inventorySlots: 10,
            moveSpeed: 4,
            sellBonus: 0,
            itemQualityBonus: 1.0,
            neverDamage: false,
            aoeRadius: 0,
            headlampRadius: 6
        };
    }

    canPurchase(upgradeId, gold) {
        if (this.purchased.has(upgradeId)) return false;

        // Find the upgrade definition
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const choice of tier.choices) {
                        if (choice.id === upgradeId) {
                            // Check requirements
                            if (choice.requires && !this.purchased.has(choice.requires)) return false;
                            // Check we haven't bought another choice from same tier
                            const sametierchoices = tier.choices.filter(c => c.id !== upgradeId);
                            for (const other of sametierchoices) {
                                if (this.purchased.has(other.id)) return false;
                            }
                            // Check gold
                            return gold >= choice.cost;
                        }
                    }
                }
            }
        }
        return false;
    }

    purchase(upgradeId) {
        this.purchased.add(upgradeId);
        this.recalcEffects();
        return this.getCost(upgradeId);
    }

    getCost(upgradeId) {
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const choice of tier.choices) {
                        if (choice.id === upgradeId) return choice.cost;
                    }
                }
            }
        }
        return 0;
    }

    recalcEffects() {
        // Reset to defaults
        this.effects = {
            digSpeed: 1.0,
            inventorySlots: 10,
            moveSpeed: 4,
            sellBonus: 0,
            itemQualityBonus: 1.0,
            neverDamage: false,
            aoeRadius: 0,
            headlampRadius: 6
        };

        // Apply all purchased upgrade effects
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const choice of tier.choices) {
                        if (this.purchased.has(choice.id)) {
                            this.applyEffect(choice.effect);
                        }
                    }
                }
            }
        }
    }

    applyEffect(effect) {
        if (effect.digSpeed !== undefined) this.effects.digSpeed = effect.digSpeed;
        if (effect.inventorySlots !== undefined) this.effects.inventorySlots = effect.inventorySlots;
        if (effect.moveSpeed !== undefined) this.effects.moveSpeed = effect.moveSpeed;
        if (effect.sellBonus !== undefined) this.effects.sellBonus = Math.max(this.effects.sellBonus, effect.sellBonus);
        if (effect.itemQualityBonus !== undefined) this.effects.itemQualityBonus = effect.itemQualityBonus;
        if (effect.neverDamage) this.effects.neverDamage = true;
        if (effect.aoeRadius !== undefined) this.effects.aoeRadius = Math.max(this.effects.aoeRadius, effect.aoeRadius);
    }

    // Apply effects to player
    applyToPlayer(player) {
        player.digPower = this.effects.digSpeed;
        player.inventorySlots = this.effects.inventorySlots;
        player.moveSpeed = this.effects.moveSpeed;
    }

    // Get current tier for a tree
    getCurrentTier(treeId) {
        const tree = UPGRADE_TREES[treeId];
        if (!tree) return 1;
        let maxTier = 1;
        for (const tier of tree.tiers) {
            if (tier.choices) {
                for (const choice of tier.choices) {
                    if (this.purchased.has(choice.id)) {
                        maxTier = Math.max(maxTier, tier.tier);
                    }
                }
            }
        }
        return maxTier;
    }

    // Get available upgrades for a tree
    getAvailableUpgrades(treeId, gold) {
        const tree = UPGRADE_TREES[treeId];
        if (!tree) return [];

        const available = [];
        for (const tier of tree.tiers) {
            if (tier.choices) {
                for (const choice of tier.choices) {
                    if (!this.purchased.has(choice.id)) {
                        const canBuy = this.canPurchase(choice.id, gold);
                        const meetsReqs = !choice.requires || this.purchased.has(choice.requires);
                        // Check no other choice in same tier is bought
                        const tierBlocked = tier.choices.some(c => c.id !== choice.id && this.purchased.has(c.id));
                        if (meetsReqs && !tierBlocked) {
                            available.push({ ...choice, canAfford: gold >= choice.cost, treeId });
                        }
                    }
                }
            }
        }
        return available;
    }
}
