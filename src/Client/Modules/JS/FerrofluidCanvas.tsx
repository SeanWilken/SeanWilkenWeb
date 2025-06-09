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

    let mouse = { x: -100, y: -100 }; // start off-canvas

    const handleMouseMove = (e: MouseEvent) => {
      const rect = canvas.getBoundingClientRect();
      mouse.x = e.clientX - rect.left;
      mouse.y = e.clientY - rect.top;
    };

    canvas.addEventListener("mousemove", handleMouseMove);

    let animationFrameId: number;

    const draw = () => {
      // Clear the canvas
      ctx.clearRect(0, 0, canvas.width, canvas.height);

      // Draw background text
      ctx.fillStyle = "rgba(255, 255, 255, 0.05)";
      ctx.font = "bold 120px 'DM Sans', sans-serif";
      ctx.textAlign = "center";
      ctx.fillText("TypeScript Components", canvas.width / 2, 200);

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
