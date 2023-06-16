# Turret's Physics Patch

This patch changes the update method in MoveTransform to a more accurate equation of motion, and changes the drag to be quadratic instead of constant. Also locks the rate at which the bullets update their position at 90 hz.

If you notice any issues, please post in the rounds modding discord.
I will look at the bug reports channel from time to time.

## Changes

EOM: a*t^2 + v0*t + s0 --> 0.5*a*t^2 + v0*t + s0
Drag: a = D  --> a = D*V^2 * 1.5/ 100
Update rate: per frame --> 90 hz
Adds some functions that impact the velocity

## Added functions (These are added to MoveTransform)

# Impulse_Dirac_Pulse
    - Adds a Dirac pulse to the velocity at the end of a step
    - Requires:
        - the velocity to add
    - Has an overload that allows for a time delay

# Impulse_Square_Pulse
    - Adds a square pulse signal to the velocity, also adjusts position.
    - Requires:
        - Time to start pulse
        - Direction of velocity
        - Magnitude of the velocity
        - Duration of the signal

# Impulse_Step_Pulse
    - Adds a step pulse signal to the velocity, also adjusts position. (This is an infinite version of the square pulse)
    - Requires:
        - Time to start pulse
        - Direction of velocity
        - Magnitude of the velocity




