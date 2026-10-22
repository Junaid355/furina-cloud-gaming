// d:\cloud\FurinaHost\wwwroot\app.js
document.addEventListener('DOMContentLoaded', () => {
  // State
  let hostStatus = null;
  let gameLibrary = [];
  let currentSessionToken = localStorage.getItem('furina_token') || '';
  let activeGame = null;
  let streamClient = null;
  let touchController = null;

  // DOM Elements - Navigation & Header
  const navButtons = document.querySelectorAll('.nav-btn');
  const tabPanes = document.querySelectorAll('.tab-pane');
  const connectionBadge = document.getElementById('connectionBadge');
  const connectionText = document.getElementById('connectionText');
  const btnPairModal = document.getElementById('btnPairModal');
  const btnHeaderEmergency = document.getElementById('btnHeaderEmergency');

  // DOM Elements - Home
  const specHostName = document.getElementById('specHostName');
  const specGpu = document.getElementById('specGpu');
  const specCpuRam = document.getElementById('specCpuRam');
  const specDisplay = document.getElementById('specDisplay');
  const specFps = document.getElementById('specFps');
  const specPin = document.getElementById('specPin');
  const hostStatusPill = document.getElementById('hostStatusPill');
  const quickLaunchGrid = document.getElementById('quickLaunchGrid');
  const btnHeroQuickPlay = document.getElementById('btnHeroQuickPlay');
  const btnHeroBrowse = document.getElementById('btnHeroBrowse');

  // DOM Elements - Library
  const gameLibraryGrid = document.getElementById('gameLibraryGrid');
  const gameSearchInput = document.getElementById('gameSearchInput');
  const btnAddCustomGame = document.getElementById('btnAddCustomGame');
  const btnRefreshGames = document.getElementById('btnRefreshGames');

  // DOM Elements - Play Viewport
  const playViewport = document.getElementById('playViewport');
  const streamCanvas = document.getElementById('streamCanvas');
  const touchOverlay = document.getElementById('touchOverlay');
  const hudGameName = document.getElementById('hudGameName');
  const hudFpsBadge = document.getElementById('hudFpsBadge');
  const hudPingBadge = document.getElementById('hudPingBadge');
  const btnHudToggleEditor = document.getElementById('btnHudToggleEditor');
  const btnHudRecenter = document.getElementById('btnHudRecenter');
  const btnHudEmergency = document.getElementById('btnHudEmergency');
  const btnHudDisconnect = document.getElementById('btnHudDisconnect');
  const editorDrawer = document.getElementById('editorDrawer');
  const btnCloseEditor = document.getElementById('btnCloseEditor');

  // DOM Elements - Editor Drawer
  const btnAddButton = document.getElementById('btnAddButton');
  const btnAddJoystick = document.getElementById('btnAddJoystick');
  const btnResetLayout = document.getElementById('btnResetLayout');
  const selectedElementConfig = document.getElementById('selectedElementConfig');
  const lblSelectedId = document.getElementById('lblSelectedId');
  const txtElemLabel = document.getElementById('txtElemLabel');
  const selElemKey = document.getElementById('selElemKey');
  const rangeElemSize = document.getElementById('rangeElemSize');
  const lblElemSize = document.getElementById('lblElemSize');
  const rangeElemOpacity = document.getElementById('rangeElemOpacity');
  const lblElemOpacity = document.getElementById('lblElemOpacity');
  const btnDeleteElement = document.getElementById('btnDeleteElement');
  const btnSaveLayout = document.getElementById('btnSaveLayout');
  const btnExportLayout = document.getElementById('btnExportLayout');
  const btnImportLayout = document.getElementById('btnImportLayout');

  // DOM Elements - Modals
  const pairModal = document.getElementById('pairModal');
  const btnClosePairModal = document.getElementById('btnClosePairModal');
  const txtPinInput = document.getElementById('txtPinInput');
  const txtDeviceName = document.getElementById('txtDeviceName');
  const btnSubmitPair = document.getElementById('btnSubmitPair');
  const pairError = document.getElementById('pairError');

  const addGameModal = document.getElementById('addGameModal');
  const btnCloseAddGame = document.getElementById('btnCloseAddGame');
  const txtNewGameTitle = document.getElementById('txtNewGameTitle');
  const txtNewGamePath = document.getElementById('txtNewGamePath');
  const selNewGameTemplate = document.getElementById('selNewGameTemplate');
  const btnSubmitAddGame = document.getElementById('btnSubmitAddGame');

  // DOM Elements - Settings
  const selResolution = document.getElementById('selResolution');
  const selFps = document.getElementById('selFps');
  const rangeQuality = document.getElementById('rangeQuality');
  const lblQuality = document.getElementById('lblQuality');
  const selFit = document.getElementById('selFit');
  const chkHaptics = document.getElementById('chkHaptics');
  const chkDynamicJoy = document.getElementById('chkDynamicJoy');
  const rangeGlobalOpacity = document.getElementById('rangeGlobalOpacity');
  const lblGlobalOpacity = document.getElementById('lblGlobalOpacity');
  const rangeLookSens = document.getElementById('rangeLookSens');
  const lblLookSens = document.getElementById('lblLookSens');
  const txtHostIp = document.getElementById('txtHostIp');
  const txtHostPort = document.getElementById('txtHostPort');
  const btnSaveHostSettings = document.getElementById('btnSaveHostSettings');
  const btnRegeneratePin = document.getElementById('btnRegeneratePin');

  // ==============================================================
  // INITIALIZATION
  // ==============================================================
  init();

  async function init() {
    setupTabNavigation();
    setupModals();
    setupSettings();
    await fetchHostStatus();
    await fetchGameLibrary();

    // Init Stream Client & Touch Controller
    streamClient = new StreamClient(streamCanvas);
    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const host = window.location.hostname || 'localhost';
    const port = window.location.port || '8080';
    const inputWsUrl = `${protocol}//${host}:${port}/ws/input`;

    touchController = new TouchController(touchOverlay, inputWsUrl);

    // Wire Stats to HUD
    streamClient.onStatsUpdate = (stats) => {
      hudFpsBadge.textContent = `${stats.fps} FPS`;
      if (stats.latency > 0) {
        hudPingBadge.textContent = `${stats.latency} ms`;
      }
    };

    touchController.onPingUpdate = (rtt) => {
      hudPingBadge.textContent = `${rtt} ms`;
    };

    touchController.onSelectElement = (elem) => {
      showSelectedElementConfig(elem);
    };

    // Auto poll status every 4 seconds
    setInterval(fetchHostStatus, 4000);
  }

  // ==============================================================
  // TAB NAVIGATION
  // ==============================================================
  function setupTabNavigation() {
    navButtons.forEach(btn => {
      btn.addEventListener('click', () => {
        const targetId = btn.dataset.tab;
        navButtons.forEach(b => b.classList.remove('active'));
        tabPanes.forEach(p => p.classList.remove('active'));
        btn.classList.add('active');
        document.getElementById(targetId)?.classList.add('active');
      });
    });

    btnHeroBrowse?.addEventListener('click', () => {
      document.querySelector('[data-tab="tab-library"]')?.click();
    });

    btnHeroQuickPlay?.addEventListener('click', () => {
      if (gameLibrary.length > 0) {
        launchAndPlay(gameLibrary[0]);
      } else {
        launchAndPlay({ id: 'desktop', name: 'Windows Desktop', controls: [] });
      }
    });

    btnHeaderEmergency?.addEventListener('click', triggerEmergencyStop);
    btnHudEmergency?.addEventListener('click', triggerEmergencyStop);
  }

  async function triggerEmergencyStop() {
    try {
      await fetch('/api/emergency_stop', { method: 'POST' });
      touchController?.emergencyRelease();
      alert('⚠️ Emergency Stop: All PC keyboard and mouse inputs released.');
    } catch (e) {
      console.warn('Emergency stop request failed:', e);
    }
  }

  // ==============================================================
  // HOST STATUS & HARDWARE
  // ==============================================================
  async function fetchHostStatus() {
    try {
      const res = await fetch('/api/status');
      if (!res.ok) throw new Error('Host unreachable');
      hostStatus = await res.json();

      // Update Header Badge
      const hostname = hostStatus.Hostname || hostStatus.hostname || 'Host';
      const fps = hostStatus.Fps ?? hostStatus.fps ?? 0;
      const currentPin = hostStatus.CurrentPin || hostStatus.currentPin || '----';
      const localIps = hostStatus.LocalIps || hostStatus.localIps || [];

      connectionBadge.className = 'status-badge connected';
      connectionText.textContent = `Connected (${hostname})`;
      hostStatusPill.className = 'pill pill-online';
      hostStatusPill.textContent = 'Online';

      // Update Spec Fields
      specHostName.textContent = hostname;
      specFps.textContent = `${fps} FPS`;
      specPin.textContent = currentPin;

      // Populate Host IP in settings if empty
      if (!txtHostIp.value && localIps.length > 0) {
        txtHostIp.value = localIps[0];
      }
    } catch (e) {
      connectionBadge.className = 'status-badge disconnected';
      connectionText.textContent = 'Disconnected';
      hostStatusPill.className = 'pill pill-offline';
      hostStatusPill.textContent = 'Offline';
    }

    // Fetch Hardware details
    try {
      const hwRes = await fetch('/api/hardware');
      if (hwRes.ok) {
        const hw = await hwRes.json();
        const gpus = hw.Gpus || hw.gpus || [];
        const displays = hw.Displays || hw.displays || [];
        const cpuName = hw.CpuName || hw.cpuName || 'CPU';
        const totalRamGb = hw.TotalRamGb ?? hw.totalRamGb ?? 0;

        if (gpus.length > 0) {
          specGpu.textContent = `${gpus[0].Name || gpus[0].name || ''}`;
        }
        specCpuRam.textContent = `${cpuName} (${totalRamGb} GB)`;
        if (displays.length > 0) {
          const d = displays[0];
          const w = d.Width ?? d.width ?? 1920;
          const h = d.Height ?? d.height ?? 1080;
          const r = d.RefreshRate ?? d.refreshRate ?? 60;
          specDisplay.textContent = `${w}x${h} @ ${r}Hz`;
        }
      }
    } catch (e) {}
  }

  // ==============================================================
  // GAME LIBRARY & RENDERING
  // ==============================================================
  async function fetchGameLibrary() {
    try {
      const res = await fetch('/api/games');
      if (!res.ok) return;
      gameLibrary = await res.json();
      renderGameLibrary(gameLibrary);
      renderQuickLaunch(gameLibrary);
    } catch (e) {
      console.error('Failed to load games:', e);
    }
  }

  function renderGameLibrary(games) {
    gameLibraryGrid.innerHTML = '';
    const query = (gameSearchInput.value || '').toLowerCase();
    const filtered = games.filter(g => {
      const name = (g.Name || g.name || '').toLowerCase();
      const cat = (g.Category || g.category || '').toLowerCase();
      return name.includes(query) || cat.includes(query);
    });

    filtered.forEach(game => {
      const card = document.createElement('div');
      card.className = 'game-card';

      const isInstalled = game.IsInstalled ?? game.isInstalled ?? false;
      const gameId = game.Id || game.id || '';
      const gameName = game.Name || game.name || 'Game';
      const gameCategory = game.Category || game.category || '';
      const gameNotes = game.VerificationNotes || game.verificationNotes || 'Custom touch overlay configured for low latency.';
      const iconEmoji = getEmojiForGame(gameId);

      card.innerHTML = `
        <div class="game-card-banner">
          ${isInstalled ? '<span class="installed-badge">Installed</span>' : ''}
          <div class="game-card-icon">${iconEmoji}</div>
        </div>
        <div class="game-card-body">
          <div class="game-card-title">${gameName}</div>
          <div class="game-card-category">${gameCategory}</div>
          <div class="game-card-notes">${gameNotes}</div>
          <div class="game-card-footer">
            <button class="btn btn-primary btn-sm btn-play-game" data-id="${gameId}">▶ Play Now</button>
            <button class="btn btn-outline btn-sm btn-edit-game" data-id="${gameId}">✏️ Controls</button>
            ${isInstalled ? `<button class="btn btn-danger btn-sm btn-stop-game" data-id="${gameId}">⏹ Stop</button>` : ''}
          </div>
        </div>
      `;

      card.querySelector('.btn-play-game').addEventListener('click', () => launchAndPlay(game));
      card.querySelector('.btn-edit-game').addEventListener('click', () => openEditorForGame(game));
      card.querySelector('.btn-stop-game')?.addEventListener('click', () => stopGameProcess(game));

      gameLibraryGrid.appendChild(card);
    });
  }

  function renderQuickLaunch(games) {
    quickLaunchGrid.innerHTML = '';
    const topGames = games.slice(0, 4);

    topGames.forEach(game => {
      const card = document.createElement('div');
      card.className = 'game-card';
      const gameId = game.Id || game.id || '';
      const gameName = game.Name || game.name || 'Game';
      const gameCategory = game.Category || game.category || '';
      const iconEmoji = getEmojiForGame(gameId);

      card.innerHTML = `
        <div class="game-card-banner">
          <div class="game-card-icon">${iconEmoji}</div>
        </div>
        <div class="game-card-body">
          <div class="game-card-title">${gameName}</div>
          <div class="game-card-category">${gameCategory}</div>
          <button class="btn btn-primary btn-sm btn-block btn-quick-play" style="margin-top:10px;">⚡ Stream</button>
        </div>
      `;

      card.querySelector('.btn-quick-play').addEventListener('click', () => launchAndPlay(game));
      quickLaunchGrid.appendChild(card);
    });
  }

  function getEmojiForGame(id) {
    switch (id) {
      case 'roblox': return '🧱';
      case 'gtav': return '🚗';
      case 'genshin': return '⚔️';
      case 'wuwa': return '🌊';
      case 'minecraft_java':
      case 'minecraft_bedrock': return '⛏️';
      case 'hsr': return '🚂';
      case 'zzz': return '⚡';
      default: return '🎮';
    }
  }

  gameSearchInput?.addEventListener('input', () => renderGameLibrary(gameLibrary));
  btnRefreshGames?.addEventListener('click', fetchGameLibrary);

  // ==============================================================
  // STREAM VIEWPORT & PLAY WORKFLOW
  // ==============================================================
  async function launchAndPlay(game) {
    activeGame = game;
    hudGameName.textContent = game.Name;

    // Trigger PC game launch if configured
    if (game.Id !== 'desktop' && game.ExecutablePath) {
      try {
        await fetch(`/api/games/${game.Id}/launch`, { method: 'POST' });
      } catch (e) {
        console.warn('Game launch command error:', e);
      }
    }

    // Switch Viewport
    playViewport.classList.remove('hidden');
    requestFullscreen();

    // Start Streaming
    streamClient.connect(txtHostIp.value, txtHostPort.value);

    // Load Controls
    touchController.loadProfile(game);
  }

  async function stopGameProcess(game) {
    try {
      const res = await fetch(`/api/games/${game.Id}/stop`, { method: 'POST' });
      const data = await res.json();
      alert(data.Message || 'Game stopped.');
    } catch (e) {
      alert(`Error stopping game: ${e.message}`);
    }
  }

  function exitPlayMode() {
    streamClient.disconnect();
    touchController.setEditMode(false);
    editorDrawer.classList.add('hidden');
    playViewport.classList.add('hidden');
    exitFullscreen();
    triggerEmergencyStop();
  }

  btnHudDisconnect?.addEventListener('click', exitPlayMode);
  btnHudRecenter?.addEventListener('click', () => {
    touchController.emergencyRelease();
  });

  // Fullscreen Helpers (Safe for iOS & Android)
  function requestFullscreen() {
    const el = document.documentElement;
    if (el.requestFullscreen) {
      el.requestFullscreen().catch(() => {});
    } else if (el.webkitRequestFullscreen) {
      el.webkitRequestFullscreen();
    }
  }

  function exitFullscreen() {
    if (document.exitFullscreen) {
      document.exitFullscreen().catch(() => {});
    } else if (document.webkitExitFullscreen) {
      document.webkitExitFullscreen();
    }
  }

  // ==============================================================
  // CONTROLLER EDITOR
  // ==============================================================
  function openEditorForGame(game) {
    launchAndPlay(game);
    setTimeout(() => {
      toggleEditorDrawer(true);
    }, 400);
  }

  btnHudToggleEditor?.addEventListener('click', () => {
    toggleEditorDrawer(!touchController.isEditMode);
  });

  btnCloseEditor?.addEventListener('click', () => {
    toggleEditorDrawer(false);
  });

  function toggleEditorDrawer(open) {
    touchController.setEditMode(open);
    if (open) {
      editorDrawer.classList.remove('hidden');
    } else {
      editorDrawer.classList.add('hidden');
    }
  }

  function showSelectedElementConfig(elem) {
    selectedElementConfig.classList.remove('hidden');
    lblSelectedId.textContent = `${elem.type.toUpperCase()}: ${elem.label || elem.id}`;
    txtElemLabel.value = elem.label || '';
    selElemKey.value = elem.primaryKey || 'WASD';
    rangeElemSize.value = elem.width ? (elem.width / 0.09).toFixed(1) : '1.0';
    lblElemSize.textContent = `${rangeElemSize.value}x`;
    rangeElemOpacity.value = Math.round((elem.opacity || 0.8) * 100);
    lblElemOpacity.textContent = `${rangeElemOpacity.value}%`;

    // Bind change listeners
    txtElemLabel.oninput = () => {
      elem.label = txtElemLabel.value;
      touchController.render();
    };

    selElemKey.onchange = () => {
      elem.primaryKey = selElemKey.value;
    };

    rangeElemSize.oninput = () => {
      const factor = parseFloat(rangeElemSize.value);
      lblElemSize.textContent = `${factor.toFixed(1)}x`;
      elem.width = (elem.type === 'joystick' ? 0.18 : 0.09) * factor;
      elem.height = elem.width;
      touchController.render();
    };

    rangeElemOpacity.oninput = () => {
      elem.opacity = parseInt(rangeElemOpacity.value) / 100;
      lblElemOpacity.textContent = `${rangeElemOpacity.value}%`;
      touchController.render();
    };

    btnDeleteElement.onclick = () => {
      touchController.elements = touchController.elements.filter(x => x.id !== elem.id);
      selectedElementConfig.classList.add('hidden');
      touchController.render();
    };
  }

  btnAddButton?.addEventListener('click', () => {
    const newBtn = {
      id: `btn_${Date.now()}`,
      type: 'button',
      label: 'ACTION',
      subLabel: 'Key',
      x: 0.5,
      y: 0.5,
      width: 0.09,
      height: 0.09,
      opacity: 0.85,
      primaryKey: 'Space',
      colorScheme: 'cyan'
    };
    touchController.elements.push(newBtn);
    touchController.render();
    showSelectedElementConfig(newBtn);
  });

  btnAddJoystick?.addEventListener('click', () => {
    const newJoy = {
      id: `joy_${Date.now()}`,
      type: 'joystick',
      label: 'MOVE',
      x: 0.2,
      y: 0.6,
      width: 0.18,
      height: 0.18,
      opacity: 0.85,
      primaryKey: 'WASD',
      colorScheme: 'cyan'
    };
    touchController.elements.push(newJoy);
    touchController.render();
    showSelectedElementConfig(newJoy);
  });

  btnResetLayout?.addEventListener('click', () => {
    if (confirm('Reset touch controls to original template?')) {
      touchController.resetCurrentLayout();
      selectedElementConfig.classList.add('hidden');
    }
  });

  btnSaveLayout?.addEventListener('click', () => {
    touchController.saveCurrentLayout();
    alert('Layout saved successfully for this game!');
    toggleEditorDrawer(false);
  });

  btnExportLayout?.addEventListener('click', () => {
    const data = JSON.stringify(touchController.elements, null, 2);
    const blob = new Blob([data], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = `furina_${activeGame?.id || 'controls'}.json`;
    a.click();
    URL.revokeObjectURL(url);
  });

  btnImportLayout?.addEventListener('click', () => {
    const input = document.createElement('input');
    input.type = 'file';
    input.accept = '.json';
    input.onchange = (e) => {
      const file = e.target.files[0];
      if (file) {
        const reader = new FileReader();
        reader.onload = (evt) => {
          try {
            touchController.elements = JSON.parse(evt.target.result);
            touchController.render();
            touchController.saveCurrentLayout();
            alert('Custom layout imported successfully.');
          } catch (err) {
            alert('Invalid JSON file format.');
          }
        };
        reader.readAsText(file);
      }
    };
    input.click();
  });

  // ==============================================================
  // PAIRING MODAL
  // ==============================================================
  function setupModals() {
    btnPairModal?.addEventListener('click', () => {
      pairModal.classList.remove('hidden');
      txtPinInput.value = '';
      const cur = hostStatus?.CurrentPin || hostStatus?.currentPin || '';
      if (cur) {
        txtPinInput.placeholder = cur;
      }
      pairError.classList.add('hidden');
      txtPinInput.focus();
    });

    btnClosePairModal?.addEventListener('click', () => pairModal.classList.add('hidden'));

    btnSubmitPair?.addEventListener('click', async () => {
      let pin = txtPinInput.value.trim();
      if (!pin && txtPinInput.placeholder && txtPinInput.placeholder !== '••••') {
        pin = txtPinInput.placeholder;
      }
      const deviceName = txtDeviceName.value.trim() || navigator.userAgent;

      if (!pin) {
        showPairError('Please enter the 4-digit PIN.');
        return;
      }

      try {
        const res = await fetch('/api/pair', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ pin, deviceName, deviceType: 'Mobile' })
        });
        const data = await res.json();
        const success = data.Success ?? data.success ?? false;
        const token = data.Token ?? data.token ?? '';
        const msg = data.Message ?? data.message ?? 'Pairing failed.';
        if (success && token) {
          localStorage.setItem('furina_token', token);
          currentSessionToken = token;
          pairModal.classList.add('hidden');
          alert('Device successfully paired with host!');
          fetchHostStatus();
        } else {
          showPairError(msg);
        }
      } catch (err) {
        showPairError(`Connection error: ${err.message}`);
      }
    });

    function showPairError(msg) {
      pairError.textContent = msg;
      pairError.classList.remove('hidden');
    }

    // Add Custom Game Modal
    btnAddCustomGame?.addEventListener('click', () => {
      addGameModal.classList.remove('hidden');
    });

    btnCloseAddGame?.addEventListener('click', () => addGameModal.classList.add('hidden'));

    btnSubmitAddGame?.addEventListener('click', async () => {
      const title = txtNewGameTitle.value.trim();
      const path = txtNewGamePath.value.trim();
      const template = selNewGameTemplate.value;

      if (!title) {
        alert('Please enter a game title.');
        return;
      }

      const newGame = {
        Id: `custom_${Date.now()}`,
        Name: title,
        ExecutablePath: path,
        Category: 'Custom Game',
        VerificationNotes: 'User added custom Windows game.',
        Controls: []
      };

      try {
        const res = await fetch('/api/games', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify(newGame)
        });
        if (res.ok) {
          addGameModal.classList.add('hidden');
          await fetchGameLibrary();
          alert(`Added ${title} to game library.`);
        }
      } catch (e) {
        alert(`Failed to add game: ${e.message}`);
      }
    });
  }

  // ==============================================================
  // SETTINGS HANDLERS
  // ==============================================================
  function setupSettings() {
    rangeQuality?.addEventListener('input', () => {
      lblQuality.textContent = `${rangeQuality.value}%`;
      updateStreamConfig();
    });

    selResolution?.addEventListener('change', updateStreamConfig);
    selFps?.addEventListener('change', updateStreamConfig);

    async function updateStreamConfig() {
      const [w, h] = selResolution.value.split('x').map(Number);
      const fps = parseInt(selFps.value);
      const quality = parseInt(rangeQuality.value);

      try {
        await fetch('/api/stream/config', {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            TargetWidth: w,
            TargetHeight: h,
            TargetFps: fps,
            Quality: quality
          })
        });
      } catch (e) {}
    }

    selFit?.addEventListener('change', () => {
      streamCanvas.style.objectFit = selFit.value;
    });

    chkHaptics?.addEventListener('change', () => {
      localStorage.setItem('furina_haptics', chkHaptics.checked ? 'true' : 'false');
      if (touchController) touchController.enableHaptics = chkHaptics.checked;
    });

    rangeGlobalOpacity?.addEventListener('input', () => {
      lblGlobalOpacity.textContent = `${rangeGlobalOpacity.value}%`;
      const val = parseInt(rangeGlobalOpacity.value) / 100;
      localStorage.setItem('furina_global_opacity', val.toString());
      if (touchController) {
        touchController.globalOpacity = val;
        touchController.render();
      }
    });

    rangeLookSens?.addEventListener('input', () => {
      lblLookSens.textContent = `${parseFloat(rangeLookSens.value).toFixed(1)}x`;
      localStorage.setItem('furina_look_sens', rangeLookSens.value);
      if (touchController) touchController.lookSensitivity = parseFloat(rangeLookSens.value);
    });

    btnRegeneratePin?.addEventListener('click', async () => {
      try {
        const res = await fetch('/api/pin/regenerate', { method: 'POST' });
        const data = await res.json();
        const pin = data.Pin || data.pin || '';
        specPin.textContent = pin;
        alert(`New Host PIN: ${pin}`);
      } catch (e) {}
    });

    btnSaveHostSettings?.addEventListener('click', () => {
      fetchHostStatus();
      alert('Host connection settings updated.');
    });
  }
});
