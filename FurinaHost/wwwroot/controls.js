// d:\cloud\FurinaHost\wwwroot\controls.js
class TouchController {
  constructor(overlayContainer, inputWsUrl) {
    this.container = overlayContainer;
    this.inputWsUrl = inputWsUrl;
    this.ws = null;
    this.isConnected = false;
    this.token = localStorage.getItem('furina_token') || '';
    
    // Profiles and Elements
    this.activeProfile = null;
    this.elements = [];
    this.isEditMode = false;
    this.selectedElement = null;
    this.onSelectElement = null;
    
    // Joystick tracking
    this.joysticks = new Map(); // touchId -> state
    this.cameraTouches = new Map(); // touchId -> lastX, lastY
    
    // Settings
    this.enableHaptics = localStorage.getItem('furina_haptics') !== 'false';
    this.lookSensitivity = parseFloat(localStorage.getItem('furina_look_sens') || '1.0');
    this.globalOpacity = parseFloat(localStorage.getItem('furina_global_opacity') || '0.85');
    
    // Heartbeat & ping
    this.pingInterval = null;
    this.rttLatency = 0;
    this.onPingUpdate = null;
    
    this.initWebSocket();
  }

  initWebSocket() {
    if (this.ws && (this.ws.readyState === WebSocket.OPEN || this.ws.readyState === WebSocket.CONNECTING)) {
      return;
    }

    try {
      this.ws = new WebSocket(`${this.inputWsUrl}?token=${encodeURIComponent(this.token)}`);
      
      this.ws.onopen = () => {
        this.isConnected = true;
        console.log('Input WebSocket connected.');
        this.startHeartbeat();
      };

      this.ws.onmessage = (event) => {
        try {
          const msg = JSON.parse(event.data);
          if (msg.Type === 'pong' && msg.ClientTimestamp) {
            this.rttLatency = Date.now() - msg.ClientTimestamp;
            if (this.onPingUpdate) this.onPingUpdate(this.rttLatency);
          }
        } catch (e) {}
      };

      this.ws.onclose = () => {
        this.isConnected = false;
        this.stopHeartbeat();
        console.log('Input WebSocket closed.');
      };

      this.ws.onerror = (err) => {
        console.error('Input WebSocket error:', err);
      };
    } catch (e) {
      console.warn('Failed to initialize input WebSocket:', e);
    }
  }

  startHeartbeat() {
    this.stopHeartbeat();
    this.pingInterval = setInterval(() => {
      this.send({
        type: 'heartbeat',
        clientTimestamp: Date.now()
      });
    }, 1000);
  }

  stopHeartbeat() {
    if (this.pingInterval) {
      clearInterval(this.pingInterval);
      this.pingInterval = null;
    }
  }

  send(msg) {
    if (this.ws && this.ws.readyState === WebSocket.OPEN) {
      msg.sessionToken = this.token;
      this.ws.send(JSON.stringify(msg));
    }
  }

  loadProfile(profile) {
    this.activeProfile = profile;
    // Check if custom layout saved in localStorage
    const saved = localStorage.getItem(`furina_layout_${profile.id}`);
    if (saved) {
      try {
        this.elements = JSON.parse(saved);
      } catch (e) {
        this.elements = JSON.parse(JSON.stringify(profile.controls || []));
      }
    } else {
      this.elements = JSON.parse(JSON.stringify(profile.controls || []));
    }
    this.render();
  }

  render() {
    this.container.innerHTML = '';
    const rect = this.container.getBoundingClientRect();
    const width = rect.width || window.innerWidth;
    const height = rect.height || window.innerHeight;

    this.elements.forEach(elem => {
      const el = this.createControlElement(elem, width, height);
      this.container.appendChild(el);
    });
  }

