import { CANVAS_WIDTH, CANVAS_HEIGHT, RARITY, GAME_STATES } from './config.js';
import { FACTIONS } from './economy.js';
import { UPGRADE_TREES } from './upgrades.js';
import { ITEMS_BY_ID } from './items.js';
import { getMouse, isKeyJustPressed } from './input.js';

export class UI {
    constructor(ctx) {
        this.ctx = ctx;
        this.scrollOffset = 0;
        this.maxScroll = 0;
        this.selectedFaction = 'curator';
        this.selectedTree = 'excavation';
        this.hoveredItem = null;
        this.hoveredUpgrade = null;
        this.sellAnimations = [];
        this.tooltipText = null;
    }

    // === MARKET SCREEN ===
    drawMarket(economy, player, contracts, particles) {
        const ctx = this.ctx;
        const mouse = getMouse();

        // Background
        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Title
        this.drawPanelHeader('HOLLOWMARKET', 0, 0, CANVAS_WIDTH);

        // Market event banner
        if (economy.activeEvent) {
            ctx.fillStyle = '#2a1a0a';
            ctx.fillRect(10, 40, CANVAS_WIDTH - 20, 30);
            ctx.strokeStyle = '#FFD700';
            ctx.lineWidth = 1;
            ctx.strokeRect(10, 40, CANVAS_WIDTH - 20, 30);
            ctx.fillStyle = '#FFD700';
            ctx.font = 'bold 12px monospace';
            ctx.fillText(`EVENT: ${economy.activeEvent.name} — ${economy.activeEvent.description}`, 20, 60);
        }

        // Faction tabs
        const tabY = economy.activeEvent ? 80 : 45;
        const factionIds = Object.keys(FACTIONS);
        const tabW = (CANVAS_WIDTH - 20) / factionIds.length;

        factionIds.forEach((fid, i) => {
            const faction = FACTIONS[fid];
            const tx = 10 + i * tabW;
            const selected = this.selectedFaction === fid;

            ctx.fillStyle = selected ? faction.color : '#333';
            ctx.fillRect(tx, tabY, tabW - 4, 30);
            ctx.fillStyle = selected ? '#000' : '#AAA';
            ctx.font = 'bold 11px monospace';
            ctx.fillText(faction.name, tx + 8, tabY + 19);

            // Reputation
            const rep = economy.getRepTier(fid);
            ctx.fillStyle = rep.color;
            ctx.font = '9px monospace';
            ctx.fillText(rep.name, tx + tabW - 60, tabY + 19);

            // Click detection
            if (mouse.justClicked && mouse.x >= tx && mouse.x < tx + tabW - 4 &&
                mouse.y >= tabY && mouse.y < tabY + 30) {
                this.selectedFaction = fid;
            }
        });

        // Inventory items for sale
        const listY = tabY + 40;
        ctx.fillStyle = '#222';
        ctx.fillRect(10, listY, CANVAS_WIDTH - 20, CANVAS_HEIGHT - listY - 50);

        if (player.inventory.length === 0) {
            ctx.fillStyle = '#666';
            ctx.font = '14px monospace';
            ctx.fillText('Your bag is empty. Go dig something up!', 30, listY + 40);
        } else {
            ctx.fillStyle = '#AAA';
            ctx.font = 'bold 11px monospace';
            ctx.fillText('ITEM', 25, listY + 16);
            ctx.fillText('RARITY', 250, listY + 16);
            ctx.fillText('PRICE', 380, listY + 16);
            ctx.fillText('', 500, listY + 16);

            let clickedSell = -1;

            for (let i = 0; i < player.inventory.length; i++) {
                const item = player.inventory[i];
                const iy = listY + 25 + i * 32;
                if (iy > CANVAS_HEIGHT - 60) break;

                const price = economy.getPrice(item, this.selectedFaction);
                const rarity = RARITY[item.rarity];
                const hovered = mouse.x >= 15 && mouse.x < CANVAS_WIDTH - 15 &&
                               mouse.y >= iy && mouse.y < iy + 30;

                // Row bg
                ctx.fillStyle = hovered ? '#333' : (i % 2 === 0 ? '#252525' : '#222');
                ctx.fillRect(15, iy, CANVAS_WIDTH - 30, 30);

                // Item name
                ctx.fillStyle = rarity.color;
                ctx.font = '12px monospace';
                ctx.fillText(item.name, 25, iy + 19);

                // Rarity
                ctx.fillStyle = rarity.color;
                ctx.fillText(rarity.name, 250, iy + 19);

                // Price
                if (price > 0) {
                    ctx.fillStyle = '#FFD700';
                    ctx.fillText(`${price} G`, 380, iy + 19);

                    // Sell button
                    const btnX = 500, btnW = 70, btnH = 22;
                    const btnHovered = mouse.x >= btnX && mouse.x < btnX + btnW &&
                                      mouse.y >= iy + 4 && mouse.y < iy + 4 + btnH;
                    ctx.fillStyle = btnHovered ? '#4CAF50' : '#2E7D32';
                    ctx.fillRect(btnX, iy + 4, btnW, btnH);
                    ctx.fillStyle = '#FFF';
                    ctx.font = 'bold 11px monospace';
                    ctx.fillText('SELL', btnX + 18, iy + 19);

                    if (btnHovered && mouse.justClicked) {
                        clickedSell = i;
                    }
                } else {
                    ctx.fillStyle = '#666';
                    ctx.font = '12px monospace';
                    ctx.fillText('Not interested', 380, iy + 19);
                }
            }

            // Process sell
            if (clickedSell >= 0) {
                return { action: 'sell', index: clickedSell, factionId: this.selectedFaction };
            }
        }

        // Sell All button
        if (player.inventory.length > 0) {
            const saX = CANVAS_WIDTH - 160, saY = CANVAS_HEIGHT - 45, saW = 140, saH = 30;
            const saHover = mouse.x >= saX && mouse.x < saX + saW && mouse.y >= saY && mouse.y < saY + saH;
            ctx.fillStyle = saHover ? '#D32F2F' : '#B71C1C';
            ctx.fillRect(saX, saY, saW, saH);
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('SELL ALL', saX + 30, saY + 20);

            if (saHover && mouse.justClicked) {
                return { action: 'sell_all', factionId: this.selectedFaction };
            }
        }

        // Gold display
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 16px monospace';
        ctx.fillText(`Gold: ${player.gold}`, 20, CANVAS_HEIGHT - 20);

        // Back button
        return this.drawBackButton(mouse);
    }

