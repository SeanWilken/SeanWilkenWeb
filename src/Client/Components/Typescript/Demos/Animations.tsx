import React, { useEffect, useMemo, useRef, useState } from "react";
import {
  motion,
  useScroll,
  useTransform,
  useSpring,
} from "framer-motion";

// =============================================================================
// Daisy theme helpers (with safe fallbacks)
// =============================================================================
const cssVar = (name: string) =>
  getComputedStyle(document.documentElement).getPropertyValue(name).trim();

/**
 * Daisy stores vars like "--p" as: "262 80% 60%"
 * This returns: "hsl(262 80% 60% / 0.6)" or fallback if missing.
 */
export const hslDaisy = (varName: string, fallbackHsl: string, alpha = 1) => {
  const v = cssVar(varName);
  return v ? `hsl(${v} / ${alpha})` : `hsl(${fallbackHsl} / ${alpha})`;
};

// =============================================================================
// Small util: stable randoms so stars don't re-randomize every render
// =============================================================================
const makeStars = (count: number, maxSize: number) =>
  Array.from({ length: count }, () => ({
    left: Math.random() * 100,
    top: Math.random() * 100,
    size: Math.random() * maxSize + 1,
    opacity: Math.random() * 0.6 + 0.2,
  }));

// =============================================================================
// PARALLAX SCROLL DEMO - Container-friendly (still 200vh demo)
// =============================================================================
export const ParallaxScrollDemo: React.FC = () => {
  const containerRef = useRef<HTMLDivElement>(null);
  const { scrollYProgress } = useScroll({
    target: containerRef,
    offset: ["start start", "end start"],
  });

  const y1 = useTransform(scrollYProgress, [0, 1], ["0%", "100%"]);
  const y2 = useTransform(scrollYProgress, [0, 1], ["0%", "50%"]);
  const y3 = useTransform(scrollYProgress, [0, 1], ["0%", "25%"]);
  const y4 = useTransform(scrollYProgress, [0, 1], ["0%", "10%"]);

  const bgStars = useMemo(() => makeStars(55, 1), []);
  const fgStars = useMemo(() => makeStars(35, 3), []);

  const bg = useMemo(
    () =>
      `radial-gradient(1200px 800px at 30% 10%, ${hslDaisy(
        "--p",
        "262 80% 60%",
        0.18
      )} 0%, transparent 60%),
       radial-gradient(900px 700px at 70% 30%, ${hslDaisy(
         "--a",
         "180 80% 55%",
         0.16
       )} 0%, transparent 55%),
       ${hslDaisy("--b1", "255 10% 10%", 1)}`,
    []
  );

  return (
    <div
      ref={containerRef}
      className="relative h-[200vh] overflow-hidden"
      style={{ background: bg }}
    >
      <div className="sticky top-0 h-screen overflow-hidden">
        {/* Background stars - slowest */}
        <motion.div style={{ y: y4 }} className="absolute inset-0 opacity-40">
          {bgStars.map((s, i) => (
            <div
              key={i}
              className="absolute rounded-full"
              style={{
                left: `${s.left}%`,
                top: `${s.top}%`,
                width: `${s.size}px`,
                height: `${s.size}px`,
                opacity: s.opacity,
                background: hslDaisy("--bc", "0 0% 100%", 0.9),
              }}
            />
          ))}
        </motion.div>

        {/* Nebula layers */}
        <motion.div
          // style={{ y: y3 }}
          className="absolute top-16 left-1/4 w-96 h-96 rounded-full blur-[110px]"
          style={{
            y: y3,
            background: `radial-gradient(circle, ${hslDaisy(
              "--p",
              "262 80% 60%",
              0.22
            )} 0%, transparent 70%)`,
          }}
        />
        <motion.div
          // style={{ y: y2 }}
          className="absolute top-1/3 right-1/4 w-80 h-80 rounded-full blur-[90px]"
          style={{
            y: y2,
            background: `radial-gradient(circle, ${hslDaisy(
              "--a",
              "180 80% 55%",
              0.24
            )} 0%, transparent 70%)`,
          }}
        />

        {/* Foreground stars - fast */}
        <motion.div style={{ y: y1 }} className="absolute inset-0 opacity-80">
          {fgStars.map((s, i) => (
            <div
              key={i}
              className="absolute rounded-full"
              style={{
                left: `${s.left}%`,
                top: `${s.top}%`,
                width: `${s.size}px`,
                height: `${s.size}px`,
                opacity: s.opacity,
                background: hslDaisy("--bc", "0 0% 100%", 0.95),
              }}
            />
          ))}
        </motion.div>

        <div className="relative z-10 flex items-center justify-center h-full">
          <h1 className="text-6xl font-bold tracking-tight text-base-content">
            Parallax Depth
          </h1>
        </div>
      </div>
    </div>
  );
};

