<script>
  import { onMount } from 'svelte';

  // Svelte 5 Runes für Reaktivität + Typisierung fixen
  let masterPassword = $state('');
  let isAuthenticated = $state(false);
  let searchQuery = $state('');
  /** @type {any[]} */
  let passwords = $state([]);
  
  // Für das neue Passwort-Formular
  let newWebsite = $state('');
  let newUsername = $state('');
  let generatedPassword = $state('');

  const API_URL = 'http://localhost:5000/api/passwords'; // Passe den Port an dein Backend an!

  // 1. Passwörter von der API laden (mit Suche & Master-Passwort Header)
  async function loadPasswords() {
    if (!masterPassword) return;
    
    try {
      const response = await fetch(`${API_URL}?search=${searchQuery}`, {
        method: 'GET',
        headers: {
          'X-Master-Password': masterPassword
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
      alert('Backend nicht erreichbar. Läuft dein dotnet run?');
    }
  }

  // 2. Neues Passwort generieren (Zufalls-Monster)
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
          'X-Master-Password': masterPassword
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

  // 4. In die Zwischenablage kopieren (Clipboard API)
  function copyToClipboard(text) {
    navigator.clipboard.writeText(text);
    alert('Passwort in die Zwischenablage kopiert! 📋');
  }
</script>

<main class="container">
  <h1>🛡️ ShieldVault Password Manager</h1>

  {#if !isAuthenticated}
    <div class="card login-box">
      <h3>Guten Tag, Jakob. Bitte verifizieren:</h3>
      <input 
        type="password" 
        placeholder="Dein Master-Passwort eingeben..." 
        bind:value={masterPassword} 
      />
      <button onclick={loadPasswords}>Tresor öffnen 🔓</button>
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
        
        <button class="btn-success" onclick={savePassword}>Ab in die SQLite-DB 💾</button>
      </div>

      <div class="card list-box">
        <h3>Deine Passwörter</h3>
        
        <input 
          type="text" 
          placeholder="🔍 Nach Website suchen..." 
          bind:value={searchQuery} 
          oninput={loadPasswords} 
        />

        {#if passwords.length === 0}
          <p class="empty-msg">Keine Einträge gefunden.</p>
        {:else}
          <div class="password-list">
            {#each passwords as entry}
              <div class="entry-item">
                <div class="entry-info">
                  <strong>{entry.website}</strong>
                  <span>{entry.username}</span>
                </div>
                <button onclick={() => copyToClipboard(entry.encryptedPassword)}>
                  Kopieren 📋
                </button>
              </div>
            {/each}
          </div>
        {/if}
      </div>

    </div>
  {/if}
</main>

<style>
  /* Ein bisschen cleanes, modernes Styling */
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
    width: 100%;
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
  .btn-sec { background: #64748b; color: white; width: auto; }
  .btn-success { background: #10b981; color: white; margin-top: 15px; }
  .generator-group { display: flex; gap: 10px; align-items: center; }
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
  .entry-item button { width: auto; padding: 6px 12px; font-size: 0.9rem; }
</style>