// https://gist.github.com/krisrok/f55ea7cf89010a9f54a9271953f952fd

#if UNITY_STANDALONE_OSX
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;

public class EnableFileSharingPostBuildProcessor
{
    [PostProcessBuild]
    public static void ChangeXcodePlist(BuildTarget buildTarget, string pathToBuiltProject)
    {
        if (buildTarget == BuildTarget.iOS)
        {
            // Get plist
            string plistPath = pathToBuiltProject + "/Info.plist";
            PlistDocument plist = new PlistDocument();
            plist.ReadFromString(File.ReadAllText(plistPath));

            // Get root
            PlistElementDict rootDict = plist.root;

            rootDict.SetBoolean("UIFileSharingEnabled", true);

            // Write to file
            File.WriteAllText(plistPath, plist.WriteToString());
        }
    }
}
#endif