    // === UPGRADE SCREEN ===
    drawUpgrades(upgradeManager, player) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('UPGRADES', 0, 0, CANVAS_WIDTH);

        // Tree tabs
        const treeIds = Object.keys(UPGRADE_TREES);
        const tabW = (CANVAS_WIDTH - 20) / treeIds.length;

        treeIds.forEach((tid, i) => {
            const tree = UPGRADE_TREES[tid];
            const tx = 10 + i * tabW;
            const selected = this.selectedTree === tid;

            ctx.fillStyle = selected ? tree.color : '#333';
            ctx.fillRect(tx, 45, tabW - 4, 30);
            ctx.fillStyle = selected ? '#000' : '#AAA';
            ctx.font = 'bold 11px monospace';
            ctx.fillText(`${tree.name} (T${upgradeManager.getCurrentTier(tid)})`, tx + 8, 65);

            if (mouse.justClicked && mouse.x >= tx && mouse.x < tx + tabW - 4 &&
                mouse.y >= 45 && mouse.y < 75) {
                this.selectedTree = tid;
            }
        });

        // Draw selected tree
        const tree = UPGRADE_TREES[this.selectedTree];
        const startY = 90;
        let y = startY;

        for (const tier of tree.tiers) {
            // Tier header
            ctx.fillStyle = '#444';
            ctx.fillRect(10, y, CANVAS_WIDTH - 20, 24);
            ctx.fillStyle = tree.color;
            ctx.font = 'bold 12px monospace';

            if (tier.auto) {
                ctx.fillText(`Tier ${tier.tier}: ${tier.name} (Base)`, 20, y + 16);
                y += 30;
                continue;
            }

            ctx.fillText(`Tier ${tier.tier}`, 20, y + 16);
            y += 30;

            if (tier.choices) {
                for (const choice of tier.choices) {
                    const owned = upgradeManager.purchased.has(choice.id);
                    const canBuy = upgradeManager.canPurchase(choice.id, player.gold);
                    const meetsReqs = !choice.requires || upgradeManager.purchased.has(choice.requires);
                    const tierBlocked = tier.choices.some(c => c.id !== choice.id && upgradeManager.purchased.has(c.id));

                    const cardH = 60;
                    const hovered = mouse.x >= 20 && mouse.x < CANVAS_WIDTH - 20 &&
                                   mouse.y >= y && mouse.y < y + cardH;

                    // Card background
                    if (owned) {
                        ctx.fillStyle = '#1a3a1a';
                    } else if (tierBlocked || !meetsReqs) {
                        ctx.fillStyle = '#1a1a1a';
                    } else if (hovered) {
                        ctx.fillStyle = '#2a2a3a';
                    } else {
                        ctx.fillStyle = '#222';
                    }
                    ctx.fillRect(20, y, CANVAS_WIDTH - 40, cardH);

                    // Border
                    ctx.strokeStyle = owned ? '#4CAF50' : (canBuy ? tree.color : '#444');
                    ctx.lineWidth = owned ? 2 : 1;
                    ctx.strokeRect(20, y, CANVAS_WIDTH - 40, cardH);

                    // Name + branch
                    ctx.fillStyle = owned ? '#4CAF50' : (meetsReqs && !tierBlocked ? '#FFF' : '#666');
                    ctx.font = 'bold 12px monospace';
                    ctx.fillText(`[${choice.branch}] ${choice.name}`, 30, y + 18);

                    // Description
                    ctx.fillStyle = owned ? '#6a6' : '#AAA';
                    ctx.font = '10px monospace';
                    ctx.fillText(choice.description, 30, y + 34);

                    // Cost / Status
                    if (owned) {
                        ctx.fillStyle = '#4CAF50';
                        ctx.font = 'bold 11px monospace';
                        ctx.fillText('OWNED', CANVAS_WIDTH - 100, y + 18);
                    } else if (meetsReqs && !tierBlocked) {
                        ctx.fillStyle = canBuy ? '#FFD700' : '#888';
                        ctx.font = 'bold 11px monospace';
                        ctx.fillText(`${choice.cost} G`, CANVAS_WIDTH - 110, y + 18);

                        if (canBuy) {
                            // Buy button
                            const bx = CANVAS_WIDTH - 110, by = y + 30, bw = 65, bh = 22;
                            const bHover = mouse.x >= bx && mouse.x < bx + bw &&
                                          mouse.y >= by && mouse.y < by + bh;
                            ctx.fillStyle = bHover ? '#FF9800' : '#E65100';
                            ctx.fillRect(bx, by, bw, bh);
                            ctx.fillStyle = '#FFF';
                            ctx.font = 'bold 10px monospace';
                            ctx.fillText('BUY', bx + 20, by + 15);

                            if (bHover && mouse.justClicked) {
                                return { action: 'buy', upgradeId: choice.id };
                            }
                        }
                    } else {
                        ctx.fillStyle = '#555';
                        ctx.font = '10px monospace';
                        if (tierBlocked) {
                            ctx.fillText('(Branch chosen)', CANVAS_WIDTH - 140, y + 18);
                        } else if (choice.requires) {
                            const reqName = this.findUpgradeName(choice.requires);
                            ctx.fillText(`Requires: ${reqName}`, CANVAS_WIDTH - 200, y + 18);
                        }
                    }

                    y += cardH + 5;
                }
            }
            y += 10;
        }

