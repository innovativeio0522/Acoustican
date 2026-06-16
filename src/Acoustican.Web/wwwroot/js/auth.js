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
        let pendingTierId = modal ? modal.getAttribute('data-pending-tier') : null;
        let pendingTierName = modal ? modal.getAttribute('data-pending-tier-name') : 'this plan';

        if (!pendingTierId) {
            pendingTierId = localStorage.getItem('pendingTierId');
            pendingTierName = localStorage.getItem('pendingTierName') || 'this plan';
        }

        if (pendingTierId) {
            if (modal) {
                modal.removeAttribute('data-pending-tier');
                modal.removeAttribute('data-pending-tier-name');
            }
            localStorage.removeItem('pendingTierId');
            localStorage.removeItem('pendingTierName');

            const token = localStorage.getItem('userToken');
            if (token) {
                await doSubscribe(pendingTierId, pendingTierName, token);
            }
        }
    };
    window.handlePendingSubscription = handlePendingSubscription;

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

            const data = await response.json();

            if (response.ok && data.success) {
                const sub = data.subscription;
                
                if (!sub.requiresPayment) {
                    showToast('Subscribed! 🎸', `Welcome to the ${tierName} plan!`, 'success');
                    setTimeout(() => window.location.reload(), 2000);
                    return;
                }

                // Requires Payment
                const verifySubscriptionPayment = async (razorpayOrderId, razorpayPaymentId, razorpaySignature) => {
                    try {
                        const verifyRes = await fetch(`${API_URL}/subscriptions/verify`, {
                            method: 'POST',
                            headers: {
                                'Content-Type': 'application/json',
                                'Authorization': `Bearer ${token}`
                            },
                            body: JSON.stringify({
                                razorpayOrderId,
                                razorpayPaymentId,
                                razorpaySignature
                            })
                        });
                        const verifyData = await verifyRes.json();

                        if (verifyRes.ok && verifyData.success) {
                            showToast('Payment Verified! 🎸', `Welcome to the ${tierName} plan!`, 'success');
                            setTimeout(() => window.location.reload(), 2000);
                        } else {
                            showToast('Verification Failed', verifyData.message || 'Verification failed.', 'error');
                        }
                    } catch {
                        showToast('Error', 'Payment verification connection failed.', 'error');
                    }
                };

                // Check if we are running in Mock Mode
                if (sub.razorpayOrderId && sub.razorpayOrderId.startsWith("order_mock_")) {
                    showSubscriptionMockModal(sub, tierName,
                        (response) => {
                            verifySubscriptionPayment(response.razorpay_order_id, response.razorpay_payment_id, response.razorpay_signature);
                        },
                        (errorMsg) => {
                            showToast('Cancelled', errorMsg, 'info');
                        }
                    );
                } else {
                    // Real Razorpay Flow: load SDK dynamically first
                    loadRazorpayScript(() => {
                        const options = {
                            "key": sub.razorpayKey,
                            "amount": Math.round(sub.planPrice * 100), // in paise
                            "currency": "INR",
                            "name": "Acoustican",
                            "description": `Subscription to ${tierName} plan`,
                            "order_id": sub.razorpayOrderId,
                            "handler": function (response) {
                                verifySubscriptionPayment(response.razorpay_order_id, response.razorpay_payment_id, response.razorpay_signature);
                            },
                            "prefill": {
                                "name": "",
                                "email": ""
                            },
                            "theme": {
                                "color": "#f5a623"
                            }
                        };

                        // Retrieve prefill info if possible
                        try {
                            const userData = JSON.parse(localStorage.getItem('userData'));
                            if (userData) {
                                options.prefill.name = userData.fullName || userData.username || "";
                                options.prefill.email = userData.email || "";
                            }
                        } catch (_) {}

                        const rzp = new Razorpay(options);
                        rzp.on('payment.failed', function (response) {
                            showToast('Payment Failed', response.error.description, 'error');
                        });
                        rzp.open();
                    });
                }
            } else {
                showToast('Subscription Failed', data?.message || 'Failed to subscribe.', 'error');
            }
        } catch (err) {
            showToast('Error', 'Connection failed. Please try again.', 'error');
        }
    }

    function loadRazorpayScript(callback) {
        if (window.Razorpay) {
            callback();
            return;
        }
        const script = document.createElement('script');
        script.src = 'https://checkout.razorpay.com/v1/checkout.js';
        script.onload = callback;
        script.onerror = () => {
            showToast('Error', 'Failed to load payment SDK. Please check your connection.', 'error');
        };
        document.head.appendChild(script);
    }

    function showSubscriptionMockModal(sub, planName, onConfirm, onCancel) {
        const existing = document.getElementById('mockSubPaymentOverlay');
        if (existing) existing.remove();

        const overlay = document.createElement('div');
        overlay.id = 'mockSubPaymentOverlay';
        overlay.style.position = 'fixed';
        overlay.style.top = '0';
        overlay.style.left = '0';
        overlay.style.width = '100%';
        overlay.style.height = '100%';
        overlay.style.backgroundColor = 'rgba(10, 10, 12, 0.85)';
        overlay.style.backdropFilter = 'blur(12px)';
        overlay.style.display = 'flex';
        overlay.style.alignItems = 'center';
        overlay.style.justifyContent = 'center';
        overlay.style.zIndex = '99999';
        overlay.style.fontFamily = "'Inter', sans-serif";

        const card = document.createElement('div');
        card.style.background = 'linear-gradient(135deg, #1e1e24, #121216)';
        card.style.border = '1px solid rgba(255, 255, 255, 0.08)';
        card.style.borderRadius = '20px';
        card.style.padding = '32px';
        card.style.width = '90%';
        card.style.maxWidth = '420px';
        card.style.color = '#ffffff';
        card.style.boxShadow = '0 20px 40px rgba(0, 0, 0, 0.5)';
        card.style.textAlign = 'center';

        card.innerHTML = `
            <div style="font-size: 3rem; margin-bottom: 16px;">🎸</div>
            <h3 style="margin: 0 0 8px; font-size: 1.5rem; font-weight: 700; background: linear-gradient(90deg, #f5a623, #f8e71c); -webkit-background-clip: text; -webkit-text-fill-color: transparent;">
                Mock Plan Subscription
            </h3>
            <div style="display: inline-block; padding: 4px 10px; font-size: 0.75rem; background: rgba(245, 166, 35, 0.15); border: 1px solid rgba(245, 166, 35, 0.3); border-radius: 20px; color: #f5a623; margin-bottom: 24px; font-weight: 600; text-transform: uppercase; letter-spacing: 0.5px;">
                Test Mode (No Keys)
            </div>
            
            <div style="background: rgba(255, 255, 255, 0.03); border: 1px solid rgba(255, 255, 255, 0.05); border-radius: 12px; padding: 16px; text-align: left; margin-bottom: 24px;">
                <div style="display: flex; justify-content: space-between; margin-bottom: 8px;">
                    <span style="color: rgba(255, 255, 255, 0.5); font-size: 0.9rem;">Plan Name</span>
                    <span style="font-weight: 600; font-size: 0.9rem; color: #f5a623;">${planName}</span>
                </div>
                <div style="display: flex; justify-content: space-between;">
                    <span style="color: rgba(255, 255, 255, 0.5); font-size: 0.9rem;">Amount Due</span>
                    <span style="font-weight: 700; color: #4ade80; font-size: 1.1rem;">₹${sub.planPrice.toLocaleString('en-IN')} / ${sub.billingPeriod || 'month'}</span>
                </div>
            </div>

            <p style="color: rgba(255, 255, 255, 0.6); font-size: 0.9rem; margin-bottom: 24px; line-height: 1.5;">
                Simulate a transaction outcome to verify plan activation.
            </p>

            <div style="display: flex; flex-direction: column; gap: 12px;">
                <button id="btnMockSubSuccess" style="background: #22c55e; border: none; border-radius: 10px; padding: 12px; color: #fff; font-weight: 600; font-size: 0.95rem; cursor: pointer; transition: all 0.2s;">
                    Simulate Successful Payment
                </button>
                <button id="btnMockSubFailure" style="background: rgba(239, 68, 68, 0.1); border: 1px solid rgba(239, 68, 68, 0.2); border-radius: 10px; padding: 12px; color: #ef4444; font-weight: 600; font-size: 0.95rem; cursor: pointer; transition: all 0.2s;">
                    Simulate Failed Payment
                </button>
                <button id="btnMockSubCancel" style="background: transparent; border: none; padding: 8px; color: rgba(255, 255, 255, 0.4); font-size: 0.85rem; cursor: pointer; text-decoration: underline;">
                    Cancel Subscription
                </button>
            </div>
        `;

        overlay.appendChild(card);
        document.body.appendChild(overlay);

        document.getElementById('btnMockSubSuccess').onclick = () => {
            overlay.remove();
            onConfirm({
                razorpay_order_id: sub.razorpayOrderId,
                razorpay_payment_id: "pay_mock_" + Math.random().toString(36).substring(2, 11),
                razorpay_signature: "sig_mock_" + Math.random().toString(36).substring(2, 11)
            });
        };

        document.getElementById('btnMockSubFailure').onclick = () => {
            overlay.remove();
            onCancel("Payment simulation failed.");
        };

        document.getElementById('btnMockSubCancel').onclick = () => {
            overlay.remove();
            onCancel("Subscription cancelled by user.");
        };
    }

    window.subscribeToPlan = async function(tierId, tierName) {
        const token = localStorage.getItem('userToken');
        if (!token) {
            const modal = document.getElementById('authModal');
            if (modal) {
                modal.setAttribute('data-pending-tier', tierId);
                modal.setAttribute('data-pending-tier-name', tierName);
                localStorage.setItem('pendingTierId', tierId);
                localStorage.setItem('pendingTierName', tierName);
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

                await window.updateAuthUI();
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
        window.updateAuthUI();
        window.location.reload();
    };

    window.updateAuthUI = async function updateAuthUI() {
        const token = localStorage.getItem('userToken');
        const userData = JSON.parse(localStorage.getItem('userData') || '{}');
        const userProfile = document.getElementById('userProfile');
        const authActions = document.getElementById('authActions');
        const userFullName = document.getElementById('userFullName');
        const userDropdownName = document.getElementById('userDropdownName');

        if (token) {
            if (userProfile) userProfile.classList.remove('d-none');
            if (authActions) authActions.classList.add('d-none');
            const name = userData.fullName || userData.FullName || 'User';
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

