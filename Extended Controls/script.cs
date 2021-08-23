// [T|G=]name:action:options
// G=Drill Pistons Forward:extendOrRetract:12.3
// G=Drill Pistons Lateral:extendOrRetract:12.3
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

        // name:extendOrReset:distance:max?:extend speed?:retract speed?
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
        // name:reverse:speed1:speed2
        else if("reverse".Equals(parts[1]))
        {
            Log("reversing " + blocks.Count + " blocks");

            List<IMyPistonBase> pistons = new List<IMyPistonBase>();
            foreach (IMyTerminalBlock block in blocks)
            {
                if (block is IMyPistonBase)
                {
                    pistons.Add((IMyPistonBase) block);
                }
            }
            
            if (parts.Length < 4)
            {
                Log("ERR: missing arguments");
                return;
            }

            float a = -1F;
            if (!float.TryParse(parts[2], out a))
            {
                Log("ERR: speed1 argument is not a number");
                return;
            }

            float b = -1F;
            if (!float.TryParse(parts[3], out b))
            {
                Log("ERR: speed2 argument is not a number");
                return;
            }

            foreach (IMyPistonBase piston in pistons)
            {
                float v = piston.Velocity;
                if (nearlyEqual(piston.Velocity, a)) piston.Velocity = -b;
                else if (nearlyEqual(piston.Velocity, -a)) piston.Velocity = b;
                else if (nearlyEqual(piston.Velocity, b)) piston.Velocity = -a;
                else if (nearlyEqual(piston.Velocity, -b)) piston.Velocity = a;
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

bool nearlyEqual(float a, float b)
{
    if (a.Equals(b)) return true;
    const float epsilon = 0.00001f;
    const float normal = (1 << 23) * float.Epsilon;
    float absA = Math.Abs(a);
    float absB = Math.Abs(b);
    float diff = Math.Abs(a - b);
    if (a == 0 || b == 0 || absA + absB < normal) return diff < (epsilon * normal);
    return diff / Math.Min((absA + absB), float.MaxValue) < epsilon;
}