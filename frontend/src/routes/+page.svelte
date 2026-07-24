<script>
  import { onMount } from 'svelte';
  import Login from '../components/login.svelte';
  import PasswordForm from '../components/PasswordForm.svelte';
  import PasswordList from '../components/PasswordList.svelte';

  let isAuthenticated = $state(false);
  let passwords = $state([]);

  const API_URL = 'https://passwordmanager-k5za.onrender.com/api/passwords';

  onMount(() => {
    const token = localStorage.getItem('token');
    if (token) {
      isAuthenticated = true;
      loadPasswords();
    }
  });

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
  <div class="header">
    <h1>🛡️ ShieldVault</h1>
    {#if isAuthenticated}
      <button class="btn-logout" onclick={logout}>Logout</button>
    {/if}
  </div>

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
</main>

<style>
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
  .header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
  }
  .btn-logout {
    background-color: #475569;
    color: white;
    padding: 8px 16px;
    font-size: 0.9rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
  }
  .btn-logout:hover { background-color: #ef4444; }
  .dashboard {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 20px;
  }
</style>