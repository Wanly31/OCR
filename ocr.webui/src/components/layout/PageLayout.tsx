import { Outlet } from 'react-router-dom';
import Sidebar from './Sidebar';
import Header from './Header';

const PageLayout = () => {
  return (
    <div style={{ display: "flex", height: "100vh" }}>

      {/* Sidebar */}
      <Sidebar />

      {/* Main area */}
      <div style={{ flex: 1, display: "flex", flexDirection: "column", overflow: "hidden" }}>

        {/* Header */}
        <Header />

        {/* Content — тут рендериться поточна сторінка */}
        <main style={{ padding: "20px", flex: 1, overflowY: "auto", background: "#0f172a" }}>
          <Outlet />
        </main>

      </div>
    </div>
  );
};

export default PageLayout;