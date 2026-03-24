import { PLAYER, TILES, SURFACE_HEIGHT, WORLD_WIDTH } from './config.js';
import { isKeyDown, isKeyJustPressed } from './input.js';

export class Player {
    constructor(world) {
        this.world = world;
        this.x = PLAYER.startX;
        this.y = PLAYER.startY;
        this.vx = 0;
        this.vy = 0;
        this.onGround = false;
        this.facingRight = true;
        this.stamina = PLAYER.maxStamina;
        this.maxStamina = PLAYER.maxStamina;
        this.digPower = PLAYER.digSpeed;
        this.moveSpeed = PLAYER.moveSpeed;
        this.headlampRadius = PLAYER.headlampRadius;

        // Digging state
        this.digging = false;
        this.digTarget = null; // { x, y }
        this.digProgress = 0;
        this.digTime = 0;

        // Inventory
        this.inventorySlots = PLAYER.inventorySlots;
        this.inventory = [];
        this.gold = 0;

        // Visual
        this.animFrame = 0;
        this.animTimer = 0;

        // Underground flag
        this.isUnderground = false;
    }

    update(dt) {
        this.isUnderground = this.y >= SURFACE_HEIGHT;

        if (this.digging) {
            return this.updateDigging(dt);
        }

        this.handleMovement(dt);
        this.handleDigInput();
        this.applyGravity(dt);
        this.applyMovement(dt);

        // Stamina drain while underground
        if (this.isUnderground) {
            this.stamina -= PLAYER.staminaDrainMove * dt;
            if (this.stamina < 0) this.stamina = 0;
        }

        // Animation
        this.animTimer += dt;
        if (this.animTimer > 0.15) {
            this.animTimer = 0;
            this.animFrame = (this.animFrame + 1) % 4;
        }
    }

    handleMovement(dt) {
        this.vx = 0;

        if (isKeyDown('ArrowLeft') || isKeyDown('KeyA')) {
            this.vx = -this.moveSpeed;
            this.facingRight = false;
        }
        if (isKeyDown('ArrowRight') || isKeyDown('KeyD')) {
            this.vx = this.moveSpeed;
            this.facingRight = true;
        }
        if ((isKeyDown('ArrowUp') || isKeyDown('KeyW') || isKeyDown('Space')) && this.onGround) {
            this.vy = -8;
            this.onGround = false;
        }
    }

    handleDigInput() {
        // Dig down
        if (isKeyDown('ArrowDown') || isKeyDown('KeyS')) {
            const tx = Math.round(this.x);
            const ty = Math.round(this.y) + 1;
            if (this.world.isDiggable(tx, ty) && this.stamina > 0) {
                this.startDig(tx, ty);
                return;
            }
        }

        // Dig left/right (hold shift + direction, or just direction into a wall)
        if (isKeyDown('ShiftLeft') || isKeyDown('ShiftRight')) {
            if (isKeyDown('ArrowLeft') || isKeyDown('KeyA')) {
                const tx = Math.round(this.x) - 1;
                const ty = Math.round(this.y);
                if (this.world.isDiggable(tx, ty) && this.stamina > 0) {
                    this.startDig(tx, ty);
                    return;
                }
            }
            if (isKeyDown('ArrowRight') || isKeyDown('KeyD')) {
                const tx = Math.round(this.x) + 1;
                const ty = Math.round(this.y);
                if (this.world.isDiggable(tx, ty) && this.stamina > 0) {
                    this.startDig(tx, ty);
                    return;
                }
            }
        }
    }

    startDig(tx, ty) {
        this.digging = true;
        this.digTarget = { x: tx, y: ty };
        this.digProgress = 0;
        this.digTime = this.world.getDigTime(tx, ty, this.digPower);
    }

    updateDigging(dt) {
        this.digProgress += dt;
        this.stamina -= PLAYER.staminaDrainDig * dt;

        if (this.stamina <= 0) {
            this.stamina = 0;
            this.digging = false;
            this.digTarget = null;
            return null;
        }

        if (this.digProgress >= this.digTime) {
            // Complete the dig
            const item = this.world.digTile(this.digTarget.x, this.digTarget.y);
            this.digging = false;
            // Keep digTarget so game.js can read it for particle position
            this.lastDigTarget = { ...this.digTarget };
            this.digTarget = null;

            if (item && this.inventory.length < this.inventorySlots) {
                this.inventory.push(item);
                return item; // Signal that an item was found
            }

            return item ? 'full' : 'dug';
        }

        return null;
    }

    applyGravity(dt) {
        if (!this.onGround) {
            this.vy += 20 * dt; // gravity
            if (this.vy > 12) this.vy = 12; // terminal velocity
        }
    }

    applyMovement(dt) {
        // Horizontal movement
        const newX = this.x + this.vx * dt;
        const tileX = Math.round(newX);
        const tileY = Math.round(this.y);

        if (!this.world.isSolid(tileX, tileY) && newX >= 0 && newX < WORLD_WIDTH) {
            this.x = newX;
        }

        // Vertical movement
        const newY = this.y + this.vy * dt;
        const checkY = Math.round(newY);

        if (this.vy >= 0) {
            // Falling - check below
            if (this.world.isSolid(Math.round(this.x), checkY)) {
                this.y = checkY - 1;
                this.vy = 0;
                this.onGround = true;
            } else {
                this.y = newY;
                this.onGround = false;
            }
        } else {
            // Jumping - check above
            if (this.world.isSolid(Math.round(this.x), checkY)) {
                this.vy = 0;
            } else {
                this.y = newY;
            }
        }
    }

    isAtSurface() {
        return this.y < SURFACE_HEIGHT;
    }

    getDepthMeters() {
        if (this.y <= SURFACE_HEIGHT) return 0;
        return Math.floor((this.y - SURFACE_HEIGHT) * 3); // 3 meters per tile
    }

    recoverStamina(amount) {
        this.stamina = Math.min(this.maxStamina, this.stamina + amount);
    }
}
