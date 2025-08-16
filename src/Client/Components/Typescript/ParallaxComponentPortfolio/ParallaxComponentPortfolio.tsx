// This is a scaffold for your interactive programming examples page
// using React (TSX), Tailwind CSS, and DaisyUI

import React, { useState, useEffect, useRef } from 'react';
import { motion } from 'framer-motion';

const sectionGroups = [
  {
    id: 'tsx',
    label: 'TSX Examples',
    items: ['Button', 'Modal', 'Form'],
  },
  {
    id: 'fsharp',
    label: 'F# Examples',
    items: ['Counter', 'Chart', 'Parser'],
  },
];

const ParallaxHeroSplit: React.FC<{ onSelect: (category: 'tsx' | 'fsharp') => void }> = ({ onSelect }) => {
  const heroRef = useRef<HTMLDivElement>(null);

  useEffect(() => {
    const handleMouseMove = (e: MouseEvent) => {
      if (heroRef.current) {
        const x = (e.clientX / window.innerWidth - 0.5) * 30;
        const y = (e.clientY / window.innerHeight - 0.5) * 30;
        heroRef.current.style.setProperty('--x', `${x}px`);
        heroRef.current.style.setProperty('--y', `${y}px`);
      }
    };
    window.addEventListener('mousemove', handleMouseMove);
    return () => window.removeEventListener('mousemove', handleMouseMove);
  }, []);

  return (
    <div
      ref={heroRef}
      className="relative h-screen w-full overflow-hidden flex"
      style={{
        perspective: '1000px',
      }}
    >
      <div
        className="w-1/2 h-full flex items-center justify-center text-6xl font-bold text-white bg-gradient-to-br from-blue-600 to-cyan-500 cursor-pointer skew-x-[-6deg] hover:scale-105 transition-transform"
        style={{ transform: 'translate3d(var(--x), var(--y), 0)' }}
        onClick={() => onSelect('tsx')}
      >
        TSX
      </div>
      <div
        className="w-1/2 h-full flex items-center justify-center text-6xl font-bold text-white bg-gradient-to-br from-purple-600 to-pink-500 cursor-pointer skew-x-[-6deg] hover:scale-105 transition-transform"
        style={{ transform: 'translate3d(var(--x), var(--y), 0)' }}
        onClick={() => onSelect('fsharp')}
      >
        F#
      </div>
    </div>
  );
};

const flatSectionIds = sectionGroups.flatMap((g) => g.items);



interface SideTimelineNavProps {
  sections: string[];
  current: string;
  onJump: (id: string) => void;
}

const SideTimelineNav: React.FC<{
  groups: typeof sectionGroups;
  activeId: string;
  onJump: (id: string) => void;
}> = ({ groups, activeId, onJump }) => {
  return (
    <div className="hidden md:flex flex-col gap-4 py-12 pl-4 pr-2 w-56 sticky top-0 h-screen bg-transparent relative">
      <div className="absolute left-0 top-0 h-full w-[4px] bg-base-300 rounded" />
      <div className="absolute left-0 w-[4px] transition-all duration-300 ease-in-out bg-primary rounded"
        style={{
          top: `calc(${flatSectionIds.indexOf(activeId)} * 3rem + 3rem)`, // 3rem per item + top offset
          height: '2rem'
        }}
      />
      {groups.map(group => (
        <div key={group.id} className="flex flex-col gap-1">
          <div className="text-xs text-base-content/40 tracking-widest mb-1 uppercase">
            {group.label}
          </div>
          {group.items.map((id) => (
            <button
              key={id}
              onClick={() => onJump(id)}
              className={`text-left text-sm font-medium px-3 py-2 rounded transition-colors ${
                activeId === id
                  ? "text-primary bg-base-100/10"
                  : "text-base-content/40 hover:text-base-content/70"
              }`}
            >
              {id}
            </button>
          ))}
        </div>
      ))}
    </div>
  );
};


interface StickyHeaderProps {
  currentComponent: string;
}

const StickyHeader: React.FC<StickyHeaderProps> = ({ currentComponent }) => (
  <div className="sticky top-0 left-0 right-0 h-screen flex items-center justify-center z-50 px-8">
    <h1 className="text-7xl font-extrabold text-primary select-none">{currentComponent}</h1>
  </div>
);

const sectionBackgrounds: Record<string, string> = {
  Button: '#3b82f6',   // blue-500
  Modal: '#10b981',    // green-500
  Form: '#f59e0b',     // amber-500
  Counter: '#6366f1',  // indigo-500
  Chart: '#ec4899',    // pink-500
  Parser: '#f97316',   // orange-500
};

const ProgrammingExamplesPage: React.FC = () => {
  const [category, setCategory] = React.useState<'tsx' | 'fsharp'>('tsx');
  const [activeSection, setActiveSection] = React.useState('Button');

  const categories = {
    tsx: ['Button', 'Modal', 'Form'],
    fsharp: ['Counter', 'Chart', 'Parser'],
  };

  // Scroll listener to update active section
  useEffect(() => {
    const handleScroll = () => {
        const offsets = flatSectionIds.map((id) => {
            const el = document.getElementById(id);
            return el ? { id, offset: el.getBoundingClientRect().top } : null;
        }).filter(Boolean) as { id: string; offset: number }[];

        const current = offsets.find(o => o.offset >= 0) || offsets[offsets.length - 1];
        setActiveSection(current?.id || flatSectionIds[0]);
    };


    window.addEventListener('scroll', handleScroll);
    handleScroll(); // initialize

    return () => window.removeEventListener('scroll', handleScroll);
  }, [category]);

  const scrollToSection = (id: string) => {
    document.getElementById(id)?.scrollIntoView({ behavior: 'smooth' });
  };

  return (
    <div className="flex-1 h-screen overflow-y-scroll snap-y snap-mandatory">
        {sectionGroups.map(group =>
            group.items.map(id => (
            <section
                key={id}
                id={id}
                className="h-screen snap-start flex flex-col justify-center items-center px-10 py-20 transition-opacity duration-500"
            >
                <h2 className="text-4xl font-bold text-white mb-4">{id}</h2>
                <div className="text-base text-white/70 w-full max-w-3xl text-center">
                {/* Placeholder block */}
                <div className="bg-base-200/40 p-10 rounded-xl shadow-lg border border-base-300"
                    style={{ backgroundColor: sectionBackgrounds[id] || '#1e293b', height: '100vh' }}>
                    This will be the interactive component for <strong>{id}</strong>.
                </div>
                </div>
            </section>
            ))
        )}
    </div>
  );
};

export default ProgrammingExamplesPage;
