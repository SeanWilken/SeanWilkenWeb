import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import tailwindcss from '@tailwindcss/vite';
import autoprefixer from 'autoprefixer'
import path from "path";

const proxyPort = process.env.SERVER_PROXY_PORT || "5000";
const proxyTarget = ('http://' + (process.env.VITE_API_BASE_URL ?? 'localhost') + ":" + proxyPort);

//"http://server:" + proxyPort;

console.log("==== Vite ENV ====");
console.log("SERVER_PROXY_PORT:", process.env.SERVER_PROXY_PORT);
console.log("VITE_API_BASE_URL:", process.env.VITE_API_BASE_URL);
console.log("Proxy Target:", proxyTarget);
console.log("===================");

// https://vitejs.dev/config/
export default defineConfig({
    envDir: "../../infrastructure",
    base: "/",
    plugins: [
        tailwindcss(),
        react(),
    ],
    build: {
        outDir: "../../deploy/public",
        emptyOutDir: true,
    },
    server: {
        port: 8080,
        proxy: {
            // redirect requests that start with /api/ to the server on port 5000
            "/api/": {
                target: "http://localhost:5000", // proxyTarget,
                changeOrigin: true,
            }
        },
    },
    css: {
        postcss: {
            plugins: [
                autoprefixer
            ]
        }
    }
});