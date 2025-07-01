import React from "react";

interface TSXNavBarProps {
  items: string[];
  activeItem: string;
  onSelect: (item: string) => void;
}

const TSXNavBar: React.FC<TSXNavBarProps> = ({ items, activeItem, onSelect }) => {
  return (
    <nav className="bg-base-100 px-6 py-4 shadow-sm z-10 w-full">
      <div className="flex justify-between items-center w-full max-w-7xl mx-auto">
        {/* Logo / Brand */}
        {/* <span className="text-xl font-bold tracking-wide text-primary">MySite</span> */}

        {/* Navigation Items */}
        <ul > {/* className="flex flex-1 justify-between items-center ml-10"> */}
          {items.map((item) => {
            const isActive = item === activeItem;

            return (
              <li key={item} className="relative">
                <button
                  onClick={() => onSelect(item)}
                  className={`relative w-full px-4 py-2 text-lg transition-all duration-200
                    ${isActive
                      ? "font-bold text-base-100"
                      : "text-primary/50 hover:text-primary"}
                  `}
                >
                  {/* Parallelogram background for active */}
                  {isActive && (
                    <span className="absolute inset-0 bg-primary skew-x-[-12deg] rounded-md -z-10"></span>
                  )}
                  <span className="relative z-10">{item}</span>
                </button>
              </li>
            );
          })}
        </ul>
      </div>
    </nav>
  );
};

export default TSXNavBar;
