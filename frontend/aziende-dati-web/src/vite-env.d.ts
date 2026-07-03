/// <reference types="vite/client" />

// Tipizziamo le variabili d'ambiente di Vite: così `import.meta.env.VITE_API_URL`
// è un `string` conosciuto da TypeScript (autocompletamento + errori se sbagli nome).
interface ImportMetaEnv {
  readonly VITE_API_URL: string
}

interface ImportMeta {
  readonly env: ImportMetaEnv
}
