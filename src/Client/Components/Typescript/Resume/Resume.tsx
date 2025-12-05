import React, { useState, useEffect, useRef } from "react";
import html2pdf from "html2pdf.js";

export function TagCloudList({ items }: { items: string[] }) {
  if (!Array.isArray(items)) {
    return null;
  }
  return (
    <div className="flex flex-wrap gap-3 justify-center items-start">
      {items.map((item, idx) => (
        <span
          key={idx}
          className={`
            px-4 py-2 rounded-full bg-primary/10 text-primary border border-primary/20
            text-sm font-medium shadow hover:shadow-md transition-all duration-300
            hover:scale-110 hover:bg-primary/20 cursor-default select-none
            animate-fade-in opacity-0 animate-delay-${idx * 50}ms
          `}
          style={{
            animation: `fadeInUp 0.4s ease ${idx * 50}ms forwards`,
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
    <div className="grid gap-6 sm:grid-cols-1 md:grid-cols-2">
      {items.map((item, idx) => (
        <div
          key={idx}
          className="bg-base-100 border-l-4 border-primary shadow-lg rounded-lg p-4 transition hover:shadow-xl hover:-translate-y-1 duration-300"
        >
          <div className="text-base text-base-content">{item}</div>
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
        <li key={idx}>
          {idx !== 0 && <hr />}
          <div className="timeline-middle">
            <div className="bg-primary text-white rounded-full p-2 shadow">
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
          </div>
          <div
            className={`timeline-${
              idx % 2 === 0 ? "start" : "end"
            } mb-10 p-2`}
          >
            <div
              className={`flex flex-col ${
                idx % 2 === 0 ? "items-end text-right" : "items-start text-left"
              }`}
            >
              <div className="font-mono italic text-sm text-base-content/60">
                {item.startDateString} - {item.endDateString}
              </div>
              <div className="text-lg font-bold text-primary clash-font">
                {item.company}
              </div>
              <div className="text-base text-base-content/70 mb-2 clash-font text-secondary">
                {item.role}
              </div>
            </div>
            <p className="text-base text-base-content/70 mb-2 satoshi-font text-accent">
              {item.summary}
            </p>
            <ul className="list-disc pl-4 text-base text-base-content space-y-1">
              {item.responsibilities.map((bullet, i) => (
                <li className="satoshi-font" key={i}>
                  {bullet}
                </li>
              ))}
            </ul>
          </div>
          <hr />
        </li>
      ))}
    </ul>
  );
}

export type ResumeSection =
  | { label: string; kind: "section"; items: string[] }
  | { label: string; kind: "timeline"; items: TimelineItem[] }
  | { label: string; kind: "tagcloud"; items: string[] };

const gradientColors = [
  "from-base-100 to-base-200",
  "from-base-200 to-base-300",
  "from-primary/5 to-primary/10",
  "from-secondary/5 to-secondary/10",
  "from-accent/5 to-accent/10",
  "from-neutral/5 to-neutral/10",
];

const RESUME_HTML_PATH = "./html/Resume.html";

interface ResumePageProps {
  sections: ResumeSection[];
}

export function openResumeForPrint() {
  const win = window.open(RESUME_HTML_PATH, "_blank");
  if (!win) {
    // Popup blocked, fallback: just navigate in current tab
    window.location.href = RESUME_HTML_PATH;
    return;
  }

  // Once the new window's document is loaded, trigger print
  const checkReady = () => {
    try {
      if (win.document && win.document.readyState === "complete") {
        win.focus();
        win.print();
        return true;
      }
    } catch {
      // If something goes cross-origin (shouldn't here), just stop trying
      return true;
    }
    return false;
  };

  // If it's already loaded (very fast load), print immediately
  if (checkReady()) return;

  // Otherwise, poll until it's ready
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
  const sectionRefs = useRef<Record<string, HTMLElement | null>>({});

  // Scroll tracking for active section (only in interactive mode)
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
      {/* Header with mode toggle + download */}
      <div className="px-4 sm:px-6 lg:px-10 pt-4 pb-3">
        <div className="max-w-4xl mx-auto flex flex-col gap-4 md:flex-row md:items-center md:justify-between">

          {/* Left: title + subtitle */}
          <div className="flex flex-col gap-1">
            <h1 className="text-2xl sm:text-3xl lg:text-4xl font-bold clash-font text-primary">
              Resume
            </h1>
            <p className="text-xs sm:text-sm text-base-content/70 satoshi-font max-w-xl">
              View interactively or switch to a clean reader mode.
            </p>
          </div>

          {/* Right: reader toggle + download button */}
          <div className="flex flex-col sm:flex-row gap-3 items-center sm:items-end md:items-center">

            {/* Download button (PDF generation / html2pdf or static if you add one) */}
            <button
              onClick={openResumeForPrint}
              className="btn btn-outline btn-sm gap-2 shadow-md hover:shadow-lg w-full sm:w-auto"
            >
              Download PDF
              <svg
                xmlns="http://www.w3.org/2000/svg"
                className="w-4 h-4"
                viewBox="0 0 20 20"
                fill="currentColor"
              >
                <path d="M3 14a1 1 0 011-1h3v-7a1 1 0 112 0v7h3a1 1 0 011 1v2a1 1 0 01-1 1H4a1 1 0 01-1-1v-2z" />
                <path d="M7 10a1 1 0 011.707-.707L10 10.586l1.293-1.293A1 1 0 1112.707 10.707l-3 3a1 1 0 01-1.414 0l-3-3A1 1 0 017 10z" />
              </svg>
            </button>

            {/* Reader mode toggle */}
            <label className="flex items-center gap-3 cursor-pointer">
              <span className="text-base sm:text-sm font-medium text-base-content/80">
                Reader Mode
              </span>
              <input
                type="checkbox"
                className="toggle toggle-xl toggle-primary"
                checked={mode === "reader"}
                onChange={(e) =>
                  setMode(e.target.checked ? "reader" : "interactive")
                }
              />
            </label>
          </div>
        </div>
      </div>

      {mode === "reader" ? (
        // Reader View: inline iframe loading the HTML resume
        <div className="px-10 pb-10 pt-2">
          <div className=" mx-auto h-[80vh] bg-base-200 rounded-xl overflow-hidden shadow-md">
            <iframe
              src={RESUME_HTML_PATH}
              title="Resume Reader View"
              className="w-full h-full"
            />
          </div>
        </div>
      ) : (
        // Interactive section/timeline view
        <main
          className="pt-2 space-y-0 scroll-snap-y-mandatory overflow-y-scroll"
          style={{ scrollSnapType: "y mandatory" }}
        >
          {sections.map((section, index) => (
            <section
              key={section.label + "-" + index}
              id={section.label}
              ref={(el) => {
                sectionRefs.current[section.label] = el;
              }}
              className={`scroll-mt-24 px-10 ${
                index === sections.length - 1 ? "pb-10 pt-10" : "py-10"
              } bg-gradient-to-b ${
                gradientColors[index % gradientColors.length]
              }`}
              style={{ scrollSnapAlign: "start" }}
            >
              <div className="max-w-4xl mx-auto">
                <h2
                  className={`text-3xl font-bold mb-6 text-center clash-font ${
                    activeSection === section.label
                      ? "text-primary"
                      : "text-base-content/70"
                  }`}
                >
                  {section.label}
                </h2>
                <div className="prose max-w-none bg-base-100 rounded-xl shadow-md p-6">
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
    </div>
  );
};

export default ResumePage;
