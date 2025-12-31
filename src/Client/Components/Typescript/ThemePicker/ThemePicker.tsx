import React, { useEffect, useState } from "react";

interface ThemePickerProps {
  isOpen: boolean;
  onClose: () => void;
}

const themeGroups = {
  light: [
    "lofi", "light", "bumblebee", "emerald", "corporate", 
    "fantasy", "garden", "cmyk", "winter", "nord", 
    "autumn", "acid", "lemonade", "valentine"
  ],
  vibrant: [
    "cyberpunk", "aqua", "jade", "night", "synthwave"
  ],
  dark: [
    "dim", "dracula", "dark", "sunset", "halloween", "forest"
  ]
};

export const ThemePickerModal: React.FC<ThemePickerProps> = ({ isOpen, onClose }) => {
  const [savedTheme, setSavedTheme] = useState(localStorage.getItem("theme") || "nord");
  const [hoveredTheme, setHoveredTheme] = useState<string | null>(null);

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", savedTheme);
  }, []);

  const setTheme = (theme: string) => {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem("theme", theme);
    setSavedTheme(theme);
    
    // Add haptic feedback delay before closing
    setTimeout(() => {
      onClose();
    }, 200);
  };

  if (!isOpen) return null;

  return (
    <div 
      className="fixed inset-0 z-50 flex items-center justify-center px-4 py-8 animate-fade-in"
      style={{
        background: 'rgba(0, 0, 0, 0.75)',
        backdropFilter: 'blur(12px)',
        WebkitBackdropFilter: 'blur(12px)',
      }}
      onClick={onClose}
    >
      <style>{`
        @import url('https://fonts.googleapis.com/css2?family=Outfit:wght@300;400;500;600&display=swap');

        @keyframes fade-in {
          from {
            opacity: 0;
          }
          to {
            opacity: 1;
          }
        }

        @keyframes slide-up {
          from {
            opacity: 0;
            transform: translateY(20px) scale(0.98);
          }
          to {
            opacity: 1;
            transform: translateY(0) scale(1);
          }
        }

        @keyframes card-in {
          from {
            opacity: 0;
            transform: translateY(10px) scale(0.95);
          }
          to {
            opacity: 1;
            transform: translateY(0) scale(1);
          }
        }

        .animate-fade-in {
          animation: fade-in 0.3s cubic-bezier(0.16, 1, 0.3, 1);
        }

        .animate-slide-up {
          animation: slide-up 0.4s cubic-bezier(0.16, 1, 0.3, 1);
        }

        .theme-card {
          transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
          position: relative;
          overflow: hidden;
        }

        .theme-card::before {
          content: '';
          position: absolute;
          inset: -2px;
          background: linear-gradient(135deg, 
            rgba(255, 255, 255, 0.1), 
            rgba(255, 255, 255, 0.05)
          );
          opacity: 0;
          transition: opacity 0.3s cubic-bezier(0.16, 1, 0.3, 1);
          border-radius: inherit;
          z-index: -1;
        }

        .theme-card:hover::before {
          opacity: 1;
        }

        .theme-card:hover {
          transform: translateY(-4px) scale(1.02);
        }

        .theme-card:active {
          transform: translateY(-2px) scale(0.98);
        }

        .theme-card.active {
          box-shadow: 
            0 0 0 2px rgba(255, 255, 255, 0.2),
            0 8px 24px rgba(0, 0, 0, 0.3);
        }

        .theme-card.active::after {
          content: '✓';
          position: absolute;
          top: 8px;
          right: 8px;
          width: 20px;
          height: 20px;
          background: rgba(255, 255, 255, 0.95);
          color: #000;
          border-radius: 50%;
          display: flex;
          align-items: center;
          justify-content: center;
          font-size: 12px;
          font-weight: 600;
          box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
        }

        .color-dot {
          transition: all 0.3s cubic-bezier(0.16, 1, 0.3, 1);
        }

        .theme-card:hover .color-dot {
          transform: scale(1.15);
        }

        .modal-content {
          font-family: 'Outfit', -apple-system, BlinkMacSystemFont, sans-serif;
        }

        .theme-section-title {
          font-size: 11px;
          font-weight: 500;
          letter-spacing: 0.1em;
          text-transform: uppercase;
          opacity: 0.5;
          margin: 24px 0 12px;
        }

        .scrollbar-thin::-webkit-scrollbar {
          width: 6px;
        }

        .scrollbar-thin::-webkit-scrollbar-track {
          background: rgba(255, 255, 255, 0.05);
          border-radius: 3px;
        }

        .scrollbar-thin::-webkit-scrollbar-thumb {
          background: rgba(255, 255, 255, 0.2);
          border-radius: 3px;
        }

        .scrollbar-thin::-webkit-scrollbar-thumb:hover {
          background: rgba(255, 255, 255, 0.3);
        }

        .close-btn {
          transition: all 0.2s cubic-bezier(0.16, 1, 0.3, 1);
        }

        .close-btn:hover {
          transform: rotate(90deg);
          opacity: 1;
        }
      `}</style>

      <div
        className="modal-content bg-base-300 rounded-2xl shadow-2xl max-h-[85vh] w-full max-w-2xl overflow-hidden animate-slide-up"
        onClick={(e) => e.stopPropagation()}
        style={{
          border: '1px solid rgba(255, 255, 255, 0.1)',
        }}
      >
        {/* Header */}
        <div 
          className="flex justify-between items-center px-6 py-5 border-b"
          style={{ borderColor: 'rgba(255, 255, 255, 0.08)' }}
        >
          <div>
            <h2 className="text-xl font-medium tracking-tight">Select a Theme</h2>
            <p className="text-xs mt-1 opacity-50 tracking-wide">Choose your visual style</p>
          </div>
          <button
            onClick={onClose}
            className="close-btn w-10 h-10 flex items-center justify-center rounded-full text-2xl opacity-60 hover:bg-base-100/50"
            aria-label="Close"
          >
            ×
          </button>
        </div>

        {/* Content */}
        <div className="p-6 overflow-y-auto scrollbar-thin" style={{ maxHeight: 'calc(85vh - 88px)' }}>
          {/* Light Themes */}
          <div className="theme-section-title">Light & Neutral</div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
            {themeGroups.light.map((theme, index) => (
              <div
                key={theme}
                data-theme={theme}
                onClick={() => setTheme(theme)}
                onMouseEnter={() => setHoveredTheme(theme)}
                onMouseLeave={() => setHoveredTheme(null)}
                className={`theme-card cursor-pointer aspect-square rounded-xl flex flex-col justify-between p-4 bg-base-100 ${
                  theme === savedTheme ? 'active' : ''
                }`}
                style={{
                  animationDelay: `${index * 0.03}s`,
                  animation: isOpen ? 'card-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both' : 'none',
                }}
              >
                <span className="text-xs font-medium tracking-wide text-base-content opacity-90">
                  {theme.toUpperCase()}
                </span>
                <div className="flex gap-1.5 justify-center">
                  <div className="color-dot w-3 h-3 rounded-full bg-primary" />
                  <div className="color-dot w-3 h-3 rounded-full bg-secondary" />
                  <div className="color-dot w-3 h-3 rounded-full bg-accent" />
                </div>
              </div>
            ))}
          </div>

          {/* Vibrant Themes */}
          <div className="theme-section-title">Vibrant & Bold</div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
            {themeGroups.vibrant.map((theme, index) => (
              <div
                key={theme}
                data-theme={theme}
                onClick={() => setTheme(theme)}
                onMouseEnter={() => setHoveredTheme(theme)}
                onMouseLeave={() => setHoveredTheme(null)}
                className={`theme-card cursor-pointer aspect-square rounded-xl flex flex-col justify-between p-4 bg-base-100 ${
                  theme === savedTheme ? 'active' : ''
                }`}
                style={{
                  animationDelay: `${(themeGroups.light.length + index) * 0.03}s`,
                  animation: isOpen ? 'card-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both' : 'none',
                }}
              >
                <span className="text-xs font-medium tracking-wide text-base-content opacity-90">
                  {theme.toUpperCase()}
                </span>
                <div className="flex gap-1.5 justify-center">
                  <div className="color-dot w-3 h-3 rounded-full bg-primary" />
                  <div className="color-dot w-3 h-3 rounded-full bg-secondary" />
                  <div className="color-dot w-3 h-3 rounded-full bg-accent" />
                </div>
              </div>
            ))}
          </div>

          {/* Dark Themes */}
          <div className="theme-section-title">Dark & Moody</div>
          <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3 pb-4">
            {themeGroups.dark.map((theme, index) => (
              <div
                key={theme}
                data-theme={theme}
                onClick={() => setTheme(theme)}
                onMouseEnter={() => setHoveredTheme(theme)}
                onMouseLeave={() => setHoveredTheme(null)}
                className={`theme-card cursor-pointer aspect-square rounded-xl flex flex-col justify-between p-4 bg-base-100 ${
                  theme === savedTheme ? 'active' : ''
                }`}
                style={{
                  animationDelay: `${(themeGroups.light.length + themeGroups.vibrant.length + index) * 0.03}s`,
                  animation: isOpen ? 'card-in 0.4s cubic-bezier(0.16, 1, 0.3, 1) both' : 'none',
                }}
              >
                <span className="text-xs font-medium tracking-wide text-base-content opacity-90">
                  {theme.toUpperCase()}
                </span>
                <div className="flex gap-1.5 justify-center">
                  <div className="color-dot w-3 h-3 rounded-full bg-primary" />
                  <div className="color-dot w-3 h-3 rounded-full bg-secondary" />
                  <div className="color-dot w-3 h-3 rounded-full bg-accent" />
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default ThemePickerModal;