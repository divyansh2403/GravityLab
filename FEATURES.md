# GravityLab – Implemented Features Matrix

**Project Name:** GravityLab  
**Subtitle:** Interactive Space & N-Body Gravity Simulator  
**Domain:** Computational Astrophysics & Physics Simulation  
**Target Platform:** Unity 2022.3 LTS / Unity 6 (Built-in Render Pipeline, Lightweight for Integrated Graphics)

---

## 1. Core Physics & Gravitational Mechanics

- [x] **Newtonian N-Body Gravity Calculations**
  - Evaluates pairwise forces using Newton's Universal Law: $F = G \frac{m_1 m_2}{r^2}$.
  - Computes net acceleration vector $\vec{a}_i$ on every body from all other bodies.
  - Implements Newton's 3rd Law symmetry ($\vec{F}_{ij} = -\vec{F}_{ji}$) to cut force evaluations in half.
  - Strictly ignores self-gravitation ($i \neq j$).
- [x] **Plummer Gravitational Softening**
  - Implements $r_{\text{eff}} = \sqrt{r^2 + \epsilon^2}$ to eliminate mathematical singularities ($1/0$) and prevent infinite velocity spikes during near-collisions.
  - Runtime adjustable softening parameter $\epsilon$ via user interface slider.
- [x] **Symplectic Velocity Verlet Integration**
  - Employs 2nd-order, time-reversible Velocity Verlet integration ($x(t+\Delta t) = x(t) + v(t)\Delta t + \frac{1}{2}a(t)\Delta t^2$).
  - Symplectic formulation preserves total mechanical energy and orbital angular momentum over long simulation runs, eliminating artificial outward spiraling seen in basic Euler methods.
  - Semi-implicit Euler-Cromer available for comparative physics demonstrations.
- [x] **Deterministic Sub-Stepping Clock**
  - Temporal accumulator decoupling physical simulation steps ($\Delta t = 0.016\text{s}$) from rendering framerates.
  - Multiple fixed sub-steps executed per frame when time scale is accelerated ($>1\text{x}$), preserving numerical stability at high playback speeds.
- [x] **Energy & Momentum Diagnostics**
  - Real-time calculation of System Kinetic Energy ($T = \frac{1}{2} \sum m_i v_i^2$) and Gravitational Potential Energy ($U = -\sum \frac{G m_i m_j}{r_{ij}}$).
  - Barycenter (Center of Mass) and net system momentum monitoring.

---

## 2. Presets & Scientific Scaling

- [x] **9-Body Solar System Preset**
  - Includes: Sun, Mercury, Venus, Earth, Mars, Jupiter, Saturn, Uranus, Neptune.
  - Calibrated initial velocities using Keplerian circular orbit dynamics: $v = \sqrt{\frac{GM}{r}}$.
  - Scientifically informed relative masses, visual radii, and realistic astronomical descriptions.
- [x] **Scientific Simulation Unit System**
  - Distance: $1.0\text{ SU} = 6.80 \times 10^6\text{ km}$ ($22.0\text{ SU} \approx 1.0\text{ AU}$).
  - Mass: $1.0\text{ MU} = 5.972 \times 10^{24}\text{ kg}$ ($1\text{ Earth Mass}$).
  - Stellar Mass: $10,000\text{ MU} \approx 1.0\text{ Solar Mass}$ ($M_\odot$).
  - Velocity: $1.0\text{ SU/s} \approx 1.40\text{ km/s}$.
  - Real-world dual-unit formatting for all telemetry (SU and AU/km, MU and kg/Earth masses/Solar masses).
- [x] **Experimental Astrophysics Choreographies**
  - **Figure-Eight 3-Body System**: The famous Chenciner-Montgomery (2000) equal-mass planar choreography.
  - **Binary Star System**: Two equal-mass stars orbiting their mutual barycenter with a circumbinary planet.
  - **Lagrange Trojan Asteroids**: Sun-Jupiter system demonstrating stable $L_4$ and $L_5$ equilateral equilibrium points.
  - **Blank Sandbox**: Empty space with central anchor for student experiments.

---

## 3. 3D Space Visualization & Graphics

- [x] **Procedural Starfield Background**
  - Lightweight spherical particle distribution with 1,500 background stars.
  - Multi-spectral stellar temperature palette (Class O/B blue-white, Class A white, Class G solar yellow, Class M red-orange).
  - Zero shadow-casting overhead; 100+ FPS on low-power integrated GPUs.
- [x] **Planetary & Stellar Rendering**
  - Sun: Emissive glowing material with dedicated non-shadow-casting Point Light.
  - Saturn: Procedural 3D planetary ring system.
  - Planets: Distinct albedo colors, specular gloss, and continuous axial spin.
