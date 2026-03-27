import React, { useEffect, useMemo, useRef, useState } from 'react';

// Types

export type TerminalAppView = 
    | 'Welcome'
    | 'About'
    | 'Skills'
    | 'Resume'
    | 'Portfolio'
    | 'Code'
    | 'Code_Animation'
    | 'Code_GoalRoll'
    | 'Code_PivotPoints'
    | 'Code_SynthNeverSets'
    | 'Code_TerminalBrowser'
    | 'Code_TileSort'
    | 'Code_TileTap'
    | 'Design'
    | 'Contact'
    | 'Shop';

type TerminalMode = 'normal' | 'expanded' | 'game';

type TerminalNodeType =
    | { kind: "directory" }
    | { kind: "file"; language: string }
    | { kind: "route"; appView: TerminalAppView }
    | { kind: "asset"; assetPath: string };

type TerminalNode = {
    name: string;
    fullPath: string;
    nodeType: TerminalNodeType;
    children: TerminalNode[];
    preview?: string;
};

type TerminalEntryKind = 'prompt' | 'output' | 'error' | 'success' | 'hint';

type TerminalEntry = {
    kind: TerminalEntryKind;
    text: string;
}

type TerminalCommand =
    | { type: 'help' }
    | { type: 'pwd' }
    | { type: 'clear' }
    | { type: 'expand' }
    | { type: 'collapse' }
    | { type: 'play' }
    | { type: 'exit' }
    | { type: 'ls'; path?: string }
    | { type: 'cd'; path: string }
    | { type: 'open'; path: string }
    | { type: 'view'; path: string }
    | { type: 'cat'; path: string }
    | { type: 'nav'; target: string }
    | { type: 'unknown'; raw: string };
    
type TerminalAction =
    | { type: 'none' }
    | { type: 'navigate'; appView: TerminalAppView }
    | { type: 'openViewer'; path: string }
    | { type: 'openAsset'; path: string }
    | { type: 'setMode'; mode: TerminalMode }
    | { type: 'startGame' };

type CommandResult = {
    entries: TerminalEntry[];
    newPath?: string[];
    action: TerminalAction;
    clearHistory: boolean;
};

type TerminalState = {
    currentPath: string[];
    input: string;
    history: TerminalEntry[];
    mode: TerminalMode;
    rootNode: TerminalNode;
};

type GameState = {
    room: string;
    hasKey: boolean;
    messages: string[];
};

export type TerminalBrowserProps = {
    onNavigate: (appView: string) => void;
    onOpenViewer: (path: string) => void;
    onOpenAsset: (path: string) => void;
    className?: string;
};

// Helpers

function dir(
    name: string,
    fullPath: string,
    children: TerminalNode[],
    preview?: string
): TerminalNode {
    return {
        name,
        fullPath,
        nodeType: { kind: "directory" },
        children,
        preview
    };
}

function file(
    name: string,
    fullPath: string,
    language: string,
    preview?: string
): TerminalNode {
    return {
        name,
        fullPath,
        nodeType: { kind: "file", language },
        children: [],
        preview
    };
}

function route(
    name: string,
    fullPath: string,
    appView: TerminalAppView,
    preview?: string
): TerminalNode {
    return {
        name,
        fullPath,
        nodeType: { kind: "route", appView },
        children: [],
        preview,
    };
}

function asset(
    name: string,
    fullPath: string,
    assetPath: string,
    preview?: string
): TerminalNode {
    return {
        name,
        fullPath,
        nodeType: { kind: "asset", assetPath },
        children: [],
        preview,
    };
}

