# Gunshot Trajectory Visualization

## About the project

This project is a Unity application for the visualization of gunshot
trajectories on an anatomical 3D model.

The application allows gunshot trajectory data to be visualized in a
3D environment. The trajectory can be entered manually or received
from 3D Slicer.

The project also contains a Virtual Reality implementation for
visualizing the model and gunshot trajectory using a Meta Quest headset.

---
## Requirements
### Software

- **Unity:** 6000.2.10f1
- **Meta XR SDK:** 
- **Blender:** 5.0 (used for editing and optimizing the 3D model)
- **Slicer:** 5.8.1 (optional, only required when using the Slicer connection)

### Hardware

- **Meta Quest Pro** – used for VR testing
- **Meta Quest 3** – expected to work, but further testing is required


## Setup

1. Clone the GitLab repository.
2. Open the project in the specified Unity version.
3. Import the required packages/SDKs.
4. Obtain the anatomical 3D model separately and add it to the project.
5. Open the `MainMenuScene` to run the desktop version of the
   application.
6. If using 3D Slicer, make sure Unity is running before sending the
   trajectory data.

## 3D Model

The anatomical 3D model used in the project was edited and prepared
using Blender.

The original model is approximately 308 MB and is therefore too large
to include directly in the GitLab repository.

The model is provided separately and needs to be added to the Unity
project before the complete visualization can be used.

---

## Virtual Reality

The project contains a test scene called `VRScene`.

`MainMenuScene` is the main version of the application intended to run
on a computer. It contains the user interface for entering and
visualizing gunshot trajectory data.

`VRScene` is a separate test scene for the VR implementation. It
contains the anatomical model and an example gunshot trajectory so that
the VR functionality can be tested without requiring a connection to
3D Slicer.

The VR implementation was tested on a Meta Quest Pro. The application
should also be compatible with the Meta Quest 3, although further
testing may be required.

### Converting the project to VR

For further VR development, the Meta XR SDK and its Building Blocks
can be used to add the required VR functionality.

Before using the anatomical model in a standalone VR application, the
model should be downsampled/decimated. The current model
has a high polygon count, so reducing the number of polygons is
necessary to improve performance on the VR headset while retaining the
relevant anatomical details.

When converting the existing project to VR, some inputs in the scripts
may need to be changed to work with the VR controllers and interaction
system.

The Canvas also needs to be adapted for VR. The current Canvas should
be changed to use a World Space configuration rather than a
screen-space overlay, so that the user interface can be displayed
correctly within the VR environment.


### Running the VR application

To test the VR application:

1. Connect the Meta Quest headset to the computer.
2. Open the Unity project and select the `VRScene`.
3. Make sure the Meta Quest headset is connected and recognized by
   Unity.
4. In Unity, select **File → Build and Run**.
5. Unity will build and install the application on the connected
   headset.
6. The application can then be found on the headset under
   **Unknown Sources**.

The `VRScene` contains an example gunshot trajectory and can therefore
be used to test the VR implementation without requiring a connection
to 3D Slicer.

## 3D Slicer connection

The project can communicate with 3D Slicer to receive gunshot
trajectory data.

The connection allows the entry point and direction of the trajectory
to be sent from 3D Slicer to Unity.

The required Python code for sending the data can be found below:

```python
import socket

# Entry
entryNode = slicer.util.getNode("entry")
entry = [0, 0, 0]
entryNode.GetNthControlPointPositionWorld(0, entry)

# Direction
directionNode = slicer.util.getNode("direction")
direction = [0, 0, 0]
directionNode.GetNthControlPointPositionWorld(0, direction)

# Message
message = f"{entry[0]},{entry[1]},{entry[2]};{direction[0]},{direction[1]},{direction[2]}"

# Send
sock = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
sock.connect(("127.0.0.1", 25001))
sock.send(message.encode("utf-8"))
sock.close()

print("Sent:", message)
```


## Scripts

### `AutoCameraPosition.cs`

Automatically positions the camera based on the size of the anatomical
model. The camera distance is calculated from the model's renderer
bounds and the camera's field of view. The camera can be positioned
using a configurable yaw and pitch.

---

### `BulletData.cs`

Static class used to store gunshot trajectory data received from
3D Slicer. It stores the entry point, direction and whether new data
has been received.

