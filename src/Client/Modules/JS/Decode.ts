const fetch = (...args) => import('node-fetch').then(mod => mod.default(...args));

async function fetchAndPrintGrid(docUrl) {
  try {
    console.log("Fetching and printing grid from Google Docs...");

    const response = await fetch(docUrl);
    if (!response.ok) throw new Error(`Failed to fetch: ${response.status}`);
    console.log("Grid fetched successfully.");

    const text = await response.text();
    const lines = text.split('\n');
    const regex = /Character:\s(.),\s*X:\s*(\d+),\s*Y:\s*(\d+)/;

    const points = [];

    for (const line of lines) {
      const match = line.match(regex);
      if (match) {
        const [, char, xStr, yStr] = match;
        points.push({
          char,
          x: parseInt(xStr, 10),
          y: parseInt(yStr, 10),
        });
      }
    }

    const maxX = Math.max(...points.map(p => p.x));
    const maxY = Math.max(...points.map(p => p.y));

    const grid = Array.from({ length: maxY + 1 }, () =>
      Array.from({ length: maxX + 1 }, () => ' ')
    );

    for (const { x, y, char } of points) {
      grid[y][x] = char;
    }

    for (const row of grid) {
      console.log(row.join(''));
    }
  } catch (err) {
    console.error('Error:', err);
  }
}

console.log("Fetching and printing grid from Google Docs...");

fetchAndPrintGrid("https://docs.google.com/document/d/e/2PACX-1vRMx5YQlZNa3ra8dYYxmv-QIQ3YJe8tbI3kqcuC7lQiZm-CSEznKfN_HYNSpoXcZIV3Y_O3YoUB1ecq/pub");

console.log("Grid fetched successfully.");