  createControlElement(elem, screenW, screenH) {
    const el = document.createElement('div');
    el.id = `elem_${elem.id}`;
    el.dataset.id = elem.id;
    el.style.opacity = elem.opacity || this.globalOpacity;

    // Normalized coordinates
    const posX = elem.x * screenW;
    const posY = elem.y * screenH;
    const sizeBase = Math.min(screenW, screenH);

    if (elem.type === 'joystick') {
      const radius = (elem.width || 0.18) * sizeBase;
      el.className = `touch-joystick-base ${elem.colorScheme || 'cyan'}`;
      el.style.width = `${radius}px`;
      el.style.height = `${radius}px`;
      el.style.left = `${posX}px`;
      el.style.top = `${posY}px`;

      const knob = document.createElement('div');
      knob.className = 'touch-joystick-knob';
      knob.id = `knob_${elem.id}`;
      el.appendChild(knob);

      this.bindJoystickEvents(el, knob, elem);
    } 
    else if (elem.type === 'camera_region') {
      el.className = 'touch-camera-region';
      el.style.left = `${elem.x * 100}%`;
      el.style.top = `${elem.y * 100}%`;
      el.style.width = `${elem.width * 100}%`;
      el.style.height = `${elem.height * 100}%`;
      el.innerHTML = `<span>${elem.label || 'CAMERA SWIPE'}</span>`;

      this.bindCameraEvents(el, elem);
    } 
    else {
      // Action Button
      const btnSize = (elem.width || 0.09) * sizeBase;
      el.className = `touch-button ${elem.colorScheme || 'cyan'}`;
      el.style.width = `${btnSize}px`;
      el.style.height = `${btnSize}px`;
      el.style.left = `${posX}px`;
      el.style.top = `${posY}px`;

      el.innerHTML = `
        <span class="btn-label">${elem.label || 'BTN'}</span>
        ${elem.subLabel ? `<span class="btn-sublabel">${elem.subLabel}</span>` : ''}
      `;

      this.bindButtonEvents(el, elem);
    }

    // Edit Mode Bindings
    if (this.isEditMode) {
      this.bindEditDragging(el, elem, screenW, screenH);
    }

    return el;
  }

  // ==============================================================
  // JOYSTICK HANDLING
  // ==============================================================
  bindJoystickEvents(base, knob, elem) {
    let activeTouchId = null;
    let baseRect = null;

    const onStart = (e) => {
      if (this.isEditMode) return;
      e.preventDefault();
      const touch = e.changedTouches ? e.changedTouches[0] : e;
      activeTouchId = touch.identifier ?? 'mouse';
      baseRect = base.getBoundingClientRect();
      this.triggerHaptic();
      onMove(e);
    };

    const onMove = (e) => {
      if (this.isEditMode || activeTouchId === null) return;
      e.preventDefault();

      let touch = null;
      if (e.changedTouches) {
        for (let i = 0; i < e.changedTouches.length; i++) {
          if (e.changedTouches[i].identifier === activeTouchId) {
            touch = e.changedTouches[i];
            break;
          }
        }
      } else {
        touch = e;
      }
      if (!touch || !baseRect) return;

      const centerX = baseRect.left + baseRect.width / 2;
      const centerY = baseRect.top + baseRect.height / 2;
      const maxRadius = baseRect.width / 2;

      let dx = touch.clientX - centerX;
      let dy = touch.clientY - centerY;
      const distance = Math.hypot(dx, dy);

      if (distance > maxRadius) {
        dx = (dx / distance) * maxRadius;
        dy = (dy / distance) * maxRadius;
      }

      // Visual knob translation
      knob.style.transform = `translate(${dx}px, ${dy}px)`;

      // Normalized vector (-1.0 to 1.0)
      const normX = dx / maxRadius;
      const normY = dy / maxRadius;

      this.send({
        type: 'joystick_vector',
        deltaX: normX,
        deltaY: normY
      });
    };

    const onEnd = (e) => {
      if (activeTouchId === null) return;
      let matched = false;
      if (e.changedTouches) {
        for (let i = 0; i < e.changedTouches.length; i++) {
          if (e.changedTouches[i].identifier === activeTouchId) {
            matched = true;
            break;
          }
        }
      } else {
        matched = true;
      }

      if (matched) {
        activeTouchId = null;
        knob.style.transform = 'translate(0px, 0px)';
        this.send({
          type: 'joystick_vector',
          deltaX: 0,
          deltaY: 0
        });
      }
    };

    base.addEventListener('touchstart', onStart, { passive: false });
    window.addEventListener('touchmove', onMove, { passive: false });
    window.addEventListener('touchend', onEnd, { passive: false });
    window.addEventListener('touchcancel', onEnd, { passive: false });

    base.addEventListener('mousedown', onStart);
    window.addEventListener('mousemove', onMove);
    window.addEventListener('mouseup', onEnd);
  }

