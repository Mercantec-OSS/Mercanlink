<script lang="ts">
  import { onMount } from 'svelte';
  import { auth } from '$lib/auth';
  import { get } from 'svelte/store';

  let rawData = '';
  let parsedUsers: any[] = [];
  let validationResult: any = null;
  let importResult: any = null;
  let loading = false;
  let importing = false;
  let step = 1; // 1: Paste, 2: Preview, 3: Settings, 4: Results
  
  // Import indstillinger
  let importSettings = {
    defaultRoles: ['Student'],
    defaultDepartment: 'Datamatiker',
    updateExisting: false,
    sendWelcomeEmails: false,
    skipDuplicates: true
  };

  let availableRoles = ['Student', 'Teacher', 'Admin', 'Developer', 'AdvisoryBoard'];
  let detectedColumns: string[] = [];
  let columnMapping: any = {};

  // Prøv at læse token fra localStorage ved load
  onMount(() => {
    const saved = localStorage.getItem('auth');
    if (saved) {
      auth.set(JSON.parse(saved));
    }
  });

  // Parse data fra textarea
  function parseRawData() {
    if (!rawData.trim()) {
      alert('Indtast venligst data der skal importeres');
      return;
    }

    loading = true;
    try {
      // Split i linjer og fjern tomme linjer
      const lines = rawData.trim().split('\n').filter(line => line.trim());
      
      if (lines.length === 0) {
        alert('Ingen gyldige linjer fundet');
        loading = false;
        return;
      }

      // Antag første linje er header hvis den indeholder almindelige ord
      const hasHeader = lines[0].toLowerCase().includes('username') || 
                       lines[0].toLowerCase().includes('name') ||
                       lines[0].toLowerCase().includes('email');
      
      let headerLine = '';
      let dataLines = lines;
      
      if (hasHeader) {
        headerLine = lines[0];
        dataLines = lines.slice(1);
        // Detekter kolonner fra header
        detectedColumns = headerLine.split(/\s+/).filter(col => col.trim());
      } else {
        // Brug første data linje til at estimere antallet af kolonner
        const firstDataCols = lines[0].split(/\s+/).length;
        detectedColumns = Array.from({length: firstDataCols}, (_, i) => `Column${i + 1}`);
      }

      // Parse hver linje af data
      parsedUsers = dataLines.map((line, index) => {
        const columns = line.split(/\s+/);
        return parseUserFromColumns(columns, index);
      }).filter(user => user.username); // Filtrer linjer uden brugernavn

      // Auto-generer kolonne mapping
      generateColumnMapping();

      console.log('Parsed users:', parsedUsers);
      console.log('Detected columns:', detectedColumns);
      
      step = 2;
    } catch (error) {
      console.error('Parse fejl:', error);
      alert('Fejl ved parsing af data: ' + error.message);
    }
    loading = false;
  }

  // Parse bruger fra kolonner baseret på position og indhold
  function parseUserFromColumns(columns: string[], lineIndex: number) {
    const user: any = {
      username: '',
      initialPassword: '',
      givenName: '',
      surname: '',
      email: '',
      studentId: '',
      department: '',
      employeeType: 'Student'
    };

    // Smart gætteri baseret på kolonne indhold og position
    for (let i = 0; i < columns.length; i++) {
      const col = columns[i];
      
      // Username - typisk korte alphanumeriske strenge
      if (!user.username && /^[a-zA-Z0-9]{3,12}$/.test(col) && !col.includes('@')) {
        user.username = col;
        columnMapping[i] = 'username';
      }
      // Password - indeholder typisk specielle tegn og bogstaver/tal
      else if (!user.initialPassword && /^[A-Za-z0-9!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]{6,}$/.test(col) && col.includes('!') || col.includes('#') || /[A-Z].*[0-9]/.test(col)) {
        user.initialPassword = col;
        columnMapping[i] = 'initialPassword';
      }
      // Email - indeholder @
      else if (!user.email && col.includes('@')) {
        user.email = col;
        columnMapping[i] = 'email';
      }
      // Student ID - numerisk
      else if (!user.studentId && /^\d{6,10}$/.test(col)) {
        user.studentId = col;
        columnMapping[i] = 'studentId';
      }
      // Given name - typisk navne med mellemrum eller lange strenge med bogstaver
      else if (!user.givenName && /^[A-Za-zÀ-ÿ\s]{2,}$/.test(col) && col.length > 3) {
        user.givenName = col;
        columnMapping[i] = 'givenName';
      }
      // Surname - efter given name, typisk kortere navne
      else if (user.givenName && !user.surname && /^[A-Za-zÀ-ÿ]{2,}$/.test(col)) {
        user.surname = col;
        columnMapping[i] = 'surname';
      }
    }

    // Hvis vi ikke kunne gætte, brug positions-baseret mapping
    if (!user.username && columns.length > 0) user.username = columns[0];
    if (!user.initialPassword && columns.length > 1) user.initialPassword = columns[1];
    if (!user.givenName && columns.length > 2) user.givenName = columns[2];
    if (!user.surname && columns.length > 3) user.surname = columns[3];

    return user;
  }

  // Generer kolonne mapping baseret på detekterede mønstre
  function generateColumnMapping() {
    // Reset mapping
    columnMapping = {};
    
    // Analysér første par linjer for at forbedre mapping
    const sampleSize = Math.min(3, parsedUsers.length);
    for (let i = 0; i < detectedColumns.length; i++) {
      const columnName = detectedColumns[i].toLowerCase();
      
      // Mapping baseret på kolonne navn
      if (columnName.includes('username') || columnName.includes('user')) {
        columnMapping[i] = 'username';
      } else if (columnName.includes('password') || columnName.includes('pass')) {
        columnMapping[i] = 'initialPassword';
      } else if (columnName.includes('email') || columnName.includes('mail')) {
        columnMapping[i] = 'email';
      } else if (columnName.includes('given') || columnName.includes('first') || columnName.includes('fullname')) {
        columnMapping[i] = 'givenName';
      } else if (columnName.includes('surname') || columnName.includes('last')) {
        columnMapping[i] = 'surname';
      } else if (columnName.includes('id') && !columnName.includes('email')) {
        columnMapping[i] = 'studentId';
      }
    }
  }

  // Validér data inden import
  async function validateData() {
    if (parsedUsers.length === 0) {
      alert('Ingen brugere at validere');
      return;
    }

    loading = true;
    try {
      const { accessToken } = get(auth);
      const response = await fetch('http://localhost:5053/api/User/validate-bulk-import', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          users: parsedUsers.map(user => ({
            username: user.username,
            initialPassword: user.initialPassword,
            givenName: user.givenName,
            surname: user.surname,
            email: user.email || null,
            studentId: user.studentId || null,
            department: user.department || importSettings.defaultDepartment,
            employeeType: user.employeeType || 'Student'
          })),
          defaultRoles: importSettings.defaultRoles,
          defaultDepartment: importSettings.defaultDepartment,
          updateExisting: importSettings.updateExisting
        })
      });

      if (response.ok) {
        validationResult = await response.json();
        step = 3;
      } else {
        const error = await response.json();
        alert('Validering fejlede: ' + (error.message || 'Ukendt fejl'));
      }
    } catch (error) {
      console.error('Validering fejl:', error);
      alert('Netværksfejl under validering');
    }
    loading = false;
  }

  // Importér brugere
  async function importUsers() {
    importing = true;
    try {
      const { accessToken } = get(auth);
      const response = await fetch('http://localhost:5053/api/User/bulk-import', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          users: parsedUsers.map(user => ({
            username: user.username,
            initialPassword: user.initialPassword,
            givenName: user.givenName,
            surname: user.surname,
            email: user.email || null,
            studentId: user.studentId || null,
            department: user.department || importSettings.defaultDepartment,
            employeeType: user.employeeType || 'Student'
          })),
          defaultRoles: importSettings.defaultRoles,
          defaultDepartment: importSettings.defaultDepartment,
          updateExisting: importSettings.updateExisting,
          sendWelcomeEmails: importSettings.sendWelcomeEmails
        })
      });

      if (response.ok) {
        importResult = await response.json();
        step = 4;
      } else {
        const error = await response.json();
        alert('Import fejlede: ' + (error.message || 'Ukendt fejl'));
      }
    } catch (error) {
      console.error('Import fejl:', error);
      alert('Netværksfejl under import');
    }
    importing = false;
  }

  // Reset til start
  function resetImport() {
    rawData = '';
    parsedUsers = [];
    validationResult = null;
    importResult = null;
    step = 1;
    columnMapping = {};
    detectedColumns = [];
  }

  // Tilføj/fjern rolle fra standard roller
  function toggleRole(role: string) {
    if (importSettings.defaultRoles.includes(role)) {
      importSettings.defaultRoles = importSettings.defaultRoles.filter(r => r !== role);
    } else {
      importSettings.defaultRoles = [...importSettings.defaultRoles, role];
    }
  }

  // Fjern bruger fra listen
  function removeUser(index: number) {
    parsedUsers = parsedUsers.filter((_, i) => i !== index);
  }

  // Rediger bruger
  function editUser(index: number, field: string, value: string) {
    parsedUsers[index][field] = value;
    parsedUsers = [...parsedUsers]; // Trigger reactivity
  }
