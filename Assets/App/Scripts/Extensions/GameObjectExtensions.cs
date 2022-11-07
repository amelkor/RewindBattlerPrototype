using UnityEngine;

namespace Game
{
    public static class GameObjectExtensions
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="gameObject"></param>
        /// <param name="name"></param>
        /// <param name="parent"></param>
        /// <returns></returns>
        public static GameObject Instantiate(this GameObject gameObject, string name, Transform parent)
        {
            var go = Object.Instantiate(gameObject, parent);
            go.name = name;
            
            return go;
        }
    }
}