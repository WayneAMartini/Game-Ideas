// Procedural audio using Web Audio API
export class AudioManager {
    constructor() {
        this.ctx = null;
        this.enabled = true;
        this.masterGain = null;
        this.initialized = false;
    }

    init() {
        try {
            this.ctx = new (window.AudioContext || window.webkitAudioContext)();
            this.masterGain = this.ctx.createGain();
            this.masterGain.gain.value = 0.3;
            this.masterGain.connect(this.ctx.destination);
            this.initialized = true;
        } catch (e) {
            this.enabled = false;
        }
    }

    ensureResumed() {
        if (this.ctx && this.ctx.state === 'suspended') {
            this.ctx.resume();
        }
    }

    playDig(hardness) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();
        const now = this.ctx.currentTime;

        // Lower pitch for harder materials
        const baseFreq = 200 - hardness * 30;
        osc.frequency.setValueAtTime(baseFreq + Math.random() * 50, now);
        osc.frequency.exponentialRampToValueAtTime(baseFreq * 0.5, now + 0.1);
        osc.type = 'square';

        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.12);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.12);
    }

    playItemFound(rarity) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;

        // Pleasant ascending arpeggio
        const notes = [523, 659, 784]; // C5, E5, G5
        if (rarity === 'RARE') notes.push(1047); // C6
        if (rarity === 'LEGENDARY') { notes.push(1047, 1319); } // C6, E6

        notes.forEach((freq, i) => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            const t = now + i * 0.08;

            osc.frequency.value = freq;
            osc.type = 'sine';

            gain.gain.setValueAtTime(0, t);
            gain.gain.linearRampToValueAtTime(0.12, t + 0.02);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.25);

            osc.connect(gain);
            gain.connect(this.masterGain);
            osc.start(t);
            osc.stop(t + 0.25);
        });
    }

    playSell(amount) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        const count = Math.min(5, Math.ceil(amount / 50));

        for (let i = 0; i < count; i++) {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            const t = now + i * 0.06;

            osc.frequency.value = 800 + i * 100 + Math.random() * 50;
            osc.type = 'sine';

            gain.gain.setValueAtTime(0.1, t);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.15);

            osc.connect(gain);
            gain.connect(this.masterGain);
            osc.start(t);
            osc.stop(t + 0.15);
        }
    }

    playUpgrade() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        // Fanfare: C-E-G-C ascending
        const notes = [262, 330, 392, 523];

        notes.forEach((freq, i) => {
            const osc = this.ctx.createOscillator();
            const gain = this.ctx.createGain();
            const t = now + i * 0.12;

            osc.frequency.value = freq;
            osc.type = 'triangle';

            gain.gain.setValueAtTime(0.15, t);
            gain.gain.linearRampToValueAtTime(0.1, t + 0.1);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.4);

            osc.connect(gain);
            gain.connect(this.masterGain);
            osc.start(t);
            osc.stop(t + 0.4);
        });
    }

    playAmbient(stratum) {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        // Create a subtle ambient drone based on depth
        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        const baseFreq = 80 - stratum * 10;
        osc.frequency.value = Math.max(30, baseFreq);
        osc.type = 'sine';

        gain.gain.setValueAtTime(0, now);
        gain.gain.linearRampToValueAtTime(0.03, now + 1);
        gain.gain.linearRampToValueAtTime(0, now + 3);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 3);
    }

    playClick() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.frequency.value = 600;
        osc.type = 'square';

        gain.gain.setValueAtTime(0.08, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.05);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.05);
    }

    playHazard() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.frequency.setValueAtTime(300, now);
        osc.frequency.linearRampToValueAtTime(100, now + 0.3);
        osc.type = 'sawtooth';

        gain.gain.setValueAtTime(0.15, now);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.3);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.3);
    }

    playReturn() {
        if (!this.enabled || !this.initialized) return;
        this.ensureResumed();

        const now = this.ctx.currentTime;
        // Rising sweep
        const osc = this.ctx.createOscillator();
        const gain = this.ctx.createGain();

        osc.frequency.setValueAtTime(200, now);
        osc.frequency.exponentialRampToValueAtTime(800, now + 0.5);
        osc.type = 'sine';

        gain.gain.setValueAtTime(0.1, now);
        gain.gain.linearRampToValueAtTime(0.15, now + 0.3);
        gain.gain.exponentialRampToValueAtTime(0.001, now + 0.6);

        osc.connect(gain);
        gain.connect(this.masterGain);
        osc.start(now);
        osc.stop(now + 0.6);
    }

    toggle() {
        this.enabled = !this.enabled;
        if (this.masterGain) {
            this.masterGain.gain.value = this.enabled ? 0.3 : 0;
        }
        return this.enabled;
    }
}
