<script>
  import { onMount } from 'svelte';

  onMount(() => {
  const token = localStorage.getItem('token');
  if (token) {
    isAuthenticated = true;
    loadPasswords();
  }
});

  // Svelte 5 Runes für Reaktivität
  let isAuthenticated = $state(false);
  let searchQuery = $state('');
  let debounceTimer;
  /** @type {any[]} */
  let passwords = $state([]);

  /** @type {any[]} */
  let pwnedList = $state([]);
  
  // Für das neue Passwort-Formular
  let newWebsite = $state('');
  let newUsername = $state('');
  let generatedPassword = $state('');

  //Variablen fürs Login
  let username = $state('');
  let password = $state('');
  let isRegistering = $state(false);

  //Berechnetes Array für die Suche
  let filteredPasswords = $derived.by(() => {
  if (!passwords || passwords.length === 0) return [];
  
  const query = searchQuery.toLowerCase();
  return passwords.filter(p => {
    const website = (p.website ?? p.Website ?? "").toLowerCase();
    return website.includes(query);
  });
});

  //Für die Automatische Berechnung: Reaktiv
  let strength = $derived(calculateStrength(generatedPassword));

  const API_URL = 'https://passwordmanager-k5za.onrender.com/api/passwords';

  //login
  async function handleAuth() {
    const endpoint = isRegistering ? 'register' : 'login';
    const response = await fetch(`https://passwordmanager-k5za.onrender.com/api/auth/${endpoint}`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ username, password })
    });

    if (response.ok) {
      if (isRegistering) {
        alert('Registrierung erfolgreich! Du kannst dich jetzt einloggen.');
        isRegistering = false; // Zurück zum Login
      } else {
        const data = await response.json();
        localStorage.setItem('token', data.token);
        isAuthenticated = true;
        loadPasswords();
      }
    } else {
      const err = await response.text();
      alert(`${isRegistering ? 'Registrierung' : 'Login'} fehlgeschlagen: ${err}`);
    }
  }

//Fürs Abmelden
function logout() {
  localStorage.removeItem('token');
  isAuthenticated = false;
  passwords = [];
}


  // Helper, um den Header zu bauen
  function getHeaders() {
    const token = localStorage.getItem('token');
    return {
    'Content-Type': 'application/json',
    'Authorization': `Bearer ${token}`
    };
}

  // 1. Passwörter von der API laden
  async function loadPasswords() {
    try {
    const response = await fetch(API_URL, {
      method: 'GET',
      headers: getHeaders() 
    });

    if (response.ok) {
      passwords = await response.json();
      isAuthenticated = true;
    } else {
      isAuthenticated = false;
    }
  } catch (error) {
    console.error('Fehler:', error);
  }
  }

  // 2. Neues Passwort generieren
  function generateSecurePassword() {
    const chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789!@#$%^&*()_+~}{[]:;?><";
    let password = "";
    for (let i = 0; i < 16; i++) {
      password += chars.charAt(Math.floor(Math.random() * chars.length));
    }
    generatedPassword = password;
  }

  // 3. Passwort in der Datenbank speichern
  async function savePassword() {
    if (!newWebsite || !newUsername || !generatedPassword) {
      alert('Bitte alle Felder ausfüllen!');
      return;
    }

    // Prüfung VOR dem Speichern
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
        loadPasswords();
        alert('Erfolgreich verschlüsselt gespeichert! 🔒');
      } else {
        const errorText = await response.text();
        alert('Server meldet Fehler: ' + errorText);
      }
    } catch (error) {
      console.error('Fehler beim Speichern:', error);
    }
  }

  async function checkAllPasswords() {
    pwnedList = []; 
    for (const p of passwords) {
        const pwd = p.EncryptedPassword || p.encryptedPassword;
        if (await isPasswordPwned(pwd)) {
            pwnedList.push(p.Id || p.id);
        }
    }
    if (pwnedList.length > 0) alert(`Gefahr! ${pwnedList.length} Passwörter sind in Leaks gefunden worden.`);
    else alert("Alles sicher! Keine Leaks gefunden.");
  }

  // 4. In die Zwischenablage kopieren
  function copyToClipboard(text) {
    navigator.clipboard.writeText(text);
    alert('Passwort in die Zwischenablage kopiert! 📋');
  }

  // 5. NEU: Eintrag aus der Datenbank löschen
  async function deletePassword(id) {
    if (!confirm('Willst du dieses Passwort wirklich für immer löschen?')) return;

    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'DELETE',
        headers: getHeaders()
      });

      if (response.ok) {
        loadPasswords(); // Liste sofort neu laden, damit der Eintrag verschwindet!
      } else {
        const errorText = await response.text();
        alert('Fehler beim Löschen: ' + errorText);
      }
    } catch (error) {
      console.error('Fehler beim Löschen:', error);
    }
  }

  function calculateStrength(pwd){
    let score = 0;
    if (pwd.length > 12) score++;
    if (/[A-Z]/.test(pwd)) score++;
    if (/[0-9]/.test(pwd)) score++;
    if (/[^A-Za-z0-9]/.test(pwd)) score++;
    return score;
  }

  function handleSearch() {
    clearTimeout(debounceTimer);
    debounceTimer = setTimeout(() => {
      loadPasswords();
    }, 500);
  }

  function exportPasswords() {
    // Header der CSV
    let csvContent = "data:text/csv;charset=utf-8,Website,Username,Password\n";
    
    // Daten hinzufügen
    passwords.forEach(p => {
        csvContent += `${p.website},${p.username},${p.encryptedPassword}\n`;
    });

    // Download-Logik
    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", "my_vault_export.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
}

