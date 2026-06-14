document.addEventListener('DOMContentLoaded', () => {
    const API_URL = '/api';

    // ===== TOAST =====
    function showToast(title, message, type = 'success') {
        const toast = document.getElementById('toast');
        if (toast) {
            toast.querySelector('.toast-title').textContent = title;
            toast.querySelector('.toast-message').textContent = message;
            toast.className = `toast-notification show ${type}`;
            setTimeout(() => toast.classList.remove('show'), 4000);
        }
    }

    // ===== CONFIRM SUBSCRIPTION MODAL =====
    function showSubscribeConfirm(tierId, tierName, onConfirm) {
        // Remove existing if any
        const existing = document.getElementById('subConfirmModal');
        if (existing) existing.remove();

        const modal = document.createElement('div');
        modal.id = 'subConfirmModal';
        modal.innerHTML = `
            <div class="sub-confirm-backdrop"></div>
            <div class="sub-confirm-box">
                <div class="sub-confirm-icon">🎸</div>
                <h3>Confirm Subscription</h3>
                <p>You are about to subscribe to the <strong>${tierName}</strong> plan.</p>
                <div class="sub-confirm-actions">
                    <button class="sub-confirm-cancel">Cancel</button>
                    <button class="sub-confirm-ok">Confirm & Subscribe</button>
                </div>
            </div>
        `;
        document.body.appendChild(modal);

        // Animate in
        requestAnimationFrame(() => modal.classList.add('active'));

        const close = () => {
            modal.classList.remove('active');
            setTimeout(() => modal.remove(), 300);
        };

        modal.querySelector('.sub-confirm-backdrop').addEventListener('click', close);
        modal.querySelector('.sub-confirm-cancel').addEventListener('click', close);
        modal.querySelector('.sub-confirm-ok').addEventListener('click', () => {
            close();
            onConfirm();
        });
    }

    // ===== INJECT CONFIRM MODAL STYLES =====
    if (!document.getElementById('subConfirmStyles')) {
        const style = document.createElement('style');
        style.id = 'subConfirmStyles';
        style.textContent = `
            #subConfirmModal {
                position: fixed; inset: 0; z-index: 99999;
                display: flex; align-items: center; justify-content: center;
                opacity: 0; transition: opacity 0.25s ease;
            }
            #subConfirmModal.active { opacity: 1; }
            .sub-confirm-backdrop {
                position: absolute; inset: 0;
                background: rgba(0,0,0,0.7); backdrop-filter: blur(6px);
            }
            .sub-confirm-box {
                position: relative; z-index: 1;
                background: #1a1a20; border: 1px solid rgba(245,166,35,0.3);
                border-radius: 20px; padding: 40px 36px; max-width: 420px; width: 90%;
                text-align: center; box-shadow: 0 20px 60px rgba(0,0,0,0.5);
                transform: translateY(20px); transition: transform 0.3s cubic-bezier(0.16,1,0.3,1);
            }
            #subConfirmModal.active .sub-confirm-box { transform: translateY(0); }
            .sub-confirm-icon { font-size: 3rem; margin-bottom: 16px; }
            .sub-confirm-box h3 {
                color: #fff; font-size: 1.4rem; font-weight: 700; margin-bottom: 10px;
            }
            .sub-confirm-box p { color: #a0a0a8; font-size: 1rem; margin-bottom: 28px; }
            .sub-confirm-box p strong { color: #f5a623; }
            .sub-confirm-actions { display: flex; gap: 12px; justify-content: center; }
            .sub-confirm-cancel {
                padding: 12px 24px; border-radius: 10px; border: 1px solid rgba(255,255,255,0.15);
                background: transparent; color: #a0a0a8; cursor: pointer; font-size: 0.95rem;
                transition: all 0.2s;
            }
            .sub-confirm-cancel:hover { border-color: rgba(255,255,255,0.3); color: #fff; }
            .sub-confirm-ok {
                padding: 12px 24px; border-radius: 10px; border: none;
                background: linear-gradient(135deg, #f5a623, #e8940d); color: #0a0a0b;
                font-weight: 700; cursor: pointer; font-size: 0.95rem;
                transition: all 0.2s; box-shadow: 0 4px 15px rgba(245,166,35,0.3);
            }
            .sub-confirm-ok:hover { transform: translateY(-2px); box-shadow: 0 6px 20px rgba(245,166,35,0.4); }
            .pricing-btn.current-plan {
                background: rgba(74,222,128,0.15) !important;
                border: 1px solid rgba(74,222,128,0.4) !important;
                color: #4ade80 !important; cursor: default !important;
                pointer-events: none;
            }
        `;
        document.head.appendChild(style);
    }

    // ===== SUBSCRIPTION FLOW =====
    const handlePendingSubscription = async () => {
        const modal = document.getElementById('authModal');
        if (!modal) return;
        const pendingTierId = modal.getAttribute('data-pending-tier');
        const pendingTierName = modal.getAttribute('data-pending-tier-name') || 'this plan';
        if (pendingTierId) {
            modal.removeAttribute('data-pending-tier');
            modal.removeAttribute('data-pending-tier-name');
            const token = localStorage.getItem('userToken');
            if (token) {
                await doSubscribe(pendingTierId, pendingTierName, token);
            }
        }
    };

    async function doSubscribe(tierId, tierName, token) {
        try {
            const response = await fetch(`${API_URL}/subscriptions`, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    'Authorization': `Bearer ${token}`
                },
                body: JSON.stringify({ pricingTierId: parseInt(tierId) })
            });

            if (response.ok) {
                showToast('Subscribed! 🎸', `Welcome to the ${tierName} plan!`, 'success');
                setTimeout(() => window.location.reload(), 2000);
            } else {
                const errData = await response.json().catch(() => null);
                showToast('Subscription Failed', errData?.message || 'Failed to subscribe.', 'error');
            }
        } catch (err) {
            showToast('Error', 'Connection failed. Please try again.', 'error');
        }
    }

    window.subscribeToPlan = async function(tierId, tierName) {
        const token = localStorage.getItem('userToken');
        if (!token) {
            const modal = document.getElementById('authModal');
            if (modal) {
                modal.setAttribute('data-pending-tier', tierId);
                modal.setAttribute('data-pending-tier-name', tierName);
                openAuthModal('login');
                showToast('Sign In Required', `Please sign in to subscribe to ${tierName}.`, 'info');
            }
            return;
        }

        // Show confirmation before subscribing
        showSubscribeConfirm(tierId, tierName, () => doSubscribe(tierId, tierName, token));
    };

    // ===== MARK CURRENT PLAN ON PRICING BUTTONS =====
    async function highlightCurrentPlan() {
        const token = localStorage.getItem('userToken');

        // Always reset all buttons first (handles post-cancel state)
        document.querySelectorAll('[data-pricing-action="subscribe"]').forEach(btn => {
            btn.textContent = 'Get Started';
            btn.classList.remove('current-plan');
            btn.disabled = false;
        });

        if (!token) return;

        try {
            const res = await fetch(`${API_URL}/subscriptions/me`, {
                headers: { 'Authorization': `Bearer ${token}` }
            });
            if (!res.ok) return;

            const data = await res.json();
            // API returns { success: true, subscription: {...} | null }
            const sub = data.subscription;

            // Only highlight if there is an active subscription
            if (!sub || (sub.status && sub.status !== 'active')) return;

            const currentTierId = sub.pricingTierId ?? sub.PricingTierId;
            if (!currentTierId) return;

            document.querySelectorAll('[data-pricing-action="subscribe"]').forEach(btn => {
                const btnTierId = parseInt(btn.getAttribute('data-tier-id'));
                if (btnTierId === parseInt(currentTierId)) {
                    btn.textContent = '✓ Current Plan';
                    btn.classList.add('current-plan');
                    btn.disabled = true;
                }
            });
        } catch (err) {
            // Silent fail
        }
    }

    // ===== ATTACH LISTENERS TO PRICING BUTTONS =====
    document.querySelectorAll('[data-pricing-action="subscribe"]').forEach(button => {
        button.addEventListener('click', (e) => {
            if (button.classList.contains('current-plan')) return;
            const tierId = button.getAttribute('data-tier-id');
            const tierName = button.getAttribute('data-tier-name');
            window.subscribeToPlan(tierId, tierName);
        });
    });

    // ===== AUTHENTICATION =====
    window.openAuthModal = function(view = 'login') {
        const modal = document.getElementById('authModal');
        if (modal) {
            modal.classList.remove('d-none');
            switchAuthView(view);
        }
    };

    window.closeAuthModal = function() {
        const modal = document.getElementById('authModal');
        if (modal) modal.classList.add('d-none');
    };

    window.switchAuthView = function(view) {
        const loginView = document.getElementById('loginView');
        const signupView = document.getElementById('signupView');
        if (!loginView || !signupView) return;
        if (view === 'login') {
            loginView.classList.remove('d-none');
            signupView.classList.add('d-none');
        } else {
            loginView.classList.add('d-none');
            signupView.classList.remove('d-none');
        }
    };

    window.handleLogin = async function(e) {
        e.preventDefault();
        const email = document.getElementById('loginEmail').value;
        const password = document.getElementById('loginPassword').value;
        const errorEl = document.getElementById('loginError');

        try {
            const response = await fetch(`${API_URL}/auth/login`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ email, password })
            });
            const data = await response.json();

            if (data.success) {
                localStorage.setItem('userToken', data.token);
                localStorage.setItem('userData', JSON.stringify(data.user));

                if (data.user && data.user.role === 'Admin') {
                    localStorage.setItem('adminToken', data.token);
                    window.location.href = '/admin/dashboard';
                    return;
                }

                await updateAuthUI();
                await handlePendingSubscription();
                closeAuthModal();
                if (window.GVCart && typeof window.GVCart.syncCartOnLogin === 'function') {
                    window.GVCart.syncCartOnLogin();
                }
            } else {
                errorEl.textContent = data.message;
                errorEl.style.display = 'block';
            }
        } catch (err) {
            errorEl.textContent = 'Connection error. Please try again.';
            errorEl.style.display = 'block';
        }
    };

    window.handleSignup = async function(e) {
        e.preventDefault();
        const fullName = document.getElementById('signupName').value;
        const email = document.getElementById('signupEmail').value;
        const password = document.getElementById('signupPassword').value;
        const errorEl = document.getElementById('signupError');

        try {
            const response = await fetch(`${API_URL}/auth/register-user`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ fullName, email, password })
            });
            const data = await response.json();

            if (data.success) {
                switchAuthView('login');
                document.getElementById('loginEmail').value = email;
                document.getElementById('loginError').textContent = 'Account created! Please sign in.';
                document.getElementById('loginError').style.display = 'block';
                document.getElementById('loginError').style.background = 'rgba(74, 222, 128, 0.1)';
                document.getElementById('loginError').style.color = 'var(--success)';
                document.getElementById('loginError').style.borderColor = 'rgba(74, 222, 128, 0.2)';
            } else {
                errorEl.textContent = data.message;
                errorEl.style.display = 'block';
            }
        } catch (err) {
            errorEl.textContent = 'Connection error. Please try again.';
            errorEl.style.display = 'block';
        }
    };

    window.handleLogout = function() {
        localStorage.removeItem('userToken');
        localStorage.removeItem('userData');
        updateAuthUI();
        window.location.reload();
    };

    async function updateAuthUI() {
        const token = localStorage.getItem('userToken');
        const userData = JSON.parse(localStorage.getItem('userData') || '{}');
        const userProfile = document.getElementById('userProfile');
        const authActions = document.getElementById('authActions');
        const userFullName = document.getElementById('userFullName');
        const userDropdownName = document.getElementById('userDropdownName');

        if (token) {
            if (userProfile) userProfile.classList.remove('d-none');
            if (authActions) authActions.classList.add('d-none');
            const name = userData.fullName || 'User';
            if (userFullName) userFullName.textContent = name;
            if (userDropdownName) userDropdownName.textContent = name;

            try {
                const subResponse = await fetch(`${API_URL}/subscriptions/me`, {
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (subResponse.ok) {
                    const data = await subResponse.json();
                    const sub = data.subscription;
                    const planBadgeRow = document.getElementById('userPlanBadgeRow');
                    const planLabel = document.getElementById('userPlanLabel');

                    if (sub && sub.planName) {
                        if (planBadgeRow) planBadgeRow.style.display = 'flex';
                        if (planLabel) planLabel.textContent = sub.planName;
                    } else {
                        if (planBadgeRow) planBadgeRow.style.display = 'none';
                    }
                }
            } catch (err) {
                console.error('Failed to fetch subscription info', err);
            }
        } else {
            if (userProfile) userProfile.classList.add('d-none');
            if (authActions) authActions.classList.remove('d-none');
        }
    }

    // ===== DROPDOWN TOGGLE =====
    const dropdownToggle = document.getElementById('userDropdownToggle');
    const dropdown = document.getElementById('userDropdown');
    if (dropdownToggle && dropdown) {
        dropdownToggle.addEventListener('click', (e) => {
            e.stopPropagation();
            dropdown.classList.toggle('open');
        });
        document.addEventListener('click', () => dropdown.classList.remove('open'));
    }

    // ===== CANCEL PLAN =====
    const cancelPlanBtn = document.getElementById('cancelPlanBtn');
    if (cancelPlanBtn) {
        cancelPlanBtn.addEventListener('click', async (e) => {
            e.stopPropagation();
            if (!confirm('Are you sure you want to cancel your subscription?')) return;

            const token = localStorage.getItem('userToken');
            if (!token) return;

            try {
                const res = await fetch(`${API_URL}/subscriptions/me`, {
                    method: 'DELETE',
                    headers: { 'Authorization': `Bearer ${token}` }
                });
                if (res.ok) {
                    showToast('Plan Cancelled', 'Your subscription has been cancelled.', 'success');
                    setTimeout(() => window.location.reload(), 2000);
                } else {
                    showToast('Error', 'Failed to cancel subscription.', 'error');
                }
            } catch (err) {
                showToast('Error', 'Connection failed.', 'error');
            }
        });
    }

    // ===== INJECT DROPDOWN CSS =====
    if (!document.getElementById('dropdownStyles')) {
        const s = document.createElement('style');
        s.id = 'dropdownStyles';
        s.textContent = `
            .user-profile { position: relative; }
            .user-profile-btn {
                display: flex; align-items: center; gap: 8px;
                background: rgba(255,255,255,0.06); border: 1px solid rgba(255,255,255,0.1);
                border-radius: 10px; padding: 8px 14px; cursor: pointer; color: #fff;
                font-size: 0.9rem; font-weight: 500; transition: all 0.2s;
            }
            .user-profile-btn:hover { background: rgba(255,255,255,0.1); border-color: rgba(245,166,35,0.3); }
            .user-avatar-icon { font-size: 1rem; }
            .dropdown-arrow { transition: transform 0.2s; }
            .user-dropdown { 
                position: absolute; top: calc(100% + 10px); right: 0;
                background: #1a1a20; border: 1px solid rgba(255,255,255,0.1);
                border-radius: 14px; min-width: 220px; padding: 8px;
                box-shadow: 0 20px 40px rgba(0,0,0,0.5);
                opacity: 0; transform: translateY(-8px) scale(0.97);
                pointer-events: none; transition: all 0.2s cubic-bezier(0.16,1,0.3,1);
                z-index: 9999;
            }
            .user-dropdown.open { opacity: 1; transform: translateY(0) scale(1); pointer-events: all; }
            .user-dropdown-header { display: flex; align-items: center; gap: 12px; padding: 12px 10px; }
            .user-dropdown-avatar { font-size: 1.6rem; }
            .user-dropdown-name { color: #fff; font-weight: 600; font-size: 0.95rem; }
            .user-dropdown-plan { 
                display: flex; align-items: center; gap: 8px; margin-top: 4px;
            }
            .user-dropdown-plan > span {
                background: rgba(245,166,35,0.15); color: #f5a623;
                font-size: 0.75rem; font-weight: 700; padding: 2px 8px; border-radius: 20px;
                border: 1px solid rgba(245,166,35,0.3);
            }
            .cancel-plan-btn {
                background: rgba(248,113,113,0.1); border: 1px solid rgba(248,113,113,0.2);
                color: #f87171; font-size: 0.7rem; font-weight: 600; padding: 2px 8px;
                border-radius: 20px; cursor: pointer; transition: all 0.2s;
            }
            .cancel-plan-btn:hover { background: rgba(248,113,113,0.2); }
            .user-dropdown-divider { height: 1px; background: rgba(255,255,255,0.08); margin: 4px 0; }
            .user-dropdown-item {
                display: flex; align-items: center; gap: 8px;
                padding: 10px 12px; border-radius: 8px; color: #a0a0a8;
                font-size: 0.9rem; text-decoration: none; cursor: pointer;
                background: none; border: none; width: 100%; text-align: left;
                transition: all 0.15s;
            }
            .user-dropdown-item:hover { background: rgba(255,255,255,0.06); color: #fff; }
            .user-dropdown-logout { color: #f87171 !important; }
            .user-dropdown-logout:hover { background: rgba(248,113,113,0.1) !important; }
        `;
        document.head.appendChild(s);
    }

    updateAuthUI();
    highlightCurrentPlan();
});

