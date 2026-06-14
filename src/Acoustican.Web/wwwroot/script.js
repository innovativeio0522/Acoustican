document.addEventListener('DOMContentLoaded', async () => {
    const API_URL = '/api';

    // ===== COURSE MODULE TOGGLE =====
    function setupCourseModules() {
        console.log("setupCourseModules running!");
        const moduleHeaders = document.querySelectorAll('.module-header');
        const expandAllBtn = document.querySelector('.expand-all-btn');
        let allOpen = false;

        moduleHeaders.forEach(header => {
            header.addEventListener('click', () => {
                console.log("Module header clicked!");
                const module = header.closest('.course-module');
                module.classList.toggle('open');
            });
        });

        if (expandAllBtn) {
            expandAllBtn.addEventListener('click', () => {
                allOpen = !allOpen;
                const modules = document.querySelectorAll('.course-module');
                modules.forEach(module => {
                    module.classList.toggle('open', allOpen);
                });
                expandAllBtn.textContent = allOpen ? 'Collapse all sections' : 'Expand all sections';
            });
        }
    }

    setupCourseModules();

    // ===== DYNAMIC CONTENT LOADING (Progressive Enhancement) =====
    // If content is already server-rendered by Razor, skip the API call.
    async function loadHeroContent() {
        // Check if hero content is already rendered server-side
        const titleEl = document.querySelector('.hero-title');
        if (titleEl && titleEl.textContent.trim().length > 0) {
            // SSR content already present — skip API call
            return;
        }

        try {
            const response = await fetch(`${API_URL}/hero`);
            if (!response.ok) return;
            const hero = await response.json();

            const descEl = document.querySelector('.hero-desc');
            const videoSource = document.querySelector('.hero-bg-video source');
            const videoEl = document.querySelector('.hero-bg-video');
            const primaryBtn = document.querySelector('.hero-buttons .btn-primary');
            const secondaryBtn = document.querySelector('.hero-buttons .btn-secondary');

            if (titleEl) titleEl.innerHTML = `${hero.title} <span>${hero.subtitle}</span>`;
            if (descEl) descEl.textContent = hero.description;
            if (primaryBtn) {
                const btnText = primaryBtn.childNodes[0];
                if (btnText.nodeType === Node.TEXT_NODE) btnText.textContent = hero.primaryButtonText + ' ';
            }
            if (secondaryBtn) {
                const btnText = secondaryBtn.childNodes[1]; // After the SVG
                if (btnText && btnText.nodeType === Node.TEXT_NODE) btnText.textContent = ' ' + hero.secondaryButtonText;
            }

            if (hero.backgroundVideoUrl && videoSource && videoEl) {
                videoSource.src = hero.backgroundVideoUrl;
                videoEl.load();
            }
        } catch (err) {
            console.error('Error loading dynamic hero:', err);
        }
    }

    async function loadDynamicCourses() {
        // Check if courses are already server-rendered
        const track = document.querySelector('.mpw-track');
        const existingSlides = track ? track.querySelectorAll('.mpw-slide') : [];
        if (existingSlides.length > 0) {
            // SSR content already present — just set up the widget
            setupModuleWidget();
            if (typeof window.updateModuleProgress === 'function') {
                window.updateModuleProgress();
            }
            return;
        }

        try {
            const response = await fetch(`${API_URL}/courses`);
            if (!response.ok) return;
            const courses = await response.json();
            const publishedCourses = courses.filter(c => c.isPublished);

            const currentNum = document.querySelector('.mpw-current-num');
            const countBadge = document.querySelector('.mpw-count-badge');

            if (track && publishedCourses.length > 0) {
                track.innerHTML = '';
                publishedCourses.forEach((course, index) => {
                    const slide = document.createElement('div');
                    slide.className = `mpw-slide ${index === 0 ? 'active' : ''}`;
                    slide.innerHTML = `
                        <div class="mpw-thumb-wrap">
                            <img src="${course.thumbnailUrl || 'https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&h=340&fit=crop&q=80'}" alt="${course.title}" class="mpw-thumb">
                            <div class="mpw-thumb-overlay">
                                <button class="mpw-play-btn" aria-label="Play module">
                                    <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor"><polygon points="5 3 19 12 5 21 5 3"/></svg>
                                </button>
                                <span class="mpw-duration">${Math.floor(course.durationMinutes / 60)}h ${course.durationMinutes % 60}m</span>
                            </div>
                        </div>
                        <div class="mpw-info">
                            <div class="mpw-module-meta">
                                <span class="mpw-module-num">Course ${String(index + 1).padStart(2, '0')}</span>
                                <span class="mpw-difficulty ${course.level.toLowerCase()}">${course.level}</span>
                            </div>
                            <h3 class="mpw-title">${course.title}</h3>
                            <p class="mpw-desc">${course.description.length > 100 ? course.description.substring(0, 100) + '...' : course.description}</p>
                            <div class="mpw-stats">
                                <span>📚 ${course.reviewCount || 0} reviews</span>
                                <span>⏱ ${course.durationMinutes}m</span>
                                <span>⭐ ${course.rating || '0.0'}</span>
                            </div>
                            </div>
                        </div>
                    `;
                    track.appendChild(slide);
                });

                if (countBadge) countBadge.innerHTML = `<span class="mpw-current-num">1</span> / ${publishedCourses.length}`;
                
                // Re-initialize slider if needed or call widget setup
                setupModuleWidget();
                if (typeof window.updateModuleProgress === 'function') {
                    window.updateModuleProgress();
                }
            }
        } catch (err) {
            console.error('Error loading dynamic courses:', err);
        }
    }

    async function loadDynamicTestimonials() {
        // Check if testimonials are already server-rendered
        const track = document.querySelector('.testimonials-track');
        const existingCards = track ? track.querySelectorAll('.testimonial-card') : [];
        if (existingCards.length > 0) {
            // SSR content already present — just set up the carousel
            setupTestimonialCarousel();
            return;
        }

        try {
            const response = await fetch(`${API_URL}/testimonials/published`);
            if (!response.ok) return;
            const testimonials = await response.json();

            if (track && testimonials.length > 0) {
                track.innerHTML = '';
                testimonials.forEach((t, index) => {
                    const slide = document.createElement('div');
                    slide.className = `testimonial-slide ${index === 0 ? 'active' : ''}`;
                    slide.innerHTML = `
                        <div class="testimonial-content">
                            <div class="testimonial-rating">
                                ${Array(5).fill(0).map((_, i) => `<span class="star ${i < t.rating ? 'filled' : ''}">★</span>`).join('')}
                            </div>
                            <p class="testimonial-text">"${t.content}"</p>
                            <div class="testimonial-author">
                                <div class="author-avatar">
                                    <img src="${t.studentImageUrl || 'https://ui-avatars.com/api/?name=' + encodeURIComponent(t.studentName)}" alt="${t.studentName}">
                                </div>
                                <div class="author-info">
                                    <h4 class="author-name">${t.studentName}</h4>
                                    <p class="author-role">${t.studentRole}</p>
                                </div>
                            </div>
                        </div>
                    `;
                    track.appendChild(slide);
                });
                setupTestimonialCarousel();
            }
        } catch (err) {
            console.error('Error loading dynamic testimonials:', err);
        }
    }

    // Load everything (skips API calls when SSR content is present)
    await loadHeroContent();
    await loadDynamicCourses();
    await loadDynamicTestimonials();

    function initSyllabusToggles() {
        document.querySelectorAll('.cp-card-syllabus-toggle').forEach(toggle => {
            const newToggle = toggle.cloneNode(true);
            toggle.parentNode.replaceChild(newToggle, toggle);
            newToggle.addEventListener('click', (e) => {
                e.stopPropagation();
                newToggle.classList.toggle('active');
                const syllabus = newToggle.nextElementSibling;
                if (syllabus) {
                    syllabus.classList.toggle('active');
                    if (syllabus.classList.contains('active')) {
                        syllabus.style.maxHeight = syllabus.scrollHeight + 'px';
                    } else {
                        syllabus.style.maxHeight = '0';
                    }
                }
            });
        });
    }
    initSyllabusToggles();

    // ===== NAVBAR SCROLL EFFECT =====
    const navbar = document.querySelector('.navbar');
    let lastScroll = 0;

    window.addEventListener('scroll', () => {
        const currentScroll = window.pageYOffset;

        if (currentScroll > 60) {
            navbar.classList.add('scrolled');
        } else {
            navbar.classList.remove('scrolled');
        }

        // Hide navbar on scroll down, show on scroll up
        if (currentScroll > lastScroll && currentScroll > 300) {
            navbar.classList.add('nav-hidden');
        } else {
            navbar.classList.remove('nav-hidden');
        }
        lastScroll = currentScroll;
    });

    // ===== MOBILE NAVIGATION MENU =====
    const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
    const navLinks = document.querySelector('.nav-links');

    if (mobileMenuBtn) {
        mobileMenuBtn.addEventListener('click', () => {
            navbar.classList.toggle('mobile-menu-active');
            if (navbar.classList.contains('mobile-menu-active')) {
                mobileMenuBtn.innerHTML = '✕';
            } else {
                mobileMenuBtn.innerHTML = '☰';
            }
        });
    }

    // Close mobile menu when a nav link is clicked
    if (navLinks) {
        navLinks.querySelectorAll('a').forEach(link => {
            link.addEventListener('click', () => {
                navbar.classList.remove('mobile-menu-active');
                if (mobileMenuBtn) mobileMenuBtn.innerHTML = '☰';
            });
        });
    }

    // ===== GUITAR ICON HOVER ANIMATION =====
    const logo = document.querySelector('.logo');
    const logoIcon = document.querySelector('.logo-icon');
    
    if (logo && logoIcon) {
        logo.addEventListener('mouseenter', () => {
            // Restart animation by removing and re-adding
            logoIcon.style.animation = 'none';
            // Force reflow
            void logoIcon.offsetWidth;
            logoIcon.style.animation = 'logoStrum 0.5s cubic-bezier(0.34, 1.56, 0.64, 1) forwards';
        });
        
        logo.addEventListener('mouseleave', () => {
            // Return to idle state after animation completes
            setTimeout(() => {
                logoIcon.style.animation = '';
            }, 500);
        });
    }

    // ===== GSAP HERO ENTRANCE ANIMATION =====
    if (typeof gsap !== 'undefined' && typeof ScrollTrigger !== 'undefined') {
        try {
            gsap.registerPlugin(ScrollTrigger);

            // Mark body so CSS can hide .reveal elements (only when GSAP is available)
            document.body.classList.add('gsap-ready');

            const tl = gsap.timeline({ defaults: { ease: 'power4.out', duration: 1.2 } });

            // Set initial states to prevent flash of unstyled content
            gsap.set('.navbar', { y: -80, opacity: 0 });
            gsap.set('.hero-bg-video', { opacity: 0 });
            gsap.set('.hero-badge', { scale: 0.6, opacity: 0, y: 30 });
            gsap.set('.hero-title', { y: 50, opacity: 0, skewY: 2 });
            gsap.set('.hero-desc', { y: 30, opacity: 0 });
            gsap.set('.hero-buttons a', { y: 20, opacity: 0 });
            gsap.set('.hero-visual', { scale: 0.9, opacity: 0, y: 30 });

            // Build hero entrance timeline
            tl.to('.hero-bg-video', { opacity: 0.35, duration: 2, ease: 'power2.out' }, 0)
              .to('.navbar', { y: 0, opacity: 1, duration: 1 }, 0.2)
              .to('.hero-badge', { scale: 1, opacity: 1, y: 0, duration: 1, ease: 'back.out(2)' }, 0.4)
              .to('.hero-title', { y: 0, opacity: 1, skewY: 0, duration: 1 }, 0.5)
              .to('.hero-desc', { y: 0, opacity: 1, duration: 1 }, 0.6)
              .to('.hero-buttons a', { y: 0, opacity: 1, stagger: 0.15, duration: 0.8, ease: 'back.out(1.5)' }, 0.7)
              .to('.hero-visual', { scale: 1, opacity: 1, y: 0, duration: 1.4, ease: 'power3.out' }, 0.5);

            // ===== GSAP SCROLL REVEAL ANIMATIONS =====
            // Individual reveal elements
            const reveals = gsap.utils.toArray('.reveal');
            reveals.forEach(elem => {
                if (elem.classList.contains('hero')) return;

                gsap.fromTo(elem, 
                    { opacity: 0, y: 40 },
                    {
                        opacity: 1,
                        y: 0,
                        duration: 1,
                        ease: 'power3.out',
                        clearProps: 'transform',
                        scrollTrigger: {
                            trigger: elem,
                            start: 'top 95%',
                            toggleActions: 'play none none none'
                        }
                    }
                );
            });

            // Stagger reveal elements
            const staggers = gsap.utils.toArray('.reveal-stagger');
            staggers.forEach(container => {
                const children = gsap.utils.toArray(container.children);
                if (children.length > 0) {
                    gsap.fromTo(children, 
                        { opacity: 0, y: 30 },
                        {
                            opacity: 1,
                            y: 0,
                            duration: 0.8,
                            stagger: 0.15,
                            ease: 'power3.out',
                            clearProps: 'transform',
                            scrollTrigger: {
                                trigger: container,
                                start: 'top 95%',
                                toggleActions: 'play none none none'
                            }
                        }
                    );
                }
            });

            // Refresh ScrollTrigger calculations after everything has fully loaded
            window.addEventListener('load', () => {
                ScrollTrigger.refresh();
            });

        } catch (e) {
            console.warn('GSAP animation failed, showing content without animation:', e);
            document.body.classList.remove('gsap-ready');
        }
    } else {
        console.warn('GSAP not loaded — hero content will display without animation.');
    }

    // ===== BUTTON RIPPLE EFFECT =====
    document.querySelectorAll('.btn-primary, .btn-secondary, .pricing-btn, .course-btn, .nav-cta').forEach(btn => {
        btn.addEventListener('click', function (e) {
            const ripple = document.createElement('span');
            ripple.classList.add('btn-ripple');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            ripple.style.width = ripple.style.height = `${size}px`;
            ripple.style.left = `${e.clientX - rect.left - size / 2}px`;
            ripple.style.top = `${e.clientY - rect.top - size / 2}px`;
            this.appendChild(ripple);
            ripple.addEventListener('animationend', () => ripple.remove());
        });
    });

    // ===== DYNAMIC 3D TILT & GLOW EFFECTS ON CARDS =====
    // Uses document-level pointermove to avoid the mouseleave/mouseenter feedback
    // loop that causes jitter when hovering from below (card moves → cursor exits →
    // card resets → cursor re-enters → repeat).
    const interactiveCards = document.querySelectorAll('.course-card, .feature-card, .pricing-card, .testimonial-card');

    interactiveCards.forEach(card => {
        let rafId        = null;
        let isActive     = false;
        let leaveTimeout = null;

        function applyTilt(clientX, clientY) {
            const rect    = card.getBoundingClientRect();
            const x       = clientX - rect.left;
            const y       = clientY - rect.top;
            const centerX = rect.width  / 2;
            const centerY = rect.height / 2;
            const rotateX = Math.max(-6, Math.min(6, -((y - centerY) / 25)));
            const rotateY = Math.max(-6, Math.min(6,  ((x - centerX) / 25)));

            card.style.setProperty('--mouse-x', `${x}px`);
            card.style.setProperty('--mouse-y', `${y}px`);
            card.style.transform = `perspective(1000px) rotateX(${rotateX}deg) rotateY(${rotateY}deg)`;
        }

        function resetCard() {
            isActive = false;
            card.style.transition = 'transform 0.5s cubic-bezier(0.16, 1, 0.3, 1)';
            card.style.transform  = '';
            card.style.setProperty('--mouse-x', '-1000px');
            card.style.setProperty('--mouse-y', '-1000px');
        }

        // Track pointer at document level — card geometry never influences hit-testing
        document.addEventListener('pointermove', (e) => {
            if (rafId) return;
            rafId = requestAnimationFrame(() => {
                rafId = null;
                const rect = card.getBoundingClientRect();
                // Add a small inset (-4 px) so the card edge doesn't count as "inside"
                const inside = e.clientX >= rect.left   + 4 &&
                               e.clientX <= rect.right  - 4 &&
                               e.clientY >= rect.top    + 4 &&
                               e.clientY <= rect.bottom - 4;

                if (inside) {
                    // Cancel any pending leave reset
                    if (leaveTimeout) { clearTimeout(leaveTimeout); leaveTimeout = null; }

                    if (!isActive) {
                        // First frame inside — kill transition for instant tracking
                        isActive = true;
                        card.style.transition = 'none';
                    }
                    applyTilt(e.clientX, e.clientY);
                } else if (isActive) {
                    // Debounce the leave by one frame to avoid edge-pixel flicker
                    if (!leaveTimeout) {
                        leaveTimeout = setTimeout(() => {
                            leaveTimeout = null;
                            resetCard();
                        }, 16);
                    }
                }
            });
        });
    });

    // ===== PROGRESS BAR SCROLL ANIMATION =====
    const progressBars = document.querySelectorAll('.progress-fill');
    const progressObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const bar = entry.target;
                const targetWidth = bar.getAttribute('data-width') || bar.style.width || '0%';
                bar.style.width = '0%';
                requestAnimationFrame(() => {
                    bar.style.transition = 'width 1.2s cubic-bezier(0.16, 1, 0.3, 1)';
                    bar.style.width = targetWidth;
                });
                progressObserver.unobserve(bar);
            }
        });
    }, { threshold: 0.5 });

    progressBars.forEach(bar => {
        bar.setAttribute('data-width', bar.style.width || '0%');
        bar.style.width = '0%';
        progressObserver.observe(bar);
    });

    // ===== TESTIMONIALS CAROUSEL (with auto-rotate) =====
    function setupTestimonialCarousel() {
        const track = document.querySelector('.testimonials-track');
        const tNavPrev = document.querySelector('.testimonial-nav button:first-child');
        const tNavNext = document.querySelector('.testimonial-nav button:last-child');
        let autoRotateInterval;

        if (track && tNavPrev && tNavNext) {
            let currentSlide = 0;
            const slides = Array.from(track.children);

            function moveToSlide(index) {
                if (index < 0) index = slides.length - 1;
                if (index >= slides.length) index = 0;

                currentSlide = index;
                track.style.transform = `translateX(-${currentSlide * 100}%)`;

                slides.forEach((slide, idx) => {
                    slide.classList.toggle('active', idx === currentSlide);
                });
            }

            // Remove old listeners to avoid duplicates
            const newPrev = tNavPrev.cloneNode(true);
            const newNext = tNavNext.cloneNode(true);
            tNavPrev.parentNode.replaceChild(newPrev, tNavPrev);
            tNavNext.parentNode.replaceChild(newNext, tNavNext);

            newPrev.addEventListener('click', () => {
                moveToSlide(currentSlide - 1);
                resetAutoRotate();
            });

            newNext.addEventListener('click', () => {
                moveToSlide(currentSlide + 1);
                resetAutoRotate();
            });

            moveToSlide(0);

            function startAutoRotate() {
                clearInterval(autoRotateInterval);
                autoRotateInterval = setInterval(() => moveToSlide(currentSlide + 1), 6000);
            }

            function resetAutoRotate() {
                clearInterval(autoRotateInterval);
                startAutoRotate();
            }

            startAutoRotate();
        }
    }

    // ===== MODULE PREVIEW WIDGET SLIDER =====
    function setupModuleWidget() {
        const mpwWidget   = document.querySelector('.module-preview-widget');
        if (mpwWidget) {
            const track       = mpwWidget.querySelector('.mpw-track');
            const slides      = Array.from(mpwWidget.querySelectorAll('.mpw-slide'));
            const dotsContainer = mpwWidget.querySelector('.mpw-dots');
            const prevBtn     = mpwWidget.querySelector('.mpw-prev');
            const nextBtn     = mpwWidget.querySelector('.mpw-next');
            const currentNum  = mpwWidget.querySelector('.mpw-current-num');
            const total       = slides.length;
            let current       = 0;

            // Clear dots
            dotsContainer.innerHTML = '';

            // Build dots
            slides.forEach((_, i) => {
                const dot = document.createElement('button');
                dot.className = 'mpw-dot' + (i === 0 ? ' active' : '');
                dot.setAttribute('aria-label', `Go to module ${i + 1}`);
                dot.addEventListener('click', () => goTo(i));
                dotsContainer.appendChild(dot);
            });

            const dots = Array.from(dotsContainer.querySelectorAll('.mpw-dot'));

            function goTo(index) {
                slides[current].classList.remove('active');
                dots[current].classList.remove('active');

                current = (index + total) % total;

                slides[current].classList.add('active');
                dots[current].classList.add('active');
                track.style.transform = `translateX(-${current * 100}%)`;
                if (currentNum) currentNum.textContent = current + 1;

                prevBtn.disabled = false;
                nextBtn.disabled = false;
            }

            // Clone to remove old listeners
            const newPrev = prevBtn.cloneNode(true);
            const newNext = nextBtn.cloneNode(true);
            prevBtn.parentNode.replaceChild(newPrev, prevBtn);
            nextBtn.parentNode.replaceChild(newNext, nextBtn);

            newPrev.addEventListener('click', () => goTo(current - 1));
            newNext.addEventListener('click', () => goTo(current + 1));

            // Keyboard navigation
            mpwWidget.addEventListener('keydown', (e) => {
                if (e.key === 'ArrowLeft')  goTo(current - 1);
                if (e.key === 'ArrowRight') goTo(current + 1);
            });
        }
    }

    // Initial setup if static content exists, though dynamic loading will override
    setupTestimonialCarousel();
    setupModuleWidget();

    // ===== SYLLABUS ACCORDION (landing page course cards) =====
    function initSyllabusToggles() {
        document.querySelectorAll('.cp-card-syllabus-toggle').forEach(toggle => {
            // Clone to remove any existing listeners
            const fresh = toggle.cloneNode(true);
            toggle.parentNode.replaceChild(fresh, toggle);

            fresh.addEventListener('click', (e) => {
                e.stopPropagation();
                const isOpen = fresh.classList.contains('active');
                // Close all other open syllabuses on same page
                document.querySelectorAll('.cp-card-syllabus-toggle.active').forEach(t => {
                    t.classList.remove('active');
                    const sib = t.nextElementSibling;
                    if (sib && sib.classList.contains('cp-card-syllabus')) {
                        sib.classList.remove('active');
                        sib.style.maxHeight = '0';
                    }
                });
                if (!isOpen) {
                    fresh.classList.add('active');
                    const syllabus = fresh.nextElementSibling;
                    if (syllabus && syllabus.classList.contains('cp-card-syllabus')) {
                        syllabus.classList.add('active');
                        syllabus.style.maxHeight = syllabus.scrollHeight + 'px';
                    }
                }
            });
        });
    }
    initSyllabusToggles();


    const videoModal = document.getElementById('videoModal');
    const previewVideo = document.getElementById('previewVideo');
    const playBtn = document.querySelector('.hero-play-btn');
    const watchPreviewBtn = document.querySelector('.btn-secondary');
    const closeVideoBtn = document.querySelector('.video-modal-close');
    const videoModalBackdrop = document.querySelector('.video-modal-backdrop');

    window.playSecureVideo = async function(videoId) {
        if (!videoModal) return;

        // Stop background hero video so screen recording doesn't capture it.
        const heroBgVideo = document.querySelector('.hero-bg-video');
        if (heroBgVideo) {
            // remember state to restore on close
            heroBgVideo.dataset.bbaiWasPlaying = (!heroBgVideo.paused).toString();
            heroBgVideo.pause();
            heroBgVideo.style.visibility = 'hidden';
        }

        // Show modal and loading state
        videoModal.classList.add('active');
        const container = videoModal.querySelector('.video-iframe-container');
        if (!container) return;

        // Store original video element to restore later
        if (!container.dataset.originalHtml) {
            container.dataset.originalHtml = container.innerHTML;
        }

        container.innerHTML = '<div style="color:#fff; display:flex; align-items:center; justify-content:center; height:100%;padding:44px ;font-family:sans-serif;">Loading secure video player...</div>';

        try {
            // ===== Hardware acceleration gate (approximation) =====
            // Browsers don't provide a direct API for “Hardware Acceleration is ON”.
            // We approximate using WebGPU adapter availability. If WebGPU isn't supported,
            // we allow playback rather than incorrectly blocking.
            try {
                if (navigator.gpu && typeof navigator.gpu.requestAdapter === 'function') {
                    const adapter = await navigator.gpu.requestAdapter();
                    if (!adapter) {
                        container.innerHTML = `
                    <div style="background:#000; color:#fff; padding:44px; text-align:center; width:100%; height:100%; display:flex; flex-direction:column; align-items:center; justify-content:center; font-family:sans-serif; box-sizing:border-box;">
                                <h3 style="margin:0 0 8px 0; line-height:1.2; padding-top:2px;">Playback Blocked</h3>
                                <p style="margin:0; opacity:.9;">To watch this video, you must enable Hardware Acceleration in your browser settings and restart your browser.</p>
                            </div>`;
                        return;
                    }
                }
            } catch (e) {
                // If the capability check fails, still show the message without hard-blocking the UI.
                container.innerHTML = `
                    <div style="background:#000; color:#fff; padding:44px; text-align:center; width:100%; height:100%; display:flex; flex-direction:column; align-items:center; justify-content:center; font-family:sans-serif; box-sizing:border-box;">
                        <h3 style="margin:0 0 8px 0; line-height:1.2; padding-top:2px;">Playback Blocked</h3>
                        <p style="margin:0; opacity:.9;">To watch this video, you must enable Hardware Acceleration in your browser settings and restart your browser.</p>
                    </div>`;
                return;
            }

            const headers = { 'Content-Type': 'application/json' };
            const token = localStorage.getItem('userToken');
            if (token) {
                headers['Authorization'] = 'Bearer ' + token;
            }

            const response = await fetch(`/api/videos/otp/${videoId}`, {
                method: 'POST',
                headers: headers
            });

            if (!response.ok) {
                const errData = await response.json();
                throw new Error(errData.message || 'Failed to fetch video tokens.');
            }

            const data = await response.json();

            // Create VdoCipher Secure Iframe Embed
            const iframe = document.createElement('iframe');
            iframe.src = `https://player.vdocipher.com/v2/?otp=${data.otp}&playbackInfo=${data.playbackInfo}&autoplay=true`;
            iframe.style.border = '0';
            iframe.style.width = '100%';
            iframe.style.height = '100%';
            iframe.style.position = 'absolute';
            iframe.style.top = '0';
            iframe.style.left = '0';
            iframe.allow = 'encrypted-media';
            iframe.allowFullscreen = true;

            container.innerHTML = '';
            container.appendChild(iframe);
        } catch (err) {
            container.innerHTML = `<div style="color:#f87171; display:flex; align-items:center; justify-content:center; height:100%; font-family:sans-serif; text-align:center; padding:20px;">${err.message || 'Error loading secure video stream.'}</div>`;
        }
    };

    function openVideoModal(e) {
        if (e) e.preventDefault();
        
        // vdoCipherTestVideoId must be set by the server/Razor view (e.g. window.vdoCipherTestVideoId = "@Model.Hero.PreviewVideoId")
        // It is NOT hardcoded here — it must come from the database via the CMS.
        const testVideoId = window.vdoCipherTestVideoId;
        if (testVideoId) {
            window.playSecureVideo(testVideoId);
            return;
        }

        if (videoModal && previewVideo) {
            videoModal.classList.add('active');
            previewVideo.play().catch(err => console.log('Autoplay blocked:', err));
        }
    }

    function closeVideoModal() {
        if (videoModal) {
            videoModal.classList.remove('active');

            // Restore background hero video
            const heroBgVideo = document.querySelector('.hero-bg-video');
            if (heroBgVideo && heroBgVideo.dataset && heroBgVideo.dataset.bbaiWasPlaying !== undefined) {
                const shouldPlay = heroBgVideo.dataset.bbaiWasPlaying === 'true';
                heroBgVideo.style.visibility = '';
                if (shouldPlay) {
                    heroBgVideo.play().catch(() => {});
                }
            }

            const container = videoModal.querySelector('.video-iframe-container');
            if (container && container.dataset.originalHtml) {
                container.innerHTML = container.dataset.originalHtml;
            } else if (previewVideo) {
                previewVideo.pause();
                previewVideo.currentTime = 0;
            }
        }
    }

    if (playBtn) playBtn.addEventListener('click', openVideoModal);
    if (watchPreviewBtn) watchPreviewBtn.addEventListener('click', openVideoModal);
    if (closeVideoBtn) closeVideoBtn.addEventListener('click', closeVideoModal);
    if (videoModalBackdrop) videoModalBackdrop.addEventListener('click', closeVideoModal);

    // Escape key closes modals
    window.addEventListener('keydown', (e) => {
        if (e.key === 'Escape') {
            closeVideoModal();
            closeEnrollModal();
        }
    });

    // ===== INTERACTIVE ENROLLMENT MODAL & TOASTS =====
    const enrollModal = document.getElementById('enrollModal');
    const enrollForm = document.getElementById('enrollForm');
    const closeEnrollBtn = document.querySelector('.enroll-modal-close');
    const enrollBackdrop = document.querySelector('.enroll-modal-backdrop');
    const selectedPlanText = document.querySelector('.selected-plan-text');
    const toast = document.getElementById('toast');

    const actionButtons = document.querySelectorAll(
        '.course-btn, .nav-cta, .btn-primary:not([type="submit"]):not(.nav-cta)'
    );

    actionButtons.forEach(btn => {
        btn.addEventListener('click', (e) => {
            if (btn.closest('#videoModal') || btn.closest('#enrollModal')) return;

            const href = btn.getAttribute('href');
            if (href && href.startsWith('#')) {
                // Let anchor links scroll naturally to their targets
                return;
            }

            e.preventDefault();
            let planName = 'GuitarVerse Masterclass';

            const pricingCard = btn.closest('.pricing-card');
            const courseCard = btn.closest('.course-card');

            if (pricingCard) {
                planName = pricingCard.querySelector('.pricing-plan').textContent + ' Plan';
            } else if (courseCard) {
                planName = courseCard.querySelector('.course-name').textContent;
            }

            // Pre-fill user data if logged in
            const token = localStorage.getItem('userToken');
            if (token) {
                const userData = JSON.parse(localStorage.getItem('userData') || '{}');
                const nameInput = document.getElementById('enrollName');
                const emailInput = document.getElementById('enrollEmail');
                if (nameInput) {
                    nameInput.value = userData.fullName || '';
                    nameInput.readOnly = true;
                }
                if (emailInput) {
                    emailInput.value = userData.email || '';
                    emailInput.readOnly = true;
                }
            } else {
                // Clear and enable inputs if not logged in
                const nameInput = document.getElementById('enrollName');
                const emailInput = document.getElementById('enrollEmail');
                if (nameInput) {
                    nameInput.value = '';
                    nameInput.readOnly = false;
                }
                if (emailInput) {
                    emailInput.value = '';
                    emailInput.readOnly = false;
                }
            }

            if (selectedPlanText) {
                selectedPlanText.innerHTML = `Plan Selected: <span>${planName}</span>`;
            }
            if (enrollModal) {
                enrollModal.classList.add('active');
            }
        });
    });

    function closeEnrollModal() {
        if (enrollModal) {
            enrollModal.classList.remove('active');
        }
        if (enrollForm) {
            enrollForm.reset();
        }
    }

    if (closeEnrollBtn) closeEnrollBtn.addEventListener('click', closeEnrollModal);
    if (enrollBackdrop) enrollBackdrop.addEventListener('click', closeEnrollModal);

    if (enrollForm) {
        enrollForm.addEventListener('submit', (e) => {
            e.preventDefault();
            closeEnrollModal();

            // Extract plan/course name from the selected plan text
            let planName = 'GuitarVerse Masterclass';
            const selectedSpan = selectedPlanText ? selectedPlanText.querySelector('span') : null;
            if (selectedSpan) {
                planName = selectedSpan.textContent.trim();
            }

            // Save to enrolledCourses in localStorage
            let enrolled = JSON.parse(localStorage.getItem('enrolledCourses') || '[]');
            if (!enrolled.includes(planName)) {
                enrolled.push(planName);
                localStorage.setItem('enrolledCourses', JSON.stringify(enrolled));
            }

            // Update module progress on page
            if (typeof updateModuleProgress === 'function') {
                updateModuleProgress();
            }

            // Dispatch event for details page to pick up
            window.dispatchEvent(new CustomEvent('course-enrolled', { detail: { courseName: planName } }));

            // Display Toast notification for Enrollment
            if (toast) {
                toast.querySelector('.toast-title').textContent = 'Success';
                toast.querySelector('.toast-message').textContent = 'Enrollment Successful! Welcome to GuitarVerse.';
                toast.classList.add('show');
                setTimeout(() => {
                    toast.classList.remove('show');
                }, 4000);
            }
        });
    }

    const contactForm = document.getElementById('contactForm');
    if (contactForm) {
        contactForm.addEventListener('submit', (e) => {
            e.preventDefault();
            contactForm.reset();

            // Display Toast notification for Contact Message
            if (toast) {
                toast.querySelector('.toast-title').textContent = 'Message Sent';
                toast.querySelector('.toast-message').textContent = 'Thank you! We will get back to you shortly.';
                toast.classList.add('show');
                setTimeout(() => {
                    toast.classList.remove('show');
                }, 4000);
            }
        });
    }

    // ===== NAV-CTA SHIMMER EFFECT =====
    const navCta = document.querySelector('.nav-cta');
    if (navCta) {
        setInterval(() => {
            navCta.classList.add('shimmer');
            setTimeout(() => navCta.classList.remove('shimmer'), 800);
        }, 5000);
    }

    const mpwWidget = document.querySelector('.module-preview-widget');
    if (mpwWidget) {
        let touchStartX = 0;
        let touchDeltaX = 0;

        mpwWidget.addEventListener('touchstart', (e) => {
            touchStartX = e.touches[0].clientX;
            touchDeltaX = 0;
        }, { passive: true });

        mpwWidget.addEventListener('touchmove', (e) => {
            touchDeltaX = e.touches[0].clientX - touchStartX;
        }, { passive: true });

        mpwWidget.addEventListener('touchend', () => {
            if (Math.abs(touchDeltaX) > 40) {
                const track = mpwWidget.querySelector('.mpw-track');
                const slides = Array.from(mpwWidget.querySelectorAll('.mpw-slide'));
                const current = slides.findIndex(s => s.classList.contains('active'));
                const next = (current + (touchDeltaX < 0 ? 1 : -1) + slides.length) % slides.length;
                
                slides[current].classList.remove('active');
                slides[next].classList.add('active');
                track.style.transform = `translateX(-${next * 100}%)`;
                
                const currentNum = mpwWidget.querySelector('.mpw-current-num');
                if (currentNum) currentNum.textContent = next + 1;
                
                const dots = Array.from(mpwWidget.querySelectorAll('.mpw-dot'));
                if (dots.length > 0) {
                    dots[current].classList.remove('active');
                    dots[next].classList.add('active');
                }
            }
        });

        // Play buttons: open video preview modal (not the enroll/join modal)
        mpwWidget.addEventListener('click', (e) => {
            const playBtn = e.target.closest('.mpw-play-btn');
            if (!playBtn) return;

            e.stopPropagation();

            const slide = playBtn.closest('.mpw-slide');
            const title = slide?.querySelector('.mpw-title')?.textContent || 'GuitarVerse Module';

            // If we have a preview video id from server, play it in the secure modal.
            // (Index.cshtml already sets: window.vdoCipherTestVideoId = "@(Model.Hero?.PreviewVideoId)")
            const videoId = window.vdoCipherTestVideoId;

            if (videoId) {
                // Ensure video modal opens
                if (typeof window.playSecureVideo === 'function') {
                    window.playSecureVideo(videoId);
                    return;
                }
            }

            // Fallback: if no video id is available, keep previous behavior.
            if (selectedPlanText) selectedPlanText.innerHTML = `Plan Selected: <span>${title}</span>`;
            if (enrollModal) enrollModal.classList.add('active');
        });
    }

    // ===== MODULE PROGRESS UPDATER =====
    window.updateModuleProgress = function() {
        const slides = document.querySelectorAll('.mpw-slide');
        const enrolledCourses = JSON.parse(localStorage.getItem('enrolledCourses') || '[]');
        const courseProgress = JSON.parse(localStorage.getItem('courseProgress') || '{}');

        slides.forEach(slide => {
            const titleEl = slide.querySelector('.mpw-title');
            if (!titleEl) return;
            const courseTitle = titleEl.textContent.trim();
            const fillEl = slide.querySelector('.mpw-progress-fill');
            const labelEl = slide.querySelector('.mpw-progress-label');

            if (!fillEl || !labelEl) return;

            if (enrolledCourses.includes(courseTitle)) {
                const progress = courseProgress[courseTitle] || 0;
                fillEl.style.width = `${progress}%`;
                if (progress > 0) {
                    labelEl.textContent = `${progress}% completed`;
                } else {
                    labelEl.textContent = 'Enrolled';
                }
            } else {
                fillEl.style.width = '0%';
                labelEl.textContent = 'Not started';
            }
        });
    };

    // Initial run
    window.updateModuleProgress();

    // ===== MOBILE RESPONSIVE OVERFLOW DEBUGGER =====
    const checkOverflow = () => {
        const badElements = [];
        document.querySelectorAll('*').forEach(el => {
            if (el.offsetWidth > window.innerWidth && el.id !== 'bb-overflow-debug') {
                const hasOverflowingChild = Array.from(el.children).some(child => child.offsetWidth > window.innerWidth);
                if (!hasOverflowingChild) {
                    badElements.push(el);
                }
            }
        });
        
        if (badElements.length > 0) {
            let debugDiv = document.getElementById('bb-overflow-debug');
            if (!debugDiv) {
                debugDiv = document.createElement('div');
                debugDiv.id = 'bb-overflow-debug';
                debugDiv.style.cssText = 'position:fixed;bottom:0;left:0;right:0;background:rgba(220,38,38,0.95);color:#fff;padding:8px 12px;font-size:12px;z-index:999999;font-family:monospace;max-height:150px;overflow-y:auto;text-align:left;line-height:1.4;';
                document.body.appendChild(debugDiv);
            }
            const info = badElements.map(el => {
                const tag = el.tagName.toLowerCase();
                const cls = el.className ? '.' + Array.from(el.classList).join('.') : '';
                const id = el.id ? '#' + el.id : '';
                return `${tag}${id}${cls} (${el.offsetWidth}px > ${window.innerWidth}px)`;
            }).join('<br>');
            debugDiv.innerHTML = '<strong>⚠️ Responsive Overflows (Innermost):</strong><br>' + info;
        } else {
            const debugDiv = document.getElementById('bb-overflow-debug');
            if (debugDiv) debugDiv.remove();
        }
    };
    
    // Check after animations complete
    setTimeout(checkOverflow, 2000);
    window.addEventListener('resize', checkOverflow);
    window.addEventListener('orientationchange', () => setTimeout(checkOverflow, 500));
});
