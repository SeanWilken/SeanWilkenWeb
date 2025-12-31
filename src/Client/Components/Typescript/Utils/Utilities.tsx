import { oklch } from "culori"; 

export function getCssVar(name) {
    return getComputedStyle(document.documentElement)
        .getPropertyValue(name)
        .trim();
}

export function convertOklchToHex(value) { 
    try { return oklch(value).hex; } 
    catch { return value; } 
}