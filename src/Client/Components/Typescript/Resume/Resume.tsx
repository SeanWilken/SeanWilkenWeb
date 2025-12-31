import React, { useState, useEffect, useRef } from "react";

export function TagCloudList({ items }: { items: string[] }) {
  if (!Array.isArray(items)) {
    return null;
  }
  return (
    <div className="flex flex-wrap gap-3 justify-center">
      {items.map((item, idx) => (
        <span
          key={idx}
          className="px-4 py-2.5 bg-transparent border border-base-content/8 
            text-sm font-light tracking-wide transition-all duration-500
            hover:border-base-content/20 hover:bg-base-100/50 
            hover:-translate-y-1 hover:scale-105 cursor-default select-none
            hover:shadow-lg hover:shadow-base-content/5"
          style={{
            animation: `fadeInScale 0.6s cubic-bezier(0.4, 0, 0.2, 1) ${idx * 40}ms forwards`,
            opacity: 0,
          }}
        >
          {item}
        </span>
      ))}
    </div>
  );
}

export function SectionList({ items }: { items: string[] }) {
  if (!Array.isArray(items)) {
    return null;
  }
  return (
    <div className="space-y-8">
      {items.map((item, idx) => (
        <div
          key={idx}
          className="pl-8 border-l border-base-content/10 transition-all duration-500
            hover:border-base-content/20 hover:pl-12 group"
          style={{
            animation: `slideInLeft 0.7s cubic-bezier(0.4, 0, 0.2, 1) ${idx * 100}ms forwards`,
            opacity: 0,
          }}
        >
          <p className="text-sm leading-loose text-base-content/70 font-light
            transition-all duration-300 group-hover:text-base-content/90">
            {item}
          </p>
        </div>
      ))}
    </div>
  );
}

export type TimelineItem = {
  company: string;
  role: string;
  summary: string;
  startDateString: string;
  endDateString: string;
  responsibilities: string[];
};

export function Timeline({ items }: { items: TimelineItem[] }) {
  if (!Array.isArray(items)) return null;

  return (
    <ul className="timeline timeline-snap-icon max-md:timeline-compact timeline-vertical">
      {items.map((item, idx) => (
        <li key={idx} className="group">
          {idx !== 0 && <hr className="bg-base-content/8" />}
          
          {/* Center icon */}
          <div className="timeline-middle">
            <div className="relative">
              <div className="bg-primary/20 text-primary rounded-full p-2 shadow-lg
                transition-all duration-500 group-hover:bg-primary/40 group-hover:scale-125
                ring-4 ring-base-100">
                <svg
                  xmlns="http://www.w3.org/2000/svg"
                  viewBox="0 0 20 20"
                  fill="currentColor"
                  className="h-5 w-5"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.857-9.809a.75.75 0 00-1.214-.882l-3.483 4.79-1.88-1.88a.75.75 0 10-1.06 1.061l2.5 2.5a.75.75 0 001.137-.089l4-5.5z"
                    clipRule="evenodd"
                  />
                </svg>
              </div>
              {/* Pulse ring on hover */}
              <div className="absolute inset-0 rounded-full bg-primary/20 
                opacity-0 group-hover:opacity-100 group-hover:animate-ping" />
            </div>
          </div>
          
          {/* Alternating content */}
          <div
            className={`timeline-${
              idx % 2 === 0 ? "start" : "end"
            } mb-10 md:mb-16 p-2 transition-all duration-700 group-hover:scale-[1.02]`}
            style={{
              animation: `fadeInUp 0.8s cubic-bezier(0.4, 0, 0.2, 1) ${idx * 150}ms forwards`,
              opacity: 0,
            }}
          >
            <div
              className={`flex flex-col ${
                idx % 2 === 0 ? "items-end text-right" : "items-start text-left"
              }`}
            >
              {/* Date */}
              <div className="font-mono text-xs tracking-widest uppercase text-base-content/40 mb-2
                transition-all duration-300 group-hover:text-base-content/60">
                {item.startDateString} — {item.endDateString}
              </div>
              
              {/* Company */}
              <div className="text-2xl font-light mb-1 tracking-tight transition-all duration-300
                group-hover:text-primary/80"
                style={{ fontFamily: "'Cormorant Garamond', serif" }}>
                {item.company}
              </div>
              
              {/* Role */}
              <div className="text-base text-base-content/70 mb-3 font-light tracking-wide
                transition-colors duration-300 group-hover:text-base-content/90">
                {item.role}
              </div>
            </div>
            
            {/* Summary */}
            <p className={`text-sm text-base-content/70 mb-4 leading-loose font-light
              transition-all duration-500 group-hover:text-base-content/90
              ${idx % 2 === 0 ? "text-right" : "text-left"}`}>
              {item.summary}
            </p>
            
            {/* Responsibilities */}
            <ul className={`list-disc space-y-2 text-sm text-base-content
              ${idx % 2 === 0 ? "pl-0 pr-4 text-right list-inside" : "pl-4 text-left"}`}>
              {item.responsibilities.map((bullet, i) => (
                <li 
                  key={i} 
                  className="font-light transition-all duration-500 hover:text-base-content/80"
                  style={{
                    animation: `fadeInRight 0.5s cubic-bezier(0.4, 0, 0.2, 1) ${(idx * 150) + (i * 80)}ms forwards`,
                    opacity: 0,
                  }}
                >
                  {bullet}
                </li>
              ))}
            </ul>
          </div>
          
          {idx !== items.length - 1 && <hr className="bg-base-content/8" />}
        </li>
      ))}
    </ul>
  );
}

