import React, { useState, useEffect, useRef } from 'react';

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
            animation: `fadeInUp 0.4s ease ${idx * 50}ms forwards`
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
          <div className="text-base text-base-content">
            {item}
          </div>
        </div>
      ))}
    </div>
  );
}

// Timeline component for resume experience
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
          <div className={`timeline-${idx % 2 === 0 ? 'start' : 'end'} mb-10 p-2`}>
            <div className={`flex flex-col ${idx % 2 === 0 ? 'items-end text-right' : 'items-start text-left'}`}>
              <div className="font-mono italic text-sm text-base-content/60">
                {item.startDateString} - {item.endDateString}
              </div>
              <div className="text-lg font-bold text-primary clash-font">{item.company}</div>
              <div className="text-base text-base-content/70 mb-2 clash-font text-secondary">{item.role}</div>
            </div>
            <p className="text-base text-base-content/70 mb-2 satoshi-font text-accent">{item.summary}</p>
            <ul className="list-disc pl-4 text-base text-base-content space-y-1">
              {item.responsibilities.map((bullet, i) => (
                <li className="satoshi-font" key={i}>{bullet}</li>
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
  | { label: string; kind: 'section'; items: string[] }
  | { label: string; kind: 'timeline'; items: TimelineItem[] }
  | { label: string; kind: 'tagcloud'; items: string[] };

const gradientColors = [
  'from-base-100 to-base-200',
  'from-base-200 to-base-300',
  'from-primary/5 to-primary/10',
  'from-secondary/5 to-secondary/10',
  'from-accent/5 to-accent/10',
  'from-neutral/5 to-neutral/10'
];


const ResumePage: React.FC<{ sections: ResumeSection[] }> = ({ sections }) => {
  const [activeSection, setActiveSection] = useState(sections[0]?.label || '');
  const sectionRefs = useRef<Record<string, HTMLElement | null>>({});
  console.log(sections[0]);

  useEffect(() => {
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

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [sections]);

  return (
    <div className="relative">
      <main
        className="pt-2 space-y-0  scroll-snap-y-mandatory overflow-y-scroll"
        style={{ scrollSnapType: 'y mandatory' }} // fallback for older browsers
      >
        {sections.map((section, index) => (
          <section
            key={section[0] + '-' + index}
            id={section[0]}
            ref={(el) => (sectionRefs.current[section[0]] = el)}
            className={`scroll-mt-24 px-10 ${
              index === sections.length - 1 ? 'pb-10 pt-10' : 'py-10'
            } bg-gradient-to-b ${gradientColors[index % gradientColors.length]}`}
            style={{ scrollSnapAlign: 'start' }}
          >
            <div className="max-w-4xl mx-auto">
              <h2 className="text-3xl font-bold mb-6 text-primary text-center">{section.label}</h2>
              <div className="prose max-w-none bg-base-100 rounded-xl shadow-md p-6">
                {section.kind === 'tagcloud' ? (
                  <TagCloudList items={section.items as string[]} />
                ) : section.kind === 'timeline' ? (
                  <Timeline items={section.items as TimelineItem[]} />
                ) : (
                  <SectionList items={section.items as string[]} />
                )}
              </div>
            </div>
          </section>
        ))}
      </main>
    </div>
  );
};

export default ResumePage;