function buildTerminalTree(): TerminalNode {
  return dir("/", "/", [
    dir("src", "/src", [
      dir("Components", "/src/Components", [
        dir("FSharp", "/src/Components/FSharp", [
          dir("Welcome", "/src/Components/FSharp/Welcome", [
            route("Welcome.fs", "/src/Components/FSharp/Welcome/Welcome.fs", "Welcome", "Welcome page component"),
          ]),
          dir("About", "/src/Components/FSharp/About", [
            route("About.fs", "/src/Components/FSharp/About/About.fs", "About", "About page component"),
          ]),
          dir("Skills", "/src/Components/FSharp/Skills", [
            route("Skills.fs", "/src/Components/FSharp/Skills/Skills.fs", "Skills", "Skills landing component"),
          ]),
          dir("Contact", "/src/Components/FSharp/Contact", [
            route("Contact.fs", "/src/Components/FSharp/Contact/Contact.fs", "Contact", "Contact page component"),
          ]),
          dir("Portfolio", "/src/Components/FSharp/Portfolio", [
            dir("Art", "/src/Components/FSharp/Portfolio/Art", [
              route(
                "ArtGallery.fs",
                "/src/Components/FSharp/Portfolio/Art/ArtGallery.fs",
                "Design",
                "Art gallery page"
              ),
            ], "Portfolio art and gallery views"),
            dir("Games", "/src/Components/FSharp/Portfolio/Games", [
              dir("GoalRoll", "/src/Components/FSharp/Portfolio/Games/GoalRoll", [
                route(
                  "GoalRoll.fs",
                  "/src/Components/FSharp/Portfolio/Games/GoalRoll/GoalRoll.fs",
                  "Code_GoalRoll",
                  "GoalRoll game page"
                ),
              ]),
              dir("PivotPoints", "/src/Components/FSharp/Portfolio/Games/PivotPoints", [
                route(
                  "PivotPoints.fs",
                  "/src/Components/FSharp/Portfolio/Games/PivotPoints/PivotPoints.fs",
                  "Code_PivotPoints",
                  "PivotPoints game page"
                ),
              ]),
              dir("TileTap", "/src/Components/FSharp/Portfolio/Games/TileTap", [
                route(
                  "TileTap.fs",
                  "/src/Components/FSharp/Portfolio/Games/TileTap/TileTap.fs",
                  "Code_TileTap",
                  "TileTap game page"
                ),
              ]),
              dir("TileSort", "/src/Components/FSharp/Portfolio/Games/TileSort", [
                route(
                  "TileSort.fs",
                  "/src/Components/FSharp/Portfolio/Games/TileSort/TileSort.fs",
                  "Code_TileSort",
                  "TileSort game page"
                ),
              ]),
            ], "F# game portfolio projects"),
            route(
              "PortfolioLanding.fs",
              "/src/Components/FSharp/Portfolio/PortfolioLanding.fs",
              "Portfolio",
              "Portfolio landing page"
            ),
          ], "Portfolio pages and subroutes"),
        ]),

        dir("Typescript", "/src/Components/Typescript", [
          dir("Resume", "/src/Components/Typescript/Resume", [
            route(
              "Resume.tsx",
              "/src/Components/Typescript/Resume/Resume.tsx",
              "Resume",
              "Interactive resume page"
            ),
          ]),
          dir("Demos", "/src/Components/Typescript/Demos", [
            dir("Animations", "/src/Components/Typescript/Demos/Animations", [
              route(
                "Animations.tsx",
                "/src/Components/Typescript/Demos/Animations/Animations.tsx",
                "Code_Animation",
                "TypeScript animation demos"
              ),
            ]),
            dir("SynthNeverSets", "/src/Components/Typescript/Demos/SynthNeverSets", [
              route(
                "SynthNeverSets.tsx",
                "/src/Components/Typescript/Demos/SynthNeverSets/SynthNeverSets.tsx",
                "Code_SynthNeverSets",
                "Disabled / low-quality demo under review"
              ),
            ], "Currently disabled; needs a quality pass"),
            dir("DemosTerminalBrowser", "/src/Components/Typescript/Demos/DemosTerminalBrowser", [
              file(
                "DemosTerminalBrowser.tsx",
                "/src/Components/Typescript/Demos/DemosTerminalBrowser/DemosTerminalBrowser.tsx",
                "Code_TerminalBrowser",
                "Interactive repo terminal/browser component"
              ),
            ]),
          ], "TypeScript bindings and demo components"),
        ]),
      ]),
    ]),
    dir("public", "/public", [
      dir("resume", "/public/resume", [
        asset(
          "Sean-Wilken-Resume.pdf",
          "/public/resume/Sean-Wilken-Resume.pdf",
          "/resume/Sean-Wilken-Resume.pdf",
          "Downloadable PDF resume"
        ),
      ]),
      dir("img", "/public/img", []),
    ]),
    file("README.md", "/README.md", "markdown", "Project readme"),
  ], "Portfolio repo root");
}

