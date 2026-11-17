# GravityLab – Interactive Space & N-Body Gravity Simulator

![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)
![Unity](https://img.shields.io/badge/Unity-2022.3%20LTS%20%7C%20Unity%206-black.svg)
![Render Pipeline](https://img.shields.io/badge/Pipeline-Built--in-informational.svg)
![Physics](https://img.shields.io/badge/Integrator-Symplectic%20Velocity%20Verlet-success.svg)

**GravityLab** is an educational and scientific 3D solar system and gravitational physics simulator built in Unity and C#. It models pairwise Newtonian gravitational interactions across $N$ celestial bodies using numerical integration, dynamic orbital trajectories, scientific telemetry readouts, interactive planetary manipulation, and educational theory panels.

Designed specifically for undergraduate Computer Science and Physics Engineering curricula, the simulation emphasizes computational accuracy, deterministic physics execution, lightweight performance on standard laptop hardware (integrated graphics), and zero third-party or paid asset dependencies.

---

## 1. Project Objectives

1. **Newtonian Gravity Visualization:** Provide an interactive 3D environment to observe how gravitational forces govern celestial kinematics according to Newton's law of universal gravitation ($F = G \frac{m_1 m_2}{r^2}$).
2. **Computational Physics in Practice:** Implement numerically stable differential integration algorithms (specifically **Velocity Verlet**) to demonstrate energy and angular momentum conservation in multi-body gravitational systems.
3. **Scientific Exploration:** Allow users to adjust universal gravitational constants, planetary masses, starting velocities, and softening parameters to observe orbital perturbations, ejections, and captures.
4. **Interactive Sandbox & Custom Entity Creation:** Enable researchers and students to construct custom celestial bodies with vector velocity calculators and experiment with non-Keplerian multi-body choreography.
5. **Educational Accessibility:** Present complex computational physics concepts (symplectic integrators, Plummer softening, barycentric mechanics, and Keplerian orbits) through an intuitive scientific dashboard.

---

## 2. Key Features

- **Newtonian N-Body Gravity Engine:**
  - Evaluates pairwise interactions with Newton's 3rd Law symmetry ($\vec{F}_{ij} = -\vec{F}_{ji}$).
  - Configurable Plummer gravitational softening ($\epsilon$) to prevent $1/r^2$ singularities and non-physical velocity spikes.
  - Symplectic 2nd-order **Velocity Verlet** integration algorithm that conserves total mechanical energy ($E = T + U$).
  - Deterministic fixed-timestep sub-stepping clock ($\Delta t = 0.016\text{s}$) to maintain physical stability even at $10.0\text{x}$ playback speeds.
- **Solar System & Experimental Choreographies:**
  - Standard 9-body Solar System preset (Sun through Neptune) calibrated with Keplerian orbital velocities.
  - Famous Chenciner-Montgomery (2000) **Figure-8 Three-Body Choreography**.
  - Binary Star system with a stable circumbinary habitable planet.
  - Sun-Jupiter system demonstrating stable equilateral **Lagrange $L_4$ and $L_5$ Trojan Asteroids**.
- **Visuals & Space Environment:**
  - Procedural 3D spherical starfield (1,500 multi-spectral stars) with zero GPU fill-rate overhead.
  - Emissive stellar glow with non-shadow-casting Point Light.
  - Dynamic procedural planetary rings (Saturn).
  - High-performance `LineRenderer` orbital trails with distance-threshold vertex pooling and alpha-fading gradients.
- **Scientific Dashboard UI:**
  - Real-time kinematic telemetry: 3D position vector, velocity vector and magnitude ($\text{SU/s}$ & $\text{km/s}$), acceleration ($\text{SU/s}^2$ & $\text{m/s}^2$), distance to Sun, and net gravitational force.
  - Scientific unit conversion between Simulation Units ($\text{SU, MU, TU}$) and SI Astronomical Units ($\text{AU, km, kg, Newtons, } M_\odot, M_\oplus$).
  - Diagnostics bar tracking active body count, simulated elapsed time, framerate (FPS), and system mechanical energy.
- **User Interactions & Sandbox:**
  - 3D Raycasting planet selection with targeting reticle and quick-select dropdown list.
  - Dynamic smooth-tracking orbital camera with mouse orbit, pan, exponential zoom, and WASD/QE navigation.
  - Custom body creation dialog with input validation, color palette selection, collision proximity checks, and an automated tangential circular orbit velocity helper ($v = \sqrt{GM/r}$).
- **Built-in Physics Education Guide:**
  - Dedicated interactive reference modal explaining 10 foundational topics in gravitation and computational physics.
- **1-Click Scene Generation:**
  - Editor wizard (`GravityLabSetup.cs`) builds the complete scene, connects all dependencies, and configures the project in seconds.

---

## 3. Technical Specifications & Requirements

- **Unity Compatibility:** Unity 2022.3 LTS or Unity 6 (6000.x).
- **Render Pipeline:** Built-in Render Pipeline (Standard).
- **Target Hardware:** Standard laptop with integrated graphics (e.g., Intel Iris Xe / UHD, AMD Radeon Vega).
- **Third-Party Packages:** **None.** (Zero external APIs, zero paid assets, zero mandatory plugins).
- **Programming Language:** C# (organized with modular namespaces).

---

## 4. Installation & Quick Start

### Step 1: Open in Unity Hub
1. Open **Unity Hub**.
2. Click **Add** ➔ **Add project from disk**.
3. Select the folder:
   ```
   c:\Users\Divyansh\OneDrive\Documents\arvr project\GravityLab
   ```
4. Open the project using Unity 6 or Unity 2022.3 LTS.

### Step 2: Generate the Scene (1-Click)
1. In the top Unity Editor menu bar, click:
   ```
   GravityLab  ➔  Generate Demo Scene
   ```
2. The Editor will procedurally construct the camera, lighting, starfield, 9-body solar system, canvas dashboard, and serialized linkages, saving the scene to `Assets/Scenes/MainSimulation.unity`.
3. Click **Start Exploring** when the confirmation dialog appears.

### Step 3: Run the Simulation
1. Click the **▶ Play** button at the top of the Unity Editor.
2. The simulation will begin immediately!

---

## 5. Controls & Keybindings

| Input | Function | Description |
| :--- | :--- | :--- |
| **Right-Click Drag** | Orbit Camera | Rotates the view around the current focal target. |
| **Middle-Click Drag** | Pan Camera | Translates camera across the current viewing plane. |
| **Scroll Wheel** | Zoom In / Out | Smooth exponential distance zoom. |
| **W, A, S, D / Arrows** | Free Pan | Fly camera across the solar ecliptic plane. |
| **E / Q (or R / F)** | Vertical Elevation | Adjust camera height up or down. |
| **Left Click** | Select Planet | Raycasts to select and display telemetry for a body. |
| **F Key** | Focus Body | Frames camera on selected body and begins tracking. |
| **Spacebar** | Toggle Pause | Pauses/resumes physical time integration. |
| **Home Key** | Reset View | Returns camera to top-down isometric solar system overview. |

---

## 6. Physics Formulation & Numerical Integration

### Newton's Law of Universal Gravitation
The gravitational force between two point masses $m_i$ and $m_j$ separated by displacement $\vec{r}_{ij} = \vec{r}_j - \vec{r}_i$ is:
$$\vec{F}_{ij} = G \frac{m_i m_j}{(|\vec{r}_{ij}|^2 + \epsilon^2)^{3/2}} \vec{r}_{ij}$$

Where:
- $G$ is the gravitational coupling constant.
- $m_i, m_j$ are the respective masses.
- $\epsilon$ is the **Plummer softening parameter**, which avoids numerical divergence as distance $|\vec{r}_{ij}| \to 0$.

Dividing by $m_i$ yields the net gravitational acceleration on body $i$:
$$\vec{a}_i = \sum_{j \neq i} G \frac{m_j}{(|\vec{r}_{ij}|^2 + \epsilon^2)^{3/2}} \vec{r}_{ij}$$

### The Symplectic Velocity Verlet Integrator
Explicit Forward Euler integration evaluates position using old velocities:
$$x(t + \Delta t) = x(t) + v(t)\Delta t$$
$$v(t + \Delta t) = v(t) + a(x(t))\Delta t$$
Because Euler integration is not symplectic, it introduces artificial positive energy into orbital systems, causing planets to continuously expand their orbital radii and spiral outward.

GravityLab implements the 2nd-order **Velocity Verlet** scheme:
1. **Position Update:**
   $$\vec{x}(t + \Delta t) = \vec{x}(t) + \vec{v}(t)\Delta t + \frac{1}{2}\vec{a}(t)\Delta t^2$$
2. **Acceleration Recalculation:**
   $$\vec{a}(t + \Delta t) = \text{EvaluateGravitationalField}(\vec{x}(t + \Delta t))$$
3. **Velocity Update:**
   $$\vec{v}(t + \Delta t) = \vec{v}(t) + \frac{1}{2}\big(\vec{a}(t) + \vec{a}(t + \Delta t)\big)\Delta t$$

Because Velocity Verlet preserves the symplectic 2-form in phase space, mechanical energy oscillates within strict, bounded limits rather than drifting monotonically.

---

## 7. Scientific Unit System

To prevent floating-point catastrophic cancellation and overflow associated with astronomical quantities ($10^{30}\text{ kg}$ and $10^{11}\text{ m}$), GravityLab uses an internally consistent Simulation Unit (SU) system:

| Quantity | Simulation Unit | SI / Astronomical Equivalent | Conversion Factor |
| :--- | :--- | :--- | :--- |
| **Distance** | $1.0\text{ SU}$ | $6.80 \times 10^6\text{ km}$ ($0.0454\text{ AU}$) | $22.0\text{ SU} \approx 1.0\text{ AU}$ |
| **Mass** | $1.0\text{ MU}$ | $5.972 \times 10^{24}\text{ kg}$ ($1.0\text{ Earth Mass}$) | $1.0\text{ MU} = 1.0\ M_\oplus$ |
| **Stellar Mass** | $10,000\text{ MU}$ | $1.989 \times 10^{30}\text{ kg}$ ($1.0\text{ Solar Mass}$) | $10,000\text{ MU} = 1.0\ M_\odot$ |
| **Velocity** | $1.0\text{ SU/s}$ | $1.396\text{ km/s}$ | Earth orbit $\approx 21.32\text{ SU/s}$ ($29.8\text{ km/s}$) |
| **Time** | $6.48\text{ s (at 1x)}$ | $365.25\text{ Days}$ ($1.0\text{ Earth Year}$) | $1\text{ simulated year} \approx 6.48\text{ seconds}$ |

---

## 8. Project Architecture & Code Organization

```
Assets/
├── Editor/
│   └── GravityLabSetup.cs           # Automated 1-click scene generation wizard
├── Scenes/
│   └── MainSimulation.unity         # Master simulation scene
├── Scripts/
│   ├── Core/
│   │   ├── GravitySimulator.cs      # Core N-body physics driver & sub-stepper
│   │   ├── SimulationManager.cs     # Central coordinator, presets & modes
│   │   ├── SimulationSettings.cs    # Global physical and visualization limits
│   │   └── SimulationClock.cs       # Time progression & framerate diagnostics
│   ├── Physics/
│   │   ├── CelestialBody.cs         # Discrete physical mass entity & state
│   │   ├── GravityCalculator.cs     # Pairwise Newtonian force calculations
│   │   └── OrbitalIntegrator.cs     # Velocity Verlet & Euler-Cromer integrators
│   ├── Visualization/
│   │   ├── OrbitTrail.cs            # Distance-sampled LineRenderer trails
│   │   ├── PlanetVisuals.cs         # Materials, axial spin & Saturn rings
│   │   ├── StarFieldGenerator.cs    # Procedural multi-spectral starfield
│   │   └── CameraController.cs      # Orbit, pan, zoom & planet tracker
│   ├── UI/
│   │   ├── MainUIController.cs      # Master dashboard coordinator
│   │   ├── SimulationControlPanel.cs# Playback, G-strength & speed sliders
│   │   ├── PlanetInformationPanel.cs# Live telemetry & astronomical facts
│   │   ├── BodyCreationPanel.cs     # Custom body creation modal dialog
│   │   └── PhysicsEducationPanel.cs # Educational theory modal dialog
│   ├── Data/
│   │   ├── CelestialBodyData.cs     # Serializable entity data model
│   │   └── SolarSystemPreset.cs     # 9-body, Figure-8 & Binary star presets
│   └── Utilities/
│       ├── UnitConverter.cs         # SU to SI / Astronomical unit formatting
│       └── SimulationLogger.cs      # Structured scientific event logging
├── Materials/                       # Procedural shader materials
├── Prefabs/                         # Exportable body & UI prefabs
└── Documentation/                   # Additional documentation assets
```

---

## 9. Limitations & Assumptions

1. **Newtonian Mechanics (Non-Relativistic):** The simulation models classical Newtonian gravity. It does not account for General Relativity (e.g., Einsteinian precession of Mercury's perihelion).
2. **Point-Mass Assumption:** Celestial bodies are integrated as spherical point masses. Tidal deformations and oblateness ($J_2$ perturbation) are not modeled.
3. **Visual Radius Scaling:** Real planetary diameters are microscopic compared to astronomical orbital radii. Planetary radii in the simulation are exaggerated by approximately $10^2$ to $10^3$ to permit simultaneous visualization of planetary surfaces and orbital trajectories.
4. **Collision Handling:** When two bodies intersect within visual radii, Plummer softening mitigates velocity divergence, but physical fragment merging or orbital destruction is not simulated by default to maintain orbital continuity.

---

## 10. Future Improvements

- **Barnes-Hut Octree Algorithm:** Implement hierarchical tree clustering to reduce $O(N^2)$ force evaluations to $O(N \log N)$ for simulations with $10,000+$ asteroid particles.
- **Inelastic Collisions & Coalescence:** Add mass merging and momentum conservation when celestial bodies physically impact.
- **Relativistic Post-Newtonian Corrections:** Incorporate first-order post-Newtonian acceleration terms to simulate gravitational time dilation and perihelion advance.
- **Orbital Maneuver Hohmann Transfer Visualizer:** Allow students to apply impulsive thrust ($\Delta v$) vectors to simulate interplanetary spacecraft trajectories.

---

## 11. License

This project is licensed under the **MIT License** – see the [LICENSE.txt](file:///c:/Users/Divyansh/OneDrive/Documents/arvr project/GravityLab/LICENSE.txt) file for details.
