# GravityLab – Step-by-Step Beginner's Guide

Welcome to **GravityLab: Interactive Space & N-Body Gravity Simulator**!  
This guide is designed for undergraduate computer science students, educators, and beginners who have little or no prior experience with Unity or computational physics engines.

Follow these simple, step-by-step instructions to get the project running in under 5 minutes without having to manually configure GameObjects, attach components, or wire references.

---

## Table of Contents
1. [Prerequisites & Software Requirements](#1-prerequisites--software-requirements)
2. [Step 1: Opening the Project in Unity Hub](#2-step-1-opening-the-project-in-unity-hub)
3. [Step 2: Generating the Simulation Scene (1-Click Setup)](#3-step-2-generating-the-simulation-scene-1-click-setup)
4. [Step 3: Running the Simulation](#4-step-3-running-the-simulation)
5. [Interactive Controls & Shortcuts](#5-interactive-controls--shortcuts)
6. [Exploring the Dashboard & UI](#6-exploring-the-dashboard--ui)
7. [Running Scientific Experiments](#7-running-scientific-experiments)
8. [Troubleshooting & Common Questions](#8-troubleshooting--common-questions)

---

## 1. Prerequisites & Software Requirements

- **Unity Editor:** Unity 2022.3 LTS or Unity 6 (6000.x).
- **Unity Hub:** Standard official Unity Hub (v3.x or newer).
- **Operating System:** Windows 10/11, macOS, or Linux.
- **Hardware:** Any standard student laptop (Intel Core i3/i5/i7, AMD Ryzen, Apple Silicon, 4GB+ RAM). No dedicated gaming GPU or high-end graphics card is required.
- **Third-Party Assets:** **NONE.** GravityLab uses 100% native Unity built-in primitives, shaders, and UI.

---

## 2. Step 1: Opening the Project in Unity Hub

1. Launch **Unity Hub**.
2. Click the **Projects** tab in the left navigation sidebar.
3. In the top-right corner, click the **Add** button (or click the arrow next to Add and choose **Add project from disk**).
4. In the file explorer dialog, navigate to your workspace:
   ```
   c:\Users\Divyansh\OneDrive\Documents\arvr project\GravityLab
   ```
5. Select the **`GravityLab`** folder (the folder containing `Assets`, `ProjectSettings`, and `Packages`) and click **Open** / **Add Project**.
6. The project will appear in your Unity Hub list under the name **`GravityLab`**.
7. In the **Editor Version** column, ensure your installed Unity 6 or Unity 2022.3 LTS version is selected.
8. Click on the project name to launch the Unity Editor.
   *(The first time you open the project, Unity may take 1–2 minutes to compile scripts and initialize its local cache).*

---

## 3. Step 2: Generating the Simulation Scene (1-Click Setup)

Once the Unity Editor interface is open:

1. Look at the top Unity menu bar (where *File*, *Edit*, *Assets*, *GameObject* are located).
2. You will see a custom menu item named **`GravityLab`**.
3. Click:
   ```
   GravityLab  ➔  Generate Demo Scene
   ```
4. A progress dialog will appear for a few seconds. Behind the scenes, the automated generator will:
   - Create a clean new simulation scene.
   - Configure the scientific 3D camera and ambient illumination.
   - Spawn the procedural starfield with multi-spectral stellar temperatures.
   - Instantiate the central Sun with its point-light emitter.
   - Instantiate all 8 canonical planets (Mercury through Neptune) with their real Newtonian orbital velocities.
   - Attach the procedural Saturn ring disc.
   - Construct the entire modern dark-slate UI Dashboard (Top Bar, Left Controls, Right Telemetry, Bottom Status, Body Creation Modal, and Physics Theory Modal).
   - Wire all serialized component references automatically.
   - Save the scene to **`Assets/Scenes/MainSimulation.unity`**.
   - Register the scene into your Unity **Build Settings** as Scene 0.
5. A confirmation dialog will pop up stating:
   *"The GravityLab demo scene has been generated successfully!"*
   Click **Start Exploring**.

---

## 4. Step 3: Running the Simulation

1. Look at the top center of the Unity Editor and locate the transport controls.
2. Click the **▶ Play** button (or press `Ctrl + P` on Windows / `Cmd + P` on Mac).
3. The simulation is now running live! You will observe:
   - The planets smoothly orbiting the Sun along elliptical/circular Newtonian trajectories.
   - Colored orbit trails drawing smooth paths through 3D space.
   - Planets rotating on their own rotational axes.
   - Real-time kinematic telemetry streaming in the right panel.
   - Center of mass and energy conservation readouts updating in the bottom status bar.

---

## 5. Interactive Controls & Shortcuts

| Action | Control / Hotkey | Description |
| :--- | :--- | :--- |
| **Orbit Camera** | **Right-Click Drag** (Mouse) | Rotates the camera view around the focal pivot point. |
| **Pan Camera** | **Middle-Click Drag** (Mouse) | Panning the camera horizontally or vertically in view plane. |
| **Zoom In / Out** | **Mouse Scroll Wheel** | Exponential smooth zoom in and out. |
| **Move Camera** | **W / A / S / D** or **Arrow Keys** | Free camera flight across the ecliptic plane. |
| **Raise / Lower View** | **E / Q** or **R / F** | Adjusts vertical elevation of the camera. |
| **Select Body** | **Left-Click** on any 3D Planet | Raycasts and selects the planet, displaying a targeting reticle. |
| **Focus Camera** | **F** or **🎯 Focus Camera Button** | Smoothly moves and zooms the camera to frame the selected body. |
| **Pause / Resume** | **Spacebar** or **⏸ Pause Button** | Pauses the physical integration while keeping camera operable. |
| **Reset View** | **Home Key** or **Reset View Button**| Resets the camera to the default isometric solar system overview. |

---

## 6. Exploring the Dashboard & UI

### A. Top Bar
- **Logo & Title:** Displays GravityLab identity.
- **Simulation Status Badge:** Displays green **● RUNNING** when physics is active, or amber **⏸ PAUSED** when stopped.
- **Speed Badge:** Shows current temporal multiplier ($1.0\text{x}$, $2.5\text{x}$, etc.).
- **Top Action Buttons:**
  - `+ Add Body`: Opens the custom celestial body creator.
  - `📖 Physics Theory`: Opens the educational physics reference manual.

### B. Left Control Panel
- **Playback Controls:** Pause, Resume, and full state Reset back to $t=0$.
- **Speed Multiplier Slider:** Accelerates or decelerates simulation time ($0.1\text{x}$ to $10.0\text{x}$).
- **Gravity Strength ($G$) Slider:** Modifies the universal gravitational constant from $0.1\text{G}$ to $5.0\text{G}$ in real-time. Notice how increasing $G$ pulls orbits tighter, while decreasing $G$ causes planets to drift outward!
- **Plummer Softening ($\epsilon$) Slider:** Adjusts the softening radius ($0.05$ to $3.0\text{ SU}$).
- **Orbit Trails Toggle & Clear Button:** Toggle trajectory lines on/off or wipe the vertex buffer clean.
- **Mode & Preset Dropdowns:** Switch between Solar System Mode and Sandbox Experimentation.

### C. Right Telemetry Panel
Click any planet (e.g., Earth, Jupiter, or Saturn) to inspect:
- **Mass:** Formatted in Simulation Units, SI kilograms ($\text{kg}$), and Solar/Earth mass equivalents.
- **Position & Distance:** Real-time $(X, Y, Z)$ coordinates and distance to the central star.
- **Velocity & Speed:** 3D vector components and total orbital speed in both SU/s and $\text{km/s}$.
- **Acceleration & Gravitational Force:** Instantaneous gravitational pull exerted by all other bodies.
- **Astronomical Facts:** Real-world semi-major axis, mean radius, and orbital periods.
- **Focus Camera:** Smoothly follows the planet as it orbits.

### D. Bottom Information Bar
- **Active Body Count:** Total number of interacting gravitation entities.
- **Simulation Clock:** Time elapsed in simulated Days and Years.
- **Timestep ($\Delta t$):** Numerical integration slice ($0.016\text{s}$).
- **Framerate Monitor:** Current display FPS.
- **Total Mechanical Energy:** Real-time conservation metric: $E = T + U$.

---

## 7. Running Scientific Experiments

### Experiment 1: Altering Universal Gravity
1. Set the simulation speed to $2.0\text{x}$ so orbits progress visibly.
2. Slowly drag the **Gravity Strength (G)** slider from $1.0$ down to $0.4$.
3. **Observation:** Centripetal gravitational pull is no longer sufficient to hold planets in circular orbits; their trajectories become eccentric ellipses or hyperbolic escape trajectories!
4. Reset $G$ to $1.0$ and click **↺ Reset** to restore stability.

### Experiment 2: The Figure-8 Three-Body Choreography
1. In the Left Control Panel, change the **Mode** dropdown to **Experiment Sandbox**.
2. Select **Figure-8 (3-Body)** in the preset dropdown.
3. Observe three equal-mass stellar bodies chasing one another indefinitely through a single figure-eight planar choreography, discovered by mathematicians Alain Chenciner and Richard Montgomery in 2000!

### Experiment 3: Spawning a Circumbinary Comet or Moon
1. Click **+ Add Body** in the top bar.
2. Enter a name (e.g., `Halley_Comet`).
3. Set Mass to `0.01` and Radius to `0.4`.
4. Enter Position: $X = 60$, $Y = 0$, $Z = 0$.
5. Click **⚡ Auto-Calculate Circular Orbital Velocity**. Notice how the simulator computes the exact tangential velocity ($v = \sqrt{GM/r} \approx 12.91\text{ SU/s}$) along the $Z$-axis!
6. Choose a bright neon color (e.g., Cyan or Purple).
7. Click **✓ Add Celestial Body**.
8. The new body immediately enters the gravitational field, interacts with all 9 bodies, and draws its own distinct orbit trail!

---

## 8. Troubleshooting & Common Questions

**Q: The planets are moving too slowly or too fast.**  
*A: Adjust the **Simulation Speed** slider in the Left Control Panel. At $1.0\text{x}$, Earth takes approximately 6.5 seconds to complete one full year (365 days). Raising the speed to $4.0\text{x}$ allows outer planets (Jupiter, Saturn) to complete orbits faster.*

**Q: I clicked outside the solar system and lost the camera view.**  
*A: Press the **Home** key or click the **Reset Camera View** button in the Left Panel to immediately return to the central overview.*

**Q: I created a custom planet and it flew out of the solar system.**  
*A: In Newtonian gravity, if an object's velocity exceeds the escape velocity ($v_{\text{esc}} = \sqrt{2} \cdot v_{\text{circ}}$), it enters a hyperbolic escape path. When creating custom bodies, click the **⚡ Auto-Calculate Circular Orbital Velocity** button to ensure the body starts with a stable circular speed.*

**Q: Can I delete default planets?**  
*A: The Sun and standard solar system planets are protected from accidental deletion to maintain reference integrity. Custom user-created bodies can be deleted at any time by selecting them and clicking the red **Delete Body** button.*
