import { TILE_SIZE, CANVAS_WIDTH, CANVAS_HEIGHT, SURFACE_HEIGHT, STRATA, TILES, RARITY, WORLD_WIDTH } from './config.js';

export class Renderer {
    constructor(ctx) {
        this.ctx = ctx;
        // Pre-render tile textures
        this.tileCache = new Map();
        this.lightCanvas = document.createElement('canvas');
        this.lightCanvas.width = CANVAS_WIDTH;
        this.lightCanvas.height = CANVAS_HEIGHT;
        this.lightCtx = this.lightCanvas.getContext('2d');
    }

    clear() {
        this.ctx.fillStyle = '#1a1a2e';
        this.ctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
    }

    drawWorld(world, camera, player) {
        const offset = camera.getOffset();
        const range = camera.getVisibleRange();

        // Draw sky background
        this.drawSky(offset);

        // Draw tiles
        for (let y = range.startY; y < range.endY; y++) {
            for (let x = range.startX; x < range.endX; x++) {
                const tile = world.getTile(x, y);
                const sx = x * TILE_SIZE + offset.x;
                const sy = y * TILE_SIZE + offset.y;

                this.drawTile(tile, sx, sy, x, y, world);
            }
        }

        // Draw surface labels
        if (!player.isUnderground) {
            this.drawSurfaceLabels(offset);
        }

        // Draw player
        this.drawPlayer(player, offset);

        // Draw lighting overlay if underground
        if (player.isUnderground) {
            this.drawLighting(player, offset);
        }

        // Draw stratum transition labels
        if (player.isUnderground) {
            this.drawStratumLabels(offset, camera);
        }
    }

    drawSurfaceLabels(offset) {
        const ctx = this.ctx;
        const mid = Math.floor(WORLD_WIDTH / 2);
        const labelX = mid * TILE_SIZE + offset.x - 40;
        const labelY = (SURFACE_HEIGHT - 3) * TILE_SIZE + offset.y;

        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 10px monospace';
        ctx.textAlign = 'center';
        ctx.fillText('HOLLOWMARKET', mid * TILE_SIZE + offset.x + TILE_SIZE / 2, labelY);
        ctx.textAlign = 'left';
    }

    drawStratumLabels(offset, camera) {
        const ctx = this.ctx;
        for (const s of STRATA) {
            if (s.id === 0) continue;
            const labelY = s.startDepth * TILE_SIZE + offset.y + 20;
            if (labelY > -20 && labelY < CANVAS_HEIGHT + 20) {
                ctx.fillStyle = s.colors?.accent2 || '#FFF';
                ctx.globalAlpha = 0.4;
                ctx.font = 'bold 16px monospace';
                ctx.fillText(`--- ${s.name} ---`, 20, labelY);
                ctx.globalAlpha = 1;
            }
        }
    }

    drawSky(offset) {
        const skyBottom = SURFACE_HEIGHT * TILE_SIZE + offset.y;
        if (skyBottom > 0) {
            // Sky gradient
            const grad = this.ctx.createLinearGradient(0, Math.min(0, offset.y), 0, skyBottom);
            grad.addColorStop(0, '#4A90D9');
            grad.addColorStop(0.7, '#87CEEB');
            grad.addColorStop(1, '#B0E0E6');
            this.ctx.fillStyle = grad;
            this.ctx.fillRect(0, 0, CANVAS_WIDTH, Math.max(0, skyBottom));
        }
    }