function splitPath(path: string): string[] {
    return path.split("/").filter(Boolean);
}

function normalizePath(currentPath: string[], rawPath: string): string[] {
    const parts = rawPath.trim().startsWith("/") ? splitPath(rawPath) : [...currentPath, ...splitPath(rawPath)];
    const output: string[] = [];

    for (const part of parts) {
        if (part === "." || part === "") continue;
        if (part === "..") {
            output.pop();
            continue;
        }
        output.push(part);
    }

    return output;
}

function tryFindNodeBySegments(root: TerminalNode, segments: string[]): TerminalNode | null {
    if (segments.length === 0) return root;

    let node: TerminalNode | null = root;

    for (const segment of segments) {
        if (!node) return null;
        node = node.children.find((child) => child.name === segment) ?? null;
    }

    return node;
}

function tryFindNodeByPath(
    root: TerminalNode,
    currentPath: string[],
    rawPath: string
): { segments: string[]; node: TerminalNode } | null {
    const segments = normalizePath(currentPath, rawPath);
    const node = tryFindNodeBySegments(root, segments);
    return node ? { segments, node } : null;
}

function listNodeNames(node: TerminalNode): string[] {
  return node.children.map((child) => {
    switch (child.nodeType.kind) {
        case "directory":
            return `${child.name}/`;
        case "route":
            return `${child.name} ->`;
        case "asset":
            return `${child.name} *`;
        case "file":
            return child.name;
    }
  });
}

function currentPathString(segments: string[]): string {
    return segments.length === 0 ? "/" : `/${segments.join("/")}`;
}

const routeAliases: Record<string, TerminalAppView> = {
    welcome: "Welcome",
    about: "About",
    skills: "Skills",
    resume: "Resume",
    portfolio: "Portfolio",
    code: "Code",
    animation: "Code_Animation",
    goalRoll: "Code_GoalRoll",
    pivotPoints: "Code_PivotPoints",
    synth: "Code_SynthNeverSets",
    terminal: "Code_TerminalBrowser",
    tileTap: "Code_TileTap",
    tileSort: "Code_TileSort",
    design: "Design",
    contact: "Contact",
    shop: "Shop",
};

function getCurrentDirectoryNode(root: TerminalNode, currentPath: string[]): TerminalNode | null {
    const node = tryFindNodeBySegments(root, currentPath);
    return node && node.nodeType.kind === "directory" ? node : null;
}

function printTree(
    node: TerminalNode,
    currentPath: string[],
    prefix = "",
    isLast = true,
    currentSegments: string[] = []
): string[] {
    const isRoot = node.fullPath === "/";
    const connector = isRoot ? "" : isLast ? "└── " : "├── ";

    const here =
        currentSegments.length === currentPath.length &&
        currentSegments.every((seg, i) => seg === currentPath[i]);

    const label = (() => {
        if (isRoot) return "/";
        switch (node.nodeType.kind) {
        case "directory":
            return `${node.name}/`;
        case "route":
            return `${node.name} ->`;
        case "asset":
            return `${node.name} *`;
        case "file":
            return node.name;
        }
    })();

    const line = `${prefix}${connector}${label}${here ? "  * <- YOU ARE HERE" : ""}`;

    const children = [...node.children].sort((a, b) => {
        const aDir = a.nodeType.kind === "directory" ? 0 : 1;
        const bDir = b.nodeType.kind === "directory" ? 0 : 1;
        if (aDir !== bDir) return aDir - bDir;
        return a.name.localeCompare(b.name);
    });

    const nextPrefix = isRoot ? "" : prefix + (isLast ? "    " : "│   ");

    const lines = [line];

    children.forEach((child, index) => {
        const childIsLast = index === children.length - 1;
        const childSegments = [...currentSegments, child.name];
        lines.push(...printTree(child, currentPath, nextPrefix, childIsLast, childSegments));
    });

    return lines;
}

