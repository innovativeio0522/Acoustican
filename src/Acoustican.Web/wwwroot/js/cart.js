/**
 * ═══════════════════════════════════════════════════════════
 * The Acoustican — Cart Module
 * Handles cart state, API calls, badge, toasts, and drawer
 * ═══════════════════════════════════════════════════════════
 */
(function () {
    'use strict';

    const API_URL = '/api';
    const CART_STORAGE_KEY = 'gv_guest_cart';

    // ─── Helpers ──────────────────────────────────────────
    function getToken() {
        return localStorage.getItem('userToken');
    }

    function isLoggedIn() {
        return !!getToken();
    }

    function authHeaders() {
        const token = getToken();
        return token
            ? { 'Content-Type': 'application/json', 'Authorization': `Bearer ${token}` }
            : { 'Content-Type': 'application/json' };
    }

    // ─── Guest Cart (localStorage) ────────────────────────
    function getGuestCart() {
        try {
            return JSON.parse(localStorage.getItem(CART_STORAGE_KEY) || '[]');
        } catch { return []; }
    }

    function saveGuestCart(items) {
        localStorage.setItem(CART_STORAGE_KEY, JSON.stringify(items));
    }

    function clearGuestCart() {
        localStorage.removeItem(CART_STORAGE_KEY);
    }

    function addToGuestCart(courseId, courseTitle, coursePrice, courseThumbnail, courseOriginalPrice, courseLevel, instructorName) {
        const cart = getGuestCart();
        if (cart.find(item => item.courseId === courseId)) {
            return false; // duplicate
        }
        cart.push({
            courseId,
            courseTitle,
            coursePrice: parseFloat(coursePrice),
            courseOriginalPrice: parseFloat(courseOriginalPrice || coursePrice),
            courseThumbnail,
            courseLevel: courseLevel || '',
            instructorName: instructorName || '',
            addedAt: new Date().toISOString()
        });
        saveGuestCart(cart);
        return true;
    }

    function removeFromGuestCart(courseId) {
        const cart = getGuestCart().filter(item => item.courseId !== courseId);
        saveGuestCart(cart);
    }

    // ─── Badge ────────────────────────────────────────────
    function updateBadge(count) {
        const badges = document.querySelectorAll('.cart-badge');
        badges.forEach(badge => {
            if (count > 0) {
                badge.textContent = count > 99 ? '99+' : count;
                badge.classList.add('visible');
            } else {
                badge.classList.remove('visible');
            }
        });
    }

    async function refreshBadge() {
        if (isLoggedIn()) {
            try {
                const res = await fetch(`${API_URL}/cart/count`, { headers: authHeaders() });
                if (res.ok) {
                    const data = await res.json();
                    updateBadge(data.count);
                }
            } catch { /* silent */ }
        } else {
            updateBadge(getGuestCart().length);
        }
    }

    // ─── Toast ────────────────────────────────────────────
    function showCartToast(title, message, type = 'success') {
        const toast = document.getElementById('cartToast');
        if (!toast) return;

        const icons = { success: '✓', info: 'ℹ', warning: '⚠', error: '✕' };
        toast.querySelector('.cart-toast-icon').textContent = icons[type] || '🛒';
        toast.querySelector('.cart-toast-title').textContent = title;
        toast.querySelector('.cart-toast-message').textContent = message;
        toast.className = `cart-toast cart-toast-${type} show`;

        clearTimeout(toast._timer);
        toast._timer = setTimeout(() => toast.classList.remove('show'), 3000);
    }

    // ─── Drawer ───────────────────────────────────────────
    function openDrawer() {
        const drawer = document.getElementById('cartDrawer');
        if (drawer) {
            renderDrawerItems();
            drawer.classList.add('open');
            document.body.style.overflow = 'hidden';
        }
    }

    function closeDrawer() {
        const drawer = document.getElementById('cartDrawer');
        if (drawer) {
            drawer.classList.remove('open');
            document.body.style.overflow = '';
        }
    }

    async function renderDrawerItems() {
        const container = document.getElementById('drawerItems');
        const totalEl = document.getElementById('drawerTotal');
        const countEl = document.getElementById('drawerCount');
        if (!container) return;

        let items = [];

        if (isLoggedIn()) {
            try {
                const res = await fetch(`${API_URL}/cart`, { headers: authHeaders() });
                if (res.ok) {
                    const data = await res.json();
                    items = data.items || [];
                }
            } catch { /* silent */ }
        } else {
            items = getGuestCart().map(g => ({
                courseId: g.courseId,
                courseTitle: g.courseTitle,
                coursePrice: g.coursePrice,
                courseOriginalPrice: g.courseOriginalPrice,
                courseThumbnailUrl: g.courseThumbnail,
                courseLevel: g.courseLevel,
                instructorName: g.instructorName
            }));
        }

        if (items.length === 0) {
            container.innerHTML = `
                <div class="drawer-empty">
                    <div class="drawer-empty-icon">🛒</div>
                    <p>Your cart is empty</p>
                    <a href="/courses" class="drawer-browse-btn">Browse Courses</a>
                </div>`;
            if (totalEl) totalEl.textContent = '₹0';
            if (countEl) countEl.textContent = '0 items';
            return;
        }

        const total = items.reduce((sum, i) => sum + (i.coursePrice || 0), 0);
        if (totalEl) totalEl.textContent = `₹${total.toLocaleString('en-IN')}`;
        if (countEl) countEl.textContent = `${items.length} item${items.length > 1 ? 's' : ''}`;

        container.innerHTML = items.map(item => `
            <div class="drawer-item" data-course-id="${item.courseId}">
                <img src="${item.courseThumbnailUrl || 'https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=120&h=80&fit=crop&q=60'}" 
                     alt="${item.courseTitle}" class="drawer-item-thumb">
                <div class="drawer-item-info">
                    <h4 class="drawer-item-title">${item.courseTitle}</h4>
                    <span class="drawer-item-price">₹${(item.coursePrice || 0).toLocaleString('en-IN')}</span>
                </div>
                <button class="drawer-item-remove" onclick="window.GVCart.removeFromCart(${item.courseId})" aria-label="Remove">
                    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
                </button>
            </div>
        `).join('');
    }

    // ─── Add to Cart ──────────────────────────────────────
    async function addToCart(courseId, courseTitle, coursePrice, courseThumbnail, courseOriginalPrice, courseLevel, instructorName) {
        if (isLoggedIn()) {
            try {
                const res = await fetch(`${API_URL}/cart`, {
                    method: 'POST',
                    headers: authHeaders(),
                    body: JSON.stringify({ courseId: parseInt(courseId) })
                });
                const data = await res.json();
                if (data.success) {
                    updateBadge(data.cartCount);
                    showCartToast('Added to Cart', courseTitle, 'success');
                } else {
                    showCartToast('Already in Cart', data.message, 'info');
                }
            } catch {
                showCartToast('Error', 'Could not add to cart. Try again.', 'error');
            }
        } else {
            const added = addToGuestCart(
                parseInt(courseId), courseTitle, coursePrice, 
                courseThumbnail, courseOriginalPrice, courseLevel, instructorName
            );
            if (added) {
                updateBadge(getGuestCart().length);
                showCartToast('Added to Cart', courseTitle, 'success');
            } else {
                showCartToast('Already in Cart', 'This course is already in your cart', 'info');
            }
        }
    }

    // ─── Remove from Cart ─────────────────────────────────
    async function removeFromCart(courseId) {
        if (isLoggedIn()) {
            try {
                const res = await fetch(`${API_URL}/cart/${courseId}`, {
                    method: 'DELETE',
                    headers: authHeaders()
                });
                const data = await res.json();
                if (data.success) {
                    updateBadge(data.cartCount);
                    showCartToast('Removed', 'Course removed from cart', 'warning');
                }
            } catch { /* silent */ }
        } else {
            removeFromGuestCart(parseInt(courseId));
            updateBadge(getGuestCart().length);
            showCartToast('Removed', 'Course removed from cart', 'warning');
        }

        // Re-render drawer if open
        renderDrawerItems();

        // Re-render cart page if on it
        if (typeof window.renderCartPage === 'function') {
            window.renderCartPage();
        }
    }

    // ─── Sync on Login ────────────────────────────────────
    async function syncCartOnLogin() {
        const guestCart = getGuestCart();
        if (guestCart.length === 0) {
            await refreshBadge();
            return;
        }

        try {
            const courseIds = guestCart.map(item => item.courseId);
            const res = await fetch(`${API_URL}/cart/sync`, {
                method: 'POST',
                headers: authHeaders(),
                body: JSON.stringify({ courseIds })
            });
            const data = await res.json();
            if (data.success) {
                clearGuestCart();
                updateBadge(data.cartCount);
            }
        } catch { /* silent */ }
    }

    // ─── Button Binding ───────────────────────────────────
    function bindAddToCartButtons() {
        document.addEventListener('click', function (e) {
            const btn = e.target.closest('[data-cart-action="add"]');
            if (!btn) return;

            // If the course is already purchased or user is subscribed, do not add to cart
            if (btn.getAttribute('data-enrolled') === 'true') {
                return; // Let the event bubble naturally to the parent <a> tag for navigation
            }

            e.preventDefault();
            e.stopPropagation();

            addToCart(
                btn.dataset.courseId,
                btn.dataset.courseTitle,
                btn.dataset.coursePrice,
                btn.dataset.courseThumbnail,
                btn.dataset.courseOriginalPrice,
                btn.dataset.courseLevel,
                btn.dataset.courseInstructor
            );
        });
    }

    // ─── Init ─────────────────────────────────────────────
    function init() {
        bindAddToCartButtons();
        refreshBadge();

        // Drawer close handlers
        const drawerBackdrop = document.querySelector('.cart-drawer-backdrop');
        const drawerClose = document.querySelector('.cart-drawer-close');
        if (drawerBackdrop) drawerBackdrop.addEventListener('click', closeDrawer);
        if (drawerClose) drawerClose.addEventListener('click', closeDrawer);

        // ESC key closes drawer
        document.addEventListener('keydown', function (e) {
            if (e.key === 'Escape') closeDrawer();
        });
    }

    // ─── Public API ───────────────────────────────────────
    window.GVCart = {
        addToCart,
        removeFromCart,
        syncCartOnLogin,
        refreshBadge,
        openDrawer,
        closeDrawer,
        getGuestCart,
        clearGuestCart,
        isLoggedIn,
        authHeaders
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();
