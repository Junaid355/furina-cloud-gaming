# d:\cloud\tests\test_websocket_stream.ps1
$token = "08AD7977C649B3CC3B3EB6414A17DAAC0C4BB74C91E528C713AB3A65C103CA7E"
$uri = New-Object System.Uri("ws://localhost:8080/ws/stream?token=$token")
$ws = New-Object System.Net.WebSockets.ClientWebSocket
$cts = New-Object System.Threading.CancellationTokenSource
$cts.CancelAfter(5000)

Write-Host "Connecting to WebSocket stream: $uri ..."
$connectTask = $ws.ConnectAsync($uri, $cts.Token)
$connectTask.Wait()

if ($ws.State -eq [System.Net.WebSockets.WebSocketState]::Open) {
    Write-Host "WebSocket connected successfully! Receiving frames..."
    $buffer = New-Object byte[] 2097152 # 2MB buffer
    $segment = New-Object System.ArraySegment[byte] -ArgumentList @(,$buffer)
    
    $receivedFrames = 0
    $startTime = [System.Diagnostics.Stopwatch]::StartNew()

    while ($receivedFrames -lt 10 -and $startTime.ElapsedMilliseconds -lt 4000) {
        $recvTask = $ws.ReceiveAsync($segment, $cts.Token)
        $recvTask.Wait()
        $res = $recvTask.Result

        if ($res.MessageType -eq [System.Net.WebSockets.WebSocketMessageType]::Binary -and $res.Count -gt 8) {
            $receivedFrames++
            $serverTimestamp = [System.BitConverter]::ToInt64($buffer, 0)
            $byte0 = $buffer[8]
            $byte1 = $buffer[9]
            $isJpeg = ($byte0 -eq 0xFF -and $byte1 -eq 0xD8)

            Write-Host "Frame #$receivedFrames received: $($res.Count) bytes | ServerTime: $serverTimestamp | JPEG Header: $isJpeg"
        }
    }

    $ws.CloseAsync([System.Net.WebSockets.WebSocketCloseStatus]::NormalClosure, "Test done", [System.Threading.CancellationToken]::None).Wait()
    Write-Host "Successfully received $receivedFrames real-time desktop stream frames!"
} else {
    Write-Host "Failed to connect WebSocket: $($ws.State)"
}
