#!/bin/sh

# Generate config.js with runtime environment variables for the WASM app
# Intercepts fetch and WebSocket calls to rewrite localhost URLs to real Azure URLs
cat > /usr/share/nginx/html/config.js <<'TEMPLATE'
(function() {
  var backendUri = "BACKEND_PLACEHOLDER";
  var signalRUri = "SIGNALR_PLACEHOLDER";
  window.__APP_CONFIG__ = { BackendUri: backendUri, SignalRUri: signalRUri, DebugMode: "DEBUG_MODE_PLACEHOLDER" };

  if (backendUri === "https://localhost:7071" && signalRUri === "https://localhost:7003") return;

  // Convert https:// to wss:// for WebSocket URLs
  var signalRWs = signalRUri.replace("https://", "wss://").replace("http://", "ws://");
  var backendWs = backendUri.replace("https://", "wss://").replace("http://", "ws://");

  function rewriteUrl(url) {
    if (typeof url !== "string") return url;
    url = url.replace("https://localhost:7071", backendUri);
    url = url.replace("https://localhost:7003", signalRUri);
    url = url.replace("wss://localhost:7071", backendWs);
    url = url.replace("wss://localhost:7003", signalRWs);
    url = url.replace("http://localhost:7071", backendUri);
    url = url.replace("http://localhost:7003", signalRUri);
    url = url.replace("ws://localhost:7071", backendWs);
    url = url.replace("ws://localhost:7003", signalRWs);
    return url;
  }

  // Intercept fetch (used by .NET WASM HttpClient)
  var originalFetch = window.fetch;
  window.fetch = function(url, options) {
    return originalFetch.call(this, rewriteUrl(url), options);
  };

  // Intercept WebSocket (used by SignalR)
  var OriginalWebSocket = window.WebSocket;
  window.WebSocket = function(url, protocols) {
    url = rewriteUrl(url);
    if (protocols) {
      return new OriginalWebSocket(url, protocols);
    }
    return new OriginalWebSocket(url);
  };
  window.WebSocket.prototype = OriginalWebSocket.prototype;
  window.WebSocket.CONNECTING = OriginalWebSocket.CONNECTING;
  window.WebSocket.OPEN = OriginalWebSocket.OPEN;
  window.WebSocket.CLOSING = OriginalWebSocket.CLOSING;
  window.WebSocket.CLOSED = OriginalWebSocket.CLOSED;
})();
TEMPLATE

# Replace placeholders with actual env var values
sed -i "s|BACKEND_PLACEHOLDER|${BACKEND_URI:-https://localhost:7071}|g" /usr/share/nginx/html/config.js
sed -i "s|SIGNALR_PLACEHOLDER|${SIGNALR_URI:-https://localhost:7003}|g" /usr/share/nginx/html/config.js
sed -i "s|DEBUG_MODE_PLACEHOLDER|${DEBUG_MODE:-false}|g" /usr/share/nginx/html/config.js

# Start nginx
exec nginx -g 'daemon off;'
