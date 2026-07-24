<script>
  let { passwords, onDelete, onExport, getHeaders, API_URL, isPasswordPwned } = $props();

  let searchQuery = $state('');
  let pwnedList = $state([]);
  let isChecking = $state(false);
  let statusMessage = $state('');
  let copiedId = $state(null);

  let filteredPasswords = $derived.by(() => {
    if (!passwords || passwords.length === 0) return [];
    const query = searchQuery.toLowerCase();
    return passwords.filter(p => {
      const website = (p.website ?? p.Website ?? "").toLowerCase();
      const username = (p.username ?? p.Username ?? "").toLowerCase();
      return website.includes(query) || username.includes(query);
    });
  });

  function copyToClipboard(text, id) {
    navigator.clipboard.writeText(text);
    copiedId = id;
    setTimeout(() => {
      if (copiedId === id) copiedId = null;
    }, 2000);
  }

  async function checkAllPasswords() {
    isChecking = true;
    pwnedList = []; 
    statusMessage = '';

    try {
      for (const p of passwords) {
          const pwd = p.EncryptedPassword || p.encryptedPassword;
          const entryId = p.Id || p.id;
          if (await isPasswordPwned(pwd)) {
              pwnedList.push(entryId);
          }
      }
      if (pwnedList.length > 0) {
        statusMessage = `Achtung: ${pwnedList.length} Passwörter wurden in Leaks gefunden!`;
      } else {
        statusMessage = 'Tresor sicher! Keine Leaks gefunden.';
      }
    } catch (err) {
      statusMessage = 'Fehler beim Prüfen der Leaks.';
    } finally {
      isChecking = false;
    }
  }
</script>

