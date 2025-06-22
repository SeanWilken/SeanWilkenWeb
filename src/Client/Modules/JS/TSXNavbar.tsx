import React from "react";

interface TSXNavBarProps {
  items: string[];
  activeItem: string;
  onSelect: (item: string) => void;
}

const TSXNavBar: React.FC<TSXNavBarProps> = ({ items, activeItem, onSelect }) => {
  return (
    <nav className="bg-[#0f172a] border-t border-cyan-600 shadow-inner w-full z-10">
      <ul className="flex justify-center items-center py-3 space-x-4">
        {items.map(item => {
          const isActive = item === activeItem;
          return (
            <li key={item}>
              <button
                onClick={() => onSelect(item)}
                className={`
                  px-4 py-2 rounded-full transition duration-200 text-base font-semibold
                  focus:outline-none focus:ring-2 focus:ring-cyan-400
                  ${isActive
                    ? "bg-cyan-600 text-white shadow-md"
                    : "bg-[#1e293b] text-cyan-300 hover:bg-cyan-500 hover:text-white"}
                `}
              >
                {item}
              </button>
            </li>
          );
        })}
      </ul>
    </nav>
  );
};

export default TSXNavBar;
