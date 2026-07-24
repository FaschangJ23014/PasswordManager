<script>
  let { onPasswordSaved, getHeaders, isPasswordPwned, API_URL } = $props();

  let newWebsite = $state('');
  let newUsername = $state('');
  let generatedPassword = $state('');

  //Verbesserung der UI
  let isLoading = $state(false);
  let error = $state('');
  let successMessage = $state('');

  let strength = $derived(calculateStrength(generatedPassword));

  function generateSecurePassword() {
    const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+~}{[]:;?><";
    let password = "";
    for (let i = 0; i < 16; i++) {
      password += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    generatedPassword = password;
    error = '';
  }

  function calculateStrength(pwd) {
    if (!pwd) return 0;
    let score = 0;
    if (pwd.length > 12) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }

  async function savePassword() {
    if (!newWebsite || !newUsername || !generatedPassword) {
      error = 'Bitte alle Felder ausfüllen!';
      successMessage = '';
      return;
    }
    
    isLoading = true;
    error = '';
    successMessage = '';

    try {
      const isPwned = await isPasswordPwned(generatedPassword);
      if (isPwned) {
        const confirmSave = confirm("WARNUNG: Dieses Passwort wurde in einem Leak gefunden! Trotzdem speichern?");
        if (!confirmSave) {
          isLoading = false;
          return;
        }
      }

      const newEntry = {
        website: newWebsite,
        username: newUsername,
        encryptedPassword: generatedPassword  
      };

      const response = await fetch(API_URL, {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify(newEntry)
      });

      if (response.ok) {
        newWebsite = '';
        newUsername = '';
        generatedPassword = '';
        successMessage = 'Erfolgreich verschlüsselt gespeichert!';
        onPasswordSaved(); 
      } else {
        const errorText = await response.text();
        error = 'Server meldet Fehler: ' + errorText;
      }
    } catch (err) {
      error = 'Netzwerkfehler beim Speichern.';
    } finally {
      isLoading = false;
    }
  }
</script>

<div class="card glass">
  <div class="form-header">
    <div class="icon-box">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20" height="20"><line x1="12" y1="5" x2="12" y2="19"/><line x1="5" y1="12" x2="19" y2="12"/></svg>
    </div>
    <h3>Neues Passwort anlegen</h3>
  </div>

  {#if error}
    <div class="alert error">
      <svg class="alert-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/></svg>
      <span>{error}</span>
    </div>
  {/if}

  {#if successMessage}
    <div class="alert success">
      <svg class="alert-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2"><path d="M22 11.08V12a10 10 0 1 1-5.93-9.14"/><polyline points="22 4 12 14.01 9 11.01"/></svg>
      <span>{successMessage}</span>
    </div>
  {/if}

  <form onsubmit={(e) => { e.preventDefault(); savePassword(); }}>
    <div class="input-group">
      <label for="website">Website / Dienst</label>
      <input id="website" type="text" placeholder="z.B. Netflix" bind:value={newWebsite} />
    </div>

    <div class="input-group">
      <label for="username">Benutzername / E-Mail</label>
      <input id="username" type="text" placeholder="name@example.com" bind:value={newUsername} />
    </div>

    <div class="input-group">
      <label for="password">Passwort</label>
      <div class="generator-group">
        <input id="password" type="text" placeholder="Sicheres Passwort eingeben oder generieren" bind:value={generatedPassword} />
        <button type="button" class="btn-sec" onclick={generateSecurePassword}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><polygon points="13 2 3 14 12 14 11 22 21 10 12 10 13 2"/></svg>
          Generieren
        </button>
      </div>
      
      {#if generatedPassword}
        <div class="strength-container">
          <div class="strength-meter">
            <div class="bar" style="width: {strength * 25}%; background-color: {['#ef4444', '#f59e0b', '#fbbf24', '#10b981', '#10b981'][strength]};"></div>
          </div>
          <small class="strength-text">Stärke: {['Sehr schwach', 'Schwach', 'Mittel', 'Sicher', 'Sehr sicher'][strength]}</small>
        </div>
      {/if}
    </div>

    <button type="submit" class="btn-success" disabled={isLoading}>
      {isLoading ? 'Wird gespeichert...' : 'Passwort im Tresor sichern'}
    </button>
  </form>
</div>

<style>
  .card.glass {
    background: rgba(30, 41, 59, 0.75);
    backdrop-filter: blur(16px);
    -webkit-backdrop-filter: blur(16px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    border-radius: 20px;
    padding: 25px;
    margin-bottom: 25px;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4);
    text-align: left;
  }

  .form-header {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 20px;
  }

  .icon-box {
    background: rgba(56, 189, 248, 0.1);
    color: #38bdf8;
    padding: 8px;
    border-radius: 10px;
    display: flex;
    border: 1px solid rgba(56, 189, 248, 0.2);
  }

  h3 {
    margin: 0;
    color: #f8fafc;
    font-size: 1.2rem;
  }

  .input-group {
    display: flex;
    flex-direction: column;
    gap: 6px;
    margin-bottom: 15px;
  }

  label {
    font-size: 0.75rem;
    font-weight: 600;
    color: #cbd5e1;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  input {
    width: 100%;
    padding: 12px 14px;
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    color: white;
    font-size: 0.9rem;
    box-sizing: border-box;
    transition: all 0.2s ease;
  }

  input:focus {
    outline: none;
    border-color: #38bdf8;
    box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.2);
  }

  .generator-group {
    display: flex;
    gap: 10px;
    align-items: center;
  }

  .generator-group input {
    margin: 0;
  }

  .btn-sec {
    background: #334155;
    color: #f8fafc;
    border: 1px solid #475569;
    padding: 12px 16px;
    border-radius: 10px;
    font-weight: 600;
    font-size: 0.9rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 6px;
    white-space: nowrap;
    transition: background 0.2s;
  }

  .btn-sec:hover {
    background: #475569;
  }

  .strength-container {
    margin-top: 6px;
  }

  .strength-meter {
    height: 6px;
    width: 100%;
    background: #334155;
    border-radius: 3px;
    overflow: hidden;
  }

  .bar {
    height: 100%;
    transition: width 0.3s ease, background-color 0.3s ease;
  }

  .strength-text {
    color: #94a3b8;
    font-size: 0.75rem;
    margin-top: 4px;
    display: inline-block;
  }

  .btn-success {
    width: 100%;
    padding: 14px;
    background: linear-gradient(135deg, #38bdf8 0%, #0284c7 100%);
    color: #0f172a;
    border: none;
    border-radius: 10px;
    font-weight: 700;
    cursor: pointer;
    transition: transform 0.2s, box-shadow 0.2s;
    box-shadow: 0 4px 12px rgba(56, 189, 248, 0.3);
    margin-top: 5px;
  }

  .btn-success:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 6px 18px rgba(56, 189, 248, 0.4);
  }

  .btn-success:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  .alert {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 10px 14px;
    border-radius: 8px;
    font-size: 0.85rem;
    margin-bottom: 15px;
  }

  .alert-icon {
    width: 18px;
    height: 18px;
    flex-shrink: 0;
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
</style>