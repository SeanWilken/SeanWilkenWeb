/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './index.html',                  // your root html
    './*.{js,jsx,ts,tsx,fs}', // your source code folder and extensions
    './Modules/*/.{fs,html,js,jsx,ts,tsx}', // if your files are outside src or elsewhere
  ],
  theme: {
    extend: {
      fontFamily: {
        sans: ['DM Sans', 'ui-sans-serif', 'system-ui'],
        heading: ['Space Grotesk', 'ui-sans-serif', 'system-ui'],
      },
    },
  },
  plugins: [require('daisyui')],
  daisyui: {
    themes:  all
  },
}
