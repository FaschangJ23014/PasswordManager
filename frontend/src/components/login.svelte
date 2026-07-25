<script>
  let { onAuthSuccess } = $props();

  let username = $state('');
  let password = $state('');
  let isRegistering = $state(false);

  let showPassword = $state(false);
  let isLoading = $state(false);
  let errorMessage = $state('');
  let successMessage = $state('');

  async function handleAuth() {
    if (!username || !password) {
      errorMessage = 'Bitte fülle alle Felder aus!';
      return;
    }

    isLoading = true;
    errorMessage = '';
    successMessage = '';

    const endpoint = isRegistering ? 'register' : 'login';
    
    try {
      const response = await fetch(`https://passwordmanager-k5za.onrender.com/api/auth/${endpoint}`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ username, password })
      });

      if (response.ok) {
        if (isRegistering) {
          successMessage = 'Registrierung erfolgreich! Du kannst dich jetzt einloggen.';
          isRegistering = false;
          password = '';
        } else {
          const data = await response.json();
          localStorage.setItem('token', data.token);
          onAuthSuccess();
        }
      } else {
        const err = await response.text();
        errorMessage = err || 'Ein Fehler ist aufgetreten.';
      }
    } catch (err) {
      errorMessage = 'Netzwerkfehler. Bitte versuche es später erneut.';
    } finally {
      isLoading = false;
    }
  }

  function switchTab(toRegister) {
    isRegistering = toRegister;
    errorMessage = '';
    successMessage = '';
  }
</script>

