# Turret's Physics Patch

This patch changes the update method in MoveTransform to a more accurate equation of motion, and changes the drag to be quadratic instead of constant. Also locks the rate at which the bullets update their position at 70 hz.

If you notice any issues, please post in the rounds modding discord.
I will look at the bug reports channel from time to time.

## changes

EOM: a*t^2 + v0*t + s0 --> 0.5*a*t^2 + v0*t + s0
Drag: a = D  --> a = D*V^2 / 100
Update rate: per frame --> 70 hz