// =============================================================================
// MOUSE FOLLOW DEMO - container-friendly, theme-aware
// =============================================================================
export const MouseFollowDemo: React.FC = () => {
  const [mousePosition, setMousePosition] = useState({ x: 0, y: 0 });
  const containerRef = useRef<HTMLDivElement>(null);

  const springConfig = { damping: 25, stiffness: 150 };
  const x = useSpring(mousePosition.x, springConfig);
  const y = useSpring(mousePosition.y, springConfig);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const handleMouseMove = (e: MouseEvent) => {
      const rect = container.getBoundingClientRect();
      setMousePosition({
        x: e.clientX - rect.left,
        y: e.clientY - rect.top,
      });
    };

    container.addEventListener("mousemove", handleMouseMove);
    return () => container.removeEventListener("mousemove", handleMouseMove);
  }, []);

  const bg = useMemo(
    () =>
      `radial-gradient(1000px 700px at 30% 20%, ${hslDaisy(
        "--a",
        "180 80% 55%",
        0.16
      )} 0%, transparent 60%),
       radial-gradient(900px 600px at 70% 70%, ${hslDaisy(
         "--p",
         "262 80% 60%",
         0.18
       )} 0%, transparent 55%),
       ${hslDaisy("--b1", "255 10% 10%", 1)}`,
    []
  );

  return (
    <div
      ref={containerRef}
      className="relative h-screen overflow-hidden"
      style={{ background: bg }}
    >
      <motion.div
        style={{ x, y }}
        className="absolute w-64 h-64 -translate-x-1/2 -translate-y-1/2 pointer-events-none"
      >
        <div
          className="w-full h-full rounded-full blur-3xl"
          style={{
            background: `radial-gradient(circle, ${hslDaisy(
              "--a",
              "180 80% 55%",
              0.35
            )} 0%, ${hslDaisy("--p", "262 80% 60%", 0.25)} 55%, transparent 70%)`,
          }}
        />
      </motion.div>

      <div className="relative z-10 flex items-center justify-center h-full gap-8">
        {[1, 2, 3].map((i) => (
          <motion.div
            key={i}
            className="w-48 h-64 backdrop-blur-xl rounded-2xl border shadow-2xl p-6"
            style={{
              background: hslDaisy("--b1", "255 10% 10%", 0.25),
              borderColor: hslDaisy("--bc", "0 0% 100%", 0.18),
              color: hslDaisy("--bc", "0 0% 100%", 0.92),
            }}
            whileHover={{
              scale: 1.05,
              borderColor: hslDaisy("--bc", "0 0% 100%", 0.35),
            }}
          >
            <div className="text-6xl mb-4">✨</div>
            <h3 className="text-xl font-semibold mb-2">Card {i}</h3>
            <p style={{ color: hslDaisy("--bc", "0 0% 100%", 0.7) }}>
              Hover and move your mouse around
            </p>
          </motion.div>
        ))}
      </div>
    </div>
  );
};

