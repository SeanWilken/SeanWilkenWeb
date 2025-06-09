/** @type {import('tailwindcss').Config} */
module.exports = {
    mode: "jit",
    content: [
        "./index.html",
        "./**/*.{fs,js,ts,jsx,tsx}",
    ],
    theme: {
        extend: {},
    },
    plugins: [require('daisyui')],
    daisyui: {
        themes: [
            "light", // Default light themeAdd commentMore actions
            "dark", // Default dark theme
            "cupcake", // Additional themes
            "synthwave"
        ],
    }
}
