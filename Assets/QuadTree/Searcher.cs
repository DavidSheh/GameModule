
namespace GameModule
{
    public class Searcher : MonoBehaviour
    {
        public void CacheTaggedGameObjectsFromScene(string tag)
        {
            Rect rect = new Rect(0, 0, 512, 512);
            Quadtree<GameObject> quadtree = new Quadtree<GameObject>(rect);
            GameObject go;
            Vector2 go2DPos;
            GameObject[] gos = GameObject.FindGameObjectsWithTag(tag);
            for (int goIdx = 0; goIdx < gos.Length; goIdx++)
            {
                go = gos[goIdx];

                //Only add it if within our bounds
                go2DPos = new Vector2(go.transform.position.x, go.transform.position.z);
                if (rect.Contains(go2DPos))
                {
                    quadtree.Insert(go2DPos, go);
                }
            }
        }

        /// <summary>
        /// Get the objects within the defined area
        /// </summary>
        /// <param name="quadtree">Quadtree of game object to search</param>
        /// <param name="area">Area to search</param>
        /// <returns></returns>
        public List<GameObject> GetNearbyObjects(Quadtree<GameObject> quadtree, Rect area)
        {
            List<GameObject> gameObjects = new List<GameObject>();
            IEnumerable<GameObject> gameObjs = quadtree.Find(area);
            foreach (GameObject go in gameObjs)
            {
                gameObjects.Add(go);
            }
            return gameObjects;
        }

        /// <summary>
        /// Get the closest gameobject to the centre of the area supplied
        /// </summary>
        /// <param name="quadtree">Quadtree of game object to search</param>
        /// <param name="area">The area to search</param>
        /// <returns></returns>
        public GameObject GetClosestObject(Quadtree<GameObject> quadtree, Rect area)
        {
            float distance;
            float closestDistance = float.MaxValue;
            GameObject closestGo = null;
            IEnumerable<GameObject> gameObjs = quadtree.Find(area);
            foreach (GameObject go in gameObjs)
            {
                distance = Vector2.Distance(area.center, new Vector2(go.transform.position.x, go.transform.position.z));
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestGo = go;
                }
            }
            return closestGo;
        }
    }
}