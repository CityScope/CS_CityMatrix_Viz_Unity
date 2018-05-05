using UnityEngine;

namespace Crosstales.UI
{
    /// <summary>Allow to Drag the Windows arround.</summary>
    public class UIDrag : MonoBehaviour
    {
        private float offsetX;
        private float offsetY;

        private Transform tf;

        public void Start()
        {
            tf = transform;
        }

        public void BeginDrag()
        {
            offsetX = tf.position.x - Input.mousePosition.x;
            offsetY = tf.position.y - Input.mousePosition.y;
        }

        public void OnDrag()
        {
            tf.position = new Vector3(offsetX + Input.mousePosition.x, offsetY + Input.mousePosition.y);
        }
    }
}
// © 2017 crosstales LLC (https://www.crosstales.com)