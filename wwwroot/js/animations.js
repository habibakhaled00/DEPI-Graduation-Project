/*!
 * NeighborHelp – Animation System v2.0
 * Particles · Scroll Reveal · Counter · Ripple · Cursor · Transitions
 */
(function () {
    'use strict';

    /* ─────────────────────────────────────────────────────────────
     * 1. FLOATING PARTICLE SYSTEM
     * ───────────────────────────────────────────────────────────── */
    function initParticles() {
        const container = document.getElementById('nh-particles');
        if (!container) return;

        const COLORS = [
            [108, 99, 255],   // purple
            [0,  212, 170],   // teal
            [255,107, 107],   // coral
            [255,170,   0],   // amber
            [6,  182, 212],   // cyan
        ];

        // Fewer particles on small screens for performance
        const count = Math.min(28, Math.max(12, Math.floor(window.innerWidth / 55)));

        for (let i = 0; i < count; i++) {
            const p    = document.createElement('span');
            p.className = 'nh-particle';

            const size    = (Math.random() * 5 + 2).toFixed(1);
            const [r,g,b] = COLORS[Math.floor(Math.random() * COLORS.length)];
            const alpha   = (Math.random() * 0.35 + 0.08).toFixed(2);
            const dur     = (Math.random() * 22 + 16).toFixed(1);
            const delay   = (Math.random() * -25).toFixed(1);
            const left    = (Math.random() * 100).toFixed(1);
            const drift   = (Math.random() * 120 - 60).toFixed(0); // horizontal drift

            p.style.cssText = [
                `width:${size}px`,
                `height:${size}px`,
                `left:${left}%`,
                `background:rgba(${r},${g},${b},${alpha})`,
                `animation-duration:${dur}s`,
                `animation-delay:${delay}s`,
                `--drift:${drift}px`,
            ].join(';');

            container.appendChild(p);
        }
    }

    /* ─────────────────────────────────────────────────────────────
     * 2. SCROLL REVEAL  (Intersection Observer)
     * ───────────────────────────────────────────────────────────── */
    function initScrollReveal() {
        const targets = document.querySelectorAll(
            '.sr, .sr-left, .sr-right, .sr-scale, .sr-fast'
        );
        if (!targets.length) return;

        const io = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('sr-in');
                    io.unobserve(entry.target);
                }
            });
        }, { threshold: 0.1, rootMargin: '0px 0px -30px 0px' });

        targets.forEach(el => io.observe(el));
    }

    /* ─────────────────────────────────────────────────────────────
     * 3. ANIMATED NUMBER COUNTER
     * ───────────────────────────────────────────────────────────── */
    function countUp(el, target, duration) {
        const start   = performance.now();
        const isFloat = String(target).includes('.');

        const tick = (now) => {
            const t = Math.min((now - start) / duration, 1);
            // Ease-out quart
            const eased = 1 - Math.pow(1 - t, 4);
            el.textContent = isFloat
                ? (eased * target).toFixed(1)
                : Math.round(eased * target);
            if (t < 1) requestAnimationFrame(tick);
        };
        requestAnimationFrame(tick);
    }

    function initCounters() {
        const io = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (!entry.isIntersecting) return;
                const el  = entry.target;
                const raw = parseFloat(el.dataset.target || el.textContent);
                if (isNaN(raw)) return;
                el.dataset.target = raw;
                el.textContent = '0';
                countUp(el, raw, 1600);
                io.unobserve(el);
            });
        }, { threshold: 0.6 });

        document.querySelectorAll('.count-up').forEach(el => {
            const val = parseFloat(el.textContent);
            if (!isNaN(val)) {
                el.dataset.target = val;
                io.observe(el);
            }
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 4. BUTTON RIPPLE EFFECT
     * ───────────────────────────────────────────────────────────── */
    function initRipple() {
        document.addEventListener('click', (e) => {
            const btn = e.target.closest(
                '.btn-nh-primary, .btn-primary, .btn-success, [data-ripple]'
            );
            if (!btn) return;

            const rect   = btn.getBoundingClientRect();
            const size   = Math.max(rect.width, rect.height) * 2;
            const x      = e.clientX - rect.left - size / 2;
            const y      = e.clientY - rect.top  - size / 2;

            const wave   = document.createElement('span');
            wave.style.cssText = `
                position:absolute;
                width:${size}px;height:${size}px;
                left:${x}px;top:${y}px;
                background:rgba(255,255,255,0.2);
                border-radius:50%;
                transform:scale(0);
                animation:nhRipple .6s ease-out forwards;
                pointer-events:none;
            `;
            const prevPos = btn.style.position;
            btn.style.position = 'relative';
            btn.style.overflow = 'hidden';
            btn.appendChild(wave);
            wave.addEventListener('animationend', () => {
                wave.remove();
                if (!prevPos) btn.style.position = '';
            });
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 5. STAGGER ANIMATION on .stagger children
     * ───────────────────────────────────────────────────────────── */
    function initStagger() {
        document.querySelectorAll('.stagger').forEach(container => {
            Array.from(container.children).forEach((child, i) => {
                child.style.animationDelay = `${(i * 70)}ms`;
                if (!child.classList.contains('fade-up')) {
                    child.classList.add('fade-up');
                }
            });
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 6. TOP PROGRESS BAR
     * ───────────────────────────────────────────────────────────── */
    function initProgressBar() {
        const bar = document.getElementById('nh-progress');
        if (!bar) return;

        let w = 0;
        const iv = setInterval(() => {
            w = Math.min(w + Math.random() * 12, 88);
            bar.style.width = w + '%';
        }, 120);

        window.addEventListener('load', () => {
            clearInterval(iv);
            bar.style.width = '100%';
            setTimeout(() => { bar.style.opacity = '0'; }, 400);
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 7. CARD TILT EFFECT  (subtle 3-D on mouse move)
     * ───────────────────────────────────────────────────────────── */
    function initTilt() {
        // Only on devices that support hover (not touch)
        if (window.matchMedia('(hover: none)').matches) return;

        document.querySelectorAll('.tilt-card').forEach(card => {
            card.addEventListener('mousemove', (e) => {
                const rect  = card.getBoundingClientRect();
                const x     = e.clientX - rect.left - rect.width  / 2;
                const y     = e.clientY - rect.top  - rect.height / 2;
                const rx    = (-y / rect.height * 8).toFixed(2);
                const ry    = ( x / rect.width  * 8).toFixed(2);
                card.style.transform = `perspective(600px) rotateX(${rx}deg) rotateY(${ry}deg) translateY(-4px)`;
            });
            card.addEventListener('mouseleave', () => {
                card.style.transform = '';
            });
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 8. GLOWING CURSOR TRAIL  (subtle, performance-safe)
     * ───────────────────────────────────────────────────────────── */
    function initCursorGlow() {
        if (window.matchMedia('(hover: none)').matches) return;
        if (window.innerWidth < 900) return; // skip on mobile

        const glow = document.createElement('div');
        glow.id = 'nh-cursor-glow';
        glow.style.cssText = `
            position:fixed;width:400px;height:400px;
            border-radius:50%;pointer-events:none;
            background:radial-gradient(circle,rgba(108,99,255,0.06) 0%,transparent 70%);
            transform:translate(-50%,-50%);
            transition:left .12s ease,top .12s ease;
            z-index:0;
        `;
        document.body.appendChild(glow);

        document.addEventListener('mousemove', (e) => {
            glow.style.left = e.clientX + 'px';
            glow.style.top  = e.clientY + 'px';
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 9. SCROLL-TO-TOP BUTTON
     * ───────────────────────────────────────────────────────────── */
    function initScrollTop() {
        const btn = document.getElementById('nh-scroll-top');
        if (!btn) return;

        window.addEventListener('scroll', () => {
            btn.classList.toggle('visible', window.scrollY > 400);
        }, { passive: true });

        btn.addEventListener('click', () => {
            window.scrollTo({ top: 0, behavior: 'smooth' });
        });
    }

    /* ─────────────────────────────────────────────────────────────
     * 10. TOAST NOTIFICATION HELPER  (global)
     * ───────────────────────────────────────────────────────────── */
    window.nhToast = function (message, type = 'success') {
        const t     = document.createElement('div');
        const color = type === 'success' ? '#00d4aa' : type === 'error' ? '#ff6b6b' : '#ffaa00';
        const icon  = type === 'success' ? 'check-circle' : type === 'error' ? 'x-circle' : 'exclamation-circle';

        t.style.cssText = `
            position:fixed;bottom:28px;right:28px;z-index:9999;
            background:rgba(13,15,26,0.95);
            border:1px solid ${color};
            color:${color};
            padding:.75rem 1.2rem;border-radius:12px;
            font-size:.875rem;font-weight:500;
            backdrop-filter:blur(16px);
            display:flex;align-items:center;gap:.6rem;
            box-shadow:0 8px 32px rgba(0,0,0,0.4);
            animation:nhToastIn .35s cubic-bezier(.34,1.56,.64,1) both;
        `;
        t.innerHTML = `<i class="bi bi-${icon}"></i><span>${message}</span>`;
        document.body.appendChild(t);

        setTimeout(() => {
            t.style.animation = 'nhToastOut .3s ease forwards';
            t.addEventListener('animationend', () => t.remove());
        }, 3200);
    };

    /* ─────────────────────────────────────────────────────────────
     * 10b. CONFIRMATION DIALOG HELPER  (global)
     * ───────────────────────────────────────────────────────────── */
    window.nhConfirm = function (message) {
        return new Promise((resolve) => {
            if (!document.getElementById('nh-confirm-styles')) {
                const style = document.createElement('style');
                style.id = 'nh-confirm-styles';
                style.innerHTML = `
                    @keyframes nhFadeIn { from { opacity: 0; } to { opacity: 1; } }
                    @keyframes nhFadeOut { from { opacity: 1; } to { opacity: 0; } }
                    @keyframes nhSlideUp { from { transform: translateY(20px); opacity: 0; } to { transform: translateY(0); opacity: 1; } }
                    @keyframes nhSlideDown { from { transform: translateY(0); opacity: 1; } to { transform: translateY(20px); opacity: 0; } }
                `;
                document.head.appendChild(style);
            }

            const backdrop = document.createElement('div');
            backdrop.style.cssText = `
                position: fixed; top: 0; left: 0; width: 100%; height: 100%; z-index: 99999;
                background: rgba(13, 15, 26, 0.7);
                backdrop-filter: blur(8px); -webkit-backdrop-filter: blur(8px);
                display: flex; align-items: center; justify-content: center;
                animation: nhFadeIn 0.2s ease forwards;
            `;

            const modal = document.createElement('div');
            modal.style.cssText = `
                background: var(--bg-card);
                border: 1px solid var(--border);
                border-radius: var(--radius);
                padding: 2.2rem 2rem;
                max-width: 420px;
                width: 90%;
                text-align: center;
                box-shadow: var(--shadow);
                animation: nhSlideUp 0.3s cubic-bezier(0.34, 1.56, 0.64, 1) both;
            `;

            modal.innerHTML = `
                <div class="mb-3" style="font-size: 2.8rem; color: var(--accent-3); display: flex; justify-content: center; align-items: center;">
                    <i class="bi bi-question-circle-fill"></i>
                </div>
                <h5 class="mb-4" style="color: var(--text-primary); font-weight: 700; line-height: 1.45; font-size: 1.1rem; margin: 0;">${message}</h5>
                <div class="d-flex gap-2 justify-content-center mt-4">
                    <button class="btn btn-outline-secondary px-4 py-2" id="nhConfirmCancel" style="border-radius: var(--radius-sm); font-weight: 600; font-size: 0.88rem;">Cancel</button>
                    <button class="btn btn-primary px-4 py-2" id="nhConfirmOk" style="background: var(--accent); border-color: var(--accent); border-radius: var(--radius-sm); font-weight: 600; font-size: 0.88rem;">Confirm</button>
                </div>
            `;

            backdrop.appendChild(modal);
            document.body.appendChild(backdrop);

            const cancelBtn = modal.querySelector('#nhConfirmCancel');
            const okBtn = modal.querySelector('#nhConfirmOk');

            const close = (result) => {
                backdrop.style.animation = 'nhFadeOut 0.2s ease forwards';
                modal.style.animation = 'nhSlideDown 0.2s ease forwards';
                backdrop.addEventListener('animationend', () => {
                    backdrop.remove();
                    resolve(result);
                });
            };

            cancelBtn.addEventListener('click', () => close(false));
            okBtn.addEventListener('click', () => close(true));
        });
    };


    /* ─────────────────────────────────────────────────────────────
     * INIT
     * ───────────────────────────────────────────────────────────── */
    document.addEventListener('DOMContentLoaded', () => {
        initParticles();
        initScrollReveal();
        initCounters();
        initRipple();
        initStagger();
        initProgressBar();
        initTilt();
        initCursorGlow();
        initScrollTop();
    });

})();