<div class="card glass list-box">
  <div class="list-header">
    <div class="header-title">
      <div class="icon-box">
        <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="20" height="20"><rect x="3" y="11" width="18" height="11" rx="2" ry="2"/><path d="M7 11V7a5 5 0 0 1 10 0v4"/></svg>
      </div>
      <h3>Dein Tresor</h3>
    </div>
    <span class="badge">{passwords.length} Einträge</span>
  </div>

  <div class="search-wrapper">
    <svg class="search-icon" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="18" height="18"><circle cx="11" cy="11" r="8"/><line x1="21" y1="21" x2="16.65" y2="16.65"/></svg>
    <input type="text" placeholder="Nach Website oder Benutzer suchen..." bind:value={searchQuery} />
  </div>

  {#if statusMessage}
    <div class="status-banner" class:error={pwnedList.length > 0} class:success={pwnedList.length === 0}>
      <span>{statusMessage}</span>
    </div>
  {/if}

  {#if passwords.length === 0}
    <div class="empty-state">
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5" width="40" height="40"><path d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"/></svg>
      <p>Keine Passwörter im Tresor gespeichert.</p>
    </div>
  {:else if filteredPasswords.length === 0}
    <div class="empty-state">
      <p>Keine Treffer für "{searchQuery}" gefunden.</p>
    </div>
  {:else}
    <div class="password-list">
      {#each filteredPasswords as entry}
        {@const entryId = entry.id ?? entry.Id}
        {@const isPwned = pwnedList.includes(entryId)}
        <div class="entry-item" class:pwned={isPwned}>
          <div class="entry-info">
            <span class="website-name">{entry.website ?? entry.Website}</span>
            <span class="username-text">{entry.username ?? entry.Username}</span>
            {#if isPwned}
              <span class="leak-badge">
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="12" height="12"><path d="M10.29 3.86L1.82 18a2 2 0 0 0 1.71 3h16.94a2 2 0 0 0 1.71-3L13.71 3.86a2 2 0 0 0-3.42 0z"/><line x1="12" y1="9" x2="12" y2="13"/><line x1="12" y1="17" x2="12.01" y2="17"/></svg>
                Leak gefunden!
              </span>
            {/if}
          </div>
          
          <div class="action-buttons">
            <button class="btn-action btn-copy" onclick={() => copyToClipboard(entry.encryptedPassword ?? entry.EncryptedPassword, entryId)} title="Passwort kopieren">
              {#if copiedId === entryId}
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><polyline points="20 6 9 17 4 12"/></svg>
              {:else}
                <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><rect x="9" y="9" width="13" height="13" rx="2" ry="2"/><path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1"/></svg>
              {/if}
            </button>
            <button class="btn-action btn-delete" onclick={() => onDelete(entryId)} title="Löschen">
              <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><polyline points="3 6 5 6 21 6"/><path d="M19 6v14a2 2 0 0 1-2 2H7a2 2 0 0 1-2-2V6m3 0V4a2 2 0 0 1 2-2h4a2 2 0 0 1 2 2v2"/></svg>
            </button>
          </div>
        </div>
      {/each}
    </div>
  {/if}

  <div class="button-group">
    <button class="btn-sec" onclick={onExport}>
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M21 15v4a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2v-4"/><polyline points="7 10 12 15 17 10"/><line x1="12" y1="15" x2="12" y2="3"/></svg>
      CSV Exportieren
    </button>
    <button class="btn-sec btn-check" onclick={checkAllPasswords} disabled={isChecking}>
      <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" width="16" height="16"><path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/></svg>
      {isChecking ? 'Prüfe Leaks...' : 'Leaks prüfen'}
    </button>
  </div>
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
    display: flex;
    flex-direction: column;
  }

  .list-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 20px;
  }

  .header-title {
    display: flex;
    align-items: center;
    gap: 12px;
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

  .badge {
    background: #334155;
    color: #cbd5e1;
    font-size: 0.75rem;
    padding: 4px 10px;
    border-radius: 20px;
    font-weight: 600;
  }

  .search-wrapper {
    position: relative;
    margin-bottom: 15px;
  }

  .search-icon {
    position: absolute;
    left: 14px;
    top: 50%;
    transform: translateY(-50%);
    color: #64748b;
  }

  .search-wrapper input {
    width: 100%;
    padding: 12px 14px 12px 42px;
    background: #0f172a;
    border: 1px solid #334155;
    border-radius: 10px;
    color: white;
    font-size: 0.9rem;
    box-sizing: border-box;
    transition: all 0.2s ease;
  }

  .search-wrapper input:focus {
    outline: none;
    border-color: #38bdf8;
    box-shadow: 0 0 0 3px rgba(56, 189, 248, 0.2);
  }

  .status-banner {
    padding: 10px 14px;
    border-radius: 8px;
    font-size: 0.85rem;
    margin-bottom: 15px;
    font-weight: 500;
  }

  .status-banner.error {
    background: rgba(239, 68, 68, 0.15);
    border: 1px solid rgba(239, 68, 68, 0.4);
    color: #fca5a5;
  }

  .status-banner.success {
    background: rgba(16, 185, 129, 0.15);
    border: 1px solid rgba(16, 185, 129, 0.4);
    color: #6ee7b7;
  }

  .empty-state {
    text-align: center;
    padding: 40px 0;
    color: #64748b;
    font-size: 0.9rem;
    display: flex;
    flex-direction: column;
    align-items: center;
    gap: 10px;
  }

  .password-list {
    margin-top: 5px;
    max-height: 380px;
    overflow-y: auto;
    display: flex;
    flex-direction: column;
    gap: 10px;
    padding-right: 4px;
  }

  .password-list::-webkit-scrollbar {
    width: 6px;
  }

  .password-list::-webkit-scrollbar-track {
    background: rgba(255, 255, 255, 0.02);
    border-radius: 4px;
  }

  .password-list::-webkit-scrollbar-thumb {
    background: rgba(56, 189, 248, 0.25);
    border-radius: 4px;
  }

  .password-list::-webkit-scrollbar-thumb:hover {
    background: rgba(56, 189, 248, 0.5);
  }

  .entry-item {
    display: flex;
    justify-content: space-between;
    align-items: center;
    background: rgba(51, 65, 85, 0.5);
    border: 1px solid rgba(255, 255, 255, 0.05);
    padding: 12px 16px;
    border-radius: 12px;
    transition: background 0.2s;
  }

  .entry-item:hover {
    background: rgba(51, 65, 85, 0.8);
  }

  .entry-item.pwned {
    border: 1px solid rgba(239, 68, 68, 0.5);
    background: rgba(69, 10, 10, 0.4);
  }

  .entry-info {
    display: flex;
    flex-direction: column;
    gap: 2px;
  }

  .website-name {
    font-weight: 600;
    color: #f8fafc;
    font-size: 0.95rem;
  }

  .username-text {
    color: #94a3b8;
    font-size: 0.8rem;
  }

  .leak-badge {
    display: flex;
    align-items: center;
    gap: 4px;
    color: #fca5a5;
    font-size: 0.7rem;
    font-weight: 600;
    margin-top: 4px;
    background: rgba(239, 68, 68, 0.15);
    padding: 2px 6px;
    border-radius: 4px;
    width: fit-content;
  }

  .action-buttons {
    display: flex;
    gap: 8px;
  }

  .btn-action {
    padding: 8px;
    border: none;
    border-radius: 8px;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: background 0.2s, transform 0.1s;
  }

  .btn-action:active {
    transform: scale(0.95);
  }

  .btn-copy {
    background: rgba(56, 189, 248, 0.15);
    color: #38bdf8;
    border: 1px solid rgba(56, 189, 248, 0.3);
  }

  .btn-copy:hover {
    background: rgba(56, 189, 248, 0.25);
  }

  .btn-delete {
    background: rgba(239, 68, 68, 0.15);
    color: #fca5a5;
    border: 1px solid rgba(239, 68, 68, 0.3);
  }

  .btn-delete:hover {
    background: rgba(239, 68, 68, 0.25);
  }

  .button-group {
    display: flex;
    gap: 10px;
    margin-top: 20px;
  }

  .btn-sec {
    flex: 1;
    background: #334155;
    color: #f8fafc;
    border: 1px solid #475569;
    padding: 12px;
    border-radius: 10px;
    font-weight: 600;
    font-size: 0.85rem;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 8px;
    transition: background 0.2s;
  }

  .btn-sec:hover:not(:disabled) {
    background: #475569;
  }

  .btn-sec:disabled {
    opacity: 0.6;
    cursor: not-allowed;
  }
</style>