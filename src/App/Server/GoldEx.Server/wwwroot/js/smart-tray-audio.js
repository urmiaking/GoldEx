// GoldEx Smart Showcase Tray - Zero-Latency Web Audio API Synthesizer
(function () {
    let audioCtx = null;

    function getAudioContext() {
        if (!audioCtx) {
            const AudioContextClass = window.AudioContext || window.webkitAudioContext;
            if (AudioContextClass) {
                audioCtx = new AudioContextClass();
            }
        }
        if (audioCtx && audioCtx.state === 'suspended') {
            audioCtx.resume();
        }
        return audioCtx;
    }

    function playTone(freq, type, durationMs, gainLevel = 0.2, startTimeOffset = 0) {
        try {
            const ctx = getAudioContext();
            if (!ctx) return;

            const osc = ctx.createOscillator();
            const gain = ctx.createGain();

            osc.type = type;
            osc.frequency.setValueAtTime(freq, ctx.currentTime + startTimeOffset);

            gain.gain.setValueAtTime(gainLevel, ctx.currentTime + startTimeOffset);
            gain.gain.exponentialRampToValueAtTime(0.0001, ctx.currentTime + startTimeOffset + (durationMs / 1000));

            osc.connect(gain);
            gain.connect(ctx.destination);

            osc.start(ctx.currentTime + startTimeOffset);
            osc.stop(ctx.currentTime + startTimeOffset + (durationMs / 1000));
        } catch (e) {
            console.debug('WebAudio playback error:', e);
        }
    }

    window.GoldExSmartTrayAudio = {
        // High crisp double-beep on item scan (C6 -> E6)
        playScanSuccess: function () {
            playTone(1046.5, 'sine', 65, 0.25, 0);
            playTone(1318.5, 'sine', 75, 0.25, 0.07);
            if (navigator.vibrate) {
                navigator.vibrate([40]);
            }
        },

        // Mellow return tone when returning gold to showcase (A5 -> E5)
        playReturnSuccess: function () {
            playTone(880.0, 'sine', 80, 0.22, 0);
            playTone(659.25, 'triangle', 110, 0.20, 0.08);
            if (navigator.vibrate) {
                navigator.vibrate([30, 20, 30]);
            }
        },

        // Warning tone (caution / duplicate scan)
        playWarning: function () {
            playTone(330.0, 'sawtooth', 140, 0.22, 0);
            if (navigator.vibrate) {
                navigator.vibrate([80, 50, 80]);
            }
        },

        // Anti-theft missing item alarm (alternating square wave siren)
        playDiscrepancyAlarm: function () {
            const ctx = getAudioContext();
            if (!ctx) return;

            for (let i = 0; i < 3; i++) {
                playTone(440.0, 'square', 100, 0.28, i * 0.25);
                playTone(880.0, 'square', 100, 0.28, i * 0.25 + 0.11);
            }
            if (navigator.vibrate) {
                navigator.vibrate([200, 100, 200, 100, 300]);
            }
        },

        // Victory chord when tray audit successfully closes (C5 -> E5 -> G5 -> C6)
        playTrayCompleted: function () {
            playTone(523.25, 'triangle', 120, 0.2, 0.00);
            playTone(659.25, 'triangle', 120, 0.2, 0.10);
            playTone(783.99, 'triangle', 120, 0.2, 0.20);
            playTone(1046.5, 'sine', 280, 0.25, 0.30);
            if (navigator.vibrate) {
                navigator.vibrate([60, 40, 100]);
            }
        }
    };
})();