  // ==============================================================
  // CAMERA SWIPE HANDLING
  // ==============================================================
  bindCameraEvents(region, elem) {
    const activeTouches = new Map();

    const onStart = (e) => {
      if (this.isEditMode) return;
      e.preventDefault();
      const touches = e.changedTouches ? Array.from(e.changedTouches) : [e];
      touches.forEach(t => {
        const id = t.identifier ?? 'mouse';
        activeTouches.set(id, { x: t.clientX, y: t.clientY });
      });
      region.classList.add('active');
    };

    const onMove = (e) => {
      if (this.isEditMode || activeTouches.size === 0) return;
      e.preventDefault();
      const touches = e.changedTouches ? Array.from(e.changedTouches) : [e];

      let totalDx = 0;
      let totalDy = 0;

      touches.forEach(t => {
        const id = t.identifier ?? 'mouse';
        const last = activeTouches.get(id);
        if (last) {
          const dx = (t.clientX - last.x) * this.lookSensitivity * (elem.sensitivity || 1.0);
          const dy = (t.clientY - last.y) * this.lookSensitivity * (elem.sensitivity || 1.0);
          totalDx += dx;
          totalDy += dy;
          activeTouches.set(id, { x: t.clientX, y: t.clientY });
        }
      });

      if (totalDx !== 0 || totalDy !== 0) {
        this.send({
          type: 'mouse_delta',
          deltaX: totalDx,
          deltaY: totalDy
        });
      }
    };

    const onEnd = (e) => {
      const touches = e.changedTouches ? Array.from(e.changedTouches) : [e];
      touches.forEach(t => {
        const id = t.identifier ?? 'mouse';
        activeTouches.delete(id);
      });
      if (activeTouches.size === 0) {
        region.classList.remove('active');
      }
    };

    region.addEventListener('touchstart', onStart, { passive: false });
    window.addEventListener('touchmove', onMove, { passive: false });
    window.addEventListener('touchend', onEnd, { passive: false });
    window.addEventListener('touchcancel', onEnd, { passive: false });

    region.addEventListener('mousedown', onStart);
    window.addEventListener('mousemove', onMove);
    window.addEventListener('mouseup', onEnd);
  }

