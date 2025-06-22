import React, { useEffect, useRef } from 'react';

interface TSXHeaderCanvasProps {
  text?: string;
  textColor?: string; // Base color for the text fill (will pulse alpha)
}

const TSXHeaderCanvas: React.FC<TSXHeaderCanvasProps> = ({
  text = "TypeScript Components",
  textColor = "255, 255, 255", // white as RGB string for easier rgba usage
}) => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    canvas.width = window.innerWidth;
    canvas.height = 300;
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const allParticles = [];
    const TOTAL_PARTICLES = 150;

    for (let i = 0; i < TOTAL_PARTICLES; i++) {
      allParticles.push({
        x: Math.random() * canvas.width,
        y: Math.random() * canvas.height,
        vx: (Math.random() - 0.5) * 1.2,
        vy: (Math.random() - 0.5) * 1.2,
        size: Math.random() * 3 + 2,
        phase: Math.random() * 100,
      });
    }

    let tick = 0;
    let animationFrameId: number;

    const draw = () => {
      tick += 1;
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Step 1: Draw text with pulsing fill and blue glow stroke
      const pulse = Math.sin(tick * 0.05) * 0.25 + 0.75; // 0.5 - 1.0
      const textX = canvas.width / 2;
      const textY = 200;
      ctx.font = "bold 120px 'DM Sans', sans-serif";
      ctx.textAlign = "center";

      // Fill with pulsing alpha based on provided textColor (expects "r, g, b")
      ctx.fillStyle = `rgba(${textColor}, ${pulse})`;
      ctx.fillText(text, textX, textY);

      // Blue glowing stroke (fixed color)
      ctx.lineWidth = 10;
      ctx.strokeStyle = `rgba(0, 200, 255, ${pulse * 0.6})`;
      ctx.strokeText(text, textX, textY);

      // Step 3: Punch blue holes in mask with particles
      ctx.globalCompositeOperation = "destination-out";
      for (const p of allParticles) {
        // Move & bounce
        p.x += p.vx;
        p.y += p.vy;

        if (p.x < 0 || p.x > canvas.width) {
          p.vx *= -1;
          p.x = Math.max(0, Math.min(canvas.width, p.x));
        }
        if (p.y < 0 || p.y > canvas.height) {
          p.vy *= -1;
          p.y = Math.max(0, Math.min(canvas.height, p.y));
        }

        const holeSize = p.size * 2;
        ctx.beginPath();
        ctx.arc(p.x, p.y, holeSize, 0, Math.PI * 2);
        ctx.fill();
      }

      // Step 4: Draw blue glowing particle halos
      ctx.globalCompositeOperation = "lighter";
      for (const p of allParticles) {
        const glow = Math.sin(tick * 0.05 + p.phase) * 0.5 + 0.5;
        const radius = p.size * 3 + glow * 6;

        const gradient = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, radius);
        gradient.addColorStop(0, `rgba(0, 200, 255, ${0.4 * glow})`);
        gradient.addColorStop(1, "rgba(0, 200, 255, 0)");

        ctx.fillStyle = gradient;
        ctx.beginPath();
        ctx.arc(p.x, p.y, radius, 0, Math.PI * 2);
        ctx.fill();
      }

      ctx.globalCompositeOperation = "source-over";
      animationFrameId = requestAnimationFrame(draw);
    };

    draw();

    return () => {
      cancelAnimationFrame(animationFrameId);
    };
  }, [text, textColor]);

  return (
    <canvas
      ref={canvasRef}
      style={{
        width: "100%",
        height: "300px",
        display: "block",
        backgroundColor: "#0f172a",
      }}
    />
  );
};

export default TSXHeaderCanvas;
