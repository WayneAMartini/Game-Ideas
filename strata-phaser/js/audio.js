// STRATA - Procedural Audio System
// ==================================
// All sounds generated via Web Audio API
(function(S) {
    'use strict';

    var ctx = null;
    var masterGain = null;
    var enabled = true;

    function getCtx() {
        if (!ctx) {
            try {
                ctx = new (window.AudioContext || window.webkitAudioContext)();
                masterGain = ctx.createGain();
                masterGain.gain.value = 0.3;
                masterGain.connect(ctx.destination);
            } catch (e) {
                console.warn('STRATA: Web Audio not available');
                return null;
            }
        }
        // Resume if suspended (autoplay policy)
        if (ctx.state === 'suspended') {
            ctx.resume();
        }
        return ctx;
    }

    function playTone(type, freq, duration, startTime, gainVal) {
        var c = getCtx();
        if (!c || !enabled) return;
        var t = startTime || c.currentTime;
        var osc = c.createOscillator();
        var gain = c.createGain();
        osc.type = type;
        osc.frequency.setValueAtTime(freq, t);
        gain.gain.setValueAtTime(gainVal || 0.2, t);
        gain.gain.exponentialRampToValueAtTime(0.001, t + duration);
        osc.connect(gain);
        gain.connect(masterGain);
        osc.start(t);
        osc.stop(t + duration);
    }

    S.Audio = {
        toggle: function() {
            enabled = !enabled;
            return enabled;
        },

        isEnabled: function() {
            return enabled;
        },

        // Dig sound - lower pitch for harder materials
        playDig: function(hardness) {
            var c = getCtx();
            if (!c || !enabled) return;
            hardness = hardness || 1;
            var baseFreq = (200 - hardness * 30) + Math.random() * 50;
            var t = c.currentTime;

            var osc = c.createOscillator();
            var gain = c.createGain();
            osc.type = 'square';
            osc.frequency.setValueAtTime(baseFreq, t);
            osc.frequency.linearRampToValueAtTime(baseFreq * 0.5, t + 0.12);
            gain.gain.setValueAtTime(0.15, t);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.12);
            osc.connect(gain);
            gain.connect(masterGain);
            osc.start(t);
            osc.stop(t + 0.12);

            // Add some noise
            var noise = c.createOscillator();
            var noiseGain = c.createGain();
            noise.type = 'sawtooth';
            noise.frequency.setValueAtTime(baseFreq * 3, t);
            noiseGain.gain.setValueAtTime(0.05, t);
            noiseGain.gain.exponentialRampToValueAtTime(0.001, t + 0.06);
            noise.connect(noiseGain);
            noiseGain.connect(masterGain);
            noise.start(t);
            noise.stop(t + 0.06);
        },

        // Item found - ascending arpeggio
        playItemFound: function(rarity) {
            var c = getCtx();
            if (!c || !enabled) return;

            var notes = [523, 659, 784]; // C5, E5, G5
            if (rarity === 'RARE' || rarity === 'LEGENDARY') notes.push(1047); // C6
            if (rarity === 'LEGENDARY') notes.push(1319); // E6

            var t = c.currentTime;
            for (var i = 0; i < notes.length; i++) {
                playTone('sine', notes[i], 0.2, t + i * 0.08, 0.15);
            }
        },

        // Sell sound - coin cascade
        playSell: function(amount) {
            var c = getCtx();
            if (!c || !enabled) return;
            amount = amount || 10;

            var count = Math.min(5, Math.ceil(amount / 50));
            var t = c.currentTime;

            for (var i = 0; i < count; i++) {
                var freq = 800 + i * 100 + Math.random() * 50;
                playTone('sine', freq, 0.15, t + i * 0.06, 0.12);
            }
        },

        // Upgrade purchase - fanfare
        playUpgrade: function() {
            var c = getCtx();
            if (!c || !enabled) return;

            var notes = [262, 330, 392, 523]; // C4, E4, G4, C5
            var t = c.currentTime;
            for (var i = 0; i < notes.length; i++) {
                playTone('triangle', notes[i], 0.4, t + i * 0.12, 0.15);
            }
        },

        // Hazard warning
        playHazard: function() {
            var c = getCtx();
            if (!c || !enabled) return;
            var t = c.currentTime;

            var osc = c.createOscillator();
            var gain = c.createGain();
            osc.type = 'sawtooth';
            osc.frequency.setValueAtTime(300, t);
            osc.frequency.exponentialRampToValueAtTime(100, t + 0.3);
            gain.gain.setValueAtTime(0.12, t);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.3);
            osc.connect(gain);
            gain.connect(masterGain);
            osc.start(t);
            osc.stop(t + 0.3);
        },

        // Return to surface - rising sweep
        playReturn: function() {
            var c = getCtx();
            if (!c || !enabled) return;
            var t = c.currentTime;

            var osc = c.createOscillator();
            var gain = c.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(200, t);
            osc.frequency.exponentialRampToValueAtTime(800, t + 0.6);
            gain.gain.setValueAtTime(0.15, t);
            gain.gain.linearRampToValueAtTime(0.2, t + 0.3);
            gain.gain.exponentialRampToValueAtTime(0.001, t + 0.6);
            osc.connect(gain);
            gain.connect(masterGain);
            osc.start(t);
            osc.stop(t + 0.6);
        },

        // UI click
        playClick: function() {
            var c = getCtx();
            if (!c || !enabled) return;
            playTone('square', 600, 0.05, null, 0.08);
        },

        // Ambient underground drone
        playAmbient: function(stratumId) {
            var c = getCtx();
            if (!c || !enabled) return;
            var freq = Math.max(30, 80 - (stratumId || 1) * 10);
            var t = c.currentTime;

            var osc = c.createOscillator();
            var gain = c.createGain();
            osc.type = 'sine';
            osc.frequency.setValueAtTime(freq, t);
            gain.gain.setValueAtTime(0, t);
            gain.gain.linearRampToValueAtTime(0.04, t + 1.5);
            gain.gain.linearRampToValueAtTime(0, t + 3);
            osc.connect(gain);
            gain.connect(masterGain);
            osc.start(t);
            osc.stop(t + 3);
        },

        // Menu/surface music - simple warm loop
        playMenuLoop: function() {
            var c = getCtx();
            if (!c || !enabled) return;

            // Simple warm pad
            var notes = [262, 330, 392, 330]; // C4 E4 G4 E4
            var t = c.currentTime;
            for (var i = 0; i < notes.length; i++) {
                var osc = c.createOscillator();
                var gain = c.createGain();
                osc.type = 'sine';
                osc.frequency.setValueAtTime(notes[i], t + i * 0.8);
                gain.gain.setValueAtTime(0, t + i * 0.8);
                gain.gain.linearRampToValueAtTime(0.06, t + i * 0.8 + 0.1);
                gain.gain.linearRampToValueAtTime(0, t + i * 0.8 + 0.7);
                osc.connect(gain);
                gain.connect(masterGain);
                osc.start(t + i * 0.8);
                osc.stop(t + i * 0.8 + 0.8);
            }
        },

        // ============================================
        // AMBIENT MUSIC SYSTEM
        // ============================================
        // Stratum-specific ambient drone loops
        _ambientNodes: null,
        _ambientStratumId: -1,
        _ambientInterval: null,

        // Chord definitions per stratum (darker as you go deeper)
        _stratumChords: [
            // Surface: warm major (C E G)
            { notes: [131, 165, 196], type: 'sine', vol: 0.03, detune: 3 },
            // The Dump: minor dissonance (C Eb Gb)
            { notes: [131, 156, 185], type: 'sine', vol: 0.025, detune: 5 },
            // The Foundation: deep industrial (low C octave + 5th)
            { notes: [65, 82, 98], type: 'triangle', vol: 0.025, detune: 8 },
            // The Catacombs: dark atonal (low tritone cluster)
            { notes: [55, 62, 78], type: 'sine', vol: 0.02, detune: 12 },
            // The Ruin: ominous resonance (very low + overtone)
            { notes: [41, 49, 62, 82], type: 'sine', vol: 0.02, detune: 6 }
        ],

        startAmbientMusic: function(stratumId) {
            if (!enabled) return;
            stratumId = stratumId || 0;
            if (stratumId === this._ambientStratumId && this._ambientNodes) return;

            this.stopAmbientMusic();
            var c = getCtx();
            if (!c) return;

            this._ambientStratumId = stratumId;
            var chord = this._stratumChords[Math.min(stratumId, this._stratumChords.length - 1)];
            this._ambientNodes = [];

            var t = c.currentTime;

            // Create sustained drone chord
            for (var i = 0; i < chord.notes.length; i++) {
                var osc = c.createOscillator();
                var gain = c.createGain();
                osc.type = chord.type;
                osc.frequency.setValueAtTime(chord.notes[i], t);
                // Slight detune for warmth
                osc.detune.setValueAtTime((i - 1) * chord.detune, t);
                gain.gain.setValueAtTime(0, t);
                gain.gain.linearRampToValueAtTime(chord.vol, t + 2);
                osc.connect(gain);
                gain.connect(masterGain);
                osc.start(t);
                this._ambientNodes.push({ osc: osc, gain: gain });
            }

            // Slow LFO to modulate volume for breathing feel
            var self = this;
            this._ambientInterval = setInterval(function() {
                if (!self._ambientNodes || !enabled) {
                    self.stopAmbientMusic();
                    return;
                }
                var c2 = getCtx();
                if (!c2) return;
                var now = c2.currentTime;
                for (var j = 0; j < self._ambientNodes.length; j++) {
                    var node = self._ambientNodes[j];
                    var breathVol = chord.vol * (0.7 + 0.3 * Math.sin(now * 0.3 + j * 0.5));
                    node.gain.gain.linearRampToValueAtTime(breathVol, now + 0.5);
                }
            }, 500);
        },

        stopAmbientMusic: function() {
            if (this._ambientInterval) {
                clearInterval(this._ambientInterval);
                this._ambientInterval = null;
            }
            if (this._ambientNodes) {
                var c = getCtx();
                var t = c ? c.currentTime : 0;
                for (var i = 0; i < this._ambientNodes.length; i++) {
                    var node = this._ambientNodes[i];
                    try {
                        node.gain.gain.linearRampToValueAtTime(0, t + 1);
                        node.osc.stop(t + 1.1);
                    } catch (e) { /* already stopped */ }
                }
                this._ambientNodes = null;
            }
            this._ambientStratumId = -1;
        }
    };

})(window.STRATA);
