import { TILE_SIZE } from './config.js';

export class ParticleSystem {
    constructor() {
        this.particles = [];
    }

    update(dt) {
        for (let i = this.particles.length - 1; i >= 0; i--) {
            const p = this.particles[i];
            p.x += p.vx * dt;
            p.y += p.vy * dt;
            p.vy += (p.gravity || 80) * dt;
            p.life -= dt;
            p.alpha = Math.max(0, p.life / p.maxLife);

            if (p.life <= 0) {
                this.particles.splice(i, 1);
            }
        }
    }

    draw(ctx, camera) {
        const offset = camera.getOffset();
        for (const p of this.particles) {
            ctx.globalAlpha = p.alpha;
            ctx.fillStyle = p.color;
            const sx = p.x + offset.x;
            const sy = p.y + offset.y;
            ctx.fillRect(sx, sy, p.size, p.size);
        }
        ctx.globalAlpha = 1;
    }

    // Dig particles
    emitDig(tileX, tileY, color) {
        const cx = tileX * TILE_SIZE + TILE_SIZE / 2;
        const cy = tileY * TILE_SIZE + TILE_SIZE / 2;
        for (let i = 0; i < 8; i++) {
            this.particles.push({
                x: cx + (Math.random() - 0.5) * TILE_SIZE,
                y: cy + (Math.random() - 0.5) * TILE_SIZE,
                vx: (Math.random() - 0.5) * 100,
                vy: -Math.random() * 60 - 20,
                gravity: 120,
                size: 2 + Math.random() * 3,
                color: color,
                life: 0.3 + Math.random() * 0.4,
                maxLife: 0.7,
                alpha: 1
            });
        }
    }

    // Item found sparkle
    emitSparkle(tileX, tileY, color) {
        const cx = tileX * TILE_SIZE + TILE_SIZE / 2;
        const cy = tileY * TILE_SIZE + TILE_SIZE / 2;
        for (let i = 0; i < 12; i++) {
            const angle = (Math.PI * 2 * i) / 12;
            this.particles.push({
                x: cx,
                y: cy,
                vx: Math.cos(angle) * (60 + Math.random() * 40),
                vy: Math.sin(angle) * (60 + Math.random() * 40),
                gravity: 0,
                size: 2 + Math.random() * 2,
                color: color,
                life: 0.5 + Math.random() * 0.3,
                maxLife: 0.8,
                alpha: 1
            });
        }
    }

    // Coin cascade for selling
    emitCoins(screenX, screenY, count) {
        for (let i = 0; i < count; i++) {
            this.particles.push({
                x: screenX + (Math.random() - 0.5) * 40,
                y: screenY,
                vx: (Math.random() - 0.5) * 80,
                vy: -Math.random() * 150 - 50,
                gravity: 200,
                size: 3 + Math.random() * 2,
                color: Math.random() > 0.3 ? '#FFD700' : '#FFA500',
                life: 0.8 + Math.random() * 0.5,
                maxLife: 1.3,
                alpha: 1
            });
        }
    }
}
