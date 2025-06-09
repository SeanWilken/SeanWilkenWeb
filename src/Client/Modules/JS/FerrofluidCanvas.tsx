import React, { useEffect, useRef } from 'react';

const FerrofluidCanvas: React.FC = () => {
  const canvasRef = useRef<HTMLCanvasElement | null>(null);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    canvas.width = window.innerWidth;
    canvas.height = 300;

    const ctx = canvas.getContext("2d");
    if (!ctx) return;

    let mouse = { x: -100, y: -100 }; // Start off-canvas

    const handleMouseMove = (e: MouseEvent) => {
      const rect = canvas.getBoundingClientRect();
      mouse.x = e.clientX - rect.left;
      mouse.y = e.clientY - rect.top;
    };

    canvas.addEventListener("mousemove", handleMouseMove);

    let animationFrameId: number;

    let lastMousePos = { x: 0, y: 0 };
    let pulseTimer = 0;

    const draw = () => {
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        // Define text bounds
        const text = "TypeScript Components";
        const textX = canvas.width / 2;
        const textY = 200;
        const textMetrics = ctx.measureText(text);
        const textWidth = textMetrics.width;

        // Check if the mouse is stationary
        const isStationary = mouse.x === lastMousePos.x && mouse.y === lastMousePos.y;
        if (isStationary) {
            pulseTimer += 0.05; // Increment pulse timer
        } else {
            pulseTimer = 0; // Reset pulse if moving
        }
        lastMousePos = { ...mouse }; // Update last mouse position

        // Calculate distance from text
        const leftEdge = textX - textWidth / 2;
        const rightEdge = textX + textWidth / 2;
        const dx = Math.max(leftEdge - mouse.x, mouse.x - rightEdge, 0);
        const dy = Math.abs(mouse.y - textY);
        const distance = Math.sqrt(dx * dx + dy * dy);

        // Glow intensity calculation with pulsing effect
        const baseIntensity = Math.max(5, 60 - distance * 0.6);
        const pulseEffect = isStationary ? Math.sin(pulseTimer) * 10 : 0;
        const glowIntensity = baseIntensity + pulseEffect;

        // Draw glowing text
        ctx.fillStyle = "rgba(255, 255, 255, 0.05)";
        ctx.font = "bold 120px 'DM Sans', sans-serif";
        ctx.textAlign = "center";

        ctx.strokeStyle = `rgba(0, 200, 255, ${glowIntensity / 100})`;  
        ctx.lineWidth = glowIntensity / 10; // Reduce stroke thickness  
        ctx.strokeText(text, textX, textY);
        ctx.fillText(text, textX, textY);

        // Draw moving blob
        ctx.beginPath();
        ctx.arc(mouse.x, mouse.y, 40, 0, 2 * Math.PI);
        ctx.fillStyle = "rgba(0, 200, 255, 0.4)";
        ctx.fill();

        animationFrameId = requestAnimationFrame(draw);
    };

    draw();

    return () => {
      canvas.removeEventListener("mousemove", handleMouseMove);
      cancelAnimationFrame(animationFrameId);
    };
  }, []);

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

export default FerrofluidCanvas;