// =============================================================================
// GPU 3D TRANSFORM DEMO - theme-aware, keeps perspective inline
// =============================================================================
export const GPU3DTransformDemo: React.FC = () => {
  const [rotation, setRotation] = useState({ x: 0, y: 0 });
  const cardRef = useRef<HTMLDivElement>(null);

  const handleMouseMove = (e: React.MouseEvent<HTMLDivElement>) => {
    if (!cardRef.current) return;

    const rect = cardRef.current.getBoundingClientRect();
    const mx = e.clientX - rect.left;
    const my = e.clientY - rect.top;

    const centerX = rect.width / 2;
    const centerY = rect.height / 2;

    const rotateX = ((my - centerY) / centerY) * -18;
    const rotateY = ((mx - centerX) / centerX) * 18;

    setRotation({ x: rotateX, y: rotateY });
  };

  const handleMouseLeave = () => setRotation({ x: 0, y: 0 });

  const bg = useMemo(
    () =>
      `radial-gradient(900px 600px at 50% 20%, ${hslDaisy(
        "--p",
        "262 80% 60%",
        0.14
      )} 0%, transparent 60%),
       ${hslDaisy("--b1", "255 8% 7%", 1)}`,
    []
  );

  const cards = [
    { gradient: ["--p", "262 80% 60%"], emoji: "🌟" },
    { gradient: ["--a", "180 80% 55%"], emoji: "🔥" },
    { gradient: ["--s", "330 85% 60%"], emoji: "💎" },
  ] as const;

  return (
    <div className="h-screen flex items-center justify-center p-8" style={{ background: bg }}>
      <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
        {cards.map((card, i) => {
          const isInteractive = i === 1;
          const grad = `linear-gradient(135deg, ${hslDaisy(
            card.gradient[0],
            card.gradient[1],
            0.95
          )}, ${hslDaisy("--p", "262 80% 60%", 0.85)})`;

          return (
            <div
              key={i}
              ref={isInteractive ? cardRef : null}
              className="relative w-64 h-80"
              onMouseMove={isInteractive ? handleMouseMove : undefined}
              onMouseLeave={isInteractive ? handleMouseLeave : undefined}
              style={{ perspective: "1000px" }}
            >
              <motion.div
                className="w-full h-full rounded-3xl shadow-2xl p-8 relative overflow-hidden"
                style={{
                  background: grad,
                  transformStyle: "preserve-3d",
                  willChange: "transform",
                }}
                animate={{
                  rotateX: isInteractive ? rotation.x : 0,
                  rotateY: isInteractive ? rotation.y : 0,
                }}
                transition={{ type: "spring", stiffness: 300, damping: 20 }}
              >
                <div
                  className="absolute inset-0 opacity-0 hover:opacity-100 transition-opacity"
                  style={{
                    transform: "translateZ(20px)",
                    background:
                      "linear-gradient(45deg, rgba(255,255,255,0), rgba(255,255,255,0.25), rgba(255,255,255,0))",
                  }}
                />
                <div
                  className="relative"
                  style={{ transform: "translateZ(50px)", color: "white" }}
                >
                  <div className="text-7xl mb-4">{card.emoji}</div>
                  <h3 className="text-2xl font-bold mb-2">3D Transform</h3>
                  <p className="text-sm opacity-80">GPU accelerated rotation</p>
                </div>
              </motion.div>
            </div>
          );
        })}
      </div>
    </div>
  );
};