<div class="login-container-wrapper">
  <div class="login-wrapper">
    <div class="card glass">
      <!-- Header / Branding -->
      <div class="brand">
        <div class="shield-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="40" height="40"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
        </div>
        <h2>ShieldVault</h2>
        <p class="subtitle">Dein sicherer Passwort-Tresor</p>
      </div>

      <!-- Segmented Tab Switcher -->
      <div class="tabs">
        <button 
          class="tab-btn" 
          class:active={!isRegistering} 
          onclick={() => switchTab(false)}>
          Einloggen
        </button>
        <button 
          class="tab-btn" 
          class:active={isRegistering} 
          onclick={() => switchTab(true)}>
          Registrieren
        </button>
      </div>

      <!-- Inline Status-Nachrichten -->
      {#if errorMessage}
        <div class="alert error">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
          {errorMessage}
        </div>
      {/if}
      {#if successMessage}
        <div class="alert success">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
          {successMessage}
        </div>
      {/if}

      <!-- Formular -->
      <form onsubmit={(e) => { e.preventDefault(); handleAuth(); }}>
        <div class="input-group">
          <label for="username">Benutzername</label>
          <div class="input-wrapper">
            <span class="icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>
            </span>
            <input 
              id="username"
              type="text" 
              placeholder="z.B. max_mustermann" 
              bind:value={username} 
              autocomplete="username"
            />
          </div>
        </div>

        <div class="input-group">
          <label for="password">Passwort</label>
          <div class="input-wrapper">
            <span class="icon">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
            </span>
            <input 
              id="password"
              type={showPassword ? 'text' : 'password'} 
              placeholder="••••••••••••" 
              bind:value={password}
              autocomplete="current-password"
            />
            <button 
              type="button" 
              class="toggle-pwd" 
              onclick={() => showPassword = !showPassword}>
              {#if showPassword}
                <!-- Auge offen -->
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M1 12s4-8 11-8 11 8 11 8-4 8-11 8-11-8-11-8z"/><circle cx="12" cy="12" r="3"/></svg>
              {:else}
                <!-- Auge zu -->
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><path d="M17.94 17.94A10.07 10.07 0 0 1 12 20c-7 0-11-8-11-8a18.45 18.45 0 0 1 5.06-5.94M9.9 4.24A9.12 9.12 0 0 1 12 4c7 0 11 8 11 8a18.5 18.5 0 0 1-2.16 3.19m-6.72-1.07a3 3 0 1 1-4.24-4.24"/><line x1="1" y1="1" x2="23" y2="23"/></svg>
              {/if}
            </button>
          </div>
        </div>

        <button type="submit" class="btn-submit" disabled={isLoading}>
          {#if isLoading}
            <span class="spinner"></span> Wird geladen...
          {:else}
            {isRegistering ? 'Account erstellen' : 'Anmelden'}
          {/if}
        </button>
      </form>
    </div>
  </div>
</div>

<style>
  .login-container-wrapper {
    display: flex;
    flex-direction: column;
    min-height: 85vh;
    justify-content: space-between;
  }

  .login-wrapper {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 20px 0;
    flex: 1;
  }

  /* Glassmorphism Card Style */
  .card.glass {
    background: rgba(30, 41, 59, 0.7);
    backdrop-filter: blur(12px);
    -webkit-backdrop-filter: blur(12px);
    border: 1px solid rgba(255, 255, 255, 0.1);
    border-radius: 20px;
    padding: 35px;
    width: 100%;
    max-width: 420px;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4),
                0 0 20px rgba(56, 189, 248, 0.1);
  }

  .brand {
    text-align: center;
    margin-bottom: 25px;
  }

  .shield-icon {
    color: #38bdf8;
    background: rgba(56, 189, 248, 0.1);
    width: 65px;
    height: 65px;
    margin: 0 auto 12px auto;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 16px;
    border: 1px solid rgba(56, 189, 248, 0.2);
    filter: drop-shadow(0 0 10px rgba(56, 189, 248, 0.3));
  }

  h2 {
    margin: 0;
    color: #f8fafc;
    font-size: 1.8rem;
    font-weight: 700;
    letter-spacing: -0.5px;
  }

  .subtitle {
    margin: 4px 0 0 0;
    color: #94a3b8;
    font-size: 0.9rem;
  }

  /* Segmented Control / Tabs */
  .tabs {
    display: flex;
    background: #0f172a;
    padding: 4px;
    border-radius: 12px;
    margin-bottom: 25px;
    border: 1px solid #334155;
  }

  .tab-btn {
    flex: 1;
    background: transparent;
    border: none;
    color: #94a3b8;
    padding: 10px;
    font-size: 0.9rem;
    font-weight: 600;
    border-radius: 8px;
    cursor: pointer;
    transition: all 0.25s ease;
  }

  .tab-btn.active {
    background: #38bdf8;
    color: #0f172a;
    box-shadow: 0 2px 8px rgba(56, 189, 248, 0.3);
  }

  /* Form Elements */
  .input-group {
    display: flex;
    flex-direction: column;
    gap: 6px;
    margin-bottom: 18px;
    text-align: left;
  }

  label {
    font-size: 0.82rem;
    font-weight: 600;
    color: #cbd5e1;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .input-wrapper {
    position: relative;
    display: flex;
    align-items: center;
  }

  .input-wrapper .icon {
    position: absolute;
    left: 14px;
    display: flex;
    align-items: center;
    color: #94a3b8;
    pointer-events: none;
  }

  input {
    width: 100%;
    padding: 12px 42px 12px 42px;
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    color: white;
    font-size: 0.95rem;
    transition: all 0.2s ease;
    box-sizing: border-box;
  }

  input:focus {
    outline: none;
    border-color: #38bdf8;
    box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.2);
  }

  .toggle-pwd {
    position: absolute;
    right: 10px;
    background: transparent;
    border: none;
    cursor: pointer;
    display: flex;
    align-items: center;
    color: #94a3b8;
    padding: 6px;
    transition: color 0.2s;
  }

  .toggle-pwd:hover {
    color: #38bdf8;
  }

  /* Submit Button */
  .btn-submit {
    width: 100%;
    padding: 14px;
    margin-top: 10px;
    background: linear-gradient(135deg, #38bdf8 0%, #0284c7 100%);
    color: #0f172a;
    border: none;
    border-radius: 10px;
    font-size: 1rem;
    font-weight: 700;
    cursor: pointer;
    transition: all 0.2s ease;
    box-shadow: 0 4px 12px rgba(56, 189, 248, 0.3);
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 8px;
  }

  .btn-submit:hover:not(:disabled) {
    transform: translateY(-2px);
    box-shadow: 0 6px 18px rgba(56, 189, 248, 0.4);
  }

  .btn-submit:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  /* Alerts */
  .alert {
    padding: 10px 14px;
    border-radius: 8px;
    font-size: 0.85rem;
    margin-bottom: 18px;
    text-align: left;
    display: flex;
    align-items: center;
    gap: 10px;
  }

  .alert.error {
    background: rgba(239, 68, 68, 0.15);
    border: 1px solid rgba(239, 68, 68, 0.4);
    color: #fca5a5;
  }

  .alert.success {
    background: rgba(16, 185, 129, 0.15);
    border: 1px solid rgba(16, 185, 129, 0.4);
    color: #6ee7b7;
  }

  /* Loading Spinner Animation */
  .spinner {
    width: 16px;
    height: 16px;
    border: 2px solid #0f172a;
    border-top-color: transparent;
    border-radius: 50%;
    animation: spin 0.8s linear infinite;
  }

  @keyframes spin {
    to { transform: rotate(360deg); }
  }
</style>