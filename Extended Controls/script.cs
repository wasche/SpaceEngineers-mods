
public void Main(string argument, UpdateType updateSource)
{
    Me.GetSurface(0).WriteText("", false);
    if (updateSource == UpdateType.Terminal || updateSource == UpdateType.Trigger)
    {
        string[] parts = argument.Split(':');
        if (parts.Length < 2)
        {
            Log("ERR: not enough arguments");
            return;
        }

        List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
        if (parts[0].StartsWith("T="))
        {
            GridTerminalSystem.GetBlocksOfType(blocks, b => b.IsSameConstructAs(Me) && b.CustomName.Contains(parts[0].Substring(2)));
        }
        else if (parts[0].StartsWith("G="))
        {
            IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(parts[0].Substring(2));
            group.GetBlocks(blocks);
        }
        else
        {
            IMyTerminalBlock b = GridTerminalSystem.GetBlockWithName(parts[0]);
            if (b != null) blocks.Add(b);
        }

        if (blocks.Count == 0)
        {
            Log("ERR: blocks not found: " + parts[0]);
            return;
        }

        if("extendOrReset".Equals(parts[1]))
        {
            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            foreach (IMyTerminalBlock block in blocks)
            {
                if (block is IMyPistonBase)
                {
                    pistons.Add((IMyPistonBase) block);
                }
            }

            if (parts.Length < 3)
            {
                Log("ERR: missing distance argument");
                return;
            }

            float distance = -1F;
            if (!float.TryParse(parts[2], out distance))
            {
                Log("ERR: distance argument is not a number");
                return;
            }

            float max = -1F;
            if (parts.Length > 3)
            {
                if (!float.TryParse(parts[3], out max))
                {
                    Log("ERR: max argument is not a number");
                    return;
                }
            }

            float extSpeed = 0.5F;
            float retSpeed = 0.5F;
            if (parts.Length > 4)
            {
                if (!float.TryParse(parts[4], out extSpeed))
                {
                    Log("ERR: extend speed argument is not a number");
                    return;
                }
            }
            if (parts.Length > 5)
            {
                if (!float.TryParse(parts[5], out retSpeed))
                {
                    Log("ERR: retract speed argument is not a number");
                    return;
                }
            }
            else retSpeed = extSpeed;

            float d = 0, l = 0, m = 0;
            foreach (IMyPistonBase piston in pistons)
            {
                d += piston.MaxLimit;
                m += piston.HighestPosition;
                l += (piston.HighestPosition - piston.MaxLimit);
            }

            if ((max < 0 && l <= 0) || (max > 0 && d >= max))
            {
                foreach (IMyPistonBase piston in pistons)
                {
                    piston.MaxLimit = 0;
                    piston.Velocity = retSpeed;
                }
                return;
            }

            if (max < 0) max = m;

            foreach (IMyPistonBase piston in pistons)
            {
                if (piston.MaxLimit < piston.HighestPosition)
                {
                    piston.Velocity = extSpeed;
                    float delta = piston.HighestPosition - piston.MaxLimit;
                    if (delta <= distance)
                    {
                        piston.MaxLimit = piston.HighestPosition;
                        distance -= delta;
                    }
                    else
                    {
                        piston.MaxLimit += distance;
                        distance = 0;
                    }
                }

                if (distance <= 0)
                {
                    break;
                }
            }
        }
        else
        {
            Log("Unknown action: " + parts[1]);
        }
    }
}

void Log(string message)
{
    Echo(message);
    Me.GetSurface(0).WriteText(message + "\n", true);
}
