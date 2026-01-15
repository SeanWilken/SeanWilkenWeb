// vite.config.mts
import { defineConfig } from "file:///C:/Users/Sean%20Wilken/Developer/Code/Projects/SeanWilkenWeb/src/Client/node_modules/.pnpm/vite@5.4.21_@types+node@20._08d47ea0716aae9bb5f022f860846468/node_modules/vite/dist/node/index.js";
import react from "file:///C:/Users/Sean%20Wilken/Developer/Code/Projects/SeanWilkenWeb/src/Client/node_modules/.pnpm/@vitejs+plugin-react@4.7.0__abe30eafe59821f492dacb4095fa0106/node_modules/@vitejs/plugin-react/dist/index.js";
import tailwindcss from "file:///C:/Users/Sean%20Wilken/Developer/Code/Projects/SeanWilkenWeb/src/Client/node_modules/.pnpm/@tailwindcss+vite@4.1.18_vi_c84da0de6525034b6504e73418c6b070/node_modules/@tailwindcss/vite/dist/index.mjs";
import autoprefixer from "file:///C:/Users/Sean%20Wilken/Developer/Code/Projects/SeanWilkenWeb/src/Client/node_modules/.pnpm/autoprefixer@10.4.23_postcss@8.5.6/node_modules/autoprefixer/lib/autoprefixer.js";
var proxyPort = process.env.SERVER_PROXY_PORT || "5000";
var proxyTarget = "http://" + (process.env.VITE_API_BASE_URL ?? "localhost") + ":" + proxyPort;
console.log("==== Vite ENV ====");
console.log("SERVER_PROXY_PORT:", process.env.SERVER_PROXY_PORT);
console.log("VITE_API_BASE_URL:", process.env.VITE_API_BASE_URL);
console.log("Proxy Target:", proxyTarget);
console.log("===================");
var vite_config_default = defineConfig({
  envDir: "../../infrastructure",
  base: "./",
  plugins: [
    tailwindcss(),
    react()
  ],
  build: {
    outDir: "../../deploy/public",
    emptyOutDir: true
  },
  server: {
    port: 8080,
    proxy: {
      // redirect requests that start with /api/ to the server on port 5000
      "/api/": {
        target: "http://localhost:5000",
        // proxyTarget,
        changeOrigin: true
      }
    }
  },
  css: {
    postcss: {
      plugins: [
        autoprefixer
      ]
    }
  }
});
export {
  vite_config_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsidml0ZS5jb25maWcubXRzIl0sCiAgInNvdXJjZXNDb250ZW50IjogWyJjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZGlybmFtZSA9IFwiQzpcXFxcVXNlcnNcXFxcU2VhbiBXaWxrZW5cXFxcRGV2ZWxvcGVyXFxcXENvZGVcXFxcUHJvamVjdHNcXFxcU2VhbldpbGtlbldlYlxcXFxzcmNcXFxcQ2xpZW50XCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ZpbGVuYW1lID0gXCJDOlxcXFxVc2Vyc1xcXFxTZWFuIFdpbGtlblxcXFxEZXZlbG9wZXJcXFxcQ29kZVxcXFxQcm9qZWN0c1xcXFxTZWFuV2lsa2VuV2ViXFxcXHNyY1xcXFxDbGllbnRcXFxcdml0ZS5jb25maWcubXRzXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ltcG9ydF9tZXRhX3VybCA9IFwiZmlsZTovLy9DOi9Vc2Vycy9TZWFuJTIwV2lsa2VuL0RldmVsb3Blci9Db2RlL1Byb2plY3RzL1NlYW5XaWxrZW5XZWIvc3JjL0NsaWVudC92aXRlLmNvbmZpZy5tdHNcIjtpbXBvcnQgeyBkZWZpbmVDb25maWcgfSBmcm9tIFwidml0ZVwiO1xuaW1wb3J0IHJlYWN0IGZyb20gXCJAdml0ZWpzL3BsdWdpbi1yZWFjdFwiO1xuaW1wb3J0IHRhaWx3aW5kY3NzIGZyb20gJ0B0YWlsd2luZGNzcy92aXRlJztcbmltcG9ydCBhdXRvcHJlZml4ZXIgZnJvbSAnYXV0b3ByZWZpeGVyJ1xuaW1wb3J0IHBhdGggZnJvbSBcInBhdGhcIjtcblxuY29uc3QgcHJveHlQb3J0ID0gcHJvY2Vzcy5lbnYuU0VSVkVSX1BST1hZX1BPUlQgfHwgXCI1MDAwXCI7XG5jb25zdCBwcm94eVRhcmdldCA9ICgnaHR0cDovLycgKyAocHJvY2Vzcy5lbnYuVklURV9BUElfQkFTRV9VUkwgPz8gJ2xvY2FsaG9zdCcpICsgXCI6XCIgKyBwcm94eVBvcnQpO1xuXG4vL1wiaHR0cDovL3NlcnZlcjpcIiArIHByb3h5UG9ydDtcblxuY29uc29sZS5sb2coXCI9PT09IFZpdGUgRU5WID09PT1cIik7XG5jb25zb2xlLmxvZyhcIlNFUlZFUl9QUk9YWV9QT1JUOlwiLCBwcm9jZXNzLmVudi5TRVJWRVJfUFJPWFlfUE9SVCk7XG5jb25zb2xlLmxvZyhcIlZJVEVfQVBJX0JBU0VfVVJMOlwiLCBwcm9jZXNzLmVudi5WSVRFX0FQSV9CQVNFX1VSTCk7XG5jb25zb2xlLmxvZyhcIlByb3h5IFRhcmdldDpcIiwgcHJveHlUYXJnZXQpO1xuY29uc29sZS5sb2coXCI9PT09PT09PT09PT09PT09PT09XCIpO1xuXG4vLyBodHRwczovL3ZpdGVqcy5kZXYvY29uZmlnL1xuZXhwb3J0IGRlZmF1bHQgZGVmaW5lQ29uZmlnKHtcbiAgICBlbnZEaXI6IFwiLi4vLi4vaW5mcmFzdHJ1Y3R1cmVcIixcbiAgICBiYXNlOiBcIi4vXCIsXG4gICAgcGx1Z2luczogW1xuICAgICAgICB0YWlsd2luZGNzcygpLFxuICAgICAgICByZWFjdCgpLFxuICAgIF0sXG4gICAgYnVpbGQ6IHtcbiAgICAgICAgb3V0RGlyOiBcIi4uLy4uL2RlcGxveS9wdWJsaWNcIixcbiAgICAgICAgZW1wdHlPdXREaXI6IHRydWUsXG4gICAgfSxcbiAgICBzZXJ2ZXI6IHtcbiAgICAgICAgcG9ydDogODA4MCxcbiAgICAgICAgcHJveHk6IHtcbiAgICAgICAgICAgIC8vIHJlZGlyZWN0IHJlcXVlc3RzIHRoYXQgc3RhcnQgd2l0aCAvYXBpLyB0byB0aGUgc2VydmVyIG9uIHBvcnQgNTAwMFxuICAgICAgICAgICAgXCIvYXBpL1wiOiB7XG4gICAgICAgICAgICAgICAgdGFyZ2V0OiBcImh0dHA6Ly9sb2NhbGhvc3Q6NTAwMFwiLCAvLyBwcm94eVRhcmdldCxcbiAgICAgICAgICAgICAgICBjaGFuZ2VPcmlnaW46IHRydWUsXG4gICAgICAgICAgICB9XG4gICAgICAgIH0sXG4gICAgfSxcbiAgICBjc3M6IHtcbiAgICAgICAgcG9zdGNzczoge1xuICAgICAgICAgICAgcGx1Z2luczogW1xuICAgICAgICAgICAgICAgIGF1dG9wcmVmaXhlclxuICAgICAgICAgICAgXVxuICAgICAgICB9XG4gICAgfVxufSk7Il0sCiAgIm1hcHBpbmdzIjogIjtBQUF1WixTQUFTLG9CQUFvQjtBQUNwYixPQUFPLFdBQVc7QUFDbEIsT0FBTyxpQkFBaUI7QUFDeEIsT0FBTyxrQkFBa0I7QUFHekIsSUFBTSxZQUFZLFFBQVEsSUFBSSxxQkFBcUI7QUFDbkQsSUFBTSxjQUFlLGFBQWEsUUFBUSxJQUFJLHFCQUFxQixlQUFlLE1BQU07QUFJeEYsUUFBUSxJQUFJLG9CQUFvQjtBQUNoQyxRQUFRLElBQUksc0JBQXNCLFFBQVEsSUFBSSxpQkFBaUI7QUFDL0QsUUFBUSxJQUFJLHNCQUFzQixRQUFRLElBQUksaUJBQWlCO0FBQy9ELFFBQVEsSUFBSSxpQkFBaUIsV0FBVztBQUN4QyxRQUFRLElBQUkscUJBQXFCO0FBR2pDLElBQU8sc0JBQVEsYUFBYTtBQUFBLEVBQ3hCLFFBQVE7QUFBQSxFQUNSLE1BQU07QUFBQSxFQUNOLFNBQVM7QUFBQSxJQUNMLFlBQVk7QUFBQSxJQUNaLE1BQU07QUFBQSxFQUNWO0FBQUEsRUFDQSxPQUFPO0FBQUEsSUFDSCxRQUFRO0FBQUEsSUFDUixhQUFhO0FBQUEsRUFDakI7QUFBQSxFQUNBLFFBQVE7QUFBQSxJQUNKLE1BQU07QUFBQSxJQUNOLE9BQU87QUFBQTtBQUFBLE1BRUgsU0FBUztBQUFBLFFBQ0wsUUFBUTtBQUFBO0FBQUEsUUFDUixjQUFjO0FBQUEsTUFDbEI7QUFBQSxJQUNKO0FBQUEsRUFDSjtBQUFBLEVBQ0EsS0FBSztBQUFBLElBQ0QsU0FBUztBQUFBLE1BQ0wsU0FBUztBQUFBLFFBQ0w7QUFBQSxNQUNKO0FBQUEsSUFDSjtBQUFBLEVBQ0o7QUFDSixDQUFDOyIsCiAgIm5hbWVzIjogW10KfQo=
