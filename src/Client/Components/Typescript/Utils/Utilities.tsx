import { oklch } from "culori"; 
import namer from "color-namer"

export function getCssVar(name) {
    return getComputedStyle(document.documentElement)
        .getPropertyValue(name)
        .trim();
}

export function convertOklchToHex(value) { 
    try { return oklch(value).hex; } 
    catch { return value; } 
}

// --- 1. Curated overrides (expand as needed) ---
export const OVERRIDES: Record<string, string> = {
  // --- Core solids ---
  black: "#000000",
  white: "#FFFFFF",
  navy: "#1A1F35",
  red: "#D62828",
  maroon: "#6A1B1A",
  cardinal: "#8C1D18",
  burgundy: "#580F18",
  brown: "#5C4033",
  chocolate: "#3F2A14",
  gold: "#D4AF37",
  yellow: "#FFD300",
  mustard: "#E1AD01",
  olive: "#556B2F",
  army: "#4B5320",
  forest: "#0B3D02",
  kelly: "#00A86B",
  emerald: "#2E8B57",
  teal: "#008080",
  aqua: "#7FDBFF",
  turquoise: "#40E0D0",
  mint: "#AAF0D1",
  'light blue': "#ADD8E6",
  royal: "#4169E1",
  'true royal': "#0A2A88",
  cobalt: "#0047AB",
  purple: "#6A0DAD",
  lavender: "#C8A2C8",
  lilac: "#D8BFD8",
  mauve: "#ba7273",
  pink: "#FFC0CB",
  'light pink': "#FFDFE6",
  berry: "#8A0253",
  coral: "#FF7F50",
  peach: "#FFDAB9",
  orange: "#FF7A00",
  'navy blazer': "#272e3a",
  'solid black triblend': "#1a1e1c",
  'athletic grey triblend': "#8c8c89",
  'grey triblend': "#666260",
  'aqua triblend': "#3193af",
  'white fleck triblend': "#d9d9d9",
  'charity pink': "#ee6181",
  'baby blue': "#bac9de",
  
  // --- Grays ---
  ash: "#B2BEB5",
  silver: "#C0C0C0",
  gray: "#808080",
  'carbon grey': "#b6b3af",
  'dark gray': "#4A4A4A",
  charcoal: "#333333",
  
  // --- Heather family ---
  heather: "#BEBEBE",
  'heather charcoal': "#3e4142",
  'heather natural': "#e7d7be",
  'heather deep teal': "#486e81",
  'charcoal heather': "#3f3e3c",
  "dark gray heather": "#4A4A4A",
  "athletic heather": "#C2C2C2",
  "black heather": "#2F2F2F",
  "heather navy": "#2A3148",
  "forest heather": "#2F4F2F",
  "deep heather": "#707070",
  "heather prism lilac": "#D8BFD8",
  "heather prism peach": "#FFDAB9",
  "heather prism mint": "#C7EDE1",
  "heather prism ice blue": "#C7E8F3",
  "heather prism dust": "#E8DCD2",
  "heather grey": "#a2a5a9",
  "heather mauve": "#a66c68",
  "heather stone": "#A49F96",
  "heather brown": "#8B6F47",
  "heather olive": "#7A8450",
  "heather slate": "#708090",
  "heather midnight navy": "#1E2A47",
  "dark grey heather": "#424242",
  "heather raspberry": "#cb4465",
  "heather dust": "#e0d4c3",
  "oatmeal heather": "#f7f4ed",
  
  // --- Vintage / washed ---
  "vintage black": "#1C1C1C",
  "vintage navy": "#2A324B",
  "vintage red": "#A63A3A",
  "vintage royal": "#3A4F8C",
  'vintage white': "#eee8dc",
  
  // --- Triblend family ---
  triblend: "#C0BFBF",
  "black triblend": "#2B2B2B",
  "gray triblend": "#A8A8A8",
  "navy triblend": "#2A3148",
  "charcoal triblend": "#3A3A3A",
  "oatmeal triblend": "#E6DCC7",
  "peach triblend": "#F6C8B6",
  "mauve triblend": "#C7A0B5",

  // --- Oddballs / Printful specials ---
  sandshell: "#f0e9d7",
  autumn: "#A64B2A",
  citreon: "#E4D00A",
  citrine: "#E4D00A",
  paprika: "#ef5354",
  chambray: "#cfe5f0",
  'soft pink': "#f5d2d9",
  'blue jean': "#939ba8",
  'ice blue': "#819399",
  citron: "#E4D00A",
  storm: "#7A7F8C",
  sand: "#E2C9A6",
  cream: "#FFFDD0",
  ivory: "#FFFFF0",
  stone: "#D3D3C3",
  clay: "#B66A50",
  rust: "#B7410E",
  denim: "#1560BD",
  sky: "#87CEEB",
  seafoam: "#93E9BE",
  moss: "#8A9A5B",
  cactus: "#5B6F4A",
  toast: "#C08A5B",
  tan: "#D2B48C",
  mocha: "#3C2F2F",
  espresso: "#3B2F2F",
  'sand dune': "#C2B280",
  'charcoalblack triblend': "#181b1a",
}

// --- 2. Normalization ---
function normalize(name: string): string {
  return name
    .trim()
    .toLowerCase()
    .replace(/[^a-z0-9\s/]/g, "")   // keep slash for split colors
    .replace(/\s+/g, " ")
}

// --- Resolve a single color name to a hex ---
function resolveSingleColor(name: string): string {
  const normalized = normalize(name)

  // 1. Override wins
  if (OVERRIDES[normalized]) {
    return OVERRIDES[normalized]
  }

  // 2. Fuzzy match via color-namer
  try {
    const result = namer(normalized)
    const best = result?.basic?.[0]
    if (best?.hex) return best.hex
  } catch {}

  // 3. Fallback
  return "#FFFFFF"
}

// --- 3. Main resolver (returns 1 or more colors) ---
export function resolvePrintfulColor(name: string): string[] {
  console.log("NAME: ", name)
  const normalized = normalize(name)
  
  console.log("NORMD: ", normalized)
  // Multi-color garment: split on "/"
  if (normalized.includes("/")) {
    return normalized
      .split("/")
      .map(part => part.trim())
      .filter(Boolean)
      .map(resolveSingleColor)
  }

  // Single color
  return [resolveSingleColor(normalized)]
}

// --- 4. Debug helper ---
export function debugPrintfulColor(name: string) {
  const colors = resolvePrintfulColor(name)

  return {
    input: name,
    normalized: normalize(name),
    colors,
    count: colors.length,
    source:
      colors.length > 1
        ? "split"
        : OVERRIDES[normalize(name)]
        ? "override"
        : "fuzzy-or-fallback",
  }
}