</script>

<style>
  .import-container {
    max-width: 1200px;
    margin: 2rem auto;
    padding: 2rem;
    background: white;
    border-radius: 1rem;
    box-shadow: 0 4px 20px rgba(0,0,0,0.08);
  }

  .step-indicator {
    display: flex;
    justify-content: center;
    margin-bottom: 2rem;
    gap: 1rem;
  }

  .step {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    border-radius: 2rem;
    font-weight: 500;
    transition: all 0.2s ease;
  }

  .step.active {
    background: #3b82f6;
    color: white;
  }

  .step.completed {
    background: #10b981;
    color: white;
  }

  .step.pending {
    background: #f3f4f6;
    color: #6b7280;
  }

  .data-textarea {
    width: 100%;
    min-height: 300px;
    padding: 1rem;
    border: 2px solid #e5e7eb;
    border-radius: 0.5rem;
    font-family: 'Courier New', monospace;
    font-size: 0.875rem;
    resize: vertical;
  }

  .data-textarea:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .preview-table {
    width: 100%;
    border-collapse: collapse;
    margin-top: 1rem;
    border: 1px solid #e5e7eb;
    border-radius: 0.5rem;
    overflow: hidden;
  }

  .preview-table th {
    background: #f8fafc;
    padding: 0.75rem;
    text-align: left;
    font-weight: 600;
    border-bottom: 1px solid #e5e7eb;
  }

  .preview-table td {
    padding: 0.75rem;
    border-bottom: 1px solid #f1f5f9;
    position: relative;
  }

  .preview-table tr:hover {
    background: #f8fafc;
  }

  .editable-cell {
    cursor: pointer;
    border: 1px solid transparent;
    border-radius: 0.25rem;
    padding: 0.25rem;
  }

  .editable-cell:hover {
    border-color: #d1d5db;
    background: #f9fafb;
  }

  .editable-input {
    width: 100%;
    border: none;
    background: none;
    font-size: inherit;
    padding: 0.25rem;
  }

  .editable-input:focus {
    outline: none;
  }

  .validation-summary {
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 0.5rem;
    padding: 1.5rem;
    margin: 1rem 0;
  }

  .validation-item {
    display: flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 0;
  }

  .validation-item.success {
    color: #059669;
  }

  .validation-item.warning {
    color: #d97706;
  }

  .validation-item.error {
    color: #dc2626;
  }

  .settings-grid {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 2rem;
    margin: 1.5rem 0;
  }

  .role-selector {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin-top: 0.5rem;
  }

  .role-chip {
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.875rem;
    cursor: pointer;
    transition: all 0.2s ease;
    border: 2px solid #e5e7eb;
  }

  .role-chip.selected {
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
  }

  .role-chip:hover {
    border-color: #3b82f6;
  }

  .import-results {
    margin-top: 1.5rem;
  }

  .result-item {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.75rem;
    border-radius: 0.5rem;
    margin-bottom: 0.5rem;
  }

  .result-item.success {
    background: #ecfdf5;
    border: 1px solid #d1fae5;
    color: #065f46;
  }

  .result-item.error {
    background: #fef2f2;
    border: 1px solid #fecaca;
    color: #991b1b;
  }

  .result-item.updated {
    background: #eff6ff;
    border: 1px solid #dbeafe;
    color: #1e40af;
  }

  .result-item.skipped {
    background: #fffbeb;
    border: 1px solid #fed7aa;
    color: #92400e;
  }

  .btn {
    padding: 0.75rem 1.5rem;
    border: none;
    border-radius: 0.5rem;
    font-weight: 500;
    cursor: pointer;
    transition: all 0.2s ease;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
  }

  .btn-primary {
    background: #3b82f6;
    color: white;
  }

  .btn-primary:hover {
    background: #2563eb;
  }

  .btn-primary:disabled {
    background: #9ca3af;
    cursor: not-allowed;
  }

  .btn-secondary {
    background: #6b7280;
    color: white;
  }

  .btn-secondary:hover {
    background: #4b5563;
  }

  .btn-danger {
    background: #ef4444;
    color: white;
  }

  .btn-danger:hover {
    background: #dc2626;
  }

  .actions {
    display: flex;
    gap: 1rem;
    margin-top: 2rem;
    justify-content: space-between;
  }

  .actions-left {
    display: flex;
    gap: 1rem;
  }

  .sample-data {
    background: #f8fafc;
    border: 1px solid #e2e8f0;
    border-radius: 0.5rem;
    padding: 1rem;
    margin-bottom: 1rem;
  }

  .sample-data pre {
    margin: 0;
    font-family: 'Courier New', monospace;
    font-size: 0.875rem;
    white-space: pre-wrap;
    color: #374151;
  }
