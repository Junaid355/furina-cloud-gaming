// d:\cloud\FurinaHost\wwwroot\stream.js
class StreamClient {
  constructor(canvas) {
    this.canvas = canvas;
    this.ctx = canvas.getContext('2d', { alpha: false });
    this.ws = null;
    this.isConnected = false;
    this.token = localStorage.getItem('furina_token') || '';
    
    // Performance & Stats
    this.fps = 0;
    this.frameCount = 0;
    this.lastFpsUpdate = performance.now();
    this.latencyMs = 0;
    this.bytesReceived = 0;
    this.lastBytesCheck = performance.now();
    this.bitrateKbps = 0;
    
    // Handlers
    this.onStatsUpdate = null;
    this.onConnectionChange = null;
  }

  connect(hostIp, hostPort) {
    if (this.ws && (this.ws.readyState === WebSocket.OPEN || this.ws.readyState === WebSocket.CONNECTING)) {
      return;
    }

    const protocol = window.location.protocol === 'https:' ? 'wss:' : 'ws:';
    const host = hostIp || window.location.hostname || 'localhost';
    const port = hostPort || window.location.port || '8080';
    const url = `${protocol}//${host}:${port}/ws/stream?token=${encodeURIComponent(this.token)}`;

    console.log(`Connecting to stream at ${url}...`);
    this.ws = new WebSocket(url);
    this.ws.binaryType = 'arraybuffer';

    this.ws.onopen = () => {
      this.isConnected = true;
      console.log('Stream WebSocket connected.');
      if (this.onConnectionChange) this.onConnectionChange(true);
    };

    this.ws.onmessage = async (event) => {
      if (typeof event.data === 'string') return;

      const buffer = event.data;
      if (buffer.byteLength < 9) return;

      this.bytesReceived += buffer.byteLength;
      this.frameCount++;

      // Extract 8-byte server timestamp (Int64 BigEndian / LittleEndian)
      const dataView = new DataView(buffer);
      // Read Int64 LittleEndian
      const serverTimestamp = Number(dataView.getBigInt64(0, true));
      const now = Date.now();
      if (serverTimestamp > 0 && now >= serverTimestamp) {
        this.latencyMs = now - serverTimestamp;
      }

      // Remaining bytes is image data
      const imageBytes = new Uint8Array(buffer, 8);
      const blob = new Blob([imageBytes], { type: 'image/jpeg' });

      try {
        // Hardware accelerated decode
        const bitmap = await createImageBitmap(blob);
        
        // Auto-size canvas to match screen stream aspect ratio
        if (this.canvas.width !== bitmap.width || this.canvas.height !== bitmap.height) {
          this.canvas.width = bitmap.width;
          this.canvas.height = bitmap.height;
        }

        this.ctx.drawImage(bitmap, 0, 0);
        bitmap.close();
      } catch (err) {
        console.warn('Frame render error:', err);
      }

      this.updateStats();
    };

    this.ws.onclose = () => {
      this.isConnected = false;
      console.log('Stream WebSocket closed.');
      if (this.onConnectionChange) this.onConnectionChange(false);
    };

    this.ws.onerror = (err) => {
      console.error('Stream WebSocket error:', err);
      this.disconnect();
    };
  }

  updateStats() {
    const now = performance.now();
    if (now - this.lastFpsUpdate >= 1000) {
      this.fps = Math.round((this.frameCount * 1000) / (now - this.lastFpsUpdate));
      this.frameCount = 0;
      this.lastFpsUpdate = now;

      const bytesDelta = this.bytesReceived;
      this.bytesReceived = 0;
      this.bitrateKbps = Math.round((bytesDelta * 8) / 1000);

      if (this.onStatsUpdate) {
        this.onStatsUpdate({
          fps: this.fps,
          latency: this.latencyMs,
          bitrate: this.bitrateKbps
        });
      }
    }
  }

  disconnect() {
    if (this.ws) {
      try {
        this.ws.close();
      } catch (e) {}
      this.ws = null;
    }
    this.isConnected = false;
    if (this.onConnectionChange) this.onConnectionChange(false);
  }
}