// =============================================================================
// PARTICLE FIELD BACKGROUND (container-sized via ResizeObserver)
// Export this for usage as a background layer in pages.
// =============================================================================
export const ParticleFieldBackground: React.FC<{
  className?: string;
  count?: number;
  interactionRadius?: number;
  linkDistance?: number;
  trailAlpha?: number;
}> = ({
  className,
  count = 110,
  interactionRadius = 150,
  linkDistance = 110,
  trailAlpha = 0.07,
}) => {
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const rafRef = useRef<number | null>(null);

  const particlesRef = useRef<
    Array<{ x: number; y: number; vx: number; vy: number; size: number }>
  >([]);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const parent = canvas.parentElement as HTMLElement | null;
    if (!parent) return;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const getSize = () => {
      const r = parent.getBoundingClientRect();
      return {
        w: Math.max(1, Math.floor(r.width)),
        h: Math.max(1, Math.floor(r.height)),
      };
    };

    let { w, h } = getSize();
    canvas.width = w;
    canvas.height = h;

    const initParticles = () => {
      particlesRef.current = Array.from({ length: count }, () => ({
        x: Math.random() * canvas.width,
        y: Math.random() * canvas.height,
        vx: (Math.random() - 0.5) * 1.6,
        vy: (Math.random() - 0.5) * 1.6,
        size: Math.random() * 2.5 + 0.8,
      }));
    };

    initParticles();

    const ro = new ResizeObserver(() => {
      const s = getSize();
      canvas.width = s.w;
      canvas.height = s.h;
      // keep particles if possible
      if (particlesRef.current.length !== count) initParticles();
    });
    ro.observe(parent);

    let mouseX = canvas.width / 2;
    let mouseY = canvas.height / 2;
    const onMove = (e: MouseEvent) => {
      const r = parent.getBoundingClientRect();
      mouseX = e.clientX - r.left;
      mouseY = e.clientY - r.top;
    };
    parent.addEventListener("mousemove", onMove);

    const particleColor = () => hslDaisy("--p", "262 80% 60%", 0.65);
    const linkColor = (alpha: number) => hslDaisy("--bc", "0 0% 100%", alpha);

    const tick = () => {
      // soft trail
      ctx.fillStyle = `rgba(0, 0, 0, ${trailAlpha})`;
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      const ps = particlesRef.current;

      for (let i = 0; i < ps.length; i++) {
        const p = ps[i];

        p.x += p.vx;
        p.y += p.vy;

        // mouse interaction (repel)
        const dx = mouseX - p.x;
        const dy = mouseY - p.y;
        const dist = Math.sqrt(dx * dx + dy * dy);

        if (dist > 0.001 && dist < interactionRadius) {
          const force = (interactionRadius - dist) / interactionRadius;
          p.vx -= (dx / dist) * force * 0.35;
          p.vy -= (dy / dist) * force * 0.35;
        }

        // bounce
        if (p.x < 0 || p.x > canvas.width) p.vx *= -1;
        if (p.y < 0 || p.y > canvas.height) p.vy *= -1;

        // damping
        p.vx *= 0.988;
        p.vy *= 0.988;

        // draw particle
        ctx.beginPath();
        ctx.arc(p.x, p.y, p.size, 0, Math.PI * 2);
        ctx.fillStyle = particleColor();
        ctx.fill();

        // draw connections
        for (let j = i + 1; j < ps.length; j++) {
          const q = ps[j];
          const ddx = q.x - p.x;
          const ddy = q.y - p.y;
          const d = Math.sqrt(ddx * ddx + ddy * ddy);

          if (d < linkDistance) {
            ctx.beginPath();
            ctx.moveTo(p.x, p.y);
            ctx.lineTo(q.x, q.y);
            ctx.strokeStyle = linkColor(0.22 * (1 - d / linkDistance));
            ctx.lineWidth = 0.6;
            ctx.stroke();
          }
        }
      }

      rafRef.current = requestAnimationFrame(tick);
    };

    rafRef.current = requestAnimationFrame(tick);

    return () => {
      ro.disconnect();
      parent.removeEventListener("mousemove", onMove);
      if (rafRef.current) cancelAnimationFrame(rafRef.current);
    };
  }, [count, interactionRadius, linkDistance, trailAlpha]);

  return (
    <canvas
      ref={canvasRef}
      className={["absolute inset-0 w-full h-full", className].filter(Boolean).join(" ")}
      style={{ pointerEvents: "none" }}
    />
  );
};

// =============================================================================
// PARTICLE SYSTEM DEMO (uses the background component)
// =============================================================================
export const ParticleSystemDemo: React.FC = () => {
  const bg = useMemo(
    () =>
      `radial-gradient(1200px 800px at 30% 10%, ${hslDaisy(
        "--p",
        "262 80% 60%",
        0.15
      )} 0%, transparent 60%),
       ${hslDaisy("--b1", "0 0% 0%", 1)}`,
    []
  );

  return (
    <div className="relative h-screen overflow-hidden" style={{ background: bg }}>
      <ParticleFieldBackground className="opacity-90" />
      <div className="relative z-10 flex items-center justify-center h-full">
        <div className="text-center">
          <h1 className="text-6xl font-bold mb-4 text-base-content">Particle System</h1>
          <p className="text-xl text-base-content/70">Move your mouse to interact</p>
        </div>
      </div>
    </div>
  );
};

// =============================================================================
// MAGNETIC (reusable wrapper) + MagneticButton
// =============================================================================
export const Magnetic: React.FC<{
  children: React.ReactNode;
  className?: string;
  strength?: number;
}> = ({ children, className, strength = 0.3 }) => {
  const ref = useRef<HTMLDivElement>(null);
  const [pos, setPos] = useState({ x: 0, y: 0 });

  const onMove = (e: React.MouseEvent<HTMLDivElement>) => {
    const el = ref.current;
    if (!el) return;
    const r = el.getBoundingClientRect();
    const x = e.clientX - r.left - r.width / 2;
    const y = e.clientY - r.top - r.height / 2;
    setPos({ x: x * strength, y: y * strength });
  };

  const onLeave = () => setPos({ x: 0, y: 0 });

  return (
    <motion.div
      ref={ref}
      className={className}
      onMouseMove={onMove}
      onMouseLeave={onLeave}
      animate={{ x: pos.x, y: pos.y }}
      transition={{ type: "spring", stiffness: 180, damping: 16 }}
      style={{ display: "inline-block" }}
    >
      {children}
    </motion.div>
  );
};

