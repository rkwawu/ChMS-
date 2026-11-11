import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";

// In development, /api and /health are forwarded to the ASP.NET Core API.
export default defineConfig({
  plugins: [react()],
  build: { target: "es2022" },
  server: {
    port: 5173,
    proxy: {
      "/api": "http://localhost:5169",
      "/health": "http://localhost:5169",
    },
  },
});
