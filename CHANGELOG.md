# Turret's Physics Patch



## V 0.2.0

Added acceleration support
    - function for altering acceleration equation
    - Nowe works with functions instead of classes

changed the update rate
    - 90 hz to 70 hz

changed the drag coefficient conversion
    - factor from 0.015 to 0.0075

changed the integration scheme
    - from forward euler to Runge Kutta 4 

removed fix for high bullet velocities with drag
    - Reason: new itergration scheme has a higher speed tolerance.


## V 0.1.0

(not published)

Added acceleration support
    - Dirac pulse (2 overloads)
    - square pulse
    - step
    - custom can be done using the class

changed the drag coefficient conversion
    - factor from 0.01 to 0.015

changed the update rate
    - 70 hz to 90 hz

Added a fix for high bullet velocities with drag
    - If the change in velocity due to drag is greater than the velocity then velocity will be reduced by 70% instead.

## V 0.0.5

initial release
