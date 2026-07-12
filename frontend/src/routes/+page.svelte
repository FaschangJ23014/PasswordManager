<script>
  import { onMount } from 'svelte';

  // Svelte 5 Runes für Reaktivität
  let masterPassword = $state('');
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

  //Für die Automatische Berechnung: Reaktiv
  let strength = $derived(calculateStrength(generatedPassword));

  const API_URL = 'https://passwordmanager-k5za.onrender.com/api/passwords';

  // 1. Passwörter von der API laden
  // 1. Passwörter von der API laden (KORRIGIERT)
  async function loadPasswords() {
    if (!masterPassword) return;
    
    try {
      const response = await fetch(`${API_URL}?search=${searchQuery}`, {
        method: 'GET',
        headers: {
          // encodeURIComponent macht das Passwort Header-sicher!
          'X-Master-Password': encodeURIComponent(masterPassword)
        }
      });

      if (response.ok) {
        passwords = await response.json();
        isAuthenticated = true;
      } else if (response.status === 401) {
        alert('Falsches Master-Passwort! Zugriff verweigert.');
        isAuthenticated = false;
      }
    } catch (error) {
      console.error('Fehler beim Laden:', error);
    }
    console.log("Meine Passwörter:", passwords);
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
        headers: {
          'Content-Type': 'application/json',
          // Auch hier absichern:
          'X-Master-Password': encodeURIComponent(masterPassword)
        },
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
        // Achtung: Da dein Backend beim 'GET' entschlüsselt, ist 'entry.EncryptedPassword' 
        // hier das Klartext-Passwort!
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
        headers: {
          'X-Master-Password': masterPassword
        }
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
  <h1>🛡️ ShieldVault</h1>

  {#if !isAuthenticated}
    <div class="card login-box">
      <h3>Guten Tag, Jakob. Bitte verifizieren:</h3>
      <input 
        type="password" 
        placeholder="Dein Master-Passwort eingeben..." 
        bind:value={masterPassword} 
      />
      <button onclick={loadPasswords}>Tresor öffnen</button>
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
  <input type="text" placeholder="🔍 Nach Website suchen..." bind:value={searchQuery} oninput={handleSearch} />
  <button onclick={checkAllPasswords}>Alle auf Leaks prüfen 🔍</button>

  {#if passwords.length === 0}
    <p class="empty-msg">Keine Einträge gefunden.</p>
  {:else}
    <div class="password-list">
      {#each passwords as entry}
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