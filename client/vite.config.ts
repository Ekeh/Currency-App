/// <reference types="vitest" />
import { defineConfig, loadEnv } from "vite";
import react from "@vitejs/plugin-react";

// In development the client talks to the .NET API through a dev proxy, so no
// CORS configuration is needed on the API. Run the API with the `http` launch
// profile (http://localhost:5283) or set VITE_API_PROXY_TARGET.
export default defineConfig(({ mode }) => {
  const env = loadEnv(mode, process.cwd(), "");
  const target = env.VITE_API_PROXY_TARGET || "http://localhost:5283";

  return {
    plugins: [react()],
    server: {
      port: 5173,
      proxy: { "/api": { target, changeOrigin: true } },
    },
    test: {
      globals: true,
      environment: "jsdom",
      setupFiles: ["./src/test/setup.ts"],
      css: false,
    },
  };
});