export const MagneticButton: React.FC<{
  children: React.ReactNode;
  gradientClassName?: string;
  className?: string;
}> = ({ children, gradientClassName, className }) => {
  const gradient =
    gradientClassName ??
    `bg-gradient-to-r from-[${hslDaisy("--p", "262 80% 60%", 1)}] to-[${hslDaisy(
      "--a",
      "180 80% 55%",
      1
    )}]`;

  // NOTE: Tailwind can't parse dynamic class strings like from-[hsl(...)] reliably.
  // So we apply a theme-aware inline background as the default.
  const inlineBg =
    gradientClassName == null
      ? {
          background: `linear-gradient(90deg, ${hslDaisy(
            "--p",
            "262 80% 60%",
            0.95
          )}, ${hslDaisy("--a", "180 80% 55%", 0.95)})`,
        }
      : undefined;

  return (
    <Magnetic className="inline-block">
      <motion.button
        className={[
          "relative px-12 py-6 text-2xl font-bold text-white rounded-full shadow-2xl overflow-hidden",
          gradientClassName ?? "",
          className ?? "",
        ].join(" ")}
        style={inlineBg}
        whileHover={{ scale: 1.05 }}
        whileTap={{ scale: 0.95 }}
      >
        <span className="relative z-10">{children}</span>
        <motion.div
          className="absolute inset-0 bg-white"
          initial={{ opacity: 0 }}
          whileHover={{ opacity: 0.18 }}
        />
      </motion.button>
    </Magnetic>
  );
};

// =============================================================================
// MAGNETIC ELEMENTS DEMO
// =============================================================================
export const MagneticElementsDemo: React.FC = () => {
  const bg = useMemo(
    () =>
      `radial-gradient(1200px 800px at 40% 20%, ${hslDaisy(
        "--p",
        "262 80% 60%",
        0.16
      )} 0%, transparent 65%),
       radial-gradient(900px 700px at 70% 80%, ${hslDaisy(
         "--a",
         "180 80% 55%",
         0.14
       )} 0%, transparent 60%),
       ${hslDaisy("--b1", "255 10% 10%", 1)}`,
    []
  );

  return (
    <div className="h-screen flex items-center justify-center gap-12" style={{ background: bg }}>
      {[
        { label: "Hover Me" },
        { label: "Magnetic" },
        { label: "Buttons" },
      ].map((b, i) => (
        <MagneticButton key={i}>{b.label}</MagneticButton>
      ))}
    </div>
  );
};

// =============================================================================
// SHADER GRADIENT BACKGROUND (reusable, container-safe)
// =============================================================================
export const ShaderGradientBackground: React.FC<{
  className?: string;
  intensity?: number;
}> = ({ className, intensity = 0.65 }) => {
  const [t, setT] = useState(0);

  useEffect(() => {
    let raf = 0;
    const loop = () => {
      setT((x) => x + 0.01);
      raf = requestAnimationFrame(loop);
    };
    raf = requestAnimationFrame(loop);
    return () => cancelAnimationFrame(raf);
  }, []);

  const g1x = 50 + Math.sin(t) * 28;
  const g1y = 50 + Math.cos(t * 0.7) * 28;
  const g2x = 50 + Math.sin(t * 1.3) * 28;
  const g2y = 50 + Math.cos(t * 0.9) * 28;

  const c1 = hslDaisy("--p", "262 80% 60%", 0.9);
  const c2 = hslDaisy("--a", "180 80% 55%", 0.85);
  const c3 = hslDaisy("--s", "330 85% 60%", 0.75);

  return (
    <div className={["absolute inset-0 overflow-hidden", className].filter(Boolean).join(" ")}>
      <motion.div
        className="absolute w-[800px] h-[800px] rounded-full blur-[120px]"
        style={{
          left: `${g1x}%`,
          top: `${g1y}%`,
          opacity: intensity,
          transform: "translate(-50%, -50%)",
          background: `radial-gradient(circle, ${c1} 0%, transparent 70%)`,
        }}
        animate={{ left: `${g1x}%`, top: `${g1y}%` }}
        transition={{ duration: 0.2 }}
      />
      <motion.div
        className="absolute w-[650px] h-[650px] rounded-full blur-[110px]"
        style={{
          left: `${g2x}%`,
          top: `${g2y}%`,
          opacity: intensity * 0.9,
          transform: "translate(-50%, -50%)",
          background: `radial-gradient(circle, ${c2} 0%, transparent 70%)`,
        }}
        animate={{ left: `${g2x}%`, top: `${g2y}%` }}
        transition={{ duration: 0.2 }}
      />
      <div
        className="absolute w-[720px] h-[720px] rounded-full blur-[115px]"
        style={{
          left: "30%",
          top: "30%",
          opacity: intensity * 0.75,
          transform: "translate(-50%, -50%)",
          background: `radial-gradient(circle, ${c3} 0%, transparent 70%)`,
        }}
      />
    </div>
  );
};

