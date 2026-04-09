import { useLocation } from "react-router-dom";

const pageTitles: Record<string, string> = {
  "/":          "Dashboard",
  "/upload":    "Завантажити документ",
  "/review":    "Перевірка результату",
  "/patients":  "Пацієнти",
  "/documents": "Документи",
};

const Header = () => {
  const location = useLocation();
  const title = pageTitles[location.pathname] ?? "OCR Medical";

  return (
    <header style={{
      height: "60px",
      background: "#1e293b",
      borderBottom: "1px solid rgba(255,255,255,0.08)",
      display: "flex",
      alignItems: "center",
      padding: "0 24px",
    }}>
      <h3 style={{ color: "#f1f5f9", fontSize: "16px", fontWeight: 600, margin: 0 }}>
        {title}
      </h3>
    </header>
  );
};

export default Header;