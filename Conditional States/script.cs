string TAG_NAME = "[cond]";
string CDATA_TAG = "COND";

// @0 COND T=Base Cargo:inv=ingot/iron,scale1000000:bg:0,0,0:0,100,0

//
// END OF CONFIG - DO NOT EDIT BELOW
//
string[] spinner = new string[]{"\\", "|", "/", "-"};
int spinnerIdx = 0;

List<Action> conditions;
bool error = false;

public Program()
{
    IMyTextSurface myLCD = Me.GetSurface(0);
    myLCD.ContentType = ContentType.TEXT_AND_IMAGE;

    setUp();
}

public void Main(string argument, UpdateType updateSource)
{
    Me.GetSurface(0).WriteText("", false);

    if (updateSource == UpdateType.Terminal)
    {
        setUp();
    }
    else if (updateSource == UpdateType.Update100)
    {
        Log("Conditional States\n==================\n\nMonitoring " + conditions.Count + " conditions " + spinner[spinnerIdx++] + "\n");
        if (spinnerIdx >= spinner.Length) spinnerIdx = 0;

        foreach (Action act in conditions)
        {
            act();
        }
    }
}

void setUp()
{
    Me.GetSurface(0).WriteText("", false);
    Runtime.UpdateFrequency = UpdateFrequency.None;
    error = false;
    Log("Updating conditions...");
    conditions = getConditions();
    if (!error) Runtime.UpdateFrequency = UpdateFrequency.Update100;
}

void Log(string message)
{
    Echo(message);
    Me.GetSurface(0).WriteText(message + "\n", true);
}

List<Action> getConditions()
{
    List<Action> conditions = new List<Action>();

    List<IMyTerminalBlock> providers = new List<IMyTerminalBlock>();
    GridTerminalSystem.GetBlocksOfType<IMyTerminalBlock>(providers, p => p.IsSameConstructAs(Me) && p.CustomName.Contains(TAG_NAME));

    Log("Found " + providers.Count + " blocks.");

    foreach (IMyTerminalBlock p in providers)
    {
        if (!(p is IMyTextSurfaceProvider))
        {
            error = true;
            Log("Not a surface provider! " + p);
            continue;
        }

        int found = 0;
        foreach (string line in ((IMyTerminalBlock) p).CustomData.Split('\n'))
        {
            Action c = getCondition((IMyTextSurfaceProvider) p, line);
            if (c != null)
            {
                found++;
                Log("Found condition: " + line);
                conditions.Add(c);
            }
        }
        if (found == 0)
        {
            Log("Custom data has no matching condition lines!");
        }
    }

    return conditions;
}

Action getCondition(IMyTextSurfaceProvider provder, string def)
{
    // @0 COND block/group name/tag:property=options:lcd prop:value1:value2?:value3?
    if (!def.StartsWith("@") || !def.Contains(CDATA_TAG)) return null;
    int idx = -1;
    if (!int.TryParse(def.Substring(1,1), out idx))
    {
        error = true;
        Log("Could not determine index for condition: " + def);
        return null;
    }
    Log("index: " + idx);

    string[] parts = def.Substring(8).Split(':');
    if (parts.Length < 4)
    {
        error = true;
        Log("Invalid condition definition (missing parts): " + def);
        return null;
    }

    IMyTextSurface surface = provder.GetSurface(idx);
    Func<int> check = getCheckFunc(parts[0], parts[1]);
    Action<int> action = getActionFunc(surface, parts[2], parts[3], parts.Length > 4 ? parts[4] : null, parts.Length > 5 ? parts[5] : null);

    if (check == null)
    {
        error = true;
        Log("Invalid check condition");
        return null;
    }
    if (action == null)
    {
        error = true;
        Log("Invalid action");
        return null;
    }

    return () => action(check());
}

Func<int> getCheckFunc(string name, string property)
{
    List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
    if (name.StartsWith("T="))
    {
        GridTerminalSystem.GetBlocksOfType(blocks, b => b.IsSameConstructAs(Me) && b.CustomName.Contains(name.Substring(2)));
    }
    else if (name.StartsWith("G="))
    {
        IMyBlockGroup group = GridTerminalSystem.GetBlockGroupWithName(name.Substring(2));
        group.GetBlocks(blocks);
    }
    else
    {
        blocks.Add(GridTerminalSystem.GetBlockWithName(name));
    }

    if (blocks.Count == 0)
    {
        error = true;
        Log("Invalid condition definition (could not find block(s)): " + name);
        return null;
    }
    else
    {
        Log("Found " + blocks.Count + " blocks to check");
    }

    if ("IsWorking".Equals(property))
    {
        return () => blocks.Exists(block => block.IsWorking) ? 1 : -1;
    }
    if ("extending".Equals(property))
    {
        return () => blocks.Exists(block => {
            IMyPistonBase piston = (IMyPistonBase) block;
            return piston.Velocity > 0 && piston.CurrentPosition < piston.MaxLimit;
        }) ? 1 : -1;
    }
    if ("retracting".Equals(property))
    {
        return () => blocks.Exists(block => {
            IMyPistonBase piston = (IMyPistonBase) block;
            return piston.Velocity < 0 && piston.CurrentPosition > piston.MinLimit;
        }) ? 1 : -1;
    }
    if ("direction".Equals(property))
    {
        return () => {
            IMyPistonBase piston = (IMyPistonBase) blocks[0];
            if (piston.Velocity == 0 ||
                (piston.Velocity < 0 && piston.CurrentPosition == piston.MinLimit) ||
                (piston.Velocity > 0 && piston.CurrentPosition == piston.MaxLimit)
            ) return 0;
            return piston.Velocity > 0 ? 1 : -1;
        };
    }

    error = true;
    Log("Invalid condition definition (unknown condition type): " + property);
    return null;
}

Action<int> getActionFunc(IMyTextSurface surface, string property, string valueA, string valueB, string valueC)
{
    if ("bg".Equals(property))
    {
        Color? a = getColor(valueA), b = getColor(valueB), c = getColor(valueC);
        return (val) => {
            if (val > 0) surface.BackgroundColor = a.Value;
            else if (b != null && val < 0) surface.BackgroundColor = b.Value;
            else if (c != null && val == 0) surface.BackgroundColor = c.Value;
        };
    }

    error = true;
    Log("Invalid condition definition (unknown action type): " + property);
    return null;
}

Color? getColor(string value, Color? dflt = null)
{
    if (value == null) return dflt;
    string[] rgb = value.Split(',');
    int r = 0, g = 0, b = 0;
    if (rgb.Length < 3 || !int.TryParse(rgb[0], out r) || !int.TryParse(rgb[1], out g) || !int.TryParse(rgb[2], out b))
    {
        error = true;
        Log("Invalid color string: " + value);
        return dflt;
    }
    return new Color(r, g, b);
}