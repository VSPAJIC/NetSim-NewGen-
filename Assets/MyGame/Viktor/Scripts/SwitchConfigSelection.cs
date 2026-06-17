public static class SwitchConfigSelection
{
    public static string SelectedSwitchID { get; private set; } = "Switch";

    public static void SelectSwitch(string switchID)
    {
        if (!string.IsNullOrEmpty(switchID))
            SelectedSwitchID = switchID;
    }
}
