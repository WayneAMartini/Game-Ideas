import { ITEM_DEFS, getItemsForStratum } from './items.js';
import { FACTIONS } from './economy.js';

export class ContractManager {
    constructor() {
        this.contracts = [];
        this.completedContracts = 0;
        this.generateContracts();
    }

    generateContracts() {
        this.contracts = [];
        // Generate 3-5 contracts
        const count = 3 + Math.floor(Math.random() * 3);
        for (let i = 0; i < count; i++) {
            this.contracts.push(this.createContract());
        }
    }

    createContract() {
        const factionIds = Object.keys(FACTIONS);
        const factionId = factionIds[Math.floor(Math.random() * factionIds.length)];
        const faction = FACTIONS[factionId];

        // Pick a random item from strata 1-4
        const stratum = 1 + Math.floor(Math.random() * 4);
        const stratumItems = getItemsForStratum(stratum);
        const targetItem = stratumItems[Math.floor(Math.random() * stratumItems.length)];

        // Quantity scales with item rarity
        const rarityQuantities = { COMMON: [3, 6], UNCOMMON: [2, 4], RARE: [1, 2], LEGENDARY: [1, 1] };
        const range = rarityQuantities[targetItem.rarity] || [1, 3];
        const quantity = range[0] + Math.floor(Math.random() * (range[1] - range[0] + 1));

        // Reward: base value * quantity * bonus multiplier
        const reward = Math.round(targetItem.baseValue * quantity * (1.5 + Math.random()));
        const repReward = 5 + Math.floor(Math.random() * 10);

        return {
            id: `contract_${Date.now()}_${Math.random().toString(36).substr(2, 5)}`,
            factionId,
            factionName: faction.name,
            factionColor: faction.color,
            targetItemId: targetItem.id,
            targetItemName: targetItem.name,
            quantity,
            collected: 0,
            reward,
            repReward,
            description: `${faction.name} wants ${quantity}x ${targetItem.name}`,
            active: false,
            completed: false,
            expires: 5 // Expires in 5 expeditions
        };
    }

    acceptContract(contractId) {
        const contract = this.contracts.find(c => c.id === contractId);
        if (contract && !contract.active && !contract.completed) {
            contract.active = true;
            return true;
        }
        return false;
    }

    // Check if selling an item contributes to any active contract
    checkSale(itemDef, factionId) {
        const contributions = [];
        for (const contract of this.contracts) {
            if (contract.active && !contract.completed &&
                contract.targetItemId === itemDef.id &&
                contract.factionId === factionId) {
                contract.collected++;
                if (contract.collected >= contract.quantity) {
                    contract.completed = true;
                    this.completedContracts++;
                    contributions.push({ contract, completed: true });
                } else {
                    contributions.push({ contract, completed: false });
                }
            }
        }
        return contributions;
    }

    // Get reward for completed contracts
    claimRewards() {
        let totalGold = 0;
        let repGains = {};

        for (const contract of this.contracts) {
            if (contract.completed && contract.active) {
                totalGold += contract.reward;
                repGains[contract.factionId] = (repGains[contract.factionId] || 0) + contract.repReward;
                contract.claimed = true;
            }
        }

        // Remove claimed contracts
        this.contracts = this.contracts.filter(c => !c.claimed);

        return { totalGold, repGains };
    }

    // Called between expeditions
    advanceContracts() {
        // Decrement expiry on active contracts
        for (const contract of this.contracts) {
            if (contract.active && !contract.completed) {
                contract.expires--;
                if (contract.expires <= 0) {
                    contract.expired = true;
                }
            }
        }

        // Remove expired contracts
        this.contracts = this.contracts.filter(c => !c.expired);

        // Generate new contracts to fill gaps
        while (this.contracts.length < 3) {
            this.contracts.push(this.createContract());
        }
    }

    getActiveContracts() {
        return this.contracts.filter(c => c.active && !c.completed);
    }

    getAvailableContracts() {
        return this.contracts.filter(c => !c.active && !c.completed);
    }
}
