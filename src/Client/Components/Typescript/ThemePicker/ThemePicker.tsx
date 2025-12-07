import React, { useEffect } from "react";

interface ThemePickerProps {
  isOpen: boolean;
  onClose: () => void;
}

const themes = [
  // light
  "lofi", "light", "bumblebee", "emerald", "corporate", "fantasy", "garden", "cmyk", "winter",  "nord", "autumn",  "acid", "lemonade", "valentine",
  // color
  "cyberpunk", "aqua", "jade", "night", "synthwave", 
  // dark
  "dim", "dracula", "dark", "sunset", "halloween", "forest"
  // , "oatgrain", "midnight", "morningglow", "quietstream"
  // "cupcake", "retro", "pastel", "wireframe",  "black", "luxury", "business", "coffee",
];

export const ThemePickerModal: React.FC<ThemePickerProps> = ({ isOpen, onClose }) => {
  const savedTheme = localStorage.getItem("theme") || "nord";

  useEffect(() => {
    document.documentElement.setAttribute("data-theme", savedTheme);
  }, []);

  const setTheme = (theme: string) => {
    document.documentElement.setAttribute("data-theme", theme);
    localStorage.setItem("theme", theme);
    onClose();
  };

  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 bg-base-100 bg-opacity-90 flex items-center justify-center px-4 py-8 bg-black/50" onClick={onClose}>
      <div
        className="bg-base-300 rounded-lg shadow-xl max-h-[80vh] w-full max-w-lg overflow-y-auto"
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center px-4 py-3 border-b border-base-content/10">
          <h2 className="text-lg font-bold">Select a Theme</h2>
          <h6>Typescript component</h6>
          <button
            onClick={onClose}
            className="text-base-content/60 hover:text-base-content"
          >
            ×
          </button>
        </div>

        <div className="grid grid-cols-3 lg:grid-cols-4 gap-2">
          {themes.map((theme) => (
            // <div
            //   key={theme}
            //   onClick={() => setTheme(theme)}
            //   className="cursor-pointer aspect-square rounded-xl flex flex-col justify-between items-center p-4 text-center shadow transition hover:scale-105"
            //   style={{ backgroundColor: `hsl(var(--b1))` }} // uses DaisyUI background color of the theme
            // >
            //   <span className="text-sm font-semibold text-base-content">{theme}</span>
            //   <div className="flex space-x-1 mt-2">
            //     <div className="w-4 h-4 rounded bg-primary" />
            //     <div className="w-4 h-4 rounded bg-secondary" />
            //     <div className="w-4 h-4 rounded bg-accent" />
            //   </div>
            // </div>
            <div
              key={theme}
              data-theme={theme}
              onClick={() => {
                setTheme(theme);
              }}
              className={`cursor-pointer aspect-square rounded-xl flex flex-col justify-between items-center p-4 m-4 text-center shadow transition hover:scale-105`}
                // ${
                // theme === savedTheme
                //   ? "bg-primary text-primary-content font-semibold"
                //   : "hover:bg-base-200"
              // }
            >
              <span className="text-sm">{theme.toUpperCase()}</span>
              <div className="flex space-x-1">
                <div className="w-4 h-4 rounded bg-primary" />
                <div className="w-4 h-4 rounded bg-secondary" />
                <div className="w-4 h-4 rounded bg-accent" />
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};

export default ThemePickerModal;