import React, { useEffect, useRef, useState } from "react";
import { Engine, Render, World, Bodies, Events, Body } from "matter-js";

type ElementType = "smoke" | "water" | "slime" | "ball";

const PhysicsSimulation: React.FC = () => {
    const sceneRef = useRef<HTMLDivElement>(null);
    const [engine] = useState(() => {
        const newEngine = Engine.create();
        newEngine.world.gravity.y = 1; // Apply downward gravity
        return newEngine;
    });
    const [isDrawing, setIsDrawing] = useState(false);
    const [gravity, setGravity] = useState(1);
    const [currentType, setCurrentType] = useState<ElementType>("ball");

    useEffect(() => {
        const render = Render.create({
            element: sceneRef.current!,
            engine: engine,
            options: {
                width: window.innerWidth,
                height: window.innerHeight - 100,
                wireframes: false,
                background: "#0f172a",
            },
        });

        World.add(engine.world, [
            Bodies.rectangle(window.innerWidth / 2, window.innerHeight - 110, window.innerWidth, 20, {
                isStatic: true,
                render: { fillStyle: "#1e293b" },
            }),
        ]);

        Engine.run(engine);
        Render.run(render);

        const handleResize = () => {
            render.options.width = window.innerWidth;
            render.options.height = window.innerHeight - 100;
            Render.lookAt(render, { min: { x: 0, y: 0 }, max: { x: window.innerWidth, y: window.innerHeight - 100 } });
        };

        window.addEventListener("resize", handleResize);

        return () => {
            Render.stop(render);
            Engine.clear(engine);
            window.removeEventListener("resize", handleResize);
        };
    }, [engine]);

    useEffect(() => {
        engine.world.gravity.y = gravity;
    }, [gravity, engine]);

    useEffect(() => {
        const update = () => {
            Engine.update(engine);
            requestAnimationFrame(update);
        };
        update();
    }, [engine]);

    useEffect(() => {
        const handleMouseDown = () => setIsDrawing(true);
        const handleMouseUp = () => setIsDrawing(false);

        window.addEventListener("mousedown", handleMouseDown);
        window.addEventListener("mouseup", handleMouseUp);

        return () => {
            window.removeEventListener("mousedown", handleMouseDown);
            window.removeEventListener("mouseup", handleMouseUp);
        };
    }, []);

    // Function to create different shape types
    const createShape = (x: number, y: number, type: ElementType) => {
        let options: any = { render: {}, restitution: 0.8, isStatic: false };

        switch (type) {
            case "smoke":
                options = {
                    ...options,
                    restitution: 0.3,
                    render: { fillStyle: "rgba(200, 200, 200, 0.5)" },
                    density: 0.1,
                    isStatic: false
                };
                break;

            case "water":
                options = {
                    ...options,
                    restitution: 0.2,
                    render: { fillStyle: "blue" },
                    density: 0.8,
                };
                break;

            case "slime":
                options = {
                    ...options,
                    restitution: 0.6,
                    render: { fillStyle: "green" },
                    frictionAir: 0.2,
                };
                break;

            case "ball":
            default:
                options = {
                    ...options,
                    restitution: 0.8,
                    render: { fillStyle: "red" },
                };
        }

        return Bodies.circle(x, y, type === "smoke" ? 10 : 15, options);
    };

    const handleMouseMove = (event: React.MouseEvent) => {
        if (!isDrawing || !sceneRef.current) return;

        const rect = sceneRef.current.getBoundingClientRect();
        const x = event.clientX - rect.left;
        const y = event.clientY - rect.top;

        const shape = createShape(x, y, currentType);
        World.add(engine.world, shape);
    };

    // Apply upward force to smoke
    useEffect(() => {
        Events.on(engine, "afterUpdate", () => {
            engine.world.bodies.forEach((body) => {
                if (body.render.fillStyle === "rgba(200, 200, 200, 0.5)") {
                    Body.applyForce(body, body.position, { x: 0, y: -0.002 });
                }
            });
        });
    }, [engine]);

    // Remove smoke when gravity is negative
    useEffect(() => {
        if (gravity < 0) {
            engine.world.bodies.forEach((body) => {
                if (body.render.fillStyle.startsWith("rgba")) {
                    // Gradually fade before removal
                    let opacity = parseFloat(body.render.fillStyle.split(",")[3]);
                    opacity -= 0.02; // Reduce opacity step-by-step
                    
                    if (opacity <= 0) {
                        World.remove(engine.world, body);
                    } else {
                        body.render.fillStyle = `rgba(200, 200, 200, ${opacity})`;
                    }
                }
            });
        }
    }, [gravity, engine]);

    return (
        <div className="w-full h-screen flex flex-col items-center justify-center bg-gray-900 text-white">
            <div ref={sceneRef} onMouseMove={handleMouseMove} className="w-full h-full cursor-crosshair shadow-lg rounded-lg" />

            {/* UI controls: one row, two columns */}
            <div className="absolute bottom-5 w-full flex justify-center">
                <div className="grid grid-cols-2 gap-8 bg-gray-800/80 p-5 rounded-lg shadow-lg backdrop-blur-lg">
                    {/* Gravity Controls */}
                    <div className="flex flex-col items-center space-y-4">
                        <button className="px-6 py-3 bg-blue-600 rounded-lg shadow-md hover:bg-blue-500 transition-all duration-200" onClick={() => setGravity(gravity + 0.5)}>Increase Gravity</button>
                        <button className="px-6 py-3 bg-red-600 rounded-lg shadow-md hover:bg-red-500 transition-all duration-200" onClick={() => setGravity(gravity - 0.5)}>Decrease Gravity</button>
                        <p className="px-6 py-3 bg-gray-700 rounded-lg shadow-md text-lg font-semibold">Gravity: {gravity.toFixed(1)}</p>
                    </div>

                    {/* Element Selection */}
                    <div className="flex flex-col items-center space-y-4">
                        {["ball", "water", "smoke", "slime"].map((type) => (
                            <button
                                key={type}
                                className={`px-6 py-3 rounded-lg shadow-md ${currentType === type ? "bg-yellow-400" : "bg-gray-600"} hover:bg-gray-500 transition-all`}
                                onClick={() => setCurrentType(type as ElementType)}
                            >
                                {type.toUpperCase()}
                            </button>
                        ))}
                    </div>
                </div>
            </div>
        </div>
    );
};

export default PhysicsSimulation;