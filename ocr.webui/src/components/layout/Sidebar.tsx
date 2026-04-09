import { Link, useLocation } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";

const navItems = [
  { to: "/",          label: "🏠 Dashboard" },
  { to: "/upload",    label: "📤 Завантажити" },
  { to: "/patients",  label: "👥 Пацієнти" },
  { to: "/documents", label: "📄 Документи" },
];

const Sidebar = () => {
  const location = useLocation();
  const { logout } = useAuth();

  return (
    <div style={{
      width: "240px",
      minWidth: "240px",
      background: "#1e293b",
      color: "white",
      padding: "24px 16px",
      display: "flex",
      flexDirection: "column",
      borderRight: "1px solid rgba(255,255,255,0.08)"
    }}>
      {/* Логотип */}
      <h2 style={{ fontSize: "18px", fontWeight: 700, marginBottom: "32px", color: "#818cf8" }}>
        🩺 OCR Medical
      </h2>

      {/* Навігація */}
      <nav style={{ flex: 1 }}>
        <ul style={{ listStyle: "none", padding: 0, margin: 0, display: "flex", flexDirection: "column", gap: "4px" }}>
          {navItems.map(item => {
            const isActive = location.pathname === item.to;
            return (
              <li key={item.to}>
                <Link
                  to={item.to}
                  style={{
                    display: "block",
                    padding: "10px 14px",
                    borderRadius: "10px",
                    color: isActive ? "#818cf8" : "#94a3b8",
                    background: isActive ? "rgba(99,102,241,0.15)" : "transparent",
                    textDecoration: "none",
                    fontWeight: isActive ? 600 : 400,
                    fontSize: "15px",
                    transition: "all 0.2s",
                  }}
                >
                  {item.label}
                </Link>
              </li>
            );
          })}
        </ul>
      </nav>

      {/* Вихід */}
      <button
        onClick={logout}
        style={{
          background: "rgba(239,68,68,0.15)",
          border: "1px solid rgba(239,68,68,0.3)",
          borderRadius: "10px",
          color: "#f87171",
          cursor: "pointer",
          fontSize: "14px",
          fontWeight: 500,
          padding: "10px 14px",
          textAlign: "left",
        }}
      >
        🚪 Вийти
      </button>
    </div>
  );
};

export default Sidebar;