# Instructions

1. Make the 2 coordinates that you want to align the ship with
2. Place a Cockpit (it should be facing the direction you want to align to)
3. Place a LCD somewhere in view
4. Name the Cockpit and LCD so the program can recognise them (see defaults below)
5. Load the script into the Programmable block
6. Copy and Paste the GPS coordinates into the programmable block, it should be of the format:
GPS1&GPS2
7. Happy Aligning!
8. When done aligning, turn the programmable block off till you need it again, it runs constantly.

## Default Names

```
alignment cockpit
alignment lcd
```
You can change these by changing the text at the top of the program, this is case sensetive.

## Usage

You are trying to get the 'aligned' value as close to 1 as possible.

I recommend turning your gyro power to something really low, then using the arrow keys for ultimate precision. I have been able to get the aligned value to 7 9s of precision... thats 0.9999999 (then some garbage numbers after all those 9s)

take the 'diff' values with a grain of salt, they should help you align each axis individually... you *should* only have to worry about the X and Y, i'm pretty sure the Z value is meaningless.


## Just the tip

you can actually delete the coordinates, all that matters is the text is in the custom data of the programmable block