// =============================================================================
// SHADER GRADIENT DEMO (uses background component)
// =============================================================================
export const ShaderGradientDemo: React.FC = () => {
  return (
    <div className="h-screen relative overflow-hidden" style={{ background: hslDaisy("--b1", "0 0% 0%", 1) }}>
      <ShaderGradientBackground intensity={0.6} />
      <div className="relative z-10 flex items-center justify-center h-full">
        <div className="text-center">
          <h1 className="text-7xl font-bold text-base-content mb-4 tracking-tight">
            Shader-Style
          </h1>
          <h2 className="text-7xl font-bold text-base-content tracking-tight">
            Gradients
          </h2>
          <p className="text-2xl text-base-content/70 mt-8">Theme-aware animation</p>
        </div>
      </div>
    </div>
  );
};

// =============================================================================
// MAIN SHOWCASE COMPONENT (default export)
// Added: onBack + initialIndex
// =============================================================================
export default function DemoShowcase(props: { onBack?: () => void; initialIndex?: number }) {
  const [activeDemo, setActiveDemo] = useState<number>(props.initialIndex ?? 0);

  const demos = [
    { name: "Parallax Scroll", component: ParallaxScrollDemo },
    { name: "Particle System", component: ParticleSystemDemo },
    { name: "Shader Gradients", component: ShaderGradientDemo },
    // { name: "Mouse Follow", component: MouseFollowDemo },
    // { name: "3D Transform", component: GPU3DTransformDemo },
    // { name: "Magnetic Elements", component: MagneticElementsDemo },
  ];

  const ActiveComponent = demos[activeDemo].component;

  return (
    <div className="relative w-full h-screen" style={{ background: hslDaisy("--b1", "0 0% 0%", 1) }}>
      <ActiveComponent />

      {/* Nav */}
      <div className="fixed bottom-8 left-1/2 -translate-x-1/2 z-50">
        <div
          className="flex gap-3 rounded-full px-6 py-4 border"
          style={{
            background: hslDaisy("--b1", "0 0% 0%", 0.75),
            borderColor: hslDaisy("--bc", "0 0% 100%", 0.12),
            backdropFilter: "blur(16px)",
          }}
        >
          {demos.map((demo, index) => (
            <button
              key={index}
              onClick={() => setActiveDemo(index)}
              className="px-6 py-3 rounded-full font-medium transition-all"
              style={
                activeDemo === index
                  ? { background: hslDaisy("--bc", "0 0% 100%", 0.95), color: "black" }
                  : { color: hslDaisy("--bc", "0 0% 100%", 0.7) }
              }
            >
              {demo.name}
            </button>
          ))}

          {props.onBack && (
            <button
              onClick={props.onBack}
              className="px-6 py-3 rounded-full font-medium transition-all"
              style={{
                background: hslDaisy("--p", "262 80% 60%", 0.35),
                color: hslDaisy("--bc", "0 0% 100%", 0.9),
                border: `1px solid ${hslDaisy("--bc", "0 0% 100%", 0.12)}`,
              }}
            >
              Back
            </button>
          )}
        </div>
      </div>

      {/* Instructions */}
      <div className="fixed top-8 right-8 z-50 text-right">
        <div
          className="rounded-2xl px-6 py-4 border"
          style={{
            background: hslDaisy("--b1", "0 0% 0%", 0.55),
            borderColor: hslDaisy("--bc", "0 0% 100%", 0.12),
            backdropFilter: "blur(16px)",
          }}
        >
          <p style={{ color: hslDaisy("--bc", "0 0% 100%", 0.9) }} className="text-sm font-medium">
            Interactive Demos
          </p>
          <p style={{ color: hslDaisy("--bc", "0 0% 100%", 0.5) }} className="text-xs mt-1">
            Click navigation below to switch
          </p>
        </div>
      </div>
    </div>
  );
}
