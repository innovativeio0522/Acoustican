document.addEventListener('DOMContentLoaded', async () => {
    const API_URL = '/api';

    // ===== NAVBAR SCROLL =====
    const navbar = document.querySelector('.navbar');
    let lastScroll = 0;
    window.addEventListener('scroll', () => {
        const cur = window.pageYOffset;
        navbar.classList.toggle('scrolled', cur > 60);
        navbar.classList.toggle('nav-hidden', cur > lastScroll && cur > 300);
        lastScroll = cur;
    });

    // ===== MOBILE MENU =====
    const mobileMenuBtn = document.querySelector('.mobile-menu-btn');
    if (mobileMenuBtn) {
        mobileMenuBtn.addEventListener('click', () => {
            navbar.classList.toggle('mobile-menu-active');
            mobileMenuBtn.innerHTML = navbar.classList.contains('mobile-menu-active') ? '✕' : '☰';
        });
    }

    // ===== DYNAMIC COURSE LOADING =====
    const grid = document.getElementById('coursesGrid');
    const emptyState = document.getElementById('cpEmpty');
    const countEl = document.getElementById('visibleCount');
    let allCards = [];
    let activeFilter = 'all';
    let searchTerm = '';
    let sortBy = 'popular';    async function loadCourses() {
        // Progressive Enhancement: if server-rendered cards exist, use them directly
        const existingCards = grid ? Array.from(grid.querySelectorAll('.cp-card')) : [];
        if (existingCards.length > 0) {
            allCards = existingCards;
            allCards.forEach(card => initCardTilt(card));
            if (countEl) countEl.textContent = allCards.length;
            initGSAP();
            initEnrollButtons();
            initSyllabusToggles();
            return;
        }

        // Fallback: fetch from API if no SSR cards present
        try {
            const response = await fetch(`${API_URL}/courses`);
            if (!response.ok) return;
            const courses = await response.json();
            const publishedCourses = courses.filter(c => c.isPublished);

            if (grid) {
                grid.innerHTML = '';
                publishedCourses.forEach((course, index) => {
                    const card = document.createElement('article');
                    card.className = 'cp-card';
                    card.dataset.level = course.level.toLowerCase();
                    card.dataset.title = course.title.toLowerCase();
                    card.dataset.rating = course.rating || '0.0';
                    card.dataset.duration = course.durationMinutes;
                    card.dataset.popular = index + 1;

                    const publishedModules = (course.modules || [])
                        .filter(m => m.isPublished)
                        .sort((a, b) => (a.displayOrder - b.displayOrder) || (a.moduleNumber - b.moduleNumber));

                    let syllabusHtml = '';
                    if (publishedModules.length > 0) {
                        syllabusHtml = `
                            <button class="cp-card-syllabus-toggle">
                                <span>View Syllabus (${publishedModules.length} Modules)</span>
                                <span class="toggle-icon">▼</span>
                            </button>
                            <div class="cp-card-syllabus">
                                <ul class="cp-syllabus-list">
                                    ${publishedModules.map(m => `
                                        <li class="cp-syllabus-item">
                                            <div class="cp-syllabus-meta">
                                                <span class="cp-syllabus-num">Module ${String(m.moduleNumber).padStart(2, '0')}</span>
                                                <span class="cp-syllabus-title">${m.title}</span>
                                            </div>
                                            <span class="cp-syllabus-duration">${m.durationMinutes} min</span>
                                        </li>
                                    `).join('')}
                                </ul>
                            </div>
                        `;
                    }

                    card.innerHTML = `
                        <div class="cp-card-image-wrap">
                            <img src="${course.thumbnailUrl || 'https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&h=340&fit=crop&q=80'}" alt="${course.title}" class="cp-card-image" loading="lazy">
                            <div class="cp-card-overlay">
                                <button class="cp-preview-btn">▶ Preview</button>
                            </div>
                            <span class="cp-card-badge ${course.level.toLowerCase()}">${course.level}</span>
                            <span class="cp-card-duration">${Math.floor(course.durationMinutes / 60)}h ${course.durationMinutes % 60}m</span>
                        </div>
                        <div class="cp-card-body">
                            <div class="cp-card-meta-top">
                                <span class="cp-card-category">Course</span>
                                <div class="cp-card-rating">★ <strong>${course.rating || '0.0'}</strong> <span>(${course.reviewCount || 0})</span></div>
                            </div>
                            <h2 class="cp-card-title">${course.title}</h2>
                            <p class="cp-card-desc">${course.description}</p>
                            <div class="cp-card-instructor">
                                <img src="https://ui-avatars.com/api/?name=Instructor&background=f5a623&color=0a0a0b" alt="Instructor" class="cp-instructor-avatar">
                                <span class="cp-instructor-name">Acoustican Instructor</span>
                            </div>
                            ${syllabusHtml}
                            <div class="cp-card-footer">
                                <div class="cp-card-lessons">📚 ${course.lessonCount || 0} lessons</div>
                                <button class="cp-enroll-btn">Enroll Now</button>
                            </div>
                        </div>
                    `;
                    grid.appendChild(card);
                    allCards.push(card);
                    initCardTilt(card);
                });

                countEl.textContent = publishedCourses.length;
                initGSAP();
                initEnrollButtons();
                initSyllabusToggles();
            }
        } catch (err) {
            console.error('Error loading dynamic courses:', err);
        }
    }
    function applyFilters() {
        let visible = allCards.filter(card => {
            const level = card.dataset.level;
            const title = card.dataset.title;
            const matchFilter = activeFilter === 'all' || level === activeFilter;
            const matchSearch = title.includes(searchTerm.toLowerCase());
            return matchFilter && matchSearch;
        });

        // Sort
        visible.sort((a, b) => {
            if (sortBy === 'popular')       return +a.dataset.popular   - +b.dataset.popular;
            if (sortBy === 'rating')        return +b.dataset.rating    - +a.dataset.rating;
            if (sortBy === 'duration-asc')  return +a.dataset.duration  - +b.dataset.duration;
            if (sortBy === 'duration-desc') return +b.dataset.duration  - +a.dataset.duration;
            if (sortBy === 'newest')        return +b.dataset.popular   - +a.dataset.popular;
            return 0;
        });

        // Hide all cards first
        allCards.forEach(c => {
            c.classList.add('hidden');
            c.style.opacity = '';
            c.style.transform = '';
        });

        // Show matched cards and re-order them in the DOM
        visible.forEach(c => {
            c.classList.remove('hidden');
            c.style.opacity = '1';
            grid.appendChild(c);
        });

        countEl.textContent = visible.length;
        if (emptyState) emptyState.style.display = visible.length === 0 ? 'block' : 'none';
    }

    // ===== GSAP ENTRANCE =====
    function initGSAP() {
        if (typeof gsap !== 'undefined') {
            gsap.registerPlugin(ScrollTrigger);
            document.body.classList.add('gsap-ready');

            gsap.set('.navbar', { y: -80, opacity: 0 });
            gsap.set('.cp-hero-text > *', { y: 30, opacity: 0 });

            const tl = gsap.timeline({ defaults: { ease: 'power3.out' } });
            tl.to('.navbar',          { y: 0, opacity: 1, duration: 0.9 }, 0.1)
              .to('.cp-hero-text > *', { y: 0, opacity: 1, stagger: 0.12, duration: 0.9 }, 0.3);

            gsap.fromTo('#coursesGrid .cp-card',
                { opacity: 0, y: 40 },
                {
                    opacity: 1, y: 0,
                    duration: 0.6,
                    ease: 'power3.out',
                    stagger: 0.07,
                    clearProps: 'all',
                    scrollTrigger: { trigger: '#coursesGrid', start: 'top 90%', toggleActions: 'play none none none' }
                }
            );
        }
    }

    // ===== 3D TILT ON CARDS =====
    function initCardTilt(card) {
        let rafId = null, isActive = false, leaveTimeout = null;

        function applyTilt(cx, cy) {
            const r = card.getBoundingClientRect();
            const x = cx - r.left, y = cy - r.top;
            const rotX = Math.max(-6, Math.min(6, -((y - r.height/2) / 25)));
            const rotY = Math.max(-6, Math.min(6,  ((x - r.width/2)  / 25)));
            card.style.setProperty('--mouse-x', `${x}px`);
            card.style.setProperty('--mouse-y', `${y}px`);
            card.style.transform = `perspective(1000px) rotateX(${rotX}deg) rotateY(${rotY}deg)`;
        }

        function resetCard() {
            isActive = false;
            card.style.transition = 'transform 0.5s cubic-bezier(0.16,1,0.3,1)';
            card.style.transform  = '';
            card.style.setProperty('--mouse-x', '-1000px');
            card.style.setProperty('--mouse-y', '-1000px');
        }

        document.addEventListener('pointermove', (e) => {
            if (rafId) return;
            rafId = requestAnimationFrame(() => {
                rafId = null;
                const r = card.getBoundingClientRect();
                const inside = e.clientX >= r.left+4 && e.clientX <= r.right-4 &&
                               e.clientY >= r.top+4  && e.clientY <= r.bottom-4;
                if (inside) {
                    if (leaveTimeout) { clearTimeout(leaveTimeout); leaveTimeout = null; }
                    if (!isActive) { isActive = true; card.style.transition = 'none'; }
                    applyTilt(e.clientX, e.clientY);
                } else if (isActive) {
                    if (!leaveTimeout) leaveTimeout = setTimeout(() => { leaveTimeout = null; resetCard(); }, 16);
                }
            });
        });
    }

    // ===== FILTER & SEARCH EVENTS =====
    const tabs = document.querySelectorAll('.cp-tab');
    const searchInput = document.querySelector('.cp-search');
    const sortSelect = document.querySelector('.cp-sort');

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            tabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');
            activeFilter = tab.dataset.filter;
            applyFilters();
        });
    });

    if (searchInput) {
        searchInput.addEventListener('input', () => {
            searchTerm = searchInput.value.trim();
            applyFilters();
        });
    }

    if (sortSelect) {
        sortSelect.addEventListener('change', () => {
            sortBy = sortSelect.value;
            applyFilters();
        });
    }

    // ===== ENROLL MODAL =====
    const enrollModal = document.getElementById('enrollModal');
    const enrollForm = document.getElementById('enrollForm');
    const closeEnrollBtn = document.querySelector('.enroll-modal-close');
    const enrollBackdrop = document.querySelector('.enroll-modal-backdrop');
    const selectedPlanText = document.querySelector('.selected-plan-text');
    const toast = document.getElementById('toast');

    function openEnroll(title) {
        if (selectedPlanText) selectedPlanText.innerHTML = `Plan Selected: <span>${title}</span>`;
        if (enrollModal) enrollModal.classList.add('active');
    }

    function closeEnroll() {
        if (enrollModal) enrollModal.classList.remove('active');
        if (enrollForm) enrollForm.reset();
    }

    function initEnrollButtons() {
        document.querySelectorAll('.cp-enroll-btn, .cp-preview-btn').forEach(btn => {
            btn.addEventListener('click', (e) => {
                e.stopPropagation();

                // If not logged in, open the auth modal (login) instead of enrolling.
                const token = localStorage.getItem('userToken');
                if (!token) {
                    if (typeof window.openAuthModal === 'function') {
                        window.openAuthModal('login');
                    }
                    return;
                }

                const card = btn.closest('.cp-card');
                const title = card.querySelector('.cp-card-title')?.textContent || 'Acoustican Course';
                openEnroll(title);
            });
        });
    }
    function initSyllabusToggles() {
        document.querySelectorAll('.cp-card-syllabus-toggle').forEach(toggle => {
            const newToggle = toggle.cloneNode(true);
            toggle.parentNode.replaceChild(newToggle, toggle);
            newToggle.addEventListener('click', (e) => {
                e.stopPropagation();
                const isOpen = newToggle.classList.contains('active');
                
                // Close all other open syllabuses
                document.querySelectorAll('.cp-card-syllabus-toggle.active').forEach(t => {
                    t.classList.remove('active');
                    const sib = t.nextElementSibling;
                    if (sib && sib.classList.contains('cp-card-syllabus')) {
                        sib.classList.remove('active');
                        sib.style.maxHeight = '0';
                    }
                });

                if (!isOpen) {
                    newToggle.classList.add('active');
                    const syllabus = newToggle.nextElementSibling;
                    if (syllabus && syllabus.classList.contains('cp-card-syllabus')) {
                        syllabus.classList.add('active');
                        syllabus.style.maxHeight = syllabus.scrollHeight + 'px';
                    }
                }
            });
        });
    }

    if (closeEnrollBtn) closeEnrollBtn.addEventListener('click', closeEnroll);
    if (enrollBackdrop) enrollBackdrop.addEventListener('click', closeEnroll);
    window.addEventListener('keydown', e => { if (e.key === 'Escape') closeEnroll(); });

    if (enrollForm) {
        enrollForm.addEventListener('submit', (e) => {
            e.preventDefault();
            closeEnroll();
            if (toast) {
                toast.querySelector('.toast-title').textContent = 'Success';
                toast.querySelector('.toast-message').textContent = 'Enrollment successful. Welcome to Acoustican.';
                toast.classList.add('show');
                setTimeout(() => toast.classList.remove('show'), 4000);
            }
        });
    }

    // ===== NAV-CTA SHIMMER =====
    const navCta = document.querySelector('.nav-cta');
    if (navCta) {
        setInterval(() => {
            navCta.classList.add('shimmer');
            setTimeout(() => navCta.classList.remove('shimmer'), 800);
        }, 5000);
    }

    // Start loading
    await loadCourses();
});