function getCompletions(
    root: TerminalNode,
    currentPath: string[],
    input: string
): string[] {
    const trimmed = input.trimStart();
    const parts = trimmed.split(/\s+/);
    const cmd = parts[0]?.toLowerCase() ?? "";
    const arg = parts.slice(1).join(" ");

    const cwdNode = getCurrentDirectoryNode(root, currentPath);
    if (!cwdNode) return [];

    const childNames = cwdNode.children.map((child) => child.name);

    if (cmd === "cd" || cmd === "open" || cmd === "view" || cmd === "cat") {
        if (!arg) return childNames;

        return childNames.filter((name) =>
        name.toLowerCase().startsWith(arg.toLowerCase())
        );
    }

    if (cmd === "nav") {
        return Object.keys(routeAliases).filter((name) =>
        name.startsWith(arg.toLowerCase())
        );
    }

    return [];
}

function applyAutocomplete(
    root: TerminalNode,
    currentPath: string[],
    input: string
): string {
    const trimmed = input.trimStart();
    const parts = trimmed.split(/\s+/);
    const cmd = parts[0] ?? "";
    const arg = parts.slice(1).join(" ");

    const completions = getCompletions(root, currentPath, input);
    if (completions.length !== 1) return input;

    const match = completions[0];
    if (!cmd) return input;

    return arg ? `${cmd} ${match}` : `${cmd} ${match}`;
}

function mkEntry(kind: TerminalEntryKind, text: string): TerminalEntry {
    return { kind, text };
}

function parseCommand(input: string): TerminalCommand {
    const trimmed = input.trim();
    const parts = trimmed.split(/\s+/).filter(Boolean);

    if (parts.length === 0) return { type: "unknown", raw: "" };

    const [cmd, ...rest] = parts;
    const arg = rest.join(" ");

    switch (cmd.toLowerCase()) {
        case "help":
            return { type: "help" };
        case "pwd":
            return { type: "pwd" };
        case "clear":
            return { type: "clear" };
        case "expand":
            return { type: "expand" };
        case "collapse":
            return { type: "collapse" };
        case "play":
            return { type: "play" };
        case "exit":
            return { type: "exit" };
        case "ls":
            return { type: "ls", path: arg || undefined };
        case "cd":
            return arg ? { type: "cd", path: arg } : { type: "unknown", raw: trimmed };
        case "open":
            return arg ? { type: "open", path: arg } : { type: "unknown", raw: trimmed };
        case "view":
            return arg ? { type: "view", path: arg } : { type: "unknown", raw: trimmed };
        case "cat":
            return arg ? { type: "cat", path: arg } : { type: "unknown", raw: trimmed };
        case "nav":
            return arg ? { type: "nav", target: arg } : { type: "unknown", raw: trimmed };
        default:
            return { type: "unknown", raw: trimmed };
    }
}

const helpEntries: TerminalEntry[] = [
    mkEntry("output", "Available commands:"),
    mkEntry("output", "help                show available commands"),
    mkEntry("output", "ls [path]           list files or directories"),
    mkEntry("output", "tree [path]         print directory tree"),
    mkEntry("output", "cd <path>           change directory"),
    mkEntry("output", "pwd                 show current path"),
    mkEntry("output", "open <asset>        open asset in current folder or by path"),
    mkEntry("output", "view <file>         open file in viewer"),
    mkEntry("output", "cat <file>          preview file or node info"),
    mkEntry("output", "nav <page>          navigate to a page/component"),
    mkEntry("output", "expand              expand terminal"),
    mkEntry("output", "collapse            collapse terminal"),
    mkEntry("output", "play                launch hidden game"),
    mkEntry("output", "clear               clear terminal history"),
];