//HIBP-Funktion
async function isPasswordPwned(password) {
    // 1. SHA-1 Hash erzeugen
    const encoder = new TextEncoder();
    const data = encoder.encode(password);
    const hashBuffer = await crypto.subtle.digest('SHA-1', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('').toUpperCase();

    // 2. Range-Suche (nur die ersten 5 Zeichen)
    const prefix = hashHex.substring(0, 5);
    const suffix = hashHex.substring(5);

    const response = await fetch(`https://api.pwnedpasswords.com/range/${prefix}`);
    const text = await response.text();

    // 3. Prüfen, ob der restliche Teil (Suffix) in der Antwort enthalten ist
    return text.includes(suffix);
}

</script>

<main class="container">
  <div class="header">
    <h1>🛡️ ShieldVault</h1>
    {#if isAuthenticated}
      <button class="btn-logout" onclick={logout}>Logout</button>
    {/if}
  </div>

  {#if !isAuthenticated}
    <div class="card login-box">
      <h3>{isRegistering ? 'Account erstellen' : 'Login'}</h3>
      
      <input type="text" placeholder="Username" bind:value={username} />
      <input type="password" placeholder="Passwort" bind:value={password} />
      
      <button onclick={handleAuth}>
        {isRegistering ? 'Registrieren' : 'Einloggen'}
      </button>

      <p style="margin-top: 15px; font-size: 0.85rem; cursor: pointer;" 
         onclick={() => isRegistering = !isRegistering}>
        {isRegistering ? 'Schon registriert? Hier einloggen.' : 'Noch keinen Account? Jetzt registrieren.'}
      </p>
    </div>
  {:else}
    
    <div class="dashboard">
      
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

      <div class="card list-box">
  <h3>Deine Passwörter</h3>
  <input type="text" placeholder="🔍 Nach Website suchen..." bind:value={searchQuery} />

  {#if passwords.length === 0}
    <p class="empty-msg">Keine Einträge gefunden.</p>
  {:else}
    <div class="password-list">
      {#each filteredPasswords as entry}
        <div class="entry-item" class:pwned={pwnedList.includes(entry.id ?? entry.Id)}>
          <div class="entry-info">
            <strong>{entry.website ?? entry.Website}</strong>
            {#if pwnedList.includes(entry.id ?? entry.Id)}
              <span style="color: #ef4444; font-size: 0.7rem;">⚠️ LEAK GEFUNDEN!</span>
            {/if}
          </div>
          
          <div class="action-buttons">
            <button class="btn-copy" onclick={() => copyToClipboard(entry.encryptedPassword ?? entry.EncryptedPassword)}>📋</button>
            <button class="btn-delete" onclick={() => deletePassword(entry.id ?? entry.Id)}>🗑️</button>
          </div>
        </div>
      {/each}
    </div>
  {/if}
    <div class="button-group">
        <button class="btn-export" onclick={exportPasswords}>Export CSV</button>
        <button class="btn-export" onclick={checkAllPasswords}>Leaks prüfen 🔍</button>
    </div>
    </div>

    </div>
  {/if}
</main>

<style>
.header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 20px;
}

.btn-logout {
  background-color: #475569; /* Ein dezentes Grau */
  color: white;
  padding: 8px 16px;
  font-size: 0.9rem;
}

.btn-logout:hover {
  background-color: #ef4444; /* Wird beim Drüberfahren rot */
}
.button-group {
  display: flex;
  gap: 10px; /* Abstand zwischen den Buttons */
  margin-top: 10px;
}
.pwned { 
  border: 2px 
  solid #ef4444; 
  background: #450a0a !important;
 }

.btn-export {
  background-color: #3b82f6; /* Ein schönes Blau */
  color: white;
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  font-weight: 600;
  transition: background-color 0.2s;
  margin-top: 10px;
}

.btn-export:hover {
  background-color: #2563eb; /* Dunkler beim Drüberfahren */
}

.strength-meter {
  height: 6px;
  width: 100%;
  background: #334155; /* Hintergrund des Balkens */
  border-radius: 3px;
  margin: 8px 0 4px 0;
  overflow: hidden;
}

.bar {
  height: 100%;
  transition: width 0.3s ease, background-color 0.3s ease;
}

  :global(body) {
    background-color: #0f172a;
    color: #f8fafc;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    margin: 0;
    padding: 20px;
  }
  .container {
    max-width: 1000px;
    margin: 0 auto;
    text-align: center;
  }
  h1 { color: #38bdf8; margin-bottom: 40px; }
  .card {
    background: #1e293b;
    padding: 25px;
    border-radius: 12px;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    border: 1px solid #334155;
  }
  .login-box { max-width: 400px; margin: 0 auto; }
  .dashboard {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 20px;
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
  button:hover { background: #0ea5e9; }
  .btn-sec { background: #64748b; color: white; }
  .btn-success { width: 100%; background: #10b981; color: white; margin-top: 15px; }
  .generator-group { display: flex; gap: 10px; align-items: center; }
  .generator-group input { flex-grow: 1; }
  .password-list { margin-top: 15px; }
  
  .entry-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: #334155;
    padding: 12px;
    border-radius: 8px;
    margin-bottom: 10px;
  }
  .entry-info { display: flex; flex-direction: column; }
  .entry-info span { font-size: 0.85rem; color: #94a3b8; }
  
  /* Styling für die Action Buttons */
  .action-buttons {
    display: flex;
    gap: 8px;
  }
  .action-buttons button {
    padding: 8px 12px;
    font-size: 1rem;
  }
  .btn-copy {
    background: #38bdf8;
    color: #0f172a;
  }
  .btn-delete {
    background: #ef4444;
    color: white;
  }
  .btn-delete:hover {
    background: #dc2626;
  }
  
</style>