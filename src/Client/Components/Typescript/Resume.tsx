import React, { useState, useEffect, useRef } from 'react';

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
  id: string;
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
  const [activeSection, setActiveSection] = useState(sections[0]?.id || '');
  const sectionRefs = useRef<Record<string, HTMLElement | null>>({});

  useEffect(() => {
    const handleScroll = () => {
      sections.forEach(({ id }) => {
        const el = sectionRefs.current[id];
        if (el) {
          const rect = el.getBoundingClientRect();
          const height = window.innerHeight;
          if (rect.top < height * 0.3 && rect.bottom > height * 0.3) {
            setActiveSection(id);
          }
        }
      });
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [sections]);

  return (
    <div className="relative">
      <nav className="fixed top-0 left-0 w-full z-50 bg-base-200 border-b p-4 backdrop-blur supports-backdrop-blur:bg-base-200/90">
        <ul className="breadcrumbs flex justify-center items-center">
          {sections.map((section, index) => (
            <li key={section.id} className="mx-2 flex items-center">
              <a
                href={`#${section.id}`}
                className={`transition-all ${
                  activeSection === section.id
                    ? 'text-primary font-bold text-base'
                    : 'text-sm text-base-content/60 hover:text-base-content'
                }`}
              >
                {section.label}
              </a>
              {index < sections.length - 1 && (
                <span className="mx-2 text-base-content/40">/</span>
              )}
            </li>
          ))}
        </ul>
      </nav>

      <main
        className="pt-24 space-y-0 min-h-screen scroll-snap-y-mandatory overflow-y-scroll"
        style={{ scrollSnapType: 'y mandatory' }} // fallback for older browsers
      >
        {sections.map((section, index) => (
          <section
            key={section.id}
            id={section.id}
            ref={(el) => (sectionRefs.current[section.id] = el)}
            className={`scroll-mt-24 min-h-screen px-10 ${
              index === sections.length - 1 ? 'pb-40 pt-20' : 'py-20'
            } bg-gradient-to-b ${gradientColors[index % gradientColors.length]}`}
            style={{ scrollSnapAlign: 'start' }}
          >
            <div className="max-w-4xl mx-auto">
              <h2 className="text-3xl font-bold mb-6 text-primary text-center">{section.label}</h2>
              <div className="prose max-w-none bg-base-100 rounded-xl shadow-md p-6">
                <SectionList items={section.items} />
              </div>import React, { useState, useEffect, useRef } from 'react';

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
  id: string;
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
  const [activeSection, setActiveSection] = useState(sections[0]?.id || '');
  const sectionRefs = useRef<Record<string, HTMLElement | null>>({});

  useEffect(() => {
    const handleScroll = () => {
      sections.forEach(({ id }) => {
        const el = sectionRefs.current[id];
        if (el) {
          const rect = el.getBoundingClientRect();
          const height = window.innerHeight;
          if (rect.top < height * 0.3 && rect.bottom > height * 0.3) {
            setActiveSection(id);
          }
        }
      });
    };

    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [sections]);

  return (
    <div className="relative">
      <nav className="fixed top-0 left-0 w-full z-50 bg-base-200 border-b p-4 backdrop-blur supports-backdrop-blur:bg-base-200/90">
        <ul className="breadcrumbs flex justify-center items-center">
          {sections.map((section, index) => (
            <li key={section.id} className="mx-2 flex items-center">
              <a
                href={`#${section.id}`}
                className={`transition-all ${
                  activeSection === section.id
                    ? 'text-primary font-bold text-base'
                    : 'text-sm text-base-content/60 hover:text-base-content'
                }`}
              >
                {section.label}
              </a>
              {index < sections.length - 1 && (
                <span className="mx-2 text-base-content/40">/</span>
              )}
            </li>
          ))}
        </ul>
      </nav>

      <main
        className="pt-24 space-y-0 min-h-screen scroll-snap-y-mandatory overflow-y-scroll"
        style={{ scrollSnapType: 'y mandatory' }} // fallback for older browsers
      >
        {sections.map((section, index) => (
          <section
            key={section.id}
            id={section.id}
            ref={(el) => (sectionRefs.current[section.id] = el)}
            className={`scroll-mt-24 min-h-screen px-10 ${
              index === sections.length - 1 ? 'pb-40 pt-20' : 'py-20'
            } bg-gradient-to-b ${gradientColors[index % gradientColors.length]}`}
            style={{ scrollSnapAlign: 'start' }}
          >
            <div className="max-w-4xl mx-auto">
              <h2 className="text-3xl font-bold mb-6 text-primary text-center">{section.label}</h2>
              <div className="prose max-w-none bg-base-100 rounded-xl shadow-md p-6">
                <SectionList items={section.items} />
              </div>
            </div>
          </section>
        ))}
      </main>
    </div>
  );
};

export default ResumePage;

            </div> b
          </section>
        ))}
      </main>
    </div>
  );i9p-[[[[[453]]]]]
};

export default ResumePage;
