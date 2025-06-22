import React from "react"
import ReactDOM from "react-dom/client"
import { BrowserRouter, Route, Routes } from "react-router-dom"
import App from "./App.tsx"
import { UsersPage } from "./pages/UsersPage.tsx"
import { LoginPage } from "./pages/LoginPage.tsx"
import { AuthProvider } from "./contexts/AuthContext.tsx"
import { ProtectedRoute } from "./components/ProtectedRoute.tsx"
import "./index.css"

ReactDOM.createRoot(document.getElementById("root")!).render(
  <React.StrictMode>
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/" element={<App />} />
          <Route path="/login" element={<LoginPage />} />
          <Route element={<ProtectedRoute />}>
            <Route path="/users" element={<UsersPage />} />
          </Route>
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  </React.StrictMode>,
)
