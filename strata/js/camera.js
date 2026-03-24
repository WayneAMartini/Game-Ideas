import { TILE_SIZE, CANVAS_WIDTH, CANVAS_HEIGHT, WORLD_WIDTH, WORLD_HEIGHT } from './config.js';

export class Camera {
    constructor() {
        this.x = 0;
        this.y = 0;
        this.targetX = 0;
        this.targetY = 0;
        this.smoothing = 8;
        this.shake = 0;
        this.shakeDecay = 5;
    }

    follow(player, dt) {
        // Target: center on player
        this.targetX = player.x * TILE_SIZE - CANVAS_WIDTH / 2 + TILE_SIZE / 2;
        this.targetY = player.y * TILE_SIZE - CANVAS_HEIGHT / 2 + TILE_SIZE / 2;

        // Smooth follow
        this.x += (this.targetX - this.x) * this.smoothing * dt;
        this.y += (this.targetY - this.y) * this.smoothing * dt;

        // Clamp to world bounds
        this.x = Math.max(0, Math.min(this.x, WORLD_WIDTH * TILE_SIZE - CANVAS_WIDTH));
        this.y = Math.max(-CANVAS_HEIGHT / 3, Math.min(this.y, WORLD_HEIGHT * TILE_SIZE - CANVAS_HEIGHT));

        // Screen shake
        if (this.shake > 0) {
            this.shake -= this.shakeDecay * dt;
            if (this.shake < 0) this.shake = 0;
        }
    }

    addShake(amount) {
        this.shake = Math.min(this.shake + amount, 10);
    }

    getOffset() {
        let ox = -Math.round(this.x);
        let oy = -Math.round(this.y);
        if (this.shake > 0) {
            ox += (Math.random() - 0.5) * this.shake * 2;
            oy += (Math.random() - 0.5) * this.shake * 2;
        }
        return { x: ox, y: oy };
    }

    // Get visible tile range for culling
    getVisibleRange() {
        const startX = Math.max(0, Math.floor(this.x / TILE_SIZE) - 1);
        const startY = Math.max(0, Math.floor(this.y / TILE_SIZE) - 1);
        const endX = Math.min(WORLD_WIDTH, Math.ceil((this.x + CANVAS_WIDTH) / TILE_SIZE) + 1);
        const endY = Math.min(WORLD_HEIGHT, Math.ceil((this.y + CANVAS_HEIGHT) / TILE_SIZE) + 1);
        return { startX, startY, endX, endY };
    }
}
