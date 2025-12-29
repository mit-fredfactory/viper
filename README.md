# Virtual Industrial Performance Enhancement Reality (VIPER)

**Immersive Cognitive Factory Twin**

VIPER is an immersive VR simulation framework for visualizing factory operations, featuring a neural network trained on simulated data to predict KPIs, and an integrated LLM to analyze predictions and suggest actionable improvements via natural language within the VR interface.

![Case Study Process](https://github.com/user-attachments/assets/e373042a-3833-4172-a24f-d18a265791ba)

Simulation experiments demonstrated **29% production improvement** and **36% waste reduction** compared to traditional training methods. This framework empowers managers with real-time, actionable insights for substantial gains in factory efficiency and resource optimization.

**Status:** First author. Under review for publication in journal Taylor & Francis: Production and Manufacturing Research.

**Collaborators:** MIT, Tecnológico de Monterrey, FrED Factory

## Overview

The system consists of three main components:

1. **Immersive VR Simulation** - Real-time visualization of factory operations with operator modeling
2. **Neural Network Predictions** - KPI prediction trained on simulated data
3. **LLM Analysis** - Natural language analysis and recommendations within the VR interface

![Framework Diagram](https://github.com/user-attachments/assets/6658508e-06f9-43a7-b69d-627c95e65488)

## Architecture

### VR Simulation System

The simulation models a three-station cooling fan assembly process:

- **Cooling Subassembly** (`RuntimeFanSupport.cs`) - Adds 6 heat inserts to a 3D piece
- **Fan Subassembly** (`RuntimeFanCrimping.cs`) - Crimps 2 wires of two fans
- **Cooling Fan Assembly** (`RuntimeCoolingFan.cs`) - Screws 2 screws to each fan (4 total screws)

The simulation manager (`SimManagerScript.cs`) handles station navigation and camera positioning for VR exploration.

#### Operator Modeling

Operator behavior is modeled through `OperatorParams.cs`, which tracks:
- Demographics: age, experience, training level
- Cognitive state: attention level, cognitive load, learning curve
- Physical state: stress level, fatigue level, motivation level

Operator efficiency is calculated dynamically based on these parameters plus environmental factors (noise, temperature, lighting) and ergonomic rating. See `OperatorParams.CalculateEfficiency()` in `OperatorParams.cs`.

#### State Machine Implementation

Each station implements a state machine for process simulation:
- `RuntimeCoolingFan.cs` - States: GrabCoolingSubassembly → GrabFanSubassembly → Screw → Done
- `RuntimeFanSupport.cs` - States: GrabMaterial → Insert → Done
- `RuntimeFanCrimping.cs` - States: GrabFan → Crimp → Done

State transitions are time-based and affected by operator efficiency, which influences failure rates.

### Data Collection for Neural Network Training

Each runtime component collects data instances every 60 seconds via `AddDataInstance()` methods:

**Data Collected:**
- Time, operator parameters (age, experience, training, attention, cognitive load, learning curve, stress, fatigue, motivation)
- Environmental factors (ergonomic rating, noise level, temperature, lighting)
- Performance metrics (success rate calculated from recent attempts)
- Break status

Data is written to CSV files in `Application.persistentDataPath`:
- `cooling_fan_{timestamp}.csv`
- `fan_support_{timestamp}.csv`
- `fan_crimping_{timestamp}.csv`

The data collection uses a rolling window approach (last 10 instances) to calculate recent success rates, as seen in `RuntimeCoolingFan.AddDataInstance()`.

### LLM Integration for Analysis

The LLM integration (`ManagerCoolingFan.cs`) provides natural language analysis of simulation data:

**Implementation:**
- `RequestModel()` in `ManagerCoolingFan.cs` aggregates analysis data from all three stations via `GetAnalysis()` methods
- Supports both local LLM (`LLMCharacter`) and API-based LLM (`ApiLlmInference.cs`)
- API integration uses OpenRouter with streaming support for real-time responses

**Analysis Data Format:**
The LLM receives:
1. User prompt from `promptInputField`
2. Aggregated CSV data from all three stations (every 10th instance)
3. System prompt configured in `ApiLlmInference.systemPrompt` (expert manufacturing advisor)

**LLM Recommendations:**
The system prompt constrains suggestions to:
- Changing operators between stations
- Taking 15-minute breaks

Responses are displayed in real-time via streaming (`HandleApiReplyChunk()`) in the VR interface.

### Environmental Factors

Environmental factors (noise, temperature, lighting) can be adjusted in real-time through UI input fields. These affect operator efficiency calculations across all stations via `SetEnvironmentalFactors()` methods.

## Setup

The project is built using LTS **Unity 6000.0.44f1**. Select the proper installation for your OS.

**Git** is required to collaborate and have the latest version of the code. Install it from [here](https://git-scm.com/download/win). When the installer is opened, click `Next` for the default installation until it is finished.

Then, open `CMD`, navigate to your preferred location with `cd` commands, and run:

```bash
git clone https://github.com/mit-fredfactory/viper.git
cd viper
```

In Unity Hub, click **Add project from disk**, navigate to where the repository was cloned, and select the folder.

## Usage

### Running the Simulation

1. Open the scene `Assets/Scenes/ViperScene.unity` or `Assets/Scenes/TheFrEDFactory.unity`
2. The `ManagerCoolingFan` component manages the simulation lifecycle
3. Use the Play/Pause buttons to control simulation time
4. Adjust speed multiplier via `speedAccelInputField`
5. Modify environmental factors (noise, temperature, lighting) in real-time

### Data Collection

Enable data collection by setting `generateDataset = true` on runtime components:
- `RuntimeCoolingFan.generateDataset`
- `RuntimeFanSupport.generateDataset`
- `RuntimeFanCrimping.generateDataset`

Data is automatically written to CSV files every 60 seconds while the simulation is running.

### LLM Analysis

1. Enter a prompt in the `promptInputField` (e.g., "Analyze current performance and suggest improvements")
2. Click the model request button
3. The system aggregates data from all stations and sends to the configured LLM
4. View streaming responses in `modelResponseText`

**LLM Configuration:**
- Set `useApiModel` to `true` for API-based LLM (requires `ApiLlmInference` component)
- Set `useApiModel` to `false` for local LLM (requires `LLMCharacter` component)
- Configure API settings in `ApiLlmInference`: `apiUrl`, `apiKey`, `modelName`, `systemPrompt`

### Operator Breaks

Use the break button to toggle operator break status. When on break:
- Operators recover (attention, motivation increase; stress, fatigue decrease)
- Work processes pause
- Break status is included in data collection

## Key Scripts

| Script | Purpose |
|--------|---------|
| `ManagerCoolingFan.cs` | Main simulation manager, LLM integration, UI controls |
| `RuntimeCoolingFan.cs` | Cooling fan assembly station state machine and data collection |
| `RuntimeFanSupport.cs` | Cooling subassembly station (heat inserts) |
| `RuntimeFanCrimping.cs` | Fan subassembly station (wire crimping) |
| `OperatorParams.cs` | Operator parameter modeling and efficiency calculation |
| `ApiLlmInference.cs` | API-based LLM integration with streaming support |
| `SimManagerScript.cs` | VR station navigation and camera management |

## Technologies

- **Unity** - VR simulation engine
- **LLMs** - Natural language analysis (OpenRouter API or local models)
- **Neural Networks** - KPI prediction (training data generated by simulation)
- **Meta XR** - VR platform support
- **Python** - Neural network training (external)
- **C#** - Unity scripting

## Research

This project is part of Industry 5.0 research focusing on human-centric manufacturing optimization through immersive digital twins.

**Date:** September 2024 - May 2025

## License

See repository for license information.

