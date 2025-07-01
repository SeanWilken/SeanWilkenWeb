import React, { useEffect, useRef } from 'react';

interface TSXHeaderCanvasProps {
  text?: string;
  textColor?: string; // Base color for the text fill (will pulse alpha)
}

function wrapAndDrawStyledText(canvas, ctx, text, centerX, centerY, maxWidth, initialFontSize, minFontSize, textColor, tick) {
  let fontSize = initialFontSize;
  let lines = [];
  let lineHeight;

  // Fit text block by adjusting font size down if needed
  do {
    ctx.font = `bold ${fontSize}px 'DM Sans', sans-serif`;
    const words = text.split(' ');
    lines = [];
    let line = '';

    for (let i = 0; i < words.length; i++) {
      const testLine = line + words[i] + ' ';
      const width = ctx.measureText(testLine).width;
      if (width > maxWidth && line !== '') {
        lines.push(line.trim());
        line = words[i] + ' ';
      } else {
        line = testLine;
      }
    }
    lines.push(line.trim());

    lineHeight = fontSize * 1.25;
    const totalHeight = lines.length * lineHeight;
    if (totalHeight > canvas.height * 0.6) {
      fontSize -= 2;
    } else {
      break;
    }
  } while (fontSize > minFontSize);

  ctx.font = `bold ${fontSize}px 'DM Sans', sans-serif`;

  const pulse = Math.sin(tick * 0.05) * 0.25 + 0.75;
  const totalHeight = lines.length * lineHeight;
  const startY = centerY - totalHeight / 2 + lineHeight / 2;

  ctx.textAlign = "center";
  ctx.textBaseline = "middle";
  ctx.lineWidth = fontSize * 0.08;

  for (let i = 0; i < lines.length; i++) {
    const y = startY + i * lineHeight;

    // Fill text with pulsing alpha
    ctx.fillStyle = `rgba(${textColor}, ${pulse})`;
    ctx.fillText(lines[i], centerX, y);

    // Blue glowing stroke
    ctx.strokeStyle = `rgba(0, 200, 255, ${pulse * 0.6})`;
    ctx.strokeText(lines[i], centerX, y);
  }

  return fontSize; // if you want to reuse it for halo later
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
    canvas.height = window.innerHeight * 0.1; // 10% of viewport height
    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    const allParticles = [];
    const TOTAL_PARTICLES = 60;

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

      // Responsive font size based on canvas width, clamped between 48 and 120 px
      const minFontSize = 32;
      const maxFontSize = 120;
      // Let's say font size scales with canvas width at about 10% of width
      const baseFontSize = canvas.width * 0.1;
      const fontSize = Math.min(maxFontSize, Math.max(minFontSize, baseFontSize));

      // Vertical position roughly 30% from the top
      const textX = canvas.width / 2;
      const textY = canvas.height / 2;

      ctx.font = `bold ${fontSize}px 'DM Sans', sans-serif`;

      // Pulsing alpha for fill color
      const pulse = Math.sin(tick * 0.05) * 0.25 + 0.75; // ranges 0.5 - 1.0

      // Fill text with pulsing alpha
      ctx.fillStyle = `rgba(${textColor}, ${pulse})`;
      ctx.textAlign = "center";
      ctx.textBaseline = "middle";
      const maxTextWidth = canvas.width * 0.85;
      const initialFontSize = canvas.width * 0.1;

      wrapAndDrawStyledText(canvas, ctx, text, textX, textY, maxTextWidth, initialFontSize, minFontSize, textColor, tick);

      // Step 3: Punch blue holes in mask with particles
      ctx.globalCompositeOperation = "destination-out";
      for (const p of allParticles) {
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
        height: "10vh",
        display: "block",
        backgroundColor: "#0f172a",
      }}
    />
  );
};

export default TSXHeaderCanvas;
