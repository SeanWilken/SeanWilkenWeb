import React, { useState, useEffect, useRef } from 'react';

export function TagCloudList({ items }: { items: string[] }) {
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

interface ResumeSection {
  label: string;
  items: string[];
}

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

  useEffect(() => {
    const handleScroll = () => {
      sections.forEach(({ label }) => {
        const el = sectionRefs.current[label];
        if (el) {
          const rect = el.getBoundingClientRect();
          const height = window.innerHeight;
          if (rect.top < height * 0.3 && rect.bottom > height * 0.3) {
            setActiveSection(label);
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
        className="pt-24 space-y-0 min-h-screen scroll-snap-y-mandatory overflow-y-scroll"
        style={{ scrollSnapType: 'y mandatory' }} // fallback for older browsers
      >
        {sections.map((section, index) => (
          <section
            key={section.label}
            id={section.label}
            ref={(el) => (sectionRefs.current[section.label] = el)}
            className={`scroll-mt-24 min-h-screen px-10 ${
              index === sections.length - 1 ? 'pb-40 pt-20' : 'py-20'
            } bg-gradient-to-b ${gradientColors[index % gradientColors.length]}`}
            style={{ scrollSnapAlign: 'start' }}
          >
            <div className="max-w-4xl mx-auto">
              <h2 className="text-3xl font-bold mb-6 text-primary text-center">{section.label}</h2>
              <div className="prose max-w-none bg-base-100 rounded-xl shadow-md p-6">
                {
                  section.label === "Skills"
                    ? <TagCloudList items={section.items} />
                    : <SectionList items={section.items} />
                }
                {/* <SectionList items={section.items} /> */}
              </div>
            </div>
          </section>
        ))}
      </main>
    </div>
  );
};

export default ResumePage;
