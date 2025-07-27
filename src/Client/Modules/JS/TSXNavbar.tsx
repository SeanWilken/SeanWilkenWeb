import React, { useState } from "react";
import { motion } from "framer-motion";

interface TSXNavBarProps {
  items: string[];
  activeItem: string;
  onSelect: (item: string) => void;
}

const TSXNavBar: React.FC<TSXNavBarProps> = ({ items, activeItem, onSelect }) => {
  const [hovered, setHovered] = useState<string | null>(null);

  return (
    <nav className="bg-base-100 px-4 sm:px-6 py-3 shadow-sm z-10 w-full">
      <div className="flex flex-wrap justify-center items-end gap-2 sm:gap-4 max-w-7xl mx-auto">
        {items.map((item) => {
          const isActive = item === activeItem;
          const isHovered = item === hovered;

          const scale = hovered
            ? isHovered
              ? 1.25
              : 0.85
            : isActive
            ? 1.15
            : 1;

          return (
            <motion.button
              key={item}
              onClick={() => onSelect(item)}
              onMouseEnter={() => setHovered(item)}
              onMouseLeave={() => setHovered(null)}
              animate={{ scale }}
              transition={{ type: "spring", stiffness: 300, damping: 22 }}
              className="relative px-2 sm:px-4 py-2 font-medium group focus:outline-none"
            >
              {isActive && (
                <motion.span
                  layoutId="nav-highlight"
                  className="absolute inset-0 bg-primary skew-x-[-12deg] rounded-md -z-10"
                  initial={{ opacity: 0 }}
                  animate={{ opacity: 1 }}
                  exit={{ opacity: 0 }}
                />
              )}

              <span
                className={`relative z-10 text-sm sm:text-base transition-colors ${
                  isActive
                    ? "text-base-100 font-bold"
                    : "text-primary/50 group-hover:text-primary"
                }`}
              >
                {item}
              </span>
            </motion.button>
          );
        })}
      </div>
    </nav>
  );
};

export default TSXNavBar;