export type ResumeSection =
  | { label: string; kind: "section"; items: string[] }
  | { label: string; kind: "timeline"; items: TimelineItem[] }
  | { label: string; kind: "tagcloud"; items: string[] };

const RESUME_HTML_PATH = "./html/Resume.html";

interface ResumePageProps {
  sections: ResumeSection[];
}

export function openResumeForPrint() {
  const win = window.open(RESUME_HTML_PATH, "_blank");
  if (!win) {
    window.location.href = RESUME_HTML_PATH;
    return;
  }

  const checkReady = () => {
    try {
      if (win.document && win.document.readyState === "complete") {
        win.focus();
        win.print();
        return true;
      }
    } catch {
      return true;
    }
    return false;
  };

  if (checkReady()) return;

  const interval = setInterval(() => {
    if (!win || win.closed || checkReady()) {
      clearInterval(interval);
    }
  }, 200);
}

const ResumePage: React.FC<ResumePageProps> = ({ sections }) => {
  const [activeSection, setActiveSection] = useState(
    sections[0]?.label || ""
  );
  const [mode, setMode] = useState<"interactive" | "reader">("interactive");
  const [scrollProgress, setScrollProgress] = useState(0);
  const [isVisible, setIsVisible] = useState(false);
  const sectionRefs = useRef<Record<string, HTMLElement | null>>({});

  // Page entrance animation
  useEffect(() => {
    setIsVisible(true);
  }, []);

  // Scroll progress tracking
  useEffect(() => {
    const handleScroll = () => {
      const totalHeight = document.documentElement.scrollHeight - window.innerHeight;
      const progress = (window.scrollY / totalHeight) * 100;
      setScrollProgress(Math.min(progress, 100));
    };

    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, []);

  // Active section tracking
  useEffect(() => {
    if (mode !== "interactive") return;

    const handleScroll = () => {
      sections.forEach((section) => {
        const el = sectionRefs.current[section.label];
        if (el) {
          const rect = el.getBoundingClientRect();
          const height = window.innerHeight;
          if (rect.top < height * 0.3 && rect.bottom > height * 0.3) {
            setActiveSection(section.label);
          }
        }
      });
    };

    window.addEventListener("scroll", handleScroll, { passive: true });
    return () => window.removeEventListener("scroll", handleScroll);
  }, [sections, mode]);

  return (
    <div className="relative">
      {/* Scroll progress bar */}
      <div className="fixed top-0 left-0 right-0 h-0.5 bg-base-content/5 z-50 print:hidden">
        <div 
          className="h-full bg-gradient-to-r from-primary/40 to-secondary/40 transition-all duration-300"
          style={{ width: `${scrollProgress}%` }}
        />
      </div>

      {/* Premium Header with entrance animation */}
      <div 
        className={`px-8 lg:px-12 pt-24 pb-12 transition-all duration-1000 print:pt-8 print:pb-4 ${
          isVisible ? 'opacity-100 translate-y-0' : 'opacity-0 translate-y-8'
        }`}
      >
        <div className="max-w-5xl mx-auto">

          {/* Title Section with staggered animation */}
          <div className="text-center mb-16 print:mb-6">
            <h1 
              className="text-6xl lg:text-7xl font-light mb-6 leading-tight tracking-tight
                transition-all duration-1000 delay-100 print:text-4xl print:mb-3"
              style={{ 
                fontFamily: "'Cormorant Garamond', serif",
                animation: 'fadeInUp 1s cubic-bezier(0.4, 0, 0.2, 1) 200ms forwards',
                opacity: 0
              }}
            >
              Resume
            </h1>
            <p 
              className="text-sm text-base-content/60 leading-loose max-w-2xl mx-auto font-light
                print:text-xs print:mb-2"
              style={{
                animation: 'fadeInUp 1s cubic-bezier(0.4, 0, 0.2, 1) 400ms forwards',
                opacity: 0
              }}
            >
              A comprehensive overview of my professional experience, technical skills, and the systems I've built. This page is entirely built in TypeScript, and leveraged using bindings through Fable.
              Toggle reader mode for a traditional view, or explore interactively.
            </p>
          </div>

          {/* Controls with entrance animation */}
          <div 
            className="flex flex-col sm:flex-row gap-6 items-center justify-center print:hidden"
            style={{
              animation: 'fadeInUp 1s cubic-bezier(0.4, 0, 0.2, 1) 600ms forwards',
              opacity: 0
            }}
          >
            {/* Download Button with hover animation */}
            <button
              onClick={openResumeForPrint}
              className="px-8 py-3.5 bg-transparent border border-base-content/20 
                text-xs tracking-widest uppercase font-light
                hover:bg-base-content hover:text-base-100 hover:border-base-content
                transition-all duration-400 hover:-translate-y-1 hover:shadow-xl
                flex items-center gap-3 group relative overflow-hidden"
            >
              {/* Shimmer effect on hover */}
              <div className="absolute inset-0 -translate-x-full group-hover:translate-x-full 
                transition-transform duration-1000 bg-gradient-to-r from-transparent 
                via-base-100/10 to-transparent" />
              <span className="relative z-10">Download PDF</span>
              <svg className="w-4 h-4 relative z-10 transition-transform duration-300 
                group-hover:translate-y-1" 
                fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" 
                  d="M12 10v6m0 0l-3-3m3 3l3-3m2 8H7a2 2 0 01-2-2V5a2 2 0 012-2h5.586a1 1 0 01.707.293l5.414 5.414a1 1 0 01.293.707V19a2 2 0 01-2 2z" />
              </svg>
            </button>

            {/* Reader Mode Toggle with smooth animation */}
            <label className="flex items-center gap-4 cursor-pointer group">
              <span className="text-xs tracking-wider uppercase font-light text-base-content/60
                group-hover:text-base-content transition-colors duration-300">
                Reader Mode
              </span>
              <div className="relative">
                <input
                  type="checkbox"
                  className="sr-only"
                  checked={mode === "reader"}
                  onChange={(e) =>
                    setMode(e.target.checked ? "reader" : "interactive")
                  }
                />
                <div className={`w-14 h-7 border border-base-content/20 rounded-full
                  transition-all duration-500 
                  ${mode === "reader" ? 'bg-base-content/10 border-base-content/40' : 'bg-transparent'}`}>
                  <div className={`w-5 h-5 bg-base-content rounded-full 
                    transition-all duration-500 mt-0.5 shadow-lg
                    ${mode === "reader" ? 'translate-x-8 scale-110' : 'translate-x-1'}`} />
                </div>
              </div>
            </label>
          </div>
        </div>
      </div>

      {mode === "reader" ? (
        // Reader View with fade in
        <div 
          className="px-8 lg:px-12 pb-24 transition-all duration-700 print:px-0 print:pb-0"
          style={{
            animation: 'fadeIn 0.7s cubic-bezier(0.4, 0, 0.2, 1) forwards',
            opacity: 0
          }}
        >
          <div className="max-w-6xl mx-auto h-[80vh] bg-base-100 
            border border-base-content/8 rounded-lg overflow-hidden
            shadow-lg hover:shadow-xl transition-shadow duration-500 print:border-0 print:rounded-none print:shadow-none print:h-auto">
            <iframe
              src={RESUME_HTML_PATH}
              title="Resume Reader View"
              className="w-full h-full"
            />
          </div>
        </div>
      ) : (
        // Interactive View with scroll reveal
        <main className="space-y-0 print:space-y-0">
          {sections.map((section, index) => (
            <section
              key={section.label + "-" + index}
              id={section.label}
              ref={(el) => {
                sectionRefs.current[section.label] = el;
              }}
              className={`px-8 lg:px-12 ${
                index === 0 ? 'pt-12 pb-24' : 'py-24'
              } transition-all duration-700 print:py-6 print:px-4`}
            >
              <div className="max-w-5xl mx-auto">
                {/* Section Label with reveal animation */}
                <div className="flex items-center justify-center mb-16 print:mb-4">
                  <div className="flex items-center gap-4">
                    <div className={`h-px bg-base-content/10 transition-all duration-700 ${
                      activeSection === section.label ? 'w-24' : 'w-12'
                    } print:w-12`} />
                    <h2
                      className={`text-xs tracking-widest uppercase font-light transition-all duration-500 print:text-[10px]
                        ${activeSection === section.label
                          ? "text-base-content scale-110"
                          : "text-base-content/40 scale-100"
                        }`}
                    >
                      {section.label}
                    </h2>
                    <div className={`h-px bg-base-content/10 transition-all duration-700 ${
                      activeSection === section.label ? 'w-24' : 'w-12'
                    } print:w-12`} />
                  </div>
                </div>

                {/* Section Content with entrance animation */}
                <div className="bg-base-100/50 backdrop-blur-sm border border-base-content/6 
                  rounded-2xl p-8 lg:p-12 transition-all duration-700
                  hover:border-base-content/12 hover:shadow-2xl hover:bg-base-100/70
                  hover:scale-[1.01] print:bg-transparent print:border-0 print:p-2 print:hover:scale-100 print:shadow-none">
                  {section.kind === "tagcloud" ? (
                    <TagCloudList items={section.items as string[]} />
                  ) : section.kind === "timeline" ? (
                    <Timeline items={section.items as TimelineItem[]} />
                  ) : (
                    <SectionList items={section.items as string[]} />
                  )}
                </div>
              </div>
            </section>
          ))}
        </main>
      )}

      {/* Animated Scroll Progress Indicator */}
      {mode === "interactive" && (
        <div className="fixed bottom-8 left-1/2 -translate-x-1/2 z-50 print:hidden">
          <div className="flex gap-2 bg-base-100/80 backdrop-blur-md px-4 py-3 rounded-full
            border border-base-content/8 shadow-lg">
            {sections.map((section, idx) => (
              <button
                key={idx}
                onClick={() => {
                  const el = sectionRefs.current[section.label];
                  if (el) {
                    el.scrollIntoView({ behavior: "smooth", block: "start" });
                  }
                }}
                className={`h-1.5 transition-all duration-500 rounded-full relative overflow-hidden
                  ${activeSection === section.label 
                    ? 'w-12 bg-base-content' 
                    : 'w-1.5 bg-base-content/20 hover:bg-base-content/40 hover:scale-125'
                  }`}
                aria-label={`Scroll to ${section.label}`}
              >
                {activeSection === section.label && (
                  <div className="absolute inset-0 bg-gradient-to-r from-primary/40 to-secondary/40 
                    animate-shimmer" />
                )}
              </button>
            ))}
          </div>
        </div>
      )}

      {/* Floating scroll indicator (on first load) */}
      {mode === "interactive" && scrollProgress < 5 && (
        <div className="fixed bottom-32 left-1/2 -translate-x-1/2 z-40 animate-bounce print:hidden">
          <svg className="w-6 h-6 text-base-content/30" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" strokeWidth="2" d="M19 14l-7 7m0 0l-7-7m7 7V3" />
          </svg>
        </div>
      )}

      {/* Enhanced CSS animations */}
      <style>{`
        @keyframes fadeInUp {
          from {
            opacity: 0;
            transform: translateY(30px);
          }
          to {
            opacity: 1;
            transform: translateY(0);
          }
        }

        @keyframes fadeInScale {
          from {
            opacity: 0;
            transform: scale(0.9);
          }
          to {
            opacity: 1;
            transform: scale(1);
          }
        }

        @keyframes fadeIn {
          from {
            opacity: 0;
          }
          to {
            opacity: 1;
          }
        }

        @keyframes slideInLeft {
          from {
            opacity: 0;
            transform: translateX(-30px);
          }
          to {
            opacity: 1;
            transform: translateX(0);
          }
        }

        @keyframes fadeInRight {
          from {
            opacity: 0;
            transform: translateX(20px);
          }
          to {
            opacity: 1;
            transform: translateX(0);
          }
        }

        @keyframes shimmer {
          0% {
            transform: translateX(-100%);
          }
          100% {
            transform: translateX(100%);
          }
        }

        .animate-shimmer {
          animation: shimmer 2s infinite;
        }

        @keyframes ping {
          75%, 100% {
            transform: scale(2);
            opacity: 0;
          }
        }

        .animate-ping {
          animation: ping 1s cubic-bezier(0, 0, 0.2, 1) infinite;
        }

        /* Print styles for single page */
        @media print {
          @page {
            size: letter;
            margin: 0.5in;
          }
          
          body {
            print-color-adjust: exact;
            -webkit-print-color-adjust: exact;
          }

          section {
            page-break-inside: avoid;
            break-inside: avoid;
          }

          .timeline li {
            page-break-inside: avoid;
            break-inside: avoid;
          }

          /* Compact spacing for print */
          h1 {
            font-size: 2rem !important;
            margin-bottom: 0.5rem !important;
          }

          p {
            font-size: 0.75rem !important;
            line-height: 1.3 !important;
          }

          .timeline {
            transform: scale(0.85);
            transform-origin: top center;
          }
        }
      `}</style>
    </div>
  );
};

export default ResumePage;