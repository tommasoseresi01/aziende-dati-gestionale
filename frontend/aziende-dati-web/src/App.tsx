import { Navigate, Route, Routes } from 'react-router-dom'
import { RequireAuth } from '@/auth/RequireAuth'
import { AppLayout } from '@/components/layout/AppLayout'
import { AziendePage } from '@/pages/AziendePage'
import { CategoriePage } from '@/pages/CategoriePage'
import { DashboardPage } from '@/pages/DashboardPage'
import { DatiPage } from '@/pages/DatiPage'
import { LoginPage } from '@/pages/LoginPage'

// LE ROTTE dell'app (React Router). La struttura ad ANNIDAMENTO è il cuore:
//  - /login è pubblica.
//  - Tutto il resto è dentro <RequireAuth> (il route guard): senza login si viene
//    rimandati a /login.
//  - Dentro la guardia, <AppLayout> fornisce sidebar + header, e le pagine si
//    montano nel suo <Outlet />.
export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />

      <Route element={<RequireAuth />}>
        <Route element={<AppLayout />}>
          <Route path="/" element={<DashboardPage />} />
          <Route path="/aziende" element={<AziendePage />} />
          <Route path="/categorie" element={<CategoriePage />} />
          <Route path="/dati" element={<DatiPage />} />
        </Route>
      </Route>

      {/* Qualunque altra rotta → home (che a sua volta protegge/ridirige). */}
      <Route path="*" element={<Navigate to="/" replace />} />
    </Routes>
  )
}