    drawTile(tile, sx, sy, wx, wy, world) {
        if (tile.type === TILES.AIR) return;

        const ctx = this.ctx;

        switch (tile.type) {
            case TILES.SURFACE_GRASS: {
                // Grass surface
                ctx.fillStyle = '#4a8c3f';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                // Grass blades on top
                ctx.fillStyle = '#5ca84f';
                for (let i = 0; i < 5; i++) {
                    const gx = sx + 3 + i * 6;
                    ctx.fillRect(gx, sy, 2, 4);
                }
                // Dirt below grass
                ctx.fillStyle = '#8B7355';
                ctx.fillRect(sx, sy + TILE_SIZE * 0.6, TILE_SIZE, TILE_SIZE * 0.4);
                break;
            }

            case TILES.BUILDING: {
                // Wooden market building
                ctx.fillStyle = '#8B6914';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                ctx.fillStyle = '#A0522D';
                ctx.fillRect(sx + 2, sy + 2, TILE_SIZE - 4, TILE_SIZE - 4);
                // Window
                ctx.fillStyle = '#FFE4B5';
                ctx.fillRect(sx + 10, sy + 8, 12, 10);
                ctx.fillStyle = '#333';
                ctx.fillRect(sx + 15, sy + 8, 2, 10);
                ctx.fillRect(sx + 10, sy + 12, 12, 2);
                break;
            }

            case TILES.DIRT: {
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                if (!colors) { ctx.fillStyle = '#5C4033'; ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE); break; }

                ctx.fillStyle = colors.dirt || colors.base;
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Pixel texture
                const hash = this.tileHash(wx, wy);
                ctx.fillStyle = colors.base;
                if (hash % 3 === 0) ctx.fillRect(sx + 4, sy + 4, 6, 4);
                if (hash % 5 === 0) ctx.fillRect(sx + 16, sy + 12, 8, 5);
                if (hash % 7 === 0) ctx.fillRect(sx + 8, sy + 20, 5, 6);
                break;
            }

            case TILES.ROCK: {
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                ctx.fillStyle = colors?.base || '#666';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Rock texture - cracks
                ctx.strokeStyle = colors?.accent1 || '#888';
                ctx.lineWidth = 1;
                ctx.beginPath();
                const h = this.tileHash(wx, wy);
                ctx.moveTo(sx + (h % 12) + 4, sy + 2);
                ctx.lineTo(sx + TILE_SIZE / 2, sy + TILE_SIZE / 2);
                ctx.lineTo(sx + TILE_SIZE - (h % 8) - 4, sy + TILE_SIZE - 4);
                ctx.stroke();
                break;
            }

            case TILES.HARD_ROCK: {
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                ctx.fillStyle = colors?.accent1 || '#555';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Harder look - crystal/metallic shards
                ctx.fillStyle = colors?.accent2 || '#777';
                const h = this.tileHash(wx, wy);
                ctx.fillRect(sx + (h % 10) + 4, sy + 6, 8, 4);
                ctx.fillRect(sx + 12, sy + (h % 10) + 8, 6, 8);
                break;
            }

            case TILES.ITEM: {
                // Draw as dirt/rock with a sparkle hint
                const stratum = this.getStratum(tile.stratum);
                const colors = stratum?.colors;
                ctx.fillStyle = colors?.dirt || '#5C4033';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);

                // Subtle sparkle to hint at embedded item
                const rarityColor = tile.itemDef ? (RARITY[tile.itemDef.rarity]?.color || '#FFF') : '#FFF';
                const sparklePhase = (Date.now() / 500 + wx * 7 + wy * 13) % (Math.PI * 2);
                const sparkleAlpha = 0.2 + Math.sin(sparklePhase) * 0.15;
                ctx.fillStyle = rarityColor;
                ctx.globalAlpha = sparkleAlpha;
                ctx.fillRect(sx + 10, sy + 10, 12, 12);
                ctx.globalAlpha = 1;
                break;
            }

            case TILES.HAZARD_GAS: {
                ctx.fillStyle = '#2D4A2D';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                // Gas wisps
                const t = Date.now() / 1000;
                ctx.fillStyle = '#4AFF4A';
                ctx.globalAlpha = 0.3 + Math.sin(t + wx) * 0.15;
                ctx.fillRect(sx + 6, sy + 4 + Math.sin(t * 2 + wx) * 3, 10, 6);
                ctx.fillRect(sx + 14, sy + 16 + Math.sin(t * 2.5 + wy) * 3, 12, 5);
                ctx.globalAlpha = 1;
                break;
            }

            case TILES.HAZARD_WATER: {
                ctx.fillStyle = '#1a3a5c';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                // Water animation
                const t = Date.now() / 800;
                ctx.fillStyle = '#4A90D9';
                ctx.globalAlpha = 0.5;
                for (let i = 0; i < 3; i++) {
                    const waveY = sy + 8 + i * 8 + Math.sin(t + wx + i) * 3;
                    ctx.fillRect(sx, waveY, TILE_SIZE, 3);
                }
                ctx.globalAlpha = 1;
                break;
            }