function evaluateCommand(state: TerminalState, command: TerminalCommand): CommandResult {
    const root = state.rootNode;
    const cwd = state.currentPath;

    switch (command.type) {
        case "help":
        return { entries: helpEntries, newPath: undefined, action: { type: "none" }, clearHistory: false };

        case "pwd":
            return {
                entries: [mkEntry("output", currentPathString(cwd))],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
            };

        case "ls": {
            const lookup = command.path
                ? tryFindNodeByPath(root, cwd, command.path)
                : (() => {
                    const node = tryFindNodeBySegments(root, cwd);
                    return node ? { segments: cwd, node } : null;
                })();

            if (!lookup) {
                return {
                entries: [mkEntry("error", "Path not found.")],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
                };
            }

            const items = listNodeNames(lookup.node);
            return {
                entries: items.length ? items.map((x) => mkEntry("output", x)) : [mkEntry("output", "(empty)")],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
            };
        }

        case "cd": {
            const lookup = tryFindNodeByPath(root, cwd, command.path);
            if (!lookup) {
                return {
                    entries: [mkEntry("error", "Directory not found.")],
                    newPath: undefined,
                    action: { type: "none" },
                    clearHistory: false,
                };
            }

            if (lookup.node.nodeType.kind !== "directory") {
                return {
                    entries: [mkEntry("error", "Target is not a directory.")],
                    newPath: undefined,
                    action: { type: "none" },
                    clearHistory: false,
                };
            }

            return {
                entries: [mkEntry("success", currentPathString(lookup.segments))],
                newPath: [...lookup.segments],
                action: { type: "none" },
                clearHistory: false,
            };
        }

        case "open": {
            const raw = command.path.trim();

            // First: look in current directory for matching asset/file name
            const cwdNode = tryFindNodeBySegments(root, cwd);
            if (cwdNode && cwdNode.nodeType.kind === "directory") {
                const localMatch = cwdNode.children.find(
                (child) =>
                    child.name.toLowerCase() === raw.toLowerCase() &&
                    child.nodeType.kind === "asset"
                );

                if (localMatch && localMatch.nodeType.kind === "asset") {
                return {
                    entries: [mkEntry("success", `Opening asset ${localMatch.name}...`)],
                    newPath: undefined,
                    action: { type: "openAsset", path: localMatch.nodeType.assetPath },
                    clearHistory: false,
                };
                }
            }

            // Second: allow explicit path lookup
            const lookup = tryFindNodeByPath(root, cwd, raw);
            if (!lookup) {
                return {
                entries: [mkEntry("error", "Asset not found.")],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
                };
            }

            switch (lookup.node.nodeType.kind) {
                case "asset":
                return {
                    entries: [mkEntry("success", `Opening asset ${lookup.node.name}...`)],
                    newPath: undefined,
                    action: { type: "openAsset", path: lookup.node.nodeType.assetPath },
                    clearHistory: false,
                };

                case "route":
                return {
                    entries: [
                    mkEntry("hint", `Use 'nav ${lookup.node.nodeType.appView}' to navigate to this component.`),
                    ],
                    newPath: undefined,
                    action: { type: "none" },
                    clearHistory: false,
                };

                case "file":
                return {
                    entries: [mkEntry("hint", `Use 'view ${lookup.node.name}' to open this file in the viewer.`)],
                    newPath: undefined,
                    action: { type: "none" },
                    clearHistory: false,
                };

                case "directory":
                return {
                    entries: [mkEntry("error", "Cannot open a directory. Use ls or cd.")],
                    newPath: undefined,
                    action: { type: "none" },
                    clearHistory: false,
                };
            }
        }

        case "view": {
            const lookup = tryFindNodeByPath(root, cwd, command.path);
            if (!lookup) {
                return {
                entries: [mkEntry("error", "File not found.")],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
                };
            }

            return {
                entries: [mkEntry("success", `Viewing ${lookup.node.name}...`)],
                newPath: undefined,
                action: { type: "openViewer", path: lookup.node.fullPath },
                clearHistory: false,
            };
        }

        case "cat": {
            const lookup = tryFindNodeByPath(root, cwd, command.path);
            if (!lookup) {
                return {
                entries: [mkEntry("error", "Path not found.")],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
                };
            }

            const node = lookup.node;
            const preview =
                node.preview ??
                (() => {
                switch (node.nodeType.kind) {
                    case "directory":
                        return `${node.fullPath} (directory)`;
                    case "file":
                        return `${node.fullPath} (${node.nodeType.language} file)`;
                    case "route":
                        return `${node.fullPath} (routable page)`;
                    case "asset":
                        return `${node.fullPath} -> ${node.nodeType.assetPath}`;
                }
                })();

            return {
                entries: [mkEntry("output", preview)],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
            };
        }

        case "nav": {
            const raw = command.target.trim().toLowerCase();

            const alias = routeAliases[raw];
            if (alias) {
                return {
                entries: [mkEntry("success", `Navigating to ${raw}...`)],
                newPath: undefined,
                action: { type: "navigate", appView: alias },
                clearHistory: false,
                };
            }

            const cwdNode = tryFindNodeBySegments(root, cwd);
            if (cwdNode && cwdNode.nodeType.kind === "directory") {
                const localRoute = cwdNode.children.find(
                (child) =>
                    child.name.toLowerCase() === raw &&
                    child.nodeType.kind === "route"
                );

                if (localRoute && localRoute.nodeType.kind === "route") {
                return {
                    entries: [mkEntry("success", `Navigating to ${localRoute.name}...`)],
                    newPath: undefined,
                    action: { type: "navigate", appView: localRoute.nodeType.appView },
                    clearHistory: false,
                };
                }
            }

            return {
                entries: [mkEntry("error", "Unknown navigation target.")],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
            };
        }

        case "expand":
            return {
                entries: [mkEntry("success", "Expanded terminal.")],
                newPath: undefined,
                action: { type: "setMode", mode: "expanded" },
                clearHistory: false,
            };

        case "collapse":
            return {
                entries: [mkEntry("success", "Collapsed terminal.")],
                newPath: undefined,
                action: { type: "setMode", mode: "normal" },
                clearHistory: false,
            };

        case "play":
            return {
                entries: [mkEntry("success", "Launching hidden game...")],
                newPath: undefined,
                action: { type: "startGame" },
                clearHistory: false,
            };

        case "exit":
            return {
                entries: [mkEntry("success", "Exited game mode.")],
                newPath: undefined,
                action: { type: "setMode", mode: "normal" },
                clearHistory: false,
            };

        case "clear":
            return {
                entries: [],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: true,
            };

        case "unknown":
            return {
                entries: [
                mkEntry(
                    "error",
                    command.raw.trim() ? `Unknown command: ${command.raw}` : "Enter a command."
                ),
                mkEntry("hint", "Type 'help' to see available commands."),
                ],
                newPath: undefined,
                action: { type: "none" },
                clearHistory: false,
            };
    }
}