        // Gold display
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 16px monospace';
        ctx.fillText(`Gold: ${player.gold}`, 20, CANVAS_HEIGHT - 20);

        return this.drawBackButton(mouse);
    }

    findUpgradeName(upgradeId) {
        for (const tree of Object.values(UPGRADE_TREES)) {
            for (const tier of tree.tiers) {
                if (tier.choices) {
                    for (const c of tier.choices) {
                        if (c.id === upgradeId) return c.name;
                    }
                }
            }
        }
        return upgradeId;
    }

    // === CONTRACT SCREEN ===
    drawContracts(contractManager, economy) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('BOUNTY BOARD', 0, 0, CANVAS_WIDTH);

        // Active contracts
        let y = 50;
        const active = contractManager.getActiveContracts();
        if (active.length > 0) {
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('ACTIVE CONTRACTS', 20, y);
            y += 20;

            for (const contract of active) {
                this.drawContractCard(contract, 20, y, true);
                y += 70;
            }
        }

        y += 10;
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 12px monospace';
        ctx.fillText('AVAILABLE CONTRACTS', 20, y);
        y += 20;

        const available = contractManager.getAvailableContracts();
        for (const contract of available) {
            const result = this.drawContractCard(contract, 20, y, false, mouse);
            if (result) return result;
            y += 70;
        }

        // Claim rewards button
        const completed = contractManager.contracts.filter(c => c.completed && c.active && !c.claimed);
        if (completed.length > 0) {
            const totalReward = completed.reduce((sum, c) => sum + c.reward, 0);
            const bx = CANVAS_WIDTH / 2 - 100, by = CANVAS_HEIGHT - 60, bw = 200, bh = 35;
            const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;

            ctx.fillStyle = bHover ? '#FFD700' : '#E8B04A';
            ctx.fillRect(bx, by, bw, bh);
            ctx.fillStyle = '#000';
            ctx.font = 'bold 14px monospace';
            ctx.fillText(`CLAIM ${totalReward} G`, bx + 40, by + 23);

            if (bHover && mouse.justClicked) {
                return { action: 'claim' };
            }
        }

        return this.drawBackButton(mouse);
    }

    drawContractCard(contract, x, y, isActive, mouse) {
        const ctx = this.ctx;
        const w = CANVAS_WIDTH - 40;

        ctx.fillStyle = contract.completed ? '#1a3a1a' : '#222';
        ctx.fillRect(x, y, w, 62);
        ctx.strokeStyle = contract.factionColor || '#555';
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, w, 62);

        // Faction + description
        ctx.fillStyle = contract.factionColor || '#FFF';
        ctx.font = 'bold 11px monospace';
        ctx.fillText(contract.factionName, x + 10, y + 16);

        ctx.fillStyle = '#CCC';
        ctx.font = '11px monospace';
        ctx.fillText(contract.description, x + 10, y + 34);

        // Progress
        ctx.fillStyle = '#AAA';
        ctx.fillText(`Progress: ${contract.collected}/${contract.quantity}`, x + 10, y + 50);

        // Reward
        ctx.fillStyle = '#FFD700';
        ctx.fillText(`Reward: ${contract.reward} G  +${contract.repReward} Rep`, x + 300, y + 50);

        if (contract.completed) {
            ctx.fillStyle = '#4CAF50';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('COMPLETE!', x + w - 100, y + 16);
        } else if (isActive) {
            ctx.fillStyle = '#888';
            ctx.font = '10px monospace';
            ctx.fillText(`Expires in ${contract.expires} expeditions`, x + w - 200, y + 16);
        }

        // Accept button for available contracts
        if (!isActive && mouse) {
            const bx = x + w - 90, by2 = y + 10, bw = 80, bh = 25;
            const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by2 && mouse.y < by2 + bh;
            ctx.fillStyle = bHover ? '#4FC3F7' : '#1565C0';
            ctx.fillRect(bx, by2, bw, bh);
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 10px monospace';
            ctx.fillText('ACCEPT', bx + 14, by2 + 17);

            if (bHover && mouse.justClicked) {
                return { action: 'accept', contractId: contract.id };
            }
        }

        return null;
    }

    // === INVENTORY SCREEN ===
    drawInventory(player) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = 'rgba(0, 0, 0, 0.85)';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader(`INVENTORY (${player.inventory.length}/${player.inventorySlots})`, 0, 0, CANVAS_WIDTH);

        if (player.inventory.length === 0) {
            ctx.fillStyle = '#666';
            ctx.font = '14px monospace';
            ctx.fillText('Empty bag. Time to dig!', 30, 80);
        } else {
            // Grid layout
            const cols = 5;
            const cellSize = 120;
            const startX = (CANVAS_WIDTH - cols * cellSize) / 2;
            const startY = 50;

            for (let i = 0; i < player.inventory.length; i++) {
                const item = player.inventory[i];
                const col = i % cols;
                const row = Math.floor(i / cols);
                const cx = startX + col * cellSize;
                const cy = startY + row * (cellSize + 10);

                if (cy > CANVAS_HEIGHT - 50) break;

                const rarity = RARITY[item.rarity];
                const hovered = mouse.x >= cx && mouse.x < cx + cellSize - 8 &&
                               mouse.y >= cy && mouse.y < cy + cellSize;

                // Cell
                ctx.fillStyle = hovered ? '#333' : '#222';
                ctx.fillRect(cx, cy, cellSize - 8, cellSize);
                ctx.strokeStyle = rarity.color;
                ctx.lineWidth = hovered ? 2 : 1;
                ctx.strokeRect(cx, cy, cellSize - 8, cellSize);

                // Item icon (colored square with letter)
                ctx.fillStyle = rarity.color;
                ctx.fillRect(cx + 30, cy + 15, 52, 40);
                ctx.fillStyle = '#000';
                ctx.font = 'bold 20px monospace';
                ctx.fillText(item.name[0], cx + 46, cy + 42);

                // Name
                ctx.fillStyle = '#FFF';
                ctx.font = '9px monospace';
                const displayName = item.name.length > 14 ? item.name.substring(0, 13) + '..' : item.name;
                ctx.fillText(displayName, cx + 5, cy + 72);

                // Rarity
                ctx.fillStyle = rarity.color;
                ctx.font = '8px monospace';
                ctx.fillText(rarity.name, cx + 5, cy + 84);

                // Value
                ctx.fillStyle = '#FFD700';
                ctx.fillText(`~${item.baseValue} G`, cx + 5, cy + 96);

                // Tooltip on hover
                if (hovered) {
                    this.drawTooltip(mouse.x + 10, mouse.y - 10, item);
                }
            }
        }

        // Drop item hint
        ctx.fillStyle = '#666';
        ctx.font = '10px monospace';
        ctx.fillText('Press I to close', CANVAS_WIDTH / 2 - 50, CANVAS_HEIGHT - 15);

        return null;
    }

    drawTooltip(x, y, item) {
        const ctx = this.ctx;
        const rarity = RARITY[item.rarity];
        const w = 200, h = 80;

        // Clamp to screen
        if (x + w > CANVAS_WIDTH) x = CANVAS_WIDTH - w - 5;
        if (y + h > CANVAS_HEIGHT) y = CANVAS_HEIGHT - h - 5;

        ctx.fillStyle = 'rgba(20, 20, 40, 0.95)';
        ctx.fillRect(x, y, w, h);
        ctx.strokeStyle = rarity.color;
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, w, h);

        ctx.fillStyle = rarity.color;
        ctx.font = 'bold 11px monospace';
        ctx.fillText(item.name, x + 8, y + 16);

        ctx.fillStyle = '#CCC';
        ctx.font = '10px monospace';
        ctx.fillText(item.description, x + 8, y + 32);

        ctx.fillStyle = '#AAA';
        ctx.fillText(`Stratum ${item.stratum} | ${rarity.name}`, x + 8, y + 48);
        ctx.fillStyle = '#FFD700';
        ctx.fillText(`Base value: ${item.baseValue} G`, x + 8, y + 64);
    }

    // === EXPEDITION SUMMARY SCREEN ===
    drawExpeditionSummary(summary) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('EXPEDITION COMPLETE', 0, 0, CANVAS_WIDTH);

        let y = 60;

        // Stats
        ctx.fillStyle = '#FFF';
        ctx.font = '14px monospace';
        ctx.fillText(`Max Depth: ${summary.maxDepth}m`, 40, y); y += 25;
        ctx.fillText(`Items Found: ${summary.itemsFound}`, 40, y); y += 25;
        ctx.fillText(`Tiles Dug: ${summary.tilesDug}`, 40, y); y += 25;

        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 16px monospace';
        ctx.fillText(`Estimated Haul Value: ${summary.estimatedValue} G`, 40, y); y += 40;

        // Items list
        if (summary.items && summary.items.length > 0) {
            ctx.fillStyle = '#AAA';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('YOUR HAUL:', 40, y); y += 20;

            for (const item of summary.items) {
                const rarity = RARITY[item.rarity];
                ctx.fillStyle = rarity.color;
                ctx.font = '11px monospace';
                ctx.fillText(`- ${item.name} (${rarity.name})`, 50, y);
                ctx.fillStyle = '#FFD700';
                ctx.fillText(`~${item.baseValue} G`, 400, y);
                y += 18;
                if (y > CANVAS_HEIGHT - 80) break;
            }
        }

        // Continue button
        const bx = CANVAS_WIDTH / 2 - 80, by = CANVAS_HEIGHT - 55, bw = 160, bh = 35;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;
        ctx.fillStyle = bHover ? '#4CAF50' : '#2E7D32';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 14px monospace';
        ctx.fillText('CONTINUE', bx + 30, by + 23);

        if (bHover && mouse.justClicked) {
            return { action: 'continue' };
        }
        return null;
    }

    // === PREP SCREEN ===
    drawPrep(player, economy) {
        const ctx = this.ctx;
        const mouse = getMouse();

        ctx.fillStyle = '#1a1a2e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        this.drawPanelHeader('EXPEDITION PREP', 0, 0, CANVAS_WIDTH);

        let y = 60;
        ctx.fillStyle = '#FFF';
        ctx.font = '14px monospace';
        ctx.fillText('Prepare for your descent into The Column.', 40, y); y += 30;

        ctx.fillStyle = '#AAA';
        ctx.font = '12px monospace';
        ctx.fillText(`Stamina: ${Math.ceil(player.stamina)}/${player.maxStamina}`, 40, y); y += 20;
        ctx.fillText(`Bag Space: ${player.inventory.length}/${player.inventorySlots}`, 40, y); y += 20;
        ctx.fillText(`Dig Power: ${player.digPower.toFixed(1)}x`, 40, y); y += 30;

        // Rest option
        if (player.stamina < player.maxStamina) {
            const rx = 40, ry = y, rw = 200, rh = 30;
            const rHover = mouse.x >= rx && mouse.x < rx + rw && mouse.y >= ry && mouse.y < ry + rh;
            ctx.fillStyle = rHover ? '#4FC3F7' : '#1565C0';
            ctx.fillRect(rx, ry, rw, rh);
            ctx.fillStyle = '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText('REST (Full Recovery)', rx + 20, ry + 20);

            if (rHover && mouse.justClicked) {
                return { action: 'rest' };
            }
            y += 40;
        }

        // Market event info
        if (economy.activeEvent) {
            ctx.fillStyle = '#FFD700';
            ctx.font = 'bold 12px monospace';
            ctx.fillText(`Active Event: ${economy.activeEvent.name}`, 40, y);
            ctx.fillStyle = '#AAA';
            ctx.font = '11px monospace';
            ctx.fillText(economy.activeEvent.description, 40, y + 18);
            y += 50;
        }

        // Begin expedition button
        const bx = CANVAS_WIDTH / 2 - 100, by = CANVAS_HEIGHT - 80, bw = 200, bh = 40;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;
        ctx.fillStyle = bHover ? '#FF6B35' : '#E65100';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 16px monospace';
        ctx.fillText('DESCEND', bx + 50, by + 27);

        if (bHover && mouse.justClicked) {
            return { action: 'descend' };
        }

        return this.drawBackButton(mouse);
    }

    // === MAIN MENU ===
    drawMenu() {
        const ctx = this.ctx;
        const mouse = getMouse();

        // Background
        ctx.fillStyle = '#0a0a1e';
        ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Parallax dirt layers
        const t = Date.now() / 3000;
        const layerColors = ['#5C4033', '#4A4A5A', '#2D1B2E', '#8B6914'];
        for (let i = 0; i < 4; i++) {
            const ly = 350 + i * 40 + Math.sin(t + i) * 5;
            ctx.fillStyle = layerColors[i];
            ctx.globalAlpha = 0.3;
            ctx.fillRect(0, ly, CANVAS_WIDTH, 40);
        }
        ctx.globalAlpha = 1;

        // Title
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 64px monospace';
        ctx.textAlign = 'center';
        ctx.fillText('STRATA', CANVAS_WIDTH / 2, 160);

        ctx.fillStyle = '#AAA';
        ctx.font = '16px monospace';
        ctx.fillText('Dig. Appraise. Sell. Go Deeper.', CANVAS_WIDTH / 2, 200);

        // Start button
        const bx = CANVAS_WIDTH / 2 - 100, by = 280, bw = 200, bh = 50;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;

        ctx.fillStyle = bHover ? '#FF6B35' : '#E65100';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 20px monospace';
        ctx.fillText('NEW GAME', CANVAS_WIDTH / 2, by + 33);

        ctx.textAlign = 'left';

        // Controls info
        ctx.fillStyle = '#666';
        ctx.font = '11px monospace';
        ctx.textAlign = 'center';
        ctx.fillText('Arrow Keys / WASD: Move & Dig  |  Space: Jump  |  Shift+Dir: Dig sideways', CANVAS_WIDTH / 2, CANVAS_HEIGHT - 60);
        ctx.fillText('M: Market  |  U: Upgrades  |  C: Contracts  |  I: Inventory  |  E: Return to Surface', CANVAS_WIDTH / 2, CANVAS_HEIGHT - 40);
        ctx.textAlign = 'left';

        if (bHover && mouse.justClicked) {
            return { action: 'start' };
        }
        return null;
    }

    // === HELPERS ===
    drawPanelHeader(text, x, y, width) {
        const ctx = this.ctx;
        ctx.fillStyle = '#2a2a3a';
        ctx.fillRect(x, y, width, 35);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 18px monospace';
        ctx.fillText(text, x + 15, y + 24);
    }

    drawBackButton(mouse) {
        const ctx = this.ctx;
        const bx = 10, by = CANVAS_HEIGHT - 42, bw = 80, bh = 28;
        const bHover = mouse.x >= bx && mouse.x < bx + bw && mouse.y >= by && mouse.y < by + bh;
        ctx.fillStyle = bHover ? '#555' : '#333';
        ctx.fillRect(bx, by, bw, bh);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 11px monospace';
        ctx.fillText('< BACK', bx + 12, by + 18);

        if (bHover && mouse.justClicked) {
            return { action: 'back' };
        }
        if (isKeyJustPressed('Escape')) {
            return { action: 'back' };
        }
        return null;
    }
}
