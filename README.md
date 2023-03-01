# Turret's Physics Patch

This patch changes the update method in MoveTransform to a more accurate equation of motion, 

## change

EOM: a*t^2 + v0*t + s0 --> 0.5*a*t^2 + v0*t + s0