/* =========================
   Game
========================= */

const initialGame: GameState = {
    room: "terminal-lab",
    hasKey: false,
    messages: [
        "You wake up in a dimly lit terminal lab.",
        "Commands: look, take key, open door, use key, exit",
    ],
};

function runGameCommand(state: GameState, input: string): GameState {
  const cmd = input.trim().toLowerCase();

  switch (cmd) {
    case "look":
        return {
            ...state,
            messages: [...state.messages, "You see a steel door and a brass key on the floor."],
        };
    case "take key":
        return state.hasKey
            ? { ...state, messages: [...state.messages, "You already picked up the key."] }
            : { ...state, hasKey: true, messages: [...state.messages, "You picked up the brass key."] };
    case "open door":
        return { ...state, messages: [...state.messages, "The door is locked."] };
    case "use key":
        return state.hasKey
            ? {
                ...state,
                messages: [...state.messages, "The key turns. The door opens. You escaped the terminal lab."],
            }
            : { ...state, messages: [...state.messages, "You do not have a key."] };
    case "exit":
        return { ...state, messages: [...state.messages, "Type 'exit' in the main terminal to leave game mode."] };
    default:
        return { ...state, messages: [...state.messages, `Unknown game command: ${cmd}`] };
  }
}

/* =========================
   Component
========================= */

