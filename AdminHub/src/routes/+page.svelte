<script lang="ts">
  import { onMount } from 'svelte';
  import { auth } from '$lib/auth';
  import { get } from 'svelte/store';

  let emailOrUsername = '';
  let password = '';
  let loginError = '';
  let users: any[] = [];
  let loading = false;
  let editingUser: any = null;
  let editModal = false;
  let saving = false;
  let webhookTesting = false;
  let webhookMessage = '';
  let showWebhookTest = false;
  let showAdvancedWebhook = false;
  let announcementData = {
    type: 'news',
    title: '',
    description: '',
    thumbnailUrl: '',
    imageUrl: '',
    // Type-specific fields
    startDate: '',
    instructor: '',
    duration: '',
    prerequisites: '',
    maxStudents: '',
    eventDate: '',
    location: '',
    maxParticipants: '',
    organizer: '',
    price: '',
    registrationLink: '',
    author: '',
    category: '',
    readMoreLink: '',
    deadline: '',
    actionRequired: '',
    contactInfo: ''
  };
  let companyPostData = {
    companyName: '',
    jobTitle: '',
    description: '',
    location: '',
    salaryRange: '',
    workType: 'Fuldtid',
    requiredSkills: [],
    applicationDeadline: '',
    contactPerson: '',
    applicationLink: '',
    companyLogo: '',
    imageUrl: ''
  };
  let availableTemplates = [];
  let showCompanyPost = false;
  let newSkill = '';

  // Prøv at læse token fra localStorage ved load
  onMount(() => {
    const saved = localStorage.getItem('auth');
    if (saved) {
      auth.set(JSON.parse(saved));
      fetchUsers();
      fetchTemplates();
    }
  });

  // Login-funktion
  async function login() {
    loginError = '';
    loading = true;
    try {
      const res = await fetch('http://localhost:5053/api/Auth/login', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ emailOrUsername, password })
      });
      if (!res.ok) {
        loginError = 'Forkert login';
        loading = false;
        return;
      }
      const data = await res.json();
      auth.set(data);
      localStorage.setItem('auth', JSON.stringify(data));
      await fetchUsers();
    } catch (e) {
      loginError = 'Netværksfejl';
    }
    loading = false;
  }

  // Hent brugere
  async function fetchUsers() {
    const { accessToken } = get(auth);
    if (!accessToken) return;
    const res = await fetch('http://localhost:5053/api/User', {
      headers: { Authorization: `Bearer ${accessToken}` }
    });
    if (res.ok) {
      users = await res.json();
    }
  }

  // Start redigering af bruger
  function startEdit(user: any) {
    editingUser = { ...user, roles: [...(user.roles || [])] };
    editModal = true;
  }

  // Gem redigering
  async function saveEdit() {
    if (!editingUser) return;
    
    saving = true;
    try {
      const { accessToken } = get(auth);
      const res = await fetch(`http://localhost:5053/api/User/${editingUser.id}`, {
        method: 'PUT',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          username: editingUser.username,
          email: editingUser.email,
          level: editingUser.level,
          experience: editingUser.experience,
          roles: editingUser.roles,
          isActive: editingUser.isActive
        })
      });
      
      if (res.ok) {
        const updatedUser = await res.json();
        const index = users.findIndex(u => u.id === editingUser.id);
        if (index >= 0) {
          users[index] = updatedUser;
          users = [...users]; // Trigger reactivity
        }
        editModal = false;
        editingUser = null;
      } else {
        const error = await res.json();
        alert(error.message || 'Fejl ved opdatering');
      }
    } catch (e) {
      alert('Netværksfejl ved opdatering');
    }
    saving = false;
  }

  // Annuller redigering
  function cancelEdit() {
    editModal = false;
    editingUser = null;
  }

  // Tilføj eller fjern rolle
  function toggleRole(role: string) {
    if (!editingUser) return;
    
    const index = editingUser.roles.indexOf(role);
    if (index >= 0) {
      editingUser.roles.splice(index, 1);
    } else {
      editingUser.roles.push(role);
    }
    editingUser.roles = [...editingUser.roles]; // Trigger reactivity
  }

  // Slet bruger
  async function deleteUser(user: any) {
    if (!confirm(`Er du sikker på at du vil slette brugeren "${user.username}"?`)) return;
    
    try {
      const { accessToken } = get(auth);
      const res = await fetch(`http://localhost:5053/api/User/${user.id}`, {
        method: 'DELETE',
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      
      if (res.ok) {
        users = users.filter(u => u.id !== user.id);
      } else {
        const error = await res.json();
        alert(error.message || 'Fejl ved sletning');
      }
    } catch (e) {
      alert('Netværksfejl ved sletning');
    }
  }

  // Tilgængelige roller i rangeret rækkefølge (højest til lavest)
  const roleHierarchy = ['Admin', 'Teacher', 'Developer', 'AdvisoryBoard', 'Student'];
  const availableRoles = ['Student', 'Teacher', 'Admin', 'Developer', 'AdvisoryBoard'];

  // Funktion til at få den højeste rolle og antal ekstra
  function getDisplayRole(userRoles: string[] = []) {
    if (!userRoles || userRoles.length === 0) {
      return { primary: 'Student', extra: 0 };
    }

    // Find højeste rangerende rolle
    let highestRole = 'Student';
    let highestIndex = roleHierarchy.length;

    for (const role of userRoles) {
      const index = roleHierarchy.indexOf(role);
      if (index !== -1 && index < highestIndex) {
        highestIndex = index;
        highestRole = role;
      }
    }

    const extraCount = userRoles.length - 1;
    return { 
      primary: highestRole, 
      extra: extraCount > 0 ? extraCount : 0 
    };
  }

  // Få rolle farve baseret på type
  function getRoleColor(role: string) {
    const colors = {
      'Admin': '#dc2626',      // rød
      'Teacher': '#2563eb',    // blå
      'Developer': '#059669',  // grøn
      'AdvisoryBoard': '#7c3aed', // lilla
      'Student': '#6b7280'     // grå
    };
    return colors[role] || colors['Student'];
  }

  // Danske oversættelser af roller
  function getRoleDanish(role: string) {
    const translations = {
      'Admin': 'Admin',
      'Teacher': 'Lærer',
      'Developer': 'Udvikler',
      'AdvisoryBoard': 'Bestyrelse',
      'Student': 'Elev'
    };
    return translations[role] || role;
  }

  // Logout
  function logout() {
    auth.set({ accessToken: null, refreshToken: null, user: null });
    localStorage.removeItem('auth');
    users = [];
  }

  // XP og Level beregninger
  function calculateXPProgress(currentXP: number, currentLevel: number) {
    // Basis XP system - dette skal matche backend XPConfig
    const baseXP = 100;
    const levelMultiplier = 1.5;
    
    // Beregn XP krævet for næste level
    const xpForNextLevel = Math.floor(baseXP * Math.pow(levelMultiplier, currentLevel));
    
    // Beregn XP krævet for nuværende level
    const xpForCurrentLevel = currentLevel > 1 ? Math.floor(baseXP * Math.pow(levelMultiplier, currentLevel - 1)) : 0;
    
    // XP progress i nuværende level
    const xpInCurrentLevel = currentXP - xpForCurrentLevel;
    const xpNeededForNextLevel = xpForNextLevel - xpForCurrentLevel;
    
    // Sørg for at vi ikke går over 100%
    const progress = Math.min((xpInCurrentLevel / xpNeededForNextLevel) * 100, 100);
    
    return {
      current: xpInCurrentLevel,
      required: xpNeededForNextLevel,
      total: currentXP,
      nextLevelAt: xpForNextLevel,
      progress: Math.max(0, progress)
    };
  }

  // Test webhook
  async function testWebhook() {
    if (!webhookMessage.trim()) {
      alert('Indtast en test besked');
      return;
    }
    
    webhookTesting = true;
    try {
      const { accessToken } = get(auth);
      const res = await fetch('http://localhost:5053/api/Webhook/test-discord', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify({
          message: webhookMessage,
          username: 'Admin Test',
          createEmbed: true
        })
      });
      
      if (res.ok) {
        const result = await res.json();
        alert('Webhook test sendt succesfuldt! Tjek Discord kanalen.');
        webhookMessage = '';
        showWebhookTest = false;
      } else {
        const error = await res.json();
        alert(error.message || 'Fejl ved webhook test');
      }
    } catch (e) {
      alert('Netværksfejl ved webhook test');
    }
    webhookTesting = false;
  }

  // Hent tilgængelige templates ved opstart
  async function fetchTemplates() {
    try {
      const { accessToken } = get(auth);
      const res = await fetch('http://localhost:5053/api/Webhook/templates', {
        headers: { 'Authorization': `Bearer ${accessToken}` }
      });
      if (res.ok) {
        availableTemplates = await res.json();
      }
    } catch (e) {
      console.error('Fejl ved hentning af templates:', e);
    }
  }

  // Send announcement
  async function sendAnnouncement() {
    webhookTesting = true;
    try {
      const { accessToken } = get(auth);
      
      // Prepare data based on type
      const requestData = {
        type: announcementData.type,
        title: announcementData.title,
        description: announcementData.description,
        thumbnailUrl: announcementData.thumbnailUrl || null,
        imageUrl: announcementData.imageUrl || null
      };

      // Add type-specific fields
      if (announcementData.type === 'new_course') {
        if (announcementData.startDate) requestData.startDate = new Date(announcementData.startDate).toISOString();
        if (announcementData.instructor) requestData.instructor = announcementData.instructor;
        if (announcementData.duration) requestData.duration = announcementData.duration;
        if (announcementData.prerequisites) requestData.prerequisites = announcementData.prerequisites;
        if (announcementData.maxStudents) requestData.maxStudents = parseInt(announcementData.maxStudents);
      } else if (announcementData.type === 'event') {
        if (announcementData.eventDate) requestData.eventDate = new Date(announcementData.eventDate).toISOString();
        if (announcementData.location) requestData.location = announcementData.location;
        if (announcementData.maxParticipants) requestData.maxParticipants = parseInt(announcementData.maxParticipants);
        if (announcementData.organizer) requestData.organizer = announcementData.organizer;
        if (announcementData.price) requestData.price = announcementData.price;
        if (announcementData.registrationLink) requestData.registrationLink = announcementData.registrationLink;
      } else if (announcementData.type === 'news') {
        if (announcementData.author) requestData.author = announcementData.author;
        if (announcementData.category) requestData.category = announcementData.category;
        if (announcementData.readMoreLink) requestData.readMoreLink = announcementData.readMoreLink;
      } else if (announcementData.type === 'urgent') {
        if (announcementData.deadline) requestData.deadline = new Date(announcementData.deadline).toISOString();
        if (announcementData.actionRequired) requestData.actionRequired = announcementData.actionRequired;
        if (announcementData.contactInfo) requestData.contactInfo = announcementData.contactInfo;
      }

      const res = await fetch('http://localhost:5053/api/Webhook/announce', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify(requestData)
      });
      
      if (res.ok) {
        alert('Announcement sendt succesfuldt! Tjek Discord kanalen.');
        resetAnnouncementData();
        showAdvancedWebhook = false;
      } else {
        const error = await res.json();
        alert(error.message || 'Fejl ved sending af announcement');
      }
    } catch (e) {
      alert('Netværksfejl ved sending af announcement');
    }
    webhookTesting = false;
  }

  // Send company post
  async function sendCompanyPost() {
    webhookTesting = true;
    try {
      const { accessToken } = get(auth);
      
      const requestData = {
        ...companyPostData,
        applicationDeadline: companyPostData.applicationDeadline ? new Date(companyPostData.applicationDeadline).toISOString() : null
      };

      const res = await fetch('http://localhost:5053/api/Webhook/company-post', {
        method: 'POST',
        headers: { 
          'Content-Type': 'application/json',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify(requestData)
      });
      
      if (res.ok) {
        alert('Virksomheds-opslag sendt succesfuldt! Tjek Discord kanalen.');
        resetCompanyPostData();
        showCompanyPost = false;
      } else {
        const error = await res.json();
        alert(error.message || 'Fejl ved sending af virksomheds-opslag');
      }
    } catch (e) {
      alert('Netværksfejl ved sending af virksomheds-opslag');
    }
    webhookTesting = false;
  }

  // Add skill to company post
  function addSkill() {
    if (newSkill.trim() && !companyPostData.requiredSkills.includes(newSkill.trim())) {
      companyPostData.requiredSkills = [...companyPostData.requiredSkills, newSkill.trim()];
      newSkill = '';
    }
  }

  // Remove skill from company post
  function removeSkill(skill) {
    companyPostData.requiredSkills = companyPostData.requiredSkills.filter(s => s !== skill);
  }

  // Reset form data
  function resetAnnouncementData() {
    announcementData = {
      type: 'news',
      title: '',
      description: '',
      thumbnailUrl: '',
      imageUrl: '',
      startDate: '',
      instructor: '',
      duration: '',
      prerequisites: '',
      maxStudents: '',
      eventDate: '',
      location: '',
      maxParticipants: '',
      organizer: '',
      price: '',
      registrationLink: '',
      author: '',
      category: '',
      readMoreLink: '',
      deadline: '',
      actionRequired: '',
      contactInfo: ''
    };
  }

  function resetCompanyPostData() {
    companyPostData = {
      companyName: '',
      jobTitle: '',
      description: '',
      location: '',
      salaryRange: '',
      workType: 'Fuldtid',
      requiredSkills: [],
      applicationDeadline: '',
      contactPerson: '',
      applicationLink: '',
      companyLogo: '',
      imageUrl: ''
    };
  }

  // Get current template
  $: currentTemplate = availableTemplates.find(t => t.type === announcementData.type);
</script>

<style>
  :global(body) {
    background: #f8fafc;
    color: #111;
    font-family: 'Inter', system-ui, sans-serif;
  }
  .admin-nav {
    display: flex;
    gap: 2rem;
    padding: 1.5rem 2rem 1rem 2rem;
    background: #fff;
    border-bottom: 1px solid #e5e7eb;
    font-weight: 500;
    font-size: 1.1rem;
    box-shadow: 0 2px 8px 0 rgba(0,0,0,0.02);
  }
  .admin-main {
    max-width: 1100px;
    margin: 2rem auto;
    background: #fff;
    border-radius: 1.2rem;
    box-shadow: 0 4px 32px 0 rgba(0,0,0,0.04);
    padding: 2.5rem 2rem;
  }
  h1 {
    font-size: 2rem;
    font-weight: 700;
    margin-bottom: 1.5rem;
  }
  .placeholder {
    color: #64748b;
    font-size: 1.1rem;
    margin-top: 2rem;
  }
  .avatar {
    width: 32px;
    height: 32px;
    border-radius: 50%;
    object-fit: cover;
    border: 2px solid #e5e7eb;
  }
  .user-info {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  .modal {
    position: fixed;
    top: 0;
    left: 0;
    width: 100%;
    height: 100%;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }
  
  .modal-content {
    background: white;
    padding: 2rem;
    border-radius: 0.5rem;
    width: 90%;
    max-width: 500px;
    max-height: 80vh;
    overflow-y: auto;
  }
  
  .form-group {
    margin-bottom: 1rem;
  }
  
  .form-group label {
    display: block;
    margin-bottom: 0.25rem;
    font-weight: 500;
  }
  
  .form-group input, .form-group select {
    width: 100%;
    padding: 0.5rem;
    border: 1px solid #d1d5db;
    border-radius: 0.25rem;
  }
  
  .roles-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
    gap: 0.75rem;
    margin-top: 0.75rem;
  }
  
  .role-card {
    position: relative;
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.75rem;
    border: 2px solid #e5e7eb;
    border-radius: 0.5rem;
    cursor: pointer;
    transition: all 0.2s ease;
    background: #fafafa;
  }
  
  .role-card:hover {
    border-color: #d1d5db;
    background: #f9fafb;
    transform: translateY(-1px);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
  }
  
  .role-card.selected {
    border-color: var(--role-color);
    background: var(--role-color);
    color: white;
    box-shadow: 0 2px 12px rgba(0, 0, 0, 0.15);
  }
  
  .role-card.selected:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 16px rgba(0, 0, 0, 0.2);
  }
  
  .role-checkbox-input {
    position: absolute;
    opacity: 0;
    pointer-events: none;
  }
  
  .role-icon {
    width: 20px;
    height: 20px;
    border: 2px solid currentColor;
    border-radius: 4px;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: all 0.2s ease;
    flex-shrink: 0;
  }
  
  .role-card.selected .role-icon {
    background: rgba(255, 255, 255, 0.2);
    border-color: rgba(255, 255, 255, 0.8);
  }
  
  .role-icon svg {
    width: 12px;
    height: 12px;
    opacity: 0;
    transition: opacity 0.2s ease;
  }
  
  .role-card.selected .role-icon svg {
    opacity: 1;
  }
  
  .role-name {
    font-weight: 500;
    font-size: 0.875rem;
    transition: color 0.2s ease;
  }
  
  .role-card.selected .role-name {
    color: white;
  }
  
  .roles-preview {
    display: flex;
    flex-wrap: wrap;
    gap: 0.5rem;
    margin-top: 1rem;
    padding: 1rem;
    background: #f8fafc;
    border-radius: 0.5rem;
    border: 1px solid #e2e8f0;
  }
  
  .roles-preview-label {
    font-weight: 600;
    color: #475569;
    margin-bottom: 0.5rem;
    display: block;
    width: 100%;
  }
  
  .role-tag {
    display: inline-flex;
    align-items: center;
    gap: 0.375rem;
    padding: 0.375rem 0.75rem;
    background: white;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 500;
    color: #374151;
    border: 1px solid #e5e7eb;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.05);
  }
  
  .role-tag-dot {
    width: 8px;
    height: 8px;
    border-radius: 50%;
    flex-shrink: 0;
  }
  
  /* Forbedret form styling */
  .form-group input[type="number"], 
  .form-group input[type="email"], 
  .form-group input[type="text"] {
    width: 100%;
    padding: 0.75rem;
    border: 2px solid #e5e7eb;
    border-radius: 0.5rem;
    font-size: 0.875rem;
    transition: border-color 0.2s ease, box-shadow 0.2s ease;
  }
  
  .form-group input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
  
  .form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 600;
    color: #374151;
  }
  
  .checkbox-card {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    padding: 0.75rem;
    border: 2px solid #e5e7eb;
    border-radius: 0.5rem;
    cursor: pointer;
    transition: all 0.2s ease;
    background: #fafafa;
  }
  
  .checkbox-card:hover {
    border-color: #d1d5db;
    background: #f9fafb;
  }
  
  .checkbox-card.checked {
    border-color: #10b981;
    background: #ecfdf5;
  }
  
  .actions {
    display: flex;
    gap: 0.5rem;
  }
  
  .btn {
    padding: 0.25rem 0.5rem;
    border: none;
    border-radius: 0.25rem;
    cursor: pointer;
    font-size: 0.875rem;
  }
  
  .btn-edit {
    background: #3b82f6;
    color: white;
  }
  
  .btn-delete {
    background: #ef4444;
    color: white;
  }
  
  .btn-primary {
    background: #3b82f6;
    color: white;
    padding: 0.5rem 1rem;
  }
  
  .btn-secondary {
    background: #6b7280;
    color: white;
    padding: 0.5rem 1rem;
  }
  
  .modal-actions {
    display: flex;
    gap: 1rem;
    margin-top: 1.5rem;
  }
  
  .role-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.125rem 0.5rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 500;
    color: white;
  }
  
  .role-extra {
    opacity: 0.8;
    font-weight: 400;
  }
  
  .xp-container {
    display: flex;
    flex-direction: column;
    gap: 0.25rem;
    min-width: 120px;
  }
  
  .xp-text {
    font-size: 0.75rem;
    color: #6b7280;
    text-align: center;
  }
  
  .xp-progress-bar {
    width: 100%;
    height: 8px;
    background: #e5e7eb;
    border-radius: 9999px;
    overflow: hidden;
    position: relative;
  }
  
  .xp-progress-fill {
    height: 100%;
    background: linear-gradient(90deg, #3b82f6, #1d4ed8);
    border-radius: 9999px;
    transition: width 0.3s ease;
    position: relative;
  }
  
  .xp-progress-fill::after {
    content: '';
    position: absolute;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: linear-gradient(90deg, transparent 0%, rgba(255,255,255,0.3) 50%, transparent 100%);
    animation: shimmer 2s infinite;
  }
  
  @keyframes shimmer {
    0% { transform: translateX(-100%); }
    100% { transform: translateX(100%); }
  }
  
  .level-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 2rem;
    height: 2rem;
    background: linear-gradient(135deg, #f59e0b, #d97706);
    color: white;
    border-radius: 50%;
    font-weight: 700;
    font-size: 0.875rem;
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
  }
  
  .xp-display {
    display: flex;
    align-items: center;
    gap: 0.75rem;
  }
  
  /* Forbedret tabel styling */
  table {
    border-spacing: 0;
  }
  
  tbody tr:hover {
    background: #f8fafc;
  }
  
  td {
    border-bottom: 1px solid #f1f5f9;
  }
  
  .nav-user-info {
    display: flex;
    align-items: center;
    gap: 1rem;
    margin-left: auto;
  }
  
  .nav-role-badge {
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
    padding: 0.125rem 0.5rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 500;
    color: white;
    box-shadow: 0 1px 2px rgba(0, 0, 0, 0.1);
  }
  
  .nav-role-extra {
    opacity: 0.8;
    font-weight: 400;
  }
  
  .logout-btn {
    padding: 0.375rem 0.75rem;
    background: #ef4444;
    color: white;
    border: none;
    border-radius: 0.375rem;
    font-size: 0.875rem;
    cursor: pointer;
    transition: background 0.2s ease;
    font-weight: 500;
  }
  
  .logout-btn:hover {
    background: #dc2626;
  }
</style>

<nav class="admin-nav">
  <span>Admin Dashboard</span>
  <a href="#" style="color:#2563eb">Brugere</a>
  <a href="/bulk-import" style="color:#059669">📥 Bulk Import</a>
  <a href="#">Statistik</a>
  <a href="#">Indstillinger</a>
  
  <!-- Webhook dropdown menu -->
  <div style="position: relative; display: inline-block;">
    <button 
      on:click={() => showWebhookTest = !showWebhookTest} 
      style="background: #059669; color: white; border: none; padding: 0.375rem 0.75rem; border-radius: 0.375rem; font-size: 0.875rem; cursor: pointer;"
    >
      🔗 Webhooks ▼
    </button>
    {#if showWebhookTest}
      <div style="position: absolute; top: 100%; left: 0; background: white; border: 1px solid #e5e7eb; border-radius: 0.5rem; box-shadow: 0 4px 12px rgba(0,0,0,0.1); z-index: 1000; min-width: 200px; padding: 0.5rem 0;">
        <button 
          on:click={() => { showWebhookTest = false; showAdvancedWebhook = true; }}
          style="width: 100%; text-align: left; padding: 0.75rem 1rem; background: none; border: none; cursor: pointer; display: flex; align-items: center; gap: 0.5rem;"
        >
          📢 Send Announcement
        </button>
        <button 
          on:click={() => { showWebhookTest = false; showCompanyPost = true; }}
          style="width: 100%; text-align: left; padding: 0.75rem 1rem; background: none; border: none; cursor: pointer; display: flex; align-items: center; gap: 0.5rem;"
        >
          🏢 Virksomheds-opslag
        </button>
        <hr style="margin: 0.5rem 0; border: none; border-top: 1px solid #e5e7eb;">
        <button 
          on:click={() => { showWebhookTest = false; /* show simple test */ }}
          style="width: 100%; text-align: left; padding: 0.75rem 1rem; background: none; border: none; cursor: pointer; display: flex; align-items: center; gap: 0.5rem;"
        >
          🧪 Simpel Test
        </button>
      </div>
    {/if}
  </div>
  
  <!-- User info -->
  {#if $auth.user}
    {@const userDisplayRole = getDisplayRole($auth.user.roles)}
    <div class="nav-user-info">
      <span style="font-weight: 500;">{$auth.user.username}</span>
      <div 
        class="nav-role-badge" 
        style="background-color: {getRoleColor(userDisplayRole.primary)}"
        title={$auth.user.roles?.join(', ') || 'Ingen roller'}
      >
        {getRoleDanish(userDisplayRole.primary)}
        {#if userDisplayRole.extra > 0}
          <span class="nav-role-extra">+{userDisplayRole.extra}</span>
        {/if}
      </div>
      <button on:click={logout} class="logout-btn">
        Log ud
      </button>
    </div>
  {/if}
</nav>

<main class="admin-main">
  {#if !$auth.accessToken}
    <h2>Login</h2>
    <form on:submit|preventDefault={login} style="max-width:350px;">
      <input
        placeholder="Email eller brugernavn"
        bind:value={emailOrUsername}
        required
        style="width:100%;margin-bottom:1rem;padding:0.5rem;"
      />
      <input
        type="password"
        placeholder="Password"
        bind:value={password}
        required
        style="width:100%;margin-bottom:1rem;padding:0.5rem;"
      />
      <button type="submit" disabled={loading} style="width:100%;padding:0.7rem;background:#2563eb;color:#fff;border:none;border-radius:0.4rem;">
        {loading ? 'Logger ind...' : 'Login'}
      </button>
      {#if loginError}
        <div style="color:#dc2626;margin-top:1rem;">{loginError}</div>
      {/if}
    </form>
  {:else}
    <h1>Brugere</h1>
    {#if users.length === 0}
      <div class="placeholder">Ingen brugere fundet.</div>
    {:else}
      <table style="width:100%;margin-top:2rem;border-collapse:collapse;">
        <thead>
          <tr style="background:#f1f5f9;">
            <th style="text-align:left;padding:0.75rem;">Bruger</th>
            <th style="text-align:left;padding:0.75rem;">Email</th>
            <th style="text-align:left;padding:0.75rem;">Discord ID</th>
            <th style="text-align:left;padding:0.75rem;">Level & XP</th>
            <th style="text-align:left;padding:0.75rem;">Rolle</th>
            <th style="text-align:left;padding:0.75rem;">Handlinger</th>
          </tr>
        </thead>
        <tbody>
          {#each users as u}
            {@const displayRole = getDisplayRole(u.roles)}
            {@const xpProgress = calculateXPProgress(u.experience || 0, u.level || 1)}
            <tr>
              <td style="padding:0.75rem;">
                <div class="user-info">
                  {#if u.avatarUrl}
                    <img src={u.avatarUrl} alt="{u.username} avatar" class="avatar" />
                  {:else}
                    <div class="avatar" style="background:#e5e7eb;display:flex;align-items:center;justify-content:center;color:#6b7280;font-size:0.75rem;">
                      {u.username?.charAt(0)?.toUpperCase() || '?'}
                    </div>
                  {/if}
                  <div>
                    <div style="font-weight:500;">{u.username}</div>
                    {#if u.globalName && u.globalName !== u.username}
                      <div style="font-size:0.875rem;color:#6b7280;">{u.globalName}</div>
                    {/if}
                    {#if !u.isActive}
                      <div style="font-size:0.75rem;color:#ef4444;">Deaktiveret</div>
                    {/if}
                  </div>
                </div>
              </td>
              <td style="padding:0.75rem;">{u.email || '-'}</td>
              <td style="padding:0.75rem;font-family:monospace;font-size:0.875rem;">{u.discordId || '-'}</td>
              <td style="padding:0.75rem;">
                <div class="xp-display">
                  <div class="level-badge">{u.level || 1}</div>
                  <div class="xp-container">
                    <div class="xp-text">
                      {xpProgress.current.toLocaleString()} / {xpProgress.required.toLocaleString()} XP
                    </div>
                    <div class="xp-progress-bar">
                      <div 
                        class="xp-progress-fill" 
                        style="width: {xpProgress.progress}%"
                        title="{xpProgress.progress.toFixed(1)}% til næste level"
                      ></div>
                    </div>
                    <div class="xp-text" style="font-size: 0.6875rem; opacity: 0.7;">
                      {xpProgress.progress.toFixed(1)}% til level {(u.level || 1) + 1}
                    </div>
                  </div>
                </div>
              </td>
              <td style="padding:0.75rem;">
                <div 
                  class="role-badge" 
                  style="background-color: {getRoleColor(displayRole.primary)}"
                  title={u.roles?.join(', ') || 'Ingen roller'}
                >
                  {getRoleDanish(displayRole.primary)}
                  {#if displayRole.extra > 0}
                    <span class="role-extra">+{displayRole.extra}</span>
                  {/if}
                </div>
              </td>
              <td style="padding:0.75rem;">
                <div class="actions">
                  <button class="btn btn-edit" on:click={() => startEdit(u)}>Rediger</button>
                  <button class="btn btn-delete" on:click={() => deleteUser(u)}>Slet</button>
                </div>
              </td>
            </tr>
          {/each}
        </tbody>
      </table>
    {/if}
  {/if}
</main>

<!-- Edit Modal -->
{#if editModal && editingUser}
  <div class="modal">
    <div class="modal-content">
      <h2>Rediger Bruger</h2>
      
      <div class="form-group">
        <label for="username">Brugernavn:</label>
        <input id="username" type="text" bind:value={editingUser.username} />
      </div>
      
      <div class="form-group">
        <label for="email">Email:</label>
        <input id="email" type="email" bind:value={editingUser.email} />
      </div>
      
      <div class="form-group">
        <label for="level">Level:</label>
        <input id="level" type="number" min="1" bind:value={editingUser.level} />
      </div>
      
      <div class="form-group">
        <label for="experience">Experience:</label>
        <input id="experience" type="number" min="0" bind:value={editingUser.experience} />
        
        <!-- XP Preview i edit modal -->
        {#if editingUser.level && editingUser.experience !== undefined}
          {@const previewXP = calculateXPProgress(editingUser.experience, editingUser.level)}
          <div style="margin-top: 0.5rem; padding: 0.75rem; background: #f8fafc; border-radius: 0.375rem; border: 1px solid #e2e8f0;">
            <div style="display: flex; align-items: center; gap: 0.75rem; margin-bottom: 0.5rem;">
              <div class="level-badge" style="transform: scale(0.8);">{editingUser.level}</div>
              <div>
                <div style="font-weight: 500; font-size: 0.875rem;">
                  {previewXP.current.toLocaleString()} / {previewXP.required.toLocaleString()} XP
                </div>
                <div style="font-size: 0.75rem; color: #6b7280;">
                  {previewXP.progress.toFixed(1)}% til level {editingUser.level + 1}
                </div>
              </div>
            </div>
            <div class="xp-progress-bar">
              <div 
                class="xp-progress-fill" 
                style="width: {previewXP.progress}%"
              ></div>
            </div>
          </div>
        {/if}
      </div>
      
      <div class="form-group">
        <label>Roller:</label>
        <div class="roles-grid">
          {#each availableRoles as role}
            {@const isSelected = editingUser.roles?.includes(role)}
            <label 
              class="role-card {isSelected ? 'selected' : ''}" 
              style="--role-color: {getRoleColor(role)}"
            >
              <input 
                type="checkbox" 
                class="role-checkbox-input"
                checked={isSelected}
                on:change={() => toggleRole(role)}
              />
              <div class="role-icon">
                <svg fill="currentColor" viewBox="0 0 20 20">
                  <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
                </svg>
              </div>
              <span class="role-name">{getRoleDanish(role)}</span>
            </label>
          {/each}
        </div>
        
        <!-- Vis preview af valgte roller -->
        {#if editingUser.roles && editingUser.roles.length > 0}
          <div class="roles-preview">
            <span class="roles-preview-label">Valgte roller:</span>
            {#each editingUser.roles as role}
              <span class="role-tag">
                <div class="role-tag-dot" style="background-color: {getRoleColor(role)}"></div>
                {getRoleDanish(role)}
              </span>
            {/each}
          </div>
        {/if}
      </div>
      
      <div class="form-group">
        <label 
          class="checkbox-card {editingUser.isActive ? 'checked' : ''}"
          style="margin-top: 0.5rem;"
        >
          <input 
            type="checkbox" 
            bind:checked={editingUser.isActive}
            style="margin: 0;"
          />
          <div class="role-icon" style="color: #10b981;">
            <svg fill="currentColor" viewBox="0 0 20 20">
              <path fill-rule="evenodd" d="M16.707 5.293a1 1 0 010 1.414l-8 8a1 1 0 01-1.414 0l-4-4a1 1 0 011.414-1.414L8 12.586l7.293-7.293a1 1 0 011.414 0z" clip-rule="evenodd" />
            </svg>
          </div>
          <span style="font-weight: 500;">Aktiv bruger</span>
        </label>
      </div>
      
      <div class="modal-actions">
        <button class="btn btn-primary" on:click={saveEdit} disabled={saving}>
          {saving ? 'Gemmer...' : 'Gem'}
        </button>
        <button class="btn btn-secondary" on:click={cancelEdit}>Annuller</button>
      </div>
    </div>
  </div>
{/if}

<!-- Advanced Webhook Modal (Announcements) -->
{#if showAdvancedWebhook}
  <div class="modal">
    <div class="modal-content" style="max-width: 600px;">
      <h2>📢 Send Announcement</h2>
      
      <div class="form-group">
        <label for="announcementType">Type:</label>
        <select id="announcementType" bind:value={announcementData.type}>
          {#each availableTemplates as template}
            <option value={template.type}>{template.icon} {template.name}</option>
          {/each}
        </select>
      </div>

      {#if currentTemplate}
        <div style="background: #f8fafc; padding: 1rem; border-radius: 0.5rem; margin: 1rem 0; border: 1px solid #e2e8f0;">
          <p style="margin: 0; font-size: 0.875rem; color: #6b7280;">
            <strong>{currentTemplate.icon} {currentTemplate.name}:</strong> {currentTemplate.description}
          </p>
        </div>
      {/if}
      
      <div class="form-group">
        <label for="announcementTitle">Titel:</label>
        <input id="announcementTitle" type="text" bind:value={announcementData.title} required />
      </div>
      
      <div class="form-group">
        <label for="announcementDescription">Beskrivelse:</label>
        <textarea 
          id="announcementDescription" 
          bind:value={announcementData.description}
          style="width: 100%; padding: 0.75rem; border: 2px solid #e5e7eb; border-radius: 0.5rem; resize: vertical; min-height: 100px;"
          required
        ></textarea>
      </div>

      <!-- Type-specific fields -->
      {#if announcementData.type === 'new_course'}
        <div class="form-group">
          <label for="startDate">Start dato:</label>
          <input id="startDate" type="date" bind:value={announcementData.startDate} />
        </div>
        <div class="form-group">
          <label for="instructor">Instruktør:</label>
          <input id="instructor" type="text" bind:value={announcementData.instructor} />
        </div>
        <div class="form-group">
          <label for="duration">Varighed:</label>
          <input id="duration" type="text" placeholder="f.eks. 8 uger" bind:value={announcementData.duration} />
        </div>
        <div class="form-group">
          <label for="prerequisites">Forudsætninger:</label>
          <input id="prerequisites" type="text" bind:value={announcementData.prerequisites} />
        </div>
        <div class="form-group">
          <label for="maxStudents">Max antal elever:</label>
          <input id="maxStudents" type="number" bind:value={announcementData.maxStudents} />
        </div>
      {:else if announcementData.type === 'event'}
        <div class="form-group">
          <label for="eventDate">Event dato og tid:</label>
          <input id="eventDate" type="datetime-local" bind:value={announcementData.eventDate} />
        </div>
        <div class="form-group">
          <label for="eventLocation">Lokation:</label>
          <input id="eventLocation" type="text" bind:value={announcementData.location} />
        </div>
        <div class="form-group">
          <label for="maxParticipants">Max deltagere:</label>
          <input id="maxParticipants" type="number" bind:value={announcementData.maxParticipants} />
        </div>
        <div class="form-group">
          <label for="organizer">Arrangør:</label>
          <input id="organizer" type="text" bind:value={announcementData.organizer} />
        </div>
        <div class="form-group">
          <label for="price">Pris:</label>
          <input id="price" type="text" placeholder="f.eks. Gratis, 100 kr." bind:value={announcementData.price} />
        </div>
        <div class="form-group">
          <label for="registrationLink">Tilmeldings-link:</label>
          <input id="registrationLink" type="url" bind:value={announcementData.registrationLink} />
        </div>
      {:else if announcementData.type === 'news'}
        <div class="form-group">
          <label for="author">Forfatter:</label>
          <input id="author" type="text" bind:value={announcementData.author} />
        </div>
        <div class="form-group">
          <label for="category">Kategori:</label>
          <input id="category" type="text" bind:value={announcementData.category} />
        </div>
        <div class="form-group">
          <label for="readMoreLink">Læs mere link:</label>
          <input id="readMoreLink" type="url" bind:value={announcementData.readMoreLink} />
        </div>
      {:else if announcementData.type === 'urgent'}
        <div class="form-group">
          <label for="deadline">Deadline:</label>
          <input id="deadline" type="datetime-local" bind:value={announcementData.deadline} />
        </div>
        <div class="form-group">
          <label for="actionRequired">Handling påkrævet:</label>
          <input id="actionRequired" type="text" bind:value={announcementData.actionRequired} />
        </div>
        <div class="form-group">
          <label for="contactInfo">Kontakt info:</label>
          <input id="contactInfo" type="text" bind:value={announcementData.contactInfo} />
        </div>
      {/if}

      <!-- Media fields -->
      <div class="form-group">
        <label for="thumbnailUrl">Thumbnail URL (valgfri):</label>
        <input id="thumbnailUrl" type="url" bind:value={announcementData.thumbnailUrl} />
      </div>
      
      <div class="form-group">
        <label for="imageUrl">Billede URL (valgfri):</label>
        <input id="imageUrl" type="url" bind:value={announcementData.imageUrl} />
      </div>
      
      <div class="modal-actions">
        <button 
          class="btn btn-primary" 
          on:click={sendAnnouncement} 
          disabled={webhookTesting || !announcementData.title.trim() || !announcementData.description.trim()}
        >
          {webhookTesting ? 'Sender...' : '🚀 Send Announcement'}
        </button>
        <button class="btn btn-secondary" on:click={() => showAdvancedWebhook = false}>
          Annuller
        </button>
      </div>
    </div>
  </div>
{/if}

<!-- Company Post Modal -->
{#if showCompanyPost}
  <div class="modal">
    <div class="modal-content" style="max-width: 600px;">
      <h2>🏢 Virksomheds-opslag</h2>
      
      <div class="form-group">
        <label for="companyName">Virksomhedsnavn:</label>
        <input id="companyName" type="text" bind:value={companyPostData.companyName} required />
      </div>
      
      <div class="form-group">
        <label for="jobTitle">Stilling:</label>
        <input id="jobTitle" type="text" bind:value={companyPostData.jobTitle} required />
      </div>
      
      <div class="form-group">
        <label for="jobDescription">Beskrivelse:</label>
        <textarea 
          id="jobDescription" 
          bind:value={companyPostData.description}
          style="width: 100%; padding: 0.75rem; border: 2px solid #e5e7eb; border-radius: 0.5rem; resize: vertical; min-height: 120px;"
          required
        ></textarea>
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
        <div class="form-group">
          <label for="jobLocation">Lokation:</label>
          <input id="jobLocation" type="text" bind:value={companyPostData.location} />
        </div>
        
        <div class="form-group">
          <label for="workType">Arbejdstype:</label>
          <select id="workType" bind:value={companyPostData.workType}>
            <option value="Fuldtid">Fuldtid</option>
            <option value="Deltid">Deltid</option>
            <option value="Praktik">Praktik</option>
            <option value="Lærerplads">Lærerplads</option>
            <option value="Freelance">Freelance</option>
          </select>
        </div>
      </div>

      <div class="form-group">
        <label for="salaryRange">Løn (valgfri):</label>
        <input id="salaryRange" type="text" placeholder="f.eks. 25.000-35.000 kr./måned" bind:value={companyPostData.salaryRange} />
      </div>

      <div class="form-group">
        <label>Krævet færdigheder:</label>
        <div style="display: flex; gap: 0.5rem; margin-bottom: 0.5rem;">
          <input 
            type="text" 
            placeholder="Tilføj færdighed..." 
            bind:value={newSkill}
            on:keypress={(e) => e.key === 'Enter' && addSkill()}
            style="flex: 1; padding: 0.5rem; border: 1px solid #d1d5db; border-radius: 0.25rem;"
          />
          <button 
            type="button" 
            on:click={addSkill}
            style="padding: 0.5rem 1rem; background: #3b82f6; color: white; border: none; border-radius: 0.25rem;"
          >
            Tilføj
          </button>
        </div>
        {#if companyPostData.requiredSkills.length > 0}
          <div style="display: flex; flex-wrap: wrap; gap: 0.5rem;">
            {#each companyPostData.requiredSkills as skill}
              <span style="background: #f3f4f6; padding: 0.25rem 0.5rem; border-radius: 9999px; font-size: 0.875rem; display: flex; align-items: center; gap: 0.25rem;">
                {skill}
                <button 
                  type="button" 
                  on:click={() => removeSkill(skill)}
                  style="background: none; border: none; color: #6b7280; cursor: pointer; padding: 0; margin-left: 0.25rem;"
                >
                  ×
                </button>
              </span>
            {/each}
          </div>
        {/if}
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
        <div class="form-group">
          <label for="applicationDeadline">Ansøgningsfrist:</label>
          <input id="applicationDeadline" type="date" bind:value={companyPostData.applicationDeadline} />
        </div>
        
        <div class="form-group">
          <label for="contactPerson">Kontaktperson:</label>
          <input id="contactPerson" type="text" bind:value={companyPostData.contactPerson} />
        </div>
      </div>

      <div class="form-group">
        <label for="applicationLink">Ansøgnings-link:</label>
        <input id="applicationLink" type="url" bind:value={companyPostData.applicationLink} />
      </div>

      <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem;">
        <div class="form-group">
          <label for="companyLogo">Virksomheds logo URL:</label>
          <input id="companyLogo" type="url" bind:value={companyPostData.companyLogo} />
        </div>
        
        <div class="form-group">
          <label for="jobImageUrl">Billede URL:</label>
          <input id="jobImageUrl" type="url" bind:value={companyPostData.imageUrl} />
        </div>
      </div>
      
      <div class="modal-actions">
        <button 
          class="btn btn-primary" 
          on:click={sendCompanyPost} 
          disabled={webhookTesting || !companyPostData.companyName.trim() || !companyPostData.jobTitle.trim() || !companyPostData.description.trim()}
        >
          {webhookTesting ? 'Sender...' : '🚀 Send Opslag'}
        </button>
        <button class="btn btn-secondary" on:click={() => showCompanyPost = false}>
          Annuller
        </button>
      </div>
    </div>
  </div>
{/if}

<!-- ... existing simple webhook test modal ... -->