            case TILES.BEDROCK: {
                ctx.fillStyle = '#1a1a1a';
                ctx.fillRect(sx, sy, TILE_SIZE, TILE_SIZE);
                ctx.fillStyle = '#2a2a2a';
                ctx.fillRect(sx + 4, sy + 4, 8, 8);
                ctx.fillRect(sx + 18, sy + 16, 10, 6);
                break;
            }
        }

        // Tile borders (subtle grid)
        if (tile.type !== TILES.AIR) {
            // Only draw edges where adjacent tiles are air
            ctx.strokeStyle = 'rgba(0,0,0,0.15)';
            ctx.lineWidth = 1;
            if (!world.isSolid(wx, wy - 1)) {
                ctx.beginPath(); ctx.moveTo(sx, sy + 0.5); ctx.lineTo(sx + TILE_SIZE, sy + 0.5); ctx.stroke();
            }
            if (!world.isSolid(wx, wy + 1)) {
                ctx.beginPath(); ctx.moveTo(sx, sy + TILE_SIZE - 0.5); ctx.lineTo(sx + TILE_SIZE, sy + TILE_SIZE - 0.5); ctx.stroke();
            }
        }
    }

    drawPlayer(player, offset) {
        const ctx = this.ctx;
        const px = player.x * TILE_SIZE + offset.x;
        const py = player.y * TILE_SIZE + offset.y;

        // Shadow
        ctx.fillStyle = 'rgba(0,0,0,0.3)';
        ctx.fillRect(px + 4, py + TILE_SIZE - 2, TILE_SIZE - 8, 4);

        // Body
        ctx.fillStyle = '#E8B04A';
        ctx.fillRect(px + 6, py + 4, TILE_SIZE - 12, TILE_SIZE - 8);

        // Hard hat
        ctx.fillStyle = '#FFD700';
        ctx.fillRect(px + 4, py, TILE_SIZE - 8, 8);
        ctx.fillRect(px + 2, py + 4, TILE_SIZE - 4, 4);

        // Eyes
        ctx.fillStyle = '#1a1a2e';
        if (player.facingRight) {
            ctx.fillRect(px + 16, py + 8, 3, 3);
        } else {
            ctx.fillRect(px + 13, py + 8, 3, 3);
        }

        // Headlamp glow
        if (player.isUnderground) {
            ctx.fillStyle = '#FFE';
            ctx.globalAlpha = 0.8;
            if (player.facingRight) {
                ctx.fillRect(px + TILE_SIZE - 6, py + 4, 4, 3);
            } else {
                ctx.fillRect(px + 2, py + 4, 4, 3);
            }
            ctx.globalAlpha = 1;
        }

        // Tool (pickaxe) while digging
        if (player.digging) {
            ctx.fillStyle = '#888';
            const swingAngle = (player.digProgress / player.digTime) * Math.PI;
            const toolX = player.facingRight ? px + TILE_SIZE : px;
            const toolLen = 12;
            ctx.save();
            ctx.translate(toolX, py + 12);
            ctx.rotate(player.facingRight ? swingAngle - 0.5 : -(swingAngle - 0.5));
            ctx.fillRect(0, -1, toolLen, 3);
            ctx.fillStyle = '#AAA';
            ctx.fillRect(toolLen - 4, -3, 6, 7);
            ctx.restore();
        }

        // Dig progress bar
        if (player.digging && player.digTarget) {
            const dtx = player.digTarget.x * TILE_SIZE + offset.x;
            const dty = player.digTarget.y * TILE_SIZE + offset.y;
            const pct = player.digProgress / player.digTime;
            ctx.fillStyle = 'rgba(0,0,0,0.5)';
            ctx.fillRect(dtx, dty - 6, TILE_SIZE, 4);
            ctx.fillStyle = '#4FC3F7';
            ctx.fillRect(dtx, dty - 6, TILE_SIZE * pct, 4);
        }
    }

    drawLighting(player, offset) {
        const lctx = this.lightCtx;
        lctx.clearRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Fill with darkness
        lctx.fillStyle = 'rgba(0, 0, 0, 0.85)';
        lctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);

        // Cut out light around player (headlamp)
        const px = player.x * TILE_SIZE + offset.x + TILE_SIZE / 2;
        const py = player.y * TILE_SIZE + offset.y + TILE_SIZE / 2;
        const radius = player.headlampRadius * TILE_SIZE;

        lctx.globalCompositeOperation = 'destination-out';

        // Main ambient glow around player
        const ambientGrad = lctx.createRadialGradient(px, py, 0, px, py, radius * 0.5);
        ambientGrad.addColorStop(0, 'rgba(0, 0, 0, 1)');
        ambientGrad.addColorStop(0.7, 'rgba(0, 0, 0, 0.5)');
        ambientGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');
        lctx.fillStyle = ambientGrad;
        lctx.fillRect(px - radius, py - radius, radius * 2, radius * 2);

        // Headlamp cone
        const lampAngle = player.facingRight ? 0 : Math.PI;
        const coneAngle = Math.PI / 4;
        const coneRadius = radius * 1.2;

        lctx.beginPath();
        lctx.moveTo(px, py);
        lctx.arc(px, py, coneRadius, lampAngle - coneAngle, lampAngle + coneAngle);
        lctx.closePath();

        const coneGrad = lctx.createRadialGradient(px, py, 0, px, py, coneRadius);
        coneGrad.addColorStop(0, 'rgba(0, 0, 0, 1)');
        coneGrad.addColorStop(0.6, 'rgba(0, 0, 0, 0.8)');
        coneGrad.addColorStop(1, 'rgba(0, 0, 0, 0)');
        lctx.fillStyle = coneGrad;
        lctx.fill();

        lctx.globalCompositeOperation = 'source-over';

        // Apply light overlay with stratum tint
        const stratum = this.getStratumAtY(player.y);
        if (stratum && stratum.id > 0) {
            lctx.fillStyle = stratum.bgColor;
            lctx.globalAlpha = 0.15;
            lctx.fillRect(0, 0, CANVAS_WIDTH, CANVAS_HEIGHT);
            lctx.globalAlpha = 1;
        }

        // Composite onto main canvas
        this.ctx.drawImage(this.lightCanvas, 0, 0);
    }

    drawHUD(player, gameState, expedition) {
        const ctx = this.ctx;

        // Stamina bar
        const barX = 10, barY = 10, barW = 200, barH = 16;
        const staminaPct = player.stamina / player.maxStamina;

        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(barX - 2, barY - 2, barW + 4, barH + 4);
        ctx.fillStyle = staminaPct > 0.3 ? '#4CAF50' : (staminaPct > 0.15 ? '#FF9800' : '#F44336');
        ctx.fillRect(barX, barY, barW * staminaPct, barH);
        ctx.strokeStyle = '#FFF';
        ctx.lineWidth = 1;
        ctx.strokeRect(barX, barY, barW, barH);

        // Stamina text
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 11px monospace';
        ctx.fillText(`STAMINA ${Math.ceil(player.stamina)}/${player.maxStamina}`, barX + 4, barY + 12);

        // Depth indicator
        const depth = player.getDepthMeters();
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(barX, barY + barH + 8, 120, 20);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 12px monospace';
        ctx.fillText(`DEPTH: ${depth}m`, barX + 6, barY + barH + 22);

        // Current stratum
        const stratum = this.getStratumAtY(player.y);
        if (stratum && stratum.name !== 'Surface') {
            ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
            ctx.fillRect(barX, barY + barH + 32, 220, 20);
            ctx.fillStyle = stratum.colors?.accent2 || '#FFF';
            ctx.font = 'bold 12px monospace';
            ctx.fillText(`${stratum.name} — ${stratum.description || ''}`, barX + 6, barY + barH + 46);
        }

        // Gold
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(CANVAS_WIDTH - 140, 10, 130, 24);
        ctx.fillStyle = '#FFD700';
        ctx.font = 'bold 14px monospace';
        ctx.fillText(`${player.gold} G`, CANVAS_WIDTH - 132, 28);

        // Inventory count
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        ctx.fillRect(CANVAS_WIDTH - 140, 40, 130, 24);
        ctx.fillStyle = '#FFF';
        ctx.font = 'bold 12px monospace';
        ctx.fillText(`BAG: ${player.inventory.length}/${player.inventorySlots}`, CANVAS_WIDTH - 132, 56);

        // Items found this expedition
        if (!player.isAtSurface() && expedition) {
            ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
            ctx.fillRect(CANVAS_WIDTH - 140, 70, 130, 24);
            ctx.fillStyle = '#AAA';
            ctx.font = '11px monospace';
            ctx.fillText(`FOUND: ${expedition.itemsFound}`, CANVAS_WIDTH - 132, 86);
        }

        // Depth sidebar (visual depth meter on right edge)
        if (!player.isAtSurface()) {
            this.drawDepthMeter(player);
        }

        // Controls hint (context-sensitive)
        if (player.isAtSurface()) {
            this.drawControlHint('ARROWS: Move  |  DOWN: Enter Mine  |  M: Market  |  U: Upgrades  |  C: Contracts');
        } else {
            this.drawControlHint('ARROWS: Move  |  DOWN/SHIFT+DIR: Dig  |  SPACE: Jump  |  E: Return  |  I: Inventory');
        }
    }

    drawDepthMeter(player) {
        const ctx = this.ctx;
        const x = CANVAS_WIDTH - 16;
        const h = CANVAS_HEIGHT - 80;
        const y = 40;

        // Background
        ctx.fillStyle = 'rgba(0, 0, 0, 0.5)';
        ctx.fillRect(x, y, 12, h);

        // Stratum colors
        const totalDepth = 200; // WORLD_HEIGHT
        for (const s of STRATA) {
            if (s.id === 0) continue;
            const sy = y + (s.startDepth / totalDepth) * h;
            const sh = ((s.endDepth - s.startDepth) / totalDepth) * h;
            ctx.fillStyle = s.colors?.accent2 || s.bgColor || '#333';
            ctx.globalAlpha = 0.6;
            ctx.fillRect(x + 1, sy, 10, sh);
        }
        ctx.globalAlpha = 1;

        // Player position marker
        const playerPct = Math.max(0, Math.min(1, player.y / totalDepth));
        const markerY = y + playerPct * h;
        ctx.fillStyle = '#FFD700';
        ctx.fillRect(x - 3, markerY - 2, 18, 4);
    }

    drawControlHint(text) {
        const ctx = this.ctx;
        ctx.fillStyle = 'rgba(0, 0, 0, 0.6)';
        const w = ctx.measureText(text).width + 20;
        ctx.font = '10px monospace';
        const measured = ctx.measureText(text).width + 20;
        ctx.fillRect((CANVAS_WIDTH - measured) / 2, CANVAS_HEIGHT - 28, measured, 22);
        ctx.fillStyle = '#AAA';
        ctx.fillText(text, (CANVAS_WIDTH - measured) / 2 + 10, CANVAS_HEIGHT - 12);
    }

    drawNotification(text, color, progress, index) {
        const ctx = this.ctx;
        const alpha = progress < 0.1 ? progress / 0.1 : (progress > 0.7 ? (1 - progress) / 0.3 : 1);
        ctx.globalAlpha = alpha;
        ctx.fillStyle = 'rgba(0, 0, 0, 0.7)';
        ctx.font = 'bold 14px monospace';
        const w = ctx.measureText(text).width + 24;
        const x = (CANVAS_WIDTH - w) / 2;
        const baseY = CANVAS_HEIGHT / 3 + (index || 0) * 35;
        const y = baseY - progress * 20;
        ctx.fillRect(x, y, w, 28);
        ctx.fillStyle = color || '#FFF';
        ctx.fillText(text, x + 12, y + 19);
        ctx.globalAlpha = 1;
    }

    getStratum(id) {
        return STRATA[id] || null;
    }

    getStratumAtY(y) {
        for (const s of STRATA) {
            if (y >= s.startDepth && y < s.endDepth) return s;
        }
        return STRATA[0];
    }

    tileHash(x, y) {
        return Math.abs(((x * 73856093) ^ (y * 19349663)) % 1000);
    }
}
