<script>
  let { API_URL, getHeaders, onClose, onUsernameUpdated } = $props();

  let newUsername = $state('');
  let oldPassword = $state('');
  let newPassword = $state('');
  
  let usernameMessage = $state({ text: '', isError: false });
  let passwordMessage = $state({ text: '', isError: false });
  let isLoading = $state(false);

  // Benutzernamen aktualisieren
  async function handleUpdateUsername(e) {
    e.preventDefault();
    if (!newUsername.trim()) return;

    isLoading = true;
    usernameMessage = { text: '', isError: false };

    try {
      const response = await fetch(`${API_URL.replace('/passwords', '/users/username')}`, {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify({ username: newUsername })
      });

      const data = await response.json();

      if (response.ok) {
        usernameMessage = { text: data.message || 'Benutzername aktualisiert!', isError: false };
        onUsernameUpdated(newUsername);
        newUsername = '';
      } else {
        usernameMessage = { text: data.message || 'Fehler beim Aktualisieren.', isError: true };
      }
    } catch (err) {
      usernameMessage = { text: 'Netzwerkfehler.', isError: true };
    } finally {
      isLoading = false;
    }
  }

  // Passwort aktualisieren
  async function handleUpdatePassword(e) {
    e.preventDefault();
    if (!oldPassword || !newPassword) return;

    isLoading = true;
    passwordMessage = { text: '', isError: false };

    try {
      const response = await fetch(`${API_URL.replace('/passwords', '/users/password')}`, {
        method: 'PUT',
        headers: getHeaders(),
        body: JSON.stringify({ oldPassword, newPassword })
      });

      const data = await response.json();

      if (response.ok) {
        passwordMessage = { text: data.message || 'Passwort erfolgreich geändert!', isError: false };
        oldPassword = '';
        newPassword = '';
      } else {
        passwordMessage = { text: typeof data === 'string' ? data : (data.message || 'Fehler beim Ändern.'), isError: true };
      }
    } catch (err) {
      passwordMessage = { text: 'Netzwerkfehler.', isError: true };
    } finally {
      isLoading = false;
    }
  }
</script>

<!-- Backdrop Overlay -->
<div class="modal-backdrop" onclick={onClose}>
  <div class="modal-content glass" onclick={(e) => e.stopPropagation()}>
    
    <div class="modal-header">
      <h3>Einstellungen</h3>
      <button class="btn-close" onclick={onClose}>
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/></svg>
      </button>
    </div>

    <div class="modal-body">
      <!-- Sektion: Benutzername ändern -->
      <section class="settings-section">
        <h4>Benutzernamen ändern</h4>
        <form onsubmit={handleUpdateUsername}>
          <div class="input-group">
            <input type="text" placeholder="Neuer Benutzername" bind:value={newUsername} required />
            <button type="submit" class="btn-primary" disabled={isLoading}>Speichern</button>
          </div>
          {#if usernameMessage.text}
            <p class="feedback" class:error={usernameMessage.isError}>{usernameMessage.text}</p>
          {/if}
        </form>
      </section>

      <hr class="divider" />

      <!-- Sektion: Passwort ändern -->
      <section class="settings-section">
        <h4>Passwort zurücksetzen</h4>
        <form onsubmit={handleUpdatePassword}>
          <div class="input-stack">
            <input type="password" placeholder="Altes Passwort" bind:value={oldPassword} required />
            <input type="password" placeholder="Neues Passwort" bind:value={newPassword} required />
            <button type="submit" class="btn-primary" disabled={isLoading}>Passwort ändern</button>
          </div>
          {#if passwordMessage.text}
            <p class="feedback" class:error={passwordMessage.isError}>{passwordMessage.text}</p>
          {/if}
        </form>
      </section>
    </div>

  </div>
</div>

<style>
  .modal-backdrop {
    position: fixed;
    top: 0;
    left: 0;
    width: 100vw;
    height: 100vh;
    background: rgba(15, 23, 42, 0.8);
    backdrop-filter: blur(8px);
    -webkit-backdrop-filter: blur(8px);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
    padding: 20px;
    box-sizing: border-box;
  }

  .modal-content {
    width: 100%;
    max-width: 450px;
    padding: 25px;
    border-radius: 20px;
    box-shadow: 0 25px 50px rgba(0, 0, 0, 0.5);
    animation: fadeIn 0.2s ease-out;
  }

  .glass {
    background: rgba(30, 41, 59, 0.9);
    border: 1px solid rgba(255, 255, 255, 0.08);
  }

  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
  }

  .modal-header h3 {
    margin: 0;
    color: #f8fafc;
    font-size: 1.2rem;
  }

  .btn-close {
    background: transparent;
    border: none;
    color: #94a3b8;
    cursor: pointer;
    padding: 4px;
    display: flex;
    border-radius: 6px;
    transition: background 0.2s, color 0.2s;
  }

  .btn-close:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }

  .settings-section h4 {
    margin: 0 0 10px 0;
    font-size: 0.95rem;
    color: #cbd5e1;
  }

  .input-group {
    display: flex;
    gap: 10px;
  }

  .input-stack {
    display: flex;
    flex-direction: column;
    gap: 10px;
  }

  input {
    width: 100%;
    padding: 10px 14px;
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    color: white;
    font-size: 0.9rem;
    box-sizing: border-box;
  }

  input:focus {
    outline: none;
    border-color: #38bdf8;
    box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.2);
  }

  .btn-primary {
    background: #38bdf8;
    color: #0f172a;
    border: none;
    padding: 10px 16px;
    border-radius: 10px;
    font-weight: 600;
    font-size: 0.85rem;
    cursor: pointer;
    transition: background 0.2s;
    white-space: nowrap;
  }

  .btn-primary:hover:not(:disabled) {
    background: #0ea5e9;
  }

  .btn-primary:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }

  .divider {
    border: none;
    border-top: 1px solid rgba(255, 255, 255, 0.08);
    margin: 20px 0;
  }

  .feedback {
    font-size: 0.8rem;
    margin: 6px 0 0 0;
    color: #6ee7b7;
  }

  .feedback.error {
    color: #fca5a5;
  }

  @keyframes fadeIn {
    from { opacity: 0; transform: scale(0.95); }
    to { opacity: 1; transform: scale(1); }
  }
</style>