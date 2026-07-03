// Configurazione ESLint (formato "flat config", il nuovo standard).
// ESLint analizza il codice e segnala errori/anti-pattern; qui aggiungiamo le
// regole per TypeScript e per gli Hook di React. eslint-config-prettier va per
// ULTIMO: disattiva le regole di stile che confliggerebbero con Prettier
// (Prettier formatta, ESLint controlla la correttezza → non si pestano i piedi).
import js from '@eslint/js'
import globals from 'globals'
import tseslint from 'typescript-eslint'
import reactHooks from 'eslint-plugin-react-hooks'
import reactRefresh from 'eslint-plugin-react-refresh'
import prettier from 'eslint-config-prettier'

export default tseslint.config(
  { ignores: ['dist', 'node_modules'] },
  js.configs.recommended,
  tseslint.configs.recommended,
  {
    files: ['**/*.{ts,tsx}'],
    languageOptions: {
      ecmaVersion: 2023,
      globals: globals.browser, // window, document, ecc. sono globali del browser
    },
    plugins: {
      'react-hooks': reactHooks,
      'react-refresh': reactRefresh,
    },
    rules: {
      // Le due regole d'oro degli Hook: chiamarli solo al top-level e dichiarare
      // tutte le dipendenze negli array di useEffect/useMemo/useCallback.
      'react-hooks/rules-of-hooks': 'error',
      'react-hooks/exhaustive-deps': 'warn',
      'react-refresh/only-export-components': ['warn', { allowConstantExport: true }],
    },
  },
  prettier,
)
