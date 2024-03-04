# Turret's Physics Patch

This patch changes the update method in MoveTransform to a more accurate equation of motion, and changes the drag to be quadratic instead of constant. Also locks the rate at which the bullets update their position at 70 hz.
This results in the bullet's motion being independant of framerate.

Bullet motion without drag remains largely unaffected.

This mod can exhibit strange behavior when a player has a high bullet velocity and drag.
This is a result of the numerical integration used, RK4 does somewhat aleviate this.

If you notice any issues, please post in the rounds modding discord.
I will look at the bug reports channel from time to time.

Small note:
I haven't been able to test the added functions since the update, they worked before and the patch seems to work now. 

## Changes

EOM: a*t^2 + v0*t + s0 --> a = F , v = da/dt, x = dv/dt.

Drag: a = D  --> a = D*V^2 * 3/ 400.

Update rate: per frame --> 70 hz.

Adds some functions that impact the acceleration.

## Added functions (These are added to MoveTransform)

# add_variate_acceleration (2 overloads)
    - Adds a function to the acceleration equation
        - this function must be of type Func<float, float, float> or list<Func<float, float, float>>
        - the order of parameters of the function is time then velocity
        - if list is used index 0 indicates x and index 1 indicates y
    - if the equation only holds in one direction the direction parameter can be used instead of the list
        - 'x' for x
        - 'y' for y
    - the equation is activated at t (time since shot for the bullet), the input value should be in the future for all players as to keep the trajectory the same for each player.

# add_constant_acceleration
    - Adds a constant acceleration
    - Requires a Vector3 but only x an y are used
    - the acceleration is added at t (time since shot for the bullet), the input value should be in the future for all players as to keep the trajectory the same for each player.

