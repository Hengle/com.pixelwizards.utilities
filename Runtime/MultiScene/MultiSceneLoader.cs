﻿using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PixelWizards.MultiScene
{
	/// <summary>
	/// Stores a single config of a multi-scene loading scenario. You can have any number of them in a single project.
	/// 
	/// Common scenarios include having a 'master' setup with key scenes for gameplay, camera, etc and then additional
	/// configs for individual sets (that contain sub-scenes for set / lighting / fx etc)
	/// </summary>
	[System.Serializable]
	public class SceneConfig
	{
        /// <summary>
        /// The name of this set of scenes
        /// </summary>
		[Header("Name")]
		public string name = "Main Scenes";
        /// <summary>
        /// List of scenes that are in this set
        /// </summary>
		[SerializeField]
		[Header("Scene List")]
		public List<Object> sceneList = new List<Object>();
	}

	/// <summary>
	/// Scriptable Object for the Multi-Scene loading system, also provides an integrated API for loading the scenes
	/// </summary>
    [CreateAssetMenu(fileName = "Multi-Scene Loader", menuName = "Scene Management/Multi-Scene Loader", order = 2)]
    public class MultiSceneLoader : ScriptableObject
    {
        /// <summary>
        /// The list of Configs that we can load
        /// </summary>
		[Header("Scene Config")]
		public List<SceneConfig> config = new List<SceneConfig>();

		/// <summary>
		/// Unloads any scenes currently loaded and then loads all of the defined scenes from config 
		/// </summary>
        public void LoadAllScenes()
		{
			if (config.Count == 0)
			{
				Debug.LogError("No scene configs have been defined - nothing to load!");
				return;
			}

			if (config[0].sceneList[0] == null)
			{
				Debug.LogError("Scene config doesn't have any scenes defined - nothing to load!");
			}
#if UNITY_EDITOR
			if( ! Application.isPlaying)
			{
				EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
			}
#endif

			// load the first scene in the list
			LoadScene(config[0].sceneList[0], false);

			// load the rest of the scenes
			for( var i = 0; i < config.Count; i++)
			{
				var counter = 0;
				if( i == 0)
				{
					// skip the first scene since we already loaded it
					counter = 1;
				}
				for (int j = counter; j < config[i].sceneList.Count; j++)
				{
					LoadScene(config[i].sceneList[j], true);
				}
			}
		}

        /// <summary>
        /// Load a specific scene config, optionally unloads existing first (ie can be optionally additively loaded)
        /// </summary>
        /// <param name="config"></param>
        /// <param name="unloadExisting"></param>
        public void LoadSceneConfig( SceneConfig config, bool unloadExisting)
		{
			
			for( int i = 0; i < config.sceneList.Count; i++)
			{
				if (i == 0)
				{
					// if we need to unload existing, then load the first in single mode, otherwise everything is additive
					LoadScene(config.sceneList[i], !unloadExisting);
				}
				else
				{
					// and the rest additive
					LoadScene(config.sceneList[i], true);
				}
			}
		}

        /// <summary>
        /// Loads an individual scene, optionally done additively. This is a wrapper for SceneManager (runtime loading) and EditorSceneManager (edit-time loading) for scenes
        /// </summary>
        /// <param name="thisScene">the scene you would like to load</param>
        /// <param name="isAdditive">whether you want to use additve loading or not</param>
		private void LoadScene( Object thisScene, bool isAdditive)
		{
			if (thisScene == null)
			{
				Debug.Log("Scene config has empty scene!");
				return;
			}

			if (isAdditive)
			{
				if (Application.isPlaying)
				{
					if (SceneManager.GetSceneByName(thisScene.name) == null)
						Debug.LogError("Scene: " + thisScene.name + " doesn't exist in build settings");
					else
						SceneManager.LoadScene(thisScene.name, LoadSceneMode.Additive);
				}
				else
				{
#if UNITY_EDITOR
					EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(thisScene), OpenSceneMode.Additive);
#endif
				}
			}
			else
			{
				if (Application.isPlaying)
				{
					if (SceneManager.GetSceneByName(thisScene.name) == null)
						Debug.LogError("Scene: " + thisScene.name + " doesn't exist in build settings");
					else
						SceneManager.LoadScene(thisScene.name, LoadSceneMode.Single);
				}
				else
				{

#if UNITY_EDITOR
					EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(thisScene), OpenSceneMode.Single);
#endif
				}
			}
		}
	}
}