  // ==============================================================
  // BUTTON HANDLING
  // ==============================================================
  bindButtonEvents(btn, elem) {
    let isPressed = false;
    let repeatTimer = null;

    const press = (e) => {
      if (this.isEditMode) return;
      e.preventDefault();
      if (isPressed) return;
      isPressed = true;
      btn.classList.add('pressed');
      this.triggerHaptic();

      if (elem.actionMode === 'toggle') {
        const active = btn.classList.toggle('toggled');
        this.send({
          type: active ? 'key_down' : 'key_up',
          key: elem.primaryKey
        });
        return;
      }

      this.send({
        type: 'key_down',
        key: elem.primaryKey
      });

      if (elem.actionMode === 'repeat') {
        repeatTimer = setInterval(() => {
          this.send({ type: 'key_up', key: elem.primaryKey });
          setTimeout(() => {
            if (isPressed) {
              this.send({ type: 'key_down', key: elem.primaryKey });
            }
          }, 30);
        }, elem.repeatIntervalMs || 100);
      }
    };

    const release = (e) => {
      if (this.isEditMode) return;
      if (!isPressed) return;
      isPressed = false;
      btn.classList.remove('pressed');

      if (repeatTimer) {
        clearInterval(repeatTimer);
        repeatTimer = null;
      }

      if (elem.actionMode !== 'toggle') {
        this.send({
          type: 'key_up',
          key: elem.primaryKey
        });
      }
    };

    btn.addEventListener('touchstart', press, { passive: false });
    btn.addEventListener('touchend', release, { passive: false });
    btn.addEventListener('touchcancel', release, { passive: false });

    btn.addEventListener('mousedown', press);
    btn.addEventListener('mouseup', release);
    btn.addEventListener('mouseleave', release);
  }

  // ==============================================================
  // VISUAL EDITOR DRAGGING & SELECTION
  // ==============================================================
  bindEditDragging(el, elem, screenW, screenH) {
    let startX, startY;
    let elemStartX, elemStartY;
    let isDragging = false;

    const onStart = (e) => {
      if (!this.isEditMode) return;
      e.preventDefault();
      e.stopPropagation();

      const point = e.touches ? e.touches[0] : e;
      startX = point.clientX;
      startY = point.clientY;
      elemStartX = elem.x;
      elemStartY = elem.y;
      isDragging = true;

      // Mark selected
      document.querySelectorAll('.touch-control-selected').forEach(x => x.classList.remove('touch-control-selected'));
      el.classList.add('touch-control-selected');
      this.selectedElement = elem;
      if (this.onSelectElement) this.onSelectElement(elem);
    };

    const onMove = (e) => {
      if (!isDragging || !this.isEditMode) return;
      e.preventDefault();
      const point = e.touches ? e.touches[0] : e;
      const dx = (point.clientX - startX) / screenW;
      const dy = (point.clientY - startY) / screenH;

      elem.x = Math.max(0.02, Math.min(0.98, elemStartX + dx));
      elem.y = Math.max(0.05, Math.min(0.95, elemStartY + dy));

      el.style.left = `${elem.x * screenW}px`;
      el.style.top = `${elem.y * screenH}px`;
    };

    const onEnd = () => {
      isDragging = false;
    };

    el.addEventListener('touchstart', onStart, { passive: false });
    window.addEventListener('touchmove', onMove, { passive: false });
    window.addEventListener('touchend', onEnd);

    el.addEventListener('mousedown', onStart);
    window.addEventListener('mousemove', onMove);
    window.addEventListener('mouseup', onEnd);
  }

  setEditMode(enable) {
    this.isEditMode = enable;
    if (enable) {
      this.container.classList.add('edit-mode');
    } else {
      this.container.classList.remove('edit-mode');
      this.selectedElement = null;
      document.querySelectorAll('.touch-control-selected').forEach(x => x.classList.remove('touch-control-selected'));
    }
    this.render();
  }

  saveCurrentLayout() {
    if (this.activeProfile) {
      const key = `furina_layout_${this.activeProfile.id}`;
      localStorage.setItem(key, JSON.stringify(this.elements));
      console.log(`Saved custom layout for ${this.activeProfile.id}`);
    }
  }

  resetCurrentLayout() {
    if (this.activeProfile) {
      localStorage.removeItem(`furina_layout_${this.activeProfile.id}`);
      this.elements = JSON.parse(JSON.stringify(this.activeProfile.controls || []));
      this.render();
    }
  }

  triggerHaptic() {
    if (this.enableHaptics && navigator.vibrate) {
      navigator.vibrate(12);
    }
  }

  emergencyRelease() {
    this.send({ type: 'emergency_stop' });
  }
}