- [x] **Optimized Orbit Trajectories (LineRenderer)**
  - Smooth alpha-fading trail gradient (dense at planet head, transparent at tail).
  - Distance-threshold vertex sampling prevents redundant point buildup.
  - Hard point memory cap (default 500-600 points per body) prevents garbage collection stutter and memory leaks.
  - Global trail visibility toggle and instant trail clearing button.

---

## 4. Scientific Dashboard UI & UX

- [x] **Top Header Bar**
  - GravityLab identity branding.
  - Real-time simulation status badge (green "● RUNNING" / amber "⏸ PAUSED").
  - Current playback speed readout.
  - Quick action buttons ("+ Add Body", "📖 Physics Theory").
- [x] **Left Control Panel**
  - Play, Pause, Resume, and Reset simulation buttons.
  - Speed Multiplier slider ($0.1\text{x}$ to $10.0\text{x}$).
  - Gravity Strength ($G$) slider ($0.1\text{G}$ to $5.0\text{G}$).
  - Softening Factor ($\epsilon$) slider ($0.05$ to $3.0\text{ SU}$).
  - Orbit Trails on/off toggle and "Clear Trails" button.
  - Camera tracking on/off toggle and "Reset View" button.
  - Mode Switcher dropdown (Solar System Mode vs Experiment Sandbox).
  - Preset Switcher dropdown for experiment configurations.
- [x] **Right Celestial Telemetry Panel**
  - Live body name, astronomical category, and scientific description.
  - Mass in both Simulation Units and SI metric ($\text{kg}$, $M_\odot$, $M_\oplus$).
  - Visual radius in SU.
  - Real-time Cartesian 3D Position $(x, y, z)$.
  - Real-time Velocity vector $(v_x, v_y, v_z)$ and scalar speed in SU/s and $\text{km/s}$.
  - Real-time Acceleration vector $(a_x, a_y, a_z)$ and magnitude in SU/s² and $\text{m/s}^2$.
  - Euclidean distance to Sun / Barycenter in SU, AU, and millions of km.
  - Net Gravitational Force in simulation force units and Newtons.
  - Astronomical reference data (real mass, radius, semi-major axis, orbital period).
  - Quick-select dropdown list of all bodies in the scene.
  - "Focus Camera on Body" button.
  - "Delete Body" button (enabled for custom bodies, locked for default system planets).
- [x] **Bottom Diagnostic Bar**
  - Active celestial body counter.
  - Elapsed simulation time in simulated Days and Years.
  - Discrete numerical timestep ($\Delta t$).
  - Real-time framerate monitor (FPS).
  - Total mechanical energy readout (conservation diagnostic).

---

## 5. Interaction & Custom Body Creation

- [x] **3D Raycast & UI Planet Selection**
  - Click any celestial sphere in 3D space to inspect its telemetry.
  - Dynamic visual target reticle marker highlights selected body.
  - Selection synchronization with right telemetry panel and camera tracker.
- [x] **Custom Celestial Body Generator**
  - Text input validation for Body Name, Mass ($>0$), and Visual Radius ($>0$).
  - 3D Cartesian coordinates input $(X, Y, Z)$ for position and initial velocity.
  - 6-color preset picker buttons with real-time preview swatch.
  - **Auto-Calculate Circular Orbital Velocity Button**: Computes tangential speed $v = \sqrt{GM/r}$ in perpendicular direction, preventing user-spawned bodies from immediately crashing or escaping into deep space.
  - Proximity collision validation prevents spawning inside existing planets.
- [x] **Protected Entity Security**
  - Prevents accidental deletion of Sun and canonical solar system planets.

---

## 6. Educational Physics Guide

- [x] **Comprehensive Theory Modal**
  - Clear mathematical display of Newton's Gravitation Law ($F = G \frac{m_1 m_2}{r^2}$).
  - Complete variable legend ($F, G, m_1, m_2, r, \epsilon, \Delta t$).
  - 10 structured academic topics formatted for undergraduate computer science and physics engineering:
    1. Newton's Law of Universal Gravitation
    2. Gravitational Force & Symmetry
    3. Mass & Gravitational Acceleration
    4. Orbital Velocity Derivation
    5. Orbital Motion & Keplerian Conics
    6. N-Body Simulation Complexity & Poincaré Chaos
    7. Numerical Integration Discretization
    8. Symplectic Velocity Verlet Method vs Forward Euler
    9. Gravitational Softening (Plummer Model)
    10. Simulation Timestep Stability & Sub-Stepping

---

## 7. Editor Automation & Setup

- [x] **One-Click Scene Generator (`GravityLabSetup.cs`)**
  - Unity Editor menu item: `GravityLab → Generate Demo Scene`.
  - Automatically constructs Camera, Lighting, Sun, 8 Planets, Starfield, and Canvas UI.
  - Automatically serializes and links all script references.
  - Saves scene to `Assets/Scenes/MainSimulation.unity` and registers in `EditorBuildSettings`.
  - Batchmode-compatible for automated build pipelines.
