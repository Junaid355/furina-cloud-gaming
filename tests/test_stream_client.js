// d:\cloud\tests\test_stream_client.js
const wsStream = new WebSocket('ws://localhost:8080/ws/stream');
const wsInput = new WebSocket('ws://localhost:8080/ws/input');

let framesReceived = 0;
let totalBytes = 0;
const startTime = Date.now();

wsStream.binaryType = 'arraybuffer';

wsStream.onopen = () => {
  console.log('[StreamWS] Connected to live desktop video stream.');
};

wsStream.onmessage = (event) => {
  const buf = event.data;
  framesReceived++;
  totalBytes += buf.byteLength;

  if (buf.byteLength >= 10) {
    const view = new DataView(buf);
    const serverTimestamp = Number(view.getBigInt64(0, true));
    const now = Date.now();
    const latency = now - serverTimestamp;

    const u8 = new Uint8Array(buf);
    const isJpeg = (u8[8] === 0xFF && u8[9] === 0xD8);

    if (framesReceived <= 5 || framesReceived % 10 === 0) {
      console.log(`[StreamWS] Frame #${framesReceived}: ${buf.byteLength} bytes | Latency: ${latency}ms | JPEG Header: ${isJpeg}`);
    }
  }

  if (framesReceived >= 20) {
    const elapsedSec = (Date.now() - startTime) / 1000;
    const fps = Math.round(framesReceived / elapsedSec);
    const kbps = Math.round((totalBytes * 8) / (elapsedSec * 1000));
    console.log(`\n=== STREAM VERIFICATION SUCCESS ===`);
    console.log(`Total Frames: ${framesReceived}`);
    console.log(`Effective FPS: ${fps}`);
    console.log(`Bandwidth: ${kbps} Kbps`);
    console.log(`===================================\n`);
    wsStream.close();
  }
};

wsInput.onopen = () => {
  console.log('[InputWS] Connected to low-latency input pipeline.');
  // Send a heartbeat ping
  wsInput.send(JSON.stringify({
    type: 'ping',
    clientTimestamp: Date.now()
  }));

  // Send a simulated touch joystick vector
  wsInput.send(JSON.stringify({
    type: 'joystick_vector',
    deltaX: 0.5,
    deltaY: -0.7
  }));

  // Send mouse delta look
  wsInput.send(JSON.stringify({
    type: 'mouse_delta',
    deltaX: 10,
    deltaY: 5
  }));
};

wsInput.onmessage = (event) => {
  console.log('[InputWS] Received response from host:', event.data);
  const msg = JSON.parse(event.data);
  if (msg.Type === 'pong') {
    const rtt = Date.now() - msg.ClientTimestamp;
    console.log(`[InputWS] Ping-Pong RTT Input Latency: ${rtt} ms`);
    wsInput.close();
  }
};

setTimeout(() => {
  console.log('Test completed.');
  process.exit(0);
}, 4000);
