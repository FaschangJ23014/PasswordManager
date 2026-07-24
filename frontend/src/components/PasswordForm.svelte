<script>
  let { onPasswordSaved, getHeaders, isPasswordPwned, API_URL } = $props();

  let newWebsite = $state('');
  let newUsername = $state('');
  let generatedPassword = $state('');

  let strength = $derived(calculateStrength(generatedPassword));

  function generateSecurePassword() {
    const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+~}{[]:;?><";
    let password = "";
    for (let i = 0; i < 16; i++) {
      password += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    generatedPassword = password;
  }

  function calculateStrength(pwd){
    let score = 0;
    if (pwd.length > 12) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }

  async function savePassword() {
    if (!newWebsite || !newUsername || !generatedPassword) {
      alert('Bitte alle Felder ausfüllen!');
      return;
    }

    const isPwned = await isPasswordPwned(generatedPassword);
    if (isPwned) {
        const confirmSave = confirm("WARNUNG: Dieses Passwort wurde in einem Leak gefunden! Trotzdem speichern?");
        if (!confirmSave) return;
    }

    const newEntry = {
      website: newWebsite,
      username: newUsername,
      encryptedPassword: generatedPassword  
    };

    try {
      const response = await fetch(API_URL, {
        method: 'POST',
        headers: getHeaders(),
        body: JSON.stringify(newEntry)
      });

      if (response.ok) {
        newWebsite = '';
        newUsername = '';
        generatedPassword = '';
        onPasswordSaved(); // Aktualisiert die Liste im Parent
        alert('Erfolgreich verschlüsselt gespeichert! 🔒');
      } else {
        const errorText = await response.text();
        alert('Server meldet Fehler: ' + errorText);
      }
    } catch (error) {
      console.error('Fehler beim Speichern:', error);
    }
  }
</script>

<div class="card form-box">
  <h3>Neues Passwort anlegen</h3>
  <input type="text" placeholder="Website (z.B. Netflix)" bind:value={newWebsite} />
  <input type="text" placeholder="Benutzername/E-Mail" bind:value={newUsername} />
  
  <div class="generator-group">
    <input type="text" placeholder="Passwort" bind:value={generatedPassword} />
    <button class="btn-sec" onclick={generateSecurePassword}>Generieren ⚡</button>
  </div>
  
  <div class="strength-meter">
      <div class="bar" style="width: {strength * 25}%; background-color: {['#ef4444', '#f59e0b', '#fbbf24', '#10b981', '#10b981'][strength]};"></div>
  </div>
  <small style="color: #94a3b8; font-size: 0.75rem;"> Stärke: {['Sehr schwach', 'Schwach', 'Mittel', 'Sicher', 'Sehr sicher'][strength]}</small>

  <button class="btn-success" onclick={savePassword}>Speichern</button>
</div>

<style>
  .card {
    background: #1e293b;
    padding: 25px;
    border-radius: 12px;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    border: 1px solid #334155;
    text-align: left;
  }
  input {
    width: 100%;
    padding: 12px;
    margin: 10px 0;
    border-radius: 6px;
    border: 1px solid #475569;
    background: #0f172a;
    color: white;
    box-sizing: border-box;
  }
  button {
    padding: 12px;
    background: #38bdf8;
    color: #0f172a;
    border: none;
    border-radius: 6px;
    font-weight: bold;
    cursor: pointer;
    transition: 0.2s;
  }
  .btn-sec { background: #64748b; color: white; }
  .btn-success { width: 100%; background: #10b981; color: white; margin-top: 15px; }
  .generator-group { display: flex; gap: 10px; align-items: center; }
  .generator-group input { flex-grow: 1; }
  .strength-meter {
    height: 6px;
    width: 100%;
    background: #334155;
    border-radius: 3px;
    margin: 8px 0 4px 0;
    overflow: hidden;
  }
  .bar { height: 100%; transition: width 0.3s ease, background-color 0.3s ease; }
</style>