<script>
  let { passwords, onDelete, onExport, onCheckLeaks, getHeaders, API_URL, isPasswordPwned } = $props();

  let searchQuery = $state('');
  let pwnedList = $state([]);

  let filteredPasswords = $derived.by(() => {
    if (!passwords || passwords.length === 0) return [];
    const query = searchQuery.toLowerCase();
    return passwords.filter(p => {
      const website = (p.website ?? p.Website ?? "").toLowerCase();
      return website.includes(query);
    });
  });

  function copyToClipboard(text) {
    navigator.clipboard.writeText(text);
    alert('Passwort in die Zwischenablage kopiert! 📋');
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
</script>

<div class="card list-box">
  <h3>Deine Passwörter</h3>
  <input type="text" placeholder="🔍 Nach Website suchen..." bind:value={searchQuery} />

  {#if passwords.length === 0}
    <p class="empty-msg">Keine Einträge gefunden.</p>
  {:else}
    <div class="password-list">
      {#each filteredPasswords as entry}
        {@const entryId = entry.id ?? entry.Id}
        <div class="entry-item" class:pwned={pwnedList.includes(entryId)}>
          <div class="entry-info">
            <strong>{entry.website ?? entry.Website}</strong>
            {#if pwnedList.includes(entryId)}
              <span style="color: #ef4444; font-size: 0.7rem;">⚠️ LEAK GEFUNDEN!</span>
            {/if}
          </div>
          
          <div class="action-buttons">
            <button class="btn-copy" onclick={() => copyToClipboard(entry.encryptedPassword ?? entry.EncryptedPassword)}>📋</button>
            <button class="btn-delete" onclick={() => onDelete(entryId)}>🗑️</button>
          </div>
        </div>
      {/each}
    </div>
  {/if}

  <div class="button-group">
      <button class="btn-export" onclick={onExport}>Export CSV</button>
      <button class="btn-export" onclick={checkAllPasswords}>Leaks prüfen 🔍</button>
  </div>
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
  .button-group {
    display: flex;
    gap: 10px;
    margin-top: 10px;
  }
  .pwned { 
    border: 2px solid #ef4444; 
    background: #450a0a !important;
  }
  .btn-export {
    background-color: #3b82f6;
    color: white;
    padding: 8px 16px;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-weight: 600;
    width: 100%;
  }
  .btn-export:hover { background-color: #2563eb; }
  .password-list { margin-top: 15px; max-height: 350px; overflow-y: auto; }
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
  .action-buttons { display: flex; gap: 8px; }
  .action-buttons button {
    padding: 8px 12px;
    font-size: 1rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
  }
  .btn-copy { background: #38bdf8; color: #0f172a; }
  .btn-delete { background: #ef4444; color: white; }
  .btn-delete:hover { background: #dc2626; }
</style>