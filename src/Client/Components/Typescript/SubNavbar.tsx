import React, { useState, useEffect, useRef } from 'react';

interface SubSection {
    label: String
    href: String
}

interface SubNavbarProps {
    CurrentSubSection: SubSection
    SubSections: SubSection[]
}

const SubNavbar: React.FC<SubNavbarProps> = ({ CurrentSubSection, SubSections }) => {
    return (
        <nav className="fixed flex flex-row-reverse  w-full z-50 bg-base-200 border-b p-4 backdrop-blur supports-backdrop-blur:bg-base-200/90">
            <div className="dropdown dropdown-end">
                <div tabIndex={0} role="button" className="btn m-1">{CurrentSubSection.label}</div>
                <ul tabIndex={0} className="dropdown-content menu bg-base-100 rounded-box z-1 w-52 p-2 shadow-sm">
                    {SubSections.map((section, index) => (
                        <li key={section.label}>
                        <a
                        href={`#${section.href}`}
                        className={`transition-all ${
                            CurrentSubSection.label === section.label
                            ? 'text-primary font-bold text-base'
                            : 'text-sm text-base-content/60 hover:text-base-content'
                            }`}
                            >
                        {section.label}
                        </a>
                    </li>
                    ))}
                </ul>
            </div>
        </nav>
    )
};

export default SubNavbar;