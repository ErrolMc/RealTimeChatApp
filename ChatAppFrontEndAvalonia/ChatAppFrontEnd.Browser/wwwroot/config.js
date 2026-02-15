// This file is overwritten at runtime by entrypoint.sh in Docker/Azure.
// These are local development defaults.
window.__APP_CONFIG__ = {
  BackendUri: "https://localhost:7071",
  SignalRUri: "https://localhost:7003"
};