function promptForPath(segments: string[]) {
  return `sean@terminal:${currentPathString(segments)}$`;
}

function entryClass(kind: TerminalEntryKind) {
  switch (kind) {
    case "prompt":
        return "text-primary";
    case "output":
        return "text-base-content/80";
    case "error":
        return "text-error";
    case "success":
        return "text-success";
    case "hint":
        return "text-base-content/50";
  }
}

function shellClass(mode: TerminalMode) {
  switch (mode) {
    case "normal":
      return "w-full max-w-4xl h-[420px]";
    case "expanded":
      return "w-full max-w-7xl h-[75vh]";
    case "game":
      return "w-full max-w-5xl h-[75vh]";
  }
}

export default function PortfolioTerminal({
  onNavigate,
  onOpenViewer,
  className = "",
}: TerminalBrowserProps) {
  const initialState: TerminalState = useMemo(
    () => ({
      currentPath: [],
      input: "",
      history: [
        mkEntry("success", "Interactive portfolio terminal ready."),
        mkEntry("hint", "Type 'help' to get started."),
      ],
      mode: "normal",
      rootNode: buildTerminalTree(),
    }),
    []
  );

  const [state, setState] = useState<TerminalState>(initialState);
  const [gameState, setGameState] = useState<GameState>(initialGame);
  const scrollRef = useRef<HTMLDivElement | null>(null);

  useEffect(() => {
    const el = scrollRef.current;
    if (el) el.scrollTop = el.scrollHeight;
  }, [state.history.length, state.mode, gameState.messages.length]);

  function handleAction(action: TerminalAction) {
    switch (action.type) {
      case "none":
        return;
      case "navigate":
        onNavigate(action.appView);
        return;
      case "openViewer":
        onOpenViewer(action.path);
        return;
      case "openAsset":
        window.open(action.path, "_blank", "noopener,noreferrer");
        return;
      case "setMode":
        setState((s) => ({ ...s, mode: action.mode }));
        return;
      case "startGame":
        setGameState(initialGame);
        setState((s) => ({ ...s, mode: "game" }));
        return;
    }
  }

  function runInput() {
    const raw = state.input.trim();
    if (!raw) return;

    const promptEntry = mkEntry("prompt", `${promptForPath(state.currentPath)} ${raw}`);

    if (state.mode === "game") {
      const nextGame = runGameCommand(gameState, raw);
      const lastMessage = nextGame.messages[nextGame.messages.length - 1];

      setGameState(nextGame);
      setState((s) => ({
        ...s,
        input: "",
        history: [...s.history, promptEntry, mkEntry("output", lastMessage)],
      }));
      return;
    }

    const command = parseCommand(raw);
    const result = evaluateCommand(state, command);

    setState((s) => {
        const baseHistory = 
            result.clearHistory
                ? [mkEntry("success", "Terminal cleared."), mkEntry("hint", "Type 'help' to get started.")]
                : [...s.history, promptEntry, ...result.entries];

        const nextPath: string[] = result.newPath ? [...result.newPath]: s.currentPath;

        return {
            ...s,
            input: "",
            currentPath: nextPath,
            history: baseHistory,
        };
    });

    handleAction(result.action);
  }

  return (
    <div className={`inter-font rounded-2xl border border-base-300/60 bg-base-100 shadow-xl overflow-hidden ${shellClass(state.mode)} ${className}`}>
        <div className="shrink-0 flex items-center justify-between gap-4 px-4 py-3 border-b border-base-300/60 bg-base-200/60">        <div className="flex items-center gap-3">
            <div className="flex gap-2">
                <span className="w-3 h-3 rounded-full bg-error inline-block" />
                <span className="w-3 h-3 rounded-full bg-warning inline-block" />
                <span className="w-3 h-3 rounded-full bg-success inline-block" />
            </div>
            <span className="cormorant-font text-sm md:text-base font-medium text-base-content/80">
                portfolio-terminal
            </span>
        </div>

        <div className="flex items-center gap-2">
            <button
                className="btn btn-sm btn-ghost"
                onClick={() =>
                    setState((s) => ({
                        ...s,
                        mode: s.mode === "expanded" ? "normal" : "expanded",
                    }))
                }
            >
            {state.mode === "expanded" ? "Collapse" : "Expand"}
          </button>

            <button
                className="btn btn-sm btn-ghost"
                onClick={() =>
                    setState((s) => ({
                        ...s,
                        history: [
                        mkEntry("success", "Terminal cleared."),
                        mkEntry("hint", "Type 'help' to get started."),
                        ],
                    }))
                }
            >
                Clear
            </button>
        </div>
    </div>

    <div
        className={
            state.mode === "expanded"
            ? "grid grid-cols-1 lg:grid-cols-[minmax(0,1fr)_340px] h-[calc(100%-53px)]"
            : "h-[calc(100%-53px)]"
        }
    >
        <div className="flex flex-col min-h-0 h-full">
            <div className="px-4 py-2 text-xs md:text-sm text-base-content/50 border-b border-base-300/40">
                {promptForPath(state.currentPath)}
            </div>

        <div ref={scrollRef} className="flex-1 min-h-0 overflow-y-auto px-4 py-4 space-y-2">
            {state.mode === "game"
              ? gameState.messages.map((line, i) => (
                    <div key={`${line}-${i}`} className="text-sm md:text-base text-base-content/80 leading-7">
                            {line}
                    </div>
                ))
                : state.history.map((entry, i) => (
                    <div
                        key={`${entry.text}-${i}`}
                        className={`text-sm md:text-base leading-7 whitespace-pre-wrap ${entryClass(entry.kind)}`}
                    >
                            {entry.text}
                    </div>
                    ))
            }
        </div>

        <form
            className="border-t border-base-300/50 px-4 py-3"
            onSubmit={(e) => {
                e.preventDefault();
                runInput();
            }}
        >
            <div className="flex items-center gap-3">
                <span className="text-sm md:text-base text-primary whitespace-nowrap">
                    {promptForPath(state.currentPath)}
                </span>
                <input
                    className="flex-1 bg-transparent outline-none text-sm md:text-base text-base-content placeholder:text-base-content/35"
                    value={state.input}
                    placeholder={state.mode === "game" ? "game command..." : "type a command..."}
                    autoFocus
                    onChange={(e) => setState((s) => ({ ...s, input: e.target.value }))}
                    onKeyDown={(e) => {
                        if (e.key === "Tab") {
                            e.preventDefault();
                            setState((s) => ({
                            ...s,
                            input: applyAutocomplete(s.rootNode, s.currentPath, s.input),
                            }));
                        }
                    }}
                />
            </div>
        </form>
    </div>
        {state.mode === "expanded" && (
            <div className="hidden lg:flex flex-col border-l border-base-300/50 bg-base-200/30">
                <div className="px-4 py-3 border-b border-base-300/40 text-sm font-medium text-base-content/75">
                    Quick Commands
                </div>
                <div className="p-4 space-y-2 text-sm md:text-base">
                {["help", "ls", "cd src/Client/Pages", "open resume", "view Welcome.tsx", "play"].map((cmd) => (
                    <button
                        key={cmd}
                        className="btn btn-sm btn-ghost justify-start w-full"
                        onClick={() => setState((s) => ({ ...s, input: cmd }))}
                    >
                        {cmd}
                    </button>
                ))}
                </div>
            </div>
        )}
    </div>    
</div>);
}