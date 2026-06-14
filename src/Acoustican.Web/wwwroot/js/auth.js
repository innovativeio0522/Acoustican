document.addEventListener('DOMContentLoaded', () => {
    const API_URL = '/api';

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
        if (modal) {
            modal.classList.add('d-none');
        }
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
                
                updateAuthUI();
                closeAuthModal();
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
            const response = await fetch(`${API_URL}/auth/register`, {
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

    function updateAuthUI() {
        const token = localStorage.getItem('userToken');
        const userData = JSON.parse(localStorage.getItem('userData') || '{}');
        const userProfile = document.getElementById('userProfile');
        const authActions = document.getElementById('authActions');
        const userFullName = document.getElementById('userFullName');

        if (token) {
            if (userProfile) userProfile.classList.remove('d-none');
            if (authActions) authActions.classList.add('d-none');
            if (userFullName) userFullName.textContent = userData.fullName || 'User';
        } else {
            if (userProfile) userProfile.classList.add('d-none');
            if (authActions) authActions.classList.remove('d-none');
        }
    }

    updateAuthUI();
});
