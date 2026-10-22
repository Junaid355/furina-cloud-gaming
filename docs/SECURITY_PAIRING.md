# Local Network Discovery, Pairing & Security Architecture

## 1. Network Discovery

When `FurinaHost` starts up, it automatically queries all local network adapters and binds to `0.0.0.0:8080`.
The host console immediately displays:
- Host Machine Name
- Detected IPv4 LAN addresses (e.g. `http://192.168.1.197:8080`)
- A randomly generated 4-Digit Pairing PIN (e.g. `1164`)

Any device on the same local Wi-Fi network can discover the host by accessing the URL or typing the IP into the mobile client connection bar.

## 2. 4-Digit PIN Pairing Protocol

To protect the host PC from unauthorized remote control on shared networks:

1. **Initial Connection**: A new mobile client visits the host IP and requests a session via `POST /api/pair`:
   ```json
   {
     "Pin": "1164",
     "DeviceName": "iPhone 15 Pro (Safari PWA)"
   }
   ```
2. **Cryptographic Token Issuance**:
   - The host verifies the PIN against the active `PairingManager` session.
   - Upon verification, the host issues a cryptographically secure 256-bit authentication token (e.g. `d8fa31c5b...`).
3. **Session Persistence**:
   - The client stores the token in `localStorage`. Subsequent connections, stream requests, and input messages automatically include this token in the header or payload.
4. **Token Revocation**:
   - The host operator can revoke all paired devices with a single click in the host dashboard, invalidating all issued tokens immediately.

## 3. Network Security Best Practices

- **Local Network Scope**: By default, `FurinaHost` is intended for use over your private home Wi-Fi network.
- **Port Forwarding Warning**: Do not expose port 8080 directly to the public internet without putting it behind a secure reverse proxy (such as Cloudflare Tunnel, Tailscale, or WireGuard VPN).
- **Remote Access (Out of Home)**: For playing outside your home Wi-Fi (e.g. on 5G cellular), install **Tailscale** on your PC and phone. Tailscale creates an encrypted peer-to-peer wireguard tunnel with a private 100.x.x.x IP that allows Furina to stream securely across the globe.
