
/// unity-iOS clipboard plugin


[DllImport("__Internal")]
private static extern string _importString();

// get clipboard data from Xcode
public static string ImportString()
{
    Debug.Log($"import copied string : {_importString()}");
    if (Application.platform == RuntimePlatform.IPhonePlayer)
    {
        return _importString();
    }
    else
    {
        return "";
    }
}

[DllImport("__Internal")]
private static extern void _exportString(string exportData);

// send selected string to Xcode
public static void ExportString(string exportData)
{
    Debug.Log($"export selected string : {exportData}");
    if (Application.platform == RuntimePlatform.IPhonePlayer)
    {
        _exportString(exportData);
    }
}