The direction is automatically normalized when new data is set.

---

### `BulletPathVisualizer.cs`

Main script for calculating and visualizing the gunshot trajectory.

It supports both:

- Manual input of the entry point and direction through the UI.
- Live trajectory data received from 3D Slicer.

The script converts the RAS coordinates from 3D Slicer to Unity
coordinates and aligns them with the anatomical model using three
anatomical reference points:

- Sternum
- Left clavicle
- Right clavicle

It also calculates the scale and rotation required to align the
3D Slicer model with the Unity model.

The trajectory is visualized using a `LineRenderer`. The script also
places and rotates the gun model at the end of the trajectory.

The trajectory length can be adjusted using a UI slider.

---

### `CameraOrbit.cs`

Controls the desktop camera around the anatomical model.

The user can rotate the camera using the mouse and zoom in or out using
the mouse scroll wheel. The camera is prevented from rotating when the
mouse pointer is positioned over the UI.

---

### `ExampleGunshotTrajectory.cs`

Provides a simple example gunshot trajectory for testing the
visualization without requiring trajectory data from 3D Slicer.

The script creates a trajectory from a starting anchor and a direction,
and positions and rotates the gun model accordingly.

This can be used for testing the scene and VR setup with a predefined
trajectory.

This script has been used in the VRScene.

---

### `Resize.cs`

Allows the height of the anatomical model to be entered through the UI.

The script calculates the current height of the model and applies a
uniform scale so that the model matches the entered height.

This can be used to adjust the model to the known height of a person. 

**Note:** This script is currently not implemented in the application.
It is a separate script that was developed as a possible feature for
future use.
---

### `SocketReceiver.cs`

Handles the TCP connection between 3D Slicer and Unity.

The script starts a local TCP server on:

- IP: `127.0.0.1`
- Port: `25001`

It receives the entry point and direction sent from 3D Slicer, parses
the received data and stores it in `BulletData`.

The `BulletPathVisualizer` is then updated with the newly received
trajectory data.

---

### `UIManager.cs`

Controls the different UI panels of the application.

It handles starting the application, returning to the home screen and
resetting the scene.

It also manages the transition between the start menu, trajectory
input panel and the main visualization interface.



## To do

- [ ] Downsample/decimate the anatomical 3D model for VR
- [ ] Add the optimized model to the Unity project
- [ ] Further test the VR implementation
- [ ] Check and update controller inputs for VR
- [ ] Adapt the Canvas to World Space
- [ ] Further test the Meta Quest 3
- [ ] Further develop the 3D Slicer connection if needed


## Possible future extensions

The following extensions were considered during the development of the
project:

- **Support for different anatomical models**  
  Allow new anatomical models to be imported directly into the
  application, with automatic or guided selection of the required
  anatomical reference points.

- **Improved coordinate registration**  
  Improve the alignment between 3D Slicer and Unity by using additional
  reference points, more advanced registration methods or physical
  reference markers.

- **Automatic anatomical landmark detection**  
  Investigate the use of AI to automatically detect and place
  anatomical reference points.

- **Realistic incident reconstruction**  
  Add different body positions and allow the position and posture of
  the shooter to be represented.

- **Crime scene reconstruction**  
  Recreate the actual environment of an incident using 3D scans,
  including walls, furniture and other relevant objects.

- **Dynamic trajectory simulation**  
  Add animations and additional visualizations such as the shooting
  angle and possible positions of the victim and shooter.

- **Network communication**  
  Extend the current local 3D Slicer ↔ Unity connection so that
  communication between different devices or networks is possible.
  Add validation and error handling for incoming data.

- **Improved VR interaction**  
  Allow the anatomical model to be rotated, moved and scaled directly
  using VR controllers.

- **Legal / forensic presentation**  
  Investigate using the visualization to communicate forensic
  reconstructions in legal contexts and compare different scenarios.

- **Multi-user VR**  
  Allow multiple users to view and analyse the same reconstruction
  simultaneously in VR.

- **Validation with real datasets**  
  Test and evaluate the system using realistic forensic datasets and
  compare the results with existing reconstruction methods or expert
  analyses.


## Notes

The bachelor thesis contains more detailed information about the
coordinate transformation, model alignment, trajectory visualization
and the development process.

