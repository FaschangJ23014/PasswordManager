<script>
  import { onMount } from 'svelte';
  import Login from '../components/login.svelte';
  import PasswordForm from '../components/PasswordForm.svelte';
  import PasswordList from '../components/PasswordList.svelte';
  import SettingsModal from '../components/SettingsModal.svelte';

  let isAuthenticated = $state(false);
  let showSettings = $state(false);
  let passwords = $state([]);
  let isDarkMode = $state(true);

  //Auto-Logout Variablen
  let inactivityTimer;
  const INACTIVITY_LIMIT = 5 * 60 * 1000;

  let currentUsername = $state('');

  const API_URL = 'https://passwordmanager-k5za.onrender.com/api/passwords';

  onMount(() => {
    const token = localStorage.getItem('token');
    if (token) {
      isAuthenticated = true;
      loadPasswords();
    }

    //Auto-Logout Logik
    const events = ['mousemove', 'keydown', 'click', 'scroll', 'touchstart'];
    
    resetInactivityTimer();

    events.forEach(event => {
      window.addEventListener(event, resetInactivityTimer);
    });

    return () => {
      clearTimeout(inactivityTimer);
      events.forEach(event => {
        window.removeEventListener(event, resetInactivityTimer);
      });
    };
  });


  //Dark-LightMode Methode
  function toggleTheme() {
  isDarkMode = !isDarkMode;
  if (isDarkMode) {
    document.documentElement.classList.remove('light-mode');
  } else {
    document.documentElement.classList.add('light-mode');
  }
}

  //Auto-Logout Methode
  function resetInactivityTimer() {
    if (!isAuthenticated) return;

    clearTimeout(inactivityTimer);
    
    inactivityTimer = setTimeout(() => {
      alert('Du wurdest wegen Inaktivität automatisch abgemeldet.');
      logout(); 
    }, INACTIVITY_LIMIT);
  }


  function getHeaders() {
    const token = localStorage.getItem('token');
    return {
      'Content-Type': 'application/json',
      'Authorization': `Bearer ${token}`
    };
  }

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

  async function deletePassword(id) {
    if (!confirm('Willst du dieses Passwort wirklich für immer löschen?')) return;

    try {
      const response = await fetch(`${API_URL}/${id}`, {
        method: 'DELETE',
        headers: getHeaders()
      });

      if (response.ok) {
        loadPasswords();
      } else {
        const errorText = await response.text();
        alert('Fehler beim Löschen: ' + errorText);
      }
    } catch (error) {
      console.error('Fehler beim Löschen:', error);
    }
  }

  function exportPasswords() {
    let csvContent = "data:text/csv;charset=utf-8,Website,Username,Password\n";
    passwords.forEach(p => {
        csvContent += `${p.website ?? p.Website},${p.username ?? p.Username},${p.encryptedPassword ?? p.EncryptedPassword}\n`;
    });

    const encodedUri = encodeURI(csvContent);
    const link = document.createElement("a");
    link.setAttribute("href", encodedUri);
    link.setAttribute("download", "my_vault_export.csv");
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  async function isPasswordPwned(password) {
    const encoder = new TextEncoder();
    const data = encoder.encode(password);
    const hashBuffer = await crypto.subtle.digest('SHA-1', data);
    const hashArray = Array.from(new Uint8Array(hashBuffer));
    const hashHex = hashArray.map(b => b.toString(16).padStart(2, '0')).join('').toUpperCase();

    const prefix = hashHex.substring(0, 5);
    const suffix = hashHex.substring(5);

    const response = await fetch(`https://api.pwnedpasswords.com/range/${prefix}`);
    const text = await response.text();

    return text.includes(suffix);
  }

  function logout() {
    localStorage.removeItem('token');
    isAuthenticated = false;
    passwords = [];
  }
</script>

<main class="container">
  {#if isAuthenticated}
    <header class="app-header glass">
      <div class="logo-area">
        <div class="brand-icon">
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="22" height="22"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
        </div>
        <h1>ShieldVault</h1>
      </div>

      <div class="header-actions">
        <button class="btn-logout" onclick={logout}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M9 21H5a2 2 0 0 1-2-2V5a2 2 0 0 1 2-2h4"/><polyline points="16 17 21 12 16 7"/><line x1="21" y1="12" x2="9" y2="12"/></svg>
          Abmelden
        </button>

        <button class="btn-settings" onclick={() => showSettings = true}>
          <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.65 1.65 0 0 0 .33 1.82l.06.06a2 2 0 0 1 0 2.83 2 2 0 0 1-2.83 0l-.06-.06a1.65 1.65 0 0 0-1.82-.33 1.65 1.65 0 0 0-1 1.51V21a2 2 0 0 1-2 2 2 2 0 0 1-2-2v-.09A1.65 1.65 0 0 0 9 19.4a1.65 1.65 0 0 0-1.82.33l-.06.06a2 2 0 0 1-2.83 0 2 2 0 0 1 0-2.83l.06-.06a1.65 1.65 0 0 0 .33-1.82 1.65 1.65 0 0 0-1.51-1H3a2 2 0 0 1-2-2 2 2 0 0 1 2-2h.09A1.65 1.65 0 0 0 4.6 9a1.65 1.65 0 0 0-.33-1.82l-.06-.06a2 2 0 0 1 0-2.83 2 2 0 0 1 2.83 0l.06.06a1.65 1.65 0 0 0 1.82.33H9a1.65 1.65 0 0 0 1-1.51V3a2 2 0 0 1 2-2 2 2 0 0 1 2 2v.09a1.65 1.65 0 0 0 1 1.51 1.65 1.65 0 0 0 1.82-.33l.06-.06a2 2 0 0 1 2.83 0 2 2 0 0 1 0 2.83l-.06.06a1.65 1.65 0 0 0-.33 1.82V9a1.65 1.65 0 0 0 1.51 1H21a2 2 0 0 1 2 2 2 2 0 0 1-2 2h-.09a1.65 1.65 0 0 0-1.51 1z"/></svg>
          Settings
        </button>
      </div>
    </header>
  {/if}

  {#if !isAuthenticated}
    <Login onAuthSuccess={() => { isAuthenticated = true; loadPasswords(); }} />
  {:else}
    <div class="dashboard">
      <PasswordForm 
        {API_URL} 
        {getHeaders} 
        {isPasswordPwned} 
        onPasswordSaved={loadPasswords} 
      />

      <PasswordList 
        {passwords} 
        {API_URL}
        {getHeaders}
        {isPasswordPwned}
        onDelete={deletePassword} 
        onExport={exportPasswords} 
      />
    </div>
  {/if}


  {#if showSettings}
      <SettingsModal 
       {API_URL} 
       {getHeaders} 
       {isDarkMode}
       {toggleTheme}
       onClose={() => showSettings = false} 
       onUsernameUpdated={(newVal) => {  currentUsername = newVal; }}
         />
      {/if}
</main>

<footer class="app-footer">
    <p>ShieldVault &copy; 2026 &bull; Entwickelt von <strong>Jakob Faschang</strong></p>
    <p><a href="mailto:jfaschang@gmail.com">jfaschang@gmail.com</a></p>
  </footer>


<style>
  :global(body) {
    background-color: #0f172a;
    color: #f8fafc;
    font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
    margin: 0;
    padding: 20px;
    transition: background-color 0.3s ease, color 0.3s ease;
  }

  :global(html.light-mode) :global(body) {
    background-color: #f1f5f9 !important;
    color: #0f172a !important;
  }

  /* Überschrift im Light Mode sichtbar machen */
  :global(html.light-mode) h1 {
    background: none !important;
    color: #0f172a !important;
    -webkit-background-clip: initial !important;
    -webkit-text-fill-color: initial !important;
  }

  /* Weichere Ecken für die Hauptboxen im Light Mode */
  :global(html.light-mode) :global(.glass),
  :global(html.light-mode) :global(.app-header),
  :global(html.light-mode) :global(.password-form),
  :global(html.light-mode) :global(.password-list),
  :global(html.light-mode) :global(.dashboard > div),
  :global(html.light-mode) :global(form) {
    border-radius: 16px !important;
    border: 1px solid rgba(0, 0, 0, 0.08) !important; /* Dezenter, weicher Rand */
  }

  /* Auch die einzelnen Passwort-Einträge abrunden */
  :global(html.light-mode) :global(.password-item),
  :global(html.light-mode) :global(.vault-card) {
    border-radius: 12px !important;
    border: 1px solid rgba(0, 0, 0, 0.06) !important;
  }

  .container {
    max-width: 1000px;
    margin: 0 auto;
  }

  .app-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 15px 25px;
    border-radius: 16px;
    margin-bottom: 30px;
    flex-wrap: wrap;
    gap: 15px;
  }

  .glass {
    background: rgba(30, 41, 59, 0.75);
    backdrop-filter: blur(16px);
    -webkit-backdrop-filter: blur(16px);
    border: 1px solid rgba(255, 255, 255, 0.08);
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.3);
  }

  /* Light Mode für Header, Formulare, Tresor-Container und Komponenten */
  :global(html.light-mode) .glass,
  :global(html.light-mode) .app-header,
  :global(html.light-mode) :global(.password-form),
  :global(html.light-mode) :global(.password-list),
  :global(html.light-mode) :global(.dashboard > div),
  :global(html.light-mode) :global(form) {
    background: #ffffff !important;
    border: 1px solid #cbd5e1 !important;
    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.05) !important;
    color: #0f172a !important;
  }

  /* Allgemeine Texte, Labels und Überschriften in den Boxen */
  :global(html.light-mode) :global(h2),
  :global(html.light-mode) :global(h3),
  :global(html.light-mode) :global(h4),
  :global(html.light-mode) :global(span),
  :global(html.light-mode) :global(label),
  :global(html.light-mode) :global(p) {
    color: #0f172a !important;
  }

  /* Eingabefelder im Light Mode */
  :global(html.light-mode) :global(input) {
    background: #f8fafc !important;
    border: 1px solid #cbd5e1 !important;
    color: #0f172a !important;
  }

  :global(html.light-mode) :global(input::placeholder) {
    color: #94a3b8 !important;
  }

  /* Einzelne Passwort-Karten im Tresor für den Light Mode */
  :global(html.light-mode) :global(.password-item),
  :global(html.light-mode) :global(.vault-card) {
    background: #f8fafc !important;
    border: 1px solid #e2e8f0 !important;
    color: #0f172a !important;
  }


  /* Light Mode für das SettingsModal und Dialoge */
  :global(html.light-mode) :global(.settings-modal),
  :global(html.light-mode) :global(.modal),
  :global(html.light-mode) :global(.modal-content) {
    background: #ffffff !important;
    border: 1px solid #cbd5e1 !important;
    color: #0f172a !important;
    box-shadow: 0 20px 40px rgba(0, 0, 0, 0.1) !important;
  }

  /* Light Mode für Passwort-Einträge und Badges */
  :global(html.light-mode) :global(.password-item),
  :global(html.light-mode) :global(.vault-item),
  :global(html.light-mode) :global([class*="item"]),
  :global(html.light-mode) :global([class*="card"]) {
    background: #f8fafc !important;
    border: 1px solid #e2e8f0 !important;
    color: #0f172a !important;
  }

  /* Badge "2 Einträge" im Light Mode lesbar machen */
  :global(html.light-mode) :global(.badge),
  :global(html.light-mode) :global([class*="count"]),
  :global(html.light-mode) :global([class*="badge"]) {
    background: #e2e8f0 !important;
    color: #0f172a !important;
  }

  .logo-area {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  .brand-icon {
    background: rgba(56, 189, 248, 0.15);
    color: #38bdf8;
    padding: 10px;
    border-radius: 12px;
    display: flex;
    border: 1px solid rgba(56, 189, 248, 0.3);
  }

  h1 {
    margin: 0;
    font-size: 1.4rem;
    font-weight: 700;
    background: linear-gradient(135deg, #f8fafc 0%, #38bdf8 100%);
    -webkit-background-clip: text;
    -webkit-text-fill-color: transparent;
    letter-spacing: 0.5px;
  }

  .header-actions {
    display: flex;
    align-items: center;
    gap: 12px;
  }

  @media (max-width: 600px) {
    .header-actions {
      justify-content: flex-end;
    }
  }

  .btn-logout {
    background: rgba(239, 68, 68, 0.15);
    color: #fca5a5;
    border: 1px solid rgba(239, 68, 68, 0.3);
    padding: 10px 16px;
    font-size: 0.85rem;
    font-weight: 600;
    border-radius: 10px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.2s ease;
  }

  .btn-logout:hover {
    background: rgba(239, 68, 68, 0.25);
    color: white;
    border-color: rgba(239, 68, 68, 0.5);
  }

  .btn-settings {
    background: rgba(56, 189, 248, 0.15);
    color: #38bdf8;
    border: 1px solid rgba(56, 189, 248, 0.3);
    padding: 10px 16px;
    font-size: 0.85rem;
    font-weight: 600;
    border-radius: 10px;
    cursor: pointer;
    display: flex;
    align-items: center;
    gap: 8px;
    transition: all 0.2s ease;
  }

  .btn-settings:hover {
    background: rgba(56, 189, 248, 0.25);
    color: white;
    border-color: rgba(56, 189, 248, 0.5);
  }

  .dashboard {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 25px;
    align-items: start;
  }

  @media (max-width: 768px) {
    .dashboard {
      grid-template-columns: 1fr;
    }
  }

  /* Footer Style */
  .app-footer {
    text-align: center;
    padding: 20px 0 10px 0;
    color: #64748b;
    font-size: 0.85rem;
    border-top: 1px solid rgba(255, 255, 255, 0.05);
    margin-top: 20px;
  }

  .app-footer p {
    margin: 4px 0;
  }

  .app-footer strong {
    color: #94a3b8;
  }

  .app-footer a {
    color: #38bdf8;
    text-decoration: none;
    transition: color 0.2s;
  }

  .app-footer a:hover {
    text-decoration: underline;
  }
</style>