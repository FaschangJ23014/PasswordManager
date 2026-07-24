<script>
  let { onAuthSuccess } = $props();

  let username = $state('');
  let password = $state('');
  let isRegistering = $state(false);

  //Verbessert die UI
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

<div class="login-wrapper">
  <div class="card glass">
    <!-- Header / Branding -->
    <div class="brand">
      <div class="shield-icon">🛡️</div>
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
      <div class="alert error">⚠️ {errorMessage}</div>
    {/if}
    {#if successMessage}
      <div class="alert success">✅ {successMessage}</div>
    {/if}

    <!-- Formular -->
    <form onsubmit={(e) => { e.preventDefault(); handleAuth(); }}>
      <div class="input-group">
        <label for="username">Benutzername</label>
        <div class="input-wrapper">
          <span class="icon">👤</span>
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
          <span class="icon">🔑</span>
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
            {showPassword ? '👁️' : '🙈'}
          </button>
        </div>
      </div>

      <button type="submit" class="btn-submit" disabled={isLoading}>
        {#if isLoading}
          <span class="spinner"></span> Laden...
        {:else}
          {isRegistering ? 'Account erstellen ✨' : 'Anmelden 🚀'}
        {/if}
      </button>
    </form>
  </div>
</div>

<style>
  .login-wrapper {
    display: flex;
    justify-content: center;
    align-items: center;
    padding: 20px 0;
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
    font-size: 3rem;
    margin-bottom: 5px;
    filter: drop-shadow(0 0 10px rgba(56, 189, 248, 0.5));
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
    font-size: 1rem;
    pointer-events: none;
    opacity: 0.7;
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
    font-size: 1.1rem;
    padding: 4px;
    opacity: 0.7;
    transition: opacity 0.2s;
  }

  .toggle-pwd:hover {
    opacity: 1;
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