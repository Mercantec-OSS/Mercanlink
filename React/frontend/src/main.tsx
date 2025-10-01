import React from "react"
import ReactDOM from "react-dom/client"
import { BrowserRouter, Route, Routes } from "react-router-dom"
import App from "./App.tsx"
import { UsersPage } from "./pages/UsersPage.tsx"
import { LoginPage } from "./pages/LoginPage.tsx"
import { SignupPage } from "./pages/SignupPage.tsx"
import { AuthProvider } from "./contexts/AuthContext.tsx"
import { ProtectedRoute } from "./components/ProtectedRoute.tsx"
import { ThemeProvider } from "@/components/ui/theme-provider"
import "./index.css"
import Valgfag from "./pages/Valgfag.tsx"
import FormPage from "./pages/FormPage.tsx"

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <ThemeProvider defaultTheme="dark" storageKey="vite-ui-theme"> {/* <- Wrap everything */}
      <BrowserRouter>
        <AuthProvider>
          <Routes>
            <Route path="/" element={<App />} />
            <Route path="/login" element={<LoginPage />} />
            <Route path="/signup" element={<SignupPage />} />
            <Route path="/valgfag" element={<Valgfag />} />
            <Route path="/form" element={<FormPage />} />
            <Route element={<ProtectedRoute />}>
              <Route path="/users" element={<UsersPage />} />
            </Route>
          </Routes>
        </AuthProvider>
      </BrowserRouter>
    </ThemeProvider>
  </React.StrictMode>
)
