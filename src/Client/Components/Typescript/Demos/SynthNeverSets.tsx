import React, { useState, useEffect, useRef } from 'react';

export const SynthNeverSets = () => {
  const canvasRef = useRef(null);
  const gameStateRef = useRef({
    // Player state
    playerX: 0,
    targetX: 0,
    playerVelocityX: 0,
    hasShield: false,
    
    // Road state
    roadZ: 0,
    speed: 0,
    
    // Road segments for 3D effect
    roadSegments: [],
    
    // Game state
    distance: 0,
    gameOver: false,
    score: 0,
    
    // Obstacles
    obstacles: [],
    powerups: [],
    
    // Camera shake
    shakeAmount: 0,
    
    // Mouse position
    mouseX: 0,
  });

  const [gameState, setGameState] = useState('menu');
  const [finalScore, setFinalScore] = useState(0);

  useEffect(() => {
    const canvas = canvasRef.current;
    if (!canvas) return;

    const ctx = canvas.getContext('2d');
    const state = gameStateRef.current;

    // Set canvas size
    const resizeCanvas = () => {
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight;
    };
    resizeCanvas();
    window.addEventListener('resize', resizeCanvas);

    // Mouse controls
    const handleMouseMove = (e) => {
      const rect = canvas.getBoundingClientRect();
      // Convert mouse X to -1 to 1 range
      state.mouseX = ((e.clientX - rect.left) / rect.width) * 2 - 1;
    };

    canvas.addEventListener('mousemove', handleMouseMove);

    // Game constants
    const SEGMENT_LENGTH = 200;
    const ROAD_WIDTH = 2000;
    const CAMERA_HEIGHT = 1000;
    const CAMERA_DEPTH = 1 / Math.tan(((90 - 60) / 2) * Math.PI / 180);
    const DRAW_DISTANCE = 300;
    const FOG_DENSITY = 5;

    // Initialize road segments
    if (state.roadSegments.length === 0) {
      for (let i = 0; i < DRAW_DISTANCE; i++) {
        state.roadSegments.push({
          index: i,
          z: i * SEGMENT_LENGTH,
          curve: 0,
          color: i % 3 === 0 ? '#1a1a2e' : '#16161f',
        });
      }
    }

    // Helper function for 3D projection
    const project = (x, y, z, cameraX, cameraY, cameraZ) => {
      const scale = CAMERA_DEPTH / (z - cameraZ);
      const projX = (x - cameraX) * scale + canvas.width / 2;
      const projY = (CAMERA_HEIGHT - y - cameraY) * scale + canvas.height / 2;
      const projW = scale * ROAD_WIDTH;
      return { x: projX, y: projY, w: projW, scale };
    };

    // Spawn obstacles
    const spawnObstacle = () => {
      const types = ['pothole', 'debris', 'car', 'slowcar'];
      const type = types[Math.floor(Math.random() * types.length)];
      const xOffset = (Math.random() - 0.5) * ROAD_WIDTH * 0.7;

      state.obstacles.push({
        type,
        x: xOffset,
        z: state.roadZ + SEGMENT_LENGTH * DRAW_DISTANCE * 0.8,
        speed: type === 'slowcar' ? 100 : 0,
        hit: false,
      });
    };

    const spawnPowerup = () => {
      const xOffset = (Math.random() - 0.5) * ROAD_WIDTH * 0.7;

      state.powerups.push({
        type: 'shield',
        x: xOffset,
        z: state.roadZ + SEGMENT_LENGTH * DRAW_DISTANCE * 0.8,
      });
    };

    // Game loop
    let lastSpawn = 0;
    let lastPowerupSpawn = 0;
    let animationId;

    const gameLoop = () => {
      if (gameState !== 'playing') {
        return;
      }

      // Clear canvas with gradient sky
      const skyGradient = ctx.createLinearGradient(0, 0, 0, canvas.height * 0.6);
      skyGradient.addColorStop(0, '#1a0033');
      skyGradient.addColorStop(0.3, '#2d1b4e');
      skyGradient.addColorStop(0.6, '#4a2472');
      skyGradient.addColorStop(1, '#6b3fa0');
      ctx.fillStyle = skyGradient;
      ctx.fillRect(0, 0, canvas.width, canvas.height);

      // Apply camera shake
      if (state.shakeAmount > 0) {
        ctx.save();
        ctx.translate(
          (Math.random() - 0.5) * state.shakeAmount,
          (Math.random() - 0.5) * state.shakeAmount
        );
        state.shakeAmount *= 0.9;
      }

      // Draw stars
      const numStars = 100;
      for (let i = 0; i < numStars; i++) {
        const x = (i * 137.5) % canvas.width;
        const y = ((i * 73.3) % (canvas.height * 0.5));
        const twinkle = Math.sin(Date.now() * 0.001 + i) * 0.5 + 0.5;
        ctx.fillStyle = `rgba(255, 255, 255, ${twinkle * 0.8})`;
        ctx.fillRect(x, y, 2, 2);
      }

      // Draw sun
      const sunY = canvas.height * 0.3;
      const sunRadius = 100;
      const sunGradient = ctx.createRadialGradient(
        canvas.width / 2, sunY, sunRadius * 0.3,
        canvas.width / 2, sunY, sunRadius * 1.5
      );
      sunGradient.addColorStop(0, '#ff006e');
      sunGradient.addColorStop(0.4, '#ff4d9e');
      sunGradient.addColorStop(0.7, 'rgba(255, 0, 110, 0.3)');
      sunGradient.addColorStop(1, 'rgba(255, 0, 110, 0)');
      ctx.fillStyle = sunGradient;
      ctx.beginPath();
      ctx.arc(canvas.width / 2, sunY, sunRadius * 1.5, 0, Math.PI * 2);
      ctx.fill();

      // Draw sun core
      ctx.fillStyle = '#ffffff';
      ctx.shadowColor = '#ff006e';
      ctx.shadowBlur = 30;
      ctx.beginPath();
      ctx.arc(canvas.width / 2, sunY, sunRadius * 0.4, 0, Math.PI * 2);
      ctx.fill();
      ctx.shadowBlur = 0;

      // Draw palm trees on sides
      const numTrees = 20;
      for (let i = 0; i < numTrees; i++) {
        const z = (i / numTrees) * SEGMENT_LENGTH * DRAW_DISTANCE + state.roadZ;
        const projLeft = project(-ROAD_WIDTH * 0.8, 0, z, state.playerX, 0, state.roadZ);
        const projRight = project(ROAD_WIDTH * 0.8, 0, z, state.playerX, 0, state.roadZ);

        if (projLeft.y > 0 && projLeft.y < canvas.height) {
          const treeHeight = 150 * projLeft.scale;
          const treeWidth = 30 * projLeft.scale;
          const opacity = Math.max(0, Math.min(1, 1 - (z - state.roadZ) / (SEGMENT_LENGTH * DRAW_DISTANCE)));

          // Left tree
          ctx.fillStyle = `rgba(64, 32, 80, ${opacity})`;
          ctx.fillRect(projLeft.x - treeWidth / 2, projLeft.y - treeHeight, treeWidth, treeHeight);
          ctx.fillStyle = `rgba(128, 0, 128, ${opacity})`;
          for (let j = 0; j < 5; j++) {
            ctx.beginPath();
            ctx.moveTo(projLeft.x, projLeft.y - treeHeight + j * 20 * projLeft.scale);
            ctx.lineTo(projLeft.x - 40 * projLeft.scale, projLeft.y - treeHeight + (j * 20 + 30) * projLeft.scale);
            ctx.stroke();
          }

          // Right tree
          ctx.fillStyle = `rgba(64, 32, 80, ${opacity})`;
          ctx.fillRect(projRight.x - treeWidth / 2, projRight.y - treeHeight, treeWidth, treeHeight);
          ctx.fillStyle = `rgba(128, 0, 128, ${opacity})`;
          for (let j = 0; j < 5; j++) {
            ctx.beginPath();
            ctx.moveTo(projRight.x, projRight.y - treeHeight + j * 20 * projRight.scale);
            ctx.lineTo(projRight.x + 40 * projRight.scale, projRight.y - treeHeight + (j * 20 + 30) * projRight.scale);
            ctx.stroke();
          }
        }
      }

      // Update player position based on mouse
      state.targetX = state.mouseX * ROAD_WIDTH * 0.4;
      const dx = state.targetX - state.playerX;
      state.playerVelocityX = dx * 0.1;
      state.playerX += state.playerVelocityX;

      // Check for oversteer (too fast movement)
      const oversteer = Math.abs(state.playerVelocityX) > 50;

      // Update speed
      state.speed = 200 + Math.floor(state.distance / 1000) * 20;
      state.roadZ += state.speed;
      state.distance += state.speed;
      state.score = Math.floor(state.distance / 100);

      // Spawn obstacles
      if (Date.now() - lastSpawn > 1500 - Math.min(state.score, 1000)) {
        spawnObstacle();
        lastSpawn = Date.now();
      }

      // Spawn powerups
      if (Date.now() - lastPowerupSpawn > 10000 && !state.hasShield) {
        spawnPowerup();
        lastPowerupSpawn = Date.now();
      }

      // Draw road segments
      const horizon = canvas.height * 0.5;
      let maxy = canvas.height;

      for (let n = DRAW_DISTANCE - 1; n >= 0; n--) {
        const segment = state.roadSegments[n];
        segment.z = (n * SEGMENT_LENGTH) + (state.roadZ % SEGMENT_LENGTH);

        const p1 = project(-ROAD_WIDTH / 2, 0, segment.z, state.playerX, 0, state.roadZ);
        const p2 = project(ROAD_WIDTH / 2, 0, segment.z, state.playerX, 0, state.roadZ);

        if (p1.y >= horizon && p1.y <= maxy) {
          // Draw road segment
          ctx.fillStyle = segment.color;
          ctx.beginPath();
          
          if (n < DRAW_DISTANCE - 1) {
            const nextSegment = state.roadSegments[n + 1];
            const nextZ = (n + 1) * SEGMENT_LENGTH + (state.roadZ % SEGMENT_LENGTH);
            const p3 = project(-ROAD_WIDTH / 2, 0, nextZ, state.playerX, 0, state.roadZ);
            const p4 = project(ROAD_WIDTH / 2, 0, nextZ, state.playerX, 0, state.roadZ);

            ctx.moveTo(p1.x, p1.y);
            ctx.lineTo(p3.x, p3.y);
            ctx.lineTo(p4.x, p4.y);
            ctx.lineTo(p2.x, p2.y);
          } else {
            ctx.moveTo(p1.x, p1.y);
            ctx.lineTo(p1.x, maxy);
            ctx.lineTo(p2.x, maxy);
            ctx.lineTo(p2.x, p2.y);
          }
          
          ctx.closePath();
          ctx.fill();

          // Draw lane markers
          if (n % 3 === 0) {
            const laneMarkerWidth = p2.w * 0.05;
            for (let lane = 1; lane <= 3; lane++) {
              const laneX = -ROAD_WIDTH / 2 + (lane / 4) * ROAD_WIDTH;
              const markerProj = project(laneX, 0, segment.z, state.playerX, 0, state.roadZ);
              
              ctx.fillStyle = `rgba(0, 255, 255, ${0.8 * (1 - n / DRAW_DISTANCE)})`;
              ctx.fillRect(markerProj.x - laneMarkerWidth / 2, markerProj.y, laneMarkerWidth, p1.y - p2.y + 5);
            }
          }

          // Draw road edge glow
          ctx.strokeStyle = `rgba(255, 0, 200, ${0.6 * (1 - n / DRAW_DISTANCE)})`;
          ctx.lineWidth = 4;
          ctx.beginPath();
          ctx.moveTo(p1.x, p1.y);
          if (n < DRAW_DISTANCE - 1) {
            const nextZ = (n + 1) * SEGMENT_LENGTH + (state.roadZ % SEGMENT_LENGTH);
            const p3 = project(-ROAD_WIDTH / 2, 0, nextZ, state.playerX, 0, state.roadZ);
            ctx.lineTo(p3.x, p3.y);
          } else {
            ctx.lineTo(p1.x, maxy);
          }
          ctx.stroke();

          ctx.beginPath();
          ctx.moveTo(p2.x, p2.y);
          if (n < DRAW_DISTANCE - 1) {
            const nextZ = (n + 1) * SEGMENT_LENGTH + (state.roadZ % SEGMENT_LENGTH);
            const p4 = project(ROAD_WIDTH / 2, 0, nextZ, state.playerX, 0, state.roadZ);
            ctx.lineTo(p4.x, p4.y);
          } else {
            ctx.lineTo(p2.x, maxy);
          }
          ctx.stroke();

          maxy = p1.y;
        }
      }

      // Update and draw obstacles
      state.obstacles = state.obstacles.filter(obs => {
        obs.z -= obs.speed;
        
        if (obs.z < state.roadZ - SEGMENT_LENGTH) return false;
        if (obs.z > state.roadZ + SEGMENT_LENGTH * DRAW_DISTANCE) return true;

        const obsProj = project(obs.x, 0, obs.z, state.playerX, 0, state.roadZ);
        
        if (obsProj.y > horizon && obsProj.y < canvas.height) {
          const size = 100 * obsProj.scale;

          // Collision detection
          const distanceToPlayer = obs.z - state.roadZ;
          const lateralDistance = Math.abs(obs.x - state.playerX);

          if (distanceToPlayer < 500 && lateralDistance < 200) {
            if (obs.type === 'pothole' && lateralDistance < 100) {
              // Centered on pothole, no damage
            } else {
              if (state.hasShield) {
                state.hasShield = false;
                state.shakeAmount = 20;
                obs.hit = true;
                return false;
              } else {
                state.gameOver = true;
                setGameState('gameover');
                setFinalScore(state.score);
                return false;
              }
            }
          }

          // Draw obstacles
          if (obs.type === 'pothole') {
            ctx.fillStyle = '#000000';
            ctx.strokeStyle = '#ff0080';
            ctx.lineWidth = 3 * obsProj.scale;
            ctx.shadowColor = '#ff0080';
            ctx.shadowBlur = 10;
            ctx.beginPath();
            ctx.ellipse(obsProj.x, obsProj.y, size * 0.8, size * 0.4, 0, 0, Math.PI * 2);
            ctx.fill();
            ctx.stroke();
            ctx.shadowBlur = 0;
          } else if (obs.type === 'debris') {
            ctx.fillStyle = '#ff4400';
            ctx.strokeStyle = '#ffaa00';
            ctx.lineWidth = 2 * obsProj.scale;
            ctx.shadowColor = '#ff4400';
            ctx.shadowBlur = 15;
            ctx.save();
            ctx.translate(obsProj.x, obsProj.y);
            ctx.rotate(state.roadZ * 0.01);
            ctx.fillRect(-size / 2, -size / 2, size, size);
            ctx.strokeRect(-size / 2, -size / 2, size, size);
            ctx.restore();
            ctx.shadowBlur = 0;
          } else if (obs.type === 'car' || obs.type === 'slowcar') {
            const carWidth = size * 0.6;
            const carHeight = size * 1.2;
            
            // Car body gradient
            const carGradient = ctx.createLinearGradient(
              obsProj.x - carWidth / 2, obsProj.y,
              obsProj.x + carWidth / 2, obsProj.y
            );
            carGradient.addColorStop(0, '#00ffff');
            carGradient.addColorStop(0.5, '#0088ff');
            carGradient.addColorStop(1, '#00ffff');
            ctx.fillStyle = carGradient;
            ctx.shadowColor = '#00ffff';
            ctx.shadowBlur = 10;
            ctx.fillRect(obsProj.x - carWidth / 2, obsProj.y - carHeight / 2, carWidth, carHeight);
            ctx.shadowBlur = 0;
            
            // Windows
            ctx.fillStyle = '#001133';
            ctx.fillRect(obsProj.x - carWidth * 0.35, obsProj.y - carHeight * 0.3, carWidth * 0.7, carHeight * 0.25);
            
            // Tail lights
            ctx.fillStyle = '#ff0040';
            ctx.shadowColor = '#ff0040';
            ctx.shadowBlur = 10;
            ctx.fillRect(obsProj.x - carWidth * 0.4, obsProj.y + carHeight * 0.3, carWidth * 0.2, carHeight * 0.1);
            ctx.fillRect(obsProj.x + carWidth * 0.2, obsProj.y + carHeight * 0.3, carWidth * 0.2, carHeight * 0.1);
            ctx.shadowBlur = 0;
          }
        }

        return true;
      });

      // Update and draw powerups
      state.powerups = state.powerups.filter(powerup => {
        if (powerup.z < state.roadZ - SEGMENT_LENGTH) return false;
        if (powerup.z > state.roadZ + SEGMENT_LENGTH * DRAW_DISTANCE) return true;

        const powerupProj = project(powerup.x, 50, powerup.z, state.playerX, 0, state.roadZ);
        
        if (powerupProj.y > horizon && powerupProj.y < canvas.height) {
          const size = 60 * powerupProj.scale;

          // Collision detection
          const distanceToPlayer = powerup.z - state.roadZ;
          const lateralDistance = Math.abs(powerup.x - state.playerX);

          if (distanceToPlayer < 500 && lateralDistance < 200) {
            state.hasShield = true;
            return false;
          }

          // Draw shield powerup with pulsing animation
          const pulse = Math.sin(Date.now() * 0.005) * 0.3 + 0.7;
          ctx.strokeStyle = `rgba(0, 255, 255, ${pulse})`;
          ctx.fillStyle = `rgba(0, 255, 255, ${0.2 * pulse})`;
          ctx.lineWidth = 4 * powerupProj.scale;
          ctx.shadowColor = '#00ffff';
          ctx.shadowBlur = 30;
          ctx.beginPath();
          ctx.arc(powerupProj.x, powerupProj.y, size, 0, Math.PI * 2);
          ctx.fill();
          ctx.stroke();
          
          // Shield symbol
          ctx.strokeStyle = `rgba(255, 255, 255, ${pulse})`;
          ctx.lineWidth = 3 * powerupProj.scale;
          ctx.beginPath();
          ctx.moveTo(powerupProj.x, powerupProj.y - size * 0.6);
          ctx.lineTo(powerupProj.x - size * 0.5, powerupProj.y);
          ctx.lineTo(powerupProj.x - size * 0.3, powerupProj.y + size * 0.5);
          ctx.lineTo(powerupProj.x, powerupProj.y + size * 0.6);
          ctx.lineTo(powerupProj.x + size * 0.3, powerupProj.y + size * 0.5);
          ctx.lineTo(powerupProj.x + size * 0.5, powerupProj.y);
          ctx.closePath();
          ctx.stroke();
          ctx.shadowBlur = 0;
        }

        return true;
      });

      // Draw player car
      const playerProj = project(state.playerX, 0, state.roadZ + 500, state.playerX, 0, state.roadZ);
      const carWidth = 70;
      const carHeight = 120;

      // Shield effect
      if (state.hasShield) {
        const shieldPulse = Math.sin(Date.now() * 0.01) * 0.3 + 0.7;
        ctx.strokeStyle = `rgba(0, 255, 255, ${shieldPulse})`;
        ctx.fillStyle = `rgba(0, 255, 255, ${0.1 * shieldPulse})`;
        ctx.lineWidth = 5;
        ctx.shadowColor = '#00ffff';
        ctx.shadowBlur = 40;
        ctx.beginPath();
        ctx.arc(playerProj.x, playerProj.y, carWidth * 1.2, 0, Math.PI * 2);
        ctx.fill();
        ctx.stroke();
        ctx.shadowBlur = 0;
      }

      // Oversteer effect (tire marks)
      if (oversteer) {
        ctx.strokeStyle = 'rgba(255, 255, 255, 0.4)';
        ctx.lineWidth = 5;
        ctx.beginPath();
        ctx.moveTo(playerProj.x - carWidth / 2, playerProj.y + carHeight / 2);
        ctx.lineTo(playerProj.x - carWidth / 2 - state.playerVelocityX * 2, playerProj.y + carHeight / 2 + 80);
        ctx.stroke();
        ctx.beginPath();
        ctx.moveTo(playerProj.x + carWidth / 2, playerProj.y + carHeight / 2);
        ctx.lineTo(playerProj.x + carWidth / 2 - state.playerVelocityX * 2, playerProj.y + carHeight / 2 + 80);
        ctx.stroke();
      }

      // Car body
      const carGradient = ctx.createLinearGradient(
        playerProj.x - carWidth / 2, playerProj.y,
        playerProj.x + carWidth / 2, playerProj.y
      );
      carGradient.addColorStop(0, '#ff00ff');
      carGradient.addColorStop(0.5, '#ff0080');
      carGradient.addColorStop(1, '#ff00ff');
      ctx.fillStyle = carGradient;
      ctx.shadowColor = '#ff00ff';
      ctx.shadowBlur = 30;
      ctx.fillRect(playerProj.x - carWidth / 2, playerProj.y - carHeight / 2, carWidth, carHeight);
      ctx.shadowBlur = 0;

      // Windshield
      ctx.fillStyle = '#000033';
      ctx.fillRect(playerProj.x - carWidth * 0.4, playerProj.y - carHeight * 0.35, carWidth * 0.8, carHeight * 0.3);
      
      // Headlights
      ctx.fillStyle = '#ffffff';
      ctx.shadowColor = '#ffffff';
      ctx.shadowBlur = 30;
      ctx.fillRect(playerProj.x - carWidth * 0.4, playerProj.y - carHeight * 0.48, carWidth * 0.25, carHeight * 0.08);
      ctx.fillRect(playerProj.x + carWidth * 0.15, playerProj.y - carHeight * 0.48, carWidth * 0.25, carHeight * 0.08);
      
      // Headlight beams
      ctx.fillStyle = 'rgba(255, 255, 255, 0.1)';
      ctx.beginPath();
      ctx.moveTo(playerProj.x - carWidth * 0.25, playerProj.y - carHeight * 0.48);
      ctx.lineTo(playerProj.x - carWidth * 0.5, playerProj.y - canvas.height);
      ctx.lineTo(playerProj.x + carWidth * 0.5, playerProj.y - canvas.height);
      ctx.lineTo(playerProj.x + carWidth * 0.25, playerProj.y - carHeight * 0.48);
      ctx.closePath();
      ctx.fill();
      ctx.shadowBlur = 0;

      if (state.shakeAmount > 0) {
        ctx.restore();
      }

      // Draw UI
      ctx.fillStyle = '#ffffff';
      ctx.font = 'bold 42px "Courier New", monospace';
      ctx.textAlign = 'right';
      ctx.shadowColor = '#ff00ff';
      ctx.shadowBlur = 15;
      ctx.fillText(`${state.score}`, canvas.width - 50, 70);
      ctx.shadowBlur = 0;

      ctx.font = '24px "Courier New", monospace';
      ctx.fillText(`${Math.floor(state.distance)}m`, canvas.width - 50, 110);

      if (state.hasShield) {
        ctx.fillStyle = '#00ffff';
        ctx.font = 'bold 28px "Courier New", monospace';
        ctx.shadowColor = '#00ffff';
        ctx.shadowBlur = 20;
        ctx.fillText('🛡 SHIELD', canvas.width - 50, 150);
        ctx.shadowBlur = 0;
      }

      // Speed indicator
      ctx.fillStyle = '#ff00ff';
      ctx.font = '22px "Courier New", monospace';
      ctx.textAlign = 'left';
      ctx.fillText(`SPEED: ${Math.floor(state.speed)}`, 50, canvas.height - 50);

      animationId = requestAnimationFrame(gameLoop);
    };

    if (gameState === 'playing') {
      gameLoop();
    }

    return () => {
      window.removeEventListener('resize', resizeCanvas);
      canvas.removeEventListener('mousemove', handleMouseMove);
      if (animationId) {
        cancelAnimationFrame(animationId);
      }
    };
  }, [gameState]);

  const startGame = () => {
    const state = gameStateRef.current;
    state.playerX = 0;
    state.targetX = 0;
    state.playerVelocityX = 0;
    state.hasShield = false;
    state.roadZ = 0;
    state.speed = 0;
    state.distance = 0;
    state.gameOver = false;
    state.score = 0;
    state.obstacles = [];
    state.powerups = [];
    state.shakeAmount = 0;
    state.mouseX = 0;
    setGameState('playing');
  };

  return (
    <div style={{
      width: '100vw',
      height: '100vh',
      margin: 0,
      padding: 0,
      overflow: 'hidden',
      fontFamily: '"Courier New", monospace',
      position: 'relative',
    }}>
      <canvas
        ref={canvasRef}
        style={{
          display: 'block',
          width: '100%',
          height: '100%',
        }}
      />

      {gameState === 'menu' && (
        <div style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'linear-gradient(180deg, #1a0033 0%, #4a2472 100%)',
          color: '#ffffff',
        }}>
          <h1 style={{
            fontSize: '72px',
            fontWeight: 'bold',
            margin: 0,
            textShadow: '0 0 20px #ff00ff, 0 0 40px #ff00ff, 0 0 60px #ff00ff',
            letterSpacing: '8px',
            background: 'linear-gradient(90deg, #ff00ff, #00ffff, #ff00ff)',
            WebkitBackgroundClip: 'text',
            WebkitTextFillColor: 'transparent',
            animation: 'glow 2s ease-in-out infinite',
          }}>
            NEON HIGHWAY
          </h1>

          <p style={{
            fontSize: '20px',
            marginTop: '20px',
            color: '#ff00ff',
            textShadow: '0 0 10px #ff00ff',
          }}>
            OUTRUN THE NIGHT
          </p>

          <div style={{
            marginTop: '60px',
            textAlign: 'center',
            fontSize: '16px',
            color: '#00ffff',
            lineHeight: '1.8',
          }}>
            <p>🖱 MOVE MOUSE to STEER</p>
            <p>⚠ FAST MOVEMENTS = OVERSTEER</p>
            <p>🛡 COLLECT SHIELDS for PROTECTION</p>
            <p>🎯 CENTER OVER POTHOLES to AVOID DAMAGE</p>
            <p>💀 ONE HIT = GAME OVER</p>
          </div>

          <button
            onClick={startGame}
            style={{
              marginTop: '60px',
              padding: '20px 60px',
              fontSize: '32px',
              fontWeight: 'bold',
              fontFamily: '"Courier New", monospace',
              color: '#ffffff',
              background: 'linear-gradient(90deg, #ff00ff, #ff0080)',
              border: '3px solid #00ffff',
              borderRadius: '0',
              cursor: 'pointer',
              textShadow: '0 0 10px #ff00ff',
              boxShadow: '0 0 20px #ff00ff, 0 0 40px #ff00ff',
              transition: 'all 0.3s',
            }}
            onMouseEnter={(e) => {
              e.target.style.transform = 'scale(1.1)';
              e.target.style.boxShadow = '0 0 30px #ff00ff, 0 0 60px #ff00ff';
            }}
            onMouseLeave={(e) => {
              e.target.style.transform = 'scale(1)';
              e.target.style.boxShadow = '0 0 20px #ff00ff, 0 0 40px #ff00ff';
            }}
          >
            START GAME
          </button>
        </div>
      )}

      {gameState === 'gameover' && (
        <div style={{
          position: 'absolute',
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          display: 'flex',
          flexDirection: 'column',
          alignItems: 'center',
          justifyContent: 'center',
          background: 'rgba(0, 0, 0, 0.9)',
          color: '#ffffff',
        }}>
          <h1 style={{
            fontSize: '64px',
            fontWeight: 'bold',
            margin: 0,
            color: '#ff0040',
            textShadow: '0 0 20px #ff0040, 0 0 40px #ff0040',
            letterSpacing: '8px',
          }}>
            CRASHED
          </h1>

          <p style={{
            fontSize: '48px',
            marginTop: '40px',
            color: '#00ffff',
            textShadow: '0 0 10px #00ffff',
          }}>
            SCORE: {finalScore}
          </p>

          <button
            onClick={startGame}
            style={{
              marginTop: '60px',
              padding: '20px 60px',
              fontSize: '28px',
              fontWeight: 'bold',
              fontFamily: '"Courier New", monospace',
              color: '#ffffff',
              background: 'linear-gradient(90deg, #ff00ff, #ff0080)',
              border: '3px solid #00ffff',
              borderRadius: '0',
              cursor: 'pointer',
              textShadow: '0 0 10px #ff00ff',
              boxShadow: '0 0 20px #ff00ff',
              transition: 'all 0.3s',
            }}
            onMouseEnter={(e) => {
              e.target.style.transform = 'scale(1.1)';
              e.target.style.boxShadow = '0 0 30px #ff00ff';
            }}
            onMouseLeave={(e) => {
              e.target.style.transform = 'scale(1)';
              e.target.style.boxShadow = '0 0 20px #ff00ff';
            }}
          >
            TRY AGAIN
          </button>
        </div>
      )}

      <style>
        {`
          @keyframes glow {
            0%, 100% {
              filter: brightness(1);
            }
            50% {
              filter: brightness(1.3);
            }
          }
        `}
      </style>
    </div>
  );
};

export default SynthNeverSets;