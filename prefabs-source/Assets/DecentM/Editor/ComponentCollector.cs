using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DecentM.EditorTools
{
    public class ComponentCollector<ComponentType>
    {
        public List<ComponentType> CollectFromActiveScene()
        {
            List<ComponentType> result = new List<ComponentType>();

            Scene scene = SceneManager.GetActiveScene();

            foreach (GameObject rootObject in scene.GetRootGameObjects())
            {
                ComponentType[] components = rootObject.GetComponentsInChildren<ComponentType>();
                result.AddRange(components);
            }

            return result;
        }
    }
}
