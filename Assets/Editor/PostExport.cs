using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using System.Collections;
#if UNITY_IOS
using UnityEditor.iOS.Xcode;
#endif
using System.IO;
using System.Linq;

public class PostExport : MonoBehaviour
{
	#if UNITY_CLOUD_BUILD
	// This method is added in the Advanced Features Settings on UCB
	// PostBuildProcessor.OnPostprocessBuildiOS
	public static void OnPostprocessBuildiOS (string exportPath)
	{
		Debug.Log("[UCB Demos] OnPostprocessBuildiOS");
		ProcessPostBuild(BuildTarget.iOS,exportPath);
	}
	#endif
	
	
	[PostProcessBuild]
	public static void OnPostprocessBuild (BuildTarget buildTarget, string path)
	{
		#if !UNITY_CLOUD_BUILD
		Debug.Log ("[UCB Demos] OnPostprocessBuild");
		ProcessPostBuild (buildTarget, path);
		#endif
	}
	
	internal static void CopyAndReplaceDirectory (string srcPath, string dstPath)
	{		
		if (!Directory.Exists (dstPath))
			Directory.CreateDirectory (dstPath);
		foreach (var file in Directory.GetFiles(srcPath))
			File.Copy (file, Path.Combine (dstPath, Path.GetFileName (file)));
		
		foreach (var dir in Directory.GetDirectories(srcPath))
			CopyAndReplaceDirectory (dir, Path.Combine (dstPath, Path.GetFileName (dir)));
	}
	
	private static void ProcessPostBuild (BuildTarget buildTarget, string path)
	{
		Debug.Log (string.Format ("Build target: {0} Scene: {1}", buildTarget, IsSceneActive ("Assets/TestScene.unity")));

		// We only need this IsSceneActive because we do not want to include the library data in other examples then the LibraryDemo
		if (buildTarget == BuildTarget.iOS && IsSceneActive ("Assets/TestScene.unity")) {

			string projPath = path + "/Unity-iPhone.xcodeproj/project.pbxproj";
			
			PBXProject proj = new PBXProject ();
			proj.ReadFromString (File.ReadAllText (projPath));
			
			string target = proj.TargetGuidByName ("Unity-iPhone");
			
			// Add custom system frameworks. Duplicate frameworks are ignored.
			// needed by our native plugin in Assets/Plugins/iOS
			Debug.Log ("Adding CoreSpotlight to XCode project");
			proj.AddFrameworkToProject (target, "CoreSpotlight.framework", false /*not weak*/);
			
			// Add our framework directory to the framework include path
			//proj.SetBuildProperty (target, "FRAMEWORK_SEARCH_PATHS", "$(inherited)");
			//proj.AddBuildProperty (target, "FRAMEWORK_SEARCH_PATHS", "$(PROJECT_DIR)/Frameworks");
			
			// Set a custom link flag
			//proj.AddBuildProperty (target, "OTHER_LDFLAGS", "-ObjC");
			
			File.WriteAllText (projPath, proj.WriteToString ());
		}
	}
	
	static bool IsSceneActive (string sceneName)
	{
		string[] levels = FillLevels ();
		for (int i = 0; i < levels.Length; ++i) {
			if (levels [i] == sceneName) {
				return true;
			}
		}
		return false;
	}
	
	private static string[] FillLevels ()
	{
		return (from scene in EditorBuildSettings.scenes where scene.enabled select scene.path).ToArray ();
	}
}