</style>

<div class="import-container">
  <h1>📥 Bulk Import af Brugere</h1>
  
  <!-- Step Indicator -->
  <div class="step-indicator">
    <div class="step {step >= 1 ? (step > 1 ? 'completed' : 'active') : 'pending'}">
      <span>1</span>
      <span>Paste Data</span>
    </div>
    <div class="step {step >= 2 ? (step > 2 ? 'completed' : 'active') : 'pending'}">
      <span>2</span>
      <span>Preview & Rediger</span>
    </div>
    <div class="step {step >= 3 ? (step > 3 ? 'completed' : 'active') : 'pending'}">
      <span>3</span>
      <span>Indstillinger</span>
    </div>
    <div class="step {step >= 4 ? 'active' : 'pending'}">
      <span>4</span>
      <span>Resultat</span>
    </div>
  </div>

  <!-- Step 1: Paste Data -->
  {#if step === 1}
    <div>
      <h2>Trin 1: Indsæt brugerdata</h2>
      <p>Copy-paste data fra Active Directory eller andre kilder. Systemet detekterer automatisk kolonner og ignorerer ukendte felter.</p>
      
      <div class="sample-data">
        <strong>📋 Eksempel format:</strong>
        <pre>Activity    Username    Fullname                    Initial pass    Given nam    Surname
daGF2250    alla437h    Allan Nicolaj i Soylu      C3vk!zRttY8C   Allan Nicolaj Selfoss
daGF2250    user123     John Doe                    TempPass123!   John         Doe</pre>
      </div>

      <textarea 
        class="data-textarea"
        bind:value={rawData}
        placeholder="Indsæt brugerdata her... 

Systemet kan håndtere forskellige formater og detekterer automatisk:
- Username (brugernavn)
- Initial Password (midlertidigt password) 
- Given Name (fornavn - kun første navn gemmes)
- Surname (efternavn - kun første bogstav gemmes)
- Email (hvis tilstede)
- Student ID (hvis tilstede)

Ukendte kolonner ignoreres automatisk."
      ></textarea>

      <div class="actions">
        <div class="actions-left">
          <button class="btn btn-primary" on:click={parseRawData} disabled={loading || !rawData.trim()}>
            {#if loading}
              ⏳ Parser data...
            {:else}
              🔍 Analyser Data
            {/if}
          </button>
        </div>
      </div>
    </div>
  {/if}

  <!-- Step 2: Preview & Edit -->
  {#if step === 2}
    <div>
      <h2>Trin 2: Preview og redigering</h2>
      <p>Detekterede {parsedUsers.length} brugere. Klik på celler for at redigere data.</p>

      {#if detectedColumns.length > 0}
        <div style="margin-bottom: 1rem; padding: 1rem; background: #f0f9ff; border-radius: 0.5rem;">
          <strong>🔍 Detekterede kolonner:</strong>
          <div style="margin-top: 0.5rem; display: flex; flex-wrap: wrap; gap: 0.5rem;">
            {#each detectedColumns as col, i}
              <span style="background: white; padding: 0.25rem 0.5rem; border-radius: 0.25rem; font-size: 0.875rem;">
                {col} → {columnMapping[i] || 'ignoreret'}
              </span>
            {/each}
          </div>
        </div>
      {/if}

      <div style="overflow-x: auto;">
        <table class="preview-table">
          <thead>
            <tr>
              <th>Brugernavn</th>
              <th>Initial Password</th>
              <th>Fornavn (GDPR)</th>
              <th>Efternavn Initial</th>
              <th>Email</th>
              <th>Student ID</th>
              <th>Handlinger</th>
            </tr>
          </thead>
          <tbody>
            {#each parsedUsers as user, index}
              <tr>
                <td>
                  <div class="editable-cell" on:click={() => editUser(index, 'username', prompt('Brugernavn:', user.username) || user.username)}>
                    {user.username}
                  </div>
                </td>
                <td>
                  <div class="editable-cell" on:click={() => editUser(index, 'initialPassword', prompt('Initial Password:', user.initialPassword) || user.initialPassword)}>
                    {'•'.repeat(Math.min(user.initialPassword.length, 8))}
                  </div>
                </td>
                <td>
                  <div class="editable-cell" on:click={() => editUser(index, 'givenName', prompt('Fornavn:', user.givenName) || user.givenName)}>
                    {user.givenName}
                  </div>
                </td>
                <td>
                  <div class="editable-cell" on:click={() => editUser(index, 'surname', prompt('Efternavn:', user.surname) || user.surname)}>
                    {user.surname}
                  </div>
                </td>
                <td>
                  <div class="editable-cell" on:click={() => editUser(index, 'email', prompt('Email:', user.email || '') || user.email)}>
                    {user.email || '-'}
                  </div>
                </td>
                <td>
                  <div class="editable-cell" on:click={() => editUser(index, 'studentId', prompt('Student ID:', user.studentId || '') || user.studentId)}>
                    {user.studentId || '-'}
                  </div>
                </td>
                <td>
                  <button class="btn btn-danger" style="padding: 0.25rem 0.5rem; font-size: 0.875rem;" on:click={() => removeUser(index)}>
                    🗑️
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>

      <div class="actions">
        <div class="actions-left">
          <button class="btn btn-secondary" on:click={() => step = 1}>
            ← Tilbage til data
          </button>
          <button class="btn btn-primary" on:click={validateData} disabled={loading || parsedUsers.length === 0}>
            {#if loading}
              ⏳ Validerer...
            {:else}
              ✅ Validér Data
            {/if}
          </button>
        </div>
      </div>
    </div>
  {/if}

  <!-- Step 3: Settings & Validation -->
  {#if step === 3}
    <div>
      <h2>Trin 3: Import indstillinger</h2>
      
      {#if validationResult}
        <div class="validation-summary">
          <h3>📊 Validerings resultat</h3>
          <div class="validation-item success">
            ✅ {validationResult.readyForImport} brugere klar til import
          </div>
          {#if validationResult.duplicatesInRequest > 0}
            <div class="validation-item warning">
              ⚠️ {validationResult.duplicatesInRequest} duplikater i data
            </div>
          {/if}
          {#if validationResult.existingInDatabase > 0}
            <div class="validation-item warning">
              ⚠️ {validationResult.existingInDatabase} brugere eksisterer allerede
            </div>
          {/if}
          {#if validationResult.usersWithIssues > 0}
            <div class="validation-item error">
              ❌ {validationResult.usersWithIssues} brugere har problemer
            </div>
          {/if}
        </div>
      {/if}

      <div class="settings-grid">
        <div>
          <h4>🎯 Standard roller</h4>
          <p>Vælg hvilke roller der automatisk tildeles til nye brugere</p>
          <div class="role-selector">
            {#each availableRoles as role}
              <div 
                class="role-chip {importSettings.defaultRoles.includes(role) ? 'selected' : ''}"
                on:click={() => toggleRole(role)}
              >
                {role}
              </div>
            {/each}
          </div>
        </div>

        <div>
          <h4>🏢 Standard afdeling</h4>
          <input 
            type="text" 
            bind:value={importSettings.defaultDepartment}
            placeholder="f.eks. Datamatiker"
            style="width: 100%; padding: 0.5rem; border: 1px solid #d1d5db; border-radius: 0.25rem;"
          />
        </div>
      </div>

      <div style="margin: 1.5rem 0;">
        <h4>⚙️ Import indstillinger</h4>
        <label style="display: flex; align-items: center; gap: 0.5rem; margin: 0.5rem 0;">
          <input type="checkbox" bind:checked={importSettings.updateExisting} />
          Opdater eksisterende brugere
        </label>
        <label style="display: flex; align-items: center; gap: 0.5rem; margin: 0.5rem 0;">
          <input type="checkbox" bind:checked={importSettings.sendWelcomeEmails} />
          Send velkomst emails
        </label>
        <label style="display: flex; align-items: center; gap: 0.5rem; margin: 0.5rem 0;">
          <input type="checkbox" bind:checked={importSettings.skipDuplicates} />
          Spring duplikater over
        </label>
      </div>

      <div class="actions">
        <div class="actions-left">
          <button class="btn btn-secondary" on:click={() => step = 2}>
            ← Tilbage til preview
          </button>
          <button class="btn btn-primary" on:click={importUsers} disabled={importing}>
            {#if importing}
              ⏳ Importerer...
            {:else}
              🚀 Start Import ({parsedUsers.length} brugere)
            {/if}
          </button>
        </div>
      </div>
    </div>
  {/if}

  <!-- Step 4: Results -->
  {#if step === 4 && importResult}
    <div>
      <h2>Trin 4: Import resultat</h2>
      
      <div class="validation-summary">
        <h3>📈 Import oversigt</h3>
        <div class="validation-item success">
          ✅ {importResult.successCount} brugere oprettet succesfuldt
        </div>
        {#if importResult.updatedCount > 0}
          <div class="validation-item success">
            🔄 {importResult.updatedCount} brugere opdateret
          </div>
        {/if}
        {#if importResult.skippedCount > 0}
          <div class="validation-item warning">
            ⏭️ {importResult.skippedCount} brugere sprunget over
          </div>
        {/if}
        {#if importResult.failedCount > 0}
          <div class="validation-item error">
            ❌ {importResult.failedCount} brugere fejlede
          </div>
        {/if}
      </div>

      {#if importResult.results && importResult.results.length > 0}
        <div class="import-results">
          <h4>📋 Detaljeret resultat</h4>
          {#each importResult.results as result}
            <div class="result-item {result.action.toLowerCase()}">
              <span style="font-weight: 600;">{result.username}</span>
              <span>→</span>
              <span>{result.action}</span>
              {#if result.message}
                <span style="color: #6b7280;">({result.message})</span>
              {/if}
              {#if result.error}
                <span style="color: #dc2626;">Fejl: {result.error}</span>
              {/if}
            </div>
          {/each}
        </div>
      {/if}

      <div class="actions">
        <div class="actions-left">
          <button class="btn btn-primary" on:click={resetImport}>
            🔄 Ny Import
          </button>
          <a href="/" class="btn btn-secondary" style="text-decoration: none;">
            👥 Gå til Brugere
          </a>
        </div>
      </div>
    </div>
  {/if}
</div> 