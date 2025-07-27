import React, { useState } from "react";
import { motion } from "framer-motion";

interface SubSection {
  label: string;
  href: string;
}

interface DockedNavBarProps {
  items: string[];
  activeItem: string;
  onSelect: (item: string) => void;
  subSections: SubSection[];
  currentSubSection: SubSection;
}

const DockedNavBar: React.FC<DockedNavBarProps> = ({
  items,
  activeItem,
  onSelect,
  subSections,
  currentSubSection
}) => {
  const [hovered, setHovered] = useState<string | null>(null);

  return (
    <nav className="bg-base-100 px-4 sm:px-6 py-3 shadow-sm z-50 w-full">
      <div className="flex flex-col items-center space-y-2">
        {/* Main Dock Nav */}
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

        {/* Sub Nav Dropdown for Active Section */}
        <div className="relative">
          <div className="dropdown dropdown-end">
            <div
              tabIndex={0}
              role="button"
              className="btn btn-sm sm:btn-md m-1 bg-base-200 shadow"
            >
              {currentSubSection.label}
            </div>
            <ul
              tabIndex={0}
              className="dropdown-content menu bg-base-100 rounded-box z-[60] w-52 p-2 shadow"
            >
              {subSections.map((section) => (
                <li key={section.label}>
                  <a
                    href={`#${section.href}`}
                    className={`transition-all ${
                      currentSubSection.label === section.label
                        ? "text-primary font-bold text-base"
                        : "text-sm text-base-content/60 hover:text-base-content"
                    }`}
                  >
                    {section.label}
                  </a>
                </li>
              ))}
            </ul>
          </div>
        </div>
      </div>
    </nav>
  );
};

export default DockedNavBar;