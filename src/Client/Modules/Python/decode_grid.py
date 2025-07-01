import requests
import re

def fetch_and_print_grid(doc_url: str) -> None:
    try:
        print("Fetching and printing grid from Google Docs...")

        response = requests.get(doc_url)
        response.raise_for_status()

        text = response.text
        lines = text.split('\n')

        # Regex to match lines like: Character: █, X: 0, Y: 0
        pattern = re.compile(r"Character:\s(.),\s*X:\s*(\d+),\s*Y:\s*(\d+)")

        points = []

        for line in lines:
            match = pattern.match(line)
            if match:
                char, x_str, y_str = match.groups()
                points.append({
                    "char": char,
                    "x": int(x_str),
                    "y": int(y_str)
                })

        if not points:
            print("No character data found in the document.")
            return

        max_x = max(p["x"] for p in points)
        max_y = max(p["y"] for p in points)

        # Initialize grid with spaces
        grid = [[" " for _ in range(max_x + 1)] for _ in range(max_y + 1)]

        # Place characters on the grid
        for p in points:
            grid[p["y"]][p["x"]] = p["char"]

        # Print the grid line by line
        for row in grid:
            print("".join(row))

    except requests.RequestException as e:
        print(f"Network error: {e}")

        print(f"Error: {e}")

# Example usage:
if __name__ == "__main__":
    url = "https://docs.google.com/document/d/e/2PACX-1vTER-wL5E8YC9pxDx43gk8eIds59GtUUk4nJo_ZWagbnrH0NFvMXIw6VWFLpf5tWTZIT9P9oLIoFJ6A/pub"
    fetch_and_print_grid(url)
