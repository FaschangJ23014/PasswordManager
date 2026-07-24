<script>
  let { onAuthSuccess } = $props();

  let username = $state('');
  let password = $state('');
  let isRegistering = $state(false);

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
        isRegistering = false;
      } else {
        const data = await response.json();
        localStorage.setItem('token', data.token);
        onAuthSuccess(); // Sagt der Hauptkomponente, dass der Login erfolgreich war
      }
    } else {
      const err = await response.text();
      alert(`${isRegistering ? 'Registrierung' : 'Login'} fehlgeschlagen: ${err}`);
    }
  }
</script>

<div class="card login-box">
  <h3>{isRegistering ? 'Account erstellen' : 'Login'}</h3>
  
  <input type="text" placeholder="Username" bind:value={username} />
  <input type="password" placeholder="Passwort" bind:value={password} />
  
  <button onclick={handleAuth}>
    {isRegistering ? 'Registrieren' : 'Einloggen'}
  </button>

  <p class="toggle-text" onclick={() => isRegistering = !isRegistering}>
    {isRegistering ? 'Schon registriert? Hier einloggen.' : 'Noch keinen Account? Jetzt registrieren.'}
  </p>
</div>

<style>
  .card {
    background: #1e293b;
    padding: 25px;
    border-radius: 12px;
    box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    border: 1px solid #334155;
  }
  .login-box { max-width: 400px; margin: 0 auto; }
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
  .toggle-text {
    margin-top: 15px;
    font-size: 0.85rem;
    cursor: pointer;
    color: #38bdf8;
  }
</style>