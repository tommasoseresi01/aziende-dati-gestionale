import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'
import { fileURLToPath, URL } from 'node:url'

// Configurazione di Vite (il build tool: dev server istantaneo + bundle di produzione).
// https://vite.dev/config/
export default defineConfig({
  plugins: [
    react(), // supporto a React + Fast Refresh (aggiorna i componenti senza ricaricare)
    tailwindcss(), // Tailwind CSS v4 come plugin di Vite (niente più postcss.config a mano)
  ],
  resolve: {
    alias: {
      // Alias "@": permette import puliti come `@/components/...` invece di
      // lunghi percorsi relativi `../../../components/...`. Va tenuto allineato
      // al "paths" del tsconfig.
      '@': fileURLToPath(new URL('./src', import.meta.url)),
    },
  },
  server: {
    port: 5173, // stessa porta consentita dalla policy CORS del backend (Fase 12)
